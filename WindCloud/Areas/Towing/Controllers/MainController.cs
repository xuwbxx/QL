using Business.Towing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Service.Base;
using Service.Base.Filter;
using Service.Towing;
using Service.Wind;
using Tool;
using Tool.Towing;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.Towing.Controllers
{
    [Area("Towing")]
    public class MainController : BaseController
    {
        private readonly MainService _mainService;
        private readonly MRUStatusInfoService _mruStatusInfoService;
        private readonly SatellteInfoService _satellteInfoService;
        private readonly NavigationalStatusService _navigationalStatusService;
        private readonly EGYROInfoService _egyroInfoService;
        private readonly WeatherInfoService _weatherInfoService;
        private readonly DeviceStatusInfoService _deviceStatusInfoService;
        private readonly ProjectPlanRouteInfoService _projectPlanRouteInfoService;
        private readonly MessageInfoService _messageInfoService;
        private readonly HikInfoService _hikInfoService;
        private readonly FileInfoService _fileInfoService;
        private readonly ShipInfoService _shipInfoService;
        private readonly AnchorageInfoService _anchorageInfoService;
        private readonly ProjectBasicInfoService _projectBasicInfoService;
        private readonly CloudWindTyphoonService _cloudWindTyphoonService;
        private readonly IWebHostEnvironment _env;

        public MainController(
            CookieService cookieService,
            MainService mainService,
            MRUStatusInfoService mruStatusInfoService,
            SatellteInfoService satellteInfoService,
            NavigationalStatusService navigationalStatusService,
            EGYROInfoService egyroInfoService,
            WeatherInfoService weatherInfoService,
            DeviceStatusInfoService deviceStatusInfoService,
            ProjectPlanRouteInfoService projectPlanRouteInfoService,
            MessageInfoService messageInfoService,
            HikInfoService hikInfoService,
            FileInfoService fileInfoService,
            ShipInfoService shipInfoService,
            AnchorageInfoService anchorageInfoService,
            ProjectBasicInfoService projectBasicInfoService,
            CloudWindTyphoonService cloudWindTyphoonService,
            IWebHostEnvironment env)
            : base(cookieService)
        {
            _mainService = mainService;
            _mruStatusInfoService = mruStatusInfoService;
            _satellteInfoService = satellteInfoService;
            _navigationalStatusService = navigationalStatusService;
            _egyroInfoService = egyroInfoService;
            _weatherInfoService = weatherInfoService;
            _deviceStatusInfoService = deviceStatusInfoService;
            _projectPlanRouteInfoService = projectPlanRouteInfoService;
            _messageInfoService = messageInfoService;
            _hikInfoService = hikInfoService;
            _fileInfoService = fileInfoService;
            _shipInfoService = shipInfoService;
            _anchorageInfoService = anchorageInfoService;
            _projectBasicInfoService = projectBasicInfoService;
            _cloudWindTyphoonService = cloudWindTyphoonService;
            _env = env;
        }

        [TypeFilter(typeof(WindCloudFilter))]
        public IActionResult Index()
        {
            var result = _mainService.PrepareIndex(
                CurrentUser?.UserCode, CurrentUser?.RealName, CurrentUser?.UserName);

            if (!result.Success)
            {
                return Redirect(result.RedirectUrl!);
            }

            ViewData["PXYKey"] = result.PXYKey;
            ViewData["LoginInfo"] = result.LoginInfoJson;
            ViewData["FlashTime"] = result.FlashTimeJson;

            return View();
        }

        [TypeFilter(typeof(WindCloudFilter))]
        public IActionResult Pad()
        {
            var result = _mainService.Prepare_Pad(
                CurrentUser?.UserCode, CurrentUser?.RealName, CurrentUser?.UserName);

            if (!result.Success)
            {
                return Redirect(result.RedirectUrl!);
            }

            ViewData["PXYKey"] = result.PXYKey;
            ViewData["LoginInfo"] = result.LoginInfoJson;
            ViewData["FlashTime"] = result.FlashTimeJson;

            return View();
        }

        [HttpPost]
        public string GetLeftPanel(DateTime? lastTime = null)
        {
            var res = new
            {
                MRU = _mruStatusInfoService.GetHistoryListByNumV2(lastTime),
                SatellteInfo = _satellteInfoService.GetInfoV2(),
                NavigationalStatus = _navigationalStatusService.GetInfoV2(),
                EGYRO = _egyroInfoService.GetInfoV2(),
                WindInfo = _weatherInfoService.GetLastWindInfoV2()
            };
            return JsonUtils.Serialize(res);
        }

        [HttpPost]
        public string GetRightPanel()
        {
            var res = new
            {
                DeviceStatusList = _deviceStatusInfoService.GetListV2()
            };
            return JsonUtils.Serialize(res);
        }

        [HttpPost]
        public string GetMRUInfo()
        {
            return JsonUtils.Serialize(_mruStatusInfoService.GetInfo());
        }

        [HttpPost]
        public string GetMRUHistoryList(DateTime? startTime = null, DateTime? endTime = null)
        {
            return JsonUtils.Serialize(_mruStatusInfoService.GetHistoryList(startTime, endTime));
        }

        [HttpPost]
        public string GetSatellteInfo()
        {
            return JsonUtils.Serialize(_satellteInfoService.GetInfo());
        }

        [HttpPost]
        public string GetSatellteHistory(DateTime? startTime = null, DateTime? endTime = null, int size = 0)
        {
            return JsonUtils.Serialize(_satellteInfoService.GetHistoryList(startTime, endTime, size));
        }

        [HttpPost]
        public string GetEGYROInfo()
        {
            return JsonUtils.Serialize(_egyroInfoService.GetInfo());
        }

        [HttpPost]
        public string GetEGYROHistoryList(DateTime? startTime = null, DateTime? endTime = null)
        {
            return JsonUtils.Serialize(_egyroInfoService.GetHistoryList(startTime, endTime));
        }

        [HttpPost]
        public string GetPlanPath()
        {
            return JsonUtils.Serialize(_projectPlanRouteInfoService.GetProjectList(0));
        }

        [HttpPost]
        public string GetForecastRoute()
        {
            return JsonUtils.Serialize(_projectPlanRouteInfoService.GetForecastList(0));
        }

        [HttpPost]
        public string GetWindInfo()
        {
            return JsonUtils.Serialize(_weatherInfoService.GetLastWindInfo());
        }

        [HttpPost]
        public string GetDeviceStatusList()
        {
            return JsonUtils.Serialize(_deviceStatusInfoService.GetList());
        }

        [HttpPost]
        public string GetWeatherForecase(string lon = "", string lat = "")
        {
            return JsonUtils.Serialize(_weatherInfoService.GetWeatherForecastList(lon, lat));
        }

        [HttpPost]
        public string GetChartZMotion(DateTime? startTime = null, DateTime? endTime = null, int size = 0)
        {
            if (size == 0)
            {
                size = 3000;
            }
            // 垂荡、横摇、纵摇
            return JsonUtils.Serialize(_mruStatusInfoService.GetChartZMotion(startTime, endTime, size));
        }

        [HttpPost]
        public string GetChartXMotion(DateTime? startTime = null, DateTime? endTime = null, int size = 0)
        {
            if (size == 0)
            {
                size = 3000;
            }
            // 横荡、纵荡、艏摇
            return JsonUtils.Serialize(_mruStatusInfoService.GetChartXMotion(startTime, endTime, size));
        }

        [HttpPost]
        public string GetChartWindSpeed(DateTime? startTime = null, DateTime? endTime = null, int size = 0)
        {
            return JsonUtils.Serialize(_weatherInfoService.GetChart(startTime, endTime, size, "风速"));
        }

        [HttpPost]
        public string GetChartWindDir(DateTime? startTime = null, DateTime? endTime = null, int size = 0)
        {
            return JsonUtils.Serialize(_weatherInfoService.GetChart(startTime, endTime, size, "风向"));
        }

        [HttpPost]
        public string GetChartShipSpeed(DateTime? startTime = null, DateTime? endTime = null, int size = 0)
        {
            return JsonUtils.Serialize(_navigationalStatusService.GetSpeedChartData(startTime, endTime, size));
        }

        [HttpPost]
        public string GetChartShipDir(DateTime? startTime = null, DateTime? endTime = null, int size = 0)
        {
            return JsonUtils.Serialize(_egyroInfoService.GetChart(startTime, endTime, size, "航向"));
        }

        [HttpPost]
        public string MessageUpload(IFormFile file, MessageInfo msg)
        {
            var res = new ResponseBase<MessageInfo>();
            if (file != null && file.Length > 0)
            {
                try
                {
                    var fileName = FileHelper.AppendStampToFileName(Path.GetFileName(file.FileName));
                    string dir = Path.Combine(_env.WebRootPath, "Content", "Towing", "Uploads");
                    string path = Path.Combine(dir, fileName);

                    // 保存文件
                    Directory.CreateDirectory(dir);
                    using (var fs = new FileStream(path, FileMode.Create))
                    {
                        file.CopyTo(fs);
                    }

                    msg.MsgContent = $"<img onclick='openThisImg(this)' src='/Content/Towing/Uploads/{fileName}' />{msg.MsgContent ?? ""}";
                    if (string.IsNullOrEmpty(msg.Sender) && CurrentUser != null)
                    {
                        msg.Sender = CurrentUser.RealName;
                    }
                    res = _messageInfoService.Send(msg);

                    return JsonUtils.Serialize(res);
                }
                catch (Exception ex)
                {
                    res.SetError(ex);
                }
            }
            else
            {
                res.SetError("请选择一个文件。");
            }
            return JsonUtils.Serialize(res);
        }

        [HttpPost]
        public string HikPicUpload(string file64, string fileName, DateTime? snapTime)
        {
            var res = new ResponseBase<MessageInfo>();
            try
            {
                // 移除可能存在的URI前缀（如"data:image/png;base64,"）
                file64 = file64 ?? "";
                string cleanBase64 = file64.IndexOf(',') > 0 ? file64.Split(',')[1] : file64;
                if (string.IsNullOrEmpty(cleanBase64))
                {
                    res.SetError("文件为空");
                    return JsonUtils.Serialize(res);
                }
                // 转换为字节数组
                byte[] imageBytes = Convert.FromBase64String(cleanBase64);
                // 指定文件保存路径
                fileName = FileHelper.AppendStampToFileName(Path.GetFileName(fileName));
                string refUrl = "/Content/Towing/Uploads/HikLocalPics/" + fileName;
                string dir = Path.Combine(_env.WebRootPath, "Content", "Towing", "Uploads", "HikLocalPics");
                string path = Path.Combine(dir, fileName);
                // 写入文件
                Directory.CreateDirectory(dir);
                System.IO.File.WriteAllBytes(path, imageBytes);

                var r = _hikInfoService.SaveLocalPic(snapTime ?? DateTime.UtcNow, refUrl);
                if (r.IsValid)
                {
                    res.SetOK();
                }
                else
                {
                    res.SetError(r.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                res.SetError(ex);
            }
            return JsonUtils.Serialize(res);
        }

        [HttpPost]
        public string GetHikLocalPics(DateTime? startTime = null, DateTime? endTime = null, int pageNo = 1, int pageSize = 12)
        {
            return JsonUtils.Serialize(_hikInfoService.GetLocalPics(startTime, endTime, pageNo, pageSize));
        }

        [HttpPost]
        public JsonResult GetTowingForecastFile(DateTime? startTime = null, DateTime? endTime = null)
        {
            var ret = new BaseReturn();
            try
            {
                var res = _fileInfoService.GetForecastFile(startTime, endTime);
                ret.Data = res;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MainController));
                ret.Message = "发生错误";
            }
            return Json(ret);
        }

        [HttpPost]
        public JsonResult GetTowingReportFile(DateTime? startTime = null, DateTime? endTime = null)
        {
            var ret = new BaseReturn();
            try
            {
                var res = _fileInfoService.GetReportFile(startTime, endTime);
                ret.Data = res;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MainController));
                ret.Message = "发生错误";
            }
            return Json(ret);
        }

        [HttpPost]
        public string MessageSend(MessageInfo msg)
        {
            if (string.IsNullOrEmpty(msg.Sender) && CurrentUser != null)
            {
                msg.Sender = CurrentUser.RealName;
            }
            return JsonUtils.Serialize(_messageInfoService.Send(msg));
        }

        [HttpPost]
        public string MessageListMsg(DateTime? lastTime = null, string query = "", int size = 0)
        {
            return JsonUtils.Serialize(_messageInfoService.ListMsg(lastTime, query, size));
        }

        [HttpPost]
        public string MessageListHistory(DateTime? dateTime = null, string query = "", string type = "", int lvl = -1, int idx = 1, int size = 20)
        {
            return JsonUtils.Serialize(_messageInfoService.MessageListHistory(dateTime, type, lvl, query, idx, size));
        }

        [HttpPost]
        public string GetShipSurShips(string mmsi = "")
        {
            return JsonUtils.Serialize(_shipInfoService.GetShipSurShips(mmsi));
        }

        [HttpPost]
        public string GetAnchorageInfo()
        {
            return JsonUtils.Serialize(_anchorageInfoService.GetList());
        }

        [HttpPost]
        public string GetProjInfo()
        {
            return JsonUtils.Serialize(_projectBasicInfoService.GetInfo());
        }

        [HttpPost]
        public string SaveProjInfo(ProjectBasicInfo info)
        {
            var adminUsers = new List<string> { "2018001515" };
            if (!adminUsers.Contains(CurrentUser?.UserCode))
            {
                return "";
            }
            return JsonUtils.Serialize(_projectBasicInfoService.SaveInfo(info));
        }

        [HttpPost]
        public string SaveProjPlanRoute(List<ProjectPlanRouteInfo> list)
        {
            var adminUsers = new List<string> { "2018001515" };
            if (!adminUsers.Contains(CurrentUser?.UserCode))
            {
                return "";
            }
            return JsonUtils.Serialize(_projectPlanRouteInfoService.SaveProjectList(list));
        }

        [HttpPost]
        public string GetPics(DateTime? startTime = null, DateTime? endTime = null, int pageNo = 1, int pageSize = 10, string netProtocol = "http")
        {
            return JsonUtils.Serialize(_hikInfoService.GetPics(startTime, endTime, pageNo, pageSize, netProtocol).GetAwaiter().GetResult());
        }

        public IActionResult DataTest()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DataGenerate()
        {
            _mainService.DataGenerate();
            return Ok();
        }

        public async Task<IActionResult> DataExport(string startTime, string endTime)
        {
            if (string.IsNullOrEmpty(CurrentUser?.UserCode))
            {
                return NotFound();
            }
            if (!_mainService.IsAdmin(CurrentUser.UserCode))
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
            {
                return NotFound();
            }

            var result = _mainService.DataExport(startTime, endTime);
            if (result == null)
            {
                return NotFound();
            }

            return File(result.Value.bytes, "application/vnd.ms-excel", result.Value.fileName);
        }

        public async Task<IActionResult> PicExport(string startTime, string endTime)
        {
            if (string.IsNullOrEmpty(CurrentUser?.UserCode))
            {
                return NotFound();
            }
            if (!_mainService.IsAdmin(CurrentUser.UserCode))
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
            {
                return NotFound();
            }

            var result = _mainService.PicExport(startTime, endTime, _env.WebRootPath);
            if (result == null)
            {
                return NotFound();
            }

            return File(result.Value.bytes, "application/x-zip-compressed", result.Value.fileName);
        }

        [HttpPost]
        public JsonResult UploadTowingReport()
        {
            var ret = new BaseReturn();
            var adminUsers = new List<string> { "2018001515" };
            if (!adminUsers.Contains(CurrentUser?.UserCode ?? ""))
            {
                ret.Message = "没有权限";
                ret.Success = false;
                return Json(ret);
            }

            try
            {
                var fileCountStr = Request.Form["FileCount"].ToString();
                if (string.IsNullOrEmpty(fileCountStr))
                {
                    ret.Message = "没有任何文件";
                    ret.Success = false;
                    return Json(ret);
                }

                List<IFormFile> files = new List<IFormFile>();
                for (int i = 1; i <= Convert.ToInt32(fileCountStr); i++)
                {
                    var file = Request.Form.Files["File" + i];
                    if (file != null && file.Length > 0)
                    {
                        files.Add(file);
                    }
                }

                string fileFolderPath = Path.Combine(_env.WebRootPath, "File", "TowingReport");

                _mainService.SaveTowingReportFiles(files, fileFolderPath);

                ret.Success = true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MainController));
                ret.Message = "发生错误";
            }
            return Json(ret);
        }

        [HttpPost]
        public JsonResult LoadTyphoonForecast()
        {
            var ret = new BaseReturn();
            try
            {
                var data = _cloudWindTyphoonService.TyphoonAnalyze();
                ret.Success = true;
                ret.Data = data;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MainController));
                ret.Message = "发生错误";
            }
            return Json(ret);
        }

        [HttpPost]
        public JsonResult LoadProjectArea()
        {
            var ret = new BaseReturn();
            try
            {
                var areas = _mainService.GetProjectAreas();
                ret.Success = true;
                ret.Data = areas;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MainController));
                ret.Message = "发生错误";
            }
            return Json(ret);
        }
    }
}
