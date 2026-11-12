using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Domain.Entities;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Persistence
{
    public class BibliotecaRepository : IBibliotecaRepository
    {
        private readonly AppSettings _settings;

        private const string PistasBibliotecaSelect = @"
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
                p.""UsuarioId"",
                (p.""Letra"" IS NOT NULL AND p.""Letra"" <> '') AS ""TieneLetra"",
                p.""Letra"",
                p.""ImagenArtista"",
                p.""FuenteMetadatos"",
                p.""EstaActivo""
            FROM ""Pista"" p
            LEFT JOIN ""Album"" a ON p.""AlbumId"" = a.""Id""
            LEFT JOIN ""Genero"" g ON p.""GeneroId"" = g.""Id""";

        public BibliotecaRepository(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<HashSet<string>> ObtenerRutasRegistradas()
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);
            var rutas = await connection.QueryAsync<string>(@"
                SELECT ""Url""
                FROM ""Pista""
                WHERE ""Url"" IS NOT NULL AND ""Url"" <> '';");

            return new HashSet<string>(rutas.Where(ruta => !string.IsNullOrWhiteSpace(ruta)), StringComparer.OrdinalIgnoreCase);
        }

        public async Task<List<PistaBibliotecaDto>> ObtenerPistas()
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);

            var sql = PistasBibliotecaSelect + @"
                WHERE p.""EstaActivo"" = TRUE
                ORDER BY p.""FechaRegistro"" DESC, p.""Nombre"" ASC;";

            var pistas = await connection.QueryAsync<PistaBibliotecaDto>(sql);
            return pistas.ToList();
        }

        public async Task<List<PistaBibliotecaDto>> ObtenerPistasDeUsuario(Guid usuarioId)
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);

            var sql = PistasBibliotecaSelect + @"
                WHERE p.""EstaActivo"" = TRUE
                  AND p.""UsuarioId"" = @UsuarioId
                ORDER BY p.""FechaRegistro"" DESC, p.""Nombre"" ASC;";

            var pistas = await connection.QueryAsync<PistaBibliotecaDto>(sql, new { UsuarioId = usuarioId });
            return pistas.ToList();
        }

        public async Task<PistaBibliotecaDto> ObtenerPistaBiblioteca(Guid id)
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);

            var sql = PistasBibliotecaSelect + @"
                WHERE p.""Id"" = @Id
                LIMIT 1;";

            return await connection.QuerySingleOrDefaultAsync<PistaBibliotecaDto>(sql, new { Id = id });
        }

        public async Task ActualizarMetadatosPista(Pista pista)
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);

            const string sql = @"
                UPDATE ""Pista""
                SET ""Nombre"" = @Nombre,
                    ""Interprete"" = @Interprete,
                    ""AnoCreacion"" = @AnoCreacion,
                    ""GeneroId"" = @GeneroId,
                    ""DuracionSegundos"" = @DuracionSegundos,
                    ""ImagenArtista"" = @ImagenArtista,
                    ""FuenteMetadatos"" = @FuenteMetadatos,
                    ""IdExternoMetadatos"" = @IdExternoMetadatos,
                    ""MetadatosJson"" = @MetadatosJson,
                    ""AlbumId"" = @AlbumId,
                    ""Letra"" = @Letra
                WHERE ""Id"" = @Id;";

            await connection.ExecuteAsync(sql, new
            {
                pista.Id,
                pista.Nombre,
                pista.Interprete,
                pista.AnoCreacion,
                pista.GeneroId,
                pista.DuracionSegundos,
                pista.ImagenArtista,
                pista.FuenteMetadatos,
                pista.IdExternoMetadatos,
                pista.MetadatosJson,
                pista.AlbumId,
                pista.Letra
            });
        }

        public async Task<Guid> ObtenerOCrearAlbum(Album album, bool soloPropietario = false)
        {
            if (soloPropietario && album.UsuarioId == Guid.Empty)
                throw new ArgumentException("El album de una pista propia debe pertenecer a un usuario.", nameof(album));

            using var connection = new NpgsqlConnection(_settings.ConexionString);
            var filtroPropietario = soloPropietario ? @" AND ""UsuarioId"" = @UsuarioId" : string.Empty;

            if (!string.IsNullOrWhiteSpace(album.IdExternoMetadatos))
            {
                var existentePorId = await connection.ExecuteScalarAsync<Guid?>(@"
                    SELECT ""Id""
                    FROM ""Album""
                    WHERE ""IdExternoMetadatos"" = @IdExterno
                    " + filtroPropietario + @"
                    LIMIT 1;",
                    new { IdExterno = album.IdExternoMetadatos, album.UsuarioId });

                if (existentePorId.HasValue)
                {
                    await CompletarAlbumAsync(connection, existentePorId.Value, album, soloPropietario);
                    return existentePorId.Value;
                }
            }

            var existentePorNombre = await connection.ExecuteScalarAsync<Guid?>(@"
                SELECT ""Id""
                FROM ""Album""
                WHERE LOWER(""Nombre"") = LOWER(@Nombre)
                  AND COALESCE(LOWER(""Artista""), '') = COALESCE(LOWER(@Artista), '')
                  " + filtroPropietario + @"
                ORDER BY ""EstaActivo"" DESC
                LIMIT 1;",
                new { album.Nombre, album.Artista, album.UsuarioId });

            if (existentePorNombre.HasValue)
            {
                await CompletarAlbumAsync(connection, existentePorNombre.Value, album, soloPropietario);
                return existentePorNombre.Value;
            }

            const string insertSql = @"
                INSERT INTO ""Album""
                    (""Id"", ""Nombre"", ""Descripcion"", ""EstaPublicado"", ""FechaPublicacion"", ""PortadaUrl"",
                     ""UsuarioId"", ""EstaActivo"", ""Artista"", ""FuenteMetadatos"", ""IdExternoMetadatos"")
                VALUES
                    (@Id, @Nombre, @Descripcion, @EstaPublicado, @FechaPublicacion, @PortadaUrl,
                     @UsuarioId, @EstaActivo, @Artista, @FuenteMetadatos, @IdExternoMetadatos);";

            await connection.ExecuteAsync(insertSql, new
            {
                album.Id,
                album.Nombre,
                album.Descripcion,
                album.EstaPublicado,
                FechaPublicacion = album.FechaPublicacion == default ? (object)DBNull.Value : album.FechaPublicacion,
                album.PortadaUrl,
                album.UsuarioId,
                album.EstaActivo,
                album.Artista,
                album.FuenteMetadatos,
                // El indice unico de IdExterno es global; los albumes privados
                // se agrupan por nombre y artista para no chocar con el catalogo
                // publico que ya usa ese identificador.
                IdExternoMetadatos = soloPropietario ? null : album.IdExternoMetadatos
            });

            return album.Id;
        }

        public async Task<bool> ActualizarPortadaAlbumDeUsuario(Guid albumId, Guid usuarioId, string portadaUrl)
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);
            var filas = await connection.ExecuteAsync(@"
                UPDATE ""Album""
                SET ""PortadaUrl"" = @PortadaUrl
                WHERE ""Id"" = @Id AND ""UsuarioId"" = @UsuarioId;",
                new { Id = albumId, UsuarioId = usuarioId, PortadaUrl = portadaUrl });

            return filas > 0;
        }

        public async Task ActualizarDatosPista(Pista pista)
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);
            await connection.ExecuteAsync(@"
                UPDATE ""Pista""
                SET ""Nombre"" = @Nombre,
                    ""Interprete"" = @Interprete,
                    ""ImagenArtista"" = @ImagenArtista,
                    ""AnoCreacion"" = @AnoCreacion,
                    ""GeneroId"" = @GeneroId,
                    ""AlbumId"" = @AlbumId
                WHERE ""Id"" = @Id;",
                new
                {
                    pista.Id,
                    pista.Nombre,
                    pista.Interprete,
                    pista.ImagenArtista,
                    pista.AnoCreacion,
                    pista.GeneroId,
                    pista.AlbumId
                });
        }

        public async Task<int> ObtenerOCrearGenero(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return 0;

            var limpio = nombre.Trim();
            using var connection = new NpgsqlConnection(_settings.ConexionString);

            await connection.ExecuteAsync(@"
                INSERT INTO ""Genero"" (""Nombre"")
                VALUES (@Nombre)
                ON CONFLICT DO NOTHING;",
                new { Nombre = limpio });

            return await connection.ExecuteScalarAsync<int>(@"
                SELECT ""Id""
                FROM ""Genero""
                WHERE LOWER(""Nombre"") = LOWER(@Nombre)
                LIMIT 1;",
                new { Nombre = limpio });
        }

        public async Task CrearPista(Pista pista)
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);

            const string sql = @"
                INSERT INTO ""Pista""
                    (""Id"", ""Nombre"", ""FechaRegistro"", ""AnoCreacion"", ""Interprete"", ""Compositor"",
                     ""Productor"", ""Reproducciones"", ""GeneroId"", ""Url"", ""AlbumId"", ""EstaActivo"",
                     ""Letra"", ""ImagenArtista"", ""DuracionSegundos"", ""FuenteMetadatos"", ""IdExternoMetadatos"", ""MetadatosJson"", ""UsuarioId"")
                VALUES
                    (@Id, @Nombre, @FechaRegistro, @AnoCreacion, @Interprete, @Compositor,
                     @Productor, @Reproducciones, @GeneroId, @Url, @AlbumId, @EstaActivo,
                     @Letra, @ImagenArtista, @DuracionSegundos, @FuenteMetadatos, @IdExternoMetadatos, @MetadatosJson, @UsuarioId);";

            await connection.ExecuteAsync(sql, new
            {
                pista.Id,
                pista.Nombre,
                pista.FechaRegistro,
                pista.AnoCreacion,
                pista.Interprete,
                pista.Compositor,
                pista.Productor,
                pista.Reproducciones,
                pista.GeneroId,
                pista.Url,
                pista.AlbumId,
                pista.EstaActivo,
                pista.Letra,
                pista.ImagenArtista,
                pista.DuracionSegundos,
                pista.FuenteMetadatos,
                pista.IdExternoMetadatos,
                pista.MetadatosJson,
                pista.UsuarioId
            });
        }

        private static async Task CompletarAlbumAsync(
            NpgsqlConnection connection,
            Guid albumId,
            Album album,
            bool soloPropietario = false)
        {
            var filtroPropietario = soloPropietario ? @" AND ""UsuarioId"" = @UsuarioId" : string.Empty;

            await connection.ExecuteAsync(@"
                UPDATE ""Album""
                SET ""PortadaUrl"" = COALESCE(""PortadaUrl"", @PortadaUrl),
                    ""FechaPublicacion"" = COALESCE(""FechaPublicacion"", @FechaPublicacion),
                    ""IdExternoMetadatos"" = COALESCE(""IdExternoMetadatos"", @IdExterno),
                    ""FuenteMetadatos"" = COALESCE(""FuenteMetadatos"", @Fuente),
                    ""Artista"" = COALESCE(""Artista"", @Artista)
                WHERE ""Id"" = @Id" + filtroPropietario + @";",
                new
                {
                    Id = albumId,
                    album.PortadaUrl,
                    FechaPublicacion = album.FechaPublicacion == default ? (object)DBNull.Value : album.FechaPublicacion,
                    IdExterno = soloPropietario ? null : album.IdExternoMetadatos,
                    Fuente = album.FuenteMetadatos,
                    album.Artista,
                    album.UsuarioId
                });
        }
    }
}

