using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AntaraSoft.Application.Services
{
    public class RegistrarUsuarioService : IRegistrarUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IEncryptText _encryptText;

        public RegistrarUsuarioService(IUsuarioRepository usuarioRepo, IEncryptText encryptText)
        {
            _usuarioRepo = usuarioRepo;
            _encryptText = encryptText;
        }

        public async Task CrearUsuario(Usuario usuario)
        {
            if (!await EsEmailValido(usuario.Email))
            {
                throw new ArgumentException("Este correo electrónico ya se encuentra registrado.");
            }

            usuario = usuario with { Password = _encryptText.GeneratePasswordHash(usuario.Password) };
            await _usuarioRepo.CrearUsuario(usuario);
        }

        public async Task<Usuario> ObtenerUsuario(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "No se proporciono ningún valor");
            }
            return await _usuarioRepo.ObtenerUsuario(id);
        }

        public async Task<List<Usuario>> ObtenerTodosUsuarios()
        {
            var resultado = await _usuarioRepo.ObtenerTodosUsuarios();
            return resultado ?? new List<Usuario>();
        }

        public async Task<bool> EsEmailValido(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email), "No se proporciono ningún valor");
            }
            return await _usuarioRepo.VerificarEmailUnico(email);
        }

        public async Task<bool> ExisteAdministrador()
        {
            return await TieneAdministrador();
        }

        public async Task<bool> TieneAdministrador()
        {
            var todos = await ObtenerTodosUsuarios();
            return todos.Any(u => u.Tipo != null && (u.Tipo.ToLower() == "admin" || u.Tipo.ToLower() == "administrador"));
        }

        public async Task<int> ContarUsuarios()
        {
            return (await ObtenerTodosUsuarios()).Count;
        }
    }
}
