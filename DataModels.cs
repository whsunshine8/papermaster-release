using System;
using System.IO;
using Newtonsoft.Json;

namespace PaperMasterWin
{
    /// <summary>
    /// 论文项目数据结构
    /// </summary>
    public class PaperProject
    {
        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("level")]
        public string Level { get; set; } = "本科毕业论文";

        [JsonProperty("discipline")]
        public string Discipline { get; set; } = "工学 (计算机/电子信息/土木/机械/电气/自动化等)";

        [JsonProperty("major")]
        public string Major { get; set; } = "";

        [JsonProperty("outlineLevel")]
        public string OutlineLevel { get; set; } = "三级目录 (例如: 1.1.1 结构紧凑规范 - 推荐)";

        [JsonProperty("wordCount")]
        public string WordCount { get; set; } = "8,000 - 10,000 字 (仅正文纯字数，标准本科推荐)";

        [JsonProperty("template")]
        public string Template { get; set; } = "标准学术实证/工程设计模板 (绪论 - 理论与技术 - 现状分析 - 核心系统架构/模型设计 - 实现与验证 - 结论)";

        [JsonProperty("outlineContent")]
        public string OutlineContent { get; set; } = "";

        [JsonProperty("proposalContent")]
        public string ProposalContent { get; set; } = "";

        [JsonProperty("paperContent")]
        public string PaperContent { get; set; } = "";

        [JsonProperty("defenseSpeech")]
        public string DefenseSpeech { get; set; } = "";

        [JsonProperty("pptContent")]
        public string PptContent { get; set; } = "";

        [JsonProperty("currentStep")]
        public int CurrentStep { get; set; } = 1;

        [JsonProperty("savedDate")]
        public string SavedDate { get; set; } = "";

        public void SaveToDisk(string path)
        {
            SavedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public static PaperProject LoadFromDisk(string path)
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<PaperProject>(json) ?? new PaperProject();
            }
            return new PaperProject();
        }
    }

    /// <summary>
    /// 配置管理器
    /// </summary>
    public class AppConfig
    {
        [JsonProperty("baseUrl")]
        public string BaseUrl { get; set; } = "http://113.57.132.2:5678";

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; } = "";

        [JsonProperty("defaultModel")]
        public string DefaultModel { get; set; } = "";

        [JsonProperty("systemPrompt")]
        public string SystemPrompt { get; set; } = @"你是一位经验丰富的学术论文指导专家，擅长帮助本科生、硕士生撰写高质量的毕业论文。
你的任务是根据用户提供的论文信息，逐步生成：
1. 论文大纲（结构完整、层次清晰）
2. 开题报告（含研究背景、意义、方法、进度安排）
3. 论文正文（学术规范、逻辑严密、数据详实）
4. 答辩演讲稿（简洁明了、重点突出）
5. PPT大纲（要点精炼、适合演示）

请严格遵守以下要求：
- 使用规范的学术语言
- 保持逻辑连贯、结构清晰
- 标注引用和参考文献位置
- 输出内容应直接可用于Word文档
- 不要输出多余的解释性文字，只输出正文内容";

        [JsonProperty("maxTokens")]
        public int MaxTokens { get; set; } = 0;
    }

    public static class ConfigManager
    {
        private static string ConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PaperMaster", "config.json");
        private static string ProjectsDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PaperMaster", "projects");

        public static void EnsureDirectories()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            Directory.CreateDirectory(ProjectsDir);
        }

        public static AppConfig LoadConfig()
        {
            EnsureDirectories();
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
            }
            return new AppConfig();
        }

        public static void SaveConfig(AppConfig config)
        {
            EnsureDirectories();
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
        }

        public static List<string> GetHistoryFiles()
        {
            EnsureDirectories();
            var dir = new DirectoryInfo(ProjectsDir);
            return dir.GetFiles("PaperProject_*.json", SearchOption.TopDirectoryOnly)
                .OrderByDescending(f => f.LastWriteTime)
                .Select(f => f.Name)
                .ToList();
        }

        public static string GetProjectPath(string filename)
        {
            return Path.Combine(ProjectsDir, filename);
        }
    }
}
