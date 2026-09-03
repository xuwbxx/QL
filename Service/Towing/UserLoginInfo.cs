using Tool;
using Model.Base;

namespace Service.Towing
{
    public class UserLoginInfo : SHJUserInfo
    {
        public const string LoginExpired = "登录超时，请重新登录";
        public const string NoToken = "无token，请联系管理员";
        public const string TokenError = "token无效：";

        /// <summary>
        /// 登录验证时间
        /// </summary>
        public DateTime LastLoginTime { set; get; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { set; get; }

        /// <summary>
        /// 是否合法
        /// </summary>
        public bool IsValid => string.IsNullOrEmpty(ErrorMessage);

        public UserLoginInfo() : base()
        {
        }

        public static UserLoginInfo GetLoginInfo(string token = "", int expireMinutes = 60)
        {
            UserLoginInfo loginInfo = new UserLoginInfo();
            try
            {
                //1.解析token
                if (string.IsNullOrEmpty(token))
                {
                    loginInfo = Utils.GetCookies<UserLoginInfo>();
                    //跳转到错误页面
                    if (loginInfo == null)
                    {
                        loginInfo = new UserLoginInfo();
                        loginInfo.ErrorMessage = NoToken;
                        return loginInfo;
                    }
                    else
                    {
                        // 验证是否过期
                        if (expireMinutes > 0)
                        {
                            if (loginInfo.LastLoginTime.AddMinutes(expireMinutes) < DateTime.Now)
                            {
                                loginInfo.ErrorMessage = LoginExpired;
                            }
                            else
                            {
                                // 刷新验证时间
                                loginInfo.LastLoginTime = DateTime.Now;
                                Utils.SaveCookies(loginInfo);
                            }
                        }
                        return loginInfo;
                    }
                }

                string key = AppSettingUtils.GetSetting("SoftwareApi:CCSHJApiKey");
                string iv = AppSettingUtils.GetSetting("AppSettings:CCSHJApiIV");

                var DesCode = CryptographyUtils.Base64Decrypt(token);

                string Code = CryptographyUtils.TripleDESDecrypt(DesCode, key, iv);

                //例子
                //20250325095938_2018001515_095938_程伟_095938_1_095938
                //主时间        交建通用户ID       姓名     角色编号（1是陆上 2是船上）

                string[] codeArr = Code.Split('_');

                if (codeArr.Length < 7 || string.IsNullOrEmpty(codeArr[0]) || string.IsNullOrEmpty(codeArr[1]) || string.IsNullOrEmpty(codeArr[3]) || string.IsNullOrEmpty(codeArr[5]))
                {
                    //token错误
                    loginInfo.ErrorMessage = $"{TokenError}[{token}]";
                }
                else
                {
                    string timeStr = codeArr[0];
                    loginInfo.LastLoginTime = Convert.ToDateTime(timeStr);

                    if (expireMinutes > 0 && loginInfo.LastLoginTime.AddMinutes(expireMinutes) < DateTime.Now)
                    {
                        //超时1小时
                        loginInfo.ErrorMessage = LoginExpired;
                    }

                    loginInfo.UserName = codeArr[3];
                    loginInfo.UserCode = codeArr[1];
                    loginInfo.RealName = codeArr[3];
                    loginInfo.RoleID = Convert.ToInt32(codeArr[5]);

                    //登录信息保存cookie
                    Utils.SaveCookies(loginInfo);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(UserLoginInfo));
                loginInfo.ErrorMessage = ex.Message;
            }
            return loginInfo;
        }
    }
}
