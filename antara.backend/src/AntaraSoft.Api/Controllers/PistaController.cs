using AntaraSoft.Api.Filters;
using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Api.Controllers
{
    [Route("api/pista")]
    [ApiController]
    public class PistaController : Controller
    {
        private readonly IGestionarPistaService _gestionarPistaService;
        private readonly IAudioStorage _audioStorage;
        private readonly IBibliotecaRepository _bibliotecaRepository;
        private readonly IMetadatosPistaService _metadatosPistaService;
        private readonly ISubidaPistaService _subidaPistaService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _entorno;
        private const string directorioId = "1hWAXKy5ZW8Y4A9ch93EmMzAfC8pyeDbd";
        private const int MaximoPortadaBytes = 10 * 1024 * 1024;

        public PistaController(
            IGestionarPistaService gestionarpistaService,
            IAudioStorage audioStorage,
            IBibliotecaRepository bibliotecaRepository,
            IMetadatosPistaService metadatosPistaService,
            ISubidaPistaService subidaPistaService,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment entorno)
        {
            _gestionarPistaService = gestionarpistaService;
            _audioStorage = audioStorage;
            _bibliotecaRepository = bibliotecaRepository;
            _metadatosPistaService = metadatosPistaService;
            _subidaPistaService = subidaPistaService;
            _httpClientFactory = httpClientFactory;
            _entorno = entorno;
        }

        /// <summary>Rutas registradas que pueden recibir una subida desde el panel.</summary>
        [HttpGet("subir/rutas")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<List<RutaSubidaDto>>> ObtenerRutasDeSubidaAsync()
        {
            try
            {
                return Ok(await _subidaPistaService.ObtenerRutasAsync());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudieron obtener las rutas de audio." });
            }
        }

        /// <summary>
        /// Sube un archivo de audio a la ruta indicada (o a la predeterminada) y lo
        /// registra en la biblioteca. Solo se aceptan extensiones de audio.
        /// </summary>
        [HttpPost("subir")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<SubidaPistaDto>> SubirPistaAsync(
            [FromForm] Guid? rutaId,
            [FromForm] string? subcarpeta,
            [FromForm] IFormFile archivo,
            CancellationToken cancellationToken)
        {
            try
            {
                if (archivo == null || archivo.Length == 0)
                    return BadRequest(new { error = "Adjunta un archivo de audio para subir." });

                var usuarioId = HttpContext.Items["UsuarioId"] is Guid valor ? valor : Guid.Empty;

                await using var contenido = archivo.OpenReadStream();
                var resultado = await _subidaPistaService.SubirAsync(
                    rutaId, subcarpeta, archivo.FileName, contenido, usuarioId, cancellationToken);

                return Created($"/api/pista/{resultado.PistaId}", resultado);
            }
            catch (KeyNotFoundException err)
            {
                return NotFound(new { error = err.Message });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new { error = err.Message });
            }
            catch (InvalidOperationException err)
            {
                return BadRequest(new { error = err.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo subir la canción." });
            }
        }

        [HttpPost]
        [RoleAuthorize("admin", "administrador")]
        public async Task<ActionResult<PistaDto>> CrearPistaAsync([FromForm] CrearPistaDto pistaDto, [FromForm] IFormFile archivo)
        {
            try
            {
                Pista pistaNueva = new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = string.IsNullOrEmpty(pistaDto.Url) && string.IsNullOrEmpty(pistaDto.RutaFisica) && archivo != null
                        ? Path.GetFileNameWithoutExtension(archivo.FileName)
                        : "SinNombre",
                    FechaRegistro = DateTime.Now,
                    AnoCreacion = pistaDto.AnoCreacion,
                    Interprete = pistaDto.Interprete,
                    Compositor = pistaDto.Compositor,
                    Productor = pistaDto.Productor,
                    Reproducciones = 0,
                    GeneroId = pistaDto.GeneroId,
                    AlbumId = pistaDto.AlbumId,
                    EstaActivo = true
                };

                if (!string.IsNullOrEmpty(pistaDto.Url))
                {
                    pistaNueva.Url = pistaDto.Url;
                }
                else if (!string.IsNullOrEmpty(pistaDto.RutaFisica))
                {
                    pistaNueva.Url = pistaDto.RutaFisica;
                }
                else if (archivo != null)
                {
                    string url = await Extensions.SubirArchivo(archivo, directorioId, "audio/mpeg");
                    if (url != null)
                    {
                        pistaNueva.Url = url.Replace("&export=download", "");
                    }
                    else
                    {
                        throw new ArgumentNullException(nameof(url), "No se pudo completar la carga del archivo");
                    }
                }
                else
                {
                    throw new ArgumentNullException(nameof(archivo), "Debe proporcionar un archivo o una ruta física");
                }

                if (_gestionarPistaService.SonDatosValidos(pistaNueva))
                {
                    await _gestionarPistaService.CrearPista(pistaNueva);
                    return CreatedAtAction("ObtenerPista", new { id = pistaNueva.Id }, pistaNueva.AsDto());
                }
                throw new ArgumentException("Argumentos invalidos", nameof(pistaDto));
            }
            catch (Exception err)
            {
                if (err.Message.Contains("No se pudo crear la pista"))
                {
                    return StatusCode(409, Json(new { error = err.Message }));
                }
                else return StatusCode(500, err.Message.ToString());
            }
        }

        /// <summary>
        /// Descarga el audio de un enlace de YouTube y lo guarda en la ruta
        /// indicada, registrándolo en la biblioteca.
        /// </summary>
        [HttpPost("subir/url")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<SubidaPistaDto>> SubirDesdeUrlAsync(
            [FromBody] SubidaDesdeUrlDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Url))
                    return BadRequest(new { error = "Ingresa el enlace de YouTube de la canción." });

                var usuarioId = HttpContext.Items["UsuarioId"] is Guid valor ? valor : Guid.Empty;
                var resultado = await _subidaPistaService.SubirDesdeUrlAsync(
                    dto.RutaId, dto.Subcarpeta, dto.Url, usuarioId, cancellationToken);

                return Created($"/api/pista/{resultado.PistaId}", resultado);
            }
            catch (KeyNotFoundException err)
            {
                return NotFound(new { error = err.Message });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new { error = err.Message });
            }
            catch (InvalidOperationException err)
            {
                return BadRequest(new { error = err.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo descargar la canción de YouTube." });
            }
        }

        [HttpGet("todos/album/{AlbumId}")]
        public async Task<ActionResult<List<PistaDto>>> ObtenerTodosPistasDeAlbumAsync(Guid AlbumId)
        {
            try
            {
                var pistasList = (await _gestionarPistaService.ObtenerTodosPistasDeAlbum(AlbumId)).Select(item => item.AsDto());
                return StatusCode(200, pistasList);
            }
            catch (Exception err)
            {
                return StatusCode(500, err);
            }
        }

        [HttpGet("todos/playlist/{PlaylistId}")]
        public async Task<ActionResult<List<PistaDto>>> ObtenerTodosPistasDePlaylistAsync(Guid PlaylistId)
        {
            try
            {
                var pistasList = (await _gestionarPistaService.ObtenerTodosPistasDePlaylist(PlaylistId)).Select(item => item.AsDto());
                return StatusCode(200, pistasList);
            }
            catch (Exception err)
            {
                return StatusCode(500, err);
            }
        }

        [HttpGet("todos")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<List<PistaBibliotecaDto>>> ObtenerBibliotecaAsync()
        {
            try
            {
                return Ok(await _bibliotecaRepository.ObtenerPistas());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo obtener la lista de pistas." });
            }
        }

        /// <summary>
        /// Proveedores de metadatos habilitados según la configuración. El panel
        /// arma su selector con esta lista, sin nombres fijos en el frontend.
        /// </summary>
        [HttpGet("metadatos/proveedores")]
        [TokenAuthorize("admin", "administrador")]
        public ActionResult<List<ProveedorMetadatosDto>> ObtenerProveedoresMetadatos()
        {
            return Ok(_metadatosPistaService.ObtenerProveedores());
        }

        [HttpGet("metadatos/buscar")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<List<MetadatosCandidatoDto>>> BuscarMetadatosAsync(
            [FromQuery] string cadena,
            [FromQuery] string proveedor,
            CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await _metadatosPistaService.BuscarAsync(cadena, proveedor, cancellationToken));
            }
            catch (ArgumentException err)
            {
                return BadRequest(new { error = err.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudieron buscar los metadatos." });
            }
        }

        [HttpPost("{id:guid}/metadatos")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<PistaBibliotecaDto>> ActualizarMetadatosAsync(
            Guid id,
            [FromBody] ActualizarMetadatosPistaDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                var usuarioId = HttpContext.Items["UsuarioId"] is Guid valor ? valor : Guid.Empty;
                return Ok(await _metadatosPistaService.ActualizarAsync(id, dto?.IdExterno, dto?.Proveedor, usuarioId, cancellationToken));
            }
            catch (KeyNotFoundException err)
            {
                return NotFound(new { error = err.Message });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new { error = err.Message });
            }
            catch (InvalidOperationException err)
            {
                return BadRequest(new { error = err.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudieron actualizar los metadatos de la pista." });
            }
        }

        /// <summary>
        /// Letra con marcas de tiempo (LRC) para resaltar la linea que suena.
        /// Responde 404 si la cancion no tiene una version sincronizada.
        /// </summary>
        [HttpGet("{id:guid}/letra-sincronizada")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult> ObtenerLetraSincronizadaAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var letra = await _metadatosPistaService.ObtenerLetraSincronizadaAsync(id, cancellationToken);
                if (string.IsNullOrWhiteSpace(letra))
                {
                    return NotFound(new
                    {
                        error = "Esta canción no tiene una letra sincronizada disponible."
                    });
                }

                return Ok(new { letraSincronizada = letra });
            }
            catch (KeyNotFoundException err)
            {
                return NotFound(new { error = err.Message });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new { error = err.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo obtener la letra sincronizada." });
            }
        }

        /// <summary>
        /// Descarga una pista como ZIP con todo lo necesario para usarla offline:
        /// audio original, portada del album, letra, letra sincronizada (si existe)
        /// y los datos de la pista y su album en <c>pista.json</c>.
        /// </summary>
        [HttpGet("{id:guid}/descarga")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<IActionResult> DescargarPistaAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                // ZipArchive escribe el directorio central de forma síncrona al
                // cerrarse; Kestrel solo lo permite si se habilita por respuesta.
                var controlCuerpo = HttpContext.Features.Get<IHttpBodyControlFeature>();
                if (controlCuerpo != null) controlCuerpo.AllowSynchronousIO = true;

                var pista = await _bibliotecaRepository.ObtenerPistaBiblioteca(id);
                if (pista == null)
                    return NotFound(new { error = "La pista no existe." });

                var rutaAudio = _audioStorage.ResolvePhysicalPath(pista.Url);
                if (string.IsNullOrEmpty(rutaAudio) || !System.IO.File.Exists(rutaAudio))
                    return NotFound(new { error = "El archivo de audio no está disponible en el servidor." });

                string letraSincronizada = null;
                try
                {
                    letraSincronizada = await _metadatosPistaService
                        .ObtenerLetraSincronizadaAsync(id, cancellationToken);
                }
                catch (Exception) when (!cancellationToken.IsCancellationRequested)
                {
                    // La letra sincronizada es opcional para el paquete.
                }

                var (portada, extensionPortada) = await ObtenerPortadaAsync(pista.PortadaUrl, cancellationToken);

                var nombreBase = LimpiarNombreArchivo($"{pista.Interprete} - {pista.Nombre}", "pista");
                var extensionAudio = Path.GetExtension(rutaAudio);
                if (string.IsNullOrWhiteSpace(extensionAudio)) extensionAudio = ".mp3";

                var nombreAudio = $"{nombreBase}{extensionAudio.ToLowerInvariant()}";
                var nombrePortada = portada == null ? null : $"portada{extensionPortada}";
                var tieneLetra = !string.IsNullOrWhiteSpace(pista.Letra);
                var tieneLetraSincronizada = !string.IsNullOrWhiteSpace(letraSincronizada);
                var nombreZip = $"{nombreBase}.zip";

                Response.ContentType = "application/zip";
                Response.Headers["Content-Disposition"] =
                    $"attachment; filename=\"{NombreAscii(nombreZip)}\"; filename*=UTF-8''{Uri.EscapeDataString(nombreZip)}";

                using (var zip = new ZipArchive(Response.Body, ZipArchiveMode.Create, leaveOpen: true))
                {
                    var entradaAudio = zip.CreateEntry(nombreAudio, CompressionLevel.NoCompression);
                    await using (var destino = entradaAudio.Open())
                    await using (var origen = System.IO.File.OpenRead(rutaAudio))
                    {
                        await origen.CopyToAsync(destino, 81920, cancellationToken);
                    }

                    if (portada != null)
                    {
                        var entradaPortada = zip.CreateEntry(nombrePortada, CompressionLevel.NoCompression);
                        await using var destinoPortada = entradaPortada.Open();
                        await destinoPortada.WriteAsync(portada.AsMemory(), cancellationToken);
                    }

                    if (tieneLetra)
                        await EscribirTextoEnZipAsync(zip, "letra.txt", pista.Letra, cancellationToken);

                    if (tieneLetraSincronizada)
                        await EscribirTextoEnZipAsync(zip, "letra.lrc", letraSincronizada, cancellationToken);

                    var informacion = new
                    {
                        pista = new
                        {
                            pista.Id,
                            pista.Nombre,
                            pista.Interprete,
                            album = new
                            {
                                id = pista.AlbumId,
                                nombre = pista.AlbumNombre,
                                artista = pista.Interprete,
                                portada = pista.PortadaUrl
                            },
                            pista.AnoCreacion,
                            genero = pista.GeneroNombre,
                            duracionSegundos = pista.DuracionSegundos,
                            duracionMinutos = Math.Round(pista.DuracionSegundos / 60d, 2),
                            duracion = FormatoDuracion.Formatear(pista.DuracionSegundos),
                            pista.Reproducciones,
                            archivoAudio = nombreAudio,
                            archivoPortada = nombrePortada,
                            archivoLetra = tieneLetra ? "letra.txt" : null,
                            archivoLetraSincronizada = tieneLetraSincronizada ? "letra.lrc" : null,
                            letra = pista.Letra,
                            letraSincronizada
                        }
                    };

                    var entradaJson = zip.CreateEntry("pista.json", CompressionLevel.Optimal);
                    await using (var stream = entradaJson.Open())
                    {
                        await JsonSerializer.SerializeAsync(stream, informacion, OpcionesJson, cancellationToken);
                    }
                }

                return new EmptyResult();
            }
            catch (OperationCanceledException)
            {
                // El cliente canceló la descarga; la respuesta ya no se puede completar.
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error al preparar la descarga de la pista {id}: {ex}");
                if (Response.HasStarted)
                    return new EmptyResult();

                return StatusCode(500, new { error = "No se pudo preparar la descarga de la pista." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPistaAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest();
                }

                var pista = await _gestionarPistaService.ObtenerPista(id);
                if (pista == null)
                {
                    return NotFound();
                }

                var filePath = _audioStorage.ResolvePhysicalPath(pista.Url);
                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                var contentType = GetContentType(filePath);
                Response.Headers["Accept-Ranges"] = "bytes";
                Response.Headers["X-Content-Type-Options"] = "nosniff";

                return PhysicalFile(filePath, contentType, enableRangeProcessing: true);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        private static readonly JsonSerializerOptions OpcionesJson = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        private static async Task EscribirTextoEnZipAsync(
            ZipArchive zip,
            string nombre,
            string contenido,
            CancellationToken cancellationToken)
        {
            var entrada = zip.CreateEntry(nombre, CompressionLevel.Optimal);
            await using var stream = entrada.Open();
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            await writer.WriteAsync(contenido.AsMemory(), cancellationToken);
        }

        /// <summary>
        /// Obtiene la portada del album desde una URL remota o desde un archivo
        /// local de <c>wwwroot</c>. Si no se puede leer, el paquete se genera igual.
        /// </summary>
        private async Task<(byte[] Contenido, string Extension)> ObtenerPortadaAsync(
            string portadaUrl,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(portadaUrl))
                return (null, null);

            if (Uri.TryCreate(portadaUrl, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                try
                {
                    using var cliente = _httpClientFactory.CreateClient("metadatos");
                    using var respuesta = await cliente.GetAsync(
                        uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                    if (!respuesta.IsSuccessStatusCode)
                        return (null, null);

                    var tipo = respuesta.Content.Headers.ContentType?.MediaType ?? string.Empty;
                    if (!tipo.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                        return (null, null);
                    if (respuesta.Content.Headers.ContentLength > MaximoPortadaBytes)
                        return (null, null);

                    await using var origen = await respuesta.Content.ReadAsStreamAsync(cancellationToken);
                    var contenido = await LeerConLimiteAsync(origen, MaximoPortadaBytes, cancellationToken);
                    return contenido == null ? (null, null) : (contenido, ExtensionDeImagen(tipo, uri.AbsolutePath));
                }
                catch (Exception) when (!cancellationToken.IsCancellationRequested)
                {
                    return (null, null);
                }
            }

            if (portadaUrl.StartsWith("/", StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(_entorno.WebRootPath))
            {
                try
                {
                    var raiz = Path.GetFullPath(_entorno.WebRootPath);
                    var ruta = Path.GetFullPath(Path.Combine(raiz, portadaUrl.TrimStart('/')));

                    if (!ruta.StartsWith(raiz, StringComparison.OrdinalIgnoreCase) ||
                        !System.IO.File.Exists(ruta))
                        return (null, null);

                    var info = new FileInfo(ruta);
                    if (info.Length > MaximoPortadaBytes)
                        return (null, null);

                    var contenido = await System.IO.File.ReadAllBytesAsync(ruta, cancellationToken);
                    return (contenido, ExtensionDeImagen(null, ruta));
                }
                catch (Exception) when (!cancellationToken.IsCancellationRequested)
                {
                    return (null, null);
                }
            }

            return (null, null);
        }

        private static async Task<byte[]> LeerConLimiteAsync(
            Stream origen,
            int maximo,
            CancellationToken cancellationToken)
        {
            using var memoria = new MemoryStream();
            var buffer = new byte[81920];
            int leidos;

            while ((leidos = await origen.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                if (memoria.Length + leidos > maximo)
                    return null;

                await memoria.WriteAsync(buffer.AsMemory(0, leidos), cancellationToken);
            }

            return memoria.ToArray();
        }

        private static string ExtensionDeImagen(string tipoContenido, string ruta)
        {
            var porTipo = tipoContenido?.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                _ => null
            };

            if (porTipo != null)
                return porTipo;

            var extension = Path.GetExtension(ruta)?.ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => ".jpg",
                ".png" => ".png",
                ".webp" => ".webp",
                ".gif" => ".gif",
                _ => ".jpg"
            };
        }

        private static string LimpiarNombreArchivo(string valor, string respaldo)
        {
            var invalidos = Path.GetInvalidFileNameChars();
            var limpio = new string((valor ?? string.Empty)
                .Select(c => invalidos.Contains(c) || c == '\n' || c == '\r' ? '_' : c)
                .ToArray())
                .Trim();

            if (string.IsNullOrWhiteSpace(limpio))
                limpio = respaldo;

            return limpio.Length > 100 ? limpio.Substring(0, 100).Trim() : limpio;
        }

        private static string NombreAscii(string nombre) =>
            new(nombre.Select(c => c <= 127 ? c : '_').ToArray());

        private static string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".mp3" => "audio/mpeg",
                ".flac" => "audio/flac",
                ".wav" => "audio/wav",
                ".m4a" => "audio/mp4",
                ".aac" => "audio/aac",
                ".ogg" => "audio/ogg",
                ".opus" => "audio/opus",
                ".wma" => "audio/x-ms-wma",
                _ => "application/octet-stream"
            };
        }

        [HttpPut]
        [RoleAuthorize("admin", "administrador")]
        public async Task<ActionResult> EditarPistaAsync(Guid id, EditarPistaDto pistaDto)
        {
            try
            {
                var pista = await _gestionarPistaService.ObtenerPista(id);
                if (pista == null)
                {
                    return NotFound();
                }
                Pista pistaEditada = pista with
                {
                    Nombre = pistaDto.Nombre,
                    AnoCreacion = pistaDto.AnoCreacion,
                    Interprete = pistaDto.Interprete,
                    Compositor = pistaDto.Compositor,
                    Productor = pistaDto.Productor,
                    GeneroId = pistaDto.GeneroId,
                };
                await _gestionarPistaService.EditarPista(pistaEditada);
                return StatusCode(200);
            }
            catch (Exception err)
            {
                return StatusCode(500, err);
            }
        }

        [HttpDelete("{id}")]
        [RoleAuthorize("admin", "administrador")]
        public async Task<ActionResult> EliminarPistaAsync(Guid id)
        {
            try
            {
                var pista = await _gestionarPistaService.ObtenerPista(id);
                if (pista == null)
                {
                    return NotFound();
                }
                await _gestionarPistaService.EliminarPista(id);
                return StatusCode(200);
            }
            catch (Exception err)
            {
                return StatusCode(500, err);
            }
        }

        [HttpGet("/buscar")]
        public async Task<ActionResult<List<PistaDto>>> SearchpistaAsync([Bind(Prefix = "cadena")] string cadena)
        {
            try
            {
                var pistaLista = (await _gestionarPistaService.BuscarPistas(cadena)).Select(item => item.AsDto());
                return StatusCode(200, pistaLista);
            }
            catch (Exception err)
            {
                return StatusCode(500, err);
            }
        }

        [HttpPut("/reproducir/{id}")]
        public async Task<ActionResult> ReproducirPista(Guid id)
        {
            try
            {
                var pista = await _gestionarPistaService.ObtenerPista(id);
                if (pista == null)
                {
                    return NotFound();
                }
                await _gestionarPistaService.ReproducirPista(pista);
                return StatusCode(200);
            }
            catch (Exception err)
            {
                return StatusCode(500, err);
            }
        }
    }
}

