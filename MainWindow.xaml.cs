using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PaperMasterWin;

public partial class MainWindow : Window
{
    private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
    private ConfigManager _configManager;
    private AppConfig _config;
    private PaperProject _project;

    public MainWindow()
    {
        InitializeComponent();
        _configManager = new ConfigManager();
        _config = _configManager.Load();
        _project = new PaperProject();

        InitSelectors();
        LoadProjectToUi();
    }

    private void InitSelectors()
    {
        // 论文层次
        CbLevel.Items.Clear();
        var levels = new[] { "本科毕业论文", "硕士学位论文", "专科毕业设计/论文", "博士学位论文", "期末课程论文", "中职/高职毕业报告" };
        foreach (var l in levels) CbLevel.Items.Add(l);
        CbLevel.SelectedIndex = 0;

        // 学科门类
        CbDiscipline.Items.Clear();
        var disciplines = new[]
        {
            "工学 (计算机/电子信息/土木/机械/电气/自动化等)",
            "管理学 (工商管理/财务管理/电子商务/物流/信息管理等)",
            "经济学 (金融学/国际经济与贸易/应用经济学等)",
            "理学 (数学/物理/化学/生物技术等)",
            "法学 (法学理论/宪法行政法/民商法/社会学等)",
            "教育学 (学前教育/教育技术/体育教育/课程教学论等)",
            "文学 (汉语言文学/英语/翻译/新闻传播等)",
            "艺术学 (视觉传达/环境设计/产品设计/数字媒体艺术等)",
            "医学 (临床/护理/药学/中医学/预防医学等)",
            "农学 (农学/植物保护/园艺/林学等)",
            "哲学 (哲学/逻辑学/伦理学/宗教学等)",
            "历史学 (中国史/世界史/考古学等)"
        };
        foreach (var d in disciplines) CbDiscipline.Items.Add(d);
        CbDiscipline.SelectedIndex = 0;

        // 目录级别
        CbOutlineLevel.Items.Clear();
        var outlineLevels = new[]
        {
            "三级目录 (例如: 1.1.1 结构紧凑规范 - 推荐)",
            "二级目录 (例如: 1.1 简明框架)",
            "四级目录 (例如: 1.1.1.1 超精细技术设计)"
        };
        foreach (var o in outlineLevels) CbOutlineLevel.Items.Add(o);
        CbOutlineLevel.SelectedIndex = 0;

        // 论文字数
        CbWordCount.Items.Clear();
        var wordCounts = new[]
        {
            "8,000 - 10,000 字 (仅正文纯字数，标准本科推荐)",
            "12,000 - 15,000 字 (仅正文纯字数，优秀本科/专硕要求)",
            "5,000 - 8,000 字 (仅正文纯字数，专科/期末大作业)",
            "20,000 - 30,000 字 (仅正文纯字数，学硕/硕士开篇)",
            "3,000 - 5,000 字 (仅正文纯字数，简版短篇课程论文)"
        };
        foreach (var w in wordCounts) CbWordCount.Items.Add(w);
        CbWordCount.SelectedIndex = 0;

        // 结构模板
        CbTemplate.Items.Clear();
        var templates = new[]
        {
            "标准学术实证/工程设计模板 (绪论 - 理论与技术 - 现状分析 - 核心系统架构/模型设计 - 实现与验证 - 结论)",
            "实证分析与计量统计模板 (引言 - 文献综述与假设 - 数据来源与模型设定 - 实证结果与机制分析 - 政策建议)",
            "案例分析研究模板 (引言 - 理论基础 - 企业/行业案例概况及痛点 - 深度原因剖析 - 优化对策方案 - 总结)",
            "实验性科技研究模板 (前言 - 实验材料与方法 - 实验结果数据图表 - 机制机理探讨 - 结论与未来工作)"
        };
        foreach (var t in templates) CbTemplate.Items.Add(t);
        CbTemplate.SelectedIndex = 0;
    }

