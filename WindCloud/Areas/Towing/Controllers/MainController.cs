using Microsoft.AspNetCore.Mvc;
using Service.Base.Filter;

namespace WindCloud.Areas.Towing.Controllers
{
    [Area("Towing")]
    public class MainController : Controller
    {
        [TypeFilter(typeof(WindCloudFilter))]
        public IActionResult Index()
        {
            return View();
        }
    }
}
