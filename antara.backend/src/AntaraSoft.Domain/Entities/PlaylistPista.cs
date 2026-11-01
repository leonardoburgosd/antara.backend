using System;

namespace AntaraSoft.Domain.Entities
{
    public record PlaylistPista
    {
        public Guid PlaylistId { get; set; }
        public Guid PistaId { get; set; }
    }
}

