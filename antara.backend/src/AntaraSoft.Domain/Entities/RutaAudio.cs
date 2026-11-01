using System;

namespace AntaraSoft.Domain.Entities
{
    public record RutaAudio
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string RutaFisica { get; set; }
        public string RutaContenedor { get; set; }
        public string Descripcion { get; set; }
        public bool EstaActiva { get; set; }
        public bool EsPredeterminada { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}

