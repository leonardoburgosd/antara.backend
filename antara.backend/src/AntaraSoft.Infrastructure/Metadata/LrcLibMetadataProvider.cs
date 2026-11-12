using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Metadata
{
    /// <summary>
    /// Proveedor de metadatos basado en la API publica de LRCLIB (sin API key).
    /// Aporta titulo, artista, album, duracion y letras; LRCLIB no publica
    /// portadas, genero ni año.
    /// </summary>
    public class LrcLibMetadataProvider : IMetadataProvider
    {
        private const string BaseUrlPorDefecto = "https://lrclib.net";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _settings;
        private readonly ProveedorMetadatosConfig _config;

        // LRCLIB pide 200-500 ms entre peticiones para clientes no registrados.
        private readonly ApiRateLimiter _limiter = new(TimeSpan.FromMilliseconds(350));

        public LrcLibMetadataProvider(
            IHttpClientFactory httpClientFactory,
            AppSettings settings,
            ProveedorMetadatosConfig config)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings ?? new AppSettings();
            _config = config ?? new ProveedorMetadatosConfig { Tipo = "lrclib" };
        }

        public string Nombre =>
            string.IsNullOrWhiteSpace(_config.Nombre) ? "lrclib" : _config.Nombre.Trim();

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

            var urls = new List<string> { UrlBusquedaEstructurada(artista, titulo) };

            // Si la carpeta no coincide con el artista puede contener el album;
            // una busqueda libre adicional ayuda cuando el nombre del archivo
            // no trae artista.
            if (!string.IsNullOrWhiteSpace(carpeta) &&
                !string.Equals(carpeta, artista, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(carpeta, titulo, StringComparison.OrdinalIgnoreCase))
            {
                urls.Add($"{BaseUrl}/api/search?q={Uri.EscapeDataString($"{carpeta.Trim()} {titulo.Trim()}")}");
            }

            LrcRecord mejor = null;
            var mejorPuntaje = 0d;

            foreach (var url in urls.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var registros = await ObtenerJsonAsync<List<LrcRecord>>(url, cancellationToken);
                if (registros == null)
                    continue;

                foreach (var registro in registros.Where(Valido))
                {
                    var puntaje = SimilitudTexto.Puntuar(
                        registro.trackName, registro.artistName, titulo, artista);

                    if (puntaje > mejorPuntaje)
                    {
                        mejorPuntaje = puntaje;
                        mejor = registro;
                    }
                }

                if (mejorPuntaje >= 0.8)
                    break;
            }

            return mejor == null || mejorPuntaje < 0.35
                ? SinResultado()
                : Mapear(mejor);
        }

        public async Task<List<MetadatosAudio>> BuscarCandidatosAsync(
            string consulta, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new List<MetadatosAudio>();

            var url = $"{BaseUrl}/api/search?q={Uri.EscapeDataString(consulta.Trim())}";
            var registros = await ObtenerJsonAsync<List<LrcRecord>>(url, cancellationToken);

            return (registros ?? new List<LrcRecord>())
                .Where(Valido)
                .Take(15)
                .Select(Mapear)
                .ToList();
        }

        public async Task<MetadatosAudio> ObtenerPorIdAsync(
            string idExterno, CancellationToken cancellationToken)
        {
            if (!long.TryParse((idExterno ?? string.Empty).Trim(), out var id) || id <= 0)
                return SinResultado();

            var url = $"{BaseUrl}/api/get/{id.ToString(CultureInfo.InvariantCulture)}";
            var registro = await ObtenerJsonAsync<LrcRecord>(url, cancellationToken);

            return registro == null ? SinResultado() : Mapear(registro);
        }

        private string UrlBusquedaEstructurada(string artista, string titulo)
        {
            var parametros = new List<string>
            {
                $"track_name={Uri.EscapeDataString(titulo.Trim())}"
            };

            if (!string.IsNullOrWhiteSpace(artista))
                parametros.Add($"artist_name={Uri.EscapeDataString(artista.Trim())}");

            return $"{BaseUrl}/api/search?{string.Join("&", parametros)}";
        }

        private MetadatosAudio Mapear(LrcRecord registro)
        {
            if (!Valido(registro))
                return SinResultado();

            // LRCLIB solo devuelve registros que ya tienen letra (plana o
            // sincronizada); se guarda la plana y se conservan las marcas.
            var letra = string.IsNullOrWhiteSpace(registro.plainLyrics)
                ? null
                : registro.plainLyrics.Trim();

            var json = JsonSerializer.Serialize(new
            {
                proveedor = Nombre,
                lrclibId = registro.id,
                instrumental = registro.instrumental,
                hasWordSync = registro.hasWordSync,
                tieneLetra = !string.IsNullOrWhiteSpace(registro.plainLyrics),
                tieneLetraSincronizada = !string.IsNullOrWhiteSpace(registro.syncedLyrics),
                duracionSegundos = Duracion(registro)
            });

            return new MetadatosAudio
            {
                Encontrado = true,
                Fuente = Nombre,
                IdExterno = registro.id.ToString(CultureInfo.InvariantCulture),
                Titulo = registro.trackName.Trim(),
                Artistas = string.IsNullOrWhiteSpace(registro.artistName)
                    ? new List<string>()
                    : new List<string> { registro.artistName.Trim() },
                Album = string.IsNullOrWhiteSpace(registro.albumName)
                    ? null
                    : registro.albumName.Trim(),
                DuracionSegundos = Duracion(registro),
                Letra = letra,
                Json = json
            };
        }

        private static int Duracion(LrcRecord registro) =>
            registro.duration.GetValueOrDefault() > 0
                ? (int)Math.Round(registro.duration.Value)
                : 0;

        private static bool Valido(LrcRecord registro) =>
            registro != null &&
            registro.id > 0 &&
            !string.IsNullOrWhiteSpace(registro.trackName);

        private async Task<T> ObtenerJsonAsync<T>(string url, CancellationToken cancellationToken)
            where T : class
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

                    if (respuesta.StatusCode == HttpStatusCode.NotFound)
                        return null;

                    if (respuesta.StatusCode == HttpStatusCode.TooManyRequests ||
                        (int)respuesta.StatusCode >= 500)
                    {
                        if (intento == 2)
                            return null;

                        await Task.Delay(EsperaReintento(respuesta, intento), cancellationToken);
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
                    if (intento == 2)
                        return null;

                    await Task.Delay(400 * (intento + 1), cancellationToken);
                }
                catch (HttpRequestException)
                {
                    if (intento == 2)
                        return null;

                    await Task.Delay(500 * (intento + 1), cancellationToken);
                }
                catch (JsonException)
                {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// LRCLIB exige respetar Retry-After cuando responde 429. Se limita la
        /// espera a 30 s para no dejar una peticion colgada demasiado tiempo.
        /// </summary>
        private static TimeSpan EsperaReintento(HttpResponseMessage respuesta, int intento)
        {
            var segundos = 2 * (intento + 1);

            if (respuesta.StatusCode == HttpStatusCode.TooManyRequests &&
                respuesta.Headers.TryGetValues("Retry-After", out var valores) &&
                int.TryParse(valores.FirstOrDefault(), out var indicado) &&
                indicado > 0)
            {
                segundos = Math.Min(indicado, 30);
            }

            var jitter = Random.Shared.NextDouble() * segundos * 0.3;
            return TimeSpan.FromSeconds(segundos + jitter);
        }

        private MetadatosAudio SinResultado() => new()
        {
            Encontrado = false,
            Fuente = Nombre
        };

        private sealed class LrcRecord
        {
            public long id { get; set; }
            public string trackName { get; set; }
            public string artistName { get; set; }
            public string albumName { get; set; }
            // LRCLIB puede devolver 234.0; un int fallaria al deserializar.
            public double? duration { get; set; }
            public bool instrumental { get; set; }
            public bool hasWordSync { get; set; }
            public string plainLyrics { get; set; }
            public string syncedLyrics { get; set; }
        }
    }
}
