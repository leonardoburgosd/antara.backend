using AntaraSoft.Api.Filters;
using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AntaraSoft.Api.Controllers
{
    [Route("api/rutas-audio")]
    [ApiController]
    [TokenAuthorize("admin", "administrador")]
    public class RutaAudioController : Controller
    {
        private readonly IRutaAudioService _rutaAudioService;

        public RutaAudioController(IRutaAudioService rutaAudioService)
        {
            _rutaAudioService = rutaAudioService;
        }

        [HttpGet]
        public async Task<ActionResult<List<RutaAudioDto>>> ObtenerTodasAsync()
        {
            try
            {
                var rutas = (await _rutaAudioService.ObtenerTodas()).Select(ruta => ruta.AsDto()).ToList();
                return Ok(rutas);
            }
            catch (Exception err)
            {
                return RespuestaError(err);
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RutaAudioDto>> ObtenerPorIdAsync(Guid id)
        {
            try
            {
                return Ok((await _rutaAudioService.ObtenerPorId(id)).AsDto());
            }
            catch (Exception err)
            {
                return RespuestaError(err);
            }
        }

        [HttpPost]
        public async Task<ActionResult<RutaAudioDto>> CrearAsync([FromBody] CrearRutaAudioDto dto)
        {
            try
            {
                var ruta = await _rutaAudioService.Crear(dto);
                return Created($"/api/rutas-audio/{ruta.Id}", ruta.AsDto());
            }
            catch (Exception err)
            {
                return RespuestaError(err);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<RutaAudioDto>> ActualizarAsync(Guid id, [FromBody] ActualizarRutaAudioDto dto)
        {
            try
            {
                return Ok((await _rutaAudioService.Actualizar(id, dto)).AsDto());
            }
            catch (Exception err)
            {
                return RespuestaError(err);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> EliminarAsync(Guid id)
        {
            try
            {
                await _rutaAudioService.Eliminar(id);
                return Ok(new { mensaje = "Ruta de audio eliminada." });
            }
            catch (Exception err)
            {
                return RespuestaError(err);
            }
        }

        [HttpPost("{id:guid}/probar")]
        public async Task<ActionResult<PruebaRutaAudioDto>> ProbarAsync(Guid id)
        {
            try
            {
                return Ok(await _rutaAudioService.Probar(id));
            }
            catch (Exception err)
            {
                return RespuestaError(err);
            }
        }

        private ActionResult RespuestaError(Exception err)
        {
            return err switch
            {
                KeyNotFoundException => NotFound(new { error = err.Message }),
                InvalidOperationException => Conflict(new { error = err.Message }),
                ArgumentException => BadRequest(new { error = err.Message }),
                _ => StatusCode(500, new { error = "No se pudo completar la operacion con las rutas de audio." })
            };
        }
    }
}

