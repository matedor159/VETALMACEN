using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SisAlmacenProductos.Data;
using System.Linq;
using SisAlmacenProductos.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace SisAlmacenProductos.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public AccountController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public IActionResult Login()
        {
            var role = HttpContext.Session.GetString("Role");

            if (!string.IsNullOrEmpty(role))
            {
                return RedirectToAction("DashboardAdmin", "Admin");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var captchaResponse = Request.Form["g-recaptcha-response"];

            if (string.IsNullOrEmpty(captchaResponse))
            {
                ViewBag.Error = "Por favor completa el reCAPTCHA.";
                return View();
            }

            bool isCaptchaValid = await ValidarReCaptcha(captchaResponse);
            if (!isCaptchaValid)
            {
                ViewBag.Error = "La validación del reCAPTCHA ha fallado. Intenta nuevamente.";
                return View();
            }

            var key = GenerarCacheKey();
            InicializarIntentosSiNoExiste(key);

            var intentosRestantes = _cache.Get<int>(key);

            if (intentosRestantes < 1)
            {
                ViewBag.Error = "Demasiados intentos fallidos. Intenta nuevamente en 5 minutos.";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null && VerifyHashedPassword(user.Password, password))
            {
                // 🔐 Crear lista de claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("Role", user.Role),
                    new Claim("UserId", user.Id.ToString())
                };

                // 🔐 Crear identidad y principal
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // 🔐 Firmar la cookie de autenticación
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // (opcional) guardar en sesión también
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("Username", user.Username);

                return RedirectToAction("DashboardAdmin", "Admin");
            }
            else
            {
                // Falla: reducir intentos
                intentosRestantes--;
                _cache.Set(
                    key,
                    intentosRestantes,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                );

                if (intentosRestantes > 0)
                {
                    ViewBag.Error = $"Usuario o contraseña incorrectos. Intentos restantes: {intentosRestantes}";
                    return View();
                }
                else
                {
                    ViewBag.Error = "Demasiados intentos fallidos. Intenta nuevamente en 5 minutos.";
                    return View();
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // <- cerrar sesión
            HttpContext.Session.Clear(); // <- limpia la sesión también
            return RedirectToAction("Login");
        }

        private bool VerifyHashedPassword(string hashedPasswordWithSalt, string providedPassword)
        {
            var parts = hashedPasswordWithSalt.Split('.');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var storedHash = parts[1];

            var hashToCompare = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: providedPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return storedHash == hashToCompare;
        }

        private void InicializarIntentosSiNoExiste(string key)
        {
            if (!_cache.TryGetValue(key, out int _))
            {
                var valor = 5;
                var opcionesCache = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)); // expira en 5 minutos

                _cache.Set(key, valor, opcionesCache);
            }
        }

        private string GenerarCacheKey()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";
            return $"NRO_INTENTOS_{ip}";
        }

        private async Task<bool> ValidarReCaptcha(string token)
        {
            var secretKey = "6LcnLlorAAAAAMT0ZCDL5pyFjkVW6148qu6BYLVd";

            using var client = new HttpClient();
            var response = await client.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}",
                null);

            var json = await response.Content.ReadAsStringAsync();
            dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            return result.success == true;
        }
    }
}
