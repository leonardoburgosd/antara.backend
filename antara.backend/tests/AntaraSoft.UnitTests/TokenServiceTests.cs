using AntaraSoft.Application;
using AntaraSoft.Domain.Entities;
using AntaraSoft.Infrastructure.Security;
using Microsoft.Extensions.Options;
using System;
using Xunit;

namespace AntaraSoft.UnitTests
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            var settings = new AppSettings
            {
                TokenSecret = "test-secret-key-that-is-long-enough-for-sha256",
                TokenExpiracionHoras = 2
            };
            _tokenService = new TokenService(Options.Create(settings));
        }

        [Fact]
        public void GenerarToken_DebeGenerarTokenValido()
        {
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "admin@antara.local",
                Nombre = "Admin",
                Tipo = "admin"
            };

            var token = _tokenService.GenerarToken(usuario);

            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.Contains(".", token);

            var validacion = _tokenService.ValidarToken(token);
            Assert.True(validacion.EsValido);
            Assert.Equal(usuario.Id, validacion.UsuarioId);
            Assert.Equal(usuario.Email, validacion.Email);
            Assert.Equal("admin", validacion.Tipo);
        }

        [Fact]
        public void ValidarToken_ConTokenInvalido_DebeRetornarInvalido()
        {
            var validacion = _tokenService.ValidarToken("token.invalido");
            Assert.False(validacion.EsValido);
        }
    }
}

