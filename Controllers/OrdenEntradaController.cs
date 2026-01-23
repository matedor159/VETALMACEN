using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SisAlmacenProductos.Data;
using SisAlmacenProductos.Models;

namespace SisAlmacenProductos.Controllers
{
    [Authorize(Roles = "Almacenero")] // Requiere login y rol Almacenero
    public class OrdenEntradaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdenEntradaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: OrdenEntrada
        public async Task<IActionResult> Index()
        {
            var ordenes = await _context.OrdenesEntrada
                .Include(o => o.Proveedor)
                .Include(o => o.Usuario)
                .Include(o => o.Detalles).ThenInclude(d => d.Producto)
                .OrderByDescending(o => o.Fecha)
                .ToListAsync();
            return View(ordenes);
        }

        // GET: OrdenEntrada/Create
        public IActionResult Create()
        {
            ViewData["ProveedorId"] = new SelectList(_context.Proveedores, "Id", "RazonSocial");
            ViewData["Productos"] = _context.Productos.Select(p => new { p.Id, Nombre = p.Nombre + " - Stock: " + p.Stock }).ToList();
            return View();
        }

        // POST: OrdenEntrada/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrdenEntrada ordenEntrada, int[] productoIds, int[] cantidades, decimal[] precios)
        {
            if (productoIds == null || productoIds.Length == 0)
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto.");
            }

            // Validar que el usuario existe (asumiendo autenticación por cookies/session)
            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
            {
                 ModelState.AddModelError("", "Usuario no identificado.");
                 // En producción manejar mejor este caso
            }
            else
            {
                ordenEntrada.UsuarioId = user.Id;
            }

            // Ignorar validaciones de propiedades de navegación que se llenan en el backend
            ModelState.Remove("Usuario");
            ModelState.Remove("Proveedor");
            ModelState.Remove("Detalles");
            // Estado tiene valor por defecto pero el binder puede quejarse si no viene
            ModelState.Remove("Estado");

            if (ModelState.IsValid)
            {
                ordenEntrada.Fecha = DateTime.Now;
                ordenEntrada.Estado = "Recibido"; // Asumimos que la entrada se confirma al crearla para activar stock
                ordenEntrada.Detalles = new List<DetalleOrdenEntrada>();
                decimal totalOrden = 0;

                for (int i = 0; i < productoIds.Length; i++)
                {
                    if (cantidades[i] <= 0) continue; // Ignorar cantidades inválidas

                    var detalle = new DetalleOrdenEntrada
                    {
                        ProductoId = productoIds[i],
                        Cantidad = cantidades[i],
                        PrecioUnitario = precios[i],
                        SubTotal = cantidades[i] * precios[i]
                    };
                    totalOrden += detalle.SubTotal;
                    ordenEntrada.Detalles.Add(detalle);

                }

                ordenEntrada.Total = totalOrden;

                if (ordenEntrada.Detalles.Count > 0)
                {
                    _context.Add(ordenEntrada);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else 
                {
                    ModelState.AddModelError("", "Debe ingresar cantidades válidas para los productos.");
                }
            }

            ViewData["ProveedorId"] = new SelectList(_context.Proveedores, "Id", "RazonSocial", ordenEntrada.ProveedorId);
            ViewData["Productos"] = _context.Productos.Select(p => new { p.Id, Nombre = p.Nombre + " - Stock: " + p.Stock }).ToList();
            return View(ordenEntrada);
        }
        // AJAX: Obtener producto por código
        [HttpGet]
        public async Task<IActionResult> GetProductoByCodigo(string codigo)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Codigo == codigo);

            if (producto == null)
            {
                return Json(new { success = false, message = "Producto no encontrado" });
            }

            return Json(new { success = true, id = producto.Id, nombre = producto.Nombre, precio = producto.Precio });
        }
    }
}
