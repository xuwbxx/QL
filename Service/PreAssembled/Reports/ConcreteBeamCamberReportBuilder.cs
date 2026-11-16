using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace Service.PreAssembled.Reports
{
    /// <summary>
    /// 预拱度「打印/预览」PDF 渲染：
    /// 页首三行 Project Title / Casting Set / Date（含时区），空行后 section 标题 Camber:，
    /// 下方为与界面一致的预拱度表格（No. / Name / ZE / ZC / ZW）。
    /// <para>A4 纵向；页脚左侧页码、右侧生成时间，与设计坐标预览 PDF 保持同一风格。</para>
    /// </summary>
    public static class ConcreteBeamCamberReportBuilder
    {
        private const double BodyFontSize = 9;
        private const double TitleFontSize = 10;
        private const double MarginCm = 2.0;
        private const string LongDateFormat = "dddd, MMMM dd, yyyy";
        private const string FooterTimeFormat = "MM/dd/yy HH:mm:ss";
        private const string HeaderShading = "#F5F7FA";
        private const string StripeShading = "#FAFAFA";
        private const string BorderColor = "#EBEEF5";

        /// <summary>生成 PDF 字节流。</summary>
        public static byte[] Build(CamberReportModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            ReportFonts.EnsureInitialized();

            var document = new Document();
            document.Info.Title = "Camber";
            document.Info.Subject = "Camber Print";

            var normal = document.Styles[StyleNames.Normal];
            normal.Font.Name = ReportFonts.MonoFamily;
            normal.Font.Size = BodyFontSize;
            normal.ParagraphFormat.SpaceBefore = Unit.Zero;
            normal.ParagraphFormat.SpaceAfter = Unit.Zero;
            normal.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;

            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.Orientation = Orientation.Portrait;
            section.PageSetup.LeftMargin = Unit.FromCentimeter(MarginCm);
            section.PageSetup.RightMargin = Unit.FromCentimeter(MarginCm);
            section.PageSetup.TopMargin = Unit.FromCentimeter(MarginCm);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(MarginCm);
            section.PageSetup.FooterDistance = Unit.FromCentimeter(1.0);

            RenderHeader(section, model);
            AddSectionTitle(section, "Camber:");
            AddTable(section, model.Headers, model.Rows);
            RenderFooter(section, model);

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();
            renderer.PdfDocument.Info.CreationDate = model.ReportDate;

            using var stream = new MemoryStream();
            renderer.PdfDocument.Save(stream, false);
            return stream.ToArray();
        }

        private static void RenderHeader(Section section, CamberReportModel model)
        {
            AddLine(section, $"Project Title : {model.ProjectTitle}");
            AddLine(section, $"Casting Set  : {model.CastingSet}");
            AddLine(section,
                $"Date         : {ReportTime.WithOffset(model.ReportDate.ToString(LongDateFormat, CultureInfo.InvariantCulture), model.TimezoneOffsetMinutes)}");
            AddBlank(section);
        }

        private static void AddSectionTitle(Section section, string title)
        {
            var paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = Unit.Zero;
            paragraph.Format.SpaceAfter = Unit.FromPoint(6);
            AddFormattedText(paragraph, title, TitleFontSize, true);
        }

        private static void AddLine(Section section, string text)
        {
            var paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = Unit.Zero;
            paragraph.Format.SpaceAfter = Unit.Zero;
            paragraph.Format.LineSpacingRule = LineSpacingRule.Single;
            // 与原报告一致：标签 + 值之间用等宽空格对齐
            AddFormattedText(paragraph, text, BodyFontSize, false);
        }

        private static void AddBlank(Section section)
        {
            var paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = Unit.Zero;
            paragraph.Format.SpaceAfter = Unit.Zero;
            paragraph.AddText(" ");
        }

        /// <summary>
        /// 预拱度表格：浅灰表头 + 细边框 + 隔行浅底纹，列序 No. / Name / ZE / ZC / ZW，与界面展示一致。
        /// </summary>
        private static void AddTable(Section section, List<string> headers, List<List<string>> rows)
        {
            var colCount = Math.Max(headers.Count, 5);
            if (colCount == 0) return;

            var table = section.AddTable();
            table.Borders.Width = Unit.FromPoint(0.5);
            table.Borders.Color = Colors.White;
            table.Format.Font.Name = ReportFonts.MonoFamily;
            table.Format.Font.Size = BodyFontSize;
            table.Rows.LeftIndent = Unit.Zero;

            var contentWidthCm = 21.0 - 2 * MarginCm;
            // No. / Name 略窄，ZE/ZC/ZW 均分剩余宽度
            var noWidth = Unit.FromCentimeter(1.6);
            var nameWidth = Unit.FromCentimeter(3.6);
            var valueWidth = Unit.FromCentimeter((contentWidthCm - 1.6 - 3.6) / 3.0);
            var widths = new List<Unit> { noWidth, nameWidth };
            for (var i = 2; i < colCount; i++) widths.Add(valueWidth);
            for (var i = 0; i < colCount; i++) table.AddColumn(i < widths.Count ? widths[i] : valueWidth);

            // 表头
            var header = table.AddRow();
            header.HeadingFormat = true;
            header.Shading.Color = ParseColor(HeaderShading);
            for (var i = 0; i < colCount; i++)
            {
                var cell = header.Cells[i];
                cell.Borders.Width = Unit.FromPoint(0.5);
                cell.Borders.Color = ParseColor(BorderColor);
                var paragraph = cell.AddParagraph();
                paragraph.Format.Alignment = ParagraphAlignment.Center;
                paragraph.Format.SpaceBefore = Unit.FromPoint(2);
                paragraph.Format.SpaceAfter = Unit.FromPoint(2);
                AddFormattedText(paragraph, i < headers.Count ? headers[i] : string.Empty, BodyFontSize, true);
            }

            // 数据行
            for (var r = 0; r < rows.Count; r++)
            {
                var row = table.AddRow();
                row.Shading.Color = r % 2 == 1 ? ParseColor(StripeShading) : Colors.White;
                for (var i = 0; i < colCount; i++)
                {
                    var cell = row.Cells[i];
                    cell.Borders.Width = Unit.FromPoint(0.5);
                    cell.Borders.Color = ParseColor(BorderColor);
                    var paragraph = cell.AddParagraph();
                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                    paragraph.Format.SpaceBefore = Unit.FromPoint(1.5);
                    paragraph.Format.SpaceAfter = Unit.FromPoint(1.5);
                    var text = i < rows[r].Count ? rows[r][i] : string.Empty;
                    AddFormattedText(paragraph, text, BodyFontSize, false);
                }
            }
        }

        private static Color ParseColor(string hex)
        {
            // #RRGGBB → MigraDoc Color
            var v = hex.TrimStart('#');
            return Color.FromRgb(
                Convert.ToByte(v.Substring(0, 2), 16),
                Convert.ToByte(v.Substring(2, 2), 16),
                Convert.ToByte(v.Substring(4, 2), 16));
        }

        private static void RenderFooter(Section section, CamberReportModel model)
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

        /// <summary>CJK（含全角标点）走兜底字体，其余走等宽字体，避免中文在 PDF 里变成空白。</summary>
        private static void AddFormattedText(Paragraph paragraph, string text, double size, bool bold)
        {
            if (string.IsNullOrEmpty(text))
            {
                paragraph.AddText(" ");
                return;
            }

            foreach (var (chunk, isCjk) in SplitByScript(text))
            {
                var formatted = paragraph.AddFormattedText(chunk);
                formatted.Font.Name = isCjk
                    ? ReportFonts.CjkFamily
                    : (bold ? ReportFonts.MonoBoldFamily : ReportFonts.MonoFamily);
                formatted.Font.Size = size;
            }
        }

        private static IEnumerable<(string Text, bool IsCjk)> SplitByScript(string text)
        {
            var buffer = new System.Text.StringBuilder();
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
                yield return (buffer.ToString(), current ?? false);
        }

        private static bool IsCjk(char c)
            => (c >= 0x2E80 && c <= 0x9FFF)
               || (c >= 0xF900 && c <= 0xFAFF)
               || (c >= 0x3000 && c <= 0x303F)
               || (c >= 0xFF00 && c <= 0xFFEF);
    }
}
