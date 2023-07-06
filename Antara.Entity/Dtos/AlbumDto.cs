using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model.Dtos
{
    public class AlbumDto
    {
        [Required]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Ingrese un nombre para el audio")]
        [StringLength(45, ErrorMessage = "Debe ser menor de 45 caracteres")]
        public string Nombre { get; set; }
        [StringLength(150, ErrorMessage = "Debe ser menor de 150 caracteres")]
        public string Descripcion { get; set; }
        public DateTime FechaPublicacion { get; set; }
        [Required]
        public bool EstaPublicado { get; set; }
        [Required]
        public Guid UsuarioId { get; set; }
        [StringLength(150)]
        public string PortadaUrl { get; set; }
        public bool EstaActivo { get; set; }
    }
}
