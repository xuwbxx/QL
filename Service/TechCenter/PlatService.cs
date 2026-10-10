using Model.Base;
using Tool;

namespace Service.TechCenter
{
    public class PlatService
    {
        private static readonly string key = AppSettingUtils.GetSetting("AppSettings:Login:Key");
        private static readonly string iv = AppSettingUtils.GetSetting("AppSettings:Login:IV");

        public string CreateToken(SHJUserInfo info)
        {
            string token = string.Empty;

            try
            {
                DateTime now = DateTime.Now;

                string text = info.UserID + "_" + now.ToString("yyyy") + "_" + info.UserCode + "_" + now.ToString("MM") + "_" + info.UserName + "_" + now.ToString("dd") + "_" + info.Mobile + "_" + now.ToString("HH") + "_" + now.ToString("mm") + "_" + now.ToString("ss");

                token = CryptographyUtils.TripleDESEncrypt(text, key, iv);
            }
            catch (Exception ex)
            {
                token = "";
                LoggerUtils.Error(ex.ToString(), typeof(PlatService));
            }

            return token;
        }

        /// <summary>
        /// 验证token，并获取用户信息
        /// </summary>
        public SHJUserInfo CheckToken(string token)
        {
            var info = new SHJUserInfo();
            if (string.IsNullOrEmpty(token))
            {
                return info;
            }
            try
            {
                string tokenText = CryptographyUtils.TripleDESDecrypt(token, key, iv);
                string[] textArr = tokenText.Split('_');
                if (textArr.Length != 10)
                {
                    return info;
                }

                int yyyy = Convert.ToInt32(textArr[1]);
                int mm = Convert.ToInt32(textArr[3]);
                int dd = Convert.ToInt32(textArr[5]);
                int hh = Convert.ToInt32(textArr[7]);
                int min = Convert.ToInt32(textArr[8]);
                int ss = Convert.ToInt32(textArr[9]);

                info.UserID = Convert.ToInt32(textArr[0]);
                info.UserCode = textArr[2];
                info.UserName = textArr[4];
                info.Mobile = textArr[6];

                var time = new DateTime(yyyy, mm, dd, hh, min, ss);

                if (time.AddMinutes(30) < DateTime.Now)
                {
                    return new SHJUserInfo();
                }
            }
            catch (Exception ex)
            {
                info = new SHJUserInfo();
                LoggerUtils.Error(ex.ToString(), typeof(PlatService));
            }
            return info;
        }
    }
}
