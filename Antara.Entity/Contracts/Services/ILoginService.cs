using Antara.Model.Entities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model.Contracts.Services
{
    public interface ILoginService
    {
        Task<Usuario> Login(string email, string password);
    }
}
