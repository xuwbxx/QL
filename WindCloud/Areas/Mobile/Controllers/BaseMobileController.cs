using Microsoft.AspNetCore.Mvc;

namespace WindCloud.Areas.Mobile
{
    public class BaseMobileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error(string ErrorText)
        {
            if (string.IsNullOrEmpty(ErrorText))
            {
                ErrorText = "您没有没有权限使用此功能";
            }
            ViewData["ErrorText"] = "出错：" + (ErrorText ?? "未知");
            return View();
        }
    }
}
