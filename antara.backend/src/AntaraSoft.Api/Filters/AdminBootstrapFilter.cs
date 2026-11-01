using AntaraSoft.Application.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AntaraSoft.Api.Filters
{
    /// <summary>
    /// Mientras no exista ningún usuario administrador registrado, bloquea el acceso
    /// a todos los endpoints, salvo los de comprobación de estado y creación del
    /// primer administrador.
    /// </summary>
    public class AdminBootstrapFilter : IAsyncAuthorizationFilter
    {
        private static readonly string[] RutasDeInicio =
        {
            "/api/sistema/health",
            "/api/sistema/crea-primero-admin"
        };

        private readonly IRegistrarUsuarioService _registrarUsuarioService;
        private readonly AdminBootstrapState _estado;

        public AdminBootstrapFilter(IRegistrarUsuarioService registrarUsuarioService, AdminBootstrapState estado)
        {
            _registrarUsuarioService = registrarUsuarioService;
            _estado = estado;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            string ruta = context.HttpContext.Request.Path.Value ?? string.Empty;

            if (RutasDeInicio.Any(r => ruta.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            bool hayAdministrador = await _estado.HayAdministradorAsync(_registrarUsuarioService);

            if (!hayAdministrador)
            {
                context.Result = new ObjectResult(new
                {
                    error = "El sistema aun no tiene un administrador registrado. Cree el primero en POST /api/sistema/crea-primero-admin"
                })
                {
                    StatusCode = 503
                };
            }
        }
    }
}

