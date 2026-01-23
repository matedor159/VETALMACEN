using System.ComponentModel.DataAnnotations;

namespace SisAlmacenProductos.Models.ViewModels
{
    public class RegistrarUsuarioAlmacenVM
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
