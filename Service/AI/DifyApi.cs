using Model.AI;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Tool;

namespace Service.AI
{
    public class DifyStreamClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public DifyStreamClient(string apiKey, string baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task StreamChatCompletion(DifyChatRequest difyRequest, Action<string> onTextReceived)
        {
            var requestData = new
            {
                inputs = difyRequest.WindApiRequest,
                query = difyRequest.Query.Trim(),
                response_mode = "streaming",
                user = difyRequest.user
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl);
            request.Content = content;

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("data: "))
                {
                    var data = line.Substring(6);
                    if (data == "[DONE]")
                    {
                        break;
                    }

                    try
                    {
                        using var jsonDoc = JsonDocument.Parse(data);

                        // 处理不同类型的事件
                        if (jsonDoc.RootElement.TryGetProperty("event", out var eventProp))
                        {
                            var eventType = eventProp.GetString();

                            // 处理消息事件
                            if (eventType == "message" &&
                                jsonDoc.RootElement.TryGetProperty("answer", out var answerProp))
                            {
                                var contentText = answerProp.GetString();
                                if (!string.IsNullOrEmpty(contentText))
                                {
                                    onTextReceived(contentText);
                                }
                            }
                            // 处理其他类型的事件
                            else if (eventType == "workflow_started" ||
                                     eventType == "node_started" ||
                                     eventType == "node_finished")
                            {
                                // 可以在这里处理工作流和节点事件

                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        LoggerUtils.Error(ex.ToString(), typeof(DifyStreamClient));
                    }
                }
            }
        }



        // 流式返回IAsyncEnumerable版本
        public async IAsyncEnumerable<string> StreamChatCompletionAsync(DifyChatRequest difyRequest)
        {
            var requestData = new
            {
                inputs = difyRequest.WindApiRequest,
                query = difyRequest.Query.Trim(),
                response_mode = "streaming",
                user = difyRequest.user
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl);
            request.Content = content;

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("data: "))
                {
                    var data = line.Substring(6);
                    if (data == "[DONE]")
                        break;

                    // 提取JSON解析到单独的方法，避免在yield return所在的try块中使用catch
                    if (TryParseContent(data, out var contentText))
                    {
                        yield return contentText;
                    }
                }
            }
        }

        // 单独的JSON解析方法，处理异常
        private bool TryParseContent(string json, out string contentText)
        {
            contentText = null;
            try
            {
                using var jsonDoc = JsonDocument.Parse(json);
                if (jsonDoc.RootElement.TryGetProperty("answer", out var answerProp))
                {
                    contentText = answerProp.GetString();
                    return !string.IsNullOrEmpty(contentText);
                }
            }
            catch (JsonException ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(DifyStreamClient));
            }
            return false;
        }

        // 回调函数版本
        public async Task StreamChatCompletionWeb(DifyChatRequest difyRequest, Action<string> onTextReceived)
        {
            await foreach (var chunk in StreamChatCompletionAsync(difyRequest))
            {
                onTextReceived(chunk);
            }
        }


    }
}
