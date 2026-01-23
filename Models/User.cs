using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SisAlmacenProductos.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "El nombre de usuario debe tener entre 4 y 20 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números y guiones bajos.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "La contraseña debe tener mayúsculas, minúsculas, números y un carácter especial.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio.")]
        public int RolId { get; set; }

        [ForeignKey("RolId")]
        public virtual Rol Rol { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // <-- Esto es clave
        public DateTime CreatedAt { get; set; }
    }
}
