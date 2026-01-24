using Microsoft.AspNetCore.Mvc;
using SisAlmacenProductos.Data;
using SisAlmacenProductos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using ClosedXML.Excel;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;


using System.Security.Claims; // <-- Added this

namespace SisAlmacenProductos.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AdminController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /* ======================================================
           DASHBOARD
        ====================================================== */
        /* ======================================================
           DASHBOARD
        ====================================================== */
        [Authorize(Roles = "Administrador,Almacenero,Logistica")]
        public IActionResult DashboardAdmin()
        {
            // Para mostrar el rol en la vista
            ViewBag.Role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "";
            ViewBag.Username = User.Identity?.Name ?? "";

            return View("~/Views/Admins/DashboardAdmin.cshtml");
        }

        /* ======================================================
           CLIENTES (VISTA)
        ====================================================== */
        // Removed [Authorize(Roles = "Cliente")] so Admin can see it too if needed, or strictly for Client.
        // But user said "part of clients" not working for Admin. Maybe Admin wants to see what clients see?
        // Or maybe "Gestión de Clientes"? The previous code had "RegistrarUsuario".
        // Let's restore VistaCliente just in case.
        [Authorize(Roles = "Cliente,Administrador,Sucursal")]
        public IActionResult VistaCliente()
        {
             var productos = _context.Productos.Include(p => p.SubCategoria).ToList();
             return View("~/Views/Admins/VistaCliente.cshtml", productos);
        }

        [Authorize(Roles = "Cliente,Administrador,Sucursal")]
        public async Task<IActionResult> MisOrdenes()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login", "Account");

            var ordenes = await _context.Ordenes
                .Include(o => o.Producto)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.FechaSolicitud)
                .ToListAsync();

            return View("~/Views/Admins/MisOrdenes.cshtml", ordenes);
        }

        /* ======================================================
           GESTION USUARIOS
        ====================================================== */
        [Authorize(Roles = "Administrador")]
        public IActionResult RegistrarUsuario()
        {
            return View("~/Views/Admins/RegistrarUsuario.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AgregarUsuario(User user)
        {
            if (ModelState.IsValid)
            {
               // user.Password = HashPassword(user.Password); // Hashing simple
               // No hashing complexity for now to match verify logic
               _context.Users.Add(user);
               await _context.SaveChangesAsync();
               return RedirectToAction(nameof(Usuarios));
            }
            return View("~/Views/Admins/RegistrarUsuario.cshtml", user);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Usuarios()
        {
            var usuarios = _context.Users.ToList();

            return View("~/Views/Admins/Usuarios.cshtml", usuarios);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarUsuario(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View("~/Views/Admins/EditarUsuario.cshtml", user);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarUsuario(int? id)
        {
             if (id == null) return NotFound();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();
             return View("~/Views/Admins/EliminarUsuario.cshtml", user);
        }

        [HttpPost, ActionName("EliminarUsuario")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarUsuarioConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Usuarios));
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarUsuario(int id, User user)
        {
            if (id != user.Id) return NotFound();
            user.Password ??= "";

            // Remove navigation property from validation
            var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (existingUser == null) return NotFound();

            if (string.IsNullOrEmpty(user.Password))
            {
                user.Password = existingUser.Password;
                ModelState.Remove("Password");
            }
            else
            {
                user.Password = HashPassword(user.Password);
            }

            user.CreatedAt = existingUser.CreatedAt;
            ModelState.Remove("CreatedAt");


            if (ModelState.IsValid) {
                try {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                } catch (DbUpdateConcurrencyException) {
                    if (!_context.Users.Any(e => e.Id == user.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Usuarios));
            }
            // Add errors to viewbag for debugging if needed
            return View("~/Views/Admins/EditarUsuario.cshtml", user);
        }

        private string HashPassword(string password)
        {
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }



        /* ======================================================
           PRODUCTOS
        ====================================================== */

        [Authorize(Roles = "Administrador,Almacenero")]
        public IActionResult Productos()
        {
            var productos = _context.Productos
                .Include(p => p.SubCategoria)
                .ToList();

            return View("~/Views/Admins/Productos.cshtml", productos);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult AgregarProducto()
        {
            ViewBag.SubCategorias = _context.SubCategorias.ToList();
            return View("~/Views/Admins/AgregarProducto.cshtml", new Producto());
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AgregarProducto(Producto producto, IFormFile Imagen)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.SubCategorias = _context.SubCategorias.ToList();
                return View("~/Views/Admins/AgregarProducto.cshtml", producto);
            }

            // 📌 SUBIDA A AZURE BLOB
            if (Imagen != null && Imagen.Length > 0)
            {
                var blobConnection = _configuration.GetConnectionString("AzureStorage");
                var containerName = "imagenes";

                var blobServiceClient = new BlobServiceClient(blobConnection);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(Imagen.FileName)}";
                var blobClient = containerClient.GetBlobClient(fileName);

                using (var stream = Imagen.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, new BlobHttpHeaders
                    {
                        ContentType = Imagen.ContentType
                    });
                }

                producto.ImagenUrl = blobClient.Uri.ToString();
            }

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Productos));
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarProducto(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();
            ViewBag.SubCategorias = _context.SubCategorias.ToList();
            return View("~/Views/Admins/EditarProducto.cshtml", producto);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarProducto(int id, Producto producto, IFormFile? Imagen)
        {
            if (id != producto.Id) return NotFound();
            if (ModelState.IsValid) {
                try {
                    if (Imagen != null && Imagen.Length > 0) {
                        var blobConnection = _configuration.GetConnectionString("AzureStorage");
                        var containerName = "imagenes";
                        var blobServiceClient = new BlobServiceClient(blobConnection);
                        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(Imagen.FileName)}";
                        var blobClient = containerClient.GetBlobClient(fileName);
                        using (var stream = Imagen.OpenReadStream()) {
                            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = Imagen.ContentType });
                        }
                        producto.ImagenUrl = blobClient.Uri.ToString();
                    } else {
                         var oldProd = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                         if(oldProd != null) producto.ImagenUrl = oldProd.ImagenUrl;
                    }
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                } catch (DbUpdateConcurrencyException) {
                     if (!_context.Productos.Any(e => e.Id == producto.Id)) return NotFound();
                     else throw;
                }
                return RedirectToAction(nameof(Productos));
            }
            ViewBag.SubCategorias = _context.SubCategorias.ToList();
            return View("~/Views/Admins/EditarProducto.cshtml", producto);
        }

        /* ======================================================
           USUARIO ALMACEN
        ====================================================== */


        public IActionResult AgregarUsuarioAlmacen()
        {
            ViewBag.Roles = new[] { "Administrador", "Logistica", "Almacenero" };

            return View("RegistrarUsuarioAlmacen");
        }


        [HttpPost]
        public IActionResult AgregarUsuarioAlmacen(User model)
        {
            if (_context.Users.Any(u => u.Username == model.Username))
                ModelState.AddModelError("Username", "Ya existe un usuario con ese nombre.");

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new[] { "Administrador", "Logistica", "Almacenero" };
                return View("RegistrarUsuarioAlmacen", model);
            }

            _context.Users.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Usuarios");
        }



        /* ======================================================
           SUCURSAL
        ====================================================== */

        public IActionResult AgregarSucursal() => View("RegistrarSucursal");

        [HttpPost]
        public IActionResult AgregarSucursal(string username, string password, string nombreSucursal, string direccion, string telefono)
        {
            if (_context.Users.Any(u => u.Username == username))
                ModelState.AddModelError("Username", "Ya existe un usuario con ese nombre.");

            if (!ModelState.IsValid) return View("RegistrarSucursal");

            var user = new User
            {
                Username = username,
                Password = password,
                Role = "Sucursal",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var sucursal = new Sucursal
            {
                NombreSucursal = nombreSucursal,
                Direccion = direccion,
                Telefono = telefono,
                UsuarioId = user.Id
            };
            _context.Sucursales.Add(sucursal);
            _context.SaveChanges();

            return RedirectToAction("Usuarios");
        }




        /* ======================================================
           PROVEEDOR
        ====================================================== */

        public IActionResult AgregarProveedor()
        {
            return View("RegistrarProveedor");
        }

        [HttpPost]
        public IActionResult AgregarProveedor(
    string username,
    string password,
    string ruc,
    string razonSocial,
    string telefono,
    string direccion,
    string correoElectronico,
    string nombreContacto
)
        {
            // Validar usuario duplicado
            if (_context.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("Username", "Ya existe un usuario con ese nombre.");
            }

            // Validar RUC: solo números y exactamente 11 caracteres
            if (string.IsNullOrEmpty(ruc) || !System.Text.RegularExpressions.Regex.IsMatch(ruc, @"^\d{11}$"))
            {
                ModelState.AddModelError("RUC", "El RUC debe contener exactamente 11 dígitos numéricos.");
            }

            // Validar email
            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(correoElectronico))
            {
                ModelState.AddModelError("CorreoElectronico", "El correo electrónico no es válido.");
            }

            if (!ModelState.IsValid)
            {
                return View("RegistrarProveedor");
            }

            // Obtener rol Proveedor, Crear usuario


            var user = new User
            {
                Username = username,
                Password = password,
                Role = "Proveedor",
                CreatedAt = DateTime.Now
            };


            _context.Users.Add(user);
            _context.SaveChanges();

            // Crear proveedor relacionado
            var proveedor = new Proveedor
            {
                RUC = ruc,
                RazonSocial = razonSocial,
                Telefono = telefono,
                Direccion = direccion,
                CorreoElectronico = correoElectronico,
                NombreContacto = nombreContacto,
                UsuarioId = user.Id
            };

            _context.Proveedores.Add(proveedor);
            _context.SaveChanges();

            return RedirectToAction("Usuarios");
        }


        /* ======================================================
           EXPORTAR EXCEL
        ====================================================== */

        public IActionResult ExportarExcel()
        {
            var productos = _context.Productos
                .Include(p => p.SubCategoria)
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Productos");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Código";
            worksheet.Cell(1, 3).Value = "Nombre";
            worksheet.Cell(1, 4).Value = "Descripción";
            worksheet.Cell(1, 5).Value = "SubCategoría";
            worksheet.Cell(1, 6).Value = "Precio";
            worksheet.Cell(1, 7).Value = "Stock";
            worksheet.Cell(1, 8).Value = "Marca";
            worksheet.Cell(1, 9).Value = "Imagen URL";

            int fila = 2;
            foreach (var p in productos)
            {
                worksheet.Cell(fila, 1).Value = p.Id;
                worksheet.Cell(fila, 2).Value = p.Codigo;
                worksheet.Cell(fila, 3).Value = p.Nombre;
                worksheet.Cell(fila, 4).Value = p.Descripcion;
                worksheet.Cell(fila, 5).Value = p.SubCategoria?.Nombre;
                worksheet.Cell(fila, 6).Value = p.Precio;
                worksheet.Cell(fila, 7).Value = p.Stock;
                worksheet.Cell(fila, 8).Value = p.Marca;
                worksheet.Cell(fila, 9).Value = p.ImagenUrl;
                fila++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Productos.xlsx"
            );
        }

        /* ======================================================
           UTILIDAD PASSWORD
        ====================================================== */

        /* ======================================================
           UTILIDADES DB (Solo Admin)
        ====================================================== */
        [Authorize(Roles = "Administrador")]
        public IActionResult FixDatabase()
        {
            try
            {
                var sql = @"
                CREATE TABLE IF NOT EXISTS ordenes (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    ProductoId INT NOT NULL,
                    UserId INT NOT NULL,
                    Cantidad INT NOT NULL,
                    Estado VARCHAR(50) DEFAULT 'Pendiente',
                    FechaSolicitud DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ModificadoPor VARCHAR(100) NULL,
                    CONSTRAINT fk_ordenes_producto FOREIGN KEY (ProductoId) REFERENCES producto(Id),
                    CONSTRAINT fk_ordenes_usuario FOREIGN KEY (UserId) REFERENCES usuario(Id)
                );";
                
                _context.Database.ExecuteSqlRaw(sql);
                return Content("Base de datos reparada: Tabla 'ordenes' verificada/creada.");
            }
            catch (Exception ex)
            {
                return Content($"Error al reparar BD: {ex.Message}");
            }
        }


        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarProducto(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos.Include(p => p.SubCategoria).FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null) return NotFound();
            return View("~/Views/Admins/EliminarProducto.cshtml", producto);
        }

        [HttpPost, ActionName("EliminarProducto")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarProductoConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Productos));
        }

    }
}
