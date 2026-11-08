using AntaraSoft.Api.Filters;
using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AntaraSoft.Api.Controllers
{
    [Route("api/album")]
    [ApiController]
    public class AlbumController : Controller
    {
        private readonly IGestionarAlbumService _gestionarAlbumService;

        public AlbumController(IGestionarAlbumService gestionarAlbumService)
        {
            _gestionarAlbumService = gestionarAlbumService;
        }

        /// <summary>Álbumes registrados, para el CRUD del panel.</summary>
        [HttpGet("todos")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<List<AlbumDto>>> ObtenerTodosAlbumesAsync()
        {
            try
            {
                var albumes = await _gestionarAlbumService.ObtenerTodosAlbumes();
                return Ok(albumes.Select(album => album.AsDto()).ToList());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudieron obtener los álbumes." });
            }
        }

        [HttpPost]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<AlbumDto>> CrearAlbumAsync([FromBody] CrearAlbumDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Nombre))
                    return BadRequest(new { error = "El nombre del álbum es obligatorio." });

                var usuarioId = UsuarioActualId();
                if (usuarioId == Guid.Empty)
                    return Unauthorized(new { error = "No se pudo identificar al usuario autenticado." });

                Album albumNuevo = new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = dto.Nombre.Trim(),
                    Descripcion = dto.Descripcion?.Trim(),
                    Artista = dto.Artista?.Trim(),
                    PortadaUrl = dto.PortadaUrl?.Trim(),
                    EstaPublicado = dto.EstaPublicado ?? false,
                    FechaPublicacion = DateTime.Now,
                    UsuarioId = usuarioId,
                    EstaActivo = true
                };

                await _gestionarAlbumService.CrearAlbum(albumNuevo);
                var creado = await _gestionarAlbumService.ObtenerAlbum(albumNuevo.Id);
                // ASP.NET recorta el sufijo "Async" del nombre de la accion.
                return CreatedAtAction("ObtenerAlbum", new { id = albumNuevo.Id }, creado.AsDto());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo crear el álbum." });
            }
        }

        [HttpPut("{id:guid}")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<AlbumDto>> EditarAlbumAsync(Guid id, [FromBody] ActualizarAlbumDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { error = "Datos del álbum requeridos." });

                var album = await _gestionarAlbumService.ObtenerAlbum(id);
                if (album == null)
                    return NotFound(new { error = "El álbum no existe." });

                if (dto.Nombre != null && string.IsNullOrWhiteSpace(dto.Nombre))
                    return BadRequest(new { error = "El nombre del álbum no puede estar vacío." });

                var albumEditado = album with
                {
                    Nombre = dto.Nombre?.Trim() ?? album.Nombre,
                    Descripcion = dto.Descripcion ?? album.Descripcion,
                    PortadaUrl = dto.PortadaUrl ?? album.PortadaUrl,
                    Artista = dto.Artista ?? album.Artista,
                    EstaPublicado = dto.EstaPublicado ?? album.EstaPublicado,
                    EstaActivo = dto.EstaActivo ?? album.EstaActivo
                };

                await _gestionarAlbumService.EditarAlbum(albumEditado);
                return Ok((await _gestionarAlbumService.ObtenerAlbum(id)).AsDto());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo actualizar el álbum." });
            }
        }

        [HttpDelete("{id:guid}")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult> EliminarAlbumAsync(Guid id)
        {
            try
            {
                var album = await _gestionarAlbumService.ObtenerAlbum(id);
                if (album == null)
                    return NotFound(new { error = "El álbum no existe." });

                await _gestionarAlbumService.EliminarAlbum(id);
                return Ok(new { mensaje = "Álbum eliminado correctamente." });
            }
            catch (Exception)
            {
                return BadRequest(new
                {
                    error = "No se pudo eliminar el álbum. Puede tener pistas asociadas; quita la relación antes de borrarlo."
                });
            }
        }

        /// <summary>Publica o despublica un álbum (alterna el estado actual).</summary>
        [HttpPut("{id:guid}/publicar")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<AlbumDto>> PublicarAlbumAsync(Guid id)
        {
            try
            {
                var album = await _gestionarAlbumService.ObtenerAlbum(id);
                if (album == null)
                    return NotFound(new { error = "El álbum no existe." });

                await _gestionarAlbumService.PublicarAlbum(album with
                {
                    EstaPublicado = !album.EstaPublicado
                });

                return Ok((await _gestionarAlbumService.ObtenerAlbum(id)).AsDto());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo cambiar la publicación del álbum." });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AlbumDto>> ObtenerAlbumAsync(Guid id)
        {
            try
            {
                var album = await _gestionarAlbumService.ObtenerAlbum(id);
                if (album == null)
                    return NotFound();
                return Ok(album.AsDto());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo obtener el álbum." });
            }
        }

        [HttpGet("todos/{userId:guid}")]
        public async Task<ActionResult<List<AlbumDto>>> ObtenerTodosAlbumesDeUsuarioAsync(Guid userId)
        {
            try
            {
                var albumList = (await _gestionarAlbumService.ObtenerTodosAlbumesDeUsuario(userId))
                    .Select(item => item.AsDto())
                    .ToList();
                return Ok(albumList);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudieron obtener los álbumes del usuario." });
            }
        }

        private Guid UsuarioActualId() =>
            HttpContext.Items["UsuarioId"] is Guid valor ? valor : Guid.Empty;
    }
}
