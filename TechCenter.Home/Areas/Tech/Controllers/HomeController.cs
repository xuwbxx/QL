using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.TechCenter;
using Service.Base;
using Service.TechCenter;
using System.Web;
using Tool;

namespace TechCenter.Home.Areas.Tech.Controllers
{
    [Area("Tech")]
    public class HomeController : BaseController
    {

        private DataLoginService _dataLoginService { get; }

        private TC_DepartService _tC_DepartService { get; }

        private TC_SoftwareService _tC_SoftwareService { get; }

        private SSOService _sSOService { get; }

        public HomeController(DataLoginService dataLoginService, CookieService cookieService, TC_DepartService tC_DepartService, TC_SoftwareService tC_SoftwareService, SSOService sSOService)
            : base(cookieService)
        {
            _dataLoginService = dataLoginService;
            _tC_DepartService = tC_DepartService;
            _tC_SoftwareService = tC_SoftwareService;
            _sSOService = sSOService;
        }

        public IActionResult Index()
        {
            SHJUserInfo userinfo = new SHJUserInfo();
            userinfo.UserID = 2018001515;  // = 2018001515;
            userinfo.UserName = "chengwei";
            userinfo.UserCode = "2018001515";
            userinfo.RealName = "程伟";
            userinfo.Mobile = "13918863121";
            userinfo.Depart = "BIM中心";

            _cookieService.SetUserCookie(userinfo);

            return Redirect("Page");

            return View();
        }

        public IActionResult Error(string ErrorText)
        {
            if (string.IsNullOrEmpty(ErrorText))
            {
                ErrorText = "您没有没有权限使用此功能";
            }
            ViewData["ErrorText"] = "出错：" + ErrorText ?? "未知";

            return View();
        }

        [TypeFilter(typeof(TcFilter))]
        public IActionResult Page()
        {
            var user = CurrentUser;

            ViewData["RealName"] = user.RealName;
            ViewData["Depart"] = user.Depart;

            List<TC_DepartInfo> departs = _tC_DepartService.GetDepartInfo();

            return View(departs);
        }

        public async Task<IActionResult> PlatRedirect(int SoftwareID)
        {
            //dilnamXTCvHhPLDqeZN4z/yhR8vVuMQV8UCIhWiuq0d8IggcEF0tU/pIzoizBiPia/cR8+tqUez/2Tzol0BxzuL6ibKdW1k9Aax54JcXk6WguyWpDoXLh3Cv3AzSyVKU3nrjNAK0QImWOFuYw1pwEtH9QZ8T+9tMBPpTp0giTEG3sT2TWAGCmwfaPlfOg3DHI8ssSBa6O/EfmeXkPMCmWOZT+4DvFyfuw27wOduZ4LdoKDqpo6nXVlhL/kUCEs7vw6OlDXS/0jO3IV8QB+WM+VlRLk35wfb8U1Q9g4iyLtsYHYaEBnSLAc126HwA1rTlIs+IbRzQS59r/fOjqDgsPN9hgzxwcUIdTO3u2RQgqqjn+1q2PEvFkodsnsYe6xQ8A9I73BnbkTkKlwHq2RCAy7fFieylPHDpwl/KtWpLUoHm+hPg+rapB0Yd1N9fJrugpBlpOeZ102AiON0ZhtfslmNW06SbbTqK4nh6sEUq6Ra5kvtpN1vqaUGv9wCgF66I


            if (SoftwareID == 0)
            {
                return RedirectToAction("Error", "Home", new { Area = "Tech", ErrorText = "平台跳转发生错误，请联系管理员。" });
            }

            var software = _tC_SoftwareService.GetSoftwareInfo(SoftwareID);

            if (software == null || string.IsNullOrEmpty(software.Url))
            {
                return RedirectToAction("Error", "Home", new { Area = "Tech", ErrorText = "软件不存在或者配置错误，请联系管理员。" });
            }

            if (CurrentUser == null)
            {
                return RedirectToAction("Error", "Home", new { Area = "Tech", ErrorText = "用户信息失效，请重新登录" });
            }

            if (software.Type == 1)
            {
                return Redirect(software.Url);
            }

            SHJUserInfo user = CurrentUser;
            user.SoftwareID = SoftwareID;

            string url = "";
            if (SoftwareID == 12)
            {
                //科研管理平台

                string code = Guid.NewGuid() + "_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "_" + user.UserCode + "_" + Guid.NewGuid();

                string EncryptKey = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:Key");
                string EncryptIV = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:IV");
                string token = CryptographyUtils.TripleDESEncrypt(code, EncryptKey, EncryptIV);
                string urlToken = HttpUtility.UrlEncode(token);

                url = software.Url + @"?token=" + urlToken + "&redirect=/shj";


            }
            else
            {
                var token = await _sSOService.CreateSSOToken(user);
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Error", "Home", new { ErrorText = "发生了错误，请联系管理员" });
                }

                url = software.Url + "?Token=" + HttpUtility.UrlEncode(token);
            }



            return Redirect(url);
        }
    }
}
