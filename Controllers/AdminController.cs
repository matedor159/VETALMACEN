using Microsoft.AspNetCore.Mvc;
using SisAlmacenProductos.Data;
using SisAlmacenProductos.Models;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.IO;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SisAlmacenProductos.PruebasUnitarias;


namespace SisAlmacenProductos.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UsuariosTests _usuariosTests;
        private readonly ProductoTests _productoTests;  

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
            _usuariosTests = new UsuariosTests();
            _productoTests = new ProductoTests();

        }

        public IActionResult VistaCliente()
        {
            var productos = _context.Productos.ToList();
            return View("~/Views/Admins/VistaCliente.cshtml", productos);
        }

        public IActionResult DashboardAdmin()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role == "Cliente")
            {
                var productos = _context.Productos.ToList();

                // ✅ Esto buscará correctamente la vista si está en /Views/Admins/VistaCliente.cshtml
                return View("~/Views/Admins/VistaCliente.cshtml", productos);
            }

            ViewBag.Role = role;
            return View("~/Views/Admins/DashboardAdmin.cshtml");
        }



        // Acción para mostrar la lista de productos
        public IActionResult Productos()
        {
            var productos = _context.Productos.ToList();
            return View("~/Views/Admins/Productos.cshtml", productos);
        }

        // Acción GET para agregar producto
        public IActionResult AgregarProducto()
        {
            var producto = new Producto();
            return View("~/Views/Admins/AgregarProducto.cshtml", producto);
        }

        // Prueba 7-9
        // Acción POST para guardar nuevo producto
        [HttpPost]
        public async Task<IActionResult> AgregarProducto(Producto producto)
        {
            if (ModelState.IsValid)
            {
                // Validaciones directas del código

                // Validación de código no mayor a 5 caracteres
                if (producto.Codigo.Length > 5)
                {
                    TempData["ErrorCodigo"] = "❌ El código no debe ser mayor a 5 caracteres.";
                    return View("~/Views/Admins/AgregarProducto.cshtml", producto);
                }

                // Validación de precio no negativo
                if (producto.Precio < 0)
                {
                    TempData["ErrorPrecio"] = "❌ El precio no puede ser negativo.";
                    return View("~/Views/Admins/AgregarProducto.cshtml", producto);
                }

                // Validación de URL de imagen con HTTPS
                if (!producto.ImagenUrl.StartsWith("https://"))
                {
                    TempData["ErrorImagenUrl"] = "❌ La URL de la imagen debe comenzar con 'https://'.";
                    return View("~/Views/Admins/AgregarProducto.cshtml", producto);
                }

                // Si todas las validaciones son correctas, guardamos el producto
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Productos));
            }

            return View("~/Views/Admins/AgregarProducto.cshtml", producto);
        }

        // Acción GET para editar producto
        public IActionResult EditarProducto(int id)
        {
            var producto = _context.Productos.Find(id);

            if (producto == null)
            {
                return NotFound();
            }

            return View("~/Views/Admins/EditarProducto.cshtml", producto); // ← Aquí se corrigió
        }

        // Acción POST para editar producto
        [HttpPost]
        public async Task<IActionResult> EditarProducto(Producto producto)
        {
            if (ModelState.IsValid)
            {
                // Validaciones directas del código

                // Validación de código no mayor a 5 caracteres
                if (producto.Codigo.Length > 5)
                {
                    TempData["ErrorCodigo"] = "❌ El código no debe ser mayor a 5 caracteres.";
                    return View("~/Views/Admins/EditarProducto.cshtml", producto);
                }

                // Validación de precio no negativo
                if (producto.Precio < 0)
                {
                    TempData["ErrorPrecio"] = "❌ El precio no puede ser negativo.";
                    return View("~/Views/Admins/EditarProducto.cshtml", producto);
                }

                // Validación de URL de imagen con HTTPS
                if (!producto.ImagenUrl.StartsWith("https://"))
                {
                    TempData["ErrorImagenUrl"] = "❌ La URL de la imagen debe comenzar con 'https://'.";
                    return View("~/Views/Admins/EditarProducto.cshtml", producto);
                }

                // Si todas las validaciones son correctas, actualizamos el producto
                _context.Productos.Update(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Productos));
            }

            return View("~/Views/Admins/EditarProducto.cshtml", producto);
        }

    // Acción GET para confirmar eliminación
    public IActionResult EliminarProducto(int id)
        {
            var producto = _context.Productos.Find(id);

            if (producto == null)
            {
                return NotFound();
            }

            return View("~/Views/Admins/EliminarProducto.cshtml", producto); // Vista de confirmación
        }

        // Acción POST para eliminar definitivamente
        [HttpPost, ActionName("EliminarProducto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProductoConfirmado(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Productos)); // Vuelve a la lista
        }


        // Muestra lista de usuarios
        public IActionResult RegistrarUsuario()
        {
            var usuarios = _context.Users.ToList();

            // Recuperar resultados de pruebas si existen
            if (TempData["ResultadosPruebas"] != null)
            {
                ViewBag.ResultadosPruebas = TempData["ResultadosPruebas"];
            }

            return View("~/Views/Admins/Usuarios.cshtml", usuarios);
        }


        // GET: Muestra formulario de registro
        public IActionResult AgregarUsuario()
        {
            return View("~/Views/Admins/RegistrarUsuario.cshtml");
        }

        // Prueba 1-6
        // Ejecuta cada prueba y guarda el resultado
        // POST: Procesa registro de usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarUsuario(User usuario)
        {
            // Validar si el username ya existe
            bool usernameExiste = await _context.Users.AnyAsync(u => u.Username == usuario.Username);
            if (usernameExiste)
            {
                ModelState.AddModelError("Username", "El nombre de usuario ya existe. Elija otro.");
            }

            // Validar si la contraseña es fuerte
            if (string.IsNullOrWhiteSpace(usuario.Password) ||
                usuario.Password.Length < 8 ||
                !usuario.Password.Any(char.IsUpper) ||
                !usuario.Password.Any(char.IsLower) ||
                !usuario.Password.Any(char.IsDigit) ||
                !usuario.Password.Any(ch => "!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?".Contains(ch)))
            {
                ModelState.AddModelError("Password", "La contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas, números y un carácter especial.");
            }

            if (ModelState.IsValid)
            {
                _context.Users.Add(usuario);
                await _context.SaveChangesAsync();

                // ✅ Ejecuta pruebas solo después de registro exitoso
                var resultadosPruebas = _usuariosTests.EjecutarPruebas();
                TempData["ResultadosPruebas"] = resultadosPruebas;

                return RedirectToAction(nameof(RegistrarUsuario));
            }

            return View("~/Views/Admins/RegistrarUsuario.cshtml", usuario);
        }


        // Acción GET para mostrar el formulario de edición
        public IActionResult EditarUsuario(int id)
        {
            var usuario = _context.Users.Find(id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View("~/Views/Admins/EditarUsuario.cshtml", usuario);
        }

        // Acción POST para actualizar el usuario
        [HttpPost]
        public async Task<IActionResult> EditarUsuario(User usuario)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Adjuntar el usuario al contexto
                    _context.Attach(usuario);

                    // Marcar toda la entidad como modificada
                    _context.Entry(usuario).State = EntityState.Modified;

                    // Evitar modificar el campo CreatedAt
                    _context.Entry(usuario).Property(u => u.CreatedAt).IsModified = false;

                    await _context.SaveChangesAsync();

                    return RedirectToAction("RegistrarUsuario");
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Manejo opcional de errores
                    return NotFound();
                }
            }

            return View("~/Views/Admins/EditarUsuario.cshtml", usuario);
        }


        // GET: Confirmación antes de eliminar
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var usuario = await _context.Users.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View("~/Views/Admins/EliminarUsuario.cshtml", usuario); // Vista de confirmación
        }

        // POST: Eliminación real
        [HttpPost, ActionName("EliminarUsuario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuarioConfirmado(int id)
        {
            var usuario = await _context.Users.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            _context.Users.Remove(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction("RegistrarUsuario");
        }


        [HttpPost]
        public IActionResult Ingreso(int id, int cantidad)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.Id == id);
            if (producto != null && cantidad > 0)
            {
                producto.Stock += cantidad;
                _context.SaveChanges();
                TempData["Success"] = $"Stock aumentado en {cantidad} unidades.";
            }
            else
            {
                TempData["Error"] = "Error al registrar ingreso.";
            }

            // 🔁 Redirige a la lista de productos (que sí espera List<Producto>)
            return RedirectToAction(nameof(Productos));
        }

        [HttpPost]
        public IActionResult Salida(int id, int cantidad)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.Id == id);
            if (producto != null && cantidad > 0 && producto.Stock >= cantidad)
            {
                producto.Stock -= cantidad;
                _context.SaveChanges();
                TempData["Success"] = $"Stock reducido en {cantidad} unidades.";
            }
            else
            {
                TempData["Error"] = "No se pudo registrar la salida. Verifique el stock disponible.";
            }

            // 🔁 Redirige a la lista de productos (que sí espera List<Producto>)
            return RedirectToAction(nameof(Productos));
        }

        public IActionResult ExportarExcel()
        {
            var productos = _context.Productos.ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Productos");

            // Encabezados
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Codigo de produto";
            worksheet.Cell(1, 3).Value = "Nombre";
            worksheet.Cell(1, 4).Value = "Descripcion";
            worksheet.Cell(1, 5).Value = "Categoría";
            worksheet.Cell(1, 6).Value = "Precio";
            worksheet.Cell(1, 7).Value = "Stock";
            worksheet.Cell(1, 8).Value = "Marca";
            worksheet.Cell(1, 9).Value = "Categoria";

            // Datos
            int fila = 2;
            foreach (var producto in productos)
            {
                worksheet.Cell(fila, 1).Value = producto.Id;
                worksheet.Cell(fila, 2).Value = producto.Id;
                worksheet.Cell(fila, 3).Value = producto.Nombre;
                worksheet.Cell(fila, 4).Value = producto.Descripcion;
                worksheet.Cell(fila, 5).Value = producto.Categoria;
                worksheet.Cell(fila, 6).Value = producto.Precio;
                worksheet.Cell(fila, 7).Value = producto.Stock;
                worksheet.Cell(fila, 8).Value = producto.Marca;
                worksheet.Cell(fila, 9).Value = producto.Categoria;
                fila++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Productos.xlsx");
        }

        [HttpPost]
        public IActionResult ExportarExcelOrdenes()
        {
            var ordenes = _context.Ordenes
                .Include(o => o.Usuario)
                .Include(o => o.Producto)
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Solicitudes");

            // Encabezados
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Cliente";
            worksheet.Cell(1, 3).Value = "Producto";
            worksheet.Cell(1, 4).Value = "Cantidad";
            worksheet.Cell(1, 5).Value = "Estado";
            worksheet.Cell(1, 6).Value = "Modificado Por";
            worksheet.Cell(1, 7).Value = "Fecha - Hora";

            // Contenido
            for (int i = 0; i < ordenes.Count; i++)
            {
                var o = ordenes[i];
                worksheet.Cell(i + 2, 1).Value = o.Id;
                worksheet.Cell(i + 2, 2).Value = o.Usuario.Username;
                worksheet.Cell(i + 2, 3).Value = o.Producto.Nombre;
                worksheet.Cell(i + 2, 4).Value = o.Cantidad;
                worksheet.Cell(i + 2, 5).Value = o.Estado;
                worksheet.Cell(i + 2, 6).Value = o.ModificadoPor ?? "Sin modificar";
                worksheet.Cell(i + 2, 7).Value = o.FechaSolicitud.ToString("g");
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Solicitudes_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }

        [HttpPost]
        public IActionResult ExportarUsuariosExcel()
        {
            var usuarios = _context.Users.ToList();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Usuarios");

            // Encabezados
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Usuario";
            worksheet.Cell(1, 3).Value = "Rol";
            worksheet.Cell(1, 4).Value = "Fecha de Registro";

            // Datos
            for (int i = 0; i < usuarios.Count; i++)
            {
                var u = usuarios[i];
                worksheet.Cell(i + 2, 1).Value = u.Id;
                worksheet.Cell(i + 2, 2).Value = u.Username;
                worksheet.Cell(i + 2, 3).Value = u.Role;
                worksheet.Cell(i + 2, 4).Value = u.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            }

            // Exportar como archivo
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Usuarios_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }

        // ✅ Vista: Solicitudes de clientes
        public async Task<IActionResult> Solicitudes()
        {
            var ordenes = await _context.Ordenes
                .Include(o => o.Producto)
                .Include(o => o.Usuario)
                .OrderByDescending(o => o.FechaSolicitud)
                .ToListAsync();

            return View("~/Views/Admins/Solicitudes.cshtml", ordenes);
        }

        // ✅ POST: Confirmar o rechazar solicitud
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            var orden = await _context.Ordenes
                .Include(o => o.Producto)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
                return NotFound();

            if (nuevoEstado == "Confirmado" && orden.Estado != "Confirmado")
            {
                if (orden.Producto.Stock >= orden.Cantidad)
                {
                    orden.Producto.Stock -= orden.Cantidad;
                }
                else
                {
                    TempData["Error"] = "No hay suficiente stock para confirmar esta orden.";
                    return RedirectToAction("Solicitudes");
                }
            }

            orden.Estado = nuevoEstado;
            orden.ModificadoPor = HttpContext.Session.GetString("Username"); // Nombre del admin actual
            await _context.SaveChangesAsync();

            return RedirectToAction("Solicitudes");
        }

        [HttpPost]
        public async Task<IActionResult> PedirOrden(int productoId, int cantidad)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                TempData["Error"] = "Usuario no autenticado.";
                return RedirectToAction("Cliente", "Home");
            }

            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction("Cliente", "Home");
            }

            if (producto.Stock < cantidad)
            {
                TempData["Error"] = "No hay suficiente stock disponible.";
                return RedirectToAction("Cliente", "Home");
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
            return RedirectToAction("Cliente", "Home");
        }

        public async Task<IActionResult> MisOrdenes()
        {
            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                TempData["Error"] = "No se pudo encontrar el usuario.";
                return RedirectToAction("VistaCliente");
            }

            var ordenes = await _context.Ordenes
                .Include(o => o.Producto)
                .Where(o => o.UserId == user.Id)
                .ToListAsync();

            return View("~/Views/Admins/MisOrdenes.cshtml", ordenes);
        }

    }
}
