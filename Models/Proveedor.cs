using System;
using System.ComponentModel.DataAnnotations;

namespace SisAlmacenProductos.Models
{
    public class Proveedor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string RazonSocial { get; set; }

        [StringLength(20)]
        public string? RUC { get; set; }

        [StringLength(150)]
        public string? NombreContacto { get; set; }

        [StringLength(255)]
        public string? Direccion { get; set; }

        [StringLength(50)]
        public string? Telefono { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
