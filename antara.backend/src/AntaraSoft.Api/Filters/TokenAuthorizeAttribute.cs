using AntaraSoft.Application.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace AntaraSoft.Api.Filters
{
    /// <summary>
    /// Exige un token Bearer valido (Authorization: Bearer {token}) y, opcionalmente,
    /// que el tipo de usuario este dentro de los roles permitidos.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class TokenAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _rolesPermitidos;

        public TokenAuthorizeAttribute(params string[] roles)
        {
            _rolesPermitidos = roles ?? Array.Empty<string>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var encabezado = context.HttpContext.Request.Headers["Authorization"].ToString();
            string token = null;

            if (!string.IsNullOrWhiteSpace(encabezado) &&
                encabezado.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = encabezado.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                NoAutorizado(context, "Debe enviar el token de autenticacion en el encabezado Authorization (Bearer).");
                return;
            }

            var tokenService = context.HttpContext.RequestServices.GetService<ITokenService>();
            var resultado = tokenService?.ValidarToken(token);

            if (resultado == null || !resultado.EsValido)
            {
                NoAutorizado(context, resultado?.Error ?? "El token no es valido.");
                return;
            }

            if (_rolesPermitidos.Length > 0)
            {
                var tipo = resultado.Tipo?.Trim();
                bool permitido = !string.IsNullOrWhiteSpace(tipo) &&
                    _rolesPermitidos.Any(rol => string.Equals(rol, tipo, StringComparison.OrdinalIgnoreCase));

                if (!permitido)
                {
                    context.Result = new ObjectResult(new
                    {
                        error = "No tienes permisos para realizar esta accion."
                    })
                    {
                        StatusCode = 403
                    };
                    return;
                }
            }

            context.HttpContext.Items["UsuarioId"] = resultado.UsuarioId;
            context.HttpContext.Items["UsuarioTipo"] = resultado.Tipo;
            context.HttpContext.Items["UsuarioEmail"] = resultado.Email;
            context.HttpContext.Request.Headers["X-Usuario-Id"] = resultado.UsuarioId.ToString();
            context.HttpContext.Request.Headers["X-Usuario-Tipo"] = resultado.Tipo;
        }

        private static void NoAutorizado(AuthorizationFilterContext context, string mensaje)
        {
            context.Result = new ObjectResult(new { error = mensaje })
            {
                StatusCode = 401
            };
        }
    }
}

