using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntaraSoft.Application.Services
{
    public class GestionarAlbumService : IGestionarAlbumService
    {
        private readonly IAlbumRepository _albumRepository;

        public GestionarAlbumService(IAlbumRepository albumRepository)
        {
            _albumRepository = albumRepository;
        }

        public async Task<Album> ObtenerAlbum(Guid id) => await _albumRepository.ObtenerAlbum(id);
        public async Task<List<Album>> ObtenerTodosAlbumes() => await _albumRepository.ObtenerTodosAlbumes();
        public async Task<List<Album>> ObtenerTodosAlbumesDeUsuario(Guid userId) => await _albumRepository.ObtenerTodosAlbumesDeUsuario(userId);
        public async Task CrearAlbum(Album album) => await _albumRepository.CrearAlbum(album);
        public async Task EliminarAlbum(Guid id) => await _albumRepository.EliminarAlbum(id);
        public async Task EditarAlbum(Album album) => await _albumRepository.EditarAlbum(album);
        public async Task<bool> PublicarAlbum(Album album) => await _albumRepository.PublicarAlbum(album);
        public bool SonDatosValidos(Album album) => album != null && !string.IsNullOrWhiteSpace(album.Nombre);
    }
}
