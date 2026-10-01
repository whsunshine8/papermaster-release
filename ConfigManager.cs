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
        if (File.Exists(ConfigFile))
        {
            try
            {
                var json = File.ReadAllText(ConfigFile);
                return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
            }
            catch
            {
                return new AppConfig();
            }
        }
        return new AppConfig();
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
