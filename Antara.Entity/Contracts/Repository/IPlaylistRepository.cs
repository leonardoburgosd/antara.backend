using Antara.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model.Contracts.Repository
{
    public interface IPlaylistRepository
    {
        Task CrearPlaylist(Playlist playlist);
        Task<Playlist> ObtenerPlaylist(Guid id);
        Task<List<Playlist>> ObtenerTodosPlaylistDeUsuario(Guid userId);
        Task EditarPlaylist(Playlist playlist);
        Task EliminarPlaylist(Guid id);
        Task<bool> AgregarPistaAPlaylist(PlaylistPista playlistPista);
        Task<bool> QuitarPistaDePlaylist(PlaylistPista playlistPista);
    }
}
