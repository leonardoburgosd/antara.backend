using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntaraSoft.Application.Services
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
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id), "No se proporciono ningún valor");
            return _pistaRepository.EliminarPista(id);
        }

        public async Task EditarPista(Pista pista)
        {
            await _pistaRepository.EditarPista(pista);
        }

        public Task<Pista> ObtenerPista(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id), "No se proporciono ningún valor");
            return _pistaRepository.ObtenerPista(id);
        }

        public Task<List<Pista>> ObtenerTodosPistasDeAlbum(Guid albumId)
        {
            if (albumId == Guid.Empty)
                throw new ArgumentNullException(nameof(albumId), "No se proporciono ningún valor");
            return _pistaRepository.ObtenerTodosPistasDeAlbum(albumId);
        }

        public Task<List<Pista>> ObtenerTodosPistasDePlaylist(Guid playlistId)
        {
            if (playlistId == Guid.Empty)
                throw new ArgumentNullException(nameof(playlistId), "No se proporciono ningún valor");
            return _pistaRepository.ObtenerTodosPistasDePlaylist(playlistId);
        }

        public Task<List<Pista>> BuscarPistas(string cadena)
        {
            if (cadena == null)
                throw new ArgumentNullException(nameof(cadena), "No se proporciono ningún valor");
            return _pistaRepository.BuscarPistass(cadena);
        }

        public async Task ReproducirPista(Pista pista)
        {
            pista.AumentarReproduccion();
            await _pistaRepository.ReproducirPista(pista.Id, pista.Reproducciones);
        }

        public bool SonDatosValidos(Pista pista)
        {
            Album album = _albumRepository.ObtenerAlbum(pista.AlbumId).Result;
            if (album == null)
                throw new ArgumentNullException(nameof(pista));
            return pista.EsValidoAnoCreacion();
        }
    }
}

