// File: Models/Categoria.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SisAlmacenProductos.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
        public string Nombre { get; set; }

        // Navegación: una categoría tiene muchas subcategorías
        public virtual ICollection<SubCategoria> SubCategorias { get; set; } = new List<SubCategoria>();
    }
}
