using Microsoft.AspNetCore.Mvc;
using Service.Base;
using Service.Base.Filter;
using Service.Towing;
using Tool;
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

        public MainController(
            CookieService cookieService,
            MainService mainService,
            MRUStatusInfoService mruStatusInfoService,
            SatellteInfoService satellteInfoService,
            NavigationalStatusService navigationalStatusService,
            EGYROInfoService egyroInfoService,
            WeatherInfoService weatherInfoService,
            DeviceStatusInfoService deviceStatusInfoService,
            ProjectPlanRouteInfoService projectPlanRouteInfoService)
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
    }
}
