using System;
using System.Windows;

namespace PaperMasterWin
{
    /// <summary>
    /// 配置对话框
    /// </summary>
    public class ConfigDialog : Window
    {
        private TextBox etBaseUrl, etApiKey, etModel, etSystemPrompt, etMaxTokens;
        private AppConfig config;

        public ConfigDialog(AppConfig config)
        {
            this.config = config;
            Title = "⚙ 配置";
            Width = 600;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.CanResize;

            Content = CreateUI();
        }

        private StackPanel CreateUI()
        {
            var panel = new StackPanel { Margin = new Thickness(15) };

            // API地址
            panel.Children.Add(new Label { Content = "API 地址", FontWeight = Bold });
            etBaseUrl = new TextBox { Text = config.BaseUrl, Height = 35, Margin = new Thickness(0, 0, 0, 15) };
            panel.Children.Add(etBaseUrl);

            // API Key
            panel.Children.Add(new Label { Content = "API Key", FontWeight = Bold });
            etApiKey = new TextBox { Text = config.ApiKey, Height = 35, Margin = new Thickness(0, 0, 0, 15) };
            panel.Children.Add(etApiKey);

            // 默认模型
            panel.Children.Add(new Label { Content = "默认模型", FontWeight = Bold });
            etModel = new TextBox { Text = config.DefaultModel, Height = 35, Margin = new Thickness(0, 0, 0, 15) };
            panel.Children.Add(etModel);

            // 系统提示
            panel.Children.Add(new Label { Content = "系统提示 (System Prompt)", FontWeight = Bold });
            etSystemPrompt = new TextBox
            {
                Text = config.SystemPrompt,
                Height = 100,
                TextWrapping = Wrap,
                AcceptsReturn = true,
                Margin = new Thickness(0, 0, 0, 15)
            };
            panel.Children.Add(etSystemPrompt);

            // 最大Token数
            panel.Children.Add(new Label { Content = "最大 Token 数", FontWeight = Bold });
            etMaxTokens = new TextBox { Text = config.MaxTokens.ToString(), Height = 35, Margin = new Thickness(0, 0, 0, 20) };
            panel.Children.Add(etMaxTokens);

            // 按钮
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var saveBtn = new Button { Content = "💾 保存", Width = 100, Height = 35, Margin = new Thickness(0, 0, 10, 0) };
            saveBtn.Click += (s, e) => SaveConfig();
            var cancelBtn = new Button { Content = "❌ 取消", Width = 100, Height = 35 };
            cancelBtn.Click += (s, e) => { DialogResult = false; Close(); };

            btnPanel.Children.Add(saveBtn);
            btnPanel.Children.Add(cancelBtn);
            panel.Children.Add(btnPanel);

            return panel;
        }

        private void SaveConfig()
        {
            config.BaseUrl = etBaseUrl.Text.Trim();
            config.ApiKey = etApiKey.Text.Trim();
            config.DefaultModel = etModel.Text.Trim();
            config.SystemPrompt = etSystemPrompt.Text.Trim();
            if (int.TryParse(etMaxTokens.Text.Trim(), out var tokens))
                config.MaxTokens = tokens;

            ConfigManager.SaveConfig(config);
            DialogResult = true;
            Close();
        }
    }
}
