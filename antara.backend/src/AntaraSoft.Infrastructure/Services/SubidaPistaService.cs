using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using AntaraSoft.Infrastructure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Services
{
    /// <summary>
    /// Sube canciones desde el panel a una de las rutas de audio registradas.
    /// El destino nunca se toma del cliente como ruta libre: se resuelve a partir
    /// de la ruta guardada en la base de datos (ruta física o de contenedor), y el
    /// archivo queda dentro de esa carpeta o de una subcarpeta suya.
    /// </summary>
    public class SubidaPistaService : ISubidaPistaService
    {
        private static readonly HashSet<string> ExtensionesAudio = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3", ".flac", ".wav", ".m4a", ".aac", ".ogg", ".opus", ".wma", ".aiff", ".alac"
        };

        private readonly IRutaAudioRepository _rutas;
        private readonly HostPathResolver _resolver;
        private readonly IEscaneoAudioService _escaneo;
        private readonly IDescargadorYouTube _descargador;

        public SubidaPistaService(
            IRutaAudioRepository rutas,
            HostPathResolver resolver,
            IEscaneoAudioService escaneo,
            IDescargadorYouTube descargador)
        {
            _rutas = rutas;
            _resolver = resolver;
            _escaneo = escaneo;
            _descargador = descargador;
        }

        /// <summary>Extensiones admitidas, para mostrarlas en la interfaz.</summary>
        public static string FormatosPermitidos =>
            string.Join(", ", ExtensionesAudio.OrderBy(extension => extension)
                .Select(extension => extension.TrimStart('.')));

        public async Task<List<RutaSubidaDto>> ObtenerRutasAsync()
        {
            var rutas = (await _rutas.ObtenerTodas())
                .Where(ruta => ruta.EstaActiva)
                .ToList();

            return rutas
                .Select(ruta =>
                {
                    var destino = _resolver.ResolverDirectorio(ruta.RutaFisica, ruta.RutaContenedor);
                    var existe = !string.IsNullOrWhiteSpace(destino) && Directory.Exists(destino);

                    return new RutaSubidaDto
                    {
                        Id = ruta.Id,
                        Nombre = ruta.Nombre,
                        RutaDestino = destino,
                        Disponible = existe,
                        Escribible = existe && PuedeEscribir(destino),
                        EsPredeterminada = ruta.EsPredeterminada
                    };
                })
                .OrderByDescending(ruta => ruta.EsPredeterminada)
                .ThenBy(ruta => ruta.Nombre)
                .ToList();
        }

        public async Task<SubidaPistaDto> SubirAsync(
            Guid? rutaId,
            string subcarpeta,
            string nombreArchivo,
            Stream contenido,
            Guid usuarioId,
            CancellationToken cancellationToken = default)
        {
            if (contenido == null)
                throw new ArgumentException("Adjunta el archivo de audio que quieres subir.");

            if (usuarioId == Guid.Empty)
                throw new ArgumentException("No se pudo identificar al usuario que sube el archivo.");

            var nombreOriginal = LimpiarNombre(nombreArchivo);
            var extension = Path.GetExtension(nombreOriginal);
            if (!ExtensionesAudio.Contains(extension))
                throw new ArgumentException($"Solo se permiten archivos de audio ({FormatosPermitidos}).");

            var (ruta, destino) = await ResolverDestinoAsync(rutaId, subcarpeta);
            var archivoFinal = NombreDisponible(destino, nombreOriginal);

            try
            {
                using (var salida = new FileStream(
                    archivoFinal, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    await contenido.CopyToAsync(salida, cancellationToken);
                }
            }
            catch
            {
                EliminarSiExiste(archivoFinal);
                throw;
            }

            return await ImportarYResponderAsync(ruta, destino, archivoFinal, usuarioId, cancellationToken);
        }

        public async Task<SubidaPistaDto> SubirDesdeUrlAsync(
            Guid? rutaId,
            string subcarpeta,
            string url,
            Guid usuarioId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Ingresa el enlace de YouTube de la canción.");

            if (!_descargador.EsUrlYouTube(url))
                throw new ArgumentException(
                    "Solo se admiten enlaces de YouTube (youtube.com o youtu.be) para descargar audio.");

            if (usuarioId == Guid.Empty)
                throw new ArgumentException("No se pudo identificar al usuario que sube el archivo.");

            var (ruta, destino) = await ResolverDestinoAsync(rutaId, subcarpeta);

            var descarga = await _descargador.DescargarAsync(url, destino, cancellationToken);
            if (descarga == null || string.IsNullOrWhiteSpace(descarga.Archivo) || !File.Exists(descarga.Archivo))
                throw new InvalidOperationException("La descarga de YouTube no generó ningún archivo de audio.");

            var resultado = await ImportarYResponderAsync(
                ruta, destino, descarga.Archivo, usuarioId, cancellationToken);

            resultado.Mensaje = $"Audio descargado desde YouTube. {resultado.Mensaje}";
            return resultado;
        }

        private async Task<(RutaAudio ruta, string destino)> ResolverDestinoAsync(Guid? rutaId, string subcarpeta)
        {
            var ruta = await ObtenerRutaDestino(rutaId);
            var raiz = _resolver.ResolverDirectorio(ruta.RutaFisica, ruta.RutaContenedor);

            if (string.IsNullOrWhiteSpace(raiz) || !Directory.Exists(raiz))
            {
                throw new InvalidOperationException(
                    $"La ruta '{ruta.Nombre}' no es accesible desde la API. Verifica que la carpeta exista " +
                    "y que su unidad esté montada en el contenedor.");
            }

            var destino = ResolverSubcarpeta(raiz, subcarpeta);
            Directory.CreateDirectory(destino);

            return (ruta, destino);
        }

        private async Task<SubidaPistaDto> ImportarYResponderAsync(
            RutaAudio ruta,
            string destino,
            string archivoFinal,
            Guid usuarioId,
            CancellationToken cancellationToken)
        {
            EscaneoAudioResultadoDto importado;
            try
            {
                importado = await _escaneo.ImportarArchivoAsync(archivoFinal, usuarioId, cancellationToken);
            }
            catch (Exception err)
            {
                throw new InvalidOperationException(
                    $"El archivo se guardó en '{archivoFinal}', pero no se pudo registrar la pista: {err.Message}. " +
                    "Aparecerá en la biblioteca en el próximo escaneo de la ruta.", err);
            }

            return new SubidaPistaDto
            {
                PistaId = importado.PistaId ?? Guid.Empty,
                Nombre = importado.Nombre,
                Interprete = importado.Interprete,
                Album = importado.Album,
                ConMetadatos = importado.Estado == "nuevo",
                Archivo = archivoFinal,
                RutaDestino = destino,
                RutaId = ruta.Id,
                RutaNombre = ruta.Nombre,
                Mensaje = importado.Mensaje
            };
        }

        private async Task<RutaAudio> ObtenerRutaDestino(Guid? rutaId)
        {
            if (rutaId.HasValue && rutaId.Value != Guid.Empty)
            {
                var seleccionada = await _rutas.ObtenerPorId(rutaId.Value);
                if (seleccionada == null)
                    throw new KeyNotFoundException("La ruta de audio seleccionada no existe.");

                if (!seleccionada.EstaActiva)
                    throw new InvalidOperationException($"La ruta '{seleccionada.Nombre}' está inactiva.");

                return seleccionada;
            }

            var activas = (await _rutas.ObtenerTodas()).Where(ruta => ruta.EstaActiva).ToList();
            var predeterminada = activas.FirstOrDefault(ruta => ruta.EsPredeterminada) ?? activas.FirstOrDefault();

            if (predeterminada == null)
                throw new InvalidOperationException(
                    "No hay rutas de audio activas. Registra una ruta antes de subir archivos.");

            return predeterminada;
        }

        /// <summary>
        /// Une la raíz con la subcarpeta indicada, sin permitir rutas absolutas ni
        /// segmentos "..", y garantiza que el resultado quede dentro de la raíz.
        /// </summary>
        private static string ResolverSubcarpeta(string raiz, string subcarpeta)
        {
            var limpia = (subcarpeta ?? string.Empty).Trim().Replace('\\', '/').Trim('/');
            if (string.IsNullOrWhiteSpace(limpia))
                return raiz;

            var segmentos = limpia.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segmentos.Any(segmento => segmento == "." || segmento == ".." || segmento.Contains(':')))
                throw new ArgumentException("La subcarpeta no es válida.");

            var partes = new List<string> { raiz };
            partes.AddRange(segmentos.Select(LimpiarSegmento));

            var destino = Path.GetFullPath(Path.Combine(partes.ToArray()));
            var prefijo = Path.GetFullPath(raiz).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                          + Path.DirectorySeparatorChar;

            if (!destino.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("La subcarpeta debe estar dentro de la ruta seleccionada.");

            return destino;
        }

        private static string LimpiarSegmento(string segmento)
        {
            var limpio = segmento.Trim();
            foreach (var invalido in Path.GetInvalidFileNameChars())
                limpio = limpio.Replace(invalido.ToString(), string.Empty);

            return string.IsNullOrWhiteSpace(limpio) ? "_" : limpio;
        }

        private static string LimpiarNombre(string nombre)
        {
            var soloNombre = Path.GetFileName((nombre ?? string.Empty).Replace('\\', '/'));

            foreach (var invalido in Path.GetInvalidFileNameChars())
                soloNombre = soloNombre.Replace(invalido.ToString(), string.Empty);

            soloNombre = soloNombre.Trim();

            if (string.IsNullOrWhiteSpace(soloNombre))
                throw new ArgumentException("El archivo no tiene un nombre válido.");

            return soloNombre;
        }

        /// <summary>Evita sobrescribir: agrega " (1)", " (2)"... antes de la extensión.</summary>
        private static string NombreDisponible(string directorio, string nombre)
        {
            var destino = Path.Combine(directorio, nombre);
            if (!File.Exists(destino))
                return destino;

            var baseNombre = Path.GetFileNameWithoutExtension(nombre);
            var extension = Path.GetExtension(nombre);

            for (var indice = 1; indice < 1000; indice++)
            {
                destino = Path.Combine(directorio, $"{baseNombre} ({indice}){extension}");
                if (!File.Exists(destino))
                    return destino;
            }

            throw new InvalidOperationException("Ya existe un archivo con ese nombre en la ruta de destino.");
        }

        private static bool PuedeEscribir(string directorio)
        {
            var prueba = Path.Combine(directorio, $".antara-prueba-{Guid.NewGuid():N}.tmp");
            try
            {
                using (var stream = File.Create(prueba, 1, FileOptions.DeleteOnClose))
                    stream.WriteByte(0);

                return true;
            }
            catch
            {
                EliminarSiExiste(prueba);
                return false;
            }
        }

        private static void EliminarSiExiste(string archivo)
        {
            try
            {
                if (File.Exists(archivo))
                    File.Delete(archivo);
            }
            catch
            {
                // Si no se puede borrar, el archivo quedará en la ruta y el escaneo lo registrará.
            }
        }
    }
}
