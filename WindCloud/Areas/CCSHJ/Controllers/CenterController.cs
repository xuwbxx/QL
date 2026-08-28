using DataFactory.KingBase.CloudWind;
using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Service.Base;
using Service.Shj;
using Service.Wind;
using System.Web;
using Tool;

namespace WindCloud.Areas.CCSHJ.Controllers
{
    [Area("CCSHJ")]
    public class CenterController : Controller
    {

        private CookieService _cookieService { get; }
        private ManageLoginRecordService _manageLoginRecordService { get; }
        private CloudWindInfoService _cloudWindInfoService { get; }

        public CenterController(CookieService cookieService, ManageLoginRecordService manageLoginRecordService, CloudWindInfoService cloudWindInfoService)
        {
            _cookieService = cookieService;
            _manageLoginRecordService = manageLoginRecordService;
            _cloudWindInfoService = cloudWindInfoService;
        }


        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 判断登录设备
        /// </summary>
        /// <returns></returns>
        public ActionResult Base()
        {
            string paramString = HttpContext.Request.Query["paramString"];

            if (string.IsNullOrEmpty(paramString))
            {
                return RedirectToAction("/CCSHJ/Center/Error", new { ErrorText = "身份信息错误" });
            }

            ViewData["Token"] = paramString;

            return View();
        }

        public ActionResult Redirect()
        {
            try
            {
                var paramString = HttpContext.Request.Query["paramString"];

                SHJUserInfo userinfo = new SHJUserInfo();
                //用来测试
                var UserID = HttpContext.Request.Query["UserID"];
                if (!string.IsNullOrEmpty(UserID))
                {
                    int UID = Convert.ToInt32(UserID);
                    if (UID == 10)
                    {
                        userinfo.UserID = 10;
                        userinfo.UserName = "程伟";
                        userinfo.RealName = "程伟";
                        userinfo.DepartName = "技术中心";
                        userinfo.UserCode = "2018001515";
                        userinfo.Mobile = "13918863121";
                        userinfo.JobName = "技术中心软件开发";
                    }

                }
                else if (!string.IsNullOrEmpty(paramString))
                {
                    string token = HttpUtility.UrlDecode(paramString);
                    LoggerUtils.Info(token, typeof(CenterController));

                    //解密token
                    SingleSinOnModel ssoModel = new SingleSinOnModel();
                    string ssoResult = _cloudWindInfoService.AESSingleSignOn(token, out ssoModel);
                    if (!string.IsNullOrEmpty(ssoResult))
                    {
                        //身份失效界面
                        return RedirectToAction("/CCSHJ/Center/Error", new { ErrorText = "身份信息错误" });
                    }
                    //获取用户信息
                    var ShjUser = _cloudWindInfoService.GetShjUserInfo(ssoModel.usrCode);
                    if (ShjUser.StatusCode != 200 || ShjUser.Data == null || ShjUser.Data.Count != 1)
                    {
                        //用户信息问题
                        return RedirectToAction("/CCSHJ/Center/Error", new { ErrorText = "身份信息错误" });
                    }

                    userinfo.UserID = ShjUser.Data[0].ID;
                    userinfo.UserName = ShjUser.Data[0].namespell;
                    userinfo.UserCode = ShjUser.Data[0].empcode;
                    userinfo.RealName = ShjUser.Data[0].name;
                    userinfo.DepartName = ShjUser.Data[0].wholeDeptPath;
                    userinfo.Email = ShjUser.Data[0].email;
                    userinfo.Mobile = ShjUser.Data[0].phone;
                    userinfo.Birthday = ShjUser.Data[0].birthday;
                    userinfo.JobName = ShjUser.Data[0].jobname;

                }
                else
                {
                    return RedirectToAction("/CCSHJ/Center/Error", new { ErrorText = "信息错误" });
                }

                //登录信息保存cookie
                _cookieService.SetUserCookie(userinfo);

                //保存登录信息
                Manage_LoginRecord se = new Manage_LoginRecord();
                se.UserCode = userinfo.UserCode;
                se.Name = userinfo.RealName;
                se.Depart = userinfo.DepartName;
                se.LoginTime = DateTime.UtcNow;
                se.CreateTime = DateTime.UtcNow;
                se.IsDelete = false;

                //跳转至相应的页面（电脑、手机、pad）
                var EquipmentNo = HttpContext.Request.Query["eq"];
                int EID = Convert.ToInt32(EquipmentNo);
                if (EID == 1)
                {
                    //电脑
                    se.LoginType = "PC";
                    _manageLoginRecordService.add(se);
                    return RedirectToAction("/CCSHJ/TechCenter/Main");
                }
                else if (EID == 2)
                {
                    //手机
                    se.LoginType = "Mobile";
                    _manageLoginRecordService.add(se);
                }
                else if (EID == 3)
                {
                    //PAD
                    se.LoginType = "Pad";
                    _manageLoginRecordService.add(se);
                }
                else
                {
                    return RedirectToAction("/CCSHJ/Center/Error", new { ErrorText = "无法判断用户设备" });
                }



            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CenterController));
            }

            return RedirectToAction("/CCSHJ/Center/Error", new { ErrorText = "信息错误" });

        }
    }
}
