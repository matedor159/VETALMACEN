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

        /* ======================================================
           USUARIO ALMACEN
        ====================================================== */


        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult RegistrarUsuarioAlmacen()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public IActionResult RegistrarUsuarioAlmacen(Models.ViewModels.RegistrarUsuarioAlmacenVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var rolesPermitidos = new[] { "Administrador", "Logistica", "Almacenero" };
            if (!rolesPermitidos.Contains(model.Role))
            {
                ModelState.AddModelError("", "Rol no permitido para Usuario Almacén.");
                return View(model);
            }

            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("", "El usuario ya existe.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Password = HashPassword(model.Password),
                Role = model.Role,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Usuario de almacén registrado correctamente.";
            return RedirectToAction("Usuarios");
        }




        /* ======================================================
           SUCURSAL
        ====================================================== */


        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult RegistrarSucursal()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public IActionResult RegistrarSucursal(Models.ViewModels.RegistrarSucursalVM model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("", "El usuario ya existe.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Password = HashPassword(model.Password),
                Role = "Sucursal",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var sucursal = new Sucursal
            {
                NombreSucursal = model.NombreSucursal,
                Direccion = model.Direccion,
                Telefono = model.Telefono,
                UserId = user.Id
            };

            _context.Sucursales.Add(sucursal);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Sucursal registrada correctamente.";
            return RedirectToAction("Usuarios");
        }



        /* ======================================================
           PROVEEDOR
        ====================================================== */


        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult RegistrarProveedor()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public IActionResult RegistrarProveedor(Models.ViewModels.RegistrarProveedorVM model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("", "El usuario ya existe.");
                return View(model);
            }

            if (_context.Proveedores.Any(p => p.Ruc == model.Ruc))
            {
                ModelState.AddModelError("", "Ya existe un proveedor con ese RUC.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Password = HashPassword(model.Password),
                Role = "Proveedor",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var proveedor = new Proveedor
            {
                Ruc = model.Ruc,
                RazonSocial = model.RazonSocial,
                Telefono = model.Telefono,
                Direccion = model.Direccion,
                CorreoElectronico = model.CorreoElectronico,
                NombreContacto = model.NombreContacto,
                UserId = user.Id
            };

            _context.Proveedores.Add(proveedor);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Proveedor registrado correctamente.";
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

        private string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32
            ));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }
    }
}
