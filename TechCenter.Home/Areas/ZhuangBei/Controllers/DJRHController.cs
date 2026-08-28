using Microsoft.AspNetCore.Mvc;

namespace TechCenter.Home.Areas.ZhuangBei.Controllers
{
    [Area("ZhuangBei")]
    public class DJRHController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Page1()
        {
            return View();
        }
    }
}
