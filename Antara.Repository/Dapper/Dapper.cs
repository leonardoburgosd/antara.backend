using Antara.Model;
using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Repository.Dapper
{
    public class Dapper : IDapper
    {
        private IDbConnection connection;
        private readonly AppSettings settings;
        public Dapper(IOptions<AppSettings> options)
        {
            settings = options.Value;
        }

        public async Task<T> QueryWithReturn<T>(string storedProcedure, dynamic parameters = null)
        {
            try
            {
                using (connection = new SqlConnection(settings.ConexionString))
                {
                    var result = await connection.QueryAsync<T>(storedProcedure, param: (object)parameters, commandType: CommandType.StoredProcedure);
                    return  await Task.Run(() => Enumerable.FirstOrDefault<T>(result));
                }
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }

        public async Task<dynamic> Consulta<T>(string procedimientoAlmacenado, dynamic parametros = null) where T : class
        {
            try
            {
                using (connection = new SqlConnection(settings.ConexionString))
                {
                    return await connection.QueryAsync<T>(procedimientoAlmacenado, param: (object)parametros, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception err)
            {
                Console.Write(err);
                throw;
            }
        }
    }
}
