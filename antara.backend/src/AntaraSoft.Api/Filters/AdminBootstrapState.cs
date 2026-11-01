using AntaraSoft.Application.Contracts.Services;
using System.Threading.Tasks;

namespace AntaraSoft.Api.Filters
{
    /// <summary>
    /// Recuerda si el sistema ya tiene al menos un administrador registrado.
    /// Solo se memoriza el resultado positivo: mientras no exista administrador
    /// se vuelve a consultar, y en cuanto existe deja de consultarse.
    /// </summary>
    public class AdminBootstrapState
    {
        private volatile bool _hayAdministrador;

        public bool HayAdministrador => _hayAdministrador;

        public async Task<bool> HayAdministradorAsync(IRegistrarUsuarioService servicio)
        {
            if (_hayAdministrador)
            {
                return true;
            }

            bool resultado = await servicio.TieneAdministrador();
            if (resultado)
            {
                _hayAdministrador = true;
            }
            return resultado;
        }

        public void MarcarAdministradorCreado()
        {
            _hayAdministrador = true;
        }
    }
}

