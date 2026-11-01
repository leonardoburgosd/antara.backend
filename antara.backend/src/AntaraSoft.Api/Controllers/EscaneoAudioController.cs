using AntaraSoft.Api.Filters;
using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AntaraSoft.Api.Controllers
{
    [Route("api/escaneo-audio")]
    [ApiController]
    [TokenAuthorize("admin", "administrador")]
    public class EscaneoAudioController : Controller
    {
        private readonly IEscaneoAudioService _escaneoAudioService;

        public EscaneoAudioController(IEscaneoAudioService escaneoAudioService)
        {
            _escaneoAudioService = escaneoAudioService;
        }

        [HttpPost]
        public ActionResult<EscaneoAudioDto> Iniciar()
        {
            try
            {
                var usuarioId = HttpContext.Items["UsuarioId"] is Guid id ? id : Guid.Empty;
                var escaneo = _escaneoAudioService.Iniciar(usuarioId);
                return Accepted(escaneo);
            }
            catch (InvalidOperationException error)
            {
                return Conflict(new { error = error.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo iniciar el escaneo de la biblioteca." });
            }
        }

        [HttpGet("actual")]
        public ActionResult<EscaneoAudioDto> ObtenerActual()
        {
            var escaneo = _escaneoAudioService.ObtenerActual();
            return escaneo == null ? NoContent() : Ok(escaneo);
        }

        [HttpGet("{id:guid}")]
        public ActionResult<EscaneoAudioDto> Obtener(Guid id)
        {
            var escaneo = _escaneoAudioService.Obtener(id);
            return escaneo == null
                ? NotFound(new { error = "El escaneo no existe o ya no está disponible." })
                : Ok(escaneo);
        }

        [HttpPost("{id:guid}/cancelar")]
        public ActionResult Cancelar(Guid id)
        {
            return _escaneoAudioService.Cancelar(id)
                ? Ok(new { mensaje = "Escaneo cancelado." })
                : NotFound(new { error = "El escaneo no está activo o ya terminó." });
        }
    }
}

