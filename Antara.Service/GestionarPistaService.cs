using Antara.Model.Contracts;
using Antara.Model.Contracts.Repository;
using Antara.Model.Contracts.Services;
using Antara.Model.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Antara.Service
{
    public class GestionarPistaService : IGestionarPistaService
    { 
        
        private readonly IPistaRepository _pistaRepository;
        private readonly IAlbumRepository _albumRepository;
        public GestionarPistaService(IPistaRepository pistaRepository, IAlbumRepository albumRepository)
        {
            _pistaRepository = pistaRepository;
            _albumRepository = albumRepository;
        }

        public async Task CrearPista(Pista pista)
        {
            try
            {
                await _pistaRepository.CrearPista(pista);
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }

        public Task EliminarPista(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(id), "No se proporciono ningún valor");
                }
                return EliminarPistaInner(id);
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }

        private async Task EliminarPistaInner(Guid id)
        {
            await _pistaRepository.EliminarPista(id);
        }

        public async Task EditarPista(Pista pista)
        {
            try
            {
                await _pistaRepository.EditarPista(pista);
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }

        public Task<Pista> ObtenerPista(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(id), "No se proporciono ningún valor");
                }
                return ObtenerPistaInner(id);
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }

        private async Task<Pista> ObtenerPistaInner(Guid id)
        {
            return await _pistaRepository.ObtenerPista(id);
        }

        public Task<List<Pista>> ObtenerTodosPistasDeAlbum(Guid albumId)
        {
            try
            {
                if (albumId == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(albumId), "No se proporciono ningún valor");
                }
                return ObtenerTodosPistasDeAlbumInner(albumId);
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }

        

        private async Task<List<Pista>> ObtenerTodosPistasDeAlbumInner(Guid albumId)
        {
            return await _pistaRepository.ObtenerTodosPistasDeAlbum(albumId);
        }

        public Task<List<Pista>> ObtenerTodosPistasDePlaylist(Guid playlistId)
        {
            try
            {
                if (playlistId == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(playlistId), "No se proporciono ningún valor");
                }
                return ObtenerTodosPistasDePlaylistInner(playlistId);
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }
        private async Task<List<Pista>> ObtenerTodosPistasDePlaylistInner(Guid playlistId)
        {
            return await _pistaRepository.ObtenerTodosPistasDePlaylist(playlistId);
        }

        public Task<List<Pista>> BuscarPistas(string cadena)
        {
            try
            {
                if (cadena == null)
                {
                    throw new ArgumentNullException(nameof(cadena), "No se proporciono ningún valor");
                }
                return BuscarPistassInner(cadena);
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }

        private async Task<List<Pista>> BuscarPistassInner(string cadena)
        {
            return await _pistaRepository.BuscarPistass(cadena);
        }

        public async Task ReproducirPista(Pista pista)
        {
            try
            {
                pista.AumentarReproduccion();
                await _pistaRepository.ReproducirPista(pista.Id, pista.Reproducciones);
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }
        public bool SonDatosValidos(Pista pista)
        {
            try
            {
                Album album = _albumRepository.ObtenerAlbum(pista.AlbumId).Result;
                if (album == null)
                {
                    throw new ArgumentNullException(nameof(pista));
                }
                if(pista.EsValidoAnoCreacion())
                {
                    return true;
                }
                return false;
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }
    }
}
