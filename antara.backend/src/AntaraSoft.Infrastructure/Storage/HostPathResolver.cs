using AntaraSoft.Application;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AntaraSoft.Infrastructure.Storage
{
    /// <summary>
    /// Traduce rutas físicas del host (por ejemplo C:/Users/Leo/Music) a la ruta
    /// equivalente dentro del contenedor, donde cada unidad se monta en
    /// /host/&lt;letra&gt; (C:/ -> /host/c). Así el panel puede aceptar rutas de
    /// Windows sin que el administrador tenga que escribir la ruta del contenedor.
    /// </summary>
    public class HostPathResolver
    {
        private static readonly Regex UnidadWindows = new(
            @"^([A-Za-z]):(?:[\\/]+(.*))?$",
            RegexOptions.Compiled);

        private readonly AppSettings _settings;

        public HostPathResolver(AppSettings settings)
        {
            _settings = settings ?? new AppSettings();
        }

        public HostPathResolver(IOptions<AppSettings> options)
            : this(options?.Value)
        {
        }

        public string MountRoot =>
            string.IsNullOrWhiteSpace(_settings.HostMountRoot)
                ? "/host"
                : _settings.HostMountRoot.Trim().TrimEnd('/');

        public static bool EsRutaWindows(string ruta) =>
            !string.IsNullOrWhiteSpace(ruta) && UnidadWindows.IsMatch(ruta.Trim());

        /// <summary>
        /// Convierte C:/Users/Leo/Music en /host/c/Users/Leo/Music. Devuelve null
        /// si la ruta no es de Windows o si intenta salir del punto de montaje.
        /// </summary>
        public string Mapear(string ruta)
        {
            if (!EsRutaWindows(ruta))
                return null;

            var coincidencia = UnidadWindows.Match(ruta.Trim());
            var unidad = coincidencia.Groups[1].Value.ToLowerInvariant();
            var resto = coincidencia.Groups[2].Success
                ? coincidencia.Groups[2].Value.Replace('\\', '/').Trim('/')
                : string.Empty;

            var candidato = string.IsNullOrEmpty(resto)
                ? $"{MountRoot}/{unidad}"
                : $"{MountRoot}/{unidad}/{resto}";

            try
            {
                var completo = Path.GetFullPath(candidato);
                var raiz = Path.GetFullPath(MountRoot + "/");

                return completo.StartsWith(raiz, StringComparison.OrdinalIgnoreCase)
                    ? completo
                    : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Devuelve la primera ruta existente entre la ruta mapeada de la unidad,
        /// la ruta del contenedor y la ruta física tal cual.
        /// </summary>
        public string ResolverDirectorio(string rutaFisica, string rutaContenedor)
        {
            var candidatos = Candidatos(rutaFisica, rutaContenedor);

            foreach (var candidato in candidatos)
            {
                if (Directory.Exists(candidato))
                    return candidato;
            }

            return candidatos.Count > 0 ? candidatos[0] : null;
        }

        /// <summary>
        /// Devuelve la primera ruta de archivo existente, contemplando la
        /// traducción de rutas de Windows.
        /// </summary>
        public string ResolverArchivo(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                return null;

            var mapeada = Mapear(ruta);
            if (!string.IsNullOrWhiteSpace(mapeada) && File.Exists(mapeada))
                return mapeada;

            var normalizada = Normalizar(ruta);
            if (File.Exists(normalizada))
                return normalizada;

            return null;
        }

        /// <summary>
        /// Candidatos en orden de preferencia: unidad mapeada, ruta del
        /// contenedor y ruta física original.
        /// </summary>
        public List<string> Candidatos(string rutaFisica, string rutaContenedor)
        {
            var candidatos = new List<string>();

            void Agregar(string valor)
            {
                if (string.IsNullOrWhiteSpace(valor))
                    return;

                var limpia = Normalizar(valor);
                if (!candidatos.Exists(actual => string.Equals(actual, limpia, StringComparison.OrdinalIgnoreCase)))
                    candidatos.Add(limpia);
            }

            Agregar(Mapear(rutaFisica));
            Agregar(rutaContenedor);
            Agregar(rutaFisica);

            return candidatos;
        }

        public static string Normalizar(string ruta) =>
            string.IsNullOrWhiteSpace(ruta) ? null : ruta.Trim().Replace('\\', '/');
    }
}

