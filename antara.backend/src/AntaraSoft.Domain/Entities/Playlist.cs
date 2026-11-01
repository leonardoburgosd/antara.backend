using System;

namespace AntaraSoft.Domain.Entities
{
    public record Playlist
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string PortadaUrl { get; set; }
        public Guid UsuarioId { get; set; }
        public bool EstaActivo { get; set; }
    }
}

