using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

namespace Service.PreAssembled.Reports
{
    /// <summary>
    /// 把 <see cref="OverallReportModel"/> 渲染成 PDF 字节流（【报告】页签 · 整体报告）。
    /// </summary>
    /// <remarks>
    /// 版式与 <see cref="ConcreteBeamSegmentReportBuilder"/> 完全一致：等宽字体 + 前导空格对齐的
    /// 纯文本流，不用 MigraDoc 表格，且同样<b>不输出原软件厂商横幅</b>。
    /// 唯一差异：整体报告跨梁段，没有"本段 / 匹配段"的概念，故头部之后不输出
    /// Wet Cast / Match Cast 两行，直接从正文开始。
    /// <para>
    /// 与单梁段渲染器分家的原因：<see cref="ConcreteBeamSegmentReportBuilder"/> 当前被 TFS 锁定为只读，
    /// 无法在其中新增重载。待其签出后，应把两者的共用部分（页面设置 / 页脚 / 逐行铺文本 / 字体切分）
    /// 抽到共享类，本类只保留"头部不打印梁段行"这一点差异。
    /// </para>
    /// </remarks>
    public static class ConcreteBeamOverallReportBuilder
    {
        /// <summary>正文字号（pt）。</summary>
        private const double BodyFontSize = 9.0;

        /// <summary>页边距（厘米）。</summary>
        private const double MarginCm = 1.8;

        /// <summary>英文日期格式，与原件一致：Friday, October 06, 2023。</summary>
        private const string LongDateFormat = "dddd, MMMM dd, yyyy";

        /// <summary>页脚时间戳格式，与原件一致：10/06/23 14:19:55。</summary>
        private const string FooterTimeFormat = "MM/dd/yy HH:mm:ss";

        /// <summary>生成整体报告 PDF。</summary>
        /// <param name="model">报告模型。</param>
        /// <returns>PDF 文件字节流。</returns>
        public static byte[] Build(OverallReportModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // Core 构建必须显式注册字体解析器，且 GlobalFontSettings.FontResolver 只能赋值一次
            ReportFonts.EnsureInitialized();

            var document = CreateDocument(model);
            var section = document.AddSection();
            ApplyPageSetup(section);
            RenderFooter(section, model.ReportDate);
            RenderBody(section, model);

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();
            // 注：MigraDoc 的 DocumentInfo 没有 CreationDate，文档创建时间在渲染后写到 PDF 信息字典上
            renderer.PdfDocument.Info.CreationDate = model.ReportDate;

            using var stream = new MemoryStream();
            // closeStream: false —— 必须留下流给我们读取
            renderer.PdfDocument.Save(stream, false);
            return stream.ToArray();
        }

        private static Document CreateDocument(OverallReportModel model)
        {
            var document = new Document();
            document.Info.Title = model.DocumentTitle;
            document.Info.Subject = model.DocumentTitle;

            var normal = document.Styles[StyleNames.Normal];
            normal.Font.Name = ReportFonts.MonoFamily;
            normal.Font.Size = BodyFontSize;
            normal.ParagraphFormat.SpaceBefore = Unit.Zero;
            normal.ParagraphFormat.SpaceAfter = Unit.Zero;
            normal.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;

            return document;
        }

        private static void ApplyPageSetup(Section section)
        {
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.LeftMargin = Unit.FromCentimeter(MarginCm);
            section.PageSetup.RightMargin = Unit.FromCentimeter(MarginCm);
            section.PageSetup.TopMargin = Unit.FromCentimeter(MarginCm);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(MarginCm);
            section.PageSetup.FooterDistance = Unit.FromCentimeter(0.9);
        }

        /// <summary>页脚：左「Page x of y」，右生成时间戳（与原件一致）。</summary>
        private static void RenderFooter(Section section, DateTime reportDate)
        {
            var footer = section.Footers.Primary;

            var pageLine = footer.AddParagraph();
            pageLine.Format.Alignment = ParagraphAlignment.Left;
            pageLine.AddText("Page ");
            pageLine.AddPageField();
            pageLine.AddText(" of ");
            pageLine.AddNumPagesField();

            var timeLine = footer.AddParagraph();
            timeLine.Format.Alignment = ParagraphAlignment.Right;
            timeLine.AddText(reportDate.ToString(FooterTimeFormat, CultureInfo.InvariantCulture));
        }

