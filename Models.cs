using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PaperMasterWin;

public class PaperProject
{
    public string Id { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
    public long UpdatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public string Title { get; set; } = "";
    public string Level { get; set; } = "本科毕业论文";
    public string Discipline { get; set; } = "工学 (计算机/电子信息/土木/机械/电气/自动化等)";
    public string Major { get; set; } = "";
    public string OutlineLevel { get; set; } = "三级目录 (例如: 1.1.1 结构紧凑规范 - 推荐)";
    public int WordCount { get; set; } = 10000;
    public string Template { get; set; } = "标准学术实证/工程设计模板 (绪论 - 理论与技术 - 现状分析 - 核心系统架构/模型设计 - 实现与验证 - 结论)";

    public string Outline { get; set; } = "";
    public string Proposal { get; set; } = "";
    public string FullPaper { get; set; } = "";
    public string DefenseSpeech { get; set; } = "";
    public string PptOutline { get; set; } = "";

    public int CurrentChapterIndex { get; set; } = 0;
    public int CurrentStep { get; set; } = 1;
    public bool IsPaperFinished { get; set; } = false;
}

public class ChapterTask
{
    public int ChapterIndex { get; set; }
    public string Title { get; set; } = "";
    public string OutlineDetails { get; set; } = "";
    public int TargetWordCount { get; set; }
    public string GeneratedContent { get; set; } = "";
    public bool IsCompleted { get; set; } = false;

    public ChapterTask(int index, string title, string details, int wordCount)
    {
        ChapterIndex = index;
        Title = title;
        OutlineDetails = details;
        TargetWordCount = wordCount;
    }

    public static string CleanTitle(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";
        string s = raw.Trim();
        s = Regex.Replace(s, @"[（(【]\s*(规划|预计|目标)?字数\s*[:：]?\s*[0-9,，~\-—至到]+\s*字?\s*[)）】]", "");
        s = Regex.Replace(s, @"(规划|预计|目标)?字数\s*[:：]\s*[0-9,，~\-—至到]+\s*字?", "");
        s = Regex.Replace(s, @"[（(【]\s*约?\s*[0-9,，~\-—至到]+\s*字\s*[)）】]", "");
        return s.Trim();
    }

    public static int CountWords(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        int n = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (!char.IsWhiteSpace(text[i])) n++;
        }
        return n;
    }

    public static string TrimToWordLimit(string text, int limit)
    {
        if (string.IsNullOrEmpty(text)) return "";
        if (CountWords(text) <= limit) return text;
        int n = 0, cut = -1;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (!char.IsWhiteSpace(c)) n++;
            if (c == '。' || c == '！' || c == '？' || c == '!' || c == '?' || c == '\n')
            {
                if (n <= limit) cut = i;
            }
            if (n > limit) break;
        }
        if (cut >= 0)
        {
            return text.Substring(0, cut + 1).Trim();
        }
        int n2 = 0, idx = 0;
        for (; idx < text.Length; idx++)
        {
            if (!char.IsWhiteSpace(text[idx])) n2++;
            if (n2 >= limit - 1) break;
        }
        return text.Substring(0, Math.Min(idx + 1, text.Length)).Trim() + "。";
    }

    public static List<ChapterTask> ParseOutlineChapters(string outline, int totalWordCount)
    {
        var tasks = new List<ChapterTask>();
        if (string.IsNullOrWhiteSpace(outline)) return tasks;

        var lines = outline.Split('\n');
        string curTitle = "";
        var curDetails = new System.Text.StringBuilder();
        int curIndex = 0;

        foreach (var line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            if (string.IsNullOrEmpty(curTitle) && (trimmed.StartsWith("【") || trimmed.Contains("大纲") || trimmed.Contains("题目")))
                continue;

            if (IsChapterHeader(trimmed))
            {
                if (!string.IsNullOrEmpty(curTitle))
                {
                    tasks.Add(new ChapterTask(curIndex, curTitle, curDetails.ToString().Trim(), 0));
                    curDetails.Clear();
                }
                curIndex++;
                curTitle = CleanTitle(trimmed);
            }
            else
            {
                if (!string.IsNullOrEmpty(curTitle))
                {
                    curDetails.AppendLine(trimmed);
                }
            }
        }

        if (!string.IsNullOrEmpty(curTitle))
        {
            tasks.Add(new ChapterTask(curIndex, curTitle, curDetails.ToString().Trim(), 0));
        }

        if (tasks.Count == 0)
        {
            tasks.Add(new ChapterTask(1, "正文核心内容", outline, totalWordCount));
            return tasks;
        }

        int count = tasks.Count;
        for (int i = 0; i < count; i++)
        {
            int words;
            if (count >= 5)
            {
                if (i == 0) words = (int)(totalWordCount * 0.12);
                else if (i == count - 1) words = (int)(totalWordCount * 0.10);
                else words = (int)((totalWordCount * 0.78) / (count - 2));
            }
            else
            {
                words = totalWordCount / count;
            }
            if (words < 500) words = 500;
            tasks[i].TargetWordCount = words;
        }

        return tasks;
    }

    private static bool IsChapterHeader(string line)
    {
        string clean = Regex.Replace(line, @"^[#\*\s]+", "");
        if (Regex.IsMatch(clean, @"^(参考文献|致谢|附录|答辩).*")) return false;
        if (Regex.IsMatch(clean, @"^第[一二三四五六七八九十0-9]+[章节篇部].*")) return true;
        if (Regex.IsMatch(clean, @"^[一二三四五六七八九十]+[、\. ].*")) return true;
        if (Regex.IsMatch(clean, @"^[0-9]+[、\. ][^0-9].*") && !Regex.IsMatch(clean, @"^[0-9]+\.[0-9]+.*")) return true;
        if (Regex.IsMatch(clean, @"^[0-9]+\s+[^0-9\s].*")) return true;
        if (Regex.IsMatch(clean, @"^(绪论|引言|结论|总结与展望|结语)($|[\s：:]).*")) return true;
        return false;
    }
}
