using Microsoft.AspNetCore.Mvc;

namespace WindCloud.Areas.BackManage.Controllers
{
    public class MenuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
