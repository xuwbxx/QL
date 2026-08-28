using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Service.Base;
using Service.Wind;
using Tool;

namespace WindCloud.Areas.Base.Controllers
{
    public class WebEncryptionController : Controller
    {
        private string EnKey = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:Key").ToString();
        private string EnIV = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:IV").ToString();

        public CookieService _cookieService { get; }

        public WebValidateService _webValidateService { get; }

        public WebEncryptionController(CookieService cookieService, WebValidateService webValidateService)
        {
            _cookieService = cookieService;
            _webValidateService = webValidateService;
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
    }
}
