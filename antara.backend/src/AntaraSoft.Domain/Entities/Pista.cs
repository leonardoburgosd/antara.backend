using System;

namespace AntaraSoft.Domain.Entities
{
    public record Pista
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int AnoCreacion { get; set; }
        public string Interprete { get; set; }
        public string Compositor { get; set; }
        public string Productor { get; set; }
        public int Reproducciones { get; set; }
        public int GeneroId { get; set; }
        public string Url { get; set; }
        public Guid AlbumId { get; set; }
        public bool EstaActivo { get; set; }
        // Metadatos obtenidos del proveedor externo durante el escaneo de rutas.
        public string Letra { get; set; }
        public string ImagenArtista { get; set; }
        public int DuracionSegundos { get; set; }
        public string FuenteMetadatos { get; set; }
        public string IdExternoMetadatos { get; set; }
        public string MetadatosJson { get; set; }

        public void AumentarReproduccion()
        {
            Reproducciones += 1;
        }

        public bool EsValidoAnoCreacion()
        {
            return AnoCreacion > 1900 && AnoCreacion < DateTime.Now.Year;
        }
    }
}

