using Antara.Model.Contracts;
using Antara.Model.Entities;
using Antara.Repository.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Repository.Repositories
{
    public class PistaRepository : IPistaRepository
    {
        private readonly IDapper _dapper;
        public PistaRepository(IDapper dapper)
        {
            _dapper = dapper;
        }

        public async Task CrearPista(Pista pista)
        {
            try
            {
                await _dapper.QueryWithReturn<Pista>("CrearPista", new
                {
                    pista.Id,
                    pista.Url,
                    pista.Nombre,
                    pista.FechaRegistro,
                    pista.AnoCreacion,
                    pista.Interprete,
                    pista.Compositor,
                    pista.Productor,
                    pista.Reproducciones,
                    pista.GeneroId,
                    pista.AlbumId,
                    pista.EstaActivo
                });
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }

        public async Task EliminarPista(Guid id)
        {
            try
            {
                await _dapper.QueryWithReturn<dynamic>("EliminarPista", new
                {
                    @Id = id
                });
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }


        public async Task EditarPista(Pista pista)
        {
            try
            {
                await _dapper.QueryWithReturn<dynamic>("EditarPista", new
                {
                    pista.Id,
                    pista.Url,
                    pista.Nombre,
                    pista.AnoCreacion,
                    pista.Interprete,
                    pista.Compositor,
                    pista.Productor,
                    pista.GeneroId
                });
            }
 
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }

        public async Task<Pista> ObtenerPista(Guid id)
        {
            try
            {
                return await _dapper.QueryWithReturn<Pista>("ObtenerPista", new
                {
                    @Id = id
                });
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }

        public async Task<List<Pista>> ObtenerTodosPistasDeAlbum(Guid albumId)
        {
            try
            {
                var pistasList = await _dapper.Consulta<Pista>("ObtenerTodosPistasDeAlbum", new
                {
                    @AlbumId = albumId
                });
                return pistasList;
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }

        public async Task<List<Pista>> ObtenerTodosPistasDePlaylist(Guid playlistId)
        {
            try
            {
                var pistasList = await _dapper.Consulta<Pista>("ObtenerTodosPistasDePlaylist", new
                {
                    @PlaylistId = playlistId
                });
                return pistasList;
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }

        public async Task<List<Pista>> BuscarPistass(string cadena)
        {
            try
            {
                var pistasList = await _dapper.Consulta<Pista>("BuscarPistas", new
                {
                    @Cadena = cadena
                });
                return pistasList;
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }

        public async Task ReproducirPista(Guid id, int reproducciones)
        {
            try
            {
                await _dapper.QueryWithReturn<Pista>("ReproducirPista", new
                {
                    @Id = id,
                    @Reproducciones = reproducciones
                });
            }
            catch (Exception err)
            {
                Console.Write(err.Message);
                throw;
            }
        }
    }
}
