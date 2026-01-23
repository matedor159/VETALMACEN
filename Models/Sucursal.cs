using System.ComponentModel.DataAnnotations;

namespace SisAlmacenProductos.Models
{
    public class Sucursal
    {
        public int Id { get; set; }

        [Required]
        public string NombreSucursal { get; set; } = string.Empty;

        [Required]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;

        // Relación con Users
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
