using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model.Dtos
{
    public class PistaDto
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Ingrese un nombre para el audio")]
        [StringLength(45, ErrorMessage = "Debe ser menor de 45 caracteres")]
        public string Nombre { get; set; }
        [Required]
        public DateTime FechaRegistro { get; set; }
        //[Range(1920,2021)]
        public int AnoCreacion { get; set; }
        [Required]
        [StringLength(45, ErrorMessage = "Debe ser menor de 45 caracteres")]
        public string Interprete { get; set; }
        public string Compositor { get; set; }
        public string Productor { get; set; }
        public int Reproducciones { get; set; }
        [Required]
        public int GeneroId { get; set; }
        [Required]
        [StringLength(150)]
        public string Url { get; set; }
        [Required]
        public Guid AlbumId { get; set; }
        public bool EstaActivo { get; set; }
    }
}
