using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.TechCenter.Monitor;
using Service.Base;
using Service.TechCenter;
using Tool;

namespace TechCenter.Home.Areas.Tech.Controllers
{
    [Area("Tech")]
    public class MonitorController : Controller
    {

        private readonly TC_MonitorService _monitorService;

        private CookieService _cookieService { get; }

        public MonitorController(TC_MonitorService monitorService, CookieService cookieService)
        {
            _monitorService = monitorService;
            _cookieService = cookieService;
        }

        public IActionResult Index()
        {


            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetProjectInfo([FromForm] MonitorRequest request)
        {
            BaseReturn ret = new BaseReturn();
            try
            {
                ProjectInfoResponse data = new ProjectInfoResponse();

                data = _monitorService.GetMonitorProject(request);

                ret.Data = data;

                ret.Success = true;

            }
            catch (Exception ex)
            {
                LoggerUtils.Log(LogLevel.Error, ex.ToString(), typeof(MonitorController));
            }

            return Ok(ret);

        }


        public IActionResult Index2()
        {

            return View("Index");
        }

        public IActionResult Index3()
        {
            return Content("我进来了！"); // 不加载视图，直接返回文字
        }


        [Route("tech/monitor/kuLunKanBan")]
        public IActionResult KuLunKanBan(int pid)
        {

            SHJUserInfo userinfo = new SHJUserInfo();
            userinfo.UserID = 2020006389;  // = 2018001515;
            userinfo.UserName = "LuXiaoYou";
            userinfo.UserCode = "2020006389";
            userinfo.RealName = "陆骁尤";
            userinfo.Mobile = "19821262905";
            userinfo.Depart = "岩土所";

            _cookieService.SetUserCookie(userinfo);

            // 1. int不能判null，只需要判断0
            if (pid == 0)
            {
                // 绝对不能return null！要返回错误页或重定向
                return RedirectToAction("Index");
            }

            ViewData["pid"] = pid;

            // 2. 安全返回视图（一定要确保这个视图文件存在）
            return View("Index" + pid); // 注意大小写 Index
        }

        [HttpPost]
        public async Task<IActionResult> GetProjectInfo_KuLun([FromForm] MonitorRequest request)
        {
            BaseReturn ret = new BaseReturn();
            try
            {
                ProjectInfoResponse data = new ProjectInfoResponse();

                data = await _monitorService.GetMonitorProject_KuLun(request.ID);

                ret.Data = data;

                ret.Success = true;

            }
            catch (Exception ex)
            {
                LoggerUtils.Log(LogLevel.Error, ex.ToString(), typeof(MonitorController));
            }

            return Ok(ret);

        }

    }
}
