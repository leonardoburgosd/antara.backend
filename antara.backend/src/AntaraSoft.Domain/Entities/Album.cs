using System;

namespace AntaraSoft.Domain.Entities
{
    public record Album
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool EstaPublicado { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public string PortadaUrl { get; set; }
        public Guid UsuarioId { get; set; }
        public bool EstaActivo { get; set; }
        // Datos adicionales que llegan desde el proveedor de metadatos.
        public string Artista { get; set; }
        public string FuenteMetadatos { get; set; }
        public string IdExternoMetadatos { get; set; }
    }
}

