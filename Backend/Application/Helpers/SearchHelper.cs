using System;
using System.Globalization;
using System.Text;

namespace Application.Helpers
{
    /// <summary>
    /// Utilidades para búsqueda flexible insensible a acentos/tildes (diacríticos)
    /// y variantes fonéticas comunes en español.
    /// </summary>
    public static class SearchHelper
    {
        /// <summary>
        /// Normaliza un texto removiendo acentos/diacríticos, pasando a minúsculas
        /// y unificando 'v' con 'b' para tolerancia a errores ortográficos habituales.
        /// </summary>
        public static string Normalize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalizedString.Length);

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            var result = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

            // Tolerancia fonética b / v (ej: "oliver" coincide con "oliber")
            return result.Replace('v', 'b');
        }

        /// <summary>
        /// Determina si 'source' contiene la subcadena 'query' ignorando tildes/acentos,
        /// mayúsculas/minúsculas y la diferencia b/v.
        /// </summary>
        public static bool ContainsFlexible(string? source, string? query)
        {
            if (string.IsNullOrWhiteSpace(query)) return true;
            if (string.IsNullOrWhiteSpace(source)) return false;

            var normSource = Normalize(source);
            var normQuery = Normalize(query.Trim());

            return normSource.Contains(normQuery);
        }
    }
}
