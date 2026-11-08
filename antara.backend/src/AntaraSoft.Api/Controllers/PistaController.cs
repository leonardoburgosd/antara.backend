using AntaraSoft.Api.Filters;
using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private const string directorioId = "1hWAXKy5ZW8Y4A9ch93EmMzAfC8pyeDbd";

        public PistaController(
            IGestionarPistaService gestionarpistaService,
            IAudioStorage audioStorage,
            IBibliotecaRepository bibliotecaRepository,
            IMetadatosPistaService metadatosPistaService,
            ISubidaPistaService subidaPistaService)
        {
            _gestionarPistaService = gestionarpistaService;
            _audioStorage = audioStorage;
            _bibliotecaRepository = bibliotecaRepository;
            _metadatosPistaService = metadatosPistaService;
            _subidaPistaService = subidaPistaService;
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

