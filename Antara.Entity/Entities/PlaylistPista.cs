using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model.Entities
{
    public class PlaylistPista
    {
        public Guid PlaylistId { get; set; }
        public Guid PistaId { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
