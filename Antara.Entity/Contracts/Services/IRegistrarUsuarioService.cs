using Antara.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model.Contracts
{
    public interface IRegistrarUsuarioService
    {
        Task CrearUsuario(Usuario usuario);
        Task<Usuario> ObtenerUsuario(Guid id);
    }
}
