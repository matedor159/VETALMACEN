using System.ComponentModel.DataAnnotations;

namespace SisAlmacenProductos.Models.ViewModels
{
    public class RegistrarProveedorVM
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "El RUC debe tener exactamente 11 dígitos numéricos.")]
        public string Ruc { get; set; } = string.Empty;

        [Required]
        public string RazonSocial { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required]
        public string NombreContacto { get; set; } = string.Empty;
    }
}
