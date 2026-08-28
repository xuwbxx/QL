using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Service.Base;
using System.Diagnostics;
using WindCloud.Models;

namespace WindCloud.Controllers
{
    public class HomeController : Controller
    {
        private CookieService _cookieService { get; }

        public HomeController(CookieService cookieService)
        {
            _cookieService = cookieService;
        }

        public IActionResult Index()
        {

            SHJUserInfo userinfo = new SHJUserInfo();
            userinfo.UserID = 10;
            userinfo.UserName = "程伟";
            userinfo.RealName = "程伟";
            userinfo.DepartName = "技术中心";
            userinfo.UserCode = "2018001515";
            userinfo.Mobile = "13918863121";
            userinfo.JobName = "技术中心软件开发";
            _cookieService.SetUserCookie(userinfo);

            return Redirect("/WindDoor/Main/Index");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult LogOut()
        {
            _cookieService.CookieCleanUp();
            return View();
        }
    }
}
