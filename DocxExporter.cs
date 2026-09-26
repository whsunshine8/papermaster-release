using System;
using System.IO;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace PaperMasterWin;

public static class DocxExporter
{
    public static void ExportToDocx(string filePath, string documentTitle, string subInfo, string content)
    {
        // 彻底解决由于文件锁、只读或异常导致的闪退问题
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".docx");

        try
        {
            using (var doc = WordprocessingDocument.Create(tempFile, WordprocessingDocumentType.Document))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = new Body();

                // 1. 文档大标题 (小二号/28pt = sz 36, 黑体居中)
                var pTitle = new Paragraph();
                var pPrTitle = new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center },
                    new SpacingBetweenLines { Before = "240", After = "120" }
                );
                pTitle.Append(pPrTitle);
                var rTitle = new Run(
                    new RunProperties(
                        new RunFonts { Ascii = "SimHei", EastAsia = "SimHei" },
                        new FontSize { Val = "36" },
                        new Bold()
                    ),
                    new Text(documentTitle ?? "")
                );
                pTitle.Append(rTitle);
                body.Append(pTitle);

                // 2. 副标题或专业信息 (小四号/12pt = sz 24, 楷体居中)
                if (!string.IsNullOrWhiteSpace(subInfo))
                {
                    var pSub = new Paragraph();
                    pSub.Append(new ParagraphProperties(
                        new Justification { Val = JustificationValues.Center },
                        new SpacingBetweenLines { After = "240" }
                    ));
                    var rSub = new Run(
                        new RunProperties(
                            new RunFonts { Ascii = "KaiTi", EastAsia = "KaiTi" },
                            new FontSize { Val = "24" }
                        ),
                        new Text(subInfo)
                    );
                    pSub.Append(rSub);
                    body.Append(pSub);
                }

                // 3. 正文段落排版
                var lines = (content ?? "").Split('\n');
                foreach (var rawLine in lines)
                {
                    var line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    // 清理 Markdown 加粗符号
                    line = Regex.Replace(line, @"\*\*(.*?)\*\*", "$1");
                    line = Regex.Replace(line, @"^#{1,6}\s*", "");

                    var p = new Paragraph();

                    // 判断是否是主章节标题 (第X章 / 1. 绪论 / 摘要 / 参考文献 等)
                    if (Regex.IsMatch(line, @"^(第[一二三四五六七八九十0-9]+章|[0-9]+[、\. ]|摘要|Abstract|参考文献|致谢|总结与展望).*"))
                    {
                        p.Append(new ParagraphProperties(
                            new SpacingBetweenLines { Before = "360", After = "180" }
                        ));
                        p.Append(new Run(
                            new RunProperties(
                                new RunFonts { Ascii = "SimHei", EastAsia = "SimHei" },
                                new FontSize { Val = "28" },
                                new Bold()
                            ),
                            new Text(line)
                        ));
                    }
                    // 二级小节标题 (1.1 或 一、)
                    else if (Regex.IsMatch(line, @"^([0-9]+\.[0-9]+[、\. ]|[一二三四五六七八九十]+[、\. ]).*"))
                    {
                        p.Append(new ParagraphProperties(
                            new SpacingBetweenLines { Before = "200", After = "100" }
                        ));
                        p.Append(new Run(
                            new RunProperties(
                                new RunFonts { Ascii = "SimHei", EastAsia = "SimHei" },
                                new FontSize { Val = "26" },
                                new Bold()
                            ),
                            new Text(line)
                        ));
                    }
                    else
                    {
                        // 普通正文：宋体四号 (sz 28)，首行缩进 2 字符 (560 dxa)
                        p.Append(new ParagraphProperties(
                            new Indentation { FirstLine = "560" },
                            new SpacingBetweenLines { Line = "360", LineRule = LineSpacingRuleValues.Auto, After = "80" }
                        ));
                        p.Append(new Run(
                            new RunProperties(
                                new RunFonts { Ascii = "Times New Roman", EastAsia = "SimSun" },
                                new FontSize { Val = "28" }
                            ),
                            new Text(line) { Space = SpaceProcessingModeValues.Preserve }
                        ));
                    }

                    body.Append(p);
                }

                mainPart.Document.Body = body;
                mainPart.Document.Save();
            }

            // 安全复制到目标路径（覆盖模式）
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.Copy(tempFile, filePath, true);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                try { File.Delete(tempFile); } catch { }
            }
        }
    }
}
