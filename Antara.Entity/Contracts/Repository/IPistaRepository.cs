using Antara.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model.Contracts
{
    public interface IPistaRepository
    {
        Task CrearPista(Pista pista);
        Task<Pista> ObtenerPista(Guid id);
        Task<List<Pista>> ObtenerTodosPistasDeAlbum(Guid albumId);
        Task<List<Pista>> ObtenerTodosPistasDePlaylist(Guid playlistId);
        Task EditarPista(Pista pista);
        Task EliminarPista(Guid id);
        Task<List<Pista>> BuscarPistass(string cadena);
        Task ReproducirPista(Guid id, int reproducciones);
    }
}
