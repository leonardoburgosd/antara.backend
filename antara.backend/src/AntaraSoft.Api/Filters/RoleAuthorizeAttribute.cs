using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace AntaraSoft.Api.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _allowedRoles = roles ?? Array.Empty<string>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var tipoUsuario = context.HttpContext.Request.Headers["X-Usuario-Tipo"].ToString();

            if (string.IsNullOrWhiteSpace(tipoUsuario))
            {
                tipoUsuario = context.HttpContext.Request.Headers["Tipo"].ToString();
            }

            if (string.IsNullOrWhiteSpace(tipoUsuario))
            {
                tipoUsuario = context.HttpContext.Request.Query["tipoUsuario"].ToString();
            }

            if (string.IsNullOrWhiteSpace(tipoUsuario))
            {
                tipoUsuario = context.HttpContext.Request.Query["Tipo"].ToString();
            }

            if (string.IsNullOrWhiteSpace(tipoUsuario))
            {
                context.Result = new StatusCodeResult(401);
                return;
            }

            var tipoNormalizado = tipoUsuario.Trim().ToLower();

            bool isAllowed = false;
            foreach (var role in _allowedRoles)
            {
                var roleNormalizado = role.Trim().ToLower();
                if (roleNormalizado == "admin" || roleNormalizado == "administrador")
                {
                    if (tipoNormalizado == "admin" || tipoNormalizado == "administrador")
                    {
                        isAllowed = true;
                        break;
                    }
                }
                else if (tipoNormalizado == roleNormalizado)
                {
                    isAllowed = true;
                    break;
                }
            }

            if (!isAllowed)
            {
                context.Result = new StatusCodeResult(403);
                return;
            }
        }
    }
}

