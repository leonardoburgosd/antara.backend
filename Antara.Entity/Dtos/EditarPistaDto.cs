using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model.Dtos
{
    public class EditarPistaDto
    {
        [Required(ErrorMessage = "Ingrese un nombre para el audio")]
        [StringLength(45, ErrorMessage = "Debe ser menor de 45 caracteres")]
        public string Nombre { get; set; }
        [Range(1920,2021)]
        public int AnoCreacion { get; set; }
        [Required]
        [StringLength(45, ErrorMessage = "Debe ser menor de 45 caracteres")]
        public string Interprete { get; set; }
        public string Compositor { get; set; }
        public string Productor { get; set; }
        [Required]
        public int GeneroId { get; set; }
    }
}
