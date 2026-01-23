using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisAlmacenProductos.Models
{
    [Table("rol")]
    public class Rol
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }
    }
}
