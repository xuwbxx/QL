using System.Text;
using System.Text.Json;

namespace Tool
{
    public class HttpUtils
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        //public HttpUtils(HttpClient httpClient)
        //{
        //    _httpClient = httpClient;
        //}

        public static async Task<string> GetAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                // 只记录详细的技术日志，供排查问题
                LoggerUtils.Error($"HTTP请求失败详情: {ex}", typeof(HttpUtils));
                // 抛出业务异常，不记录Error级别日志
                throw new ApplicationException("调用外部接口失败", ex);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error($"请求处理异常详情: {ex}", typeof(HttpUtils));
                throw;
            }
        }

        public static string GetSync(string url)
        {
            try
            {
                var response = _httpClient.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (HttpRequestException ex)
            {
                LoggerUtils.Error($"HTTP请求失败详情: {ex}", typeof(HttpUtils));
                throw new ApplicationException("调用外部接口失败", ex);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error($"请求处理异常详情: {ex}", typeof(HttpUtils));
                throw;
            }
        }

        public static async Task<string> PostAsync<T>(string url, T data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                // 只记录详细的技术日志，供排查问题
                LoggerUtils.Error($"HTTP请求失败详情: {ex}", typeof(HttpUtils));
                // 抛出业务异常，不记录Error级别日志
                throw new ApplicationException("调用外部接口失败", ex);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error($"请求处理异常详情: {ex}", typeof(HttpUtils));
                throw;
            }
        }

        public static string PostSync<T>(string url, T data)
        {
            try
            {
                // 与异步方法一致的JSON序列化逻辑
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 同步执行POST请求（替换异步的await PostAsync）
                var response = _httpClient.PostAsync(url, content).Result;
                // 同步验证响应状态码（与异步的EnsureSuccessStatusCode逻辑一致）
                response.EnsureSuccessStatusCode();
                // 同步读取响应内容（替换异步的await ReadAsStringAsync）
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (HttpRequestException ex)
            {
                // 保持原日志记录和业务异常抛出逻辑
                LoggerUtils.Error($"HTTP请求失败详情: {ex}", typeof(HttpUtils));
                throw new ApplicationException("调用外部接口失败", ex);
            }
            // 捕获序列化、网络异常等其他异常（与原异步方法一致）
            catch (Exception ex)
            {
                LoggerUtils.Error($"请求处理异常详情: {ex}", typeof(HttpUtils));
                throw;
            }
        }

        public static string HttpsPostSync<T>(string url, T data)
        {
            try
            {
                // 与PostSync一致的JSON序列化逻辑
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // HttpClient默认支持HTTPS，使用PostAsync即可
                var response = _httpClient.PostAsync(url, content).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (HttpRequestException ex)
            {
                LoggerUtils.Error($"HTTPS请求失败详情: {ex}", typeof(HttpUtils));
                throw new ApplicationException("调用外部HTTPS接口失败", ex);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error($"HTTPS请求处理异常详情: {ex}", typeof(HttpUtils));
                throw;
            }
        }

        public static async Task<string> PostAsyncTest<T>(string url, T data, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 设置合理的超时时间
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(30)); // 默认30秒超时

                LoggerUtils.Info(json, typeof(HttpUtils));

                var response = await _httpClient.PostAsync(url, content, cts.Token);

                LoggerUtils.Info(JsonUtils.Serialize(response), typeof(HttpUtils));

                // 更详细的状态码检查
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    LoggerUtils.Info($"HTTP请求失败，状态码: {response.StatusCode}, 错误内容: {errorContent}", typeof(HttpUtils));
                    response.EnsureSuccessStatusCode(); // 抛出异常
                }

                return await response.Content.ReadAsStringAsync(cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                LoggerUtils.Error($"请求被取消: {ex.Message}", typeof(HttpUtils));
                throw; // 重新抛出取消异常，让调用者处理
            }
            catch (HttpRequestException ex)
            {
                LoggerUtils.Error($"HTTP请求异常: {ex.Message}, 状态码: {ex.StatusCode}", typeof(HttpUtils));
                throw;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error($"处理请求时发生未知异常: {ex}", typeof(HttpUtils));
                throw;
            }
        }


        //public void Test()
        //{
        //    using (var httpClient = new HttpClient())
        //    {
        //        // 创建HttpService实例并传入HttpClient
        //        var HttpUtils = new HttpUtils(httpClient);

        //        WebApiRequest request = new WebApiRequest();
        //        request.DataBase = DatabaseType.Wind.ToString();
        //        request.SaftSql = @"SELECT * FROM Wind_Project WHERE IsDelete = 0";


        //        string Url = @"http://10.6.74.208:8031/CCSHJ/ApiTest/SqlExecute";

        //        // 调用PostAsync方法，记得替换成实际的URL
        //        var result = await httpClient.PostAsync(Url, request);



        //    }
        //}

    }
}
