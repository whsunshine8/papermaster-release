using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PaperMasterWin;

public partial class ConfigDialog : Window
{
    public const string WECHAT_ID = "ai_dxlw";
    public const string XIANYU_URL = "https://m.tb.cn/h.8E3c6Ov?tk=va4tTmOlu8K";

    private AppConfig _config;
    private PasswordBox _pbApiKey = null!;
    private ComboBox _cbModel = null!;
    private Button _btnRefresh = null!;
    private TextBlock _tbStatus = null!;

    public ConfigDialog(AppConfig config)
    {
        _config = config;
        InitializeComponent();
        LoadConfig();
    }

    private void InitializeComponent()
    {
        Title = "⚙️ 系统与模型配置";
        Width = 520;
        Height = 440;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = System.Windows.Media.Brushes.White;

        var mainPanel = new StackPanel { Margin = new Thickness(24) };

        // 提示信息
        var tipText = new TextBlock
        {
            Text = "🔑 API 授权设置",
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 41, 59)),
            Margin = new Thickness(0, 0, 0, 15)
        };
        mainPanel.Children.Add(tipText);

        // API Key 输入
        mainPanel.Children.Add(new TextBlock { Text = "API Key / 授权密钥:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 5, 0, 5) });
        _pbApiKey = new PasswordBox { Height = 34, Padding = new Thickness(8, 6, 8, 6), FontSize = 13 };
        mainPanel.Children.Add(_pbApiKey);

        // 模型选择 & 刷新
        mainPanel.Children.Add(new TextBlock { Text = "大模型选择:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 15, 0, 5) });
        var modelGrid = new Grid();
        modelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        modelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });

        _cbModel = new ComboBox { Height = 34, FontSize = 13, VerticalContentAlignment = VerticalAlignment.Center };
        Grid.SetColumn(_cbModel, 0);

        _btnRefresh = new Button
        {
            Content = "🔄 刷新模型",
            Height = 34,
            Margin = new Thickness(10, 0, 0, 0),
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(241, 245, 249)),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        _btnRefresh.Click += async (s, e) => await RefreshModelsAsync();
        Grid.SetColumn(_btnRefresh, 1);

        modelGrid.Children.Add(_cbModel);
        modelGrid.Children.Add(_btnRefresh);
        mainPanel.Children.Add(modelGrid);

        // 状态提示
        _tbStatus = new TextBlock
        {
            Text = "",
            FontSize = 12,
            Foreground = System.Windows.Media.Brushes.Gray,
            Margin = new Thickness(0, 8, 0, 15),
            TextWrapping = TextWrapping.Wrap
        };
        mainPanel.Children.Add(_tbStatus);

        // 分割线
        mainPanel.Children.Add(new Separator { Margin = new Thickness(0, 0, 0, 15) });

        // 官方客服 & 闲鱼购买
        var extraGrid = new Grid();
        extraGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        extraGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var btnWechat = new Button
        {
            Content = "💬 复制客服微信 (ai_dxlw)",
            Height = 36,
            Margin = new Thickness(0, 0, 6, 0),
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(236, 253, 245)),
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(5, 150, 105)),
            BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(167, 243, 208)),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        btnWechat.Click += (s, e) =>
        {
            Clipboard.SetText(WECHAT_ID);
            MessageBox.Show($"客服微信号 【{WECHAT_ID}】 已复制到剪贴板，备注体验即可！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        };
        Grid.SetColumn(btnWechat, 0);

        var btnXianyu = new Button
        {
            Content = "🐟 访问官方闲鱼直营店",
            Height = 36,
            Margin = new Thickness(6, 0, 0, 0),
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 247, 237)),
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(234, 88, 12)),
            BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(254, 215, 170)),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        btnXianyu.Click += (s, e) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = XIANYU_URL, UseShellExecute = true });
            }
            catch
            {
                Clipboard.SetText(XIANYU_URL);
                MessageBox.Show("已将闲鱼店铺链接复制到剪贴板，请在浏览器或闲鱼 APP 打开！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        };
        Grid.SetColumn(btnXianyu, 1);

        extraGrid.Children.Add(btnWechat);
        extraGrid.Children.Add(btnXianyu);
        mainPanel.Children.Add(extraGrid);

        // 底部保存与取消
        var bottomPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };

        var btnSave = new Button
        {
            Content = "保存配置",
            Width = 90,
            Height = 34,
            Margin = new Thickness(0, 0, 10, 0),
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 99, 235)),
            Foreground = System.Windows.Media.Brushes.White,
            Cursor = System.Windows.Input.Cursors.Hand
        };
        btnSave.Click += (s, e) => SaveConfig();

        var btnCancel = new Button
        {
            Content = "取消",
            Width = 70,
            Height = 34,
            Cursor = System.Windows.Input.Cursors.Hand
        };
        btnCancel.Click += (s, e) => Close();

        bottomPanel.Children.Add(btnSave);
        bottomPanel.Children.Add(btnCancel);
        mainPanel.Children.Add(bottomPanel);

        Content = mainPanel;
    }

    private void LoadConfig()
    {
        _pbApiKey.Password = _config.ApiKey ?? "";
        _cbModel.Items.Clear();

        var currentModel = string.IsNullOrEmpty(_config.Model) ? "qwen3-235b-a22b" : _config.Model;
        _cbModel.Items.Add(currentModel);
        if (currentModel != "gemini-3.6-flash-low") _cbModel.Items.Add("gemini-3.6-flash-low");
        if (currentModel != "gpt-4o-mini") _cbModel.Items.Add("gpt-4o-mini");
        _cbModel.SelectedIndex = 0;
    }

    private async Task RefreshModelsAsync()
    {
        var key = _pbApiKey.Password.Trim();
        if (string.IsNullOrEmpty(key))
        {
            _tbStatus.Text = "⚠️ 请先输入 API Key 再点击刷新模型！";
            _tbStatus.Foreground = System.Windows.Media.Brushes.OrangeRed;
            return;
        }

        _btnRefresh.IsEnabled = false;
        _tbStatus.Text = "⏳ 正在连接模型网关并获取可用模型列表...";
        _tbStatus.Foreground = System.Windows.Media.Brushes.Blue;

        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            var normUrl = _config.ApiUrl;
            if (normUrl.EndsWith("/chat/completions"))
                normUrl = normUrl.Substring(0, normUrl.Length - "/chat/completions".Length);
            if (!normUrl.EndsWith("/v1")) normUrl += "/v1";
            normUrl += "/models";

            var req = new HttpRequestMessage(HttpMethod.Get, normUrl);
            req.Headers.Add("Authorization", $"Bearer {key}");

            var resp = await client.SendAsync(req);
            if (resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                var json = JObject.Parse(body);
                var data = json["data"] as JArray;
                var list = new List<string>();

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        var id = item["id"]?.ToString();
                        if (!string.IsNullOrEmpty(id)) list.Add(id);
                    }
                }

                if (list.Count > 0)
                {
                    _cbModel.Items.Clear();
                    foreach (var m in list) _cbModel.Items.Add(m);
                    _cbModel.SelectedIndex = 0;
                    _tbStatus.Text = $"✅ 成功刷新！获取到 {list.Count} 个可用模型。";
                    _tbStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 185, 129));
                }
                else
                {
                    _tbStatus.Text = "⚠️ 未解析到模型列表，已保留默认设置。";
                }
            }
            else
            {
                _tbStatus.Text = $"❌ 刷新失败: HTTP {(int)resp.StatusCode}，请确认 Key 是否正确或联系微信号 {WECHAT_ID}";
                _tbStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        catch (Exception ex)
        {
            _tbStatus.Text = $"❌ 网络异常: {ex.Message}";
            _tbStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
        finally
        {
            _btnRefresh.IsEnabled = true;
        }
    }

    private void SaveConfig()
    {
        _config.ApiKey = _pbApiKey.Password.Trim();
        if (_cbModel.SelectedItem != null)
        {
            _config.Model = _cbModel.SelectedItem.ToString()!;
        }
        var manager = new ConfigManager();
        manager.Save(_config);
        DialogResult = true;
        Close();
    }
}
