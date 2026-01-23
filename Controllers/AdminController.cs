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


        public IActionResult AgregarUsuarioAlmacen()
        {
            var roles = _context.Roles
                .Where(r => r.Nombre == "Administrador" || r.Nombre == "Logistica" || r.Nombre == "Almacenero")
                .ToList();

            ViewBag.Roles = roles;
            return View("RegistrarUsuarioAlmacen");
        }


        [HttpPost]
        public IActionResult AgregarUsuarioAlmacen(User model)
        {
            if (_context.Users.Any(u => u.Username == model.Username))
                ModelState.AddModelError("Username", "Ya existe un usuario con ese nombre.");

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _context.Roles
                    .Where(r => r.Nombre == "Administrador" || r.Nombre == "Logistica" || r.Nombre == "Almacenero")
                    .ToList();
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

            var rolSucursal = _context.Roles.First(r => r.Nombre == "Sucursal");

            var user = new User { Username = username, Password = password, RolId = rolSucursal.Id, CreatedAt = DateTime.Now };
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

            // Obtener rol Proveedor
            var rolProveedor = _context.Roles.First(r => r.Nombre == "Proveedor");

            // Crear usuario
            var user = new User
            {
                Username = username,
                Password = password, // ⚠️ luego puedes hashearlo
                RolId = rolProveedor.Id,
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
