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
    public class PlatformController : WebEncryptionController
    {
        private readonly BackPlatformService _platformService;

        public PlatformController(CookieService cookieService, Service.Wind.WebValidateService webValidateService, BackPlatformService platformService)
            : base(cookieService, webValidateService)
        {
            _platformService = platformService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Software()
        {
            if (!await RightValidate())
            {
                return Redirect("/Cloud/CloudBase/ErrorPage?ErrorText=不能进入后台配置界面。");
            }

            ViewData["PostToken"] = CreateWebToken();
            return View();
        }

        [HttpPost]
        public IActionResult SoftwareListQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();
            try
            {
                var (list, totalCount, pageIndex, msg) = _platformService.SoftwareListQuery(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Data = list;
                ret.TotalCount = totalCount;
                ret.TotalPage = totalCount % request.PageSize == 0 ? (totalCount / request.PageSize) : (totalCount / request.PageSize) + 1;
                ret.Success = true;
                ret.PageIndex = pageIndex;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PlatformController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult SoftwareDataQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.ID == 0)
            {
                return Ok(ret);
            }

            try
            {
                var (model, msg) = _platformService.SoftwareDataQuery(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Data = model;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PlatformController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }

        public async Task<IActionResult> Company()
        {
            if (!await RightValidate())
            {
                return Redirect("/Cloud/CloudBase/ErrorPage?ErrorText=不能进入后台配置界面。");
            }

            ViewData["PostToken"] = CreateWebToken();
            return View();
        }

        [HttpPost]
        public IActionResult CompanyListQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();
            try
            {
                var (list, totalCount, pageIndex, msg) = _platformService.CompanyListQuery(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Data = list;
                ret.TotalCount = totalCount;
                ret.TotalPage = totalCount % request.PageSize == 0 ? (totalCount / request.PageSize) : (totalCount / request.PageSize) + 1;
                ret.Success = true;
                ret.PageIndex = pageIndex;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PlatformController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult CompanyDataQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.ID == 0)
            {
                return Ok(ret);
            }

            try
            {
                var (data, msg) = _platformService.CompanyDataQuery(request);

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
                LoggerUtils.Error(ex.ToString(), typeof(PlatformController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult CompanyDataSave(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (string.IsNullOrEmpty(request.CompanyName))
            {
                ret.Message = "用户名不能为空";
                return Ok(ret);
            }

            try
            {
                var msg = _platformService.CompanyDataSave(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Success = false;
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Success = true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PlatformController));
                ret.Success = false;
                ret.Message = "发生错误，请联系管理员";
            }
            return Ok(ret);
        }
    }
}
