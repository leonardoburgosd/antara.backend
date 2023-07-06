using Antara.Model.Entities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model.Contracts
{
    public interface IUsuarioRepository
    {
        Task CrearUsuario(Usuario usuario);
        Task<Usuario> ObtenerUsuario(Guid id);
        Task<Usuario> Login(string email);
        Task<Boolean> VerificarEmailUnico(string email);
        Task EliminarFisicoUsuario(Guid id);

    }
}
