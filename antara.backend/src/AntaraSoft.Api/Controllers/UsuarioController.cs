using AntaraSoft.Api.Filters;
using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Application.Services;
using AntaraSoft.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public UsuarioController(IRegistrarUsuarioService registrarUsuarioService, ILoginService loginService, ITokenService tokenService)
        {
            _registrarUsuarioService = registrarUsuarioService;
            _loginService = loginService;
            _tokenService = tokenService;
        }

        /// <summary>Usuarios registrados, para el CRUD del panel.</summary>
        [HttpGet("todos")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<List<UsuarioDto>>> ObtenerTodosAsync()
        {
            try
            {
                var usuarios = await _registrarUsuarioService.ObtenerTodosUsuarios();
                return Ok(usuarios
                    .OrderByDescending(usuario => usuario.FechaRegistro)
                    .Select(usuario => usuario.AsDto())
                    .ToList());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo obtener la lista de usuarios." });
            }
        }

        /// <summary>Datos del usuario autenticado, para la sección de configuración.</summary>
        [HttpGet("perfil")]
        [TokenAuthorize]
        public async Task<ActionResult<UsuarioDto>> ObtenerPerfilAsync()
        {
            try
            {
                var usuario = await _registrarUsuarioService.ObtenerUsuario(UsuarioActualId());
                if (usuario == null)
                    return NotFound(new { error = "El usuario no existe." });

                return Ok(usuario.AsDto());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo obtener el perfil." });
            }
        }

        /// <summary>Permite al usuario autenticado actualizar sus propios datos.</summary>
        [HttpPut("perfil")]
        [TokenAuthorize]
        public async Task<ActionResult<UsuarioDto>> ActualizarPerfilAsync([FromBody] ActualizarPerfilDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { error = "Datos del perfil requeridos." });

                var actual = await _registrarUsuarioService.ObtenerUsuario(UsuarioActualId());
                if (actual == null)
                    return NotFound(new { error = "El usuario no existe." });

                // El perfil propio no cambia rol ni estado: eso solo lo hace un administrador
                // desde el CRUD de usuarios.
                var usuario = actual with
                {
                    Email = string.IsNullOrWhiteSpace(dto.Email) ? actual.Email : dto.Email,
                    Nombre = string.IsNullOrWhiteSpace(dto.Nombre) ? actual.Nombre : dto.Nombre,
                    FechaNacimiento = dto.FechaNacimiento ?? actual.FechaNacimiento,
                    Genero = dto.Genero ?? actual.Genero,
                    Pais = dto.Pais ?? actual.Pais,
                    FotoPerfil = dto.FotoPerfil ?? actual.FotoPerfil,
                    // null = conservar la contraseña; si se enviara el hash actual,
                    // el servicio lo cifraría otra vez y la cuenta quedaría bloqueada.
                    Password = null
                };

                return Ok((await _registrarUsuarioService.ActualizarUsuario(usuario)).AsDto());
            }
            catch (ArgumentException err)
            {
                return BadRequest(new { error = err.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo actualizar el perfil." });
            }
        }

        [HttpPost("perfil/password")]
        [TokenAuthorize]
        public async Task<ActionResult> CambiarPasswordAsync([FromBody] CambiarPasswordDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { error = "Datos requeridos." });

                await _registrarUsuarioService.CambiarPassword(
                    UsuarioActualId(), dto.PasswordActual, dto.PasswordNueva);

                return Ok(new { mensaje = "Contraseña actualizada correctamente." });
            }
            catch (ArgumentException err)
            {
                return BadRequest(new { error = err.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo cambiar la contraseña." });
            }
        }

        [HttpPost]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<UsuarioDto>> CrearUsuarioAsync([FromBody] CrearUsuarioDto usuarioDto)
        {
            try
            {
                if (usuarioDto == null)
                    return BadRequest(new { error = "Datos del usuario requeridos." });

                if (string.IsNullOrWhiteSpace(usuarioDto.Email) || !usuarioDto.Email.Contains("@"))
                    return BadRequest(new { error = "El correo electrónico debe ser válido." });

                if (string.IsNullOrWhiteSpace(usuarioDto.Password) || usuarioDto.Password.Length < 6)
                    return BadRequest(new { error = "La contraseña debe tener al menos 6 caracteres." });

                if (string.IsNullOrWhiteSpace(usuarioDto.Nombre))
                    return BadRequest(new { error = "El nombre no puede estar vacío." });

                Usuario usuarioNuevo = new()
                {
                    Id = Guid.NewGuid(),
                    Email = usuarioDto.Email.Trim(),
                    Password = usuarioDto.Password,
                    Nombre = usuarioDto.Nombre.Trim(),
                    FechaNacimiento = usuarioDto.FechaNacimiento,
                    Genero = usuarioDto.Genero,
                    EstaActivo = true,
                    FechaRegistro = DateTime.Now,
                    Pais = usuarioDto.Pais?.Trim(),
                    FotoPerfil = usuarioDto.FotoPerfil?.Trim(),
                    Tipo = NormalizarTipo(usuarioDto.Tipo)
                };

                await _registrarUsuarioService.CrearUsuario(usuarioNuevo);
                var creado = await _registrarUsuarioService.ObtenerUsuario(usuarioNuevo.Id);
                // ASP.NET recorta el sufijo "Async" del nombre de la accion, por eso
                // se referencia "ObtenerUsuario" y no nameof(ObtenerUsuarioAsync).
                return CreatedAtAction("ObtenerUsuario", new { id = usuarioNuevo.Id }, creado.AsDto());
            }
            catch (ArgumentException err)
            {
                return BadRequest(new { error = err.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo crear el usuario." });
            }
        }

        [HttpPut("{id:guid}")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<UsuarioDto>> ActualizarUsuarioAsync(Guid id, [FromBody] ActualizarUsuarioDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { error = "Datos del usuario requeridos." });

                var actual = await _registrarUsuarioService.ObtenerUsuario(id);
                if (actual == null)
                    return NotFound(new { error = "El usuario no existe." });

                var usuario = actual with
                {
                    Email = dto.Email ?? actual.Email,
                    Nombre = dto.Nombre ?? actual.Nombre,
                    FechaNacimiento = dto.FechaNacimiento ?? actual.FechaNacimiento,
                    Genero = dto.Genero ?? actual.Genero,
                    Pais = dto.Pais ?? actual.Pais,
                    FotoPerfil = dto.FotoPerfil ?? actual.FotoPerfil,
                    Tipo = dto.Tipo == null ? actual.Tipo : NormalizarTipo(dto.Tipo),
                    EstaActivo = dto.EstaActivo ?? actual.EstaActivo,
                    // null = conservar la contraseña actual; si viene, el servicio la cifra.
                    Password = string.IsNullOrWhiteSpace(dto.Password) ? null : dto.Password
                };

                return Ok((await _registrarUsuarioService.ActualizarUsuario(usuario)).AsDto());
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
                return StatusCode(500, new { error = "No se pudo actualizar el usuario." });
            }
        }

        [HttpDelete("{id:guid}")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult> EliminarUsuarioAsync(Guid id)
        {
            try
            {
                if (id == UsuarioActualId())
                    return BadRequest(new { error = "No puedes eliminar tu propia cuenta." });

                await _registrarUsuarioService.EliminarUsuario(id);
                return Ok(new { mensaje = "Usuario eliminado correctamente." });
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
                return StatusCode(500, new { error = "No se pudo eliminar el usuario. Verifica que no tenga álbumes o playlists asociados." });
            }
        }

        [HttpGet("{id:guid}")]
        [TokenAuthorize("admin", "administrador")]
        public async Task<ActionResult<UsuarioDto>> ObtenerUsuarioAsync(Guid id)
        {
            try
            {
                Usuario usuario = await _registrarUsuarioService.ObtenerUsuario(id);
                if (usuario == null)
                    return NotFound();
                return Ok(usuario.AsDto());
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo obtener el usuario." });
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

        private Guid UsuarioActualId() =>
            HttpContext.Items["UsuarioId"] is Guid valor ? valor : Guid.Empty;

        private static string NormalizarTipo(string tipo)
        {
            var valor = tipo?.Trim();
            if (string.IsNullOrWhiteSpace(valor))
                return "usuario";

            return RegistrarUsuarioService.EsAdministrador(valor) ? "admin" : "usuario";
        }
    }
}
