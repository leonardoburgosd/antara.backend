using AntaraSoft.Infrastructure.Security;
using Xunit;

namespace AntaraSoft.UnitTests
{
    public class SecurityTests
    {
        [Theory]
        [InlineData("hola mundo", true)]
        [InlineData("hello world", false)]
        public void CompararHashText_DebeValidarCorrectamente(string texto, bool esperado)
        {
            var encriptador = new EncryptText();
            var textoNoEncriptado = "hola mundo";
            var textoEncriptado = encriptador.GeneratePasswordHash(textoNoEncriptado);

            var actual = encriptador.CompararHash(texto, textoEncriptado);

            Assert.Equal(esperado, actual);
        }

        [Fact]
        public void EncryptYVerify_DebenSerConsistentes()
        {
            var encriptador = new EncryptText();
            var password = "SuperSecretPassword123!";
            var hash = encriptador.Encrypt(password);

            Assert.True(encriptador.Verify(password, hash));
            Assert.False(encriptador.Verify("WrongPassword", hash));
        }
    }
}

