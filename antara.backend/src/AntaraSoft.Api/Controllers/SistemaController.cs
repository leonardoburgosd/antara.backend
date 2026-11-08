using AntaraSoft.Api.Filters;
using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using AntaraSoft.Infrastructure.Storage;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AntaraSoft.Api.Controllers
{
    /// <summary>
    /// Controlador para la gestión del sistema y bootstrap de administradores
    /// </summary>
    [Route("api/sistema")]
    [ApiController]
    [Produces("application/json")]
    public class SistemaController : ControllerBase
    {
        private readonly IRegistrarUsuarioService _registrarUsuarioService;
        private readonly AdminBootstrapState _estado;
        private readonly HostPathResolver _rutasHost;

        public SistemaController(
            IRegistrarUsuarioService registrarUsuarioService,
            AdminBootstrapState estado,
            HostPathResolver rutasHost)
        {
            _registrarUsuarioService = registrarUsuarioService;
            _estado = estado;
            _rutasHost = rutasHost;
        }

        [HttpGet("health")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult> GetHealth()
        {
            try
            {
                bool hayAdmins = await _registrarUsuarioService.TieneAdministrador();
                int usuariosTotales = await _registrarUsuarioService.ContarUsuarios();

                return Ok(new SistemaDto
                {
                    Activo = true,
                    Version = "1.0.0",
                    Timestamp = DateTime.UtcNow,
                    UsuariosTotales = usuariosTotales,
                    AdminsRegistrados = hayAdmins,
                    Estado = hayAdmins ? "Operativo" : "Esperando primera creacion de administrador"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new SistemaDto
                {
                    Activo = false,
                    Version = "1.0.0",
                    Timestamp = DateTime.UtcNow,
                    UsuariosTotales = 0,
                    AdminsRegistrados = false,
                    Estado = "Error de conexion"
                });
            }
        }

        /// <summary>
        /// Unidades del host que la API puede leer ahora mismo. Se calculan desde
        /// los montajes del contenedor, así el panel no necesita conocer letras
        /// de unidad de antemano.
        /// </summary>
        [HttpGet("unidades-host")]
        [TokenAuthorize("admin", "administrador")]
        [ProducesResponseType(typeof(UnidadesHostDto), 200)]
        [ProducesResponseType(401)]
        public ActionResult<UnidadesHostDto> GetUnidadesHost()
        {
            return Ok(new UnidadesHostDto
            {
                Raiz = _rutasHost.MountRoot,
                Unidades = _rutasHost.UnidadesMontadas().ToList()
            });
        }

        [HttpPost("crea-primero-admin")]
        [ProducesResponseType(typeof(UsuarioDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UsuarioDto>> CrearPrimeroAdminAsync([FromBody] CrearUsuarioDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new { error = "Datos de usuario requeridos." });
                }

                if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
                {
                    return BadRequest(new { error = "El correo electrónico debe ser válido." });
                }

                if (dto.Password == null || dto.Password.Length < 6)
                {
                    return BadRequest(new { error = "La contraseña debe tener al menos 6 caracteres." });
                }

                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new { error = "El nombre no puede estar vacío." });
                }

                if (await _registrarUsuarioService.TieneAdministrador())
                {
                    return BadRequest(new
                    {
                        error = "Ya existe al menos un administrador registrado. No se puede crear otro mediante este endpoint."
                    });
                }

                var usuarioNuevo = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Email = dto.Email.Trim(),
                    Password = dto.Password,
                    Nombre = dto.Nombre.Trim(),
                    FechaNacimiento = dto.FechaNacimiento,
                    Genero = dto.Genero,
                    EstaActivo = true,
                    FechaRegistro = DateTime.Now,
                    Pais = dto.Pais?.Trim(),
                    FotoPerfil = null,
                    Tipo = "admin"
                };

                await _registrarUsuarioService.CrearUsuario(usuarioNuevo);

                _estado.MarcarAdministradorCreado();

                return CreatedAtAction(nameof(ObtenerUsuario), new { id = usuarioNuevo.Id }, usuarioNuevo.AsDto());
            }
            catch (Exception err)
            {
                Console.Write(err);
                if (err.Message.Contains("ya se encuentra registrado"))
                {
                    return StatusCode(409, new { error = err.Message });
                }
                return StatusCode(500, new { error = "No se pudo crear el administrador." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDto>> ObtenerUsuario(Guid id)
        {
            var usuario = await _registrarUsuarioService.ObtenerUsuario(id);
            if (usuario == null)
                return NotFound();

            return usuario.AsDto();
        }
    }
}

