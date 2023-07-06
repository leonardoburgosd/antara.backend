using Antara.Model.Contracts;
using Antara.Model.Entities;
using Antara.Repository.Dapper;
using System;
using System.Threading.Tasks;

namespace Antara.Repository.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IDapper _dapper;
        public UsuarioRepository(IDapper dapper)
        {
            _dapper = dapper;
        }

        public async Task<Boolean> VerificarEmailUnico(string email)
        {
            try
            {
                Usuario respuesta = await _dapper.QueryWithReturn<Usuario>("VerificarEmailUnico", new
                {
                    @Email = email
                });
                if (respuesta == null)
                {
                    return true;
                }
                return false;
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

        public Task EliminarFisicoUsuario(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(id), "No se proporciono ningún valor");
                }
                return EliminarFisicoUsuarioInner(id);
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        private async Task EliminarFisicoUsuarioInner(Guid id)
        {
            await _dapper.QueryWithReturn<Usuario>("EliminarFisicoUsuario", new
            {
                @Id = id
            });
        }

        public async Task<Usuario> ObtenerUsuario(Guid id)
        {
            try
            {
                return await _dapper.QueryWithReturn<Usuario>("ObtenerUsuario", new
                {
                    @Id = id
                });
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
                return await _dapper.QueryWithReturn<Usuario>("Login", new
                {
                    @Email = email
                });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }
    }
}
