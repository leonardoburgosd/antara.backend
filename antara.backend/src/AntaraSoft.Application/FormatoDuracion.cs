using System;

namespace AntaraSoft.Application
{
    /// <summary>
    /// Convierte una duración en segundos al formato legible que usa la API
    /// (<c>m:ss</c> o <c>h:mm:ss</c>) para que el reproductor muestre el total
    /// de la canción sin cálculos adicionales.
    /// </summary>
    public static class FormatoDuracion
    {
        public static string Formatear(int segundos)
        {
            var total = segundos > 0 ? segundos : 0;
            var tiempo = TimeSpan.FromSeconds(total);

            return total >= 3600
                ? $"{(int)tiempo.TotalHours}:{tiempo.Minutes:D2}:{tiempo.Seconds:D2}"
                : $"{tiempo.Minutes}:{tiempo.Seconds:D2}";
        }
    }
}
