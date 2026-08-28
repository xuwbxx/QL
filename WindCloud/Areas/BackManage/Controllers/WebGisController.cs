using Microsoft.AspNetCore.Mvc;
using Service.Base;
using Service.Wind.BackManage;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.BackManage.Controllers
{
    [Area("BackManage")]
    public class WebGisController : WebEncryptionController
    {
        public WebGisController(CookieService cookieService, Service.Wind.WebValidateService webValidateService)
            : base(cookieService, webValidateService)
        {

        }

        public async Task<IActionResult> Index()
        {
            ViewData["PostToken"] = CreateWebToken();
            return View();
        }

        public async Task<IActionResult> Ship()
        {
            ViewData["PostToken"] = CreateWebToken();
            return View();
        }

    }
}
