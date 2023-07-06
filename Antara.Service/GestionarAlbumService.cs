using Antara.Model.Contracts.Repository;
using Antara.Model.Contracts.Services;
using Antara.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Service
{
    public class GestionarAlbumService : IGestionarAlbumService
    {
        private readonly IAlbumRepository _albumRepository;


        public GestionarAlbumService(IAlbumRepository albumRepository)
        {
            _albumRepository = albumRepository;
        }


        public async Task CrearAlbum(Album album)
        {
            try
            {
                await _albumRepository.CrearAlbum(album);
            }
            catch (Exception err)
            {
                Console.Write(err.Message.ToString());
                throw;
            }
        }

        public Task EliminarAlbum(Guid id)
        {
            try
            {
                if(id == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(id), "No se proporciono ningún valor");
                }
                return EliminarAlbumInner(id);
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        private async Task EliminarAlbumInner(Guid id)
        {
            await _albumRepository.EliminarAlbum(id);
        }

        
        public Task<Album> ObtenerAlbum(Guid id)
        {
            try
            {
                if(id == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(id), "No se proporciono ningún valor");
                }
                return ObtenerAlbumInner(id);
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }
        private async Task<Album> ObtenerAlbumInner(Guid id)
        {
            return await _albumRepository.ObtenerAlbum(id);
        }

        public Task<List<Album>> ObtenerTodosAlbumesDeUsuario(Guid usuarioId)
        {

            try
            {
                if (usuarioId == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(usuarioId), "No se proporciono ningún valor");
                }
                return ObtenerTodosAlbumesDeUsuarioInner(usuarioId);
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        private async Task<List<Album>> ObtenerTodosAlbumesDeUsuarioInner(Guid usuarioId)
        {
            return await _albumRepository.ObtenerTodosAlbumesDeUsuario(usuarioId);
        }

        
        public async Task EditarAlbum(Album album)
        {
            try
            {
                await _albumRepository.EditarAlbum(album);
            }
            catch (Exception err)
            {
                Console.Write(err.Message.ToString());
                throw;
            }
        }

        public async Task<bool> PublicarAlbum(Album album)
        {
            try
            {
                if(!album.EstaPublicado)
                {
                    album.EstaPublicado = true;
                    album.FechaPublicacion = DateTime.Now;
                    return await _albumRepository.PublicarAlbum(album);
                }
                throw new ArgumentException("La agrupación ya se encuentra publicada");
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }
    }
}
