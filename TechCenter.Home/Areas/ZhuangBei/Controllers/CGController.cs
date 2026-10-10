using Microsoft.AspNetCore.Mvc;

namespace TechCenter.Home.Areas.ZhuangBei.Controllers
{
    [Area("ZhuangBei")]
    public class CGController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 可行性分析软件V1.0
        /// </summary>
        /// <returns></returns>
        public IActionResult FXRJV1()
        {
            return View();
        }

    }
}
