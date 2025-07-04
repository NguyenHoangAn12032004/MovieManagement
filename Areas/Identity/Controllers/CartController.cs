using Microsoft.AspNetCore.Mvc;

namespace MovieManagement.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Cart", new { area = "" });
        }
    }
} 