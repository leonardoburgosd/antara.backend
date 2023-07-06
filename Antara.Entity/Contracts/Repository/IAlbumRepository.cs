using Antara.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model.Contracts.Repository
{
    public interface IAlbumRepository
    {
        Task CrearAlbum(Album album);
        Task<Album> ObtenerAlbum(Guid id);
        Task<List<Album>> ObtenerTodosAlbumesDeUsuario(Guid userId);
        Task EditarAlbum(Album album);
        Task EliminarAlbum(Guid id);
        Task<bool> PublicarAlbum(Album album);
    }
}
