using Microsoft.AspNetCore.Mvc;

namespace LibraryInventoryManagementSystem1.Controllers
{
    public class SplashController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}