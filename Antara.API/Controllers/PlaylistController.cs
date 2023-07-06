using Antara.Model;
using Antara.Model.Contracts.Services;
using Antara.Model.Dtos;
using Antara.Model.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Antara.API.Controllers
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

        // url: "localhost:8080/api/grupo"
        [HttpPost]
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
        public async Task<ActionResult<PlaylistDto>> ObtenerPlaylistAsync(Guid id)
        {
            try
            {
                var playlist = await _gestionarPlaylistService.ObtenerPlaylist(id);
                if (playlist == null)
                {
                    return NotFound();
                }
                return StatusCode(200, playlist.AsDto());
            }
            catch (Exception err)
            {
                return StatusCode(500, err.Message);
            }
        }

        [HttpGet("todos/{userId}")]
        public async Task<ActionResult<List<PlaylistDto>>> ObtenerTodosPlaylistDeUsuarioAsync(Guid userId)
        {
            try
            {
                var playlistList = (await _gestionarPlaylistService.ObtenerTodosPlaylistDeUsuario(userId)).Select(item => item.AsDto());
                return StatusCode(200, playlistList);
            }
            catch (Exception err)
            {
                return StatusCode(500, err.Message);
            }
        }

        [HttpPut]
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
