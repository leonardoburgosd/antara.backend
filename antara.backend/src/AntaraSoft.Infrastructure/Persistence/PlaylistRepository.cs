using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Domain.Entities;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Persistence
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly IDapper _dapper;
        private readonly AppSettings _settings;

        // Una playlist puede mezclar canciones de distintos artistas: cada pista
        // conserva su propio intérprete y álbum, y el resumen cuenta los artistas
        // distintos que reúne.
        private const string ResumenSelect = @"
            SELECT
                pl.""Id"",
                pl.""Nombre"",
                pl.""Descripcion"",
                pl.""PortadaUrl"",
                pl.""UsuarioId"",
                COALESCE(u.""Nombre"", '') AS ""UsuarioNombre"",
                pl.""EstaActivo"",
                COUNT(DISTINCT pp.""PistaId"")::int AS ""TotalPistas"",
                COUNT(DISTINCT NULLIF(LOWER(TRIM(COALESCE(NULLIF(p.""Interprete"", ''), a.""Artista""))), ''))::int AS ""TotalArtistas"",
                COALESCE(STRING_AGG(DISTINCT NULLIF(TRIM(COALESCE(NULLIF(p.""Interprete"", ''), a.""Artista"")), ''), ', '), '') AS ""Artistas""
            FROM ""Playlist"" pl
            LEFT JOIN ""Usuario"" u ON u.""Id"" = pl.""UsuarioId""
            LEFT JOIN ""PlaylistPista"" pp ON pp.""PlaylistId"" = pl.""Id""
            LEFT JOIN ""Pista"" p ON p.""Id"" = pp.""PistaId""
            LEFT JOIN ""Album"" a ON a.""Id"" = p.""AlbumId""
            GROUP BY pl.""Id"", pl.""Nombre"", pl.""Descripcion"", pl.""PortadaUrl"", pl.""UsuarioId"", u.""Nombre"", pl.""EstaActivo""
            ORDER BY LOWER(pl.""Nombre"") ASC;";

        private const string PistasDePlaylistSelect = @"
            SELECT
                p.""Id"",
                p.""Nombre"",
                p.""Interprete"",
                p.""AlbumId"",
                a.""Nombre"" AS ""AlbumNombre"",
                a.""PortadaUrl"",
                p.""AnoCreacion"",
                p.""GeneroId"",
                g.""Nombre"" AS ""GeneroNombre"",
                p.""DuracionSegundos"",
                p.""Reproducciones"",
                p.""FechaRegistro"",
                p.""Url"",
                (p.""Letra"" IS NOT NULL AND p.""Letra"" <> '') AS ""TieneLetra"",
                p.""Letra"",
                p.""ImagenArtista"",
                p.""FuenteMetadatos"",
                p.""EstaActivo""
            FROM ""PlaylistPista"" pp
            INNER JOIN ""Pista"" p ON p.""Id"" = pp.""PistaId""
            LEFT JOIN ""Album"" a ON a.""Id"" = p.""AlbumId""
            LEFT JOIN ""Genero"" g ON g.""Id"" = p.""GeneroId""
            WHERE pp.""PlaylistId"" = @PlaylistId
            ORDER BY pp.""FechaRegistro"" ASC, LOWER(p.""Nombre"") ASC;";

        public PlaylistRepository(IDapper dapper, IOptions<AppSettings> options)
        {
            _dapper = dapper;
            _settings = options.Value;
        }

        public async Task<bool> AgregarPistaAPlaylist(PlaylistPista playlistPista)
        {
            try
            {
                // La función ignora duplicados (ON CONFLICT), así que repetir el
                // alta de una canción deja la playlist igual.
                PlaylistPista respuesta = await _dapper.QueryWithReturn<PlaylistPista>("AgregarPistaAPlaylist", new
                {
                    playlistPista.PlaylistId,
                    playlistPista.PistaId
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

        public async Task<List<PlaylistResumenDto>> ObtenerResumenes()
        {
            try
            {
                using var connection = new NpgsqlConnection(_settings.ConexionString);
                var resumenes = await connection.QueryAsync<PlaylistResumenDto>(ResumenSelect);
                return new List<PlaylistResumenDto>(resumenes);
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<List<PistaBibliotecaDto>> ObtenerPistasDePlaylist(Guid playlistId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_settings.ConexionString);
                var pistas = await connection.QueryAsync<PistaBibliotecaDto>(
                    PistasDePlaylistSelect,
                    new { PlaylistId = playlistId });
                return new List<PistaBibliotecaDto>(pistas);
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
                return resultado > 0;
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }
    }
}
