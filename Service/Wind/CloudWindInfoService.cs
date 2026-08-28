using Model.Tech.Cloud;
using Model.Tech.System;
using Service.Shj;
using System.Security.Cryptography;
using System.Text;
using Tool;

namespace Service.Wind
{
    public class CloudWindInfoService
    {
        private string DoJJTInform = AppSettingUtils.GetSetting("DigitalCenter:JJT:DoJJTInform");

        /// <summary>
        /// PMIP系统单点登录用的参数解密公用类，用来解密参数,并验证时间是否过期,验证通过返回usercode
        /// </summary>
        /// <returns></returns>
        /// paramString包含了三个东西1.usercode 2.returnurl(用于单点之后的二次跳转) 3.客户端请求的timespan
        public string AESSingleSignOn(string paramString, out SingleSinOnModel result)
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
            catch (Exception ee)
            {
                LoggerUtils.Error(ee.ToString(), typeof(CloudWindInfoService));
            }
            return "身份验证不通过";
        }

        /// <summary>
        ///  AES 解密
        /// </summary>
        /// <param name="str">明文（待解密）</param>
        /// <param name="key">密文</param>
        /// <returns></returns>
        private string AesDecrypt(string str, string key, string AES_IV = "ABCDEF1234123412")
        {
            byte[] inputBytes = HexStringToByteArray(str);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = Encoding.UTF8.GetBytes(AES_IV == "" ? "" : AES_IV.Substring(0, 16));

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream(inputBytes))
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srEncrypt = new StreamReader(csEncrypt))
                        {
                            return srEncrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 将指定的16进制字符串转换为byte数组
        /// </summary>
        /// <param name="s">16进制字符串(如："7F 2C 4A"或"7F2C4A"都可以)</param>
        /// <returns>16进制字符串对应的byte数组</returns>
        public byte[] HexStringToByteArray(string s)
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


        /// <summary>
        /// 通过三航局用户编号，获取用户信息
        /// </summary>
        /// <param name="User4ACode"></param>
        /// <returns></returns>
        public CloudBaseInfoResponse<List<CloudBaseUserInfo>> GetShjUserInfo(string User4ACode)
        {
            CloudBaseInfoResponse<List<CloudBaseUserInfo>> ret = new CloudBaseInfoResponse<List<CloudBaseUserInfo>>();
            try
            {
                string PostUrl = @"http://10.6.54.10:9098/In_Emp/QueryEmpBy4ACode";
                CloudBaseInfoRequest request = new CloudBaseInfoRequest();
                request.User4ACode = User4ACode;
                request.OID = User4ACode;
                var JsonRet = HttpUtils.PostSync(PostUrl, request);
                ret = JsonUtils.Deserialize<CloudBaseInfoResponse<List<CloudBaseUserInfo>>>(JsonRet);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWindInfoService));
            }
            return ret;
        }

        /// <summary>
        /// 通过关键字查询三航局用户(模糊搜索)
        /// </summary>
        /// <param name="Name"></param>
        public CloudBaseInfoResponse<List<CloudBaseUserInfo>> GetShjUserInfoByName(string Name)
        {
            CloudBaseInfoResponse<List<CloudBaseUserInfo>> ret = new CloudBaseInfoResponse<List<CloudBaseUserInfo>>();
            try
            {
                //
                string PostUrl = @"http://10.6.54.10:9098/In_Emp/QueryEmpByName";
                CloudBaseInfoRequest request = new CloudBaseInfoRequest();
                request.UserName = Name;

                var JsonRet = HttpUtils.PostSync(PostUrl, request);
                ret = JsonUtils.Deserialize<CloudBaseInfoResponse<List<CloudBaseUserInfo>>>(JsonRet);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWindInfoService));
            }
            return ret;
        }


        /// <summary>
        /// 保存用户登录云平台记录
        /// </summary>
        public void SaveUserLoginInfo()
        {

        }


        public string GetShjToken()
        {
            string ret = "";
            try
            {
                var SHJToken_corpid = AppSettingUtils.GetSetting("DigitalCenter:JJT:SHJToken_corpid");
                var SHJToken_corpsecret = AppSettingUtils.GetSetting("DigitalCenter:JJT:SHJToken_corpsecret");
                var url = "https://jjt.ccccltd.cn/cgi-bin/gettoken?corpid=" + SHJToken_corpid + "&corpsecret=" + SHJToken_corpsecret;
                var JsonStr = HttpUtils.GetSync(url);
                var JsonRet = JsonUtils.Deserialize<ShjTokenModel>(JsonStr);

                if (JsonRet.errcode == 0)
                {
                    LoggerUtils.Info("获取accessToken成功", typeof(CloudWindInfoService));
                    return JsonRet.access_token;
                }
                else
                {
                    LoggerUtils.Info("获取accessToken失败" + JsonStr, typeof(CloudWindInfoService));
                }

            }
            catch (Exception ex)
            {
                ret = "";
                LoggerUtils.Error(ex.ToString(), typeof(CloudWindInfoService));
            }
            return ret;
        }


        /// <summary>
        /// 交建通发信息
        /// </summary>
        /// <param name="Informs"></param>
        /// <returns></returns>
        public async Task<bool> SendJJTMessage(List<SystemJJTInform> Informs)
        {
            if (DoJJTInform.Equals("0"))
            {
                return false;
            }

            if (Informs == null || Informs.Count == 0)
            {
                return false;
            }

            try
            {
                var AccessToken = GetShjToken();
                if (string.IsNullOrEmpty(AccessToken))
                {
                    return false;
                }
                var CloudWindAgentID = Convert.ToInt32(AppSettingUtils.GetSetting("DigitalCenter:App:CloudWindAgentID"));

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

                    var ret = HttpUtils.HttpsPostSync(PostUrl, request);

                }

                return true;

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWindInfoService));
                return false;
            }
        }


        /// <summary>
        /// 船舶可作业预报消息推送
        /// </summary>
        /// <param name="AccessCode"></param>
        /// <param name="UserID"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public bool SendJJTMessageForKZYReport(string AccessToken, string UserID, string Url, string TaskCode)
        {
            if (DoJJTInform.Equals("0"))
            {
                return false;
            }

            if (string.IsNullOrEmpty(UserID) || string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(AccessToken))
            {
                return false;
            }

            try
            {

                var CloudWindAgentID = Convert.ToInt32(AppSettingUtils.GetSetting("DigitalCenter:App:CloudWindAgentID"));

                ShjJJTMessageRequest request = new ShjJJTMessageRequest();
                request.touser = UserID;
                request.toparty = "";
                request.totag = "";
                request.msgtype = "textcard";
                request.agentid = CloudWindAgentID;
                request.textcard.title = "风电云服务消息通知";
                request.textcard.description = DateTime.Now.ToString("yyyy-MM-dd") + "起重船基础施工可作业性预报文件(" + TaskCode + ")(点击获取)\r\n（" + DateTime.Now.AddDays(30).ToString("yyyy-MM-dd HH:mm") + " 前链接有效)";
                request.textcard.url = Url;

                var PostUrl = "https://jjt.ccccltd.cn/cgi-bin/message/send?access_token=" + AccessToken;

                var JsonStr = HttpUtils.HttpsPostSync(PostUrl, request);

                var JsonRet = JsonUtils.Deserialize<ShjTokenModel>(JsonStr);


                if (JsonRet.errcode == 0)
                {
                    LoggerUtils.Info(UserID + "-" + TaskCode + ": 推送成功。", typeof(CloudWindInfoService));
                    return true;
                }
                else
                {
                    LoggerUtils.Info(UserID + "-" + TaskCode + ": 推送失败。" + JsonStr, typeof(CloudWindInfoService));
                    return false;
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWindInfoService));
                return false;
            }
        }

    }
}
