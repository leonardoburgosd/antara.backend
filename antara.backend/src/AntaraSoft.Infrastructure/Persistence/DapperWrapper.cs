using AntaraSoft.Application;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Persistence
{
    public interface IDapper
    {
        Task<T> QueryWithReturn<T>(string storedProcedure, dynamic parameters = null);
        Task<List<T>> Consulta<T>(string procedimientoAlmacenado, dynamic parametros = null) where T : class;
    }

    public class DapperWrapper : IDapper
    {
        private readonly AppSettings _settings;

        public DapperWrapper(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<T> QueryWithReturn<T>(string storedProcedure, dynamic parameters = null)
        {
            try
            {
                using IDbConnection connection = new NpgsqlConnection(_settings.ConexionString);
                string sql;
                var paramNames = parameters == null
                    ? Array.Empty<string>()
                    : ((object)parameters).GetType().GetProperties().Select(p => p.Name.Replace("@", "")).ToArray();

                if (paramNames.Length > 0)
                {
                    var paramList = string.Join(", ", paramNames.Select(p => $"@{p}"));
                    sql = $"SELECT * FROM \"{storedProcedure}\"({paramList})";
                }
                else
                {
                    sql = $"SELECT * FROM \"{storedProcedure}\"()";
                }

                var result = await connection.QueryAsync<T>(sql, (object)parameters);
                return result.FirstOrDefault();
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<List<T>> Consulta<T>(string procedimientoAlmacenado, dynamic parametros = null) where T : class
        {
            try
            {
                using IDbConnection connection = new NpgsqlConnection(_settings.ConexionString);
                string sql;
                if (parametros == null)
                {
                    sql = $"SELECT * FROM \"{procedimientoAlmacenado}\"()";
                }
                else
                {
                    var paramNames = ((object)parametros).GetType().GetProperties().Select(p => p.Name.Replace("@", "")).ToArray();
                    var paramList = string.Join(", ", paramNames.Select(p => $"@{p}"));
                    sql = $"SELECT * FROM \"{procedimientoAlmacenado}\"({paramList})";
                }

                var result = await connection.QueryAsync<T>(sql, (object)parametros);
                return result.ToList();
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }
    }
}

