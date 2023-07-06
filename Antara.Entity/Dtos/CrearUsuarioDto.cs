using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model.Dtos
{
    public class CrearUsuarioDto
    {
        [Required(ErrorMessage = "Ingrese un correo electrónico")]
        [RegularExpression("^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$", ErrorMessage = "Correo electrónico no cumple con el formato")]
        [StringLength(45, ErrorMessage = "Debe ser menor de 45 caracteres")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Ingrese una contraseña")]
        [StringLength(18, ErrorMessage = "Debe tener entre 6 y 18 caracteres", MinimumLength = 6)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Ingrese un nombre de perfil")]
        [StringLength(45, ErrorMessage = "Debe ser menor de 45 caracteres")]
        [RegularExpression("^([a-zA-Z]{2,}\\s?([a-zA-Z]{1,})?)", ErrorMessage = "Nombre no cumple con el formato")]
        public string Nombre { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public char Genero { get; set; }
        [Required(ErrorMessage = "Ingrese un país")]
        public string Pais { get; set; }
        [Required(ErrorMessage = "Ingrese un tipo de usuario")]
        public string Tipo { get; set; }
    }
}
