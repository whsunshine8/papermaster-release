using System;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Net.Http;

namespace PaperMasterWin
{
    /// <summary>
    /// 主窗口代码后台
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConfigManager configManager;
        private PaperProject currentProject;
        private volatile bool isTaskCancelled = false;
        private AppConfig appConfig;

        public MainWindow()
        {
            InitializeComponent();
            currentProject = new PaperProject();
            appConfig = ConfigManager.LoadConfig();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 尝试从最近的项目恢复
            var historyFiles = ConfigManager.GetHistoryFiles();
            if (historyFiles.Count > 0)
            {
                var latestPath = ConfigManager.GetProjectPath(historyFiles[0]);
                if (File.Exists(latestPath))
                {
                    var loaded = PaperProject.LoadFromDisk(latestPath);
                    if (!string.IsNullOrEmpty(loaded.Title))
                    {
                        currentProject = loaded;
                        RestoreUI();
                    }
                }
            }
        }

        private void RestoreUI()
        {
            EtTitle.Text = currentProject.Title;
            SpLevel.SelectedIndex = GetSpinnerIndex(SpLevel, currentProject.Level, "本科毕业论文");
            SpDiscipline.SelectedIndex = GetSpinnerIndex(SpDiscipline, currentProject.Discipline);
            EtMajor.Text = currentProject.Major;
            SpOutlineLevel.SelectedIndex = GetSpinnerIndex(SpOutlineLevel, currentProject.OutlineLevel);
            SpWordCount.SelectedIndex = GetSpinnerIndex(SpWordCount, currentProject.WordCount);
            SpTemplate.SelectedIndex = GetSpinnerIndex(SpTemplate, currentProject.Template);

            if (!string.IsNullOrEmpty(currentProject.OutlineContent))
                EtOutlineContent.Text = currentProject.OutlineContent;
            if (!string.IsNullOrEmpty(currentProject.ProposalContent))
                EtProposalContent.Text = currentProject.ProposalContent;
            if (!string.IsNullOrEmpty(currentProject.PaperContent))
                EtPaperContent.Text = currentProject.PaperContent;
            if (!string.IsNullOrEmpty(currentProject.DefenseSpeech))
                EtDefenseSpeech.Text = currentProject.DefenseSpeech;
            if (!string.IsNullOrEmpty(currentProject.PptContent))
                EtPptContent.Text = currentProject.PptContent;
        }

        private int GetSpinnerIndex(ComboBox sp, string value, string defaultValue = "")
        {
            for (int i = 0; i < sp.Items.Count; i++)
            {
                var item = (ComboBoxItem)sp.Items[i];
                if (item.Content?.ToString() == value)
                    return i;
            }
            return 0;
        }

