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
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Metadata
{
    /// <summary>
    /// Proveedor de metadatos basado en TheAudioDB API v1 (soporta key publica '123' o personalizada).
    /// </summary>
    public class TheAudioDbMetadataProvider : IMetadataProvider
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        private static readonly Regex Espacios = new(@"\s+", RegexOptions.Compiled);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _settings;
        private readonly ApiRateLimiter _limiter = new(TimeSpan.FromMilliseconds(400));
        private readonly ConcurrentDictionary<string, TheAudioDbAlbum> _albumes = new();
        private readonly ConcurrentDictionary<string, TheAudioDbArtist> _artistas = new();

        public TheAudioDbMetadataProvider(IHttpClientFactory httpClientFactory, IOptions<AppSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options?.Value ?? new AppSettings();
        }

        public string Nombre => "theaudiodb";

        private string BaseUrl
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_settings.TheAudioDbApiBase))
                    return _settings.TheAudioDbApiBase.TrimEnd('/');

                var key = string.IsNullOrWhiteSpace(_settings.TheAudioDbApiKey) ? "123" : _settings.TheAudioDbApiKey.Trim();
                return $"https://www.theaudiodb.com/api/v1/json/{key}";
            }
        }

        public async Task<MetadatosAudio> BuscarAsync(string artista, string titulo, string carpeta, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                return SinResultado();

            var pares = new List<(string Artista, string Titulo)>();

            if (!string.IsNullOrWhiteSpace(artista))
                pares.Add((artista, titulo));

            if (!string.IsNullOrWhiteSpace(carpeta) && !string.Equals(carpeta, artista, StringComparison.OrdinalIgnoreCase))
                pares.Add((carpeta, titulo));

            var analizado = AnalizadorNombreArchivo.Analizar(titulo);
            if (!string.IsNullOrWhiteSpace(analizado.Artista) && !string.IsNullOrWhiteSpace(analizado.Titulo))
                pares.Add((analizado.Artista, analizado.Titulo));

            TheAudioDbTrack mejor = null;
            var mejorPuntaje = 0d;

            foreach (var (art, tit) in pares.Distinct())
            {
                var canciones = await BuscarCancionesAsync(art, tit, cancellationToken);
                foreach (var cancion in canciones)
                {
                    var puntaje = Puntuar(cancion, art, tit);
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

        public async Task<List<MetadatosAudio>> BuscarCandidatosAsync(string consulta, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new List<MetadatosAudio>();

            var texto = consulta.Trim();
            var cancionesEncontradas = new List<TheAudioDbTrack>();

            var analizado = AnalizadorNombreArchivo.Analizar(texto);
            if (!string.IsNullOrWhiteSpace(analizado.Artista) && !string.IsNullOrWhiteSpace(analizado.Titulo))
            {
                var porAnalisis = await BuscarCancionesAsync(analizado.Artista, analizado.Titulo, cancellationToken);
                cancionesEncontradas.AddRange(porAnalisis);
            }

            var partes = texto.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (cancionesEncontradas.Count == 0 && partes.Length >= 2)
            {
                var art1 = partes[0];
                var tit1 = string.Join(" ", partes.Skip(1));
                cancionesEncontradas.AddRange(await BuscarCancionesAsync(art1, tit1, cancellationToken));

                if (cancionesEncontradas.Count == 0 && partes.Length >= 3)
                {
                    var art2 = string.Join(" ", partes.Take(partes.Length - 1));
                    var tit2 = partes[partes.Length - 1];
                    cancionesEncontradas.AddRange(await BuscarCancionesAsync(art2, tit2, cancellationToken));
                }
            }

            if (cancionesEncontradas.Count == 0)
            {
                var urlTop10 = $"{BaseUrl}/track-top10.php?s={Uri.EscapeDataString(texto)}";
                var respuestaTop = await ObtenerJsonAsync<TheAudioDbTrackResponse>(urlTop10, cancellationToken);
                if (respuestaTop?.track != null)
                    cancionesEncontradas.AddRange(respuestaTop.track);
            }

            var pistasUnicas = cancionesEncontradas
                .Where(c => c != null && !string.IsNullOrWhiteSpace(c.idTrack) && !string.IsNullOrWhiteSpace(c.strTrack))
                .GroupBy(c => c.idTrack)
                .Select(g => g.First())
                .Take(12)
                .ToList();

            var resultados = new List<MetadatosAudio>();
            foreach (var pista in pistasUnicas)
            {
                var portada = pista.strTrackThumb;
                var ano = ParsearAnio(pista.intYearReleased);

                if ((string.IsNullOrWhiteSpace(portada) || ano == 0) && !string.IsNullOrWhiteSpace(pista.idAlbum))
                {
                    var album = await ObtenerAlbumAsync(pista.idAlbum, cancellationToken);
                    if (album != null)
                    {
                        if (string.IsNullOrWhiteSpace(portada))
                            portada = album.strAlbumThumb;
                        if (ano == 0)
                            ano = ParsearAnio(album.intYearReleased);
                    }
                }

                resultados.Add(new MetadatosAudio
                {
                    Encontrado = true,
                    Fuente = Nombre,
                    IdExterno = pista.idTrack,
                    Titulo = pista.strTrack,
                    Artistas = string.IsNullOrWhiteSpace(pista.strArtist)
                        ? new List<string>()
                        : new List<string> { pista.strArtist },
                    Album = pista.strAlbum,
                    IdAlbumExterno = pista.idAlbum,
                    PortadaUrl = portada,
                    Ano = ano,
                    DuracionSegundos = ParsearDuracion(pista.intDuration)
                });
            }

            return resultados;
        }

        public async Task<MetadatosAudio> ObtenerPorIdAsync(string idExterno, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idExterno))
                return SinResultado();

            var url = $"{BaseUrl}/track.php?h={Uri.EscapeDataString(idExterno.Trim())}";
            var respuesta = await ObtenerJsonAsync<TheAudioDbTrackResponse>(url, cancellationToken);
            var pista = respuesta?.track?.FirstOrDefault();

            return pista == null
                ? SinResultado()
                : await ConstruirMetadatosAsync(pista, cancellationToken);
        }

        private async Task<List<TheAudioDbTrack>> BuscarCancionesAsync(string artista, string titulo, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(artista) || string.IsNullOrWhiteSpace(titulo))
                return new List<TheAudioDbTrack>();

            var url = $"{BaseUrl}/searchtrack.php?s={Uri.EscapeDataString(artista.Trim())}&t={Uri.EscapeDataString(titulo.Trim())}";
            var respuesta = await ObtenerJsonAsync<TheAudioDbTrackResponse>(url, cancellationToken);
            return respuesta?.track ?? new List<TheAudioDbTrack>();
        }

        private async Task<MetadatosAudio> ConstruirMetadatosAsync(TheAudioDbTrack track, CancellationToken cancellationToken)
        {
            TheAudioDbAlbum albumDetalle = null;
            if (!string.IsNullOrWhiteSpace(track.idAlbum))
                albumDetalle = await ObtenerAlbumAsync(track.idAlbum, cancellationToken);

            TheAudioDbArtist artistaDetalle = null;
            if (!string.IsNullOrWhiteSpace(track.idArtist))
                artistaDetalle = await ObtenerArtistaAsync(track.idArtist, cancellationToken);

            var artistas = new List<string>();
            if (!string.IsNullOrWhiteSpace(track.strArtist))
                artistas.Add(track.strArtist);
            else if (!string.IsNullOrWhiteSpace(albumDetalle?.strArtist))
                artistas.Add(albumDetalle.strArtist);

            var imagenArtista = artistaDetalle?.strArtistThumb
                ?? artistaDetalle?.strArtistFanart
                ?? artistaDetalle?.strArtistWideThumb;

            var portada = track.strTrackThumb
                ?? albumDetalle?.strAlbumThumb;

            var ano = ParsearAnio(track.intYearReleased) != 0
                ? ParsearAnio(track.intYearReleased)
                : ParsearAnio(albumDetalle?.intYearReleased);

            DateTime? fechaPublicacion = ano > 0 ? new DateTime(ano, 1, 1) : null;
            var genero = track.strGenre ?? albumDetalle?.strGenre;
            var sello = albumDetalle?.strLabel;
            var duracion = ParsearDuracion(track.intDuration);

            var json = JsonSerializer.Serialize(new
            {
                proveedor = Nombre,
                theAudioDbTrackId = track.idTrack,
                theAudioDbAlbumId = track.idAlbum,
                theAudioDbArtistId = track.idArtist,
                musicBrainzTrackId = track.strMusicBrainzID,
                sello,
                genero,
                artistas,
                duracionSegundos = duracion
            });

            return new MetadatosAudio
            {
                Encontrado = true,
                Fuente = Nombre,
                IdExterno = track.idTrack,
                Titulo = track.strTrack,
                Artistas = artistas,
                Album = track.strAlbum ?? albumDetalle?.strAlbum,
                IdAlbumExterno = track.idAlbum,
                PortadaUrl = portada,
                ImagenArtistaUrl = imagenArtista,
                Ano = ano,
                FechaPublicacion = fechaPublicacion,
                Genero = genero,
                Sello = sello,
                DuracionSegundos = duracion,
                Letra = track.strTrackLyrics,
                Isrc = track.strMusicBrainzID,
                Json = json
            };
        }

        private async Task<TheAudioDbAlbum> ObtenerAlbumAsync(string idAlbum, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idAlbum))
                return null;

            if (_albumes.TryGetValue(idAlbum, out var cache))
                return cache;

            var url = $"{BaseUrl}/album.php?m={Uri.EscapeDataString(idAlbum)}";
            var respuesta = await ObtenerJsonAsync<TheAudioDbAlbumResponse>(url, cancellationToken);
            var album = respuesta?.album?.FirstOrDefault();

            if (album != null)
                _albumes[idAlbum] = album;

            return album;
        }

        private async Task<TheAudioDbArtist> ObtenerArtistaAsync(string idArtist, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idArtist))
                return null;

            if (_artistas.TryGetValue(idArtist, out var cache))
                return cache;

            var url = $"{BaseUrl}/artist.php?i={Uri.EscapeDataString(idArtist)}";
            var respuesta = await ObtenerJsonAsync<TheAudioDbArtistResponse>(url, cancellationToken);
            var artista = respuesta?.artists?.FirstOrDefault();

            if (artista != null)
                _artistas[idArtist] = artista;

            return artista;
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

                    if (respuesta.StatusCode == HttpStatusCode.TooManyRequests || (int)respuesta.StatusCode >= 500)
                    {
                        await Task.Delay(400 * (intento + 1), cancellationToken);
                        continue;
                    }

                    if (!respuesta.IsSuccessStatusCode)
                        return null;

                    var json = await respuesta.Content.ReadAsStringAsync(timeout.Token);
                    if (string.IsNullOrWhiteSpace(json) || json.Trim() == "{\"track\":null}" || json.Trim() == "{\"album\":null}" || json.Trim() == "{\"artists\":null}")
                        return null;

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

        private static MetadatosAudio SinResultado() => new()
        {
            Encontrado = false,
            Fuente = "theaudiodb"
        };

        private static int ParsearAnio(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return 0;

            if (int.TryParse(valor.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var anio) && anio > 1900 && anio < 2100)
                return anio;

            var match = Regex.Match(valor, @"\b(19\d{2}|20\d{2})\b");
            return match.Success && int.TryParse(match.Value, out var anoExt) ? anoExt : 0;
        }

        private static int ParsearDuracion(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return 0;

            if (!long.TryParse(valor.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var numero) || numero <= 0)
                return 0;

            if (numero > 1000)
                return (int)Math.Round(numero / 1000.0);

            return (int)numero;
        }

        private static double Puntuar(TheAudioDbTrack cancion, string artista, string titulo)
        {
            if (cancion == null)
                return 0;

            var tituloCancion = Normalizar(cancion.strTrack);
            var tituloBuscado = Normalizar(titulo);
            var similitudTitulo = Similitud(tituloCancion, tituloBuscado);

            if (string.IsNullOrWhiteSpace(artista))
                return similitudTitulo;

            var similitudArtista = Similitud(Normalizar(cancion.strArtist), Normalizar(artista));
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

        public sealed class TheAudioDbTrackResponse
        {
            public List<TheAudioDbTrack> track { get; set; } = new();
        }

        public sealed class TheAudioDbTrack
        {
            public string idTrack { get; set; }
            public string idAlbum { get; set; }
            public string idArtist { get; set; }
            public string strTrack { get; set; }
            public string strAlbum { get; set; }
            public string strArtist { get; set; }
            public string intDuration { get; set; }
            public string strGenre { get; set; }
            public string strDescriptionEN { get; set; }
            public string strTrackThumb { get; set; }
            public string strTrackLyrics { get; set; }
            public string strMusicBrainzID { get; set; }
            public string intYearReleased { get; set; }
        }

        public sealed class TheAudioDbAlbumResponse
        {
            public List<TheAudioDbAlbum> album { get; set; } = new();
        }

        public sealed class TheAudioDbAlbum
        {
            public string idAlbum { get; set; }
            public string idArtist { get; set; }
            public string strAlbum { get; set; }
            public string strArtist { get; set; }
            public string intYearReleased { get; set; }
            public string strGenre { get; set; }
            public string strAlbumThumb { get; set; }
            public string strLabel { get; set; }
        }

        public sealed class TheAudioDbArtistResponse
        {
            public List<TheAudioDbArtist> artists { get; set; } = new();
        }

        public sealed class TheAudioDbArtist
        {
            public string idArtist { get; set; }
            public string strArtist { get; set; }
            public string strArtistThumb { get; set; }
            public string strArtistFanart { get; set; }
            public string strArtistWideThumb { get; set; }
        }
    }
}

