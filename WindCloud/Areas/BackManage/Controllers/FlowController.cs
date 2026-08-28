using Microsoft.AspNetCore.Mvc;

namespace WindCloud.Areas.BackManage.Controllers
{
    public class FlowController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
