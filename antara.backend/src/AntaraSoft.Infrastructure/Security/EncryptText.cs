using AntaraSoft.Application.Contracts.Services;
using BCryptNet = BCrypt.Net.BCrypt;

namespace AntaraSoft.Infrastructure.Security
{
    public class EncryptText : IEncryptText
    {
        public string Encrypt(string text)
        {
            return BCryptNet.HashPassword(text);
        }

        public bool Verify(string text, string hash)
        {
            return BCryptNet.Verify(text, hash);
        }

        public string GeneratePasswordHash(string texto)
        {
            return BCryptNet.HashPassword(texto);
        }

        public bool CompararHash(string textoNoEncriptado, string textoEncriptado)
        {
            return BCryptNet.Verify(textoNoEncriptado, textoEncriptado);
        }
    }
}

