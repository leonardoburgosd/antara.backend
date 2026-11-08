using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntaraSoft.Application.Services
{
    public class GestionarPlaylistService : IGestionarPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;

        public GestionarPlaylistService(IPlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        public async Task<Playlist> ObtenerPlaylist(Guid id) => await _playlistRepository.ObtenerPlaylist(id);
        public async Task<List<Playlist>> ObtenerTodasPlaylists(Guid usuarioId) => await _playlistRepository.ObtenerTodasPlaylists(usuarioId);
        public async Task<List<Playlist>> ObtenerTodosPlaylistDeUsuario(Guid userId) => await _playlistRepository.ObtenerTodosPlaylistDeUsuario(userId);
        public async Task<List<PlaylistResumenDto>> ObtenerResumenes() => await _playlistRepository.ObtenerResumenes();

        public async Task<List<PistaBibliotecaDto>> ObtenerPistasDePlaylist(Guid playlistId)
        {
            if (playlistId == Guid.Empty)
                throw new ArgumentNullException(nameof(playlistId), "No se proporciono ningún valor");
            return await _playlistRepository.ObtenerPistasDePlaylist(playlistId);
        }

        public async Task CrearPlaylist(Playlist playlist) => await _playlistRepository.CrearPlaylist(playlist);
        public async Task EliminarPlaylist(Guid id) => await _playlistRepository.EliminarPlaylist(id);
        public async Task EditarPlaylist(Playlist playlist) => await _playlistRepository.EditarPlaylist(playlist);
        public async Task<bool> AgregarPistaAPlaylist(PlaylistPista playlistPista) => await _playlistRepository.AgregarPistaAPlaylist(playlistPista);
        public async Task<bool> QuitarPistaDePlaylist(PlaylistPista playlistPista) => await _playlistRepository.QuitarPistaDePlaylist(playlistPista);
    }
}
