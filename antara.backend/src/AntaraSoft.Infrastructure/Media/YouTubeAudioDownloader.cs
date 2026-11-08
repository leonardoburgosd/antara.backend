using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Media
{
    /// <summary>
    /// Descarga el audio de un enlace de YouTube con yt-dlp (y ffmpeg para la
    /// conversión). Solo acepta dominios de YouTube y guarda siempre un archivo
    /// de audio en la carpeta indicada.
    /// </summary>
    public class YouTubeAudioDownloader : IDescargadorYouTube
    {
        private static readonly HashSet<string> ExtensionesResultado = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3", ".m4a", ".aac", ".flac", ".wav", ".ogg", ".opus"
        };

        // Sufijo que agrega la plantilla de salida para evitar colisiones:
        // "Titulo [dQw4w9WgXcQ].mp3"
        private static readonly Regex SufijoId = new(
            @"\s*\[[A-Za-z0-9_-]{6,}\]\s*$", RegexOptions.Compiled);

        private readonly AppSettings _settings;
        private readonly ILogger<YouTubeAudioDownloader> _logger;

        public YouTubeAudioDownloader(IOptions<AppSettings> options, ILogger<YouTubeAudioDownloader> logger)
        {
            _settings = options?.Value ?? new AppSettings();
            _logger = logger;
        }

        public bool EsUrlYouTube(string url)
        {
            if (!Uri.TryCreate((url ?? string.Empty).Trim(), UriKind.Absolute, out var uri))
                return false;

            if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
                return false;

            var host = uri.Host.ToLowerInvariant();

            return host == "youtube.com"
                || host.EndsWith(".youtube.com", StringComparison.Ordinal)
                || host == "youtu.be"
                || host.EndsWith(".youtu.be", StringComparison.Ordinal);
        }

        public async Task<DescargaAudio> DescargarAsync(
            string url, string directorio, CancellationToken cancellationToken = default)
        {
            if (!EsUrlYouTube(url))
                throw new ArgumentException("Solo se admiten enlaces de YouTube (youtube.com o youtu.be).");

            if (string.IsNullOrWhiteSpace(directorio) || !Directory.Exists(directorio))
                throw new InvalidOperationException("La carpeta de destino de la descarga no existe.");

            var ejecutable = string.IsNullOrWhiteSpace(_settings.YtDlpPath) ? "yt-dlp" : _settings.YtDlpPath;
            var inicio = new ProcessStartInfo
            {
                FileName = ejecutable,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Argumentos como lista (sin shell): la URL nunca se interpreta como comando.
            var argumentos = new List<string>
            {
                "--no-playlist",
                "--no-warnings",
                "--newline",
                "--no-simulate",
                "--extract-audio",
                "--audio-format", "mp3",
                "--audio-quality", "0",
                "--paths", directorio,
                "--output", "%(title).150B [%(id)s].%(ext)s",
                "--print", "after_move:filepath"
            };

            // Cliente de YouTube configurable: "android" evita el bloqueo de bots
            // en redes donde el cliente web exige iniciar sesión.
            if (!string.IsNullOrWhiteSpace(_settings.YtDlpPlayerClient))
            {
                argumentos.Add("--extractor-args");
                argumentos.Add($"youtube:player_client={_settings.YtDlpPlayerClient.Trim()}");
            }

            // Cookies opcionales para redes que exigen sesión de YouTube.
            if (!string.IsNullOrWhiteSpace(_settings.YtDlpCookiesPath) && File.Exists(_settings.YtDlpCookiesPath))
            {
                argumentos.Add("--cookies");
                argumentos.Add(_settings.YtDlpCookiesPath.Trim());
            }
            else if (!string.IsNullOrWhiteSpace(_settings.YtDlpCookiesPath))
            {
                _logger.LogWarning(
                    "Se configuró YtDlpCookiesPath pero el archivo no existe: {Ruta}", _settings.YtDlpCookiesPath);
            }

            argumentos.Add(url.Trim());

            foreach (var argumento in argumentos)
            {
                inicio.ArgumentList.Add(argumento);
            }

            var limite = Math.Max(30, _settings.DescargaYouTubeTimeoutSegundos);

            using var proceso = new Process { StartInfo = inicio };
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(limite));

            if (!proceso.Start())
                throw new InvalidOperationException("No se pudo iniciar la descarga de YouTube (yt-dlp).");

            var salida = proceso.StandardOutput.ReadToEndAsync();
            var errores = proceso.StandardError.ReadToEndAsync();

            try
            {
                await proceso.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    proceso.Kill(entireProcessTree: true);
                }
                catch
                {
                    // El proceso ya terminó.
                }

                throw new InvalidOperationException(cancellationToken.IsCancellationRequested
                    ? "La descarga se canceló."
                    : $"La descarga superó el límite de {limite} segundos.");
            }

            var textoSalida = await salida;
            var textoErrores = await errores;

            if (proceso.ExitCode != 0)
            {
                _logger.LogWarning("yt-dlp terminó con código {Codigo}: {Error}", proceso.ExitCode, textoErrores);
                throw new InvalidOperationException(
                    $"No se pudo descargar el audio del enlace: {Recortar(textoErrores, 400)}");
            }

            var archivo = ObtenerArchivoDescargado(textoSalida, directorio);
            if (archivo == null)
                throw new InvalidOperationException("La descarga terminó sin generar un archivo de audio.");

            archivo = QuitarSufijoId(archivo);

            return new DescargaAudio
            {
                Archivo = archivo,
                Titulo = Path.GetFileNameWithoutExtension(archivo)
            };
        }

        private static string ObtenerArchivoDescargado(string salida, string directorio)
        {
            var rutas = salida
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(linea => linea.Trim())
                .Where(linea => !string.IsNullOrWhiteSpace(linea))
                .ToList();

            foreach (var linea in rutas.AsEnumerable().Reverse())
            {
                var candidato = Path.IsPathRooted(linea) ? linea : Path.Combine(directorio, linea);
                if (ExtensionesResultado.Contains(Path.GetExtension(candidato)) && File.Exists(candidato))
                    return Path.GetFullPath(candidato);
            }

            // Respaldo: si yt-dlp no imprimió la ruta, se toma el audio más reciente.
            return Directory.EnumerateFiles(directorio)
                .Where(archivo => ExtensionesResultado.Contains(Path.GetExtension(archivo)))
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }

        /// <summary>
        /// Quita el "[id]" del nombre del archivo para que la pista quede con un
        /// título legible. Si el nombre limpio ya existe, se conserva el original.
        /// </summary>
        private static string QuitarSufijoId(string archivo)
        {
            var titulo = Path.GetFileNameWithoutExtension(archivo);
            var limpio = SufijoId.Replace(titulo, string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(limpio) || string.Equals(limpio, titulo, StringComparison.Ordinal))
                return archivo;

            try
            {
                var destino = Path.Combine(Path.GetDirectoryName(archivo) ?? string.Empty,
                    limpio + Path.GetExtension(archivo));

                if (File.Exists(destino))
                    return archivo;

                File.Move(archivo, destino);
                return destino;
            }
            catch
            {
                return archivo;
            }
        }

        private static string Recortar(string texto, int maximo)
        {
            var limpio = (texto ?? string.Empty).Replace('\n', ' ').Replace('\r', ' ').Trim();
            if (string.IsNullOrWhiteSpace(limpio))
                return "error desconocido de yt-dlp";

            return limpio.Length <= maximo ? limpio : limpio[..maximo] + "…";
        }
    }
}
