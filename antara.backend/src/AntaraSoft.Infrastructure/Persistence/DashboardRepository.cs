using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Repository;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Persistence
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppSettings _settings;

        public DashboardRepository(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<DashboardDto> ObtenerDashboard()
        {
            using var connection = new NpgsqlConnection(_settings.ConexionString);

            const string statsSql = @"
                SELECT
                    (SELECT COUNT(*)::int FROM ""Usuario"") AS ""UsuariosTotales"",
                    (SELECT COUNT(*)::int FROM ""Usuario"" WHERE ""EstaActivo"" = TRUE) AS ""UsuariosActivos"",
                    (SELECT COUNT(*)::int FROM ""Album"" WHERE ""EstaActivo"" = TRUE) AS ""AlbumesTotales"",
                    (SELECT COUNT(*)::int FROM ""Pista"" WHERE ""EstaActivo"" = TRUE) AS ""PistasTotales"",
                    (SELECT COUNT(*)::int FROM ""Playlist"" WHERE ""EstaActivo"" = TRUE) AS ""PlaylistsTotales"",
                    (SELECT COALESCE(SUM(""Reproducciones""), 0)::int FROM ""Pista"") AS ""ReproduccionesTotales"";
            ";

            var dashboard = await connection.QuerySingleAsync<DashboardDto>(statsSql);

            const string pistasRecientesSql = @"
                SELECT
                    p.""Id"",
                    p.""Nombre"",
                    p.""Interprete"",
                    p.""Reproducciones"",
                    p.""FechaRegistro"",
                    a.""Nombre"" AS ""AlbumNombre""
                FROM ""Pista"" p
                LEFT JOIN ""Album"" a ON p.""AlbumId"" = a.""Id""
                WHERE p.""EstaActivo"" = TRUE
                ORDER BY p.""FechaRegistro"" DESC
                LIMIT 6;
            ";
            dashboard.PistasRecientes = (await connection.QueryAsync<DashboardPistaDto>(pistasRecientesSql)).ToList();

            const string pistasTopSql = @"
                SELECT
                    p.""Id"",
                    p.""Nombre"",
                    p.""Interprete"",
                    p.""Reproducciones"",
                    p.""FechaRegistro"",
                    a.""Nombre"" AS ""AlbumNombre""
                FROM ""Pista"" p
                LEFT JOIN ""Album"" a ON p.""AlbumId"" = a.""Id""
                WHERE p.""EstaActivo"" = TRUE
                ORDER BY p.""Reproducciones"" DESC, p.""Nombre"" ASC
                LIMIT 5;
            ";
            dashboard.PistasMasReproducidas = (await connection.QueryAsync<DashboardPistaDto>(pistasTopSql)).ToList();

            const string usuariosSql = @"
                SELECT ""Id"", ""Nombre"", ""Email"", ""Tipo"", ""FechaRegistro""
                FROM ""Usuario""
                ORDER BY ""FechaRegistro"" DESC
                LIMIT 5;
            ";
            dashboard.UsuariosRecientes = (await connection.QueryAsync<DashboardUsuarioDto>(usuariosSql)).ToList();

            const string albumesSql = @"
                SELECT ""Id"", ""Nombre"", ""PortadaUrl"", ""EstaPublicado"", ""FechaPublicacion""
                FROM ""Album""
                WHERE ""EstaActivo"" = TRUE
                ORDER BY ""FechaPublicacion"" DESC NULLS LAST, ""Nombre"" ASC
                LIMIT 4;
            ";
            dashboard.AlbumesRecientes = (await connection.QueryAsync<DashboardAlbumDto>(albumesSql)).ToList();

            dashboard.Timestamp = DateTime.UtcNow;
            return dashboard;
        }
    }
}

