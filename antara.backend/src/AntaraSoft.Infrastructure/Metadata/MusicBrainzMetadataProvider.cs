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
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Metadata
{
    /// <summary>
    /// Proveedor de metadatos basado en MusicBrainz (base de datos abierta, sin
    /// API key) con portadas de Cover Art Archive. Respeta el límite de 1 petición
    /// por segundo que pide MusicBrainz.
    /// </summary>
    public class MusicBrainzMetadataProvider : IMetadataProvider
    {
        private const string BaseUrlPorDefecto = "https://musicbrainz.org";
        private const string CoverArtPorDefecto = "https://coverartarchive.org";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _settings;
        private readonly ProveedorMetadatosConfig _config;
        private readonly ApiRateLimiter _limiter = new(TimeSpan.FromMilliseconds(1100));

        public MusicBrainzMetadataProvider(
            IHttpClientFactory httpClientFactory,
            AppSettings settings,
            ProveedorMetadatosConfig config)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings ?? new AppSettings();
            _config = config ?? new ProveedorMetadatosConfig { Tipo = "musicbrainz" };
        }

        public string Nombre =>
            string.IsNullOrWhiteSpace(_config.Nombre) ? "musicbrainz" : _config.Nombre.Trim();

        public string Etiqueta =>
            string.IsNullOrWhiteSpace(_config.Etiqueta) ? Nombre : _config.Etiqueta.Trim();

        private string BaseUrl =>
            string.IsNullOrWhiteSpace(_config.BaseUrl)
                ? BaseUrlPorDefecto
                : _config.BaseUrl.TrimEnd('/');

        // La portada se toma del mismo host que la BaseUrl configurada, así un
        // espejo de MusicBrainz sigue funcionando.
        private string CoverArtBase =>
            BaseUrl.Contains("musicbrainz.org", StringComparison.OrdinalIgnoreCase)
                ? CoverArtPorDefecto
                : BaseUrl;

        public async Task<MetadatosAudio> BuscarAsync(
            string artista, string titulo, string carpeta, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                return SinResultado();

            var consultas = new List<(string Artista, string Titulo)>
            {
                (artista, titulo)
            };

            if (!string.IsNullOrWhiteSpace(carpeta) &&
                !string.Equals(carpeta, artista, StringComparison.OrdinalIgnoreCase))
            {
                consultas.Add((carpeta, titulo));
            }

            consultas.Add((null, titulo));

            MbGrabacion mejor = null;
            string mejorArtista = null;
            var mejorPuntaje = 0d;

            foreach (var (art, tit) in consultas.Distinct())
            {
                var grabaciones = await BuscarGrabacionesAsync(art, tit, cancellationToken, 8);

                foreach (var grabacion in grabaciones)
                {
                    var puntaje = SimilitudTexto.Puntuar(
                        grabacion.title, NombreArtistaPrincipal(grabacion), tit, art);

                    if (puntaje > mejorPuntaje)
                    {
                        mejorPuntaje = puntaje;
                        mejor = grabacion;
                        mejorArtista = art;
                    }
                }

                if (mejorPuntaje >= 0.8)
                    break;
            }

            if (mejor == null || mejorPuntaje < 0.35)
                return SinResultado();

            return await ConstruirMetadatosAsync(mejor, cancellationToken);
        }

        public async Task<List<MetadatosAudio>> BuscarCandidatosAsync(string consulta, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new List<MetadatosAudio>();

            var grabaciones = await BuscarPorTextoAsync(consulta.Trim(), cancellationToken, 12);

            return grabaciones
                .Where(grabacion => grabacion != null && !string.IsNullOrWhiteSpace(grabacion.title))
                .Select(grabacion => MapearCandidato(grabacion))
                .ToList();
        }

        public async Task<MetadatosAudio> ObtenerPorIdAsync(string idExterno, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idExterno))
                return SinResultado();

            var url = $"{BaseUrl}/ws/2/recording/{Uri.EscapeDataString(idExterno.Trim())}" +
                      "?inc=artist-credits+releases+genres+isrcs&fmt=json";
            var grabacion = await ObtenerJsonAsync<MbGrabacion>(url, cancellationToken);

            return grabacion == null
                ? SinResultado()
                : await ConstruirMetadatosAsync(grabacion, cancellationToken);
        }

        private async Task<List<MbGrabacion>> BuscarGrabacionesAsync(
            string artista, string titulo, CancellationToken cancellationToken, int limite)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                return new List<MbGrabacion>();

            var consulta = string.IsNullOrWhiteSpace(artista)
                ? $"recording:\"{EscaparLucene(titulo)}\""
                : $"recording:\"{EscaparLucene(titulo)}\" AND artist:\"{EscaparLucene(artista)}\"";

            var url = $"{BaseUrl}/ws/2/recording?query={Uri.EscapeDataString(consulta)}" +
                      $"&fmt=json&limit={limite}";

            var respuesta = await ObtenerJsonAsync<MbBusqueda>(url, cancellationToken);
            return respuesta?.recordings ?? new List<MbGrabacion>();
        }

        /// <summary>Busqueda de texto libre para el selector de candidatos del panel.</summary>
        private async Task<List<MbGrabacion>> BuscarPorTextoAsync(
            string consulta, CancellationToken cancellationToken, int limite)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new List<MbGrabacion>();

            var url = $"{BaseUrl}/ws/2/recording?query={Uri.EscapeDataString(consulta)}&fmt=json&limit={limite}";
            var respuesta = await ObtenerJsonAsync<MbBusqueda>(url, cancellationToken);
            return respuesta?.recordings ?? new List<MbGrabacion>();
        }

        private async Task<MetadatosAudio> ConstruirMetadatosAsync(MbGrabacion grabacion, CancellationToken cancellationToken)
        {
            var lanzamiento = grabacion.releases?
                .Where(release => release != null && !string.IsNullOrWhiteSpace(release.id))
                .OrderByDescending(release => FechaDe(release.date) ?? DateTime.MinValue)
                .FirstOrDefault();

            var artistas = (grabacion.artistCredit ?? new List<MbCreditoArtista>())
                .Where(credito => credito != null && !string.IsNullOrWhiteSpace(credito.name))
                .Select(credito => credito.name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var fecha = FechaDe(lanzamiento?.date) ?? FechaDe(grabacion.firstReleaseDate);
            var genero = PrimerGenero(grabacion);
            var portada = lanzamiento == null ? null : await ObtenerPortadaAsync(lanzamiento.id, cancellationToken);

            var json = JsonSerializer.Serialize(new
            {
                proveedor = Nombre,
                musicBrainzRecordingId = grabacion.id,
                musicBrainzReleaseId = lanzamiento?.id,
                tipoLanzamiento = lanzamiento?.releaseGroup?.primaryType,
                pais = lanzamiento?.country,
                artistas,
                isrcs = grabacion.isrcs,
                duracionSegundos = Duracion(grabacion)
            });

            return new MetadatosAudio
            {
                Encontrado = true,
                Fuente = Nombre,
                IdExterno = grabacion.id,
                Titulo = string.IsNullOrWhiteSpace(grabacion.title) ? null : grabacion.title.Trim(),
                Artistas = artistas,
                Album = string.IsNullOrWhiteSpace(lanzamiento?.title) ? null : lanzamiento.title.Trim(),
                IdAlbumExterno = lanzamiento?.id,
                PortadaUrl = portada,
                Ano = fecha?.Year ?? 0,
                FechaPublicacion = fecha,
                Genero = genero,
                DuracionSegundos = Duracion(grabacion),
                Isrc = grabacion.isrcs?.FirstOrDefault(),
                Json = json
            };
        }

        private MetadatosAudio MapearCandidato(MbGrabacion grabacion)
        {
            var lanzamiento = grabacion.releases?.FirstOrDefault(release => release != null);
            var fecha = FechaDe(lanzamiento?.date) ?? FechaDe(grabacion.firstReleaseDate);

            return new MetadatosAudio
            {
                Encontrado = true,
                Fuente = Nombre,
                IdExterno = grabacion.id,
                Titulo = grabacion.title,
                Artistas = (grabacion.artistCredit ?? new List<MbCreditoArtista>())
                    .Where(credito => credito != null && !string.IsNullOrWhiteSpace(credito.name))
                    .Select(credito => credito.name.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                Album = lanzamiento?.title,
                IdAlbumExterno = lanzamiento?.id,
                Ano = fecha?.Year ?? 0,
                FechaPublicacion = fecha,
                Genero = PrimerGenero(grabacion),
                DuracionSegundos = Duracion(grabacion)
            };
        }

        /// <summary>
        /// Cover Art Archive responde 404 cuando el lanzamiento no tiene portada,
        /// así que se comprueba con HEAD antes de devolver la URL.
        /// </summary>
        private async Task<string> ObtenerPortadaAsync(string releaseId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(releaseId))
                return null;

            var url = $"{CoverArtBase}/release/{Uri.EscapeDataString(releaseId)}/front-500";

            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(5, _settings.MetadatosTimeoutSegundos)));

                using var cliente = _httpClientFactory.CreateClient("metadatos");
                using var peticion = new HttpRequestMessage(HttpMethod.Head, url);
                using var respuesta = await cliente.SendAsync(
                    peticion, HttpCompletionOption.ResponseHeadersRead, timeout.Token);

                return respuesta.IsSuccessStatusCode ? url : null;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            catch (HttpRequestException)
            {
                return null;
            }
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
                        await Task.Delay(700 * (intento + 1), cancellationToken);
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

        private static string NombreArtistaPrincipal(MbGrabacion grabacion) =>
            grabacion.artistCredit?.FirstOrDefault(credito => credito != null)?.name
            ?? grabacion.artistCredit?.FirstOrDefault()?.artist?.name
            ?? grabacion.artistCreditString;

        private static string PrimerGenero(MbGrabacion grabacion) =>
            grabacion.genres?.FirstOrDefault()?.name
            ?? grabacion.tags?.OrderByDescending(tag => tag.count).FirstOrDefault()?.name;

        private static int Duracion(MbGrabacion grabacion) =>
            grabacion.length.HasValue && grabacion.length.Value > 0
                ? (int)Math.Round(grabacion.length.Value / 1000.0)
                : 0;

        private static DateTime? FechaDe(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            var texto = valor.Trim();

            if (DateTime.TryParseExact(texto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var completa))
                return completa;

            if (DateTime.TryParseExact(texto, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var mes))
                return mes;

            if (DateTime.TryParseExact(texto, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var anio))
                return anio;

            return null;
        }

        private static string EscaparLucene(string valor) =>
            (valor ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Trim();

        private sealed class MbBusqueda
        {
            public List<MbGrabacion> recordings { get; set; } = new();
        }

        private sealed class MbGrabacion
        {
            public string id { get; set; }
            public string title { get; set; }
            public long? length { get; set; }
            public List<string> isrcs { get; set; }

            [JsonPropertyName("artist-credit")]
            public List<MbCreditoArtista> artistCredit { get; set; }

            [JsonPropertyName("artist-credit-phrase")]
            public string artistCreditString { get; set; }

            [JsonPropertyName("first-release-date")]
            public string firstReleaseDate { get; set; }

            public List<MbLanzamiento> releases { get; set; }
            public List<MbGenero> genres { get; set; }
            public List<MbGenero> tags { get; set; }
        }

        private sealed class MbCreditoArtista
        {
            public string name { get; set; }
            public MbArtista artist { get; set; }
        }

        private sealed class MbArtista
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        private sealed class MbLanzamiento
        {
            public string id { get; set; }
            public string title { get; set; }
            public string date { get; set; }
            public string country { get; set; }

            [JsonPropertyName("release-group")]
            public MbGrupoLanzamiento releaseGroup { get; set; }
        }

        private sealed class MbGrupoLanzamiento
        {
            public string id { get; set; }
            public string title { get; set; }

            [JsonPropertyName("primary-type")]
            public string primaryType { get; set; }
        }

        private sealed class MbGenero
        {
            public string name { get; set; }
            public int count { get; set; }
        }
    }
}
