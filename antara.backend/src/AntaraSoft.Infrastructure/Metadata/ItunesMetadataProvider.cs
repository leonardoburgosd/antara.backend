using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Metadata
{
    /// <summary>
    /// Proveedor de metadatos basado en la API publica de busqueda de iTunes
    /// (sin API key). Devuelve titulo, artista, album, genero, ano, duracion y
    /// portada en alta resolucion. Apple limita a ~20 peticiones por minuto, por
    /// eso el limitador es conservador.
    /// </summary>
    public class ItunesMetadataProvider : IMetadataProvider
    {
        private const string BaseUrlPorDefecto = "https://itunes.apple.com";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly Regex TamanoPortada = new(@"\d+x\d+bb", RegexOptions.Compiled);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _settings;
        private readonly ProveedorMetadatosConfig _config;
        private readonly ApiRateLimiter _limiter = new(TimeSpan.FromMilliseconds(1500));

        public ItunesMetadataProvider(
            IHttpClientFactory httpClientFactory,
            AppSettings settings,
            ProveedorMetadatosConfig config)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings ?? new AppSettings();
            _config = config ?? new ProveedorMetadatosConfig { Tipo = "itunes" };
        }

        public string Nombre =>
            string.IsNullOrWhiteSpace(_config.Nombre) ? "itunes" : _config.Nombre.Trim();

        public string Etiqueta =>
            string.IsNullOrWhiteSpace(_config.Etiqueta) ? Nombre : _config.Etiqueta.Trim();

        private string BaseUrl =>
            string.IsNullOrWhiteSpace(_config.BaseUrl)
                ? BaseUrlPorDefecto
                : _config.BaseUrl.TrimEnd('/');

        public async Task<MetadatosAudio> BuscarAsync(
            string artista, string titulo, string carpeta, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                return SinResultado();

            var consultas = new List<string>();
            if (!string.IsNullOrWhiteSpace(artista))
                consultas.Add($"{artista} {titulo}");
            consultas.Add(titulo);

            if (!string.IsNullOrWhiteSpace(carpeta) &&
                !string.Equals(carpeta, artista, StringComparison.OrdinalIgnoreCase))
            {
                consultas.Add($"{carpeta} {titulo}");
            }

            ItunesPista mejor = null;
            var mejorPuntaje = 0d;

            foreach (var consulta in consultas.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var pistas = await BuscarPistasAsync(consulta, cancellationToken, 8);

                foreach (var pista in pistas)
                {
                    var puntaje = SimilitudTexto.Puntuar(
                        pista.trackName, pista.artistName, titulo, artista);

                    if (puntaje > mejorPuntaje)
                    {
                        mejorPuntaje = puntaje;
                        mejor = pista;
                    }
                }

                if (mejorPuntaje >= 0.8)
                    break;
            }

            if (mejor == null || mejorPuntaje < 0.35)
                return SinResultado();

            return Mapear(mejor);
        }

        public async Task<List<MetadatosAudio>> BuscarCandidatosAsync(string consulta, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new List<MetadatosAudio>();

            var pistas = await BuscarPistasAsync(consulta.Trim(), cancellationToken, 12);
            return pistas
                .Where(pista => pista != null && !string.IsNullOrWhiteSpace(pista.trackName))
                .Select(Mapear)
                .ToList();
        }

        public async Task<MetadatosAudio> ObtenerPorIdAsync(string idExterno, CancellationToken cancellationToken)
        {
            if (!long.TryParse((idExterno ?? string.Empty).Trim(), out var id) || id <= 0)
                return SinResultado();

            var url = $"{BaseUrl}/lookup?id={id.ToString(CultureInfo.InvariantCulture)}&entity=song";
            var respuesta = await ObtenerJsonAsync<ItunesRespuesta>(url, cancellationToken);
            var pista = respuesta?.results?.FirstOrDefault();

            return pista == null ? SinResultado() : Mapear(pista);
        }

        private async Task<List<ItunesPista>> BuscarPistasAsync(
            string consulta, CancellationToken cancellationToken, int limite)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new List<ItunesPista>();

            var url = $"{BaseUrl}/search?term={Uri.EscapeDataString(consulta)}" +
                      $"&media=music&entity=song&limit={limite}";

            var respuesta = await ObtenerJsonAsync<ItunesRespuesta>(url, cancellationToken);
            return respuesta?.results ?? new List<ItunesPista>();
        }

        private MetadatosAudio Mapear(ItunesPista pista)
        {
            var fecha = FechaDe(pista.releaseDate);
            var artistas = string.IsNullOrWhiteSpace(pista.artistName)
                ? new List<string>()
                : new List<string> { pista.artistName.Trim() };

            var json = JsonSerializer.Serialize(new
            {
                proveedor = Nombre,
                itunesTrackId = pista.trackId,
                itunesCollectionId = pista.collectionId,
                genero = pista.primaryGenreName,
                pais = pista.country,
                duracionSegundos = Duracion(pista.trackTimeMillis)
            });

            return new MetadatosAudio
            {
                Encontrado = true,
                Fuente = Nombre,
                IdExterno = pista.trackId > 0
                    ? pista.trackId.ToString(CultureInfo.InvariantCulture)
                    : null,
                Titulo = string.IsNullOrWhiteSpace(pista.trackName) ? null : pista.trackName.Trim(),
                Artistas = artistas,
                Album = string.IsNullOrWhiteSpace(pista.collectionName) ? null : pista.collectionName.Trim(),
                IdAlbumExterno = pista.collectionId > 0
                    ? pista.collectionId.ToString(CultureInfo.InvariantCulture)
                    : null,
                PortadaUrl = PortadaGrande(pista.artworkUrl100 ?? pista.artworkUrl60),
                Ano = fecha?.Year ?? 0,
                FechaPublicacion = fecha,
                Genero = pista.primaryGenreName,
                DuracionSegundos = Duracion(pista.trackTimeMillis),
                Json = json
            };
        }

        /// <summary>iTunes entrega la portada como .../100x100bb.jpg; se pide 600x600.</summary>
        private static string PortadaGrande(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            return TamanoPortada.Replace(url.Trim(), "600x600bb");
        }

        private static int Duracion(long? milisegundos) =>
            milisegundos.HasValue && milisegundos.Value > 0
                ? (int)Math.Round(milisegundos.Value / 1000.0)
                : 0;

        private static DateTime? FechaDe(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            return DateTime.TryParse(
                valor.Trim(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out var fecha)
                ? fecha
                : null;
        }

        private async Task<T> ObtenerJsonAsync<T>(string url, CancellationToken cancellationToken) where T : class
        {
            var timeoutSegundos = Math.Max(5, _settings.MetadatosTimeoutSegundos);

            for (var intento = 0; intento < 3; intento++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _limiter.EsperarAsync(cancellationToken);

                try
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSegundos));

                    using var cliente = _httpClientFactory.CreateClient("metadatos");
                    using var respuesta = await cliente.GetAsync(url, timeout.Token);

                    if (respuesta.StatusCode == HttpStatusCode.TooManyRequests ||
                        (int)respuesta.StatusCode >= 500)
                    {
                        await Task.Delay(800 * (intento + 1), cancellationToken);
                        continue;
                    }

                    if (!respuesta.IsSuccessStatusCode)
                        return null;

                    var json = await respuesta.Content.ReadAsStringAsync(timeout.Token);
                    return string.IsNullOrWhiteSpace(json)
                        ? null
                        : JsonSerializer.Deserialize<T>(json, JsonOptions);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(400 * (intento + 1), cancellationToken);
                }
                catch (HttpRequestException)
                {
                    await Task.Delay(500 * (intento + 1), cancellationToken);
                }
            }

            return null;
        }

        private MetadatosAudio SinResultado() => new()
        {
            Encontrado = false,
            Fuente = Nombre
        };

        private sealed class ItunesRespuesta
        {
            public List<ItunesPista> results { get; set; } = new();
        }

        private sealed class ItunesPista
        {
            public long trackId { get; set; }
            public long collectionId { get; set; }
            public string trackName { get; set; }
            public string artistName { get; set; }
            public string collectionName { get; set; }
            public string releaseDate { get; set; }
            public long? trackTimeMillis { get; set; }
            public string primaryGenreName { get; set; }
            public string artworkUrl100 { get; set; }
            public string artworkUrl60 { get; set; }
            public string country { get; set; }
            public string kind { get; set; }
        }
    }
}
