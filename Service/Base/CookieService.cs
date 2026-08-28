using Model.Base;
using System.Text;
using Tool;

namespace Service.Base
{
    public class CookieService
    {
        private readonly string _cookieKey;
        private readonly string _desKey;
        private readonly string _desIv;

        /// <summary>
        /// 构造函数注入配置信息
        /// </summary>
        public CookieService()
        {
            // 从配置读取（如果需要通过DI注入IConfiguration，可以添加构造函数参数）
            _cookieKey = AppSettingUtils.GetSetting("AppSettings:Login:CookieKey");
            _desKey = AppSettingUtils.GetSetting("AppSettings:Login:Key");
            _desIv = AppSettingUtils.GetSetting("AppSettings:Login:IV");
        }

        public void SetUserCookie(SHJUserInfo user)
        {
            if (user == null)
            {
                return;
            }

            string jsonUser = JsonUtils.Serialize(user);
            string encryptedUser = CryptographyUtils.TripleDESEncrypt(jsonUser, _desKey, _desIv);
            string desValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptedUser));

            CookieUtils.SetCookie(_cookieKey, desValue);
        }

        public SHJUserInfo? GetUserCookie()
        {
            var cookieValue = CookieUtils.GetCookie(_cookieKey);

            if (string.IsNullOrEmpty(cookieValue))
            {
                return null;
            }

            try
            {
                byte[] bytes = Convert.FromBase64String(cookieValue);
                string encryptedUser = Encoding.UTF8.GetString(bytes);
                string jsonUser = CryptographyUtils.TripleDESDecrypt(encryptedUser, _desKey, _desIv);

                return JsonUtils.Deserialize<SHJUserInfo>(jsonUser);
            }
            catch (Exception)
            {
                // 解密失败时清除Cookie并返回null
                CookieUtils.DeleteCookie(_cookieKey);
                return null;
            }
        }

        /// <summary>
        /// 清除用户Cookie
        /// </summary>
        public void CookieCleanUp()
        {
            CookieUtils.DeleteCookie(_cookieKey);
        }

    }
}
