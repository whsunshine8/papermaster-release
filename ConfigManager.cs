using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PaperMasterWin
{
    /// <summary>
    /// 应用程序配置管理
    /// </summary>
    public class AppConfig
    {
        [JsonProperty("api_url")]
        public string BaseUrl { get; set; } = "https://api.seniverse.com/v1/chat";

        [JsonProperty("api_key")]
        public string ApiKey { get; set; } = "";

        [JsonProperty("default_model")]
        public string DefaultModel { get; set; } = "qwen3.6-35b";

        [JsonProperty("system_prompt")]
        public string SystemPrompt { get; set; } = "你是专业的学术论文生成助手。请根据用户需求生成高质量、格式规范的学术论文。使用规范的学术语言，包含必要的图表引用和参考文献。";

        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; } = 16000;

        [JsonProperty("temperature")]
        public double Temperature { get; set; } = 0.7;
    }

    /// <summary>
    /// 配置管理器
    /// </summary>
    public class ConfigManager
    {
        private static string ConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PaperMasterWin", "config.json");

        public static AppConfig LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载配置失败: {ex.Message}");
            }
            return new AppConfig();
        }

        public static void SaveConfig(AppConfig config)
        {
            try
            {
                EnsureDirectories();
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置失败: {ex.Message}");
                throw;
            }
        }

        public static void EnsureDirectories()
        {
            var dir = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public static List<string> GetHistoryFiles()
        {
            var historyDir = GetHistoryDir();
            if (!Directory.Exists(historyDir))
                return new List<string>();

            var files = new List<string>();
            foreach (var file in Directory.GetFiles(historyDir, "PaperProject_*.json"))
            {
                files.Add(Path.GetFileName(file));
            }
            files.Sort();
            files.Reverse(); // 最新的在前
            return files;
        }

        public static string GetHistoryDir()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PaperMasterWin", "projects");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        public static string GetProjectPath(string filename)
        {
            return Path.Combine(GetHistoryDir(), filename);
        }
    }
}
