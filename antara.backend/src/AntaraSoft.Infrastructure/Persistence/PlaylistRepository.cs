using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Persistence
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
                    FechaRegistro = DateTime.UtcNow
                });
                return respuesta != null;
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
                await _dapper.QueryWithReturn<dynamic>("EliminarPlaylist", new { Id = id });
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
                return await _dapper.QueryWithReturn<Playlist>("ObtenerPlaylist", new { Id = id });
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<List<Playlist>> ObtenerTodasPlaylists(Guid usuarioId)
        {
            return await ObtenerTodosPlaylistDeUsuario(usuarioId);
        }

        public async Task<List<Playlist>> ObtenerTodosPlaylistDeUsuario(Guid userId)
        {
            try
            {
                return await _dapper.Consulta<Playlist>("ObtenerTodosPlaylistsDeUsuarios", new { UsuarioId = userId });
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
                return resultado == 0;
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }
    }
}