    private void SyncUiToProject()
    {
        _project.Title = TbTitle.Text.Trim();
        _project.Level = CbLevel.SelectedItem?.ToString() ?? "本科毕业论文";
        _project.Discipline = CbDiscipline.SelectedItem?.ToString() ?? "工学";
        _project.Major = TbMajor.Text.Trim();
        _project.OutlineLevel = CbOutlineLevel.SelectedItem?.ToString() ?? "三级目录 (1.1.1)";
        _project.Template = CbTemplate.SelectedItem?.ToString() ?? "";

        var wcStr = CbWordCount.SelectedItem?.ToString() ?? "";
        if (wcStr.Contains("12,000")) _project.WordCount = 13500;
        else if (wcStr.Contains("5,000")) _project.WordCount = 6500;
        else if (wcStr.Contains("20,000")) _project.WordCount = 22000;
        else if (wcStr.Contains("3,000")) _project.WordCount = 4000;
        else _project.WordCount = 9000;

        _project.Outline = TbOutline.Text;
        _project.Proposal = TbProposal.Text;
        _project.FullPaper = TbPaper.Text;
        _project.DefenseSpeech = TbDefense.Text;
        _project.PptOutline = TbPpt.Text;
    }

    private void LoadProjectToUi()
    {
        TbTitle.Text = _project.Title;
        TbMajor.Text = _project.Major;
        TbOutline.Text = _project.Outline;
        TbProposal.Text = _project.Proposal;
        TbPaper.Text = _project.FullPaper;
        TbDefense.Text = _project.DefenseSpeech;
        TbPpt.Text = _project.PptOutline;
    }

    private async Task<string> CallApiWithTokensAsync(string systemPrompt, string userPrompt, int maxTokens = 0)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            MessageBox.Show("请先点击右上角【⚙️ 系统配置】输入并保存 API Key！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            BtnConfig_Click(this, new RoutedEventArgs());
            return "";
        }

        var reqObj = new JObject
        {
            ["model"] = _config.Model,
            ["temperature"] = 0.7
        };

        if (maxTokens > 0)
        {
            reqObj["max_tokens"] = maxTokens;
        }

