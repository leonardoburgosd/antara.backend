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
    public class GestionarPlaylistService : IGestionarPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;
        public GestionarPlaylistService(IPlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        public Task<bool> AgregarPistaAPlaylist(PlaylistPista playlistPista)
        {
            try
            {
                if (playlistPista.PlaylistId == Guid.Empty || playlistPista.PistaId == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(playlistPista), "No se proporciono ningún valor");
                }
                playlistPista.FechaRegistro = DateTime.Now;
                return AgregarPistaAPlaylistInner(playlistPista);
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }
        private async Task<bool> AgregarPistaAPlaylistInner(PlaylistPista playlistPista)
        {
            return await _playlistRepository.AgregarPistaAPlaylist(playlistPista);
        }

        public async Task CrearPlaylist(Playlist playlist)
        {
            try
            {
                await _playlistRepository.CrearPlaylist(playlist);
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public Task EliminarPlaylist(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(id), "No se proporciono ningún valor");
                }
                return EliminarPlaylistInner(id);
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        private async Task EliminarPlaylistInner(Guid id)
        {
            await _playlistRepository.EliminarPlaylist(id);
        }
        public Task<Playlist> ObtenerPlaylist(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(id), "No se proporciono ningún valor");
                }
                return ObtenerPlaylistInner(id);
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }
        private async Task<Playlist> ObtenerPlaylistInner(Guid id)
        {
            return await _playlistRepository.ObtenerPlaylist(id);
        }
        public Task<List<Playlist>> ObtenerTodosPlaylistDeUsuario(Guid usuarioId)
        {

            try
            {
                if (usuarioId == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(usuarioId), "No se proporciono ningún valor");
                }
                return ObtenerTodosPlaylistDeUsuarioInner(usuarioId);
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }
        private async Task<List<Playlist>> ObtenerTodosPlaylistDeUsuarioInner(Guid usuarioId)
        {
            return await _playlistRepository.ObtenerTodosPlaylistDeUsuario(usuarioId);
        }
        public Task EditarPlaylist(Playlist playlist)
        {
            try
            {
                return EditarPlaylistInner(playlist);
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }
        private async Task EditarPlaylistInner(Playlist playlist)
        {
            await _playlistRepository.EditarPlaylist(playlist);
        }
        public Task<bool> QuitarPistaDePlaylist(PlaylistPista playlistPista)
        {
            try
            {
                if (playlistPista.PlaylistId == Guid.Empty || playlistPista.PistaId == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(playlistPista), "No se proporciono ningún valor");
                }
                return QuitarPistaDePlaylistInner(playlistPista);
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }
        private async Task<bool> QuitarPistaDePlaylistInner(PlaylistPista playlistPista)
        {
            return await _playlistRepository.QuitarPistaDePlaylist(playlistPista);
        }
    }
}
