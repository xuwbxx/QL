using BIM.Business.CCSHJWebApi;
using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Service.Base;
using Service.Wind;
using System.Diagnostics;
using Tool;
using WindCloud.Models;

namespace WindCloud.Controllers
{
    public class HomeController : Controller
    {
        private CookieService _cookieService { get; }
        private CloudWindInfoService _cloudWindInfoService { get; }
        private LoginService _loginService { get; }
        private ProjectService _projectService { get; }

        public HomeController(CookieService cookieService, CloudWindInfoService cloudWindInfoService, LoginService loginService, ProjectService projectService)
        {
            _cookieService = cookieService;
            _cloudWindInfoService = cloudWindInfoService;
            _loginService = loginService;
            _projectService = projectService;
        }

        public IActionResult Index()
        {

            SHJUserInfo userinfo = new SHJUserInfo();
            userinfo.UserID = 10;
            userinfo.UserName = "程伟";
            userinfo.RealName = "程伟";
            userinfo.DepartName = "三航局-技术中心-BIM中心";
            userinfo.UserCode = "2018001515";
            userinfo.Mobile = "13918863121";
            userinfo.JobName = "技术管理";
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

        public IActionResult JJTMessageRedirect()
        {
            if (string.IsNullOrEmpty(HttpContext.Request.Query["CloudToken"]))
            {
                //身份失效界面
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "没有权限登录风电云服务" });
            }

            string token = HttpContext.Request.Query["CloudToken"].ToString().Trim();

            string TokenStr = CloudCenterService.DecryptToken(token);

            string[] TokenArr = TokenStr.Split('-');

            string Type = TokenArr[2];
            string UserID = TokenArr[1];
            string ID = TokenArr[3];
            var Time = Convert.ToDateTime(TokenArr[4]);

