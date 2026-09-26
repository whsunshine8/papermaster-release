using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace PaperMasterWin;

/// <summary>
/// API 响应结构
/// </summary>
public class ApiResponse
{
    public Choice[]? choices { get; set; }
}

public class Choice
{
    public Message? message { get; set; }
}

public class Message
{
    public string? content { get; set; }
}

/// <summary>
/// 应用配置
/// </summary>
public class AppConfig
{
    [JsonProperty("api_key")]
    public string ApiKey { get; set; } = string.Empty;

    [JsonProperty("api_url")]
    public string ApiUrl { get; set; } = "https://aiapi.whsunshine.link/v1/chat/completions";

    [JsonProperty("model")]
    public string Model { get; set; } = "qwen3-235b-a22b";

    [JsonProperty("project_directory")]
    public string ProjectDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PaperMaster";
}

/// <summary>
/// 配置管理器
/// </summary>
public class ConfigManager
{
    private readonly string _configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PaperMaster", "config.json");

    public AppConfig Load()
    {
        if (File.Exists(_configPath))
        {
            try
            {
                var json = File.ReadAllText(_configPath);
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
        Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
        var json = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText(_configPath, json);
    }
}

/// <summary>
/// 主窗口
/// </summary>
public partial class MainWindow : Window
{
    private static readonly HttpClient _client = new HttpClient();
    private ConfigManager _configManager;
    private AppConfig _config;

    public MainWindow()
    {
        InitializeComponent();
        _configManager = new ConfigManager();
        _config = _configManager.Load();
    }

    private async Task<string> CallApiAsync(string prompt)
    {
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            MessageBox.Show("请先配置 API Key", "提示",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return string.Empty;
        }

        var requestBody = new
        {
            model = _config.Model,
            messages = new object[]
            {
                new { role = "system", content = "你是一个专业的论文写作助手。" },
                new { role = "user", content = prompt }
            },
            temperature = 0.7
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");

        try
        {
            var response = await _client.PostAsync(_config.ApiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var parsed = JsonConvert.DeserializeObject<ApiResponse>(result);
                var text = parsed?.choices?[0]?.message?.content;
                return text ?? "未收到有效回复";
            }
            else
            {
                return $"API 错误: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            return $"请求失败: {ex.Message}";
        }
    }

    private async void BtnStep1_Click(object? sender, RoutedEventArgs e)
    {
        var title = TitleInput.Text;
        var research = ResearchInput.Text;
        EtTaskbook.Text = "正在生成任务书，请稍候...";

        var prompt = $"论文题目: {title}\n研究方向: {research}\n\n请生成论文任务书，包含研究背景、研究目标、研究内容、技术路线、进度安排等。";
        var result = await CallApiAsync(prompt);
        EtTaskbook.Text = result ?? "生成失败";
    }

    private async void BtnStep2_Click(object? sender, RoutedEventArgs e)
    {
        var title = TitleInput.Text;
        var research = ResearchInput.Text;
        EtProposal.Text = "正在生成开题报告，请稍候...";

        var prompt = $"论文题目: {title}\n研究方向: {research}\n\n请生成开题报告，包含研究意义、国内外现状、研究内容、研究方法、预期成果等。";
        var result = await CallApiAsync(prompt);
        EtProposal.Text = result ?? "生成失败";
    }

    private async void BtnStep3_Click(object? sender, RoutedEventArgs e)
    {
        var title = TitleInput.Text;
        var research = ResearchInput.Text;
        EtLitReview.Text = "正在生成文献综述，请稍候...";

        var prompt = $"论文题目: {title}\n研究方向: {research}\n\n请生成文献综述，包含相关领域研究现状、主要观点对比、研究空白分析等。";
        var result = await CallApiAsync(prompt);
        EtLitReview.Text = result ?? "生成失败";
    }

    private async void BtnStep4_Click(object? sender, RoutedEventArgs e)
    {
        var title = TitleInput.Text;
        var research = ResearchInput.Text;
        EtPaperContent.Text = "正在生成论文正文，请稍候...";

        var prompt = $"论文题目: {title}\n研究方向: {research}\n\n请生成论文正文，包含引言、相关工作、方法、实验、结果分析、结论等章节。";
        var result = await CallApiAsync(prompt);
        EtPaperContent.Text = result ?? "生成失败";
    }

    private async void BtnStep5_Click(object? sender, RoutedEventArgs e)
    {
        var title = TitleInput.Text;
        var research = ResearchInput.Text;
        EtDefensePpt.Text = "正在生成答辩PPT大纲，请稍候...";

        var prompt = $"论文题目: {title}\n研究方向: {research}\n\n请生成答辩PPT大纲，包含封面、目录、研究背景、方法、结果、结论、致谢等页面内容。";
        var result = await CallApiAsync(prompt);
        EtDefensePpt.Text = result ?? "生成失败";
    }

    private void BtnConfig_Click(object? sender, RoutedEventArgs e)
    {
        var dlg = new ConfigDialog(_config);
        dlg.Owner = this;
        dlg.ShowDialog();
        _config = _configManager.Load();
    }

    private async void BtnSaveDocx_Click(object? sender, RoutedEventArgs e)
    {
        var saveDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Word文档 (*.docx)|*.docx",
            Title = "保存论文文档"
        };

        if (saveDialog.ShowDialog() == true)
        {
            await Task.Run(() =>
            {
                using var doc = WordprocessingDocument.Create(
                    saveDialog.FileName, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
                var mainPart = doc.AddMainDocumentPart();
                var body = new Body();

                // Title
                var heading = new Paragraph();
                var run1 = new Run();
                var props1 = new RunProperties();
                props1.Append(new RunFonts { Ascii = "Arial" });
                props1.Append(new FontSize { Val = "48" });
                run1.Append(props1);
                run1.Append(new Text(TitleInput.Text));
                heading.Append(run1);
                body.Append(heading);

                // Steps
                var steps = new[]
                {
                    ("任务书", EtTaskbook.Text),
                    ("开题报告", EtProposal.Text),
                    ("文献综述", EtLitReview.Text),
                    ("论文正文", EtPaperContent.Text),
                    ("答辩PPT", EtDefensePpt.Text)
                };

                foreach (var (name, text) in steps)
                {
                    if (string.IsNullOrEmpty(text)) continue;

                    var secHeading = new Paragraph();
                    var secRun = new Run();
                    var secProps = new RunProperties();
                    secProps.Append(new FontSize { Val = "32" });
                    secRun.Append(secProps);
                    secRun.Append(new Text(name));
                    secHeading.Append(secRun);
                    body.Append(secHeading);

                    var para = new Paragraph();
                    var run = new Run();
                    run.Append(new Text(text));
                    para.Append(run);
                    body.Append(para);
                }

                mainPart.Document = new Document();
                mainPart.Document.Body = body;
                mainPart.Document.Save();
            });

            MessageBox.Show("文档已保存", "成功",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}