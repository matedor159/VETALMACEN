using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisAlmacenProductos.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El código no debe tener más de 50 caracteres.")]
        public string Codigo { get; set; }  // varchar(50) en la BD

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no debe tener más de 100 caracteres.")]
        public string Nombre { get; set; } // varchar(100)

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; } // text

        [Required(ErrorMessage = "La marca es obligatoria.")]
        [MaxLength(100, ErrorMessage = "La marca no debe tener más de 100 caracteres.")]
        public string Marca { get; set; } // varchar(100)

        // --- Subcategoría (FK existente en la BD) ---
        [Required(ErrorMessage = "La subcategoría es obligatoria.")]
        public int SubCategoriaId { get; set; }

        [ForeignKey(nameof(SubCategoriaId))]
        public virtual SubCategoria SubCategoria { get; set; }


        // ImagenUrl en la BD permite NULL; la dejo como opcional aquí.
        [MaxLength(255, ErrorMessage = "La URL de la imagen no debe exceder 255 caracteres.")]
        [Url(ErrorMessage = "La URL de la imagen no es válida.")]
        public string ImagenUrl { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0, (double)99999999.99, ErrorMessage = "El precio no puede ser negativo.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; } // decimal(10,2)

        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; } = 0; // default 0 en BD
    }
}
