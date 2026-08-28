using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Model.Base;
using Service.Base;
using Tool;

namespace Service.Wind
{
    public class TechCenterService : WindBaseService
    {
        public TechCenterService(CloudWind_KingBase_UnitOfWorkFactory techCenterUowFactory, CookieService cookieService)
        : base(techCenterUowFactory, cookieService)
        {

        }

        private string key = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:Key").ToString();
        private string iv = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:IV").ToString();

        public string CreateToken(SHJUserInfo info)
        {
            string Token = string.Empty;
            try
            {
                //1.拼接秘文(用户名、软件ID、时间)
                DateTime now = DateTime.Now;

                string Text = info.UserID + "_" + now.ToString("yyyy") + "_" + info.UserCode + "_" + now.ToString("MM") + "_" + info.UserName + "_" + now.ToString("dd") + "_" + info.Mobile + "_" + now.ToString("HH") + "_" + now.ToString("mm") + "_" + now.ToString("ss");

                //3DES加密
                Token = CryptographyUtils.TripleDESEncrypt(Text, key, iv);

            }
            catch (Exception ex)
            {
                Token = "";
            }

            return Token;
        }

        /// <summary>
        /// 验证token，并获取用户信息（UserCode）
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public SHJUserInfo CheckToken(string token)
        {
            SHJUserInfo info = new SHJUserInfo();
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
                int MM = Convert.ToInt32(textArr[3]);
                int dd = Convert.ToInt32(textArr[5]);
                int HH = Convert.ToInt32(textArr[7]);
                int mm = Convert.ToInt32(textArr[8]);
                int ss = Convert.ToInt32(textArr[8]);

                int UserID = Convert.ToInt32(textArr[0]);
                string UserCode = textArr[2];
                string UserName = textArr[4];
                string Mobile = textArr[6];

                DateTime time = new DateTime(yyyy, MM, dd, HH, mm, ss);

                if (time.AddMinutes(30) < DateTime.Now)
                {
                    return info;
                }

                info.UserID = UserID;
                info.UserCode = UserCode;
                info.UserName = UserName;
                info.Mobile = Mobile;

            }
            catch (Exception ex)
            {
                info = new SHJUserInfo();
                LoggerUtils.Error(ex.ToString(), typeof(TechCenterService));
            }
            return info;
        }

        public async Task<(string Url, string msg)> GetUrl(int PlatID)
        {
            string msg = "";
            string Url = "";
            using (var uow = _techCenterUowFactory.Create())
            {
                var Manage_Platform_repo = uow.GetRepository<Manage_Platform>();

                var PlatAsync = await Manage_Platform_repo.FindAsync(a => !a.IsDelete && a.ID == PlatID);
                var Plat = PlatAsync.FirstOrDefault();
                if (Plat == null)
                {
                    msg = "不存在此平台";
                    return (Url, msg);
                }

                if (string.IsNullOrEmpty(Plat.Url))
                {
                    msg = "没有配置平台网址";
                    return (Url, msg);
                }

                string token = CreateToken(CurrentUser);

                Url = Plat.Url + @"?Token=" + token;

            }

            return (Url, msg);
        }

    }
}
