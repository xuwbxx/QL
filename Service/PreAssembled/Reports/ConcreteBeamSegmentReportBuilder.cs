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
    /// 把 <see cref="SegmentReportModel"/> 渲染成 PDF 字节流。
    /// </summary>
    /// <remarks>
    /// 版式取向：<b>照原系统（MC3D Unlimited）1:1 还原</b>。
    /// 原件是「等宽字体 + 前导空格对齐」的纯文本流报告，没有表格边框，
    /// 因此这里不用 MigraDoc 表格，而是逐行铺等宽文本 —— 这样列对齐与原件的视觉一致，
    /// 且导出文本时可被复制/搜索。
    /// <para>
    /// 唯一与原件的差异：<b>不输出原软件厂商横幅</b>（原件首行 "MC3D Unlimited - Program Licensed to:"），
    /// 报告直接从 Project Title 开始。
    /// </para>
    /// </remarks>
    public static class ConcreteBeamSegmentReportBuilder
    {
        /// <summary>正文字号（pt）。</summary>
        private const double BodyFontSize = 9.0;

        /// <summary>页边距（厘米）。</summary>
        private const double MarginCm = 1.8;

        /// <summary>英文日期格式，与原件一致：Friday, October 06, 2023。</summary>
        private const string LongDateFormat = "dddd, MMMM dd, yyyy";

        /// <summary>页脚时间戳格式，与原件一致：10/06/23 14:19:55。</summary>
        private const string FooterTimeFormat = "MM/dd/yy HH:mm:ss";

        /// <summary>
        /// 生成报告 PDF。
        /// </summary>
        /// <param name="model">报告模型。</param>
        /// <returns>PDF 文件字节流。</returns>
        public static byte[] Build(SegmentReportModel model)
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
            RenderFooter(section, model);
            RenderBody(section, model);

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();
            renderer.PdfDocument.Info.CreationDate = model.ReportDate;

            using var stream = new MemoryStream();
            // closeStream: false —— 必须留下流给我们读取
            renderer.PdfDocument.Save(stream, false);
            return stream.ToArray();
        }

        private static Document CreateDocument(SegmentReportModel model)
        {
            var document = new Document();
            document.Info.Title = $"Segment Report - {model.WetCastSegmentName}";
            document.Info.Subject = model.Kind switch
            {
                SegmentReportKind.Setup => "Match-Cast Setup",
                SegmentReportKind.Pouring => "As-Cast Coordinates",
                _ => "As-Cast Segment Report"
            };
            // 注：MigraDoc 的 DocumentInfo 没有 CreationDate，文档创建时间在渲染后写到 PDF 信息字典上

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
        private static void RenderFooter(Section section, SegmentReportModel model)
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
            timeLine.AddText(ReportTime.WithOffset(
                model.ReportDate.ToString(FooterTimeFormat, CultureInfo.InvariantCulture),
                model.TimezoneOffsetMinutes));
        }

        private static void RenderBody(Section section, SegmentReportModel model)
        {
            // 头部：Project Title / Casting Set / Date，以及 Wet Cast、Match Cast 两行
            AddLine(section, $"    Project Title : {model.ProjectTitle}");
            AddLine(section, $"    Casting Set : {model.CastingSet}");
            AddLine(section, $"    Date: {ReportTime.WithOffset(FormatLongDate(model.ReportDate), model.TimezoneOffsetMinutes)}");
            AddLine(section, string.Empty);
            AddLine(section, BuildSegmentHeader("Wet   Cast Segment", model.WetCastSegmentName, model.WetCastCastOn, model.TimezoneOffsetMinutes));
            AddLine(section, BuildSegmentHeader("Match Cast Segment", model.MatchCastSegmentName, model.MatchCastCastOn, model.TimezoneOffsetMinutes, model.IsFirstSegment));
            AddLine(section, string.Empty);

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

        /// <summary>
        /// 头部那两行的对齐方式：名称列固定宽度，Cast on 再对齐一列，与原件观感一致。
        /// </summary>
        private static string BuildSegmentHeader(string label, string segmentName, DateTime? castOn, int? timezoneOffsetMinutes, bool suppressCastOn = false)
        {
            var name = string.IsNullOrWhiteSpace(segmentName) ? SegmentPlaceholder : segmentName;
            // 起始段没有匹配段：不输出 Cast on，避免显示 1970 哨兵时间
            if (suppressCastOn)
                return $"        {label} : {name.PadRight(20)}";
            var date = ReportTime.WithOffset(FormatLongDate(castOn ?? SegmentReportModel.NotCastSentinel), timezoneOffsetMinutes);
            return $"        {label} : {name.PadRight(20)} Cast on: {date}";
        }

        /// <summary>无匹配段（起始段）时匹配段名称的占位符。用 ASCII 双连字符，避免单字节编码下的破折号字形风险。</summary>
        private const string SegmentPlaceholder = "--";

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
