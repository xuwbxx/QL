using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Tool;

namespace Service.Towing.Tool
{
    /// <summary>
    /// 海康威视调用iSC用的HttpUtilLib
    /// </summary>
    public class HikHttpUtilLib
    {
        public string HikVisionUrl { get; private set; }
        public string HikVisionUrlPort { get; private set; }
        public string HikVisionUrlIsHttps { get; private set; }
        public string HikActionPrefix { get; private set; }
        public string HikIsDebug { get; private set; }
        public string HikAppKey { get; private set; }
        public string HikAppSecret { get; private set; }
        public string HikIdxCode { get; private set; }

        public HikHttpUtilLib()
        {
            HikVisionUrl = AppSettingUtils.GetSetting("HikVision:HikVisionUrl") ?? "";
            HikVisionUrlPort = AppSettingUtils.GetSetting("HikVision:HikVisionUrlPort") ?? "443";
            HikVisionUrlIsHttps = AppSettingUtils.GetSetting("HikVision:HikVisionUrlIsHttps") ?? "true";
            HikActionPrefix = AppSettingUtils.GetSetting("HikVision:HikActionPrefix") ?? "";
            HikIsDebug = AppSettingUtils.GetSetting("HikVision:HikIsDebug") ?? "false";
            HikAppKey = AppSettingUtils.GetSetting("HikVision:HikAppKey") ?? "";
            HikAppSecret = AppSettingUtils.GetSetting("HikVision:HikAppSecret") ?? "";
            HikIdxCode = AppSettingUtils.GetSetting("HikVision:HikIdxCode") ?? "";
        }

        public async Task<T> CallApiAsync<T>(string action, object req, bool isPost = true)
        {
            var port = 443;
            var isHttps = true;
            var isDebug = false;
            int.TryParse(HikVisionUrlPort, out port);
            bool.TryParse(HikVisionUrlIsHttps, out isHttps);
            bool.TryParse(HikIsDebug, out isDebug);

            SetPlatformInfo(HikAppKey, HikAppSecret, HikVisionUrl, port, isHttps, isDebug, HikActionPrefix);

            var reqStr = JsonUtils.Serialize(req);
            var resBytes = await HttpPostAsync(action, reqStr, 100);
            if (resBytes == null)
            {
                SetError("No Response");
            }
            resBytes = resBytes ?? Array.Empty<byte>();
            var resStr = Encoding.UTF8.GetString(resBytes);

            return JsonUtils.Deserialize<T>(resStr);
        }

        /// <summary>
        /// 设置信息参数
        /// </summary>
        public void SetPlatformInfo(string appkey, string secret, string ip, int port = 443, bool isHttps = true, bool isDebug = false, string actionPrefix = "")
        {
            _appkey = appkey;
            _secret = secret;
            _ip = ip;
            _port = port;
            _isHttps = isHttps;
            _isDebug = isDebug;
            _actionPrefix = actionPrefix;
        }

        /// <summary>
        /// HTTP GET请求
        /// </summary>
        public async Task<byte[]> HttpGetAsync(string uri, int timeout)
        {
            var header = new Dictionary<string, string>();
            InitRequest(header, uri, "", false);

            var url = $"{(_isHttps ? "https://" : "http://")}{_ip}:{_port}{_actionPrefix}{uri}";

            using var handler = new HttpClientHandler();
            if (_isHttps)
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, error) => true;
            }
            using var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(timeout);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            foreach (var kvp in header)
            {
                if (kvp.Key.Contains("x-ca-"))
                {
                    request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }
            }

