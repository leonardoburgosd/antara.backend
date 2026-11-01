using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AntaraSoft.Infrastructure.Storage
{
    public class AudioStorageService : IAudioStorage
    {
        private readonly AppSettings _settings;
        private readonly RutaAudioCache _cache;
        private readonly HostPathResolver _resolver;

        public AudioStorageService(AppSettings settings, RutaAudioCache cache = null, HostPathResolver resolver = null)
        {
            _settings = settings;
            _cache = cache;
            _resolver = resolver ?? new HostPathResolver(settings);
        }

        public string ResolvePhysicalPath(string relativeOrStoredPath)
        {
            if (string.IsNullOrWhiteSpace(relativeOrStoredPath))
                return null;

            // Rutas de Windows (C:/Users/...): se traducen a la unidad montada
            // en el contenedor (por ejemplo /host/c/Users/...).
            if (HostPathResolver.EsRutaWindows(relativeOrStoredPath))
            {
                var archivoMapeado = _resolver.ResolverArchivo(relativeOrStoredPath);
                if (!string.IsNullOrWhiteSpace(archivoMapeado))
                    return archivoMapeado;
            }

            // If it's an absolute path (for mounted volumes in container)
            if (Path.IsPathRooted(relativeOrStoredPath))
            {
                var normalizedPath = Path.GetFullPath(relativeOrStoredPath);
                foreach (var location in UbicacionesActivas())
                {
                    var raiz = location.ContainerPath ?? location.PhysicalPath;
                    if (string.IsNullOrWhiteSpace(raiz))
                        continue;

                    var allowedRoot = Path.GetFullPath(raiz);
                    if (normalizedPath.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        return normalizedPath;
                    }
                }
                if (!string.IsNullOrEmpty(_settings.DefaultAudioStoragePath))
                {
                    var defaultRoot = Path.GetFullPath(_settings.DefaultAudioStoragePath);
                    if (normalizedPath.StartsWith(defaultRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        return normalizedPath;
                    }
                }
                return normalizedPath;
            }

            foreach (var location in UbicacionesActivas())
            {
                var bases = new List<string>();
                if (!string.IsNullOrEmpty(location.ContainerPath))
                    bases.Add(location.ContainerPath);

                var mapeada = _resolver.Mapear(location.PhysicalPath);
                if (!string.IsNullOrEmpty(mapeada))
                    bases.Add(mapeada);

                if (!string.IsNullOrEmpty(location.PhysicalPath))
                    bases.Add(location.PhysicalPath);

                foreach (var basePath in bases)
                {
                    var candidatePath = Path.GetFullPath(Path.Combine(basePath, relativeOrStoredPath.TrimStart('/', '\\')));
                    var allowedRoot = Path.GetFullPath(basePath);
                    if (candidatePath.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(candidatePath))
                    {
                        return candidatePath;
                    }
                    if (File.Exists(candidatePath))
                    {
                        return candidatePath;
                    }
                }
            }

            if (!string.IsNullOrEmpty(_settings.DefaultAudioStoragePath))
            {
                var candidatePath = Path.GetFullPath(Path.Combine(_settings.DefaultAudioStoragePath, relativeOrStoredPath.TrimStart('/', '\\')));
                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }
            }

            var localPath = Path.GetFullPath(relativeOrStoredPath);
            if (File.Exists(localPath))
            {
                return localPath;
            }

            return null;
        }

        public bool FileExists(string relativeOrStoredPath)
        {
            var path = ResolvePhysicalPath(relativeOrStoredPath);
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }

        private IEnumerable<AudioStorageLocation> UbicacionesActivas()
        {
            var ubicaciones = new List<AudioStorageLocation>();

            if (_cache != null)
            {
                foreach (var ruta in _cache.Rutas.Where(r => r.EstaActiva))
                {
                    ubicaciones.Add(new AudioStorageLocation
                    {
                        Name = ruta.Nombre,
                        PhysicalPath = ruta.RutaFisica,
                        ContainerPath = ruta.RutaContenedor,
                        IsActive = true
                    });
                }
            }

            foreach (var configurada in _settings.AudioStorageLocations ?? new List<AudioStorageLocation>())
            {
                if (!configurada.IsActive)
                    continue;

                bool duplicada = ubicaciones.Any(actual =>
                    string.Equals(actual.ContainerPath, configurada.ContainerPath, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(actual.PhysicalPath, configurada.PhysicalPath, StringComparison.OrdinalIgnoreCase));

                if (!duplicada)
                    ubicaciones.Add(configurada);
            }

            return ubicaciones;
        }
    }
}

