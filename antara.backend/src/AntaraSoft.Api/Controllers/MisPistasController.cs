using AntaraSoft.Api.Filters;
using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Api.Controllers
{
    /// <summary>
    /// Pistas propias de un usuario. Siempre se suben a la ruta predeterminada
    /// y quedan ligadas al usuario autenticado; otro usuario solo puede verlas
    /// si es administrador.
    /// </summary>
    [Route("api/mis-pistas")]
    [ApiController]
    [TokenAuthorize("admin", "administrador", "usuario", "user")]
    public class MisPistasController : Controller
    {
        private readonly ISubidaPistaService _subidaPistaService;
        private readonly IBibliotecaRepository _bibliotecaRepository;
        private readonly IMetadatosPistaService _metadatosPistaService;

        public MisPistasController(
            ISubidaPistaService subidaPistaService,
            IBibliotecaRepository bibliotecaRepository,
            IMetadatosPistaService metadatosPistaService)
        {
            _subidaPistaService = subidaPistaService;
            _bibliotecaRepository = bibliotecaRepository;
            _metadatosPistaService = metadatosPistaService;
        }

        private Guid UsuarioActual =>
            HttpContext.Items["UsuarioId"] is Guid id ? id : Guid.Empty;

        private string TipoUsuario =>
            HttpContext.Items["UsuarioTipo"] as string ?? string.Empty;

        private bool EsAdministrador =>
            string.Equals(TipoUsuario, "admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(TipoUsuario, "administrador", StringComparison.OrdinalIgnoreCase);

        /// <summary>Lista las pistas propias; un administrador ve toda la biblioteca.</summary>
        [HttpGet]
        public async Task<ActionResult<List<PistaBibliotecaDto>>> ObtenerAsync()
        {
            try
            {
                if (EsAdministrador)
                    return Ok(await _bibliotecaRepository.ObtenerPistas());

                return Ok(await _bibliotecaRepository.ObtenerPistasDeUsuario(UsuarioActual));
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudieron obtener tus pistas." });
            }
        }

        /// <summary>Sube un archivo de audio propio a la ruta predeterminada.</summary>
        [HttpPost("subir")]
        public async Task<ActionResult<SubidaPistaDto>> SubirAsync(
            [FromForm] IFormFile archivo,
            [FromForm] string? subcarpeta,
            CancellationToken cancellationToken)
        {
            try
            {
                if (archivo == null || archivo.Length == 0)
                    return BadRequest(new { error = "Adjunta un archivo de audio para subir." });

                await using var contenido = archivo.OpenReadStream();
                var resultado = await _subidaPistaService.SubirPropiaAsync(
                    subcarpeta, archivo.FileName, contenido, UsuarioActual, cancellationToken);

                return Created($"/api/mis-pistas/{resultado.PistaId}", resultado);
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

        /// <summary>Descarga una canción de YouTube en la ruta predeterminada.</summary>
        [HttpPost("subir/url")]
        public async Task<ActionResult<SubidaPistaDto>> SubirDesdeUrlAsync(
            [FromBody] SubirMiPistaDesdeUrlDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Url))
                    return BadRequest(new { error = "Ingresa el enlace de YouTube de la canción." });

                var resultado = await _subidaPistaService.SubirPropiaDesdeUrlAsync(
                    dto.Subcarpeta, dto.Url, UsuarioActual, cancellationToken);

                return Created($"/api/mis-pistas/{resultado.PistaId}", resultado);
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

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PistaBibliotecaDto>> ObtenerPorIdAsync(Guid id)
        {
            var pista = await _bibliotecaRepository.ObtenerPistaBiblioteca(id);
            if (!PuedeAcceder(pista))
                return NotFound(new { error = "La pista no existe." });

            return Ok(pista);
        }

        /// <summary>
        /// Actualiza los datos editables de una pista propia: nombre, artista,
        /// imagen, año, género y álbum/portada.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<PistaBibliotecaDto>> ActualizarAsync(
            Guid id,
            [FromBody] ActualizarMiPistaDto dto)
        {
            if (dto == null)
                return BadRequest(new { error = "No se recibieron los datos de la pista." });

            var actual = await _bibliotecaRepository.ObtenerPistaBiblioteca(id);
            if (!PuedeAcceder(actual))
                return NotFound(new { error = "La pista no existe." });

            try
            {
                var propietarioId = actual.UsuarioId ?? UsuarioActual;
                var nombre = string.IsNullOrWhiteSpace(dto.Nombre) ? actual.Nombre : dto.Nombre.Trim();
                var interprete = string.IsNullOrWhiteSpace(dto.Interprete)
                    ? actual.Interprete
                    : dto.Interprete.Trim();

                var albumId = actual.AlbumId;
                if (!string.IsNullOrWhiteSpace(dto.AlbumNombre) ||
                    !string.IsNullOrWhiteSpace(dto.PortadaUrl))
                {
                    var albumNombre = string.IsNullOrWhiteSpace(dto.AlbumNombre)
                        ? actual.AlbumNombre
                        : dto.AlbumNombre.Trim();
                    var portada = string.IsNullOrWhiteSpace(dto.PortadaUrl)
                        ? actual.PortadaUrl
                        : dto.PortadaUrl.Trim();

                    albumId = await _bibliotecaRepository.ObtenerOCrearAlbum(new Album
                    {
                        Id = Guid.NewGuid(),
                        Nombre = albumNombre,
                        Descripcion = $"Álbum de {interprete}",
                        EstaPublicado = false,
                        PortadaUrl = portada,
                        UsuarioId = propietarioId,
                        EstaActivo = true,
                        Artista = interprete
                    }, soloPropietario: true);

                    if (!string.IsNullOrWhiteSpace(dto.PortadaUrl))
                    {
                        await _bibliotecaRepository.ActualizarPortadaAlbumDeUsuario(
                            albumId, propietarioId, dto.PortadaUrl.Trim());
                    }
                }

                var generoId = actual.GeneroId;
                if (!string.IsNullOrWhiteSpace(dto.Genero))
                    generoId = await _bibliotecaRepository.ObtenerOCrearGenero(dto.Genero.Trim());

                var pista = new Pista
                {
                    Id = actual.Id,
                    Nombre = nombre,
                    FechaRegistro = actual.FechaRegistro,
                    AnoCreacion = dto.AnoCreacion ?? actual.AnoCreacion,
                    Interprete = interprete,
                    Reproducciones = actual.Reproducciones,
                    GeneroId = generoId,
                    Url = actual.Url,
                    AlbumId = albumId,
                    UsuarioId = actual.UsuarioId,
                    EstaActivo = actual.EstaActivo,
                    ImagenArtista = dto.ImagenArtista is null
                        ? actual.ImagenArtista
                        : dto.ImagenArtista.Trim()
                };

                await _bibliotecaRepository.ActualizarDatosPista(pista);
                return Ok(await _bibliotecaRepository.ObtenerPistaBiblioteca(id));
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
                return StatusCode(500, new { error = "No se pudieron actualizar los datos de la pista." });
            }
        }

        /// <summary>Proveedores habilitados para buscar una nueva coincidencia.</summary>
        [HttpGet("metadatos/proveedores")]
        public ActionResult<List<ProveedorMetadatosDto>> ObtenerProveedoresMetadatos()
        {
            return Ok(_metadatosPistaService.ObtenerProveedores());
        }

        /// <summary>Busca metadatos entre todos los proveedores configurados.</summary>
        [HttpGet("metadatos/buscar")]
        public async Task<ActionResult<List<MetadatosCandidatoDto>>> BuscarMetadatosAsync(
            [FromQuery] string cadena,
            [FromQuery] string? proveedor,
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

        /// <summary>Reasigna los metadatos de una pista propia desde un candidato.</summary>
        [HttpPost("{id:guid}/metadatos")]
        public async Task<ActionResult<PistaBibliotecaDto>> ActualizarMetadatosAsync(
            Guid id,
            [FromBody] ActualizarMetadatosPistaDto dto,
            CancellationToken cancellationToken)
        {
            var actual = await _bibliotecaRepository.ObtenerPistaBiblioteca(id);
            if (!PuedeAcceder(actual))
                return NotFound(new { error = "La pista no existe." });

            try
            {
                var propietarioId = actual.UsuarioId ?? UsuarioActual;
                return Ok(await _metadatosPistaService.ActualizarAsync(
                    id, dto?.IdExterno, dto?.Proveedor, propietarioId, cancellationToken));
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

        [HttpGet("{id:guid}/letra-sincronizada")]
        public async Task<ActionResult> ObtenerLetraSincronizadaAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var actual = await _bibliotecaRepository.ObtenerPistaBiblioteca(id);
            if (!PuedeAcceder(actual))
                return NotFound(new { error = "La pista no existe." });

            try
            {
                var letra = await _metadatosPistaService.ObtenerLetraSincronizadaAsync(id, cancellationToken);
                if (string.IsNullOrWhiteSpace(letra))
                    return NotFound(new { error = "Esta canción no tiene una letra sincronizada disponible." });

                return Ok(new { letraSincronizada = letra });
            }
            catch (KeyNotFoundException err)
            {
                return NotFound(new { error = err.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo obtener la letra sincronizada." });
            }
        }

        private bool PuedeAcceder(PistaBibliotecaDto pista)
        {
            if (pista == null)
                return false;

            if (EsAdministrador)
                return true;

            return pista.UsuarioId.HasValue && pista.UsuarioId.Value == UsuarioActual;
        }
    }
}
