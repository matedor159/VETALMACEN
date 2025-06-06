using System;
using System.ComponentModel.DataAnnotations;

namespace SisAlmacenProductos.Models
{
    public class Orden
    {
        public int Id { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        public string Estado { get; set; } = "Pendiente";

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        public string? ModificadoPor { get; set; }


        // Relaciones
        public Producto Producto { get; set; }
        public User Usuario { get; set; }
    }
}
