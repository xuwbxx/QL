using Microsoft.AspNetCore.Mvc;

namespace TechCenter.Home.Areas.YanTu.Controllers
{
    [Area("YanTu")]
    public class CBZworkController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Page1()
        {
            return View();
        }

        public IActionResult Page2()
        {
            return View();
        }

        public IActionResult Page3()
        {
            return View();
        }

        public IActionResult Page4()
        {
            return View();
        }
    }
}
