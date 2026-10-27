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
    /// 把 <see cref="OrientationReportModel"/> 渲染成 PDF 字节流。
    /// <para>两段式：上半 General Coordinates（joint 坐标表），下半 ORIENTATION REPORT（PrintReport 几何报告）。</para>
    /// <para>用 A4 横向 + 表格，保证 9 个坐标列 / 10 个报告列在 PDF 里可读；末页页脚带页码与生成时间（含时区）。</para>
    /// </summary>
    public static class ConcreteBeamOrientationReportBuilder
    {
        private const double BodyFontSize = 7.5;
        private const double TitleFontSize = 8.5;
        private const double MarginCm = 1.2;
        private const string LongDateFormat = "dddd, MMMM dd, yyyy";
        private const string FooterTimeFormat = "MM/dd/yy HH:mm:ss";

        /// <summary>生成 PDF。</summary>
        public static byte[] Build(OrientationReportModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            ReportFonts.EnsureInitialized();

            var document = new Document();
            document.Info.Title = "Orientation Report";
            document.Info.Subject = "Design Coordinate Check and Preview";

            var normal = document.Styles[StyleNames.Normal];
            normal.Font.Name = ReportFonts.MonoFamily;
            normal.Font.Size = BodyFontSize;
            normal.ParagraphFormat.SpaceBefore = Unit.Zero;
            normal.ParagraphFormat.SpaceAfter = Unit.Zero;
            normal.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;

            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.LeftMargin = Unit.FromCentimeter(MarginCm);
            section.PageSetup.RightMargin = Unit.FromCentimeter(MarginCm);
            section.PageSetup.TopMargin = Unit.FromCentimeter(MarginCm);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(MarginCm);
            section.PageSetup.FooterDistance = Unit.FromCentimeter(0.7);

            RenderHeader(section, model);
            AddSectionTitle(section, "GENERAL COORDINATES");
            AddTable(section, model.JointHeaders, model.JointRows);
            AddBlank(section);
            AddSectionTitle(section, "ORIENTATION REPORT");
            AddOrientationReportTable(section, model.ReportRows);
            RenderFooter(section, model);

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();
            renderer.PdfDocument.Info.CreationDate = model.ReportDate;

            using var stream = new MemoryStream();
            renderer.PdfDocument.Save(stream, false);
            return stream.ToArray();
        }

        private static void RenderHeader(Section section, OrientationReportModel model)
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
            paragraph.Format.SpaceAfter = Unit.FromPoint(4);
            AddFormattedText(paragraph, title, TitleFontSize, true);
        }

        private static void AddLine(Section section, string text)
        {
            var paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = Unit.Zero;
            paragraph.Format.SpaceAfter = Unit.Zero;
            paragraph.Format.LineSpacingRule = LineSpacingRule.Single;
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
        /// 表格：表头加粗、数据行逐格写入；列数取表头与数据行的最大值，列宽均分横向可用宽度。
        /// </summary>
        private static void AddTable(Section section, List<string> headers, List<List<string>> rows)
        {
            var colCount = headers.Count;
            foreach (var row in rows)
                if (row.Count > colCount) colCount = row.Count;
            if (colCount == 0) return;

            var table = CreateTable(section, colCount);

            var header = table.AddRow();
            header.HeadingFormat = true;
            for (var i = 0; i < colCount; i++)
                SetHeaderCell(header, i, i < headers.Count ? headers[i] : string.Empty);

            AddDataRows(table, rows, colCount);
        }

        /// <summary>
        /// ORIENTATION REPORT 的复杂表头（两行）：
        /// 第一行 Joint / B_East / B_West / Bulkhead Angle / Long Offset / Cross-Slope / Z-Axis；
        /// Bulkhead Angle、Long Offset、Cross-Slope 各跨 East/West 两列，第二行写 East/West。
        /// </summary>
        private static void AddOrientationReportTable(Section section, List<List<string>> rows)
        {
            const int colCount = 10;
            var table = CreateTable(section, colCount);

            // 两行表头先建出来，再设合并，避免 MergeDown 找不到下方单元格
            var h1 = table.AddRow();
            var h2 = table.AddRow();
            h1.HeadingFormat = true;
            h2.HeadingFormat = true;

            // 单列：跨两行
            SetHeaderCell(h1, 0, "Joint", mergeDown: 1);
            SetHeaderCell(h1, 1, "B_East", mergeDown: 1);
            SetHeaderCell(h1, 2, "B_West", mergeDown: 1);
            // 分组：只跨两列（第二行还要写 East/West，不能向下合并）
            SetHeaderCell(h1, 3, "Bulkhead Angle", mergeRight: 1);
            SetHeaderCell(h1, 5, "Long Offset", mergeRight: 1);
            SetHeaderCell(h1, 7, "Cross-Slope", mergeRight: 1);
            // 单列：跨两行
            SetHeaderCell(h1, 9, "Z-Axis", mergeDown: 1);

            // 第二行：三个分组的 East / West 子列
            SetHeaderCell(h2, 3, "East");
            SetHeaderCell(h2, 4, "West");
            SetHeaderCell(h2, 5, "East");
            SetHeaderCell(h2, 6, "West");
            SetHeaderCell(h2, 7, "East");
            SetHeaderCell(h2, 8, "West");

            AddDataRows(table, rows, colCount);
        }

        /// <summary>建表：透明边框 + 均分列宽（仅用于布局，不显示表格线）。</summary>
        private static Table CreateTable(Section section, int colCount)
        {
            var table = section.AddTable();
            table.Borders.Width = 0;
            table.Borders.Style = BorderStyle.None;
            table.Format.Font.Name = ReportFonts.MonoFamily;
            table.Format.Font.Size = BodyFontSize;

            var contentWidthCm = 29.7 - 2 * MarginCm;
            var colWidth = Unit.FromCentimeter(contentWidthCm / colCount);
            for (var i = 0; i < colCount; i++) table.AddColumn(colWidth);

            return table;
        }

        /// <summary>写一个表头单元格；mergeRight / mergeDown 用于复杂表头。</summary>
        private static void SetHeaderCell(Row row, int index, string text, int mergeRight = 0, int mergeDown = 0)
        {
            var cell = row.Cells[index];
            cell.MergeRight = mergeRight;
            cell.MergeDown = mergeDown;
            cell.Format.Font.Bold = true;
            var paragraph = cell.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            AddFormattedText(paragraph, text, BodyFontSize, true);
        }

        /// <summary>逐行写数据；列数不足的补空串。</summary>
        private static void AddDataRows(Table table, List<List<string>> rows, int colCount)
        {
            foreach (var row in rows)
            {
                var tableRow = table.AddRow();
                for (var i = 0; i < colCount; i++)
                {
                    var text = i < row.Count ? row[i] : string.Empty;
                    var paragraph = tableRow.Cells[i].AddParagraph();
                    paragraph.Format.Alignment = ParagraphAlignment.Right;
                    AddFormattedText(paragraph, text, BodyFontSize, false);
                }
            }
        }

        private static void RenderFooter(Section section, OrientationReportModel model)
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

        /// <summary>
        /// 写入文本：CJK（含全角标点）交给兜底字体，其余用等宽字体。
        /// 与单梁段/整体报告同一套字体策略，避免中文在 PDF 里变成空白。
        /// </summary>
        private static void AddFormattedText(Paragraph paragraph, string text, double size, bool bold)
        {
            if (string.IsNullOrEmpty(text))
            {
                paragraph.AddText(" ");
                return;
            }

            // 支持主动换行：调用方用 \n 把“数值”和“偏差”分成两行，这里转成 MigraDoc 的 LineBreak。
            var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                if (lineIndex > 0) paragraph.AddLineBreak();

                foreach (var (chunk, isCjk) in SplitByScript(lines[lineIndex]))
                {
                    var formatted = paragraph.AddFormattedText(chunk);
                    formatted.Font.Name = isCjk
                        ? ReportFonts.CjkFamily
                        : (bold ? ReportFonts.MonoBoldFamily : ReportFonts.MonoFamily);
                    formatted.Font.Size = size;
                }
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
            => (c >= 0x2E80 && c <= 0x9FFF)     // 部首扩展 / 汉字
               || (c >= 0xF900 && c <= 0xFAFF)  // 兼容汉字
               || (c >= 0x3000 && c <= 0x303F)  // CJK 符号与标点
               || (c >= 0xFF00 && c <= 0xFFEF); // 全角形式
    }
}
