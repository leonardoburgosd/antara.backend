using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Persistence
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IDapper _dapper;

        public UsuarioRepository(IDapper dapper)
        {
            _dapper = dapper;
        }

        public async Task<bool> VerificarEmailUnico(string email)
        {
            try
            {
                Usuario respuesta = await _dapper.QueryWithReturn<Usuario>("VerificarEmailUnico", new
                {
                    Email = email
                });
                return respuesta == null;
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task CrearUsuario(Usuario usuario)
        {
            try
            {
                await _dapper.QueryWithReturn<Usuario>("CrearUsuario", new
                {
                    usuario.Id,
                    usuario.Email,
                    usuario.Password,
                    usuario.Nombre,
                    usuario.FechaNacimiento,
                    usuario.Genero,
                    usuario.EstaActivo,
                    usuario.FechaRegistro,
                    usuario.Pais,
                    usuario.FotoPerfil,
                    usuario.Tipo
                });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        /// <summary>
        /// Actualiza los datos del usuario. Si <see cref="Usuario.Password"/> llega
        /// vacío, la función conserva la contraseña actual.
        /// </summary>
        public async Task ActualizarUsuario(Usuario usuario)
        {
            try
            {
                // El panel envía la fecha en ISO con zona ("...Z"); la columna y la
                // función usan TIMESTAMP sin zona, así que se normaliza el Kind.
                var fechaNacimiento = usuario.FechaNacimiento == default
                    ? usuario.FechaNacimiento
                    : DateTime.SpecifyKind(usuario.FechaNacimiento, DateTimeKind.Unspecified);

                await _dapper.QueryWithReturn<Usuario>("ActualizarUsuario", new
                {
                    usuario.Id,
                    usuario.Email,
                    usuario.Nombre,
                    FechaNacimiento = fechaNacimiento,
                    usuario.Genero,
                    usuario.EstaActivo,
                    usuario.Pais,
                    usuario.FotoPerfil,
                    usuario.Tipo,
                    Password = string.IsNullOrWhiteSpace(usuario.Password) ? null : usuario.Password
                });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task EliminarFisicoUsuario(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(id), "No se proporciono ningún valor");
                }
                await _dapper.QueryWithReturn<Usuario>("EliminarFisicoUsuario", new { Id = id });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<List<Usuario>> ObtenerTodosUsuarios()
        {
            try
            {
                var resultado = await _dapper.Consulta<Usuario>("ObtenerTodosUsuarios");
                return resultado ?? new List<Usuario>();
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<Usuario> ObtenerUsuario(Guid id)
        {
            try
            {
                return await _dapper.QueryWithReturn<Usuario>("ObtenerUsuario", new { Id = id });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<Usuario> Login(string email)
        {
            try
            {
                return await _dapper.QueryWithReturn<Usuario>("Login", new { Email = email });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<bool> ExisteAdministrador()
        {
            var usuarios = await ObtenerTodosUsuarios();
            return usuarios.Any(u => u.Tipo != null && (u.Tipo.ToLower() == "admin" || u.Tipo.ToLower() == "administrador"));
        }
    }
}

