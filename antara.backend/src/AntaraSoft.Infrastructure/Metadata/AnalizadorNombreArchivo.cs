using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AntaraSoft.Infrastructure.Metadata
{
    /// <summary>
    /// Deduce artista y titulo a partir del nombre de un archivo de audio.
    /// Soporta formatos habituales como "01 - Artista - Titulo.mp3",
    /// "Artista - Titulo.flac" o "Titulo.mp3".
    /// </summary>
    public static class AnalizadorNombreArchivo
    {
        private static readonly Regex NumeroInicial = new(
            @"^\s*\d{1,3}\s*[-._)]\s*",
            RegexOptions.Compiled);

        private static readonly Regex Ruido = new(
            @"\s*[\[\(]\s*(official\s*(video|audio|music\s*video)|video\s*official|audio|lyrics?|letra|hd|hq|remaster(ed)?(\s*\d{4})?)\s*[\]\)]",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex Colaboracion = new(
            @"\s+(?:feat\.?|ft\.?|featuring)\s+.*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SufijoRuido = new(
            @"\s*[-–—]?\s*(official\s*(video|audio|music\s*video)|video\s*official|lyric\s*video|lyrics?|letra|hd|hq|remaster(ed)?(\s*\d{4})?)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex Espacios = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex SoloNumero = new(@"^\d{1,3}$", RegexOptions.Compiled);
        private static readonly Regex SoloAnio = new(@"^(19|20)\d{2}$", RegexOptions.Compiled);

        public static (string Artista, string Titulo) Analizar(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                return (null, null);

            var nombre = Path.GetFileNameWithoutExtension(ruta);
            if (string.IsNullOrWhiteSpace(nombre))
                return (null, null);

            nombre = nombre.Replace('_', ' ');
            nombre = NumeroInicial.Replace(nombre, string.Empty);
            nombre = Ruido.Replace(nombre, " ");
            nombre = Espacios.Replace(nombre, " ").Trim();

            var partes = nombre
                .Split(new[] { " - ", " – ", " — " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(parte => Limpiar(parte))
                .Where(parte => parte.Length > 0)
                .ToList();

            if (partes.Count == 0)
                return (null, nombre);

            var utiles = partes
                .Where(parte => !SoloNumero.IsMatch(parte) && !SoloAnio.IsMatch(parte))
                .ToList();

            if (utiles.Count == 0)
                utiles = partes;

            if (utiles.Count == 1)
                return (null, utiles[0]);

            var artista = utiles[0];
            var titulo = LimpiarTitulo(utiles[utiles.Count - 1]);

            if (string.IsNullOrWhiteSpace(titulo))
                titulo = utiles[utiles.Count - 1];

            if (string.Equals(artista, titulo, StringComparison.OrdinalIgnoreCase))
                return (null, titulo);

            return (artista, titulo);
        }

        private static string LimpiarTitulo(string titulo)
        {
            var limpio = Colaboracion.Replace(titulo ?? string.Empty, string.Empty);
            limpio = SufijoRuido.Replace(limpio, string.Empty);
            return Limpiar(limpio);
        }

        private static string Limpiar(string valor) =>
            Espacios.Replace(valor ?? string.Empty, " ").Trim(' ', '-', '.', '_');
    }
}

