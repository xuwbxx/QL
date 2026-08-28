using Microsoft.AspNetCore.Mvc;
using Service.Base;
using Service.Wind;
using Service.Wind.BackManage;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.BackManage.Controllers
{
    [Area("BackManage")]
    public class MainController : BaseController
    {
        private readonly BackManageService _backManageService;

        public MainController(CookieService cookieService, ProjectService projectService, BackManageService backManageService)
            : base(cookieService)
        {
            _backManageService = backManageService;
        }

        public IActionResult Door()
        {
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                return RedirectToAction("ErrorPage");
            }

            //限制超级管理员进入
            if (!_backManageService.IsAdminOrProjectDirector(CurrentUser.UserCode))
            {
                return Redirect("/Cloud/CloudBase/ErrorPage?ErrorText=用户不是超级管理员或项目经理，不能进入后台配置界面。");
            }

            return View();
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
