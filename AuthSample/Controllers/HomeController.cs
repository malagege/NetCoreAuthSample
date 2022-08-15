using Microsoft.AspNetCore.Mvc;

namespace AuthSample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
