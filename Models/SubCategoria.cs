// File: Models/SubCategoria.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisAlmacenProductos.Models
{
    public class SubCategoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la subcategoría es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
        public string Nombre { get; set; }

        // FK hacia Categoria
        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public int CategoriaId { get; set; }

        [ForeignKey(nameof(CategoriaId))]
        public virtual Categoria Categoria { get; set; }
    }
}