        var msgs = new JArray();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            msgs.Add(new JObject { ["role"] = "system", ["content"] = systemPrompt });
        }
        msgs.Add(new JObject { ["role"] = "user", ["content"] = userPrompt });
        reqObj["messages"] = msgs;

        var json = reqObj.ToString(Formatting.None);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var reqMsg = new HttpRequestMessage(HttpMethod.Post, _config.ApiUrl) { Content = content };
        reqMsg.Headers.Add("Authorization", $"Bearer {_config.ApiKey.Trim()}");

        try
        {
            var resp = await _httpClient.SendAsync(reqMsg);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                throw new Exception($"API 响应错误 HTTP {(int)resp.StatusCode}: {body}");
            }

            var resJson = JObject.Parse(body);
            var choice = resJson["choices"]?[0];
            var ret = choice?["message"]?["content"]?.ToString();
            return ret ?? "";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"网络请求异常:\n{ex.Message}", "API 调用失败", MessageBoxButton.OK, MessageBoxImage.Error);
            return "";
        }
    }

    private void SetStatus(string msg)
    {
        TbStatusBar.Text = $"[{DateTime.Now:HH:mm:ss}] {msg}";
    }

    private async void BtnGenerateOutline_Click(object sender, RoutedEventArgs e)
    {
        SyncUiToProject();
        if (string.IsNullOrWhiteSpace(_project.Title))
        {
            MessageBox.Show("请输入论文题目！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SetStatus("正在根据论文层次、学科与模板规划精细大纲...");
        BtnGenerateOutline.IsEnabled = false;

        string sys = "你是一位经验极其丰富的高校教授与学术论文导师，精通国家教育部学术规范与各学科毕业论文结构标准。";
        string prompt = "请为以下毕业论文规划一份逻辑严谨、论据充分、符合学术规范的【论文完整大纲】：\n" +
                "【论文题目】：" + _project.Title + "\n" +
                "【论文层次】：" + _project.Level + "\n" +
                "【学科门类】：" + _project.Discipline + "\n" +
                "【所学专业】：" + (string.IsNullOrEmpty(_project.Major) ? "未指定（请按学科门类主流方向）" : _project.Major) + "\n" +
                "【目录精细度要求】：" + _project.OutlineLevel + "\n" +
                "【目标纯正文字数】：" + _project.WordCount + "字\n" +
                "【预设参考结构模板】：" + _project.Template + "\n\n" +
                "【极重要规划要求】：\n" +
                "1. 一级章节标题必须干净利落，只写标准章名称（例如：第1章 绪论、第7章 总结与展望），严禁在章标题后附加“(规划字数: 2000字)”之类的字数标注或任何括号说明！\n" +
                "2. 严禁出现 ##、###、** 等 markdown 符号！直接输出中文标准章节编号（第1章、1.1、1.1.1）。\n" +
                "3. 结构应完整包含：绪论/引言、理论与现状综述、需求分析/模型架构、核心系统设计/实证研究、实现测试与结果讨论、总结与展望、参考文献等完整学术闭环。\n" +
                "4. 每一小节给出具体研究内容要点说明，方便后续按章分块并行生成与自由修改。";

        var res = await CallApiWithTokensAsync(sys, prompt, 4000);
        BtnGenerateOutline.IsEnabled = true;

        if (!string.IsNullOrWhiteSpace(res))
        {
            _project.Outline = res;
            TbOutline.Text = res;
            HistoryManager.SaveProject(_project);
            MainTabs.SelectedIndex = 1;
            SetStatus("大纲规划成功！已自动切换至大纲编辑页。");
        }
        else
        {
            SetStatus("大纲生成失败。");
        }
    }

    private void BtnResetProject_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("确定要重置当前输入并新建论文项目吗？", "确认新建", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            _project = new PaperProject();
            LoadProjectToUi();
            SetStatus("已新建空白论文项目。");
        }
    }

    private void BtnRegenerateOutline_Click(object sender, RoutedEventArgs e)
    {
        BtnGenerateOutline_Click(sender, e);
    }

    private void BtnNextToProposal_Click(object sender, RoutedEventArgs e)
    {
        _project.Outline = TbOutline.Text.Trim();
        if (string.IsNullOrWhiteSpace(_project.Outline))
        {
            MessageBox.Show("大纲内容为空，请先规划或输入大纲！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        HistoryManager.SaveProject(_project);
        MainTabs.SelectedIndex = 2;
        SetStatus("已保存大纲，进入开题报告。");
    }

    private async void BtnGenerateProposal_Click(object sender, RoutedEventArgs e)
    {
        _project.Outline = TbOutline.Text.Trim();
        if (string.IsNullOrWhiteSpace(_project.Outline))
        {
            MessageBox.Show("大纲为空，请先在第二步完成或输入大纲！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SetStatus("正在严格依照大纲撰写标准开题报告...");
        BtnGenerateProposal.IsEnabled = false;

        string sys = "你是一位高校学术委员会专家，专门指导与评审毕业论文开题报告。";
        string prompt = "请根据以下论文信息及已确认的论文大纲，撰写一份符合国家高校规范的【完整毕业论文开题报告】：\n" +
                "【论文题目】：" + _project.Title + "\n" +
                "【论文层次】：" + _project.Level + " | 【学科专业】：" + _project.Discipline + " " + _project.Major + "\n" +
                "【已确定的论文大纲】：\n" + _project.Outline + "\n\n" +
                "【排版与内容硬性要求】：\n" +
                "1. 严禁使用 ##、###、** 等 markdown 符号！段落、标题该空格就空格，纯正整洁中文公文排版。\n" +
                "2. 严格按以下核心模块完整展开：\n" +
                "一、选题背景与研究意义（理论意义与实际应用价值）\n" +
                "二、国内外研究现状及文献综述（国外现状、国内现状、现有研究不足分析）\n" +
                "三、研究目标、核心研究内容与拟解决的关键问题\n" +
                "四、研究方法、技术路线与实验/实证方案（可包含标准三线表格、图示说明、关键计算公式）\n" +
                "五、论文特色与创新点（列出2-3个扎实的创新要素）\n" +
                "六、进度计划安排表（采用标准 Markdown 表格呈现）\n" +
                "七、主要参考文献目录：【极为重要：所有参考文献必须真实存在！严禁伪造！包含作者真实姓名、近3-5年真实发表的权威期刊/学术会议/专著名称、卷期页码，严格符合GB/T 7714-2015标准格式】。";

        var res = await CallApiWithTokensAsync(sys, prompt, 5000);
        BtnGenerateProposal.IsEnabled = true;

        if (!string.IsNullOrWhiteSpace(res))
        {
            _project.Proposal = res;
            TbProposal.Text = res;
            HistoryManager.SaveProject(_project);
            SetStatus("开题报告生成完成！");
        }
        else
        {
            SetStatus("开题报告生成失败。");
        }
    }

    private void BtnNextToPaper_Click(object sender, RoutedEventArgs e)
    {
        _project.Proposal = TbProposal.Text.Trim();
        HistoryManager.SaveProject(_project);
        MainTabs.SelectedIndex = 3;
        SetStatus("已进入正文撰写阶段。");
    }

    private async void BtnGeneratePaper_Click(object sender, RoutedEventArgs e)
    {
        _project.Outline = TbOutline.Text.Trim();
        if (string.IsNullOrWhiteSpace(_project.Outline))
        {
            MessageBox.Show("大纲为空，请先在第二步规划或输入大纲！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var tasks = ChapterTask.ParseOutlineChapters(_project.Outline, _project.WordCount);
        if (tasks.Count == 0)
        {
            MessageBox.Show("未能从大纲中解析到有效章节，请检查大纲格式！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        BtnGeneratePaper.IsEnabled = false;
        var combined = new StringBuilder();

        SetStatus("【流水线 1/3】正在撰写中英文摘要与核心关键词...");
        string sysAbs = "你是一位高校学术委员会专家与核心期刊审稿人。";
        string promptAbs = "请根据以下毕业论文信息，撰写标准的【中文摘要与关键词】和【英文摘要(Abstract)与Keywords】：\n" +
                "【论文题目】：" + _project.Title + "\n" +
                "【论文层次与专业】：" + _project.Level + " / " + _project.Discipline + " " + _project.Major + "\n" +
                "【论文完整大纲】：\n" + _project.Outline + "\n\n" +
                "【格式要求】：\n" +
                "严禁出现 ##、** 等 markdown 符号！中文摘要约400字，客观概括研究背景、方法、设计与成果结论。随后附带3-5个中文关键词。接着给出准确无误的英文 Abstract 与 Keywords。";

        var absText = await CallApiWithTokensAsync(sysAbs, promptAbs, 2000);
        if (!string.IsNullOrWhiteSpace(absText))
        {
            combined.AppendLine(absText).AppendLine();
            TbPaper.Text = combined.ToString();
        }

        for (int i = 0; i < tasks.Count; i++)
        {
            var task = tasks[i];
            int curNum = i + 1;
            int maxWords = (int)(task.TargetWordCount * 1.2);
            SetStatus($"【流水线 2/3】正在推进第 {curNum}/{tasks.Count} 章【{task.Title}】(目标: {task.TargetWordCount}字)...");

            string sys = "你是一位国家级学术期刊审稿专家与资深高校博导，擅长长篇学术论著撰写。严守知网与 PaperPass AIGC 低疑似度规范。";
            string prompt = "请根据以下毕业论文信息，撰写【" + task.Title + "】的完整正文：\n\n" +
                    "【论文题目】：" + _project.Title + "\n" +
                    "【学科专业】：" + _project.Discipline + " " + _project.Major + "\n" +
                    "【本章在大纲中的小节规划】：\n" + (string.IsNullOrEmpty(task.OutlineDetails) ? task.Title : task.OutlineDetails) + "\n\n" +
                    "【严防 PaperPass / 知网 AIGC 判红的学术规范（违者重写）】：\n" +
                    "1. 绝不使用任何 AI 八股模板词：绝对禁止以“随着……”开头！坚决禁用“本文旨在”、“值得注意的是”、“由此可见”、“总而言之”、“综上所述”！\n" +
                    "2. 句式长短错落（打破均一机器困惑度）：交替撰写 40-50 字的严密长定语修饰长句与 8-12 字的凝练论断短句，融入“质言之”、“诚然”、“毋庸讳言”、“进而言之”等学者手写惯用语；\n" +
                    "3. 纯正客观学术视角：禁止出现“我们”、“本文认为”，一律采用被动或客观引证语态；\n" +
                    "4. 每个小节必须完整展开 3-5 个扎实段落，结合具体工程实现/实证细节与数据论证，严禁敷衍，严禁用（略）；\n\n" +
                    "【字数硬性规定】：\n" +
                    "本章正文总字数目标为 " + task.TargetWordCount + " 字，绝对下限 " + task.TargetWordCount + " 字，绝对上限 " + maxWords + " 字（即目标字数的1.2倍），严禁低于下限、严禁超过上限！\n" +
                    "必须在字数上下限之间把全章写完，最后必须以完整的句子自然收尾；\n\n" +
                    "【排版要求】：\n" +
                    "严禁出现 markdown 标题符号（禁止使用 ##、###、** 等）。直接顺序列出一级标题及子小节标题并展开具体内容。\n" +
                    "请直接输出本章全部完整正文：";

            int maxTokens = Math.Clamp((int)(task.TargetWordCount * 1.6), 2000, 6000);
            var chapterRes = await CallApiWithTokensAsync(sys, prompt, maxTokens);

            if (!string.IsNullOrWhiteSpace(chapterRes))
            {
                var clean = chapterRes.Trim();
                if (!clean.StartsWith(task.Title)) clean = task.Title + "\n\n" + clean;
                clean = ChapterTask.TrimToWordLimit(clean, maxWords);
                combined.AppendLine(clean).AppendLine();
                _project.FullPaper = combined.ToString();
                TbPaper.Text = _project.FullPaper;
                HistoryManager.SaveProject(_project);
            }
        }

        SetStatus("【流水线 3/3】正在编制 100% 真实权威参考文献目录与查验表...");
        string sysRef = "你是一位国家重点学科高校图书文献检索专家。";
        string promptRef = "请为以下毕业论文编制一份权威、翔实、100%真实存在且近3-5年发表的【参考文献目录及可查链接检索表】：\n" +
                "【论文题目】：" + _project.Title + "\n" +
                "【学科专业】：" + _project.Discipline + " " + _project.Major + "\n" +
                "【论文大纲】：\n" + _project.Outline + "\n\n" +
                "【硬性要求】：\n" +
                "1. 列出 20 - 25 篇真实权威文献（含 15 篇以上中文核心/CSSCI/CSCD 期刊与硕博论文，5-8 篇英文 IEEE/ACM/SCI 国际知名期刊会议）；\n" +
                "2. 严禁任何虚构文献！作者名、刊物名、年份、卷期、起止页码必须符合真实学术引证；\n" +
                "3. 【极为重要 - 参考文献查询链接表】：在文末以标准的 Markdown 表格形式，输出【主要参考文献在线查验与检索链接表】，表格列包含：| 序号 | 论文题目 | 发表刊物/机构 | 年份 | DOI或知网/IEEE官方查询链接或检索号 |，方便读者或评审专家直接点击查验真实性！\n" +
                "4. 严格按照 GB/T 7714-2015 格式排版，不要任何 ## 或 ** 符号。";

        var refText = await CallApiWithTokensAsync(sysRef, promptRef, 3000);
        if (!string.IsNullOrWhiteSpace(refText))
        {
            combined.AppendLine("参考文献\n").AppendLine(refText);
            _project.FullPaper = combined.ToString();
            TbPaper.Text = _project.FullPaper;
            HistoryManager.SaveProject(_project);
        }

        BtnGeneratePaper.IsEnabled = true;
        SetStatus($"🎉 全篇万字论文正文撰写全部完成！当前总字数约 {_project.FullPaper.Length} 字。");
        MessageBox.Show($"论文正文各章节流水线推进完毕！\n当前正文总字数约 {_project.FullPaper.Length} 字。\n您可继续点击【导出 Word】或进行【深度去AI味】处理。", "生成成功", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void BtnReduceAigc_Click(object sender, RoutedEventArgs e)
    {
        var content = TbPaper.Text.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            MessageBox.Show("当前正文为空，请先生成论文正文！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SetStatus("🛡️ 正在进行学术降重与深度去AI味重构处理...");
        BtnReduceAigc.IsEnabled = false;

        string sys = "你是一位精通知网、维普、万方、PaperPass 等国家权威学术期刊 AIGC 审稿系统底层检测逻辑的资深主编。你掌握彻底消除大模型句法特征、压低 AIGC 困惑度与突发度（Perplexity & Burstiness）的核心学术重构技巧。";
        string prompt = "【极紧急学术任务：彻底粉碎 PaperPass / 知网 AIGC 特征检测，将 AI 检出率压至 5% 以下】\n\n" +
                "根据 PaperPass 官方权威红字检测报告显示，以下文本目前被系统判定为高达 75% 的高危 AIGC 模式！\n" +
                "请将以下内容重构成 100% 具备真实资深学者手写质感的顶级学术论文：\n\n" +
                "【原文内容】：\n" + (content.Length > 8000 ? content.Substring(0, 8000) : content) + "\n\n" +
                "【彻底击穿 PaperPass / 知网 AIGC 判定规则之核心禁令与改写法则】：\n" +
                "1. 【坚决粉碎 AI 八股套话与句式套路（命中 75% 的头号元凶）】：\n" +
                "   - 彻底删除并终生禁止出现以下词汇：“随着……的深入推进”、“本文旨在……”、“在……背景下”、“显而易见”、“不可否认”、“值得注意的是”、“由此可见”、“总而言之”、“综上所述”、“不仅……而且……”对仗套话！\n" +
                "   - 开篇第一句严禁用“随着”，必须直接以具体研究事实、田野数据、反差现象或学术矛盾切入！\n" +
                "2. 【拉爆行文突发度 (Burstiness) 与句式反差】：\n" +
                "   - AI 生成文章句长极度平稳（每句20-30字），极易被算法识破。必须交替使用“长短句对撞”：\n" +
                "     一个包含多重限定修饰成分的40-60字复杂复合长句，紧跟一个仅6-12字的短促论断短句，打碎机器的均匀概率节奏！\n" +
                "3. 【增加真实学者写作的情态助词与学术微瑕】：\n" +
                "   - 自然融入真实中文学术论文的高频习惯表达：如“诚然”、“质言之”、“毋庸讳言”、“进而言之”、“揆诸现实”、“殊途同归”；\n" +
                "   - 引入真实的学术限定语（如“在特定样本空间内”、“从目前可得之证据推演”、“尚难断言”），消除 AI 的绝对化与机械确定感；\n" +
                "4. 【被动与客观视角重构】：\n" +
                "   - 禁止出现“我们”、“本文认为”等口语化第一人称，全部转化为“经由……之测度”、“通过对……之溯源”、“在……视阈下”等客观学术语态；\n" +
                "5. 【排版与结构完整性】：\n" +
                "   - 严禁出现 markdown 标题符号（禁止使用 ##、###、** 等），段落完整自然，保留原文专业术语、数据与论证主线。\n\n" +
                "请直接输出重构后的高自然度、彻底清除 AIGC 痕迹的学术正文：";

        var res = await CallApiWithTokensAsync(sys, prompt, 6000);
        BtnReduceAigc.IsEnabled = true;

        if (!string.IsNullOrWhiteSpace(res))
        {
            _project.FullPaper = res;
            TbPaper.Text = res;
            HistoryManager.SaveProject(_project);
            SetStatus("🎉 深度去AI味完成！");
            MessageBox.Show("深度去 AI 味与 AIGC 降重处理完成！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            SetStatus("去AI味处理遇到问题。");
        }
    }

    private void BtnNextToDefense_Click(object sender, RoutedEventArgs e)
    {
        MainTabs.SelectedIndex = 4;
        SetStatus("已进入答辩准备阶段。");
    }

    private async void BtnGenerateDefense_Click(object sender, RoutedEventArgs e)
    {
        var outline = string.IsNullOrWhiteSpace(TbOutline.Text) ? _project.Outline : TbOutline.Text.Trim();
        if (string.IsNullOrWhiteSpace(outline))
        {
            MessageBox.Show("大纲为空，请先在第二步规划或输入大纲！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SetStatus("正在生成毕业答辩自述演讲稿及专家提问预案...");
        BtnGenerateDefense.IsEnabled = false;

        string sys = "你是一位高校答辩委员会主席，深谙毕业论文答辩评分标准与答辩礼仪技巧。";
        string prompt = "请为以下毕业论文编写一份【毕业答辩自述演讲稿】与【答辩专家可能提出的高频问题及回答策略预案】：\n" +
                "【论文题目】：" + _project.Title + "\n" +
                "【论文层次】：" + _project.Level + " / " + _project.Major + "\n" +
                "【论文核心大纲】：\n" + outline + "\n\n" +
                "【格式要求】：严禁使用 ##、** 等 markdown 符号，纯正中文排版。\n" +
                "第一部分：【5-8分钟答辩自述发言稿】（包含开头致谢各位评委老师、选题初衷与背景、核心研究工作与创新贡献、实验/应用成效、总结致谢，语气自信谦逊、时间把控精准）；\n\n" +
                "第二部分：【答辩评委专家高频追问 TOP 5 及标准满分回答策略】（针对本文技术瓶颈、创新点对比、实证数据真实性等评委最爱揪住的问题，提供专业且从容的回答要点与措辞示例）。";

        var res = await CallApiWithTokensAsync(sys, prompt, 4000);
        BtnGenerateDefense.IsEnabled = true;

        if (!string.IsNullOrWhiteSpace(res))
        {
            _project.DefenseSpeech = res;
            TbDefense.Text = res;
            HistoryManager.SaveProject(_project);
            SetStatus("答辩自述稿生成完成！");
        }
        else
        {
            SetStatus("答辩自述稿生成失败。");
        }
    }

    private async void BtnGeneratePpt_Click(object sender, RoutedEventArgs e)
    {
        var outline = string.IsNullOrWhiteSpace(TbOutline.Text) ? _project.Outline : TbOutline.Text.Trim();
        if (string.IsNullOrWhiteSpace(outline))
        {
            MessageBox.Show("大纲为空，请先在第二步规划或输入大纲！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SetStatus("正在设计标准 Markdown 格式的答辩PPT幻灯片逐页结构与演说备注...");
        BtnGeneratePpt.IsEnabled = false;

        string sys = "你是一位资深学术报告视觉与演讲辅导专家，精通 Marp / MindShow / 闪击 PPT 等 AI PPT 快速生成工具的 Markdown 语法标准。";
        string prompt = "请根据论文大纲，生成标准的【Markdown 格式答辩 PPT 逐页设计大纲及演讲备注】（约15-20页幻灯片）。用户将此 Markdown 复制到任何支持 Markdown 转 PPT 的软件或 AI 工具（如 Gamma, MindShow, Marp）中可直接一键生成 PPT：\n" +
                "【论文题目】：" + _project.Title + "\n" +
                "【论文大纲】：\n" + outline + "\n\n" +
                "【Markdown 规范要求】：\n" +
                "1. 每张幻灯片之间使用三个短横线 `---` 进行分页分隔；\n" +
                "2. 每页标题采用 `# 幻灯片标题`，核心要点采用有序或无序列表；\n" +
                "3. 每页文末包含 `<!-- 演讲备注: 这里是脱稿讲稿 -->`；\n" +
                "4. 覆盖：封面、选题背景、国内外现状痛点、总体技术架构与方案设计、关键创新点、核心实验与实证结果、总结反思、致谢。";

        var res = await CallApiWithTokensAsync(sys, prompt, 4000);
        BtnGeneratePpt.IsEnabled = true;

        if (!string.IsNullOrWhiteSpace(res))
        {
            _project.PptOutline = res;
            TbPpt.Text = res;
            HistoryManager.SaveProject(_project);
            SetStatus("PPT设计大纲生成完毕！");
        }
        else
        {
            SetStatus("PPT大纲生成失败。");
        }
    }

    private void ExportModuleDocx(string defaultFilename, string docTitle, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            MessageBox.Show("当前内容为空，无法导出！请先生成或输入内容。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dlg = new SaveFileDialog
        {
            Title = "导出 Word 文档",
            FileName = defaultFilename,
            Filter = "Word 文档 (*.docx)|*.docx"
        };

        if (dlg.ShowDialog() == true)
        {
            try
            {
                DocxExporter.ExportToDocx(dlg.FileName, docTitle, $"{_project.Discipline} {_project.Major}", content);
                SetStatus($"文档已成功导出至: {dlg.FileName}");
                var res = MessageBox.Show($"Word 文档已成功导出！\n路径: {dlg.FileName}\n\n是否立即打开查看？", "导出成功", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (res == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = dlg.FileName, UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出 Word 遇到错误: {ex.Message}", "导出失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void BtnExportOutlineDocx_Click(object sender, RoutedEventArgs e)
    {
        ExportModuleDocx($"{_project.Title}_论文大纲.docx", $"{_project.Title} 详细大纲", TbOutline.Text);
    }

    private void BtnExportProposalDocx_Click(object sender, RoutedEventArgs e)
    {
        ExportModuleDocx($"{_project.Title}_开题报告.docx", $"{_project.Title} 开题报告", TbProposal.Text);
    }

    private void BtnExportPaperDocx_Click(object sender, RoutedEventArgs e)
    {
        ExportModuleDocx($"{_project.Title}_毕业论文正文.docx", _project.Title, TbPaper.Text);
    }

    private void BtnExportDefenseDocx_Click(object sender, RoutedEventArgs e)
    {
        ExportModuleDocx($"{_project.Title}_答辩自述稿.docx", $"{_project.Title} 答辩自述演讲稿", TbDefense.Text);
    }

    private void BtnExportPptDocx_Click(object sender, RoutedEventArgs e)
    {
        ExportModuleDocx($"{_project.Title}_答辩PPT大纲.docx", $"{_project.Title} 答辩PPT逐页大纲", TbPpt.Text);
    }

    private void BtnHistory_Click(object sender, RoutedEventArgs e)
    {
        var historyProjects = HistoryManager.GetProjects();
        if (historyProjects.Count == 0)
        {
            MessageBox.Show("暂无历史论文项目记录！", "历史记录", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dlg = new Window
        {
            Title = "🕒 历史论文记录 (双击载入)",
            Width = 650,
            Height = 450,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this
        };

        var sp = new StackPanel { Margin = new Thickness(16) };
        var lb = new ListBox { Height = 320, FontSize = 13 };

        foreach (var p in historyProjects)
        {
            var updateTime = DateTimeOffset.FromUnixTimeMilliseconds(p.UpdatedAt).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            var item = new ListBoxItem
            {
                Content = $"【{p.Level}】{p.Title} ({p.Discipline}) - {updateTime}",
                Tag = p
            };
            lb.Items.Add(item);
        }

        lb.MouseDoubleClick += (s, ev) =>
        {
            if (lb.SelectedItem is ListBoxItem sel && sel.Tag is PaperProject prj)
            {
                _project = prj;
                LoadProjectToUi();
                SetStatus($"已恢复历史项目: {prj.Title}");
                dlg.Close();
            }
        };

        var btnLoad = new Button
        {
            Content = "载入选中项目",
            Width = 110,
            Height = 34,
            Margin = new Thickness(0, 10, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        btnLoad.Click += (s, ev) =>
        {
            if (lb.SelectedItem is ListBoxItem sel && sel.Tag is PaperProject prj)
            {
                _project = prj;
                LoadProjectToUi();
                SetStatus($"已恢复历史项目: {prj.Title}");
                dlg.Close();
            }
        };

        sp.Children.Add(lb);
        sp.Children.Add(btnLoad);
        dlg.Content = sp;
        dlg.ShowDialog();
    }

    private async void BtnCheckUpdate_Click(object sender, RoutedEventArgs e)
    {
        await UpdateChecker.CheckUpdateAsync(this, isManual: true);
    }

    private void BtnConfig_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new ConfigDialog(_config) { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            _config = _configManager.Load();
            SetStatus("系统与模型配置已更新。");
        }
    }
}