            if (string.IsNullOrEmpty(ID) || ID.Equals("0") || string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(UserID))
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "链接发生错误。" });
            }

            //获取用户信息
            var ShjUser = _cloudWindInfoService.GetShjUserInfo(UserID);
            if (ShjUser.StatusCode != 200 || ShjUser.Data == null || ShjUser.Data.Count != 1)
            {
                //用户信息问题
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "用户信息错误" });
            }

            string RedirectUrl = "";
            if (Type.Equals("Project"))
            {
                int ProjectID = Convert.ToInt32(ID);
                var WindProject = _projectService.getProjectByID(ProjectID);
                if (WindProject == null || WindProject.IsDelete)
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "链接发生错误。" });
                }
                DateTime limitTime = DateTime.Now.AddDays(7);
                if (DateTime.Now > limitTime)
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "此链接已经失效(有效期是7天)，请从交建通工作台“风电云服务”进入。" });
                }

                RedirectUrl = "CloudRedirect?Type=Project&ID=" + WindProject.ID;
            }
            else if (Type.Equals("Task"))
            {
                int TaskID = Convert.ToInt32(ID);
                var WindTask = _projectService.getTaskByID(TaskID);
                if (WindTask == null || WindTask.IsDelete)
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "链接发生错误。" });
                }
                var WindProject = _projectService.getProjectByID(WindTask.ProjectID);
                if (WindProject == null || WindProject.IsDelete)
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "链接发生错误。" });
                }
                DateTime limitTime = DateTime.Now.AddDays(7);
                if (DateTime.Now > limitTime)
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "此链接已经失效，请从交建通工作台“风电云服务”进入。" });
                }
                RedirectUrl = "CloudRedirect?Type=Task&ID=" + WindTask.ID;
            }
            else
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "链接错误" });
            }

            SHJUserInfo userinfo = new SHJUserInfo();
            userinfo.UserID = ShjUser.Data[0].ID;
            userinfo.UserName = ShjUser.Data[0].namespell;
            userinfo.UserCode = ShjUser.Data[0].empcode;
            userinfo.RealName = ShjUser.Data[0].name;
            userinfo.DepartName = ShjUser.Data[0].wholeDeptPath;
            userinfo.Email = ShjUser.Data[0].email;
            userinfo.Mobile = ShjUser.Data[0].phone;
            userinfo.Birthday = ShjUser.Data[0].birthday;

            //登录信息保存cookie
            _cookieService.SetUserCookie(userinfo);

            //保存登录信息
            _loginService.JJTLoginRecord(userinfo);

            return Redirect(RedirectUrl);
        }


        /// <summary>
        /// 起重船基础施工可作业性预报跳转
        /// </summary>
        public IActionResult JJTMessageKZYReportRedirect()
        {
            if (string.IsNullOrEmpty(HttpContext.Request.Query["CloudToken"]))
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "没有权限登录风电云服务" });
            }

            string token = HttpContext.Request.Query["CloudToken"].ToString().Trim();

            string TokenStr = CloudCenterService.DecryptToken(token);

            string[] TokenArr = TokenStr.Split('-');

            string UserID = TokenArr[1];
            string Type = TokenArr[2];
            string TaskCode = TokenArr[3];

            var Time = Convert.ToDateTime(TokenArr[4]);
            if (Time.AddDays(30) < DateTime.Now)
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "此链接已经失效，请从交建通工作台“风电云服务”进入。" });
            }

            if (string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(UserID) || string.IsNullOrEmpty(TaskCode))
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "没有权限登录风电云服务" });
            }

            //获取用户信息
            var ShjUser = _cloudWindInfoService.GetShjUserInfo(UserID);
            if (ShjUser.StatusCode != 200 || ShjUser.Data == null || ShjUser.Data.Count != 1)
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "用户信息错误" });
            }

            SHJUserInfo userinfo = new SHJUserInfo();
            userinfo.UserID = ShjUser.Data[0].ID;
            userinfo.UserName = ShjUser.Data[0].namespell;
            userinfo.UserCode = ShjUser.Data[0].empcode;
            userinfo.RealName = ShjUser.Data[0].name;
            userinfo.DepartName = ShjUser.Data[0].wholeDeptPath;
            userinfo.Email = ShjUser.Data[0].email;
            userinfo.Mobile = ShjUser.Data[0].phone;
            userinfo.Birthday = ShjUser.Data[0].birthday;

            //登录信息保存cookie
            _cookieService.SetUserCookie(userinfo);

            //保存登录信息
            _loginService.JJTLoginRecord(userinfo);

            if (Type.Equals("KZY"))
            {
                return RedirectToAction("KZYRedirect", new { TaskCode = TaskCode, Type = "KZY" });
            }
            else
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "发生错误，请从交建通登录风电云服务平台获取文件报告。" });
            }
        }

        public IActionResult JJTMessageRedirect__PlatRegister()
        {
            if (string.IsNullOrEmpty(HttpContext.Request.Query["CloudToken"]))
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "没有权限登录风电云服务" });
            }

            string token = HttpContext.Request.Query["CloudToken"].ToString().Trim();

            string TokenStr = CloudCenterService.DecryptToken(token);

            string[] TokenArr = TokenStr.Split('-');

            string UserCode = TokenArr[1];
            string Plat = TokenArr[2];

            var Time = Convert.ToDateTime(TokenArr[3]);
            if (Time.AddDays(30) < DateTime.Now)
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "此链接已经失效，请从交建通工作台“风电云服务”进入“后台管理”。" });
            }

            if (string.IsNullOrEmpty(UserCode) || string.IsNullOrEmpty(Plat))
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "发生错误，请联系开发人员" });
            }

            //获取用户信息
            var ShjUser = _cloudWindInfoService.GetShjUserInfo(UserCode);
            if (ShjUser.StatusCode != 200 || ShjUser.Data == null || ShjUser.Data.Count != 1)
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "用户信息错误" });
            }

            SHJUserInfo userinfo = new SHJUserInfo();
            userinfo.UserID = ShjUser.Data[0].ID;
            userinfo.UserName = ShjUser.Data[0].namespell;
            userinfo.UserCode = ShjUser.Data[0].empcode;
            userinfo.RealName = ShjUser.Data[0].name;
            userinfo.DepartName = ShjUser.Data[0].wholeDeptPath;
            userinfo.Email = ShjUser.Data[0].email;
            userinfo.Mobile = ShjUser.Data[0].phone;
            userinfo.Birthday = ShjUser.Data[0].birthday;

            //登录信息保存cookie
            _cookieService.SetUserCookie(userinfo);

            //保存登录信息
            _loginService.JJTLoginRecord(userinfo);

            if (Plat.Equals("Towing"))
            {
                return Redirect("/BackManage/Towing/Index");
            }
            else
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "发生错误，请联系开发人员。" });
            }
        }

        public IActionResult JJTMessageRedirect__Common()
        {
            if (string.IsNullOrEmpty(HttpContext.Request.Query["CloudToken"]))
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "没有权限登录风电云服务" });
            }

            string token = HttpContext.Request.Query["CloudToken"].ToString().Trim();

            string TokenStr = CloudCenterService.DecryptToken(token);

            string[] TokenArr = TokenStr.Split('-');

            string UserCode = TokenArr[1];
            string Url = TokenArr[2];

            var Time = Convert.ToDateTime(TokenArr[3]);
            if (Time.AddDays(30) < DateTime.Now)
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "此链接已经失效，请从交建通工作台“风电云服务”进入“后台管理”。" });
            }

            if (string.IsNullOrEmpty(UserCode) || string.IsNullOrEmpty(Url))
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "发生错误，请联系开发人员" });
            }

            //获取用户信息
            var ShjUser = _cloudWindInfoService.GetShjUserInfo(UserCode);
            if (ShjUser.StatusCode != 200 || ShjUser.Data == null || ShjUser.Data.Count != 1)
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { area = "Cloud", ErrorText = "用户信息错误" });
            }

            SHJUserInfo userinfo = new SHJUserInfo();
            userinfo.UserID = ShjUser.Data[0].ID;
            userinfo.UserName = ShjUser.Data[0].namespell;
            userinfo.UserCode = ShjUser.Data[0].empcode;
            userinfo.RealName = ShjUser.Data[0].name;
            userinfo.DepartName = ShjUser.Data[0].wholeDeptPath;
            userinfo.Email = ShjUser.Data[0].email;
            userinfo.Mobile = ShjUser.Data[0].phone;
            userinfo.Birthday = ShjUser.Data[0].birthday;

            //登录信息保存cookie
            _cookieService.SetUserCookie(userinfo);

            //保存登录信息
            _loginService.JJTLoginRecord(userinfo);

            return Redirect(Url);
        }

        public IActionResult TechCenterRedirect()
        {
            string Token = HttpContext.Request.Query["Token"].ToString();
            if (string.IsNullOrEmpty(Token))
            {
                return Redirect("/CCSHJ/Center/Error?ErrorText=身份信息错误");
            }

            TechCloudApiRequest request = new TechCloudApiRequest();
            request.Token = Token.Replace(" ", "+");

            var PostUrl = @"http://10.6.48.20:8088/CCSHJ/DataPort/VerifyToken";

            var JsonStr = HttpUtils.PostSync(PostUrl, request);

            var JsonModel = JsonUtils.Deserialize<BaseApiReturn<SSOUserInfo>>(JsonStr);

            if (JsonModel == null || JsonModel.Data == null || !JsonModel.Success || string.IsNullOrEmpty(JsonModel.Data.UserCode))
            {
                return Redirect("/CCSHJ/Center/Error?ErrorText=身份信息错误");
            }

            //获取用户信息
            var ShjUser = _cloudWindInfoService.GetShjUserInfo(JsonModel.Data.UserCode);
            if (ShjUser.StatusCode != 200 || ShjUser.Data == null || ShjUser.Data.Count != 1)
            {
                return Redirect("/CCSHJ/Center/Error?ErrorText=身份信息错误");
            }

            SHJUserInfo userinfo = new SHJUserInfo();
            userinfo.UserID = ShjUser.Data[0].ID;
            userinfo.UserName = ShjUser.Data[0].namespell;
            userinfo.UserCode = ShjUser.Data[0].empcode;
            userinfo.RealName = ShjUser.Data[0].name;
            userinfo.DepartName = ShjUser.Data[0].wholeDeptPath;
            userinfo.Email = ShjUser.Data[0].email;
            userinfo.Mobile = ShjUser.Data[0].phone;
            userinfo.Birthday = ShjUser.Data[0].birthday;
            userinfo.JobName = ShjUser.Data[0].jobname;

            //登录信息保存cookie
            _cookieService.SetUserCookie(userinfo);

            //保存登录信息
            _loginService.JJTLoginRecord(userinfo);

            return Redirect("/WindDoor/Main/Index");
        }

        public ActionResult ErrorPage(string ErrorText)
        {

            if (string.IsNullOrEmpty(ErrorText))
            {
                ErrorText = "您没有没有权限使用此功能";
            }
            ViewData["ErrorText"] = "出错：" + ErrorText ?? "未知";

            return View();


        }

        public ActionResult KZYRedirect()
        {
            string TaskCode = HttpContext.Request.Query["TaskCode"].ToString();
            string Type = HttpContext.Request.Query["Type"].ToString();
            ViewData["TaskCode"] = TaskCode;
            ViewData["Type"] = Type;
            return View();
        }

        public ActionResult CloudRedirect()
        {
            string Type = HttpContext.Request.Query["Type"].ToString();
            ViewData["Type"] = Type;

            string ID = HttpContext.Request.Query["ID"].ToString();
            ViewData["ID"] = ID;

            return View();
        }

    }
}
