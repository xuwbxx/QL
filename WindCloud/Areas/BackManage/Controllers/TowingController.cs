using BIM.Business.CCSHJWebApi;
using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.Cloud.BackManage;
using Model.Tech.System;
using Service.Base;
using Service.Wind;
using Service.Wind.BackManage;
using Tool;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.BackManage.Controllers
{
    [Area("BackManage")]
    public class TowingController : WebEncryptionController
    {
        private readonly BackTowingManageService _backTowingManageService;
        private readonly CloudWindInfoService _cloudWindInfoService;

        public TowingController(
            CookieService cookieService,
            WebValidateService webValidateService,
            BackTowingManageService backTowingManageService,
            CloudWindInfoService cloudWindInfoService)
            : base(cookieService, webValidateService)
        {
            _backTowingManageService = backTowingManageService;
            _cloudWindInfoService = cloudWindInfoService;
        }

        // GET: BackManage/Towing
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
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            try
            {
                var (list, totalCount, pageIndex, msg) = _backTowingManageService.ListQuery(request);

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
                LoggerUtils.Error(ex.ToString(), typeof(TowingController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult DataQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ID == 0)
            {
                return Ok(ret);
            }
            try
            {
                var (data, msg) = _backTowingManageService.DataQuery(request);

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
                LoggerUtils.Error(ex.ToString(), typeof(TowingController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }

        [HttpPost]
        public async Task<IActionResult> DataSave(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            try
            {
                var (success, msg, userCode) = _backTowingManageService.DataSave(request);

                if (!success)
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Success = true;

                if (request.IsConfirm && !string.IsNullOrEmpty(userCode))
                {
                    string CloudPlatUrl = AppSettingUtils.GetSetting("CloudPlatUrl");
                    var Url = "/WindDoor/IOT/Index";
                    var Token = CloudCenterService.CreateJJTMessageRedirectToken_Common(userCode, Url);
                    var RedirectUrl = CloudPlatUrl + @"Home/JJTMessageRedirect__Common?CloudToken=" + Token;

                    //发送消息
                    List<SystemJJTInform> InformRequests = new List<SystemJJTInform>();
                    SystemJJTInform InformRequest = new SystemJJTInform();
                    InformRequest.UserCode = userCode;
                    InformRequest.Url = RedirectUrl;
                    InformRequest.Title = "您有云服务平台消息";
                    InformRequest.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情";
                    InformRequests.Add(InformRequest);

                    _cloudWindInfoService.SendJJTMessage(InformRequests);

                    //await Task.Run(() =>
                    //{
                    //    _cloudWindInfoService.SendJJTMessage(InformRequests);
                    //});
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = "发生错误，请联系管理员";
                LoggerUtils.Error(ex.ToString(), typeof(TowingController));
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult DataDelete(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            try
            {
                var (success, msg) = _backTowingManageService.DataDelete(request);

                if (!success)
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = "发生错误，请联系管理员";
                LoggerUtils.Error(ex.ToString(), typeof(TowingController));
            }
            return Ok(ret);
        }
    }
}
