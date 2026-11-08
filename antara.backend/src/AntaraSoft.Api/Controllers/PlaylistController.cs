using AntaraSoft.Api.Filters;
using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntaraSoft.Api.Controllers
{
    [Route("api/playlist")]
    [ApiController]
    public class PlaylistController : Controller
    {
        private readonly IGestionarPlaylistService _gestionarPlaylistService;
        private readonly IGestionarPistaService _gestionarPistaService;
        private const string directorioPortadas = "1eWGKxmVjUsvfH2PNpfO1N_CUT2A6SkM6";

        public PlaylistController(
            IGestionarPlaylistService gestionarPlaylistService,
            IGestionarPistaService gestionarPistaService)
        {
            _gestionarPlaylistService = gestionarPlaylistService;
            _gestionarPistaService = gestionarPistaService;
        }

        private Guid UsuarioActual => HttpContext.Items["UsuarioId"] is Guid id ? id : Guid.Empty;

        private bool EsAdministrador =>
            HttpContext.Items["UsuarioTipo"] is string tipo &&
            (string.Equals(tipo.Trim(), "admin", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(tipo.Trim(), "administrador", StringComparison.OrdinalIgnoreCase));

        /// <summary>Playlists con su número de canciones y de artistas distintos.</summary>
        [HttpGet("todos")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<List<PlaylistResumenDto>>> ObtenerTodasAsync()
        {
            try
            {
                return Ok(await _gestionarPlaylistService.ObtenerResumenes());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudieron obtener las playlists." });
            }
        }

        /// <summary>Playlists de un usuario; cada usuario solo ve las propias.</summary>
        [HttpGet("todos/{userId:guid}")]
        [TokenAuthorize("admin", "administrador", "usuario", "user")]
        public async Task<ActionResult<List<PlaylistDto>>> ObtenerTodasDeUsuarioAsync(Guid userId)
        {
            if (!EsAdministrador && UsuarioActual != userId)
            {
                return StatusCode(403, new { error = "No tienes permisos para ver las playlists de otro usuario." });
            }

            try
            {
                var playlists = await _gestionarPlaylistService.ObtenerTodosPlaylistDeUsuario(userId);
                return Ok(playlists.ConvertAll(playlist => playlist.AsDto()));
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudieron obtener las playlists." });
            }
        }

        [HttpGet("{id:guid}")]
        [TokenAuthorize("admin", "administrador", "usuario", "user")]
        public async Task<ActionResult<PlaylistDto>> ObtenerPlaylistAsync(Guid id)
        {
            try
            {
                var playlist = await _gestionarPlaylistService.ObtenerPlaylist(id);
                if (playlist == null)
                {
                    return NotFound(new { error = "La playlist no existe." });
                }

                if (!EsAdministrador && playlist.UsuarioId != UsuarioActual)
                {
                    return StatusCode(403, new { error = "No tienes permisos para ver esta playlist." });
                }

                return Ok(playlist.AsDto());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo obtener la playlist." });
            }
        }

        /// <summary>
        /// Canciones de la playlist con los datos de la biblioteca (artista,
        /// álbum, portada y duración), sin importar de qué artista provengan.
        /// </summary>
        [HttpGet("{id:guid}/pistas")]
        [TokenAuthorize("admin", "administrador", "usuario", "user")]
        public async Task<ActionResult<List<PistaBibliotecaDto>>> ObtenerPistasAsync(Guid id)
        {
            try
            {
                var playlist = await _gestionarPlaylistService.ObtenerPlaylist(id);
                if (playlist == null)
                {
                    return NotFound(new { error = "La playlist no existe." });
                }

                if (!EsAdministrador && playlist.UsuarioId != UsuarioActual)
                {
                    return StatusCode(403, new { error = "No tienes permisos para ver esta playlist." });
                }

                return Ok(await _gestionarPlaylistService.ObtenerPistasDePlaylist(id));
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudieron obtener las canciones de la playlist." });
            }
        }

        [HttpPost]
        [TokenAuthorize("admin", "administrador", "usuario", "user")]
        public async Task<ActionResult<PlaylistDto>> CrearPlaylistAsync(
            [FromForm] CrearAgrupacionDto crearPlaylistDto,
            [FromForm] IFormFile? imagenDePortada)
        {
            try
            {
                if (crearPlaylistDto == null || string.IsNullOrWhiteSpace(crearPlaylistDto.Nombre))
                {
                    return BadRequest(new { error = "El nombre de la playlist es obligatorio." });
                }

                var usuarioId = EsAdministrador && crearPlaylistDto.UsuarioId != Guid.Empty
                    ? crearPlaylistDto.UsuarioId
                    : UsuarioActual;

                Playlist playlistNueva = new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = crearPlaylistDto.Nombre.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(crearPlaylistDto.Descripcion) ? null : crearPlaylistDto.Descripcion.Trim(),
                    UsuarioId = usuarioId,
                    EstaActivo = true
                };

                if (imagenDePortada != null && imagenDePortada.Length > 0)
                {
                    string url = await Extensions.SubirArchivo(imagenDePortada, directorioPortadas);
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        return StatusCode(500, new { error = "No se pudo guardar la portada de la playlist." });
                    }
                    playlistNueva.PortadaUrl = url.Replace("&export=download", "");
                }
                else
                {
                    playlistNueva.PortadaUrl = string.IsNullOrWhiteSpace(crearPlaylistDto.PortadaUrl)
                        ? null
                        : crearPlaylistDto.PortadaUrl.Trim();
                }

                await _gestionarPlaylistService.CrearPlaylist(playlistNueva);
                return Created($"/api/playlist/{playlistNueva.Id}", playlistNueva.AsDto());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo crear la playlist." });
            }
        }

        [HttpPut("{id:guid}")]
        [TokenAuthorize("admin", "administrador", "usuario", "user")]
        public async Task<ActionResult<PlaylistDto>> EditarPlaylistAsync(
            Guid id,
            [FromForm] EditarAgrupacionDto editarPlaylistDto,
            [FromForm] IFormFile? imagenDePortada)
        {
            try
            {
                var playlist = await _gestionarPlaylistService.ObtenerPlaylist(id);
                if (playlist == null)
                {
                    return NotFound(new { error = "La playlist no existe." });
                }

                if (!EsAdministrador && playlist.UsuarioId != UsuarioActual)
                {
                    return StatusCode(403, new { error = "No tienes permisos para editar esta playlist." });
                }

                if (editarPlaylistDto == null || string.IsNullOrWhiteSpace(editarPlaylistDto.Nombre))
                {
                    return BadRequest(new { error = "El nombre de la playlist es obligatorio." });
                }

                Playlist playlistEditada = playlist with
                {
                    Nombre = editarPlaylistDto.Nombre.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(editarPlaylistDto.Descripcion) ? null : editarPlaylistDto.Descripcion.Trim(),
                    PortadaUrl = playlist.PortadaUrl
                };

                if (imagenDePortada != null && imagenDePortada.Length > 0)
                {
                    string url = await Extensions.SubirArchivo(imagenDePortada, directorioPortadas);
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        return StatusCode(500, new { error = "No se pudo guardar la portada de la playlist." });
                    }
                    playlistEditada.PortadaUrl = url.Replace("&export=download", "");
                }
                else if (!string.IsNullOrWhiteSpace(editarPlaylistDto.PortadaUrl))
                {
                    playlistEditada.PortadaUrl = editarPlaylistDto.PortadaUrl.Trim();
                }
                else if (editarPlaylistDto.QuitarPortada)
                {
                    // El formulario marca explícitamente cuando se quiere dejar sin portada.
                    playlistEditada.PortadaUrl = null;
                }

                await _gestionarPlaylistService.EditarPlaylist(playlistEditada);
                return Ok(playlistEditada.AsDto());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo actualizar la playlist." });
            }
        }

        [HttpDelete("{id:guid}")]
        [TokenAuthorize("admin", "administrador", "usuario", "user")]
        public async Task<ActionResult> EliminarPlaylistAsync(Guid id)
        {
            try
            {
                var playlist = await _gestionarPlaylistService.ObtenerPlaylist(id);
                if (playlist == null)
                {
                    return NotFound(new { error = "La playlist no existe." });
                }

                if (!EsAdministrador && playlist.UsuarioId != UsuarioActual)
                {
                    return StatusCode(403, new { error = "No tienes permisos para eliminar esta playlist." });
                }

                await _gestionarPlaylistService.EliminarPlaylist(id);
                return Ok(new { id });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo eliminar la playlist." });
            }
        }

        /// <summary>Agrega una canción de cualquier artista a la playlist.</summary>
        [HttpPost("{id:guid}/pistas")]
        [TokenAuthorize("admin", "administrador", "usuario", "user")]
        public async Task<ActionResult> AgregarPistaAsync(Guid id, [FromBody] AgregarPistaPlaylistDto dto)
        {
            try
            {
                if (dto == null || dto.PistaId == Guid.Empty)
                {
                    return BadRequest(new { error = "Selecciona una canción para agregar." });
                }

                var playlist = await _gestionarPlaylistService.ObtenerPlaylist(id);
                if (playlist == null)
                {
                    return NotFound(new { error = "La playlist no existe." });
                }

                if (!EsAdministrador && playlist.UsuarioId != UsuarioActual)
                {
                    return StatusCode(403, new { error = "No tienes permisos para editar esta playlist." });
                }

                var pista = await _gestionarPistaService.ObtenerPista(dto.PistaId);
                if (pista == null)
                {
                    return NotFound(new { error = "La canción no existe en la biblioteca." });
                }

                await _gestionarPlaylistService.AgregarPistaAPlaylist(new PlaylistPista
                {
                    PlaylistId = id,
                    PistaId = dto.PistaId
                });

                return Ok(await _gestionarPlaylistService.ObtenerPistasDePlaylist(id));
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo agregar la canción a la playlist." });
            }
        }

        [HttpDelete("{id:guid}/pistas/{pistaId:guid}")]
        [TokenAuthorize("admin", "administrador", "usuario", "user")]
        public async Task<ActionResult> QuitarPistaAsync(Guid id, Guid pistaId)
        {
            try
            {
                var playlist = await _gestionarPlaylistService.ObtenerPlaylist(id);
                if (playlist == null)
                {
                    return NotFound(new { error = "La playlist no existe." });
                }

                if (!EsAdministrador && playlist.UsuarioId != UsuarioActual)
                {
                    return StatusCode(403, new { error = "No tienes permisos para editar esta playlist." });
                }

                await _gestionarPlaylistService.QuitarPistaDePlaylist(new PlaylistPista
                {
                    PlaylistId = id,
                    PistaId = pistaId
                });

                return Ok(await _gestionarPlaylistService.ObtenerPistasDePlaylist(id));
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo quitar la canción de la playlist." });
            }
        }
    }
}
