using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model.Dtos
{
    public class CrearPistaDto
    {
        [Range(1920,2021)]
        public int AnoCreacion { get; set; }
        [Required]
        [StringLength(45, ErrorMessage = "Debe ser menor de 45 caracteres")]
        public string Interprete { get; set; }
        [Required]
        [StringLength(45, ErrorMessage = "Debe ser menor de 45 caracteres")]
        public string Compositor { get; set; }
        public string Productor { get; set; }
        [Required]
        public int GeneroId { get; set; }
        [Required]
        public Guid AlbumId { get; set; }
    }
}
