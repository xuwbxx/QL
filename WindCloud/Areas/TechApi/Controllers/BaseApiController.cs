using BIM.Business.CCSHJWebApi;
using Microsoft.AspNetCore.Mvc;
using Tool;

namespace WindCloud.Areas.TechApi.Controllers
{
    //[Route("CCSHJTechApi/[controller]/[action]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        /// <summary>
        /// 验证API接口Token是否有效（10分钟内有效）
        /// </summary>
        protected bool Authored(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            try
            {
                string WholeKey = AppSettingUtils.GetSetting("AppSettings:Login:Key");
                string Key = WholeKey.Substring(WholeKey.Length - 8);
                string IV = AppSettingUtils.GetSetting("AppSettings:Login:IV");

                string AuthoredText = CryptographyUtils.DESDecrypt(token, Key, IV);
                string[] SplitText = AuthoredText.Split('_');

                int yyyy = int.Parse(SplitText[0]);
                int MM = int.Parse(SplitText[1]);
                int dd = int.Parse(SplitText[2]);
                int HH = int.Parse(SplitText[3]);
                int mm = int.Parse(SplitText[4]);
                int ss = int.Parse(SplitText[5]);

                var Time = new DateTime(yyyy, MM, dd, HH, mm, ss);

                if (Time.AddMinutes(10) < DateTime.Now)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BaseApiController));
                return false;
            }
        }

        /// <summary>
        /// 验证跨平台接口Token是否有效（10分钟内有效）
        /// </summary>
        protected bool InterAuthored(string token, string Platform)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(Platform))
            {
                return false;
            }

            try
            {
                string Text = CloudCenterService.DecryptTokenInter(token);

                string[] SplitText = Text.Split('_');

                string PlatFormText = SplitText[0];
                if (!PlatFormText.Equals(Platform))
                {
                    return false;
                }

                int yyyy = int.Parse(SplitText[1]);
                int MM = int.Parse(SplitText[2]);
                int dd = int.Parse(SplitText[3]);
                int HH = int.Parse(SplitText[4]);
                int mm = int.Parse(SplitText[5]);
                int ss = int.Parse(SplitText[6]);

                var Time = new DateTime(yyyy, MM, dd, HH, mm, ss);

                if (Time.AddMinutes(10) < DateTime.Now)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BaseApiController));
                return false;
            }
        }
    }
}
