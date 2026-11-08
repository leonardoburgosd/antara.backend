using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AntaraSoft.Infrastructure.Metadata
{
    /// <summary>
    /// Normalización y similitud de textos usadas por los proveedores de
    /// metadatos para puntuar los resultados de búsqueda.
    /// </summary>
    public static class SimilitudTexto
    {
        private static readonly Regex Espacios = new(@"\s+", RegexOptions.Compiled);

        public static string Normalizar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            var descompuesto = valor.Normalize(NormalizationForm.FormD);
            var sinAcentos = new StringBuilder(descompuesto.Length);

            foreach (var caracter in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
                    sinAcentos.Append(caracter);
            }

            var limpio = sinAcentos.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
            var resultado = new StringBuilder(limpio.Length);

            foreach (var caracter in limpio)
                resultado.Append(char.IsLetterOrDigit(caracter) ? caracter : ' ');

            return Espacios.Replace(resultado.ToString(), " ").Trim();
        }

        /// <summary>1 = idéntico; 0.85 = uno contiene al otro; si no, Jaccard por tokens.</summary>
        public static double Similitud(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return 0;

            if (string.Equals(a, b, StringComparison.Ordinal))
                return 1;

            if (a.Contains(b, StringComparison.Ordinal) || b.Contains(a, StringComparison.Ordinal))
                return 0.85;

            var tokensA = a.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            var tokensB = b.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

            if (tokensA.Count == 0 || tokensB.Count == 0)
                return 0;

            var interseccion = tokensA.Intersect(tokensB).Count();
            var union = tokensA.Union(tokensB).Count();

            return union == 0 ? 0 : (double)interseccion / union;
        }

        /// <summary>Puntaje combinado: 70% título, 30% artista (si se conoce).</summary>
        public static double Puntuar(string tituloResultado, string artistaResultado, string tituloBuscado, string artistaBuscado)
        {
            var similitudTitulo = Similitud(Normalizar(tituloResultado), Normalizar(tituloBuscado));

            if (string.IsNullOrWhiteSpace(artistaBuscado))
                return similitudTitulo;

            var similitudArtista = Similitud(Normalizar(artistaResultado), Normalizar(artistaBuscado));
            return similitudTitulo * 0.7 + similitudArtista * 0.3;
        }
    }
}
