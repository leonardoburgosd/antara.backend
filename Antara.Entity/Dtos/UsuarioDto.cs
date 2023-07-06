using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Antara.Model.Dtos
{
    public class UsuarioDto
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Ingrese un correo electrónico")]
        [RegularExpression("^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$", ErrorMessage = "Correo electrónico no cumple con el formato")]
        [StringLength(45, ErrorMessage = "Debe ser menor de 45 caracteres")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Ingrese una nombre de perfil")]
        [StringLength(45, ErrorMessage = "Debe ser menor de 45 caracteres")]
        public string Nombre { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public char Genero { get; set; }
        [Required]
        public DateTime FechaRegistro { get; set; }
        [Required(ErrorMessage = "Ingrese un país")]
        public string Pais { get; set; }
        [StringLength(150, ErrorMessage = "Debe ser menor de 150 caracteres")]
        public string FotoPerfil { get; set; }
    }
}
