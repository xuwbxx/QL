using Model.TechCenter.JJT;
using System.Security.Cryptography;
using System.Text;
using Tool;

namespace Service.TechCenter
{
    /// <summary>
    /// 技术中心信息平台，对接数字化部封装的接口（尽量用新的）
    /// </summary>
    public class TC_JJTService
    {

        private static string sysType = AppSettingUtils.GetSetting("DigitalCenter:SHJUser4A:TC_sysType");


        /// <summary>
        /// 交建通发信息
        /// </summary>
        /// <param name="Informs"></param>
        /// <returns></returns>
        public static async Task<bool> SendJJTMessage(ShjJJTMessageTCRequest request)
        {

            if (string.IsNullOrEmpty(request.title) || string.IsNullOrEmpty(request.content) || string.IsNullOrEmpty(request.url) || request.userlist.Count == 0)
            {
                return false;
            }

            try
            {

                string PostUrl = @"http://10.6.54.10:9098/JJT/SendMessage_V2";

                ShjJJTMessageTC inform = new ShjJJTMessageTC();
                inform.sysType = sysType;
                inform.msgtype = "textcard";
                inform.title = request.title;
                inform.userlist = request.userlist;
                inform.content = request.content;
                inform.url = request.url;
                var retJson = await HttpUtils.PostAsync(PostUrl, inform);

                var ret = JsonUtils.Deserialize<ShjJJTMessageTCResponse>(retJson);

                if (ret.StatusCode == 200)
                {
                    return true;
                }
                else
                {
                    return false;
                }


            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWind_JJTService));
                return false;
            }
        }



        /// <summary>
        /// PMIP系统单点登录用的参数解密公用类，用来解密参数,并验证时间是否过期,验证通过返回usercode
        /// </summary>
        /// <returns></returns>
        /// paramString包含了三个东西1.usercode 2.returnurl(用于单点之后的二次跳转) 3.客户端请求的timespan
        public static string AESSingleSignOn(string paramString, out SingleSinOnModel result)
        {
            result = new SingleSinOnModel();
            try
            {
                if (string.IsNullOrEmpty(paramString))
                {
                    return "参数为空,身份验证不通过";    //参数是空，那么返回false
                }
                paramString = paramString.Replace(" ", "+");
                DateTime nowTime = DateTime.Now;
                string AESKey = string.Format("{0}{1}{2}", nowTime.Year.ToString()
                    , nowTime.Month >= 10 ? nowTime.Month.ToString() : "0" + nowTime.Month.ToString()
                    , nowTime.Day >= 10 ? nowTime.Day.ToString() : "0" + nowTime.Day.ToString());
                AESKey = AESKey + AESKey;
                //AESKey= "1234123412ABCDEF";elhglkUnR9T1p94Byc1wvJjLYg636CE7wp/5Gw0icPx80Sr++vKn6wRMNq6zYwTQ
                string sourceString = AesDecrypt(paramString, AESKey);
                string[] sourceList = sourceString.Split(new Char[] { '$' }, StringSplitOptions.None);
                if (sourceList.Length == 4)
                {
                    //这个抓出来的数组一定要是三个对象
                    string sendTime = sourceList[3];
                    DateTime sendTimeD = Convert.ToDateTime(sendTime);
                    TimeSpan ts = DateTime.Now.Subtract(sendTimeD).Duration();
                    double wholeSecond = ts.TotalSeconds;   //算出当前时间比传接口时候的时间的差的秒数
                    if (wholeSecond >= -300000 && wholeSecond <= 300000)    //必须在五分钟之内
                    {
                        result.usrName = sourceList[0];
                        result.usrCode = sourceList[1];
                        return "";    //返回空表示身份验证通过
                    }
                    else
                    {

                        return "验证超时，身份验证不通过";
                    }
                }
                else
                {
                    return "解密失败，身份验证不通过";
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(SSOService));
            }
            return "身份验证不通过";
        }


        private static string AesDecrypt(string str, string key, string AES_IV = "ABCDEF1234123412")
        {
            byte[] inputBytes = HexStringToByteArray(str);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            // 替换过时的 AesCryptoServiceProvider 为 Aes.Create()
            using (Aes aesAlg = Aes.Create())
            {
                // 配置密钥（保持原有逻辑）
                aesAlg.Key = keyBytes;
                // 配置IV：截取前16字节（保持原有逻辑）
                aesAlg.IV = Encoding.UTF8.GetBytes(string.IsNullOrEmpty(AES_IV) ? "" : AES_IV.Substring(0, 16));
                // 保持默认模式和填充（AesCryptoServiceProvider默认是CBC模式+PKCS7填充，与Aes.Create()一致）
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // 创建解密器
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // 解密流程（保持原有逻辑不变）
                using (MemoryStream msEncrypt = new MemoryStream(inputBytes))
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srEncrypt = new StreamReader(csEncrypt, Encoding.UTF8))
                        {
                            // 读取解密后的明文
                            return srEncrypt.ReadToEnd();
                        }
                    }
                }
            }

            //using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            //{
            //    aesAlg.Key = keyBytes;
            //    aesAlg.IV = Encoding.UTF8.GetBytes(AES_IV == "" ? "" : AES_IV.Substring(0, 16));

            //    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            //    using (MemoryStream msEncrypt = new MemoryStream(inputBytes))
            //    {
            //        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, decryptor, CryptoStreamMode.Read))
            //        {
            //            using (StreamReader srEncrypt = new StreamReader(csEncrypt))
            //            {
            //                return srEncrypt.ReadToEnd();
            //            }
            //        }
            //    }
            //}

        }

        /// <summary>
        /// 将指定的16进制字符串转换为byte数组
        /// </summary>
        /// <param name="s">16进制字符串(如：“7F 2C 4A”或“7F2C4A”都可以)</param>
        /// <returns>16进制字符串对应的byte数组</returns>
        public static byte[] HexStringToByteArray(string s)
        {
            string dummyData = s.Replace("%", "").Replace(",", "").Replace(" ", "+");
            if (dummyData.Length % 4 > 0)
            {
                dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');
            }
            //MemoryStream stream = new MemoryStream(Convert.FromBase64String(dummyData));
            byte[] buffer = Convert.FromBase64String(dummyData);
            return buffer;
        }

    }


    /// <summary>
    /// 风电云服务APP，直接对接交建通应用
    /// </summary>
    public class CloudWind_JJTService
    {
        private static string SHJToken_corpid = AppSettingUtils.GetSetting("DigitalCenter:JJT:SHJToken_corpid");
        private static string SHJToken_corpsecret = AppSettingUtils.GetSetting("DigitalCenter:JJT:SHJToken_corpsecret");
        private static string CloudWindAgentID = AppSettingUtils.GetSetting("DigitalCenter:App:CloudWindAgentID");




        /// <summary>
        /// 获取交建通推送消息的token
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetShjToken()
        {
            string ret = "";
            try
            {
                var url = "https://jjt.ccccltd.cn/cgi-bin/gettoken?corpid=" + SHJToken_corpid + "&corpsecret=" + SHJToken_corpsecret;
                var JsonStr = await HttpUtils.GetAsync(url);
                var JsonRet = JsonUtils.Deserialize<ShjJJTToken>(JsonStr);

                if (JsonRet.errcode == 0)
                {
                    LoggerUtils.Info("获取accessToken成功", typeof(CloudWind_JJTService));
                    return JsonRet.access_token;
                }
                else
                {
                    LoggerUtils.Info("获取accessToken失败：" + JsonStr, typeof(CloudWind_JJTService));
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWind_JJTService));
                throw;
            }
            return ret;
        }


        /// <summary>
        /// 交建通发信息
        /// </summary>
        /// <param name="Informs"></param>
        /// <returns></returns>
        public static async Task<bool> SendJJTMessage(List<ShjJJTInform> Informs, int CloudWindAgentID)
        {

            if (Informs == null || Informs.Count == 0 || CloudWindAgentID == 0)
            {
                return false;
            }

            try
            {
                var AccessToken = await GetShjToken();
                if (string.IsNullOrEmpty(AccessToken))
                {
                    return false;
                }

                foreach (var item in Informs)
                {
                    if (string.IsNullOrEmpty(item.UserCode) || string.IsNullOrEmpty(item.Url) || string.IsNullOrEmpty(item.Title) || string.IsNullOrEmpty(item.Content))
                    {
                        continue;
                    }

                    ShjJJTMessageRequest request = new ShjJJTMessageRequest();
                    request.touser = item.UserCode;
                    request.toparty = "";
                    request.totag = "";
                    request.msgtype = "textcard";
                    request.agentid = CloudWindAgentID;
                    request.textcard.title = item.Title;
                    request.textcard.description = item.Content;
                    request.textcard.url = item.Url;

                    var PostUrl = "https://jjt.ccccltd.cn/cgi-bin/message/send?access_token=" + AccessToken;

                    var ret = await HttpUtils.PostAsync(PostUrl, request);

                }

                return true;

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWind_JJTService));
                throw;
            }
        }


    }


    public class SingleSinOnModel
    {
        public string? usrName { set; get; }
        public string? usrCode { get; set; }
        public string? businessUrl { get; set; }
    }
}
