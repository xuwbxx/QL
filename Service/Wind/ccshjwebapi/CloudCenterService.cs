using Tool;

namespace BIM.Business.CCSHJWebApi
{
    public class CloudCenterService
    {
        private static string key = AppSettingUtils.GetSetting("AppSettings:Login:Key").ToString();
        private static string iv = AppSettingUtils.GetSetting("AppSettings:Login:IV").ToString();

        /// <summary>
        /// 生成token(插拔桩计算)
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="SoftwareID"></param>
        /// <returns></returns>
        public static string CreateToken(string UserID, int TaskID)
        {
            string Token = string.Empty;

            try
            {
                //1.拼接秘文(用户名、软件ID、时间)
                DateTime now = DateTime.Now;

                string PostToken = CryptographyUtils.DESEncrypt(DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"), key.Substring(key.Length - 8), iv);

                string Code = now.ToShortTimeString() + "-" + UserID + "-" + TaskID + "-" + now.ToString("yyyy/MM/dd HH:mm:ss") + "-" + PostToken;

                //3DES加密
                var DESCode = CryptographyUtils.TripleDESEncrypt(Code, key, iv);

                //Base64加密
                Token = CryptographyUtils.Base64Encrypt(DESCode);
            }
            catch (Exception ex)
            {
                Token = "";
            }

            return Token;
        }

        /// <summary>
        /// 解析token
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static string DecryptToken(string Token)
        {
            string Code = string.Empty;

            if (string.IsNullOrEmpty(Token))
                return Code;

            try
            {
                var DesCode = CryptographyUtils.Base64Decrypt(Token);

                Code = CryptographyUtils.TripleDESDecrypt(DesCode, key, iv);

            }
            catch (Exception ex)
            {
                Code = "";
            }

            return Code;
        }


        /// <summary>
        /// 生成交建通消息提醒链接Token
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public string CreateJJTMessageRedirectToken(string Type, string UserID, int ID)
        {
            string Token = string.Empty;
            if (string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(UserID) || ID == 0)
            {
                return Token;
            }
            try
            {
                //1.拼接秘文(用户名、软件ID、时间)
                DateTime now = DateTime.Now;

                string Code = now.ToShortTimeString() + "-" + UserID + "-" + Type + "-" + ID + "-" + now.ToString("yyyy/MM/dd HH:mm:ss");

                //3DES加密
                var DESCode = CryptographyUtils.TripleDESEncrypt(Code, key, iv);

                //Base64加密
                Token = CryptographyUtils.Base64Encrypt(DESCode);
            }
            catch (Exception ex)
            {
                Token = "";
            }

            return Token;


        }

        /// <summary>
        /// 生成交建通消息提醒链接Token（可作业预报）
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public static string CreateJJTKZYReportMessageRedirectToken(string Type, string UserID, string TaskCode)
        {
            string Token = string.Empty;
            if (string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(UserID) || string.IsNullOrEmpty(TaskCode))
            {
                return Token;
            }
            try
            {
                //1.拼接秘文(用户名、软件ID、时间)
                DateTime now = DateTime.Now;

                string Code = now.ToShortTimeString() + "-" + UserID + "-" + Type + "-" + TaskCode + "-" + now.ToString("yyyy/MM/dd HH:mm:ss");

                //3DES加密
                var DESCode = CryptographyUtils.TripleDESEncrypt(Code, key, iv);

                //Base64加密
                Token = CryptographyUtils.Base64Encrypt(DESCode);
            }
            catch (Exception ex)
            {
                Token = "";
            }

            return Token;


        }


        /// <summary>
        /// 生成内网的token
        /// </summary>
        /// <returns></returns>
        public static string CreateTokenInter(string Platform)
        {
            string Token = string.Empty;

            try
            {
                //1.拼接秘文(用户名、软件ID、时间)
                DateTime now = DateTime.Now;

                string Text = Platform + "_" + now.Year + "_" + now.Month + "_" + now.Day + "_" + now.Hour + "_" + now.Minute + "_" + now.Second + "_" + now.Millisecond;

                //3DES加密
                var DESCode = CryptographyUtils.TripleDESEncrypt(Text, key, iv);

                //Base64加密
                Token = CryptographyUtils.Base64Encrypt(DESCode);
            }
            catch (Exception ex)
            {
                Token = "";
            }

            return Token;
        }

        /// <summary>
        /// 解析内网token
        /// </summary>
        /// <returns></returns>
        public static string DecryptTokenInter(string token)
        {
            string Code = string.Empty;
            if (string.IsNullOrEmpty(token))
            {
                return Code;
            }
            try
            {
                var DesCode = CryptographyUtils.Base64Decrypt(token);

                Code = CryptographyUtils.TripleDESDecrypt(DesCode, key, iv);

            }
            catch (Exception ex)
            {
                Code = "";
            }
            return Code;
        }


        /// <summary>
        /// 平台申请提示
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public static string CreateJJTMessageRedirectToken_PlatRegister(string UserCode, string Plat)
        {
            string Token = string.Empty;
            if (string.IsNullOrEmpty(UserCode) || string.IsNullOrEmpty(Plat))
            {
                return Token;
            }
            try
            {
                //1.拼接秘文(用户名、软件ID、时间)
                DateTime now = DateTime.Now;

                string Code = now.ToShortTimeString() + "-" + UserCode + "-" + Plat + "-" + now.ToString("yyyy/MM/dd HH:mm:ss");

                //3DES加密
                var DESCode = CryptographyUtils.TripleDESEncrypt(Code, key, iv);

                //Base64加密
                Token = CryptographyUtils.Base64Encrypt(DESCode);
            }
            catch (Exception ex)
            {
                Token = "";
            }

            return Token;


        }


        /// <summary>
        /// 生成交建通跳转链接
        /// </summary>
        /// <param name="UserCode"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static string CreateJJTMessageRedirectToken_Common(string UserCode, string Url)
        {
            string Token = string.Empty;
            if (string.IsNullOrEmpty(UserCode) || string.IsNullOrEmpty(Url))
            {
                return Token;
            }
            try
            {
                //1.拼接秘文(用户名、软件ID、时间)
                DateTime now = DateTime.Now;

                string Code = now.ToShortTimeString() + "-" + UserCode + "-" + Url + "-" + now.ToString("yyyy/MM/dd HH:mm:ss");

                //3DES加密
                var DESCode = CryptographyUtils.TripleDESEncrypt(Code, key, iv);

                //Base64加密
                Token = CryptographyUtils.Base64Encrypt(DESCode);
            }
            catch (Exception ex)
            {
                Token = "";
            }

            return Token;


        }


    }
}
