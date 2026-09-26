using System;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace PaperMasterWin;

/// <summary>
/// 配置对话框
/// </summary>
public partial class ConfigDialog : Window
{
    private AppConfig _config;
    private TextBox _txtApiKey = null!;
    private TextBox _txtApiUrl = null!;
    private TextBox _txtModel = null!;
    private TextBox _txtProjectDir = null!;

    public ConfigDialog(AppConfig config)
    {
        _config = config;
        InitializeComponent();
        LoadConfig();
    }

    private void InitializeComponent()
    {
        Title = "⚙️ 配置";
        Width = 600;
        Height = 500;
        ResizeMode = ResizeMode.CanResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var grid = new Grid();
        grid.Margin = new Thickness(10);
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var lbl1 = new Label { Content = "API Key:" };
        Grid.SetRow(lbl1, 0);
        _txtApiKey = new TextBox();
        Grid.SetRow(_txtApiKey, 0);
        _txtApiKey.Margin = new Thickness(0, 5, 0, 5);

        var lbl2 = new Label { Content = "API URL:" };
        Grid.SetRow(lbl2, 1);
        _txtApiUrl = new TextBox();
        Grid.SetRow(_txtApiUrl, 1);
        _txtApiUrl.Margin = new Thickness(0, 5, 0, 5);

        var lbl3 = new Label { Content = "模型:" };
        Grid.SetRow(lbl3, 2);
        _txtModel = new TextBox();
        Grid.SetRow(_txtModel, 2);
        _txtModel.Margin = new Thickness(0, 5, 0, 5);

        var lbl4 = new Label { Content = "项目目录:" };
        Grid.SetRow(lbl4, 3);
        _txtProjectDir = new TextBox();
        Grid.SetRow(_txtProjectDir, 3);
        _txtProjectDir.Margin = new Thickness(0, 5, 0, 5);
        _txtProjectDir.IsEnabled = false;

        var btnPanel = new StackPanel();
        btnPanel.Orientation = Orientation.Horizontal;
        btnPanel.HorizontalAlignment = HorizontalAlignment.Right;
        btnPanel.Margin = new Thickness(0, 10, 0, 0);

        var btnSave = new Button { Content = "保存", Width = 80, Margin = new Thickness(0, 0, 10, 0) };
        btnSave.Click += (s, e) => SaveConfig();

        var btnCancel = new Button { Content = "取消", Width = 80 };
        btnCancel.Click += (s, e) => Close();

        btnPanel.Children.Add(btnSave);
        btnPanel.Children.Add(btnCancel);

        Grid.SetRow(btnPanel, 4);

        grid.Children.Add(lbl1);
        grid.Children.Add(_txtApiKey);
        grid.Children.Add(lbl2);
        grid.Children.Add(_txtApiUrl);
        grid.Children.Add(lbl3);
        grid.Children.Add(_txtModel);
        grid.Children.Add(lbl4);
        grid.Children.Add(_txtProjectDir);
        grid.Children.Add(btnPanel);

        Content = grid;
    }

    private void LoadConfig()
    {
        _txtApiKey.Text = _config.ApiKey;
        _txtApiUrl.Text = _config.ApiUrl;
        _txtModel.Text = _config.Model;
        _txtProjectDir.Text = _config.ProjectDirectory;
    }

    private void SaveConfig()
    {
        _config.ApiKey = _txtApiKey.Text.Trim();
        _config.ApiUrl = _txtApiUrl.Text.Trim();
        _config.Model = _txtModel.Text.Trim();
        var manager = new ConfigManager();
        manager.Save(_config);
        Close();
    }
}