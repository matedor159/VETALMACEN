using System.ComponentModel.DataAnnotations;

namespace SisAlmacenProductos.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required]
        public string Codigo { get; set; }  // Código de producto

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Descripcion { get; set; } // Descripción del producto

        [Required]
        public string Marca { get; set; } // Marca del producto

        [Required]
        public string Categoria { get; set; }

        public string ImagenUrl { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Precio { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}
