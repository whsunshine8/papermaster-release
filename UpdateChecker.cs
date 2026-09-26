using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace PaperMasterWin;

public static class UpdateChecker
{
    public const string CurrentVersion = "1.1.2";
    public const int CurrentVersionCode = 112;
    public const string GitHubReleasesUrl = "https://github.com/whsunshine8/papermaster-release/releases";
    public const string UpdateJsonUrl = "https://raw.githubusercontent.com/whsunshine8/papermaster-release/main/update.json";

    public static async Task CheckUpdateAsync(Window owner, bool isManual = true)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var resp = await client.GetAsync(UpdateJsonUrl);
            if (resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                var json = JObject.Parse(body);
                var remoteVer = json["versionName"]?.ToString() ?? "";
                var remoteCode = json["versionCode"]?.Value<int>() ?? 0;
                var updateLog = json["updateLog"]?.ToString() ?? "功能优化与体验提升";
                var downloadUrl = json["downloadUrl"]?.ToString() ?? GitHubReleasesUrl;

                if (remoteCode > CurrentVersionCode)
                {
                    var msg = $"🎉 发现新版本: v{remoteVer}\n\n更新说明:\n{updateLog}\n\n是否立即前往 GitHub 下载最新版？";
                    var res = MessageBox.Show(owner, msg, "在线升级检测", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (res == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo { FileName = downloadUrl, UseShellExecute = true });
                    }
                    return;
                }
            }
        }
        catch { }

        if (isManual)
        {
            var msg = $"当前版本已是最新版 (v{CurrentVersion})！\n\n是否打开 GitHub Releases 发布主页查看详细历史日志？";
            var res = MessageBox.Show(owner, msg, "版本检测", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (res == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo { FileName = GitHubReleasesUrl, UseShellExecute = true });
            }
        }
    }
}
