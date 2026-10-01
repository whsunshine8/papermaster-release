using System;
using System.IO;
using Newtonsoft.Json;

namespace PaperMasterWin;

public class AppConfig
{
    [JsonProperty("api_key")]
    public string ApiKey { get; set; } = string.Empty;

    [JsonProperty("api_url")]
    public string ApiUrl { get; set; } = "http://token.cnkiedu.cn/v1/chat/completions";

    [JsonProperty("model")]
    public string Model { get; set; } = "qwen3-235b-a22b";

    [JsonProperty("project_directory")]
    public string ProjectDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PaperMaster";
}

public class ConfigManager
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PaperMaster");
    private static readonly string ConfigFile = Path.Combine(ConfigDir, "config.json");

    public AppConfig Load()
    {
        AppConfig cfg = new AppConfig();
        if (File.Exists(ConfigFile))
        {
            try
            {
                var json = File.ReadAllText(ConfigFile);
                cfg = JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
            }
            catch
            {
                cfg = new AppConfig();
            }
        }

        // 关键自动迁移：若用户本地历史 config.json 存有旧域名或旧IP，自动平滑升级为最新默认地址并保存
        if (string.IsNullOrWhiteSpace(cfg.ApiUrl) || 
            cfg.ApiUrl.Contains("113.57.132.2") || 
            cfg.ApiUrl.Contains("aiapi.whsunshine.link"))
        {
            cfg.ApiUrl = "http://token.cnkiedu.cn/v1/chat/completions";
            Save(cfg);
        }

        return cfg;
    }

    public void Save(AppConfig config)
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigFile, json);
        }
        catch { }
    }
}
