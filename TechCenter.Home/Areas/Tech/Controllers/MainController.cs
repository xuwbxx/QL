using DataFactory.KingBase;
using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Service.Base;
using Service.TechCenter;
using System.Web;
using Tool;

namespace TechCenter.Home.Areas.Tech.Controllers
{
    [Area("Tech")]
    public class MainController : Controller
    {

        private CookieService _cookieService { get; }

        private DataLoginService _dataLoginService { get; }

        public MainController(CookieService cookieService, DataLoginService dataLoginService)
        {
            _cookieService = cookieService;
            _dataLoginService = dataLoginService;
        }

        public async Task<IActionResult> Redirect()
        {

            try
            {
                string paramString = HttpContext.Request.Query["paramString"];

                //qlJn5XIutws5C4yQGCZad9wPHJyCXoT25pTSlFZbk74&state=STATE

                LoggerUtils.Log(LogLevel.Error, paramString, typeof(MainController));

                SHJUserInfo userinfo = new SHJUserInfo();
                if (string.IsNullOrEmpty(paramString))
                {
                    return RedirectToAction("Error", "Home", new { Area = "Tech", ErrorText = "没有任何跳转参数。" });
                }
                else
                {
                    string token = HttpUtility.UrlDecode(paramString);
                    //解密token
                    SingleSinOnModel ssoModel = new SingleSinOnModel();
                    string ssoResult = TC_JJTService.AESSingleSignOn(token, out ssoModel);
                    if (!string.IsNullOrEmpty(ssoResult))
                    {
                        //身份失效界面
                        return RedirectToAction("Error", "Home", new { Area = "Tech", ErrorText = "身份信息错误" });
                    }


                    //获取用户信息
                    var ShjUser = await TC_SHJ4AService.GetShjUserInfo(ssoModel.usrCode);
                    if (ShjUser.StatusCode != 200 || ShjUser.Data == null || ShjUser.Data.Count != 1)
                    {
                        //用户信息问题
                        return RedirectToAction("Error", "Home", new { Area = "Tech", ErrorText = "身份信息错误。" });
                    }
                    userinfo.UserID = ShjUser.Data[0].ID;
                    userinfo.UserName = ShjUser.Data[0].namespell;
                    userinfo.UserCode = ShjUser.Data[0].empcode;
                    userinfo.RealName = ShjUser.Data[0].name;
                    userinfo.Depart = ShjUser.Data[0].wholeDeptPath;
                    userinfo.Email = ShjUser.Data[0].email;
                    userinfo.Mobile = ShjUser.Data[0].phone;
                    userinfo.Birthday = ShjUser.Data[0].birthday;
                    userinfo.Job = ShjUser.Data[0].jobname;


                    //测试
                    //userinfo.UserID = 2018001515;
                    //userinfo.UserName = "chengwei";
                    //userinfo.UserCode = "2018001515";
                    //userinfo.RealName = "程伟";
                    //userinfo.Mobile = "13918863121";
                    //userinfo.Depart = "BIM中心";

                    //userinfo.UserID = 2020006389;  // = 2018001515;
                    //userinfo.UserName = "chengwei";
                    //userinfo.UserCode = "2020006389";
                    //userinfo.RealName = "陆小尤";
                    //userinfo.Mobile = "13918863121";
                    //userinfo.Depart = "BIM中心";


                }

                //保存cookie
                _cookieService.SetUserCookie(userinfo);

                //跳转数据记录
                TechCenter_DataLogin data = new TechCenter_DataLogin();
                data.UserCode = userinfo.UserCode;
                data.UserName = userinfo.RealName;
                data.Depart = userinfo.Depart;
                data.CreateTime = DateTime.Now;
                data.IsDelete = false;
                await _dataLoginService.DataLoginAdd(data);

                return Redirect("/Tech/Home/Page");

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(HomeController));
                return RedirectToAction("Error", "Home", new { Area = "Tech", ErrorText = "身份错误，请联系管理员。" });
            }

            return RedirectToAction("Error", "Home", new { Area = "Tech", ErrorText = "身份错误，请联系管理员。" });

        }
    }
}
