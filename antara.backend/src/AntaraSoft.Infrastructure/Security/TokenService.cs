using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AntaraSoft.Infrastructure.Security
{
    /// <summary>
    /// Token propio firmado con HMAC-SHA256. Formato: base64url(payload).base64url(firma).
    /// </summary>
    public class TokenService : ITokenService
    {
        private const string SecretoPorDefecto = "antara-secreto-de-desarrollo-cambiar-en-produccion";
        private readonly AppSettings _settings;

        public TokenService(IOptions<AppSettings> options)
        {
            _settings = options?.Value ?? new AppSettings();
        }

        public TokenService(AppSettings settings)
        {
            _settings = settings ?? new AppSettings();
        }

        private string Secreto =>
            string.IsNullOrWhiteSpace(_settings.TokenSecret) ? SecretoPorDefecto : _settings.TokenSecret;

        private int HorasVigencia =>
            _settings.TokenExpiracionHoras > 0 ? _settings.TokenExpiracionHoras : 8;

        public string GenerarToken(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            var payload = new TokenPayload
            {
                sub = usuario.Id.ToString(),
                email = usuario.Email,
                nombre = usuario.Nombre,
                tipo = usuario.Tipo,
                exp = DateTimeOffset.UtcNow.AddHours(HorasVigencia).ToUnixTimeSeconds()
            };

            var payloadJson = JsonSerializer.Serialize(payload);
            var payloadCodificado = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
            var firma = Firmar(payloadCodificado);

            return $"{payloadCodificado}.{firma}";
        }

        public TokenValidationResult ValidarToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Invalido("No se proporciono un token.");

            var partes = token.Trim().Split('.');
            if (partes.Length != 2)
                return Invalido("El token no tiene un formato valido.");

            string firmaEsperada;
            try
            {
                firmaEsperada = Firmar(partes[0]);
            }
            catch
            {
                return Invalido("El token no tiene un formato valido.");
            }

            var firmaRecibida = partes[1];
            if (!FirmaValida(firmaEsperada, firmaRecibida))
                return Invalido("La firma del token no es valida.");

            TokenPayload payload;
            try
            {
                var json = Encoding.UTF8.GetString(Base64UrlDecode(partes[0]));
                payload = JsonSerializer.Deserialize<TokenPayload>(json);
            }
            catch
            {
                return Invalido("No se pudo leer el contenido del token.");
            }

            if (payload == null || string.IsNullOrWhiteSpace(payload.sub))
                return Invalido("El token no contiene un usuario.");

            var expira = DateTimeOffset.FromUnixTimeSeconds(payload.exp).UtcDateTime;
            if (expira <= DateTime.UtcNow)
                return Invalido("El token expiro. Inicie sesion nuevamente.");

            if (!Guid.TryParse(payload.sub, out var usuarioId))
                return Invalido("El identificador del usuario en el token no es valido.");

            return new TokenValidationResult
            {
                EsValido = true,
                UsuarioId = usuarioId,
                Email = payload.email,
                Nombre = payload.nombre,
                Tipo = payload.tipo,
                ExpiraEn = expira
            };
        }

        private static TokenValidationResult Invalido(string error) => new() { EsValido = false, Error = error };

        private string Firmar(string contenido)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secreto));
            var firma = hmac.ComputeHash(Encoding.UTF8.GetBytes(contenido));
            return Base64UrlEncode(firma);
        }

        private static bool FirmaValida(string esperada, string recibida)
        {
            var a = Encoding.UTF8.GetBytes(esperada);
            var b = Encoding.UTF8.GetBytes(recibida ?? string.Empty);
            return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
        }

        private static string Base64UrlEncode(byte[] data) =>
            Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static byte[] Base64UrlDecode(string data)
        {
            var normalizado = data.Replace('-', '+').Replace('_', '/');
            switch (normalizado.Length % 4)
            {
                case 2: normalizado += "=="; break;
                case 3: normalizado += "="; break;
            }
            return Convert.FromBase64String(normalizado);
        }

        private class TokenPayload
        {
            public string sub { get; set; }
            public string email { get; set; }
            public string nombre { get; set; }
            public string tipo { get; set; }
            public long exp { get; set; }
        }
    }
}

