using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.Cloud.BackManage;
using Service.Base;
using Service.Wind.BackManage;
using Tool;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.BackManage.Controllers
{
    [Area("BackManage")]
    public class TowingReportController : WebEncryptionController
    {
        private readonly BackTowingReportService _backTowingReportService;

        public TowingReportController(CookieService cookieService, Service.Wind.WebValidateService webValidateService, BackTowingReportService backTowingReportService)
            : base(cookieService, webValidateService)
        {
            _backTowingReportService = backTowingReportService;
        }

        public async Task<IActionResult> Index()
        {
            if (!await RightValidate())
            {
                return Redirect("/Cloud/CloudBase/ErrorPage?ErrorText=不能进入后台配置界面。");
            }

            ViewData["PostToken"] = CreateWebToken();
            return View();
        }

        [HttpPost]
        public IActionResult ListQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            try
            {
                var (list, totalCount, pageIndex, msg) = _backTowingReportService.ListQuery(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Data = list;
                ret.Success = true;
                ret.TotalCount = totalCount;
                ret.PageIndex = pageIndex;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(TowingReportController));
            }

            return Ok(ret);
        }

        [HttpPost]
        public IActionResult DataQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }

            try
            {
                var (data, msg) = _backTowingReportService.DataQuery(request);

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
                LoggerUtils.Error(ex.ToString(), typeof(TowingReportController));
            }

            return Ok(ret);
        }

        [HttpPost]
        public IActionResult DataSave(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }

            try
            {
                string result = _backTowingReportService.DataSave(request);

                if (!string.IsNullOrEmpty(result))
                {
                    ret.Message = result;
                    return Ok(ret);
                }

                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = "发生错误，请联系管理员";
                LoggerUtils.Error(ex.ToString(), typeof(TowingReportController));
            }

            return Ok(ret);
        }
    }
}
