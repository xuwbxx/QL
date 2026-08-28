using Microsoft.AspNetCore.Mvc;

namespace WindCloud.Areas.Towing.Controllers
{
    public class ManageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
