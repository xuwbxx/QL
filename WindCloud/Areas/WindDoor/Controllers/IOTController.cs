using Microsoft.AspNetCore.Mvc;
using Service.Base;
using Service.Wind;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.WindDoor.Controllers
{
    [Area("WindDoor")]
    public class IOTController : BaseController
    {
        public IOTController(CookieService cookieService)
            : base(cookieService)
        {

        }

        public IActionResult Index()
        {
            var User = CurrentUser;
            if (User == null)
            {
                return null;
            }
            ViewData["Name"] = User.RealName ?? "";

            ViewData["Title"] = "风电物联网";

            return View();
        }
    }
}
