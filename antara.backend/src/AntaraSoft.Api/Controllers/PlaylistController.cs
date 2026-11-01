using AntaraSoft.Api.Filters;
using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AntaraSoft.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController : Controller
    {
        private readonly IGestionarPlaylistService _gestionarPlaylistService;
        private const string directorioId = "1eWGKxmVjUsvfH2PNpfO1N_CUT2A6SkM6";

        public PlaylistController(IGestionarPlaylistService gestionarPlaylistService)
        {
            _gestionarPlaylistService = gestionarPlaylistService;
        }

        [HttpPost]
        [RoleAuthorize("admin", "administrador", "usuario", "user")]
        public async Task<ActionResult<PlaylistDto>> CrearPlaylistAsync([FromForm] CrearAgrupacionDto crearPlaylistDto, [FromForm] IFormFile imagenDePortada)
        {
            try
            {
                Playlist playlistNueva = new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = crearPlaylistDto.Nombre,
                    Descripcion = crearPlaylistDto.Descripcion,
                    UsuarioId = crearPlaylistDto.UsuarioId,
                    EstaActivo = true
                };
                if (imagenDePortada == null)
                {
                    playlistNueva.PortadaUrl = null;
                }
                else
                {
                    string url = await Extensions.SubirArchivo(imagenDePortada, directorioId);
                    playlistNueva.PortadaUrl = url.Replace("&export=download", "");
                }
                await _gestionarPlaylistService.CrearPlaylist(playlistNueva);
                return CreatedAtAction("ObtenerPlaylist", new { id = playlistNueva.Id }, playlistNueva.AsDto());
            }
            catch (Exception err)
            {
                if (err.Message.Contains("No se pudo crear"))
                {
                    return StatusCode(409, Json(new { error = err.Message }));
                }
                else return StatusCode(500, err.Message);
            }
        }

        [HttpGet("{id}")]
        [RoleAuthorize("admin", "administrador", "usuario", "user")]
        public async Task<ActionResult<PlaylistDto>> ObtenerPlaylistAsync(Guid id)
        {
            try
            {
                var tipoUsuario = Request.Headers["X-Usuario-Tipo"].ToString();
                if (string.IsNullOrWhiteSpace(tipoUsuario)) tipoUsuario = Request.Headers["Tipo"].ToString();
                var tipoNormalizado = tipoUsuario.Trim().ToLower();

                var playlist = await _gestionarPlaylistService.ObtenerPlaylist(id);
                if (playlist == null)
                {
                    return NotFound();
                }

                if (tipoNormalizado != "admin" && tipoNormalizado != "administrador")
                {
                    var requestUserId = Request.Headers["X-Usuario-Id"].ToString();
                    if (string.IsNullOrWhiteSpace(requestUserId) || !Guid.TryParse(requestUserId, out var reqId) || reqId != playlist.UsuarioId)
                    {
                        return StatusCode(403);
                    }
                }

                return StatusCode(200, playlist.AsDto());
            }
            catch (Exception err)
            {
                return StatusCode(500, err.Message);
            }
        }

        [HttpGet("todos/{userId}")]
        [RoleAuthorize("admin", "administrador")]
        public async Task<ActionResult<List<PlaylistDto>>> ObtenerTodosPlaylistDeUsuarioAsync(Guid userId)
        {
            try
            {
                var tipoUsuario = Request.Headers["X-Usuario-Tipo"].ToString();
                if (string.IsNullOrWhiteSpace(tipoUsuario)) tipoUsuario = Request.Headers["Tipo"].ToString();
                var tipoNormalizado = tipoUsuario.Trim().ToLower();
                if (tipoNormalizado != "admin" && tipoNormalizado != "administrador")
                {
                    var requestUserId = Request.Headers["X-Usuario-Id"].ToString();
                    if (string.IsNullOrWhiteSpace(requestUserId) || !Guid.TryParse(requestUserId, out var reqId) || reqId != userId)
                    {
                        return StatusCode(403);
                    }
                }

                var playlistList = (await _gestionarPlaylistService.ObtenerTodosPlaylistDeUsuario(userId)).Select(item => item.AsDto());
                return StatusCode(200, playlistList);
            }
            catch (Exception err)
            {
                return StatusCode(500, err.Message);
            }
        }

        [HttpPut]
        [RoleAuthorize("admin", "administrador", "usuario", "user")]
        public async Task<ActionResult> EditarPlaylistAsync(Guid id, EditarAgrupacionDto editarPlaylistDto)
        {
            try
            {
                Playlist playlist = await _gestionarPlaylistService.ObtenerPlaylist(id);
                if (playlist == null)
                {
                    return NotFound();
                }
                Playlist playlistEditada = playlist with
                {
                    Nombre = editarPlaylistDto.Nombre,
                    Descripcion = editarPlaylistDto.Descripcion,
                    PortadaUrl = editarPlaylistDto.PortadaUrl
                };
                await _gestionarPlaylistService.EditarPlaylist(playlistEditada);
                return StatusCode(200);
            }
            catch (Exception err)
            {
                return StatusCode(500, err.Message);
            }
        }

        [HttpDelete("{id}")]
        [RoleAuthorize("admin", "administrador", "usuario", "user")]
        public async Task<ActionResult> EliminarPlaylistAsync(Guid id)
        {
            try
            {
                var grupo = await _gestionarPlaylistService.ObtenerPlaylist(id);
                if (grupo == null)
                {
                    return NotFound();
                }
                await _gestionarPlaylistService.EliminarPlaylist(id);
                return StatusCode(200);
            }
            catch (Exception err)
            {
                return StatusCode(500, err.Message);
            }
        }

        [HttpPost("/agregar")]
        public async Task<ActionResult> AgregarPistaAPlaylistAsync(PlaylistPista playlistPista)
        {
            try
            {
                var playlist = await _gestionarPlaylistService.ObtenerPlaylist(playlistPista.PlaylistId);
                if (playlist == null)
                {
                    return NotFound(playlistPista.PlaylistId);
                }
                await _gestionarPlaylistService.AgregarPistaAPlaylist(playlistPista);
                return StatusCode(200);
            }
            catch (Exception err)
            {
                return StatusCode(500, err.Message);
            }
        }

        [HttpDelete("/quitar")]
        public async Task<ActionResult> QuitarPistaDePlaylistAsync(PlaylistPista playlistPista)
        {
            try
            {
                var grupo = await _gestionarPlaylistService.ObtenerPlaylist(playlistPista.PlaylistId);
                if (grupo == null)
                {
                    return NotFound(playlistPista.PlaylistId);
                }
                await _gestionarPlaylistService.QuitarPistaDePlaylist(playlistPista);
                return StatusCode(200);
            }
            catch (Exception err)
            {
                return StatusCode(500, err.Message);
            }
        }
    }
}

