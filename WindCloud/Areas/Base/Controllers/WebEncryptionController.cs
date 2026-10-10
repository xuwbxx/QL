using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.Cloud;
using Service.Base;
using Service.Wind;
using Tool;
using WindCloud.Areas.Cloud.Controllers;

namespace WindCloud.Areas.Base.Controllers
{
    public class WebEncryptionController : Controller
    {
        private string EnKey = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:Key").ToString();
        private string EnIV = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:IV").ToString();

        public CookieService _cookieService { get; }
        private readonly ProjectService _projectService;

        public WebValidateService _webValidateService { get; }

        public WebEncryptionController(CookieService cookieService, ProjectService projectService, WebValidateService webValidateService)
        {
            _cookieService = cookieService;
            _webValidateService = webValidateService;
            _projectService = projectService;
        }

        public SHJUserInfo? CurrentUser
        {
            get
            {
                try
                {
                    return _cookieService.GetUserCookie();

                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        protected string CreateWebToken()
        {
            string token = CryptographyUtils.TripleDESEncrypt(CurrentUser.UserCode, EnKey, EnIV);
            return token;
        }

        protected bool ValidateWebToken(string token)
        {
            bool Success = false;
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    token = token.Substring(0, 24);

                    string UserCode = CryptographyUtils.TripleDESDecrypt(token, EnKey, EnIV);

                    if (UserCode.Equals(CurrentUser.UserCode))
                    {
                        Success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(WebEncryptionController));
            }
            return Success;
        }


        protected async Task<bool> RightValidate()
        {
            bool IsRight = false;
            try
            {


                //管理员或项目经理
                if (string.IsNullOrEmpty(CurrentUser.UserCode))
                {
                    return false;
                }

                IsRight = await _webValidateService.RightValidate(CurrentUser.UserCode);

                return IsRight;
            }
            catch (Exception ex)
            {
                IsRight = false;
                LoggerUtils.Error(ex.ToString(), typeof(WebEncryptionController));
            }
            return IsRight;
        }


        protected async Task<List<string>> GetAdminUserCode()
        {
            List<string> userCodes = new List<string>();
            userCodes = await _webValidateService.GetAdminUserCode();
            return userCodes;
        }

        public async Task<IActionResult> FindShjUser(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.Name))
            {
                return Ok(ret);
            }
            try
            {
                string msg = "";
                var list = _projectService.FindShjUser(request.Name, out msg);

                if (string.IsNullOrEmpty(msg))
                {
                    ret.Data = list;
                    ret.Success = true;
                }
                else
                {
                    ret.Data = list;
                    ret.Message = msg;
                    ret.Success = false;
                }

            }
            catch (Exception ex)
            {
                ret.Success = false;
                //记录日志
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

    }
}
