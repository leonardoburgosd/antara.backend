using Antara.Model.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Antara.Model.Contracts.Services
{
    public interface IGestionarPlaylistService
    {
        Task CrearPlaylist(Playlist playlist);
        Task<Playlist> ObtenerPlaylist(Guid id);
        Task<List<Playlist>> ObtenerTodosPlaylistDeUsuario(Guid usuarioId);
        Task EditarPlaylist(Playlist playlist);
        Task EliminarPlaylist(Guid id);
        Task<bool> AgregarPistaAPlaylist(PlaylistPista playlistPista);
        Task<bool> QuitarPistaDePlaylist(PlaylistPista playlistPista);
    }
}
