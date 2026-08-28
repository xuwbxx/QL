using Microsoft.AspNetCore.Mvc;
using Service.Base;
using Service.Base.Filter;
using Service.Wind;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.WindDoor.Controllers
{
    [Area("WindDoor")]
    public class MainController : BaseController
    {
        public ProjectService _projectService;

        public MainController(CookieService cookieService, ProjectService projectService) : base(cookieService)
        {
            _projectService = projectService;
        }


        [TypeFilter(typeof(WindCloudFilter))]
        public async Task<IActionResult> Index()
        {

            var list = await _projectService.GetWindProject();
            ViewData["Title"] = "三航局技术中心风电数字化板块";

            var User = CurrentUser;
            if (User == null)
            {
                return null;
            }
            ViewData["Name"] = User.RealName ?? "";

            return View(list);
        }
    }
}
