using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace AntaraSoft.Application.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IEncryptText _encryptText;

        public LoginService(IUsuarioRepository usuarioRepo, IEncryptText encryptText)
        {
            _usuarioRepo = usuarioRepo;
            _encryptText = encryptText;
        }

        public async Task<Usuario> Login(string email, string password)
        {
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email), "No se proporciono ningún valor");
            }
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password), "No se proporciono ningún valor");
            }

            Usuario user = await _usuarioRepo.Login(email);
            if (user != null)
            {
                bool pass = _encryptText.CompararHash(password, user.Password);
                if (pass)
                {
                    return await _usuarioRepo.ObtenerUsuario(user.Id);
                }
            }
            return null;
        }
    }
}
