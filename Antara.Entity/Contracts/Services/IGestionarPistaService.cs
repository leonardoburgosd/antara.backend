using Antara.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model.Contracts.Services
{
    public interface IGestionarPistaService
    {
        Task CrearPista(Pista pista);
        Task<Pista> ObtenerPista(Guid id);
        Task<List<Pista>> ObtenerTodosPistasDeAlbum(Guid albumId);
        Task<List<Pista>> ObtenerTodosPistasDePlaylist(Guid playlistId);
        Task EditarPista(Pista pista);
        Task EliminarPista(Guid id);
        Task<List<Pista>> BuscarPistas(string cadena);
        Task ReproducirPista(Pista pista);
        bool SonDatosValidos(Pista pista);

    }
}
