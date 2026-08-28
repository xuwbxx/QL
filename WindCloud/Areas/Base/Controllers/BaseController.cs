using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.Cloud;
using Service.Base;
using Service.Wind;
using Tool;

namespace WindCloud.Areas.Base.Controllers
{
    [Area("Cloud")]
    public class BaseController : Controller
    {
        public CookieService _cookieService { get; }

        public BaseController(CookieService cookieService)
        {
            _cookieService = cookieService;
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

        //private string EnKey = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:Key").ToString();
        //private string EnIV = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:IV").ToString();

        //protected string CreateWebToken(string UserCode)
        //{
        //    string token = CryptographyUtils.TripleDESEncrypt(UserCode, EnKey, EnIV);
        //    return token;
        //}

        //protected bool ValidateWebToken(string token)
        //{
        //    bool Success = false;
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(token))
        //        {
        //            token = token.Substring(0, 24);

        //            string UserCode = CryptographyUtils.TripleDESDecrypt(token, EnKey, EnIV);

        //            if (UserCode.Equals(UserCode))
        //            {
        //                Success = true;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Success = false;
        //        LoggerUtils.Error(ex.ToString(), typeof(BaseController));
        //    }
        //    return Success;
        //}


        //public async Task<IActionResult> FindShjUser(CloudWindRequest request)
        //{
        //    BaseReturn ret = new BaseReturn();
        //    if (!ValidateWebToken(request.PostToken))
        //    {
        //        ret.Message = "身份信息失效，请重新登录。";
        //        return Ok(ret);
        //    }
        //    if (string.IsNullOrEmpty(request.Name))
        //    {
        //        return Ok(ret);
        //    }
        //    try
        //    {
        //        string msg = "";
        //        var list = _projectService.FindShjUser(request.Name, out msg);

        //        if (string.IsNullOrEmpty(msg))
        //        {
        //            ret.Data = list;
        //            ret.Success = true;
        //        }
        //        else
        //        {
        //            ret.Data = list;
        //            ret.Message = msg;
        //            ret.Success = false;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ret.Success = false;
        //        //记录日志
        //        LoggerUtils.Error(ex.ToString(), typeof(BaseController));
        //    }
        //    return Ok(ret);
        //}

    }
}
