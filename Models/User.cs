using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SisAlmacenProductos.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // <-- Esto es clave
        public DateTime CreatedAt { get; set; }


    }
}
