using AntaraSoft.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace AntaraSoft.Infrastructure.Storage
{
    /// <summary>
    /// Cache en memoria de las rutas de audio activas. Se refresca cuando el CRUD
    /// modifica las rutas y se carga al iniciar la API.
    /// </summary>
    public class RutaAudioCache
    {
        private IReadOnlyList<RutaAudio> _rutas = new List<RutaAudio>();

        public IReadOnlyList<RutaAudio> Rutas => _rutas;

        public void Actualizar(IEnumerable<RutaAudio> rutas)
        {
            _rutas = (rutas ?? Enumerable.Empty<RutaAudio>()).ToList();
        }
    }
}

