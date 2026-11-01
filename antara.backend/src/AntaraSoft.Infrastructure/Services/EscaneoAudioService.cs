using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using AntaraSoft.Infrastructure.Metadata;
using AntaraSoft.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Services
{
    /// <summary>
    /// Escanea las rutas de audio activas, detecta las canciones que aun no
    /// estan registradas y las enriquece con metadatos de un proveedor externo.
    /// El trabajo corre en segundo plano y limita la concurrencia (4 por defecto).
    /// </summary>
    public class EscaneoAudioService : IEscaneoAudioService
    {
        private static readonly HashSet<string> ExtensionesAudio = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3", ".flac", ".wav", ".m4a", ".aac", ".ogg", ".opus", ".wma", ".aiff", ".alac", ".ape"
        };

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly EnriquecedorMetadatos _enriquecedor;
        private readonly HostPathResolver _resolver;
        private readonly AppSettings _settings;
        private readonly ILogger<EscaneoAudioService> _logger;

        private readonly object _sync = new();
        private readonly Dictionary<Guid, EscaneoJob> _jobs = new();
        private EscaneoJob _activo;

        public EscaneoAudioService(
            IServiceScopeFactory scopeFactory,
            EnriquecedorMetadatos enriquecedor,
            HostPathResolver resolver,
            IOptions<AppSettings> options,
            ILogger<EscaneoAudioService> logger)
        {
            _scopeFactory = scopeFactory;
            _enriquecedor = enriquecedor;
            _resolver = resolver;
            _settings = options?.Value ?? new AppSettings();
            _logger = logger;
        }

        public EscaneoAudioDto Iniciar(Guid usuarioId)
        {
            lock (_sync)
            {
                if (_activo != null && EsActivo(_activo))
                    throw new InvalidOperationException("Ya hay un escaneo de biblioteca en curso.");

                var job = new EscaneoJob
                {
                    Id = Guid.NewGuid(),
                    Estado = "en_cola",
                    IniciadoEn = DateTime.UtcNow,
                    MaxParalelo = Math.Max(1, _settings.EscaneoAudioMaxParalelo)
                };

                _jobs[job.Id] = job;
                _activo = job;
                LimpiarHistorial();

                _ = Task.Run(() => EjecutarAsync(job, usuarioId));
                return Snapshot(job);
            }
        }

        public EscaneoAudioDto ObtenerActual()
        {
            lock (_sync)
            {
                var job = _activo ?? _jobs.Values.OrderByDescending(item => item.IniciadoEn).FirstOrDefault();
                return job == null ? null : Snapshot(job);
            }
        }

        public EscaneoAudioDto Obtener(Guid id)
        {
            lock (_sync)
            {
                return _jobs.TryGetValue(id, out var job) ? Snapshot(job) : null;
            }
        }

        public bool Cancelar(Guid id)
        {
            lock (_sync)
            {
                if (!_jobs.TryGetValue(id, out var job) || !EsActivo(job))
                    return false;

                job.Cts.Cancel();
                return true;
            }
        }

        private async Task EjecutarAsync(EscaneoJob job, Guid usuarioId)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var rutasRepository = scope.ServiceProvider.GetRequiredService<IRutaAudioRepository>();
                var biblioteca = scope.ServiceProvider.GetRequiredService<IBibliotecaRepository>();
                var audioStorage = scope.ServiceProvider.GetRequiredService<IAudioStorage>();

                CambiarEstado(job, "descubriendo");

                var rutas = (await rutasRepository.ObtenerTodas())
                    .Where(ruta => ruta.EstaActiva)
                    .ToList();

                if (rutas.Count == 0)
                    throw new InvalidOperationException("No hay rutas de audio activas para escanear.");

                var registradas = await biblioteca.ObtenerRutasRegistradas();
                var normalizadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var url in registradas)
                {
                    AgregarNormalizada(normalizadas, url);
                    try
                    {
                        AgregarNormalizada(normalizadas, audioStorage.ResolvePhysicalPath(url));
                    }
                    catch
                    {
                    }
                }

                var archivos = new List<string>();
                var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var ruta in rutas)
                {
                    var raiz = ElegirRaiz(ruta);
                    if (raiz == null)
                    {
                        lock (job.Sync)
                            job.Mensaje = $"La ruta '{ruta.Nombre}' no es accesible desde la API. " +
                                          "Verifica que la carpeta exista y que su unidad esté montada " +
                                          $"en el contenedor (por ejemplo C:/ -> {_resolver.MountRoot}/c).";

                        continue;
                    }

                    try
                    {
                        foreach (var archivo in EnumerarArchivos(raiz, job.Cts.Token))
                        {
                            var completo = Path.GetFullPath(archivo);
                            if (vistos.Add(NormalizarRuta(completo)))
                                archivos.Add(completo);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception error)
                    {
                        lock (job.Sync)
                            job.Mensaje = $"No se pudo leer '{ruta.Nombre}': {error.Message}";

                        _logger.LogWarning(error, "No se pudo leer la ruta {Ruta}", ruta.Nombre);
                    }
                }

                var nuevos = archivos.Where(archivo => !normalizadas.Contains(NormalizarRuta(archivo))).ToList();

                lock (job.Sync)
                {
                    job.TotalArchivos = archivos.Count;
                    job.Omitidos = archivos.Count - nuevos.Count;
                    job.Pendientes = nuevos.Count;
                }

                CambiarEstado(job, "procesando");

                var albumes = new ConcurrentDictionary<string, Lazy<Task<Guid>>>(StringComparer.OrdinalIgnoreCase);
                var generos = new ConcurrentDictionary<string, Lazy<Task<int>>>(StringComparer.OrdinalIgnoreCase);

                await Parallel.ForEachAsync(
                    nuevos,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Math.Max(1, _settings.EscaneoAudioMaxParalelo),
                        CancellationToken = job.Cts.Token
                    },
                    async (archivo, token) =>
                    {
                        lock (job.Sync)
                            job.ArchivoActual = Path.GetFileName(archivo);

                        try
                        {
                            await ProcesarArchivoAsync(job, biblioteca, archivo, usuarioId, albumes, generos, token);
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception error)
                        {
                            lock (job.Sync)
                            {
                                job.Errores++;
                                AgregarResultado(job, new EscaneoAudioResultadoDto
                                {
                                    Archivo = archivo,
                                    Nombre = Path.GetFileNameWithoutExtension(archivo),
                                    Estado = "error",
                                    Mensaje = error.Message,
                                    Fecha = DateTime.UtcNow
                                });
                            }

                            _logger.LogWarning(error, "No se pudo procesar el archivo {Archivo}", archivo);
                        }
                        finally
                        {
                            lock (job.Sync)
                                job.Procesados++;
                        }
                    });

                CambiarEstado(job, "completado");
            }
            catch (OperationCanceledException)
            {
                CambiarEstado(job, "cancelado");
            }
            catch (Exception error)
            {
                lock (job.Sync)
                    job.Mensaje = error.Message;

                CambiarEstado(job, "error");
                _logger.LogError(error, "Fallo el escaneo de biblioteca");
            }
            finally
            {
                lock (job.Sync)
                {
                    job.FinalizadoEn = DateTime.UtcNow;
                    job.ArchivoActual = null;
                }

                lock (_sync)
                {
                    if (ReferenceEquals(_activo, job))
                        _activo = null;
                }
            }
        }

        private async Task ProcesarArchivoAsync(
            EscaneoJob job,
            IBibliotecaRepository biblioteca,
            string archivo,
            Guid usuarioId,
            ConcurrentDictionary<string, Lazy<Task<Guid>>> albumes,
            ConcurrentDictionary<string, Lazy<Task<int>>> generos,
            CancellationToken cancellationToken)
        {
            var (artista, titulo) = AnalizadorNombreArchivo.Analizar(archivo);
            if (string.IsNullOrWhiteSpace(titulo))
                titulo = Path.GetFileNameWithoutExtension(archivo);

            var carpeta = Path.GetFileName(Path.GetDirectoryName(archivo) ?? string.Empty);
            var metadatos = await _enriquecedor.ObtenerAsync(artista, titulo, carpeta, cancellationToken);

            var nombre = Truncar(string.IsNullOrWhiteSpace(metadatos.Titulo) ? titulo : metadatos.Titulo, 255);
            var interprete = Truncar(
                metadatos.Artistas != null && metadatos.Artistas.Count > 0
                    ? string.Join(", ", metadatos.Artistas)
                    : (string.IsNullOrWhiteSpace(artista) ? "Desconocido" : artista),
                255);

            var albumNombre = Truncar(
                !string.IsNullOrWhiteSpace(metadatos.Album)
                    ? metadatos.Album
                    : (!string.IsNullOrWhiteSpace(carpeta) ? carpeta : "Desconocido"),
                255);

            var albumArtista = Truncar(metadatos.ArtistaPrincipal ?? interprete, 255);

            var albumClave = !string.IsNullOrWhiteSpace(metadatos.IdAlbumExterno)
                ? $"ext:{metadatos.IdAlbumExterno}"
                : $"nom:{albumNombre?.ToLowerInvariant()}|{albumArtista?.ToLowerInvariant()}";

            var albumPerezoso = albumes.GetOrAdd(albumClave, _ => new Lazy<Task<Guid>>(() =>
                biblioteca.ObtenerOCrearAlbum(new Album
                {
                    Id = Guid.NewGuid(),
                    Nombre = albumNombre,
                    Descripcion = $"Álbum de {albumArtista}",
                    EstaPublicado = false,
                    FechaPublicacion = metadatos.FechaPublicacion ?? default,
                    PortadaUrl = metadatos.PortadaUrl,
                    UsuarioId = usuarioId,
                    EstaActivo = true,
                    Artista = albumArtista,
                    FuenteMetadatos = metadatos.Fuente,
                    IdExternoMetadatos = Truncar(metadatos.IdAlbumExterno, 100)
                })));

            var albumId = await albumPerezoso.Value;

            var generoId = 0;
            if (!string.IsNullOrWhiteSpace(metadatos.Genero))
            {
                var generoLimpio = Truncar(metadatos.Genero, 100);
                var generoPerezoso = generos.GetOrAdd(generoLimpio, nombreGenero =>
                    new Lazy<Task<int>>(() => biblioteca.ObtenerOCrearGenero(nombreGenero)));

                generoId = await generoPerezoso.Value;
            }

            var pista = new Pista
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                FechaRegistro = DateTime.UtcNow,
                AnoCreacion = metadatos.Ano,
                Interprete = interprete,
                Compositor = null,
                Productor = null,
                Reproducciones = 0,
                GeneroId = generoId,
                Url = archivo,
                AlbumId = albumId,
                EstaActivo = true,
                Letra = metadatos.Letra,
                ImagenArtista = metadatos.ImagenArtistaUrl,
                DuracionSegundos = metadatos.DuracionSegundos,
                FuenteMetadatos = metadatos.Encontrado ? metadatos.Fuente : null,
                IdExternoMetadatos = Truncar(metadatos.IdExterno, 100),
                MetadatosJson = metadatos.Json
            };

            await biblioteca.CrearPista(pista);

            lock (job.Sync)
            {
                if (metadatos.Encontrado)
                    job.Nuevos++;
                else
                    job.SinMetadatos++;

                AgregarResultado(job, new EscaneoAudioResultadoDto
                {
                    Archivo = archivo,
                    Nombre = pista.Nombre,
                    Interprete = pista.Interprete,
                    Album = albumNombre,
                    PortadaUrl = metadatos.PortadaUrl,
                    ImagenArtista = metadatos.ImagenArtistaUrl,
                    Ano = metadatos.Ano,
                    TieneLetra = !string.IsNullOrWhiteSpace(metadatos.Letra),
                    Estado = metadatos.Encontrado ? "nuevo" : "sin_metadatos",
                    Mensaje = metadatos.Encontrado
                        ? (string.IsNullOrWhiteSpace(metadatos.Letra)
                            ? "Metadatos importados."
                            : "Metadatos y letra importados.")
                        : "El proveedor no encontró la canción; se registró con los datos del archivo.",
                    Fecha = DateTime.UtcNow
                });
            }
        }

        private static void AgregarResultado(EscaneoJob job, EscaneoAudioResultadoDto resultado)
        {
            job.Resultados.Insert(0, resultado);
            if (job.Resultados.Count > 30)
                job.Resultados.RemoveRange(30, job.Resultados.Count - 30);
        }

        private static void CambiarEstado(EscaneoJob job, string estado)
        {
            lock (job.Sync)
                job.Estado = estado;
        }

        private static bool EsActivo(EscaneoJob job) =>
            job.Estado == "en_cola" || job.Estado == "descubriendo" || job.Estado == "procesando";

        private static EscaneoAudioDto Snapshot(EscaneoJob job)
        {
            lock (job.Sync)
            {
                return new EscaneoAudioDto
                {
                    Id = job.Id,
                    Estado = job.Estado,
                    Activo = EsActivo(job),
                    IniciadoEn = job.IniciadoEn,
                    FinalizadoEn = job.FinalizadoEn,
                    TotalArchivos = job.TotalArchivos,
                    Pendientes = job.Pendientes,
                    Procesados = job.Procesados,
                    Nuevos = job.Nuevos,
                    SinMetadatos = job.SinMetadatos,
                    Omitidos = job.Omitidos,
                    Errores = job.Errores,
                    ArchivoActual = job.ArchivoActual,
                    Mensaje = job.Mensaje,
                    MaxParalelo = job.MaxParalelo,
                    Resultados = job.Resultados.ToList()
                };
            }
        }

        private void LimpiarHistorial()
        {
            var antiguos = _jobs.Values
                .Where(job => !EsActivo(job))
                .OrderByDescending(job => job.IniciadoEn)
                .Skip(10)
                .ToList();

            foreach (var job in antiguos)
            {
                _jobs.Remove(job.Id);
                job.Cts.Dispose();
            }
        }

        private string ElegirRaiz(RutaAudio ruta)
        {
            var resuelta = _resolver.ResolverDirectorio(ruta.RutaFisica, ruta.RutaContenedor);
            return !string.IsNullOrWhiteSpace(resuelta) && Directory.Exists(resuelta) ? resuelta : null;
        }

        private static IEnumerable<string> EnumerarArchivos(string raiz, CancellationToken cancellationToken)
        {
            var opciones = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                ReturnSpecialDirectories = false,
                AttributesToSkip = FileAttributes.System
            };

            foreach (var archivo in Directory.EnumerateFiles(raiz, "*", opciones))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (ExtensionesAudio.Contains(Path.GetExtension(archivo)))
                    yield return archivo;
            }
        }

        private static void AgregarNormalizada(HashSet<string> normalizadas, string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                return;

            try
            {
                normalizadas.Add(NormalizarRuta(ruta));
            }
            catch
            {
            }
        }

        private static string NormalizarRuta(string ruta)
        {
            var completo = Path.GetFullPath(ruta).Replace('\\', '/');
            return completo.TrimEnd('/').ToLowerInvariant();
        }

        private static string Truncar(string valor, int maximo)
        {
            if (string.IsNullOrEmpty(valor))
                return valor;

            var limpio = valor.Trim();
            return limpio.Length <= maximo ? limpio : limpio[..maximo];
        }

        private sealed class EscaneoJob
        {
            public Guid Id;
            public string Estado = "en_cola";
            public DateTime IniciadoEn;
            public DateTime? FinalizadoEn;
            public int TotalArchivos;
            public int Pendientes;
            public int Procesados;
            public int Nuevos;
            public int SinMetadatos;
            public int Omitidos;
            public int Errores;
            public string ArchivoActual;
            public string Mensaje;
            public int MaxParalelo;
            public List<EscaneoAudioResultadoDto> Resultados = new();
            public CancellationTokenSource Cts = new();
            public readonly object Sync = new();
        }
    }
}

