using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Repository.Dapper
{
    public interface IDapper
    {
        Task<T> QueryWithReturn<T>(string storedProcedure, dynamic parameters = null);
        Task<dynamic> Consulta<T>(string procedimientoAlmacenado, dynamic parametros = null) where T : class;
    }
}
