using Antara.Model.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Antara.Model.Contracts.Services
{
    public interface IGestionarAlbumService
    {
        Task CrearAlbum(Album album);
        Task<Album> ObtenerAlbum(Guid id);
        Task<List<Album>> ObtenerTodosAlbumesDeUsuario(Guid usuarioId);
        Task EditarAlbum(Album album);
        Task EliminarAlbum(Guid id);
        Task<bool> PublicarAlbum(Album album);
    }
}
