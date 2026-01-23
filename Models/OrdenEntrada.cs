using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisAlmacenProductos.Models
{
    public class OrdenEntrada
    {
        public int Id { get; set; }

        [Required]
        public int ProveedorId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente";

        [Column(TypeName = "decimal(12, 2)")]
        public decimal Total { get; set; } = 0.00m;

        public string? Observacion { get; set; }

        // Relaciones
        public Proveedor Proveedor { get; set; }
        public User Usuario { get; set; }
        public List<DetalleOrdenEntrada> Detalles { get; set; }
    }
}
