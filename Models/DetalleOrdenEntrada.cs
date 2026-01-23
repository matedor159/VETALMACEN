using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisAlmacenProductos.Models
{
    public class DetalleOrdenEntrada
    {
        public int Id { get; set; }

        [Required]
        public int OrdenEntradaId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal SubTotal { get; set; }

        // Relaciones
        public OrdenEntrada OrdenEntrada { get; set; }
        public Producto Producto { get; set; }
    }
}
