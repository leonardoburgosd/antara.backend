using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace AntaraSoft.Application.Services
{
    public class SistemaService : ISistemaService
    {
        private readonly IRegistrarUsuarioService _registrarUsuarioService;

        public SistemaService(IRegistrarUsuarioService registrarUsuarioService)
        {
            _registrarUsuarioService = registrarUsuarioService;
        }

        public Task<bool> TieneAdministrador() => _registrarUsuarioService.TieneAdministrador();
        public Task<int> ContarUsuarios() => _registrarUsuarioService.ContarUsuarios();
        public Task<Usuario> ObtenerUsuario(Guid id) => _registrarUsuarioService.ObtenerUsuario(id);
        public Task CrearUsuario(Usuario usuario) => _registrarUsuarioService.CrearUsuario(usuario);
    }
}

