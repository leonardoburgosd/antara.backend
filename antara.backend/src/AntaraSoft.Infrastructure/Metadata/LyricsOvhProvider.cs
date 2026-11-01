using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Metadata
{
    /// <summary>
    /// Obtiene la letra de una cancion desde la API publica de lyrics.ovh.
    /// </summary>
    public class LyricsOvhProvider : ILetrasProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _settings;
        private readonly ApiRateLimiter _limiter = new(TimeSpan.FromMilliseconds(350));

        public LyricsOvhProvider(IHttpClientFactory httpClientFactory, IOptions<AppSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options?.Value ?? new AppSettings();
        }

        private string BaseUrl =>
            string.IsNullOrWhiteSpace(_settings.LetrasApiBase)
                ? "https://api.lyrics.ovh"
                : _settings.LetrasApiBase.TrimEnd('/');

        public async Task<string> ObtenerAsync(string artista, string titulo, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(artista) || string.IsNullOrWhiteSpace(titulo))
                return null;

            var url = $"{BaseUrl}/v1/{Uri.EscapeDataString(artista.Trim())}/{Uri.EscapeDataString(titulo.Trim())}";
            var timeoutSegundos = Math.Max(5, _settings.MetadatosTimeoutSegundos);

            for (var intento = 0; intento < 2; intento++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _limiter.EsperarAsync(cancellationToken);

                try
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSegundos));

                    using var cliente = _httpClientFactory.CreateClient("metadatos");
                    using var respuesta = await cliente.GetAsync(url, timeout.Token);

                    if (respuesta.StatusCode == HttpStatusCode.NotFound)
                        return null;

                    if (respuesta.StatusCode == HttpStatusCode.TooManyRequests ||
                        (int)respuesta.StatusCode >= 500)
                    {
                        await Task.Delay(400 * (intento + 1), cancellationToken);
                        continue;
                    }

                    if (!respuesta.IsSuccessStatusCode)
                        return null;

                    var json = await respuesta.Content.ReadAsStringAsync(timeout.Token);
                    using var documento = JsonDocument.Parse(json);

                    if (documento.RootElement.TryGetProperty("lyrics", out var letra))
                    {
                        var texto = letra.GetString();
                        return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
                    }

                    return null;
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(300 * (intento + 1), cancellationToken);
                }
                catch (HttpRequestException)
                {
                    await Task.Delay(400 * (intento + 1), cancellationToken);
                }
                catch (JsonException)
                {
                    return null;
                }
            }

            return null;
        }
    }
}

