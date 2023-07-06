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
    [Route("api/album")]
    [ApiController]
    public class AlbumController : Controller
    {
        private readonly IGestionarAlbumService _gestionarAlbumService;
        private const string directorioId = "1eWGKxmVjUsvfH2PNpfO1N_CUT2A6SkM6";

        public AlbumController(IGestionarAlbumService gestionarAlbumService)
        {
            _gestionarAlbumService = gestionarAlbumService;
        }

        // url: "localhost:8080/api/grupo"
        [HttpPost]
        public async Task<ActionResult<AlbumDto>> CrearAlbumAsync([FromForm] CrearAgrupacionDto crearAlbumDto, [FromForm] IFormFile imagenDePortada)
        {
            try
            {
                Album albumNuevo = new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = crearAlbumDto.Nombre,
                    Descripcion = crearAlbumDto.Descripcion,
                    FechaPublicacion = DateTime.Parse("1900-01-01"),
                    EstaPublicado = false,
                    UsuarioId = crearAlbumDto.UsuarioId,
                    EstaActivo = true
                };
                if (imagenDePortada == null)
                {
                    albumNuevo.PortadaUrl = null;
                }
                else
                {
                    string url = await Extensions.SubirArchivo(imagenDePortada, directorioId);
                    albumNuevo.PortadaUrl = url.Replace("&export=download", "");
                }
                await _gestionarAlbumService.CrearAlbum(albumNuevo);
                return CreatedAtAction("ObtenerAlbum", new { id = albumNuevo.Id }, albumNuevo.AsDto());
            }
            catch (Exception err)
            {
                if (err.Message.Contains("No se pudo crear el grupo"))
                {
                    return StatusCode(409, Json(new { error = err.Message }));
                }
                else return StatusCode(500, err.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AlbumDto>> ObtenerAlbumAsync(Guid id)
        {
            try
            {
                var album = await _gestionarAlbumService.ObtenerAlbum(id);
                if (album == null)
                {
                    return NotFound();
                }
                return StatusCode(200, album.AsDto());
            }
            catch (Exception err)
            {
                return StatusCode(500, err.Message);
            }
        }

        [HttpGet("todos/{userId}")]
        public async Task<ActionResult<List<AlbumDto>>> ObtenerTodosAlbumesDeUsuarioAsync(Guid userId)
        {
            try
            {
                var albumList = (await _gestionarAlbumService.ObtenerTodosAlbumesDeUsuario(userId)).Select(item => item.AsDto());
                return StatusCode(200, albumList);
            }
            catch (Exception err)
            {
                return StatusCode(500, err.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> EditarAlbumAsync(Guid id, EditarAgrupacionDto editarAlbumDto)
        {
            try
            {
                Album album = await _gestionarAlbumService.ObtenerAlbum(id);
                if (album == null)
                {
                    return NotFound();
                }
                Album albumEditado = album with
                {
                    Nombre = editarAlbumDto.Nombre,
                    Descripcion = editarAlbumDto.Descripcion,
                    PortadaUrl = editarAlbumDto.PortadaUrl
                };
                await _gestionarAlbumService.EditarAlbum(albumEditado);
                return StatusCode(200);
            }
            catch (Exception err)
            {
                return StatusCode(500, err.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarAlbumAsync(Guid id)
        {
            try
            {
                var grupo = await _gestionarAlbumService.ObtenerAlbum(id);
                if (grupo == null)
                {
                    return NotFound();
                }
                await _gestionarAlbumService.EliminarAlbum(id);
                return StatusCode(200);
            }
            catch (Exception err)
            {
                return StatusCode(500, err.Message);
            }
        }

        [HttpPut("/publicar/{id}")]
        public async Task<ActionResult> PublicarAlbum(Guid id)
        {
            try
            {
                var grupo = await _gestionarAlbumService.ObtenerAlbum(id);
                if (grupo == null)
                {
                    return NotFound(id);
                }
                await _gestionarAlbumService.PublicarAlbum(grupo);
                return StatusCode(200);
            }
            catch (Exception err)
            {
                return StatusCode(500, err.Message);
            }
        }
    }
}
