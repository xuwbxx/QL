using Microsoft.AspNetCore.Mvc;
using Service.Base;
using Service.Wind;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.Cloud.Controllers
{
    [Area("Cloud")]
    public class CloudBaseController : BaseController
    {
        public CloudBaseController(CookieService cookieService, ProjectService projectService)
            : base(cookieService)
        {

        }

        public IActionResult Index()
        {
            return View();
        }

        public PartialViewResult CloudBoard()
        {
            if (CurrentUser == null)
            {
                return null;
            }
            ViewData["Name"] = CurrentUser.RealName ?? "";
            ViewData["FirstName"] = string.IsNullOrEmpty(CurrentUser.RealName) ? "无" : CurrentUser.RealName.Substring(0, 1);
            ViewData["DepartName"] = CurrentUser.DepartName ?? "";

            return PartialView();
        }

        public ActionResult ErrorPage(string ErrorText)
        {
            if (string.IsNullOrEmpty(ErrorText))
            {
                ErrorText = "您的登录信息失效了，请从交建通重新进入。";
            }
            ViewData["ErrorText"] = ErrorText ?? "未知";
            return View();
        }
    }
}
