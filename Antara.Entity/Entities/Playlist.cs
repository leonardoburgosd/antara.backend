using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model.Entities
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