        // ==================== 步骤切换 ====================
        private void StepTab_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button)?.Tag?.ToString();
            switch (tag)
            {
                case "Step1": SwitchStep(1); break;
                case "Step2": SwitchStep(2); break;
                case "Step3": SwitchStep(3); break;
                case "Step4": SwitchStep(4); break;
                case "Step5": SwitchStep(5); break;
            }
        }

        private void SwitchStep(int step)
        {
            PanelStep1.Visibility = step == 1 ? Visibility.Visible : Visibility.Collapsed;
            PanelStep2.Visibility = step == 2 ? Visibility.Visible : Visibility.Collapsed;
            PanelStep3.Visibility = step == 3 ? Visibility.Visible : Visibility.Collapsed;
            PanelStep4.Visibility = step == 4 ? Visibility.Visible : Visibility.Collapsed;
            PanelStep5.Visibility = step == 5 ? Visibility.Visible : Visibility.Collapsed;

            currentProject.CurrentStep = step;
        }

        // ==================== Step 1: 生成大纲 ====================
        private async void BtnGenerateOutline_Click(object sender, RoutedEventArgs e)
        {
            var title = EtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("请输入论文题目", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowLoading("正在生成论文大纲...");
            isTaskCancelled = false;

            try
            {
                var level = ((ComboBoxItem)SpLevel.SelectedItem)?.Content?.ToString() ?? "本科毕业论文";
                var discipline = ((ComboBoxItem)SpDiscipline.SelectedItem)?.Content?.ToString() ?? "工学";
                var major = EtMajor.Text.Trim();
                var outlineLevel = ((ComboBoxItem)SpOutlineLevel.SelectedItem)?.Content?.ToString() ?? "三级目录";
                var wordCount = ((ComboBoxItem)SpWordCount.SelectedItem)?.Content?.ToString() ?? "8000-10000字";
                var template = ((ComboBoxItem)SpTemplate.SelectedItem)?.Content?.ToString() ?? "标准学术实证/工程设计模板";

                var prompt = $"""
                    请为以下论文生成详细的三级目录大纲：
                    题目：{title}
                    层次：{level}
                    学科：{discipline}
                    专业：{(string.IsNullOrEmpty(major) ? "未指定" : major)}
                    字数要求：{wordCount}
                    模板类型：{template}
                    目录级别要求：{outlineLevel}

                    请输出完整的论文大纲，包含章节编号、标题和简要说明。
                    格式要求：
                    第一章 XXX
                        1.1 XXX
                            1.1.1 XXX
                    第二章 XXX
                        ...
                    参考文献
                    致谢
                    """;

                var result = await ApiClient.CallChatAsync(
                    appConfig.BaseUrl,
                    appConfig.ApiKey,
                    appConfig.DefaultModel,
                    appConfig.SystemPrompt,
                    prompt,
                    appConfig.MaxTokens
                );

                EtOutlineContent.Text = result;
                currentProject.Title = title;
                currentProject.Level = level;
                currentProject.Discipline = discipline;
                currentProject.Major = major;
                currentProject.OutlineLevel = outlineLevel;
                currentProject.WordCount = wordCount;
                currentProject.Template = template;

                SwitchStep(2);
                SaveProject();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideLoading();
            }
        }

        // ==================== Step 2: 重新生成/保存大纲 ====================
        private async void BtnRegenerateOutline_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading("正在重新生成大纲...");
            isTaskCancelled = false;

            try
            {
                var prompt = $"请根据以下参数重新生成论文大纲：\n题目：{EtTitle.Text}\n层次：{((ComboBoxItem)SpLevel.SelectedItem)?.Content}\n学科：{((ComboBoxItem)SpDiscipline.SelectedItem)?.Content}\n\n{EtOutlineContent.Text}\n\n请优化并重新生成大纲，保持相同结构但改进内容质量。";

                var result = await ApiClient.CallChatAsync(
                    appConfig.BaseUrl,
                    appConfig.ApiKey,
                    appConfig.DefaultModel,
                    appConfig.SystemPrompt,
                    prompt,
                    appConfig.MaxTokens
                );

                EtOutlineContent.Text = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"重新生成失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideLoading();
            }
        }

        private void BtnSaveOutlineNext_Click(object sender, RoutedEventArgs e)
        {
            currentProject.OutlineContent = EtOutlineContent.Text;
            SaveProject();
            SwitchStep(3);
        }

        // ==================== Step 3: 生成开题报告 ====================
        private async void BtnGenerateProposal_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading("正在生成开题报告...");
            isTaskCancelled = false;

            try
            {
                var prompt = $"""
                    请根据以下论文大纲生成完整的开题报告：

                    论文题目：{EtTitle.Text}
                    层次：{((ComboBoxItem)SpLevel.SelectedItem)?.Content}
                    学科：{((ComboBoxItem)SpDiscipline.SelectedItem)?.Content}
                    专业：{(string.IsNullOrEmpty(EtMajor.Text) ? "未指定" : EtMajor.Text)}

                    论文大纲：
                    {EtOutlineContent.Text}

                    请生成开题报告，包含：
                    1. 研究背景与意义
                    2. 国内外研究现状综述
                    3. 研究内容与目标
                    4. 研究方法与技术路线
                    5. 创新点
                    6. 进度安排
                    7. 参考文献（不少于15篇，含中英文）

                    格式规范，直接可用于Word文档。
                    """;

                var result = await ApiClient.CallChatAsync(
                    appConfig.BaseUrl,
                    appConfig.ApiKey,
                    appConfig.DefaultModel,
                    appConfig.SystemPrompt,
                    prompt,
                    appConfig.MaxTokens
                );

                EtProposalContent.Text = result;
                currentProject.ProposalContent = result;
                SaveProject();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成开题报告失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideLoading();
            }
        }

        private void BtnSaveProposalNext_Click(object sender, RoutedEventArgs e)
        {
            currentProject.ProposalContent = EtProposalContent.Text;
            SaveProject();
            SwitchStep(4);
        }

        // ==================== Step 4: 生成论文正文 ====================
        private async void BtnGeneratePaper_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading("正在生成论文正文（可能耗时较长）...");
            isTaskCancelled = false;

            try
            {
                var prompt = $"""
                    请根据以下论文大纲和开题报告，撰写完整的论文正文：

                    论文题目：{EtTitle.Text}
                    层次：{((ComboBoxItem)SpLevel.SelectedItem)?.Content}
                    学科：{((ComboBoxItem)SpDiscipline.SelectedItem)?.Content}
                    专业：{(string.IsNullOrEmpty(EtMajor.Text) ? "未指定" : EtMajor.Text)}
                    字数要求：{((ComboBoxItem)SpWordCount.SelectedItem)?.Content}

                    论文大纲：
                    {EtOutlineContent.Text}

                    开题报告：
                    {EtProposalContent.Text}

                    要求：
                    1. 按照大纲的章节结构撰写
                    2. 使用规范的学术语言
                    3. 包含图表描述和数据支撑
                    4. 标注引用和参考文献位置
                    5. 逻辑严密、论证充分
                    6. 直接输出正文内容，不要多余解释
                    7. 确保内容充实，达到字数要求

                    请开始撰写正文第一部分。
                    """;

                var result = await ApiClient.CallChatAsync(
                    appConfig.BaseUrl,
                    appConfig.ApiKey,
                    appConfig.DefaultModel,
                    appConfig.SystemPrompt,
                    prompt,
                    appConfig.MaxTokens
                );

                EtPaperContent.Text = result;
                currentProject.PaperContent = result;
                SaveProject();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成论文正文失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideLoading();
            }
        }

        private async void BtnReduceAigc_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading("正在优化降低AI检测率...");
            isTaskCancelled = false;

            try
            {
                var prompt = $"""
                    以下论文正文可能被AI检测工具识别为AI生成，请对其进行人工化改写：
                    - 增加个人化的表达
                    - 调整句式结构
                    - 增加过渡自然度
                    - 保留学术规范性

                    正文内容：
                    {EtPaperContent.Text}

                    请直接输出改写后的正文，不要额外解释。
                    """;

                var result = await ApiClient.CallChatAsync(
                    appConfig.BaseUrl,
                    appConfig.ApiKey,
                    appConfig.DefaultModel,
                    appConfig.SystemPrompt,
                    prompt,
                    appConfig.MaxTokens
                );

                EtPaperContent.Text = result;
                currentProject.PaperContent = result;
                SaveProject();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"优化失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideLoading();
            }
        }

        private void BtnSavePaperNext_Click(object sender, RoutedEventArgs e)
        {
            currentProject.PaperContent = EtPaperContent.Text;
            SaveProject();
            SwitchStep(5);
        }

        // ==================== Step 5: 答辩演讲 & PPT ====================
        private async void BtnGenerateDefense_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading("正在生成答辩演讲稿...");
            isTaskCancelled = false;

            try
            {
                var prompt = $"""
                    请根据以下论文信息，生成一份5-8分钟的答辩演讲稿：

                    论文题目：{EtTitle.Text}
                    层次：{((ComboBoxItem)SpLevel.SelectedItem)?.Content}
                    学科：{((ComboBoxItem)SpDiscipline.SelectedItem)?.Content}
                    专业：{(string.IsNullOrEmpty(EtMajor.Text) ? "未指定" : EtMajor.Text)}

                    论文大纲：
                    {EtOutlineContent.Text}

                    开题报告：
                    {EtProposalContent.Text}

                    论文正文摘要/核心内容：
                    {EtPaperContent.Text.Length > 2000 ? EtPaperContent.Text.Substring(0, 2000) : EtPaperContent.Text}

                    演讲稿要求：
                    1. 开场问候（尊敬的各位老师，大家好...）
                    2. 论文背景与研究意义
                    3. 主要研究内容与方法
                    4. 研究成果与创新点
                    5. 不足与展望
                    6. 结束语（谢谢各位老师，请批评指正）
                    7. 口语化表达，适合演讲
                    """;

                var result = await ApiClient.CallChatAsync(
                    appConfig.BaseUrl,
                    appConfig.ApiKey,
                    appConfig.DefaultModel,
                    appConfig.SystemPrompt,
                    prompt,
                    appConfig.MaxTokens
                );

                EtDefenseSpeech.Text = result;
                currentProject.DefenseSpeech = result;
                SaveProject();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成演讲稿失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideLoading();
            }
        }

        private async void BtnGeneratePpt_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading("正在生成PPT大纲...");
            isTaskCancelled = false;

            try
            {
                var prompt = $"""
                    请根据以下论文信息，生成一份答辩PPT的大纲（每页PPT的标题和要点）：

                    论文题目：{EtTitle.Text}
                    层次：{((ComboBoxItem)SpLevel.SelectedItem)?.Content}
                    学科：{((ComboBoxItem)SpDiscipline.SelectedItem)?.Content}

                    论文大纲：
                    {EtOutlineContent.Text}

                    开题报告摘要：
                    {EtProposalContent.Text.Substring(0, Math.Min(1500, EtProposalContent.Text.Length))}

                    PPT要求：
                    - 共10-15页
                    - 每页标题简洁
                    - 列出每页的关键要点（3-5条）
                    - 适合学术答辩场景
                    - 格式示例：

                    第1页：封面
                    标题：XXX论文答辩
                    要点：姓名、学号、导师、专业

                    第2页：研究背景
                    要点：
                    - XXX
                    - XXX
                    """;

                var result = await ApiClient.CallChatAsync(
                    appConfig.BaseUrl,
                    appConfig.ApiKey,
                    appConfig.DefaultModel,
                    appConfig.SystemPrompt,
                    prompt,
                    appConfig.MaxTokens
                );

                EtPptContent.Text = result;
                currentProject.PptContent = result;
                SaveProject();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成PPT大纲失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideLoading();
            }
        }

        // ==================== 辅助功能 ====================
        private void ShowLoading(string text)
        {
            TvLoadingText.Text = text;
            LoadingOverlay.Visibility = Visibility.Visible;
        }

        private void HideLoading()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnManualStop_Click(object sender, RoutedEventArgs e)
        {
            isTaskCancelled = true;
            HideLoading();
            MessageBox.Show("已停止当前生成任务", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveProject()
        {
            try
            {
                ConfigManager.EnsureDirectories();
                var filename = $"PaperProject_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                currentProject.SaveToDisk(ConfigManager.GetProjectPath(filename));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存项目失败: {ex.Message}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ==================== 复制功能 ====================
        private void BtnCopyProposal_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(EtProposalContent.Text);
            MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCopyPaper_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(EtPaperContent.Text);
            MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCopyDefense_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(EtDefenseSpeech.Text);
            MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCopyPpt_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(EtPptContent.Text);
            MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ==================== DOCX导出 ====================
        private void BtnDownloadOutline_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "Word文档|*.docx",
                Title = "导出论文大纲",
                FileName = $"论文大纲_{EtTitle.Text?.Replace(" ", "_")}.docx"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ExportDocx(dialog.FileName, "论文大纲", EtOutlineContent.Text);
                MessageBox.Show($"已导出到: {dialog.FileName}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDownloadProposal_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "Word文档|*.docx",
                Title = "导出开题报告",
                FileName = $"开题报告_{EtTitle.Text?.Replace(" ", "_")}.docx"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ExportDocx(dialog.FileName, "开题报告", EtProposalContent.Text);
                MessageBox.Show($"已导出到: {dialog.FileName}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDownloadPaper_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "Word文档|*.docx",
                Title = "导出论文正文",
                FileName = $"论文正文_{EtTitle.Text?.Replace(" ", "_")}.docx"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ExportDocx(dialog.FileName, "论文正文", EtPaperContent.Text);
                MessageBox.Show($"已导出到: {dialog.FileName}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDownloadDefense_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "Word文档|*.docx",
                Title = "导出答辩演讲稿",
                FileName = $"答辩演讲稿_{EtTitle.Text?.Replace(" ", "_")}.docx"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ExportDocx(dialog.FileName, "答辩演讲稿", EtDefenseSpeech.Text);
                MessageBox.Show($"已导出到: {dialog.FileName}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 简易DOCX导出 - 使用OpenXml
        /// </summary>
        private void ExportDocx(string fileName, string title, string content)
        {
            try
            {
                using var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Create(fileName, DocumentFormat.OpenXml.Packaging.DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                var body = new DocumentFormat.OpenXml.Wordprocessing.Body();

                // 标题
                var heading = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                var run = new DocumentFormat.OpenXml.Wordprocessing.Run();
                var text = new DocumentFormat.OpenXml.Wordprocessing.RunProperties();
                text.Append(new DocumentFormat.OpenXml.Wordprocessing.Bold());
                text.Append(new DocumentFormat.OpenXml.Wordprocessing.FontSize() { Val = "48" });
                run.Append(text);
                run.Append(new DocumentFormat.OpenXml.Wordprocessing.Text(title));
                heading.Append(run);
                body.Append(heading);

                // 内容
                var lines = content.Split('\n');
                foreach (var line in lines)
                {
                    var p = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                    p.Append(new DocumentFormat.OpenXml.Wordprocessing.Run());
                    var t = new DocumentFormat.OpenXml.Wordprocessing.Text(line);
                    // 需要重新构建run
                    var r = new DocumentFormat.OpenXml.Wordprocessing.Run();
                    r.Append(t);
                    p.RemoveAllChildren();
                    p.Append(r);
                    body.Append(p);
                }

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== 配置/历史/关于 ====================
        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ConfigDialog(appConfig);
            if (dlg.ShowDialog() == true)
            {
                ConfigManager.SaveConfig(appConfig);
            }
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyFiles = ConfigManager.GetHistoryFiles();
            if (historyFiles.Count == 0)
            {
                MessageBox.Show("暂无历史记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dlg = new System.Windows.Forms.ListBox();
            dlg.Items.AddRange(historyFiles.ToArray());
            dlg.SelectionMode = System.Windows.Forms.SelectionMode.One;
            var form = new System.Windows.Forms.Form
            {
                Text = "历史记录",
                Width = 500,
                Height = 400,
                Controls = { dlg },
                StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
            };
            form.ShowDialog();

            if (dlg.SelectedItem != null)
            {
                var path = ConfigManager.GetProjectPath(dlg.SelectedItem.ToString());
                if (File.Exists(path))
                {
                    currentProject = PaperProject.LoadFromDisk(path);
                    RestoreUI();
                    MessageBox.Show("已加载历史记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "论文智作大师 (PaperMaster) Windows版 v1.0.0\n\n" +
                "AI驱动的毕业论文生成工具\n" +
                "支持5步论文生成流程\n" +
                "\n" +
                "AI API: aiapi.whsunshine.link\n" +
                "微信公众号: ai_dxlw\n" +
                "GitHub: https://github.com/whsunshine8/papermaster-release/\n" +
                "闲鱼购买: https://m.tb.cn/h.8E3c6Ov?tk=va4tTmOlu8K\n\n" +
                "© 2026 whsunshine",
                "关于 PaperMaster",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
