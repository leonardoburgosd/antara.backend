using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Metadata
{
    /// <summary>
    /// Obtiene letras con marcas de tiempo (LRC) desde la API publica de LRCLIB.
    /// La duracion es parte de la firma de la cancion: LRCLIB solo responde si
    /// coincide con la registrada (tolerancia de +/- 2 segundos).
    /// </summary>
    public class LrcLibProvider : ILetrasSincronizadasProvider
    {
        private const string BaseUrlPorDefecto = "https://lrclib.net";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _settings;
        private readonly ApiRateLimiter _limiter = new(TimeSpan.FromMilliseconds(350));

        // Evita repetir la consulta por la misma cancion mientras la API siga viva.
        private readonly ConcurrentDictionary<string, string> _cache = new();

        public LrcLibProvider(IHttpClientFactory httpClientFactory, AppSettings settings)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings ?? new AppSettings();
        }

        public async Task<string> ObtenerAsync(
            string artista,
            string titulo,
            string album,
            int duracionSegundos,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(artista) || string.IsNullOrWhiteSpace(titulo) || duracionSegundos <= 0)
                return null;

            var firma = $"{artista.Trim()}|{titulo.Trim()}|{album?.Trim()}|{duracionSegundos}";
            if (_cache.TryGetValue(firma, out var cacheado))
                return cacheado;

            var parametros = new[]
            {
                $"artist_name={Uri.EscapeDataString(artista.Trim())}",
                $"track_name={Uri.EscapeDataString(titulo.Trim())}",
                $"album_name={Uri.EscapeDataString(album?.Trim() ?? string.Empty)}",
                $"duration={duracionSegundos}"
            };
            var url = $"{BaseUrlPorDefecto}/api/get?{string.Join("&", parametros)}";
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

                    if (documento.RootElement.TryGetProperty("syncedLyrics", out var letra))
                    {
                        var texto = letra.GetString()?.Trim();
                        if (string.IsNullOrWhiteSpace(texto))
                            return null;

                        _cache[firma] = texto;
                        return texto;
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
