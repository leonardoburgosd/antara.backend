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

        public async Task<Usuario> ActualizarUsuario(Usuario usuario)
        {
            if (usuario == null || usuario.Id == Guid.Empty)
                throw new ArgumentException("El usuario no es valido.");

            var actual = await _usuarioRepo.ObtenerUsuario(usuario.Id);
            if (actual == null)
                throw new KeyNotFoundException("El usuario no existe.");

            if (string.IsNullOrWhiteSpace(usuario.Email) || !usuario.Email.Contains("@"))
                throw new ArgumentException("El correo electrónico debe ser válido.");

            if (string.IsNullOrWhiteSpace(usuario.Nombre))
                throw new ArgumentException("El nombre no puede estar vacío.");

            var todos = await ObtenerTodosUsuarios();
            if (todos.Any(u => u.Id != usuario.Id &&
                    string.Equals(u.Email?.Trim(), usuario.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Este correo electrónico ya se encuentra registrado.");

            // No se puede dejar el sistema sin administrador activo.
            if (EsAdministrador(actual.Tipo) && (!EsAdministrador(usuario.Tipo) || !usuario.EstaActivo))
            {
                bool hayOtroAdmin = todos.Any(u =>
                    u.Id != usuario.Id && EsAdministrador(u.Tipo) && u.EstaActivo);

                if (!hayOtroAdmin)
                    throw new InvalidOperationException("Debe existir al menos un administrador activo.");
            }

            if (!string.IsNullOrWhiteSpace(usuario.Password))
            {
                if (usuario.Password.Length < 6)
                    throw new ArgumentException("La contraseña debe tener al menos 6 caracteres.");

                usuario = usuario with { Password = _encryptText.GeneratePasswordHash(usuario.Password) };
            }

            usuario = usuario with { Email = usuario.Email.Trim(), Nombre = usuario.Nombre.Trim() };
            await _usuarioRepo.ActualizarUsuario(usuario);
            return await _usuarioRepo.ObtenerUsuario(usuario.Id);
        }

        public async Task EliminarUsuario(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("El usuario no es valido.");

            var actual = await _usuarioRepo.ObtenerUsuario(id);
            if (actual == null)
                throw new KeyNotFoundException("El usuario no existe.");

            if (EsAdministrador(actual.Tipo))
            {
                var admins = (await ObtenerTodosUsuarios()).Count(u => EsAdministrador(u.Tipo));
                if (admins <= 1)
                    throw new InvalidOperationException("No se puede eliminar al último administrador del sistema.");
            }

            await _usuarioRepo.EliminarFisicoUsuario(id);
        }

        public async Task CambiarPassword(Guid id, string passwordActual, string passwordNueva)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("El usuario no es valido.");

            var usuario = await _usuarioRepo.ObtenerUsuario(id);
            if (usuario == null)
                throw new KeyNotFoundException("El usuario no existe.");

            if (!_encryptText.CompararHash(passwordActual ?? string.Empty, usuario.Password))
                throw new ArgumentException("La contraseña actual no es correcta.");

            if (string.IsNullOrWhiteSpace(passwordNueva) || passwordNueva.Length < 6)
                throw new ArgumentException("La nueva contraseña debe tener al menos 6 caracteres.");

            await _usuarioRepo.ActualizarUsuario(usuario with
            {
                Password = _encryptText.GeneratePasswordHash(passwordNueva)
            });
        }

        public static bool EsAdministrador(string tipo) =>
            tipo != null &&
            (string.Equals(tipo.Trim(), "admin", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(tipo.Trim(), "administrador", StringComparison.OrdinalIgnoreCase));

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
