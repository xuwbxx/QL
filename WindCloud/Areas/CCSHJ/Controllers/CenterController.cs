using DataFactory.KingBase.CloudWind;
using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.TechCenter.JJT;
using Service.Base;
using Service.Base.Filter;
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
        private ProjectService _projectService { get; }
        private LoginService _loginService { get; }

        public CenterController(CookieService cookieService, ManageLoginRecordService manageLoginRecordService, CloudWindInfoService cloudWindInfoService, ProjectService projectService, LoginService loginService)
        {
            _cookieService = cookieService;
            _manageLoginRecordService = manageLoginRecordService;
            _cloudWindInfoService = cloudWindInfoService;
            _projectService = projectService;
            _loginService = loginService;
        }


        [TypeFilter(typeof(WindCloudFilter))]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 判断登录设备
        /// </summary>
        /// <returns></returns>
        [TypeFilter(typeof(WindCloudFilter))]
        public ActionResult Base()
        {
            string paramString = HttpContext.Request.Query["paramString"];

            if (string.IsNullOrEmpty(paramString))
            {
                return RedirectToAction("Error", "Center", new { area = "CCSHJ", ErrorText = "身份信息错误" });
            }

            ViewData["Token"] = paramString;

            return View();
        }

        public ActionResult Error(string ErrorText)
        {
            if (string.IsNullOrEmpty(ErrorText))
            {
                ErrorText = "您没有权限使用此功能";
            }
            ViewData["ErrorText"] = "出错：" + ErrorText ?? "未知";
            return View();
        }

        /// <summary>
        /// 账号密码登录
        /// </summary>
        /// <returns></returns>
        public ActionResult Login(string Text)
        {
            ViewData["Text"] = Text ?? "";
            return View();
        }

        [HttpGet]
        public ActionResult DoLogin(LoginInfo info)
        {
            string ErrorText = "";
            try
            {
                if (string.IsNullOrEmpty(info.UserName) || string.IsNullOrEmpty(info.PassWord))
                {
                    ErrorText = "用户名或密码错误";
                    return RedirectToAction("Login", new { Text = ErrorText });
                }
                string msg = "";
                var Users = _projectService.FindShjUser(info.UserName, out msg);
                if (string.IsNullOrEmpty(msg))
                {
                    if (Users == null || Users.Count == 0)
                    {
                        ErrorText = "没有 " + info.UserName;
                        return RedirectToAction("Login", new { Text = ErrorText });
                    }

                    string dataStr = DateTime.Now.ToString("MMddHH");

                    bool Success = false;
                    foreach (var item in Users)
                    {
                        string WholePassWord = TxtUtils.GetChineseSpell(item.name) + "_" + item.certno.Substring(item.certno.Length - 4) + "_" + dataStr;
                        if (info.PassWord.Equals(WholePassWord))
                        {
                            SHJUserInfo userinfo = new SHJUserInfo();
                            userinfo.UserID = item.ID;
                            userinfo.UserName = item.namespell;
                            userinfo.UserCode = item.empcode;
                            userinfo.RealName = item.name;
                            userinfo.DepartName = item.wholeDeptPath;
                            userinfo.Email = item.email;
                            userinfo.Mobile = item.phone;
                            userinfo.Birthday = item.birthday;
                            userinfo.JobName = item.jobname;
                            Success = true;

                            //登录信息保存cookie
                            _cookieService.SetUserCookie(userinfo);

                            //保存登录信息
                            Manage_LoginRecord se = new Manage_LoginRecord();
                            se.UserCode = userinfo.UserCode;
                            se.Name = userinfo.RealName;
                            se.Depart = userinfo.DepartName;
                            se.LoginTime = DateTime.Now;
                            se.CreateTime = DateTime.Now;
                            se.IsDelete = false;
                            _loginService.AddManageLoginRecord(se);

                            break;
                        }
                    }
                    if (Success)
                    {
                        return RedirectToAction("LoginJudge");
                    }
                    else
                    {
                        ErrorText = "用户名或密码错误";
                        return RedirectToAction("Login", new { Text = ErrorText });
                    }

                }
                else
                {
                    ErrorText = "系统错误或查无此人";
                    return RedirectToAction("Login", new { Text = ErrorText });
                }


            }
            catch (Exception ex)
            {
                ErrorText = "发生错误";
                LoggerUtils.Error(ex.ToString(), typeof(CenterController));
                return RedirectToAction("Login", new { Text = ErrorText });
            }
        }

        [TypeFilter(typeof(WindCloudFilter))]
        public ActionResult LoginJudge()
        {
            return View();
        }

        public ActionResult Redirect()
        {
            try
            {
                var paramString = HttpContext.Request.Query["paramString"];

                SHJUserInfo userinfo = new SHJUserInfo();
                //用来测试
                if (!string.IsNullOrEmpty(paramString))
                {
                    string token = HttpUtility.UrlDecode(paramString);
                    LoggerUtils.Info(token, typeof(CenterController));

                    //解密token
                    SingleSinOnModel ssoModel = new SingleSinOnModel();
                    string ssoResult = _cloudWindInfoService.AESSingleSignOn(token, out ssoModel);
                    if (!string.IsNullOrEmpty(ssoResult))
                    {
                        //身份失效界面
                        return RedirectToAction("Error", "Center", new { area = "CCSHJ", ErrorText = "身份信息错误" });
                    }
                    //获取用户信息
                    var ShjUser = _cloudWindInfoService.GetShjUserInfo(ssoModel.usrCode);
                    if (ShjUser.StatusCode != 200 || ShjUser.Data == null || ShjUser.Data.Count != 1)
                    {
                        //用户信息问题
                        return RedirectToAction("Error", "Center", new { area = "CCSHJ", ErrorText = "身份信息错误" });
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
                    return RedirectToAction("Error", "Center", new { area = "CCSHJ", ErrorText = "信息错误" });
                }

                //登录信息保存cookie
                _cookieService.SetUserCookie(userinfo);

                //保存登录信息
                Manage_LoginRecord se = new Manage_LoginRecord();
                se.UserCode = userinfo.UserCode;
                se.Name = userinfo.RealName;
                se.Depart = userinfo.DepartName;
                se.LoginTime = DateTime.Now;
                se.CreateTime = DateTime.Now;
                se.IsDelete = false;

                //跳转至相应的页面（电脑、手机、pad）
                var EquipmentNo = HttpContext.Request.Query["eq"];
                int EID = Convert.ToInt32(EquipmentNo);
                if (EID == 1)
                {
                    //电脑
                    se.LoginType = "PC";
                    _manageLoginRecordService.add(se);
                    return RedirectToAction("Main", "TechCenter", new { area = "CCSHJ" });
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
                    return RedirectToAction("Error", "Center", new { area = "CCSHJ", ErrorText = "无法判断用户设备" });
                }



            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CenterController));
            }

            return RedirectToAction("Error", "Center", new { area = "CCSHJ", ErrorText = "信息错误" });

        }
    }
}
