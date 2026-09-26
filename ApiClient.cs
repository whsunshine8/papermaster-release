using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PaperMasterWin
{
    /// <summary>
    /// API客户端 - 对接OpenAI兼容的LLM接口
    /// </summary>
    public class ApiClient
    {
        private static readonly HttpClient client = new HttpClient()
        {
            Timeout = TimeSpan.FromMinutes(3)
        };

        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// 标准化基础URL
        /// </summary>
        public static string NormalizeBaseUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "http://113.57.132.2:5678/v1";

            url = url.Trim();
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "http://" + url;

            // 移除末尾斜杠
            while (url.EndsWith("/"))
                url = url.Substring(0, url.Length - 1);

            // 移除 /chat/completions
            if (url.EndsWith("/chat/completions"))
                url = url.Substring(0, url.Length - "/chat/completions".Length);

            // 确保以 /v1 结尾
            if (!url.EndsWith("/v1"))
                url += "/v1";

            return url;
        }

        /// <summary>
        /// 获取可用模型列表
        /// </summary>
        public static async Task<List<string>> FetchModelsAsync(string baseUrl, string apiKey, CancellationToken ct = default)
        {
            string normUrl = NormalizeBaseUrl(baseUrl) + "/models";
            var request = new HttpRequestMessage(HttpMethod.Get, normUrl);
            if (!string.IsNullOrEmpty(apiKey))
                request.Headers.Add("Authorization", "Bearer " + apiKey.Trim());

            var response = await client.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(ct);
            var root = JObject.Parse(body);

            var models = new List<string>();
            if (root["data"] is JArray data && data != null)
            {
                foreach (var item in data)
                {
                    if (item["id"] != null)
                        models.Add(item["id"].ToString());
                }
            }

            if (models.Count == 0)
                models.Add("gemini-3.6-flash-low");

            return models;
        }

        /// <summary>
        /// 调用聊天接口
        /// </summary>
        public static async Task<string> CallChatAsync(
            string baseUrl, string apiKey, string model,
            string systemPrompt, string userPrompt,
            int maxTokens = 0, CancellationToken ct = default)
        {
            string normUrl = NormalizeBaseUrl(baseUrl) + "/chat/completions";

            var payload = new JObject
            {
                ["model"] = model,
                ["temperature"] = 0.7,
                ["messages"] = new JArray()
            };

            if (maxTokens > 0)
                payload["max_tokens"] = maxTokens;

            var messages = (JArray)payload["messages"];

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                messages.Add(new JObject
                {
                    ["role"] = "system",
                    ["content"] = systemPrompt
                });
            }

            messages.Add(new JObject
            {
                ["role"] = "user",
                ["content"] = userPrompt
            });

            var json = payload.ToString();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, normUrl)
            {
                Content = content
            };

            if (!string.IsNullOrEmpty(apiKey))
                request.Headers.Add("Authorization", "Bearer " + apiKey.Trim());

            var response = await client.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(ct);
            var root = JObject.Parse(body);

            if (root["choices"] is JArray choices && choices != null && choices.Count > 0)
            {
                var choice = (JObject)choices[0];
                if (choice["message"] is JObject msg && msg["content"] != null)
                    return msg["content"].ToString();
            }

            throw new Exception("返回格式异常: choices为空");
        }
    }
}
