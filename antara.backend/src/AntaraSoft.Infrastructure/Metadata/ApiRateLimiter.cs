using System;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Metadata
{
    /// <summary>
    /// Limita la frecuencia de las peticiones a una API externa para respetar
    /// sus limites de uso. Es seguro para usarse desde varios hilos.
    /// </summary>
    public class ApiRateLimiter
    {
        private readonly SemaphoreSlim _turno = new(1, 1);
        private readonly TimeSpan _intervalo;
        private DateTime _ultimaPeticion = DateTime.MinValue;

        public ApiRateLimiter(TimeSpan intervalo)
        {
            _intervalo = intervalo;
        }

        public async Task EsperarAsync(CancellationToken cancellationToken)
        {
            await _turno.WaitAsync(cancellationToken);
            try
            {
                var transcurrido = DateTime.UtcNow - _ultimaPeticion;
                var espera = _intervalo - transcurrido;
                if (espera > TimeSpan.Zero)
                    await Task.Delay(espera, cancellationToken);

                _ultimaPeticion = DateTime.UtcNow;
            }
            finally
            {
                _turno.Release();
            }
        }
    }
}

