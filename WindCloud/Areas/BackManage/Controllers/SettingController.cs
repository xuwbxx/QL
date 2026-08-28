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
    public class SettingController : WebEncryptionController
    {
        private readonly BackSettingService _backSettingService;

        public SettingController(CookieService cookieService, Service.Wind.WebValidateService webValidateService, BackSettingService backSettingService)
            : base(cookieService, webValidateService)
        {
            _backSettingService = backSettingService;
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
        public async Task<IActionResult> DataSave(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }

            if (string.IsNullOrEmpty(request.OldProjectCode) || string.IsNullOrEmpty(request.NewProjectCode))
            {
                ret.Message = "项目编号不能是空的";
                return Ok(ret);
            }

            // 项目编号格式
            if (!request.NewProjectCode.StartsWith("OW") && !request.NewProjectCode.StartsWith("PW") && request.NewProjectCode.Length != 6)
            {
                ret.Message = "项目编号必须是OW或者PW开头，后面再加四位数字";
                return Ok(ret);
            }

            try
            {
                string result = _backSettingService.DataSave(request.OldProjectCode, request.NewProjectCode);

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
                LoggerUtils.Error(ex.ToString(), typeof(SettingController));
            }

            return Ok(ret);
        }
    }
}
