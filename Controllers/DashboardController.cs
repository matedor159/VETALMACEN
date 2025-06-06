using Microsoft.AspNetCore.Mvc;

namespace SisAlmacenProductos.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var role = TempData["Role"] as string;

            ViewBag.Role = role;

            return View();
        }
    }
}
