using Antara.Model.Contracts.Repository;
using Antara.Model.Entities;
using Antara.Repository.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Repository.Repositories
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly IDapper _dapper;

        public PlaylistRepository(IDapper dapper)
        {
            _dapper = dapper;
        }
        public async Task<bool> AgregarPistaAPlaylist(PlaylistPista playlistPista)
        {
            try
            {
                PlaylistPista respuesta = await _dapper.QueryWithReturn<PlaylistPista>("AgregarPistaAPlaylist", new
                {
                    playlistPista.PlaylistId,
                    playlistPista.PistaId,
                    playlistPista.FechaRegistro
                });
                if (respuesta == null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task CrearPlaylist(Playlist playlist)
        {
            try
            {
                await _dapper.QueryWithReturn<Playlist>("CrearPlaylist", new
                {
                    playlist.Id,
                    playlist.Nombre,
                    playlist.Descripcion,
                    playlist.PortadaUrl,
                    playlist.UsuarioId,
                    playlist.EstaActivo
                });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task EditarPlaylist(Playlist playlist)
        {
            try
            {
                await _dapper.QueryWithReturn<Playlist>("EditarPlaylist", new
                {
                    playlist.Id,
                    playlist.Nombre,
                    playlist.Descripcion,
                    playlist.PortadaUrl
                });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task EliminarPlaylist(Guid id)
        {
            try
            {
                await _dapper.QueryWithReturn<Usuario>("EliminarPlaylist", new
                {
                    @Id = id
                });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<Playlist> ObtenerPlaylist(Guid id)
        {
            try
            {
                return await _dapper.QueryWithReturn<Playlist>("ObtenerPlaylist", new
                {
                    @Id = id,
                });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<List<Playlist>> ObtenerTodosPlaylistDeUsuario(Guid userId)
        {
            try
            {
                return await _dapper.Consulta<Playlist>("ObtenerTodosPlaylistsDeUsuarios", new
                {
                    @UsuarioId = userId,
                });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<bool> QuitarPistaDePlaylist(PlaylistPista playlistPista)
        {
            try
            {
                int resultado = await _dapper.QueryWithReturn<int>("QuitarPistaDePlaylist", new
                {
                    playlistPista.PlaylistId,
                    playlistPista.PistaId
                });
                if (resultado == 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }
    }
}
