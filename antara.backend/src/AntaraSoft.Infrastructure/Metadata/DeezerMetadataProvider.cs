using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Metadata
{
    /// <summary>
    /// Proveedor de metadatos basado en la API publica de Deezer (sin API key).
    /// </summary>
    public class DeezerMetadataProvider : IMetadataProvider
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly Regex Espacios = new(@"\s+", RegexOptions.Compiled);

        private const string BaseUrlPorDefecto = "https://api.deezer.com";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _settings;
        private readonly ProveedorMetadatosConfig _config;
        private readonly ApiRateLimiter _limiter = new(TimeSpan.FromMilliseconds(220));
        private readonly ConcurrentDictionary<long, DeezerAlbum> _albumes = new();

        public DeezerMetadataProvider(
            IHttpClientFactory httpClientFactory,
            AppSettings settings,
            ProveedorMetadatosConfig config)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings ?? new AppSettings();
            _config = config ?? new ProveedorMetadatosConfig { Tipo = "deezer" };
        }

        public string Nombre =>
            string.IsNullOrWhiteSpace(_config.Nombre) ? "deezer" : _config.Nombre.Trim();

        public string Etiqueta =>
            string.IsNullOrWhiteSpace(_config.Etiqueta) ? Nombre : _config.Etiqueta.Trim();

        private string BaseUrl =>
            string.IsNullOrWhiteSpace(_config.BaseUrl)
                ? BaseUrlPorDefecto
                : _config.BaseUrl.TrimEnd('/');

        public async Task<MetadatosAudio> BuscarAsync(string artista, string titulo, string carpeta, CancellationToken cancellationToken)
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

            DeezerTrack mejor = null;
            var mejorPuntaje = 0d;

            foreach (var consulta in consultas.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var canciones = await BuscarCancionesAsync(consulta, cancellationToken);
                foreach (var cancion in canciones)
                {
                    var puntaje = Puntuar(cancion, artista, titulo);
                    if (puntaje > mejorPuntaje)
                    {
                        mejorPuntaje = puntaje;
                        mejor = cancion;
                    }
                }

                if (mejorPuntaje >= 0.8)
                    break;
            }

            if (mejor == null || mejorPuntaje < 0.35)
                return SinResultado();

            return await ConstruirMetadatosAsync(mejor, cancellationToken);
        }

        private async Task<List<DeezerTrack>> BuscarCancionesAsync(string consulta, CancellationToken cancellationToken, int limite = 8)
        {
            var url = $"{BaseUrl}/search/track?q={Uri.EscapeDataString(consulta)}&limit={limite}";
            var respuesta = await ObtenerJsonAsync<DeezerBusqueda>(url, cancellationToken);
            return respuesta?.data ?? new List<DeezerTrack>();
        }

        public async Task<List<MetadatosAudio>> BuscarCandidatosAsync(string consulta, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new List<MetadatosAudio>();

            var canciones = await BuscarCancionesAsync(consulta, cancellationToken, 12);

            return canciones
                .Where(cancion => cancion != null && !string.IsNullOrWhiteSpace(cancion.title_short ?? cancion.title))
                .Select(cancion => new MetadatosAudio
                {
                    Encontrado = true,
                    Fuente = Nombre,
                    IdExterno = cancion.id.ToString(CultureInfo.InvariantCulture),
                    Titulo = cancion.title_short ?? cancion.title,
                    Artistas = string.IsNullOrWhiteSpace(cancion.artist?.name)
                        ? new List<string>()
                        : new List<string> { cancion.artist.name },
                    Album = cancion.album?.title,
                    IdAlbumExterno = cancion.album != null && cancion.album.id > 0
                        ? cancion.album.id.ToString(CultureInfo.InvariantCulture)
                        : null,
                    PortadaUrl = cancion.album?.cover_xl,
                    ImagenArtistaUrl = cancion.artist?.picture_xl,
                    Ano = ParsearFecha(cancion.release_date)?.Year ?? 0,
                    DuracionSegundos = cancion.duration
                })
                .ToList();
        }

        public async Task<MetadatosAudio> ObtenerPorIdAsync(string idExterno, CancellationToken cancellationToken)
        {
            if (!long.TryParse(idExterno, out var id) || id <= 0)
                return SinResultado();

            var detalle = await ObtenerJsonAsync<DeezerTrack>($"{BaseUrl}/track/{id}", cancellationToken);
            return detalle == null
                ? SinResultado()
                : await ConstruirMetadatosAsync(detalle, cancellationToken);
        }

        private async Task<MetadatosAudio> ConstruirMetadatosAsync(DeezerTrack cancion, CancellationToken cancellationToken)
        {
            var detalle = await ObtenerJsonAsync<DeezerTrack>($"{BaseUrl}/track/{cancion.id}", cancellationToken) ?? cancion;

            DeezerAlbum albumDetalle = null;
            var albumId = ObtenerAlbumId(cancion, detalle);
            if (albumId > 0)
                albumDetalle = await ObtenerAlbumAsync(albumId, cancellationToken);

            var artistas = new List<string>();
            string imagenArtista = null;

            if (detalle.contributors != null && detalle.contributors.Count > 0)
            {
                foreach (var contribuyente in detalle.contributors)
                {
                    if (string.IsNullOrWhiteSpace(contribuyente.name))
                        continue;

                    if (!artistas.Any(nombre => string.Equals(nombre, contribuyente.name, StringComparison.OrdinalIgnoreCase)))
                        artistas.Add(contribuyente.name);

                    if (string.IsNullOrWhiteSpace(imagenArtista))
                        imagenArtista = contribuyente.picture_xl;
                }
            }

            if (artistas.Count == 0 && !string.IsNullOrWhiteSpace(cancion.artist?.name))
                artistas.Add(cancion.artist.name);

            if (string.IsNullOrWhiteSpace(imagenArtista))
                imagenArtista = cancion.artist?.picture_xl;

            var fecha = ParsearFecha(albumDetalle?.release_date)
                ?? ParsearFecha(detalle.release_date)
                ?? ParsearFecha(cancion.release_date);

            var generos = albumDetalle?.genres?.data;
            var genero = generos?.FirstOrDefault()?.name;
            var albumTitulo = albumDetalle?.title ?? cancion.album?.title;
            var portada = albumDetalle?.cover_xl ?? cancion.album?.cover_xl;
            var idExterno = (detalle.id > 0 ? detalle.id : cancion.id).ToString(CultureInfo.InvariantCulture);
            var idAlbum = albumDetalle?.id ?? albumId;

            var json = JsonSerializer.Serialize(new
            {
                proveedor = Nombre,
                deezerTrackId = idExterno,
                isrc = detalle.isrc ?? cancion.isrc,
                sello = albumDetalle?.label,
                generos = generos?.Select(g => g.name).ToList(),
                artistas,
                duracionSegundos = detalle.duration > 0 ? detalle.duration : cancion.duration
            });

            return new MetadatosAudio
            {
                Encontrado = true,
                Fuente = Nombre,
                IdExterno = idExterno,
                Titulo = detalle.title ?? cancion.title,
                Artistas = artistas,
                Album = albumTitulo,
                IdAlbumExterno = idAlbum > 0 ? idAlbum.ToString(CultureInfo.InvariantCulture) : null,
                PortadaUrl = portada,
                ImagenArtistaUrl = imagenArtista,
                Ano = fecha?.Year ?? 0,
                FechaPublicacion = fecha,
                Genero = genero,
                Sello = albumDetalle?.label,
                DuracionSegundos = detalle.duration > 0 ? detalle.duration : cancion.duration,
                Isrc = detalle.isrc ?? cancion.isrc,
                Json = json
            };
        }

        private async Task<DeezerAlbum> ObtenerAlbumAsync(long albumId, CancellationToken cancellationToken)
        {
            if (_albumes.TryGetValue(albumId, out var cache))
                return cache;

            var album = await ObtenerJsonAsync<DeezerAlbum>($"{BaseUrl}/album/{albumId}", cancellationToken);
            if (album != null)
                _albumes[albumId] = album;

            return album;
        }

        private static long ObtenerAlbumId(DeezerTrack cancion, DeezerTrack detalle)
        {
            if (detalle.album != null && detalle.album.id > 0)
                return detalle.album.id;
            if (cancion.album != null && cancion.album.id > 0)
                return cancion.album.id;
            return 0;
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
                        await Task.Delay(400 * (intento + 1), cancellationToken);
                        continue;
                    }

                    if (!respuesta.IsSuccessStatusCode)
                        return null;

                    var json = await respuesta.Content.ReadAsStringAsync(timeout.Token);
                    return JsonSerializer.Deserialize<T>(json, JsonOptions);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(300 * (intento + 1), cancellationToken);
                }
                catch (HttpRequestException)
                {
                    await Task.Delay(400 * (intento + 1), cancellationToken);
                }
            }

            return null;
        }

        private MetadatosAudio SinResultado() => new()
        {
            Encontrado = false,
            Fuente = Nombre
        };

        private static DateTime? ParsearFecha(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            return DateTime.TryParseExact(
                valor.Trim(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fecha)
                ? fecha
                : null;
        }

        private static double Puntuar(DeezerTrack cancion, string artista, string titulo)
        {
            if (cancion == null)
                return 0;

            var tituloCancion = Normalizar(cancion.title_short ?? cancion.title);
            var tituloBuscado = Normalizar(titulo);
            var similitudTitulo = Similitud(tituloCancion, tituloBuscado);

            if (string.IsNullOrWhiteSpace(artista))
                return similitudTitulo;

            var similitudArtista = Similitud(Normalizar(cancion.artist?.name), Normalizar(artista));
            return similitudTitulo * 0.7 + similitudArtista * 0.3;
        }

        private static string Normalizar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            var descompuesto = valor.Normalize(NormalizationForm.FormD);
            var sinAcentos = new StringBuilder(descompuesto.Length);

            foreach (var caracter in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
                    sinAcentos.Append(caracter);
            }

            var limpio = sinAcentos.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
            var resultado = new StringBuilder(limpio.Length);

            foreach (var caracter in limpio)
                resultado.Append(char.IsLetterOrDigit(caracter) ? caracter : ' ');

            return Espacios.Replace(resultado.ToString(), " ").Trim();
        }

        private static double Similitud(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return 0;

            if (string.Equals(a, b, StringComparison.Ordinal))
                return 1;

            if (a.Contains(b, StringComparison.Ordinal) || b.Contains(a, StringComparison.Ordinal))
                return 0.85;

            var tokensA = a.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            var tokensB = b.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

            if (tokensA.Count == 0 || tokensB.Count == 0)
                return 0;

            var interseccion = tokensA.Intersect(tokensB).Count();
            var union = tokensA.Union(tokensB).Count();
            return union == 0 ? 0 : (double)interseccion / union;
        }

        private sealed class DeezerBusqueda
        {
            public List<DeezerTrack> data { get; set; } = new();
        }

        private sealed class DeezerTrack
        {
            public long id { get; set; }
            public string title { get; set; }
            public string title_short { get; set; }
            public string isrc { get; set; }
            public int duration { get; set; }
            public string release_date { get; set; }
            public DeezerArtist artist { get; set; }
            public DeezerAlbum album { get; set; }
            public List<DeezerContributor> contributors { get; set; }
        }

        private sealed class DeezerArtist
        {
            public long id { get; set; }
            public string name { get; set; }
            public string picture_xl { get; set; }
        }

        private sealed class DeezerContributor
        {
            public long id { get; set; }
            public string name { get; set; }
            public string role { get; set; }
            public string picture_xl { get; set; }
        }

        private sealed class DeezerAlbum
        {
            public long id { get; set; }
            public string title { get; set; }
            public string cover_xl { get; set; }
            public string release_date { get; set; }
            public string label { get; set; }
            public DeezerGeneros genres { get; set; }
        }

        private sealed class DeezerGeneros
        {
            public List<DeezerGenero> data { get; set; } = new();
        }

        private sealed class DeezerGenero
        {
            public int id { get; set; }
            public string name { get; set; }
        }
    }
}

