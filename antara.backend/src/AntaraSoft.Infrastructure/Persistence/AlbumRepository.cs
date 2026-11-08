using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Persistence
{
    public class AlbumRepository : IAlbumRepository
    {
        private readonly IDapper _dapper;

        public AlbumRepository(IDapper dapper)
        {
            _dapper = dapper;
        }

        public async Task CrearAlbum(Album album)
        {
            try
            {
                await _dapper.QueryWithReturn<Album>("CrearAlbum", new
                {
                    album.Id,
                    album.Nombre,
                    album.Descripcion,
                    album.EstaPublicado,
                    album.FechaPublicacion,
                    album.PortadaUrl,
                    album.UsuarioId,
                    album.EstaActivo
                });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task EliminarAlbum(Guid id)
        {
            try
            {
                await _dapper.QueryWithReturn<dynamic>("EliminarAlbum", new { Id = id });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task EditarAlbum(Album album)
        {
            try
            {
                await _dapper.QueryWithReturn<Album>("EditarAlbum", new
                {
                    album.Id,
                    album.Nombre,
                    album.Descripcion,
                    album.PortadaUrl,
                    album.EstaPublicado,
                    album.EstaActivo,
                    album.Artista
                });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<Album> ObtenerAlbum(Guid id)
        {
            try
            {
                return await _dapper.QueryWithReturn<Album>("ObtenerAlbum", new { Id = id });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<List<Album>> ObtenerTodosAlbumes()
        {
            try
            {
                return await _dapper.Consulta<Album>("ObtenerTodosAlbumes", null);
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<List<Album>> ObtenerTodosAlbumesDeUsuario(Guid userId)
        {
            try
            {
                return await _dapper.Consulta<Album>("ObtenerTodosAlbumesDeUsuarios", new { UsuarioId = userId });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<bool> PublicarAlbum(Album album)
        {
            try
            {
                int resultado = await _dapper.QueryWithReturn<int>("PublicarAlbum", new
                {
                    album.Id,
                    album.EstaPublicado
                });
                return resultado == 0;
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }
    }
}

