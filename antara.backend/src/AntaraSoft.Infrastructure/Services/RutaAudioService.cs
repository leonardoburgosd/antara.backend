using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using AntaraSoft.Infrastructure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Services
{
    public class RutaAudioService : IRutaAudioService
    {
        private static readonly HashSet<string> ExtensionesAudio = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3", ".flac", ".wav", ".m4a", ".aac", ".ogg", ".opus", ".wma", ".aiff", ".alac"
        };

        private readonly IRutaAudioRepository _repository;
        private readonly RutaAudioCache _cache;
        private readonly HostPathResolver _resolver;

        public RutaAudioService(IRutaAudioRepository repository, RutaAudioCache cache, HostPathResolver resolver)
        {
            _repository = repository;
            _cache = cache;
            _resolver = resolver;
        }

        public Task<List<RutaAudio>> ObtenerTodas() => _repository.ObtenerTodas();

        public async Task<RutaAudio> ObtenerPorId(Guid id)
        {
            var ruta = await _repository.ObtenerPorId(id);
            if (ruta == null)
                throw new KeyNotFoundException("La ruta de audio no existe.");
            return ruta;
        }

        public async Task<RutaAudio> Crear(CrearRutaAudioDto dto)
        {
            var nombre = Validar(dto);

            if (await _repository.ExisteNombre(nombre))
                throw new InvalidOperationException("Ya existe una ruta de audio con ese nombre.");

            var ruta = new RutaAudio
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                RutaFisica = Normalizar(dto.RutaFisica),
                RutaContenedor = Normalizar(dto.RutaContenedor),
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                EstaActiva = dto.EstaActiva,
                EsPredeterminada = dto.EsPredeterminada,
                FechaRegistro = DateTime.UtcNow,
                FechaActualizacion = null
            };

            await _repository.Crear(ruta);

            if (ruta.EsPredeterminada)
            {
                await _repository.QuitarPredeterminada(ruta.Id);
            }
            else if (!await _repository.ExistePredeterminada())
            {
                await _repository.EstablecerPredeterminada(ruta.Id);
            }

            await RefrescarCacheAsync();
            return await _repository.ObtenerPorId(ruta.Id);
        }

        public async Task<RutaAudio> Actualizar(Guid id, ActualizarRutaAudioDto dto)
        {
            var actual = await _repository.ObtenerPorId(id);
            if (actual == null)
                throw new KeyNotFoundException("La ruta de audio no existe.");

            var nombre = Validar(dto);

            if (await _repository.ExisteNombre(nombre, id))
                throw new InvalidOperationException("Ya existe una ruta de audio con ese nombre.");

            var ruta = actual with
            {
                Nombre = nombre,
                RutaFisica = Normalizar(dto.RutaFisica),
                RutaContenedor = Normalizar(dto.RutaContenedor),
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                EstaActiva = dto.EstaActiva,
                EsPredeterminada = dto.EsPredeterminada,
                FechaActualizacion = DateTime.UtcNow
            };

            await _repository.Actualizar(ruta);

            if (ruta.EsPredeterminada)
            {
                await _repository.QuitarPredeterminada(ruta.Id);
            }

            await AsegurarPredeterminadaAsync();
            await RefrescarCacheAsync();

            return await _repository.ObtenerPorId(id);
        }

        public async Task Eliminar(Guid id)
        {
            var actual = await _repository.ObtenerPorId(id);
            if (actual == null)
                throw new KeyNotFoundException("La ruta de audio no existe.");

            await _repository.Eliminar(id);
            await AsegurarPredeterminadaAsync();
            await RefrescarCacheAsync();
        }

        public async Task<PruebaRutaAudioDto> Probar(Guid id)
        {
            var ruta = await _repository.ObtenerPorId(id);
            if (ruta == null)
                throw new KeyNotFoundException("La ruta de audio no existe.");

            var rutaEfectiva = _resolver.ResolverDirectorio(ruta.RutaFisica, ruta.RutaContenedor);
            var resultado = new PruebaRutaAudioDto
            {
                RutaEvaluada = rutaEfectiva,
                RutaFisicaConfigurada = !string.IsNullOrWhiteSpace(ruta.RutaFisica),
                Timestamp = DateTime.UtcNow
            };

            if (string.IsNullOrWhiteSpace(rutaEfectiva))
            {
                resultado.Mensaje = "La ruta no tiene una ruta física ni una ruta de contenedor configurada.";
                return resultado;
            }

            try
            {
                resultado.Existe = Directory.Exists(rutaEfectiva);
                resultado.EsDirectorio = resultado.Existe;

                if (!resultado.Existe)
                {
                    resultado.Mensaje = MensajeRutaNoVisible(ruta);
                }
                else
                {
                    resultado.Escribible = ProbarEscritura(rutaEfectiva);
                    resultado.ArchivosAudio = ContarArchivosAudio(rutaEfectiva);
                    resultado.Subdirectorios = Directory.GetDirectories(rutaEfectiva).Length;

                    if (resultado.Escribible)
                        resultado.Mensaje = $"Ruta accesible. Se encontraron {resultado.ArchivosAudio} archivos de audio.";
                    else
                        resultado.Mensaje = $"Ruta accesible en solo lectura. Se encontraron {resultado.ArchivosAudio} archivos de audio.";
                }
            }
            catch (Exception err)
            {
                resultado.Mensaje = $"No se pudo comprobar la ruta: {err.Message}";
            }

            if (resultado.RutaFisicaConfigurada &&
                !string.Equals(ruta.RutaFisica, rutaEfectiva, StringComparison.OrdinalIgnoreCase))
            {
                resultado.RutaFisicaVisible = EsRutaFisicaVisible(ruta.RutaFisica);
            }

            return resultado;
        }

        private bool EsRutaFisicaVisible(string rutaFisica)
        {
            if (string.IsNullOrWhiteSpace(rutaFisica))
                return false;

            try
            {
                if (Directory.Exists(rutaFisica))
                    return true;

                var mapeada = _resolver.Mapear(rutaFisica);
                return !string.IsNullOrWhiteSpace(mapeada) && Directory.Exists(mapeada);
            }
            catch
            {
                return false;
            }
        }

        private string MensajeRutaNoVisible(RutaAudio ruta)
        {
            if (HostPathResolver.EsRutaWindows(ruta.RutaFisica))
            {
                var unidad = char.ToLowerInvariant(ruta.RutaFisica.Trim()[0]);
                var montada = _resolver.UnidadesMontadas()
                    .Contains(unidad.ToString(), StringComparer.OrdinalIgnoreCase);

                if (!montada)
                {
                    return $"La ruta no existe o la unidad {char.ToUpperInvariant(unidad)}: no está montada en el " +
                           $"contenedor. Unidades montadas: {_resolver.ResumenUnidades()}. Vuelve a desplegar con " +
                           "scripts/montar-unidades.sh o indica la ruta del contenedor.";
                }

                return $"La unidad {char.ToUpperInvariant(unidad)}: está montada, pero la carpeta " +
                       $"\"{ruta.RutaFisica}\" no existe dentro de ella.";
            }

            return "La ruta no existe o no es visible desde el contenedor de la API.";
        }

        private static string Validar(CrearRutaAudioDto dto)
        {
            if (dto == null)
                throw new ArgumentException("Los datos de la ruta son obligatorios.");

            var nombre = dto.Nombre?.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("Ingrese un nombre para la ruta.");

            if (string.IsNullOrWhiteSpace(dto.RutaFisica) && string.IsNullOrWhiteSpace(dto.RutaContenedor))
                throw new ArgumentException("Ingrese la ruta física, la ruta del contenedor o ambas.");

            return nombre;
        }

        private static string Validar(ActualizarRutaAudioDto dto)
        {
            if (dto == null)
                throw new ArgumentException("Los datos de la ruta son obligatorios.");

            var nombre = dto.Nombre?.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("Ingrese un nombre para la ruta.");

            if (string.IsNullOrWhiteSpace(dto.RutaFisica) && string.IsNullOrWhiteSpace(dto.RutaContenedor))
                throw new ArgumentException("Ingrese la ruta física, la ruta del contenedor o ambas.");

            return nombre;
        }

        private static string Normalizar(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                return null;

            var limpia = ruta.Trim().Replace('\\', '/');
            while (limpia.Length > 3 && limpia.EndsWith("/"))
                limpia = limpia[..^1];

            return limpia;
        }

        private async Task AsegurarPredeterminadaAsync()
        {
            if (await _repository.ExistePredeterminada())
                return;

            var todas = await _repository.ObtenerTodas();
            var candidata = todas.FirstOrDefault(r => r.EstaActiva) ?? todas.FirstOrDefault();
            if (candidata != null)
                await _repository.EstablecerPredeterminada(candidata.Id);
        }

        private async Task RefrescarCacheAsync()
        {
            var rutas = await _repository.ObtenerTodas();
            _cache.Actualizar(rutas.Where(r => r.EstaActiva));
        }

        private static bool ProbarEscritura(string directorio)
        {
            var archivo = Path.Combine(directorio, $".antara-prueba-{Guid.NewGuid():N}.tmp");
            try
            {
                using (var stream = File.Create(archivo, 1, FileOptions.DeleteOnClose))
                {
                    stream.WriteByte(0);
                }
                return true;
            }
            catch
            {
                try
                {
                    if (File.Exists(archivo))
                        File.Delete(archivo);
                }
                catch
                {
                }
                return false;
            }
        }

        private static int ContarArchivosAudio(string directorio)
        {
            try
            {
                var opciones = new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    IgnoreInaccessible = true,
                    ReturnSpecialDirectories = false,
                    AttributesToSkip = FileAttributes.System
                };

                return Directory.EnumerateFiles(directorio, "*", opciones)
                    .Count(archivo => ExtensionesAudio.Contains(Path.GetExtension(archivo)));
            }
            catch
            {
                return 0;
            }
        }
    }
}

