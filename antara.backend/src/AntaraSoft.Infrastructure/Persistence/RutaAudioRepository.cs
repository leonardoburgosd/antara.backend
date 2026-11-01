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
    public class RutaAudioRepository : IRutaAudioRepository
    {
        private readonly AppSettings _settings;

        public RutaAudioRepository(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        private const string Columnas = @"""Id"", ""Nombre"", ""RutaFisica"", ""RutaContenedor"", ""Descripcion"", ""EstaActiva"", ""EsPredeterminada"", ""FechaRegistro"", ""FechaActualizacion""";

        public async Task<List<RutaAudio>> ObtenerTodas()
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);
            var sql = $@"
                SELECT {Columnas}
                FROM ""RutaAudio""
                ORDER BY ""EsPredeterminada"" DESC, ""EstaActiva"" DESC, ""Nombre"" ASC;";
            return (await connection.QueryAsync<RutaAudio>(sql)).ToList();
        }

        public async Task<RutaAudio> ObtenerPorId(Guid id)
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);
            var sql = $@"
                SELECT {Columnas}
                FROM ""RutaAudio""
                WHERE ""Id"" = @Id
                LIMIT 1;";
            return await connection.QueryFirstOrDefaultAsync<RutaAudio>(sql, new { Id = id });
        }

        public async Task<bool> ExisteNombre(string nombre, Guid? excluirId = null)
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);
            var sql = @"
                SELECT COUNT(*)::int
                FROM ""RutaAudio""
                WHERE LOWER(""Nombre"") = LOWER(@Nombre)
                  AND (@ExcluirId IS NULL OR ""Id"" <> @ExcluirId);";
            var total = await connection.ExecuteScalarAsync<int>(sql, new { Nombre = nombre, ExcluirId = excluirId });
            return total > 0;
        }

        public async Task<bool> ExistePredeterminada()
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);
            var total = await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*)::int FROM ""RutaAudio"" WHERE ""EsPredeterminada"" = TRUE;");
            return total > 0;
        }

        public async Task Crear(RutaAudio ruta)
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);
            const string sql = @"
                INSERT INTO ""RutaAudio""
                    (""Id"", ""Nombre"", ""RutaFisica"", ""RutaContenedor"", ""Descripcion"", ""EstaActiva"", ""EsPredeterminada"", ""FechaRegistro"", ""FechaActualizacion"")
                VALUES
                    (@Id, @Nombre, @RutaFisica, @RutaContenedor, @Descripcion, @EstaActiva, @EsPredeterminada, @FechaRegistro, @FechaActualizacion);";
            await connection.ExecuteAsync(sql, ruta);
        }

        public async Task Actualizar(RutaAudio ruta)
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);
            const string sql = @"
                UPDATE ""RutaAudio""
                SET ""Nombre"" = @Nombre,
                    ""RutaFisica"" = @RutaFisica,
                    ""RutaContenedor"" = @RutaContenedor,
                    ""Descripcion"" = @Descripcion,
                    ""EstaActiva"" = @EstaActiva,
                    ""EsPredeterminada"" = @EsPredeterminada,
                    ""FechaActualizacion"" = @FechaActualizacion
                WHERE ""Id"" = @Id;";
            await connection.ExecuteAsync(sql, ruta);
        }

        public async Task Eliminar(Guid id)
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);
            await connection.ExecuteAsync(@"DELETE FROM ""RutaAudio"" WHERE ""Id"" = @Id;", new { Id = id });
        }

        public async Task QuitarPredeterminada(Guid? excluirId = null)
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);
            const string sql = @"
                UPDATE ""RutaAudio""
                SET ""EsPredeterminada"" = FALSE
                WHERE ""EsPredeterminada"" = TRUE
                  AND (@ExcluirId IS NULL OR ""Id"" <> @ExcluirId);";
            await connection.ExecuteAsync(sql, new { ExcluirId = excluirId });
        }

        public async Task EstablecerPredeterminada(Guid id)
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);
            await connection.ExecuteAsync(
                @"UPDATE ""RutaAudio"" SET ""EsPredeterminada"" = TRUE WHERE ""Id"" = @Id;",
                new { Id = id });
        }
    }
}

