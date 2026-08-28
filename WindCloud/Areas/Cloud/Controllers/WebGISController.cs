using Microsoft.AspNetCore.Mvc;

namespace WindCloud.Areas.Cloud.Controllers
{
    public class WebGISController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
