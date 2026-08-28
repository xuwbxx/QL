using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.StructHandle;
using Service.Base;
using Service.StructHandle;

namespace TongShiBaseStructHandleSys.Areas.Base.Controllers
{
    [Area("Base")]
    public class SystemController : Controller
    {
        private CookieService _cookieService { get; }
        private SystemService _systemService { get; }

        public SystemController(SystemService systemService, CookieService cookieService)
        {
            _cookieService = cookieService;
            _systemService = systemService;
        }

        public IActionResult Login()
        {

            //var IsSuccess = _systemService.UserLogin

            return View();
        }

        [HttpPost]
        public IActionResult DoLogin([FromBody] StructHandleRequest request)
        {
            // 验证用户名密码

            if (request == null || string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
            {
                return Json(new { success = false, message = "登录信息是空的" });
            }

            var IsSuccess = _systemService.UserLogin(request.UserName, request.Password);

            if (!IsSuccess)
            {
                return Json(new { success = false, message = "用户名或密码错误" });
            }

            SHJUserInfo userinfo = new SHJUserInfo();
            userinfo.UserName = request.UserName;

            //保存cookie
            _cookieService.SetUserCookie(userinfo);

            return Json(new { success = true, message = "登录成功" });

        }


    }
}