            var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else if (response.StatusCode == HttpStatusCode.Found || response.StatusCode == HttpStatusCode.Moved)
            {
                var redirectUrl = response.Headers.Location?.ToString();
                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    using var redirectClient = new HttpClient(handler);
                    return await redirectClient.GetByteArrayAsync(redirectUrl);
                }
            }

            return null;
        }

        /// <summary>
        /// HTTP Post请求
        /// </summary>
        public async Task<byte[]> HttpPostAsync(string uri, string body, int timeout)
        {
            var header = new Dictionary<string, string>();
            InitRequest(header, uri, body, true);

            var url = $"{(_isHttps ? "https://" : "http://")}{_ip}:{_port}{_actionPrefix}{uri}";

            using var handler = new HttpClientHandler();
            if (_isHttps)
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, error) => true;
            }
            using var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(timeout);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            foreach (var kvp in header)
            {
                if (kvp.Key.Contains("x-ca-"))
                {
                    request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(body))
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            try
            {
                var response = await client.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else if (response.StatusCode == HttpStatusCode.Found || response.StatusCode == HttpStatusCode.Moved)
                {
                    try
                    {
                        var redirectUrl = response.Headers.Location?.ToString();
                        if (!string.IsNullOrEmpty(redirectUrl))
                        {
                            using var redirectClient = new HttpClient(handler);
                            return await redirectClient.GetByteArrayAsync(redirectUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        SetError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                SetError("[ERROR] HttpPost 调用错。", ex);
            }

            return null;
        }

        private void InitRequest(Dictionary<string, string> header, string url, string body, bool isPost)
        {
            // Accept
            header.Add("Accept", "application/json");

            // ContentType
            header.Add("Content-Type", "application/json");

            if (isPost)
            {
                // content-md5
                var contentMd5 = ComputeContentMd5(body);
                header.Add("content-md5", contentMd5);
            }

            // x-ca-timestamp
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            header.Add("x-ca-timestamp", timestamp);

            // x-ca-nonce
            header.Add("x-ca-nonce", Guid.NewGuid().ToString());

            // x-ca-key
            header.Add("x-ca-key", _appkey);

            // build string to sign
            var strToSign = BuildSignString(isPost ? "POST" : "GET", _actionPrefix + url, header);
            var signedStr = ComputeHmacSha256(strToSign, _secret);

            // x-ca-signature
            header.Add("x-ca-signature", signedStr);

            Error = null;
        }

        private void SetError(string message, Exception ex = null)
        {
            Error = ex == null ? new Exception(message) : new Exception(message, ex);
        }

        private void SetError(Exception ex)
        {
            Error = new Exception("内部程序错误，详见innerException", ex);
        }

        /// <summary>
        /// 计算content-md5
        /// </summary>
        private static string ComputeContentMd5(string body)
        {
            using var md5 = MD5.Create();
            var result = md5.ComputeHash(Encoding.UTF8.GetBytes(body));
            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// 计算HMACSHA256
        /// </summary>
        private static string ComputeHmacSha256(string str, string secret)
        {
            var encoder = new UTF8Encoding();
            var secretBytes = encoder.GetBytes(secret);
            var strBytes = encoder.GetBytes(str);
            using var hmac = new HMACSHA256(secretBytes);
            var hashBytes = hmac.ComputeHash(strBytes);
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// 计算签名字符串
        /// </summary>
        private static string BuildSignString(string method, string url, Dictionary<string, string> header)
        {
            var sb = new StringBuilder();
            sb.Append(method.ToUpper()).Append("\n");

            if (header != null)
            {
                if (header.ContainsKey("Accept") && header["Accept"] != null)
                {
                    sb.Append(header["Accept"]);
                    sb.Append("\n");
                }

                if (header.ContainsKey("Content-MD5") && header["Content-MD5"] != null)
                {
                    sb.Append(header["Content-MD5"]);
                    sb.Append("\n");
                }

                if (header.ContainsKey("Content-Type") && header["Content-Type"] != null)
                {
                    sb.Append(header["Content-Type"]);
                    sb.Append("\n");
                }

                if (header.ContainsKey("Date") && header["Date"] != null)
                {
                    sb.Append(header["Date"]);
                    sb.Append("\n");
                }
            }

            // build and add header to sign
            var signHeader = BuildSignHeader(header);
            sb.Append(signHeader);
            sb.Append(url);
            return sb.ToString();
        }

        /// <summary>
        /// 计算签名头
        /// </summary>
        private static string BuildSignHeader(Dictionary<string, string> header)
        {
            var dic = from kvp in header orderby kvp.Key ascending select kvp;

            var sbSignHeader = new StringBuilder();
            var sb = new StringBuilder();

            foreach (var kvp in dic)
            {
                if (kvp.Key.Replace(" ", "").Contains("x-ca-"))
                {
                    sb.Append(kvp.Key + ":");
                    if (!string.IsNullOrWhiteSpace(kvp.Value))
                    {
                        sb.Append(kvp.Value);
                    }
                    sb.Append("\n");
                    if (sbSignHeader.Length > 0)
                    {
                        sbSignHeader.Append(",");
                    }
                    sbSignHeader.Append(kvp.Key);
                }
            }

            header.Add("x-ca-signature-headers", sbSignHeader.ToString());

            return sb.ToString();
        }

        // 平台参数
        private string _ip;
        private int _port = 443;
        private string _appkey;
        private string _secret;
        private bool _isHttps = true;
        private bool _isDebug = false;
        private string _actionPrefix = "";

        public Exception Error { private set; get; }
        public string ErrorMsg => Error == null ? "" : Error.Message;
    }
}
