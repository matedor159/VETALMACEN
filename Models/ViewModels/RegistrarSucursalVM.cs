using System.ComponentModel.DataAnnotations;

namespace SisAlmacenProductos.Models.ViewModels
{
    public class RegistrarSucursalVM
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string NombreSucursal { get; set; } = string.Empty;

        [Required]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;
    }
}