        private static void RenderBody(Section section, OverallReportModel model)
        {
            // 头部三行与单梁段报告一致；其后直接进入正文（整体报告没有 Wet Cast / Match Cast 行）
            AddLine(section, $"    Project Title : {model.ProjectTitle}");
            AddLine(section, $"    Casting Set : {model.CastingSet}");
            AddLine(section, $"    Date: {FormatLongDate(model.ReportDate)}");

            foreach (var line in model.Lines)
            {
                if (string.IsNullOrEmpty(line.Text))
                {
                    AddLine(section, string.Empty);
                    continue;
                }

                AddMixedLine(section, line);
            }
        }

        private static void AddLine(Section section, string text)
        {
            var paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = Unit.Zero;
            paragraph.Format.SpaceAfter = Unit.Zero;
            paragraph.Format.LineSpacingRule = LineSpacingRule.Single;

            if (text.Length > 0)
            {
                AddMixedText(paragraph, text, BodyFontSize, false);
            }
            else
            {
                // 空行也要占位：给个空格，否则 MigraDoc 会把它压成零高
                paragraph.AddText(" ");
            }
        }

        private static void AddMixedLine(Section section, ReportLine line)
        {
            var paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = Unit.Zero;
            paragraph.Format.SpaceAfter = Unit.Zero;
            paragraph.Format.LineSpacingRule = LineSpacingRule.Single;

            var text = line.LeadingSpaces > 0 ? new string(' ', line.LeadingSpaces) + line.Text : line.Text;
            AddMixedText(paragraph, text, BodyFontSize, line.Bold);
        }

        /// <summary>
        /// 写入文本，并按字符集切成多段分别指定字体：
        /// Fira Mono 不含 CJK 字形，项目名/梁段名里的中文必须切到兜底字体，否则渲染成空白。
        /// </summary>
        private static void AddMixedText(Paragraph paragraph, string text, double size, bool bold)
        {
            foreach (var (chunk, isCjk) in SplitByScript(text))
            {
                var formatted = paragraph.AddFormattedText(chunk);
                formatted.Font.Name = isCjk
                    ? ReportFonts.CjkFamily
                    : (bold ? ReportFonts.MonoBoldFamily : ReportFonts.MonoFamily);
                formatted.Font.Size = size;
            }
        }

        /// <summary>按「CJK/全角」与「其余」把字符串切成若干块。</summary>
        private static IEnumerable<(string Text, bool IsCjk)> SplitByScript(string text)
        {
            var buffer = new StringBuilder();
            bool? current = null;

            foreach (var ch in text)
            {
                var isCjk = IsCjk(ch);
                if (current.HasValue && isCjk != current.Value)
                {
                    yield return (buffer.ToString(), current.Value);
                    buffer.Clear();
                }

                current = isCjk;
                buffer.Append(ch);
            }

            if (buffer.Length > 0)
            {
                yield return (buffer.ToString(), current ?? false);
            }
        }

        /// <summary>
        /// 判断字符是否交给 CJK 兜底字体。除汉字外，全角标点（U+3000–303F / U+FF00–FFEF）
        /// 也归入此类——Fira Mono 同样不含这些字形。
        /// </summary>
        private static bool IsCjk(char c)
            => (c >= 0x2E80 && c <= 0x9FFF)     // 部首扩展 / 汉字
               || (c >= 0xF900 && c <= 0xFAFF)  // 兼容汉字
               || (c >= 0x3000 && c <= 0x303F)  // CJK 符号与标点
               || (c >= 0xFF00 && c <= 0xFFEF); // 全角形式

        /// <summary>英文长日期（Friday, October 06, 2023），固定用不变文化，避免随服务端语言变化。</summary>
        private static string FormatLongDate(DateTime date)
            => date.ToString(LongDateFormat, CultureInfo.InvariantCulture);
    }
}
