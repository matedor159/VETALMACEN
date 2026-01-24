using DocumentFormat.OpenXml.Spreadsheet;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisAlmacenProductos.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(20, MinimumLength = 4)]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "El usuario solo puede contener letras, números y _.")]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [RegularExpression(
        "^(Administrador|Almacenero|Cliente|Logistica|Sucursal|Proveedor)$",
        ErrorMessage = "Rol inválido.")]
        public string Role { get; set; } = string.Empty;




        public DateTime CreatedAt { get; set; }
    }
}
