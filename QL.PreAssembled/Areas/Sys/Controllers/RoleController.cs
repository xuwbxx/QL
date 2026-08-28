using Microsoft.AspNetCore.Mvc;

namespace QL.PreAssembled.Areas.Sys.Controllers
{
    [Area("Sys")]
    public class RoleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
