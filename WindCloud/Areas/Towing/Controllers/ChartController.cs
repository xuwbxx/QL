using Microsoft.AspNetCore.Mvc;

namespace WindCloud.Areas.Towing.Controllers
{
    public class ChartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
