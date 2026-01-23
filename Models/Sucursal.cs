using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisAlmacenProductos.Models
{
    [Table("sucursal")]
    public class Sucursal
    {
        public int Id { get; set; }

        [Required] public string NombreSucursal { get; set; }
        [Required] public string Direccion { get; set; }
        [Required] public string Telefono { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual User Usuario { get; set; }
    }
}
