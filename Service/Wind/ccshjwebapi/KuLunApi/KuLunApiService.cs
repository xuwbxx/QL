using Model.Base;
using Model.TechApi.KuLun;
using System.Security.Cryptography;
using System.Text;
using Tool;

namespace BIM.Business.CCSHJWebApi.KuLunApi
{

    public class CryptographyExtUtils
    {

        // ===================== 固定配置（和下游厂商必须完全一致） =====================
        // 密钥 Key：必须是 32 字节（AES-256），UTF8 编码后长度固定
        private const string FixedKey = "9s7G2pQ5kLzX8cR6aT4bV1mN0dF3hJ7w";
        // 向量 IV：必须是 16 字节，固定不变
        private const string FixedIV = "P8s2Kf5zBn7gQr9x";
        // ============================================================================

        private static readonly byte[] _keyBytes = Encoding.UTF8.GetBytes(FixedKey);
        private static readonly byte[] _ivBytes = Encoding.UTF8.GetBytes(FixedIV);

        /// <summary>
        /// 加密
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            using (var aes = Aes.Create())
            {
                aes.Key = _keyBytes;
                aes.IV = _ivBytes;
                aes.Mode = CipherMode.CBC;       // 固定模式
                aes.Padding = PaddingMode.PKCS7; // 固定填充

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                    sw.Flush();
                    cs.FlushFinalBlock();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            using (var aes = Aes.Create())
            {
                aes.Key = _keyBytes;
                aes.IV = _ivBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

    }

    public class KuLunApiService
    {
        private static string KuLunServerAbled = AppSettingUtils.GetSetting("AppSettings:KuLun:KuLunServerAbled").ToString();
        private static string KuLunServerPostUrl = AppSettingUtils.GetSetting("AppSettings:KuLun:KuLunServerUrl").ToString();

        public static async Task<BaseReturn> queryProjects(WindCloudApiKuLunRequest request)
        {
            BaseReturn ret = new BaseReturn();
            try
            {
                if (KuLunServerAbled.Equals("0"))
                {
                    ret.Success = false;
                    ret.Message = "接口已经关闭";
                    return ret;
                }

                if (string.IsNullOrEmpty(request.Token))
                {
                    ret.Success = false;
                    ret.Message = "没有Token";
                    return ret;
                }

                KuLunReturn<List<KuLunProject>> projects = new KuLunReturn<List<KuLunProject>>();

                string ApiUrl = KuLunServerPostUrl + @"/apis/queryProjects";

                string JsonStr = await HttpUtils.GetAsync(ApiUrl);
                if (string.IsNullOrEmpty(JsonStr))
                {
                    ret.Success = false;
                    ret.Message = "没有任何数据";
                    return ret;
                }

                projects = JsonUtils.Deserialize<KuLunReturn<List<KuLunProject>>>(JsonStr);

                if (projects == null || projects.errcode != 0)
                {
                    ret.Success = false;
                    ret.Message = "查询发生错误";
                    return ret;
                }

                ret.Success = true;
                ret.Data = projects.data;

                return ret;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(KuLunApiService));
                ret.Success = false;
                ret.Message = "发生错误";
                return ret;
            }
        }

        public static async Task<BaseReturn> queryMonitorPoints(WindCloudApiKuLunRequest request)
        {
            BaseReturn ret = new BaseReturn();
            try
            {

                if (KuLunServerAbled.Equals("0"))
                {
                    ret.Success = false;
                    ret.Message = "接口已经关闭";
                    return ret;
                }

                if (string.IsNullOrEmpty(request.Token))
                {
                    ret.Success = false;
                    ret.Message = "没有Token";
                    return ret;
                }

                if (request.DataRequest.pid == 0)
                {
                    ret.Success = false;
                    ret.Message = "项目编号是0";
                    return ret;
                }

                KuLunReturn<List<KuLunProjectPoint>> projectPoints = new KuLunReturn<List<KuLunProjectPoint>>();

                string ApiUrl = KuLunServerPostUrl + @"/apis/queryMonitorPoints" + "?pid=" + request.DataRequest.pid;

                string JsonStr = await HttpUtils.GetAsync(ApiUrl);
                if (string.IsNullOrEmpty(JsonStr))
                {
                    ret.Success = false;
                    ret.Message = "没有任何数据";
                    return ret;
                }

                projectPoints = JsonUtils.Deserialize<KuLunReturn<List<KuLunProjectPoint>>>(JsonStr);

                if (projectPoints == null || projectPoints.errcode != 0)
                {
                    ret.Success = false;
                    ret.Message = "查询发生错误";
                    return ret;
                }

                ret.Success = true;
                ret.Data = projectPoints.data;

                return ret;

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(KuLunApiService));
                throw;
            }
        }


        public static string CreateToken()
        {
            string code = Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "_" + Guid.NewGuid().ToString();

            return CryptographyExtUtils.Encrypt(code);

        }

        public static string ValidateToken(string token)
        {
            string ret = "";
            if (string.IsNullOrEmpty(token))
            {
                return "token是空值";
            }
            string code = CryptographyExtUtils.Decrypt(token);
            string[] codeArr = code.Split('_');
            if (codeArr.Length != 3)
            {
                return "token格式不正确";
            }
            DateTime time = Convert.ToDateTime(codeArr[1]);
            if (time.AddDays(1) <= DateTime.Now)
            {
                return "token时间过期了";
            }
            return ret;
        }


        public static async Task<string> queryProjectsOrig()
        {
            try
            {
                string ApiUrl = KuLunServerPostUrl + @"/apis/queryProjects";

                string JsonStr = await HttpUtils.GetAsync(ApiUrl);

                return JsonStr;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(KuLunApiService));
                return "发生错误, 请联系技术中心。";
            }
        }

        public static async Task<string> queryMonitorPointsOrig(int pid)
        {
            try
            {
                string ApiUrl = KuLunServerPostUrl + @"/apis/queryMonitorPoints?pid=" + pid;

                string JsonStr = await HttpUtils.GetAsync(ApiUrl);

                return JsonStr;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(KuLunApiService));
                return "发生错误, 请联系技术中心。";
            }
        }

        public static async Task<string> batchImportMonitorRecordsOrig(KuLunRequest request)
        {
            try
            {
                string ApiUrl = KuLunServerPostUrl + @"/apis/batchImportMonitorRecords";

                string JsonStr = await HttpUtils.PostAsync(ApiUrl, request);

                return JsonStr;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(KuLunApiService));
                return "发生错误, 请联系技术中心。";
            }
        }
    }
}
