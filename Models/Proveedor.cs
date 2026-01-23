using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisAlmacenProductos.Models
{
    [Table("proveedor")]
    public class Proveedor
    {
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "El RUC debe tener exactamente 11 dígitos numéricos.")]
        public string RUC { get; set; }

        [Required] public string RazonSocial { get; set; }
        [Required] public string Telefono { get; set; }
        [Required] public string Direccion { get; set; }

        [Required, EmailAddress]
        public string CorreoElectronico { get; set; }

        [Required]
        public string NombreContacto { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual User Usuario { get; set; }
    }
}
