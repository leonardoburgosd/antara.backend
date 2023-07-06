using Antara.Model.Contracts;
using Antara.Model.Contracts.Services;
using Antara.Model.Entities;
using Antara.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Service
{
    public class LoginService:ILoginService
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IEncryptText _encryptText;
        public LoginService(IUsuarioRepository usuarioRepo, IEncryptText encryptText)
        {
            _usuarioRepo = usuarioRepo;
            _encryptText = encryptText;
        }

        public Task<Usuario> Login(string email, string password)
        {
            try
            {
                if (email == null)
                {
                    throw new ArgumentNullException(nameof(email), "No se proporciono ningún valor");
                }
                else if(password == null)
                {
                    throw new ArgumentNullException(nameof(password), "No se proporciono ningún valor");
                }
                return LoginInner(email, password);
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        private async Task<Usuario> LoginInner(string email, string password)
        {
            try
            {
                Usuario user = await _usuarioRepo.Login(email);
                if (user != null)
                {
                    bool pass = _encryptText.CompararHash(password, user.Password);
                    if (pass)
                    {
                        return _usuarioRepo.ObtenerUsuario(user.Id).Result;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Console.Write(e);
                throw;
            }
        }
    }
}
