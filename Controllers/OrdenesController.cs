using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SisAlmacenProductos.Data;
using SisAlmacenProductos.Models;

namespace SisAlmacenProductos.Controllers
{
    public class OrdenesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdenesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]

        [HttpPost]
        public async Task<IActionResult> PedirOrden(int productoId, int cantidad)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Error"] = "Debe iniciar sesión para hacer pedidos.";
                return RedirectToAction("VistaCliente", "Admin");
            }

            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("VistaCliente", "Admin");
            }

            var orden = new Orden
            {
                ProductoId = productoId,
                UserId = user.Id,
                Cantidad = cantidad,
                Estado = "Pendiente"
            };

            _context.Ordenes.Add(orden);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Orden enviada correctamente.";
            return RedirectToAction("VistaCliente", "Admin");
        }


        // ADMIN: Ver Solicitudes
        public async Task<IActionResult> Solicitudes()
        {
            var ordenes = await _context.Ordenes
                .Include(o => o.Producto)
                .Include(o => o.Usuario)
                .ToListAsync();

            return View(ordenes);
        }

        // ADMIN: Cambiar estado
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            var orden = await _context.Ordenes.Include(o => o.Producto).FirstOrDefaultAsync(o => o.Id == id);
            if (orden == null) return NotFound();

            if (nuevoEstado == "Confirmado" && orden.Estado != "Confirmado")
            {
                if (orden.Producto.Stock >= orden.Cantidad)
                {
                    orden.Producto.Stock -= orden.Cantidad;
                }
                else
                {
                    TempData["Error"] = "Stock insuficiente.";
                    return RedirectToAction("Solicitudes");
                }
            }

            orden.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            return RedirectToAction("Solicitudes");
        }
    }

}
