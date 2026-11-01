using AntaraSoft.Api.Filters;
using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AntaraSoft.Api.Controllers
{
    [Route("api/usuario")]
    [ApiController]
    public class UsuarioController : Controller
    {
        private readonly IRegistrarUsuarioService _registrarUsuarioService;
        private readonly ILoginService _loginService;
        private readonly ITokenService _tokenService;
        private const string directorioId = "1eI8a20SdXovOvzSdjofhn7Z4uTRez2t4";

        public UsuarioController(IRegistrarUsuarioService registrarUsuarioService, ILoginService loginService, ITokenService tokenService)
        {
            _registrarUsuarioService = registrarUsuarioService;
            _loginService = loginService;
            _tokenService = tokenService;
        }

        [HttpPost]
        [RoleAuthorize("admin", "administrador")]
        public async Task<ActionResult<UsuarioDto>> CrearUsuarioAsync([FromForm] CrearUsuarioDto usuarioDto, [FromForm] IFormFile fotoPerfil)
        {
            try
            {
                Usuario usuarioNuevo = new()
                {
                    Id = Guid.NewGuid(),
                    Email = usuarioDto.Email,
                    Password = usuarioDto.Password,
                    Nombre = usuarioDto.Nombre,
                    FechaNacimiento = usuarioDto.FechaNacimiento,
                    Genero = usuarioDto.Genero,
                    EstaActivo = true,
                    FechaRegistro = DateTime.Now,
                    Pais = usuarioDto.Pais,
                    Tipo = usuarioDto.Tipo
                };
                if (fotoPerfil == null)
                {
                    usuarioNuevo.FotoPerfil = null;
                }
                else
                {
                    string url = await Extensions.SubirArchivo(fotoPerfil, directorioId);
                    usuarioNuevo.FotoPerfil = url.Replace("&export=download", "");
                }
                await _registrarUsuarioService.CrearUsuario(usuarioNuevo);
                return CreatedAtAction("ObtenerUsuario", new { id = usuarioNuevo.Id }, usuarioNuevo.AsDto());
            }
            catch (Exception err)
            {
                if (err.Message.Contains("ya se encuentra registrado")
                    || err.Message.Contains("No se pudo crear el usuario"))
                {
                    return StatusCode(409, Json(new { error = err.Message }));
                }
                else return StatusCode(500, err.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDto>> ObtenerUsuarioAsync(Guid id)
        {
            try
            {
                Usuario usuario = await _registrarUsuarioService.ObtenerUsuario(id);
                if (usuario == null)
                    return NotFound();
                return StatusCode(200, usuario.AsDto());
            }
            catch (Exception err)
            {
                return StatusCode(500, err.Message);
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> LoginAsync([FromBody] LoginUsuarioDto loginDto)
        {
            try
            {
                Usuario usuario = await _loginService.Login(loginDto.Email, loginDto.Password);
                if (usuario == null)
                {
                    return NotFound();
                }
                string token = _tokenService.GenerarToken(usuario);
                return Json(new { usuario.Id, usuario.Email, usuario.Nombre, usuario.FechaNacimiento, usuario.Genero, usuario.FechaRegistro, usuario.Pais, usuario.Tipo, usuario.FotoPerfil, token });
            }
            catch (Exception err)
            {
                if (err.Message.Contains("Correo electrónico"))
                {
                    return StatusCode(401, Json(new { error = err.Message }));
                }
                throw;
            }
        }
    }
}

