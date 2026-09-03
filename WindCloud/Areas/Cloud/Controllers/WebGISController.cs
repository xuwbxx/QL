using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.Cloud;
using Model.Tech.Cloud.Webgis;
using Service.Base;
using Service.Wind;
using Tool;

namespace WindCloud.Areas.Cloud.Controllers
{
    [Area("Cloud")]
    public class WebGISController : ProjectTaskBaseController
    {
        public CloudWindInfoService _cloudWindInfoService;

        public WebGISService _webGISService;

        public WebGISController(CookieService cookieService, ProjectService projectService, CloudWindInfoService cloudWindInfoService, WebGISService webGISService)
            : base(cookieService, projectService)
        {
            _cloudWindInfoService = cloudWindInfoService;
            _webGISService = webGISService;
        }

        // GET: Cloud/WebGIS
        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(Request.Query["ProjectID"]))
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "项目信息错误" });
            }

            int ProjectID = Convert.ToInt32(Request.Query["ProjectID"]);

            var Project = _webGISService.GetProject(ProjectID);
            if (Project == null)
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "项目信息错误" });
            }

            ViewData["ProjectID"] = Project.ID;
            ViewData["ProjectLon"] = Project.Lon;
            ViewData["ProjectLat"] = Project.Lat;
            ViewData["ProjectName"] = Project.ProjectName;
            ViewData["PXYKey"] = AppSettingUtils.GetSetting("PXY:PXYKey");
            ViewData["PostToken"] = CreateWebToken(CurrentUser.UserCode);

            return View();
        }

        [HttpPost]
        public IActionResult GetProjectInfo([FromForm] GisRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ProjectID == 0)
            {
                return Ok(ret);
            }
            try
            {
                var (data, msg) = _webGISService.GetProjectInfo(request);
                if (string.IsNullOrEmpty(msg))
                {
                    ret.Data = data;
                    ret.Success = true;
                }
                else
                {
                    ret.Data = data;
                    ret.Message = msg;
                    ret.Success = false;
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(WebGISController));
            }
            return Ok(ret);
        }

        public IActionResult ShowPosition(GisRequest request)
        {
            return View();
        }

        public IActionResult ShowPoint(GisRequest request)
        {
            var paramString = Request.Query["Points"];
            if (string.IsNullOrEmpty(paramString))
            {
                return Content("");
            }

            ViewData["Points"] = paramString.ToString().Trim();
            ViewData["PXYKey"] = AppSettingUtils.GetSetting("PXY:PXYKey");

            return View();
        }

        [HttpPost]
        public IActionResult GetShips([FromForm] GisRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            try
            {
                var (data, msg) = _webGISService.GetShips();
                if (string.IsNullOrEmpty(msg))
                {
                    ret.Success = true;
                    ret.Data = data;
                }
                else
                {
                    ret.Success = false;
                    ret.Message = msg;
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = "出错了";
                LoggerUtils.Error(ex.ToString(), typeof(WebGISController));
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult GetPoints([FromForm] GisRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            try
            {
                List<PXYShipInfo> list = new List<PXYShipInfo>();
                list.Add(new PXYShipInfo() { ID = 11, ShipName = "1号", Lon = "108.81963620628815", Lat = "19.178486788710273", LonValue = 108.81963620628815, LatValue = 19.178486788710273 });
                list.Add(new PXYShipInfo() { ID = 12, ShipName = "2号", Lon = "108.81963620628815", Lat = "20.095477063310113", LonValue = 108.81963620628815, LatValue = 20.095477063310113 });
                list.Add(new PXYShipInfo() { ID = 13, ShipName = "3号", Lon = "109.1059274554269", Lat = "20.095477063310113", LonValue = 109.1059274554269, LatValue = 20.095477063310113 });
                list.Add(new PXYShipInfo() { ID = 14, ShipName = "4号", Lon = "109.1059274554269", Lat = "19.178486788710273", LonValue = 109.1059274554269, LatValue = 19.178486788710273 });
                list.Add(new PXYShipInfo() { ID = 15, ShipName = "5号", Lon = "108.81963620628815", Lat = "19.178486788710273", LonValue = 108.81963620628815, LatValue = 19.178486788710273 });
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = "出错了";
                LoggerUtils.Error(ex.ToString(), typeof(WebGISController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 天气预报
        /// </summary>
        [HttpPost]
        public IActionResult GetPreWeatherInfo([FromForm] GisRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                return Ok(ret);
            }
            try
            {
                var (data, msg) = _webGISService.GetPreWeatherInfo(request.ProjectID);
                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(WebGISController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 查询船舶信息
        /// </summary>
        [HttpPost]
        public IActionResult GetShip([FromForm] GisRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ShipID == 0)
            {
                return Ok(ret);
            }
            try
            {
                var data = _webGISService.GetShip(request.ShipID);
                if (data == null)
                {
                    return Ok(ret);
                }
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(WebGISController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 获取船轨迹信息
        /// </summary>
        [HttpPost]
        public IActionResult GetShipTrack([FromForm] GisRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ShipID == 0)
            {
                return Ok(ret);
            }
            try
            {
                var data = _webGISService.GetShipTrackData(request.ShipID, request.StartTime, request.EndTime);
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(WebGISController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 获取项目部锚点
        /// </summary>
        [HttpPost]
        public IActionResult GetShipAnchor([FromForm] GisRequest request)
        {
            BaseReturn ret = new BaseReturn();
            try
            {
                var data = _webGISService.GetShipAnchor();
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(WebGISController));
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult GetDataResource([FromForm] GisRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            try
            {
                var (data, msg) = _webGISService.GetDataResource(request.ProjectID);
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(WebGISController));
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult GetReport([FromForm] GisRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            try
            {
                var (data, msg) = _webGISService.GetReport(request.ProjectID);
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(WebGISController));
            }
            return Ok(ret);
        }
    }
}
