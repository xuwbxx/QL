using ClosedXML.Excel;
using Service.PreAssembled.BridgeSegmentAlignment;
using System.Globalization;
using Tool;

namespace Service.PreAssembled.Reports
{
    // 说明：本文件原在 Tool/ConcreteBeamExcelHelper.cs，随 SteelBeamExcelHelper 一并迁入
    // Service/PreAssembled/Reports（业务专属的导入解析，不属通用工具）。
    // Excel 的打开/读取/校验/样式/模板骨架等通用能力统一走 Tool.ExcelUtils，本文件只保留业务部分。

    public sealed record ConcreteBeamSegmentImportRow(
        int ExcelRow, string SegmentName, string? Downstream, string? Upstream,
        string Node1, string Node2, DateTime? PouringDate);

    public sealed record ConcreteBeamDesignCoordinateImportRow(
        int ExcelRow, string NodeName,
        decimal XE, decimal YE, decimal ZE,
        decimal XC, decimal YC, decimal ZC,
        decimal XW, decimal YW, decimal ZW)
    {
        public decimal ZEWithoutCamber { get; init; }
        public decimal ZCWithoutCamber { get; init; }
        public decimal ZWWithoutCamber { get; init; }
    }
    public sealed record ConcreteBeamCamberImportRow(int ExcelRow, string NodeName, decimal? ZE, decimal ZC, decimal? ZW);

    public sealed record ConcreteBeamImportError(
        int ExcelRow, string RecordKey, string FieldName, string OriginalValue, string Reason);

    public sealed class ConcreteBeamParseResult<T>
    {
        public List<T> Rows { get; } = new();
        public List<ConcreteBeamImportError> Errors { get; } = new();
        public bool Success => Rows.Count > 0 && Errors.Count == 0;
    }

    public static class ConcreteBeamExcelHelper
    {
        public static readonly string[] SegmentHeaders =
            ["梁段名称", "下游", "上游", "节点1", "节点2", "浇筑日期"];

        public static readonly string[] DesignCoordinateHeaders =
            ["节点名称", "XE", "YE", "ZE", "XC", "YC", "ZC", "XW", "YW", "ZW"];
        public static readonly string[] CamberHeaders = ["节点名称", "ZE", "ZC", "ZW"];

        public static ConcreteBeamParseResult<ConcreteBeamSegmentImportRow> ParseSegments(Stream stream, string? lang)
        {
            var result = new ConcreteBeamParseResult<ConcreteBeamSegmentImportRow>();
            ExcelUtils.ExcelErrorSink addError = (row, rowKey, field, originalValue, reason)
                => result.Errors.Add(new(row, rowKey, field, originalValue, reason));

            using var workbook = ExcelUtils.OpenWorkbook(stream);
            var sheet = ExcelUtils.FirstWorksheet(workbook);
            var columns = ExcelUtils.ReadHeaderMap(sheet, SegmentHeaders, lang, addError);
            if (result.Errors.Count > 0) return result;
            var lastRow = ValidateDataCount(sheet, columns.Values, result.Errors, lang);
            if (result.Errors.Count > 0) return result;

            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var row = 2; row <= lastRow; row++)
            {
                if (ExcelUtils.IsEmptyRow(sheet, row, columns.Values)) continue;
                var name = ReadText(sheet.Cell(row, columns["梁段名称"]));
                ExcelUtils.ValidateNoFormula(sheet, row, columns, name, lang, addError);
                ExcelUtils.ValidateRequiredText(row, name, "梁段名称", name, 100, lang, addError);
                if (!string.IsNullOrWhiteSpace(name) && !names.Add(name))
                    result.Errors.Add(new(row, name, "梁段名称", name, BizMessages.DuplicateSegmentName(lang)));
                var downstream = ReadOptionalText(sheet.Cell(row, columns["下游"]));
                var upstream = ReadOptionalText(sheet.Cell(row, columns["上游"]));
                var node1 = ReadText(sheet.Cell(row, columns["节点1"]));
                var node2 = ReadText(sheet.Cell(row, columns["节点2"]));
                ExcelUtils.ValidateRequiredText(row, name, "节点1", node1, 100, lang, addError);
                ExcelUtils.ValidateRequiredText(row, name, "节点2", node2, 100, lang, addError);
                if (!string.IsNullOrWhiteSpace(node1) && string.Equals(node1, node2, StringComparison.OrdinalIgnoreCase))
                    result.Errors.Add(new(row, name, "节点2", node2, BizMessages.Node2SameAsNode1(lang)));
                var pouringDate = ReadOptionalDate(sheet.Cell(row, columns["浇筑日期"]), row, name, result.Errors, lang);
                if (!result.Errors.Any(a => a.ExcelRow == row))
                    result.Rows.Add(new(row, name, downstream, upstream, node1, node2, pouringDate));
            }
            return result;
        }

        public static ConcreteBeamParseResult<ConcreteBeamDesignCoordinateImportRow> ParseDesignCoordinates(Stream stream, string? lang)
        {
            var result = new ConcreteBeamParseResult<ConcreteBeamDesignCoordinateImportRow>();
            ExcelUtils.ExcelErrorSink addError = (row, rowKey, field, originalValue, reason)
                => result.Errors.Add(new(row, rowKey, field, originalValue, reason));

            using var workbook = ExcelUtils.OpenWorkbook(stream);
            var sheet = ExcelUtils.FirstWorksheet(workbook);
            var columns = ExcelUtils.ReadHeaderMap(sheet, DesignCoordinateHeaders, lang, addError);
            if (result.Errors.Count > 0) return result;
            var lastRow = ValidateDataCount(sheet, columns.Values, result.Errors, lang);
            if (result.Errors.Count > 0) return result;

            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var xcValues = new List<(int ExcelRow, string NodeName, decimal Value, string RawValue)>();
            for (var row = 2; row <= lastRow; row++)
            {
                if (ExcelUtils.IsEmptyRow(sheet, row, columns.Values)) continue;
                var name = ReadText(sheet.Cell(row, columns["节点名称"]));
                ExcelUtils.ValidateNoFormula(sheet, row, columns, name, lang, addError);
                ExcelUtils.ValidateRequiredText(row, name, "节点名称", name, 100, lang, addError);
                if (!string.IsNullOrWhiteSpace(name) && !names.Add(name))
                    result.Errors.Add(new(row, name, "节点名称", name, BizMessages.DuplicateNodeName(lang)));
                var values = DesignCoordinateHeaders.Skip(1)
                    .Select(field => ExcelUtils.ReadDecimal(sheet.Cell(row, columns[field]), row, name, field, 4, null, null, lang, addError))
                    .ToArray();
                if (values[3] is decimal xcValue)
                    xcValues.Add((row, name, xcValue,
                        sheet.Cell(row, columns["XC"]).GetFormattedString()));
                if (!result.Errors.Any(a => a.ExcelRow == row))
                    result.Rows.Add(new(row, name, values[0]!.Value, values[1]!.Value, values[2]!.Value,
                        values[3]!.Value, values[4]!.Value, values[5]!.Value,
                        values[6]!.Value, values[7]!.Value, values[8]!.Value));
            }
            ValidateXcMonotonicity(xcValues, result.Errors, lang);
            return result;
        }

        public static ConcreteBeamParseResult<ConcreteBeamCamberImportRow> ParseCamber(Stream stream, string? lang)
        {
            var result = new ConcreteBeamParseResult<ConcreteBeamCamberImportRow>();
            ExcelUtils.ExcelErrorSink addError = (row, rowKey, field, originalValue, reason)
                => result.Errors.Add(new(row, rowKey, field, originalValue, reason));

            using var workbook = ExcelUtils.OpenWorkbook(stream);
            var sheet = ExcelUtils.FirstWorksheet(workbook);
            var columns = ExcelUtils.ReadHeaderMap(sheet, CamberHeaders, lang, addError);
            if (result.Errors.Count > 0) return result;
            var lastRow = ValidateDataCount(sheet, columns.Values, result.Errors, lang);
            var names = new HashSet<string>(StringComparer.Ordinal);
            for (var row = 2; row <= lastRow; row++)
            {
                if (ExcelUtils.IsEmptyRow(sheet, row, columns.Values)) continue;
                var name = ReadText(sheet.Cell(row, columns["节点名称"]));
                ExcelUtils.ValidateNoFormula(sheet, row, columns, name, lang, addError);
                ExcelUtils.ValidateRequiredText(row, name, "节点名称", name, 100, lang, addError);
                if (!string.IsNullOrWhiteSpace(name) && !names.Add(name)) result.Errors.Add(new(row, name, "节点名称", name, BizMessages.DuplicateNodeName(lang)));
                var ze = ReadOptionalDecimal(sheet.Cell(row, columns["ZE"]), row, name, "ZE", lang, addError);
                var zc = ExcelUtils.ReadDecimal(sheet.Cell(row, columns["ZC"]), row, name, "ZC", 4, null, null, lang, addError);
                var zw = ReadOptionalDecimal(sheet.Cell(row, columns["ZW"]), row, name, "ZW", lang, addError);
                if (!result.Errors.Any(a => a.ExcelRow == row)) result.Rows.Add(new(row, name, ze, zc!.Value, zw));
            }
            return result;
        }

        public static byte[] CreateSegmentTemplate() => ExcelUtils.CreateTemplate(SegmentHeaders,
        [
            ["梁段名称", "是", "同一浇筑组内唯一，最多100个字符", "KF-P2-1U"],
            ["下游/上游", "否", "允许为空，不做内容校验", "KF-P2-0U"],
            ["节点1/节点2", "是", "最多100个字符，两个节点不得相同", "1 / 2"],
            ["浇筑日期", "否", "格式：yyyy-MM-dd", "2026-08-31"]
        ]);

        public static byte[] CreateDesignCoordinateTemplate() => ExcelUtils.CreateTemplate(DesignCoordinateHeaders,
        [
            ["节点名称", "是", "同一浇筑组内唯一，最多100个字符", "KF-P2-1U"],
            ["XE/YE/ZE", "是", "东侧控制点坐标，单位m，最多4位小数", "713038.0931"],
            ["XC", "是", "单位m，最多4位小数；按数据行顺序必须严格递增或严格递减，不允许相等", "713038.0931"],
            ["YC/ZC", "是", "中间控制点坐标，单位m，最多4位小数", "713038.0931"],
            ["XW/YW/ZW", "是", "西侧控制点坐标，单位m，最多4位小数", "713038.0931"]
        ]);
        public static byte[] CreateCamberTemplate() => ExcelUtils.CreateTemplate(CamberHeaders,
        [
            ["节点名称", "是", "名称和数量必须与当前浇筑组的设计坐标完全一致", "KF-P2-1U"],
            ["ZE/ZW", "否", "允许为空；为空时计算表使用ZC填充；有值时最多4位小数", "0.0150"],
            ["ZC", "是", "不能为空，最多4位小数", "0.0120"]
        ]);

        public static byte[] CreateErrorFile(IEnumerable<ConcreteBeamImportError> errors, string? lang)
        {
            var headers = BizMessages.ErrorFileHeaders(lang);
            var rows = errors.Select(error => new[]
            {
                error.ExcelRow.ToString(),
                error.RecordKey ?? string.Empty,
                error.FieldName ?? string.Empty,
                error.OriginalValue ?? string.Empty,
                error.Reason ?? string.Empty
            });
            return ExcelUtils.CreateTableWorkbook(BizMessages.ErrorSheetName(lang), headers, rows);
        }

        private static void ValidateXcMonotonicity(
            IReadOnlyList<(int ExcelRow, string NodeName, decimal Value, string RawValue)> values,
            List<ConcreteBeamImportError> errors, string? lang)
        {
            var direction = 0;
            for (var index = 1; index < values.Count; index++)
            {
                var current = values[index];
                var comparison = current.Value.CompareTo(values[index - 1].Value);
                if (comparison == 0)
                {
                    errors.Add(new(current.ExcelRow, current.NodeName, "XC", current.RawValue, BizMessages.XcNotMonotonic(lang)));
                    continue;
                }

                if (direction == 0)
                {
                    direction = comparison;
                    continue;
                }

                if (comparison != direction)
                    errors.Add(new(current.ExcelRow, current.NodeName, "XC", current.RawValue, BizMessages.XcDirectionInconsistent(lang)));
            }
        }

        private static int ValidateDataCount(IXLWorksheet sheet, IEnumerable<int> columns, List<ConcreteBeamImportError> errors, string? lang)
        {
            var columnList = columns.ToArray();
            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
            var count = ExcelUtils.CountNonEmptyRows(sheet, 2, lastRow, columnList);
            if (count == 0) errors.Add(new(0, string.Empty, BizMessages.FieldFile(lang), string.Empty, BizMessages.NoDataInFirstSheet(lang)));
            else if (count > ExcelUtils.MaxDataRows) errors.Add(new(0, string.Empty, BizMessages.FieldFile(lang), count.ToString(), BizMessages.TooManyRows(lang, ExcelUtils.MaxDataRows)));
            return lastRow;
        }

        private static DateTime? ReadOptionalDate(IXLCell cell, int row, string key, List<ConcreteBeamImportError> errors, string? lang)
        {
            if (cell.IsEmpty()) return null;
            var raw = cell.GetFormattedString();
            if (cell.DataType == XLDataType.DateTime && cell.TryGetValue<DateTime>(out var date)) return date.Date;
            if (DateTime.TryParseExact(cell.GetString().Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out date)) return date.Date;
            errors.Add(new(row, key, "浇筑日期", raw, BizMessages.PouringDateFormat(lang)));
            return null;
        }

        /// <summary>必填数值：空/非数字报"不能为空"/"必须是有效数字"。</summary>
        private static decimal? ReadOptionalDecimal(IXLCell cell, int row, string key, string field,
            string? lang, ExcelUtils.ExcelErrorSink addError)
        {
            if (cell.IsEmpty() || string.IsNullOrWhiteSpace(cell.GetFormattedString())) return null;
            return ExcelUtils.ReadDecimal(cell, row, key, field, 4, null, null, lang, addError);
        }

        // 与 ExcelUtils.ReadText 的差别只有 Trim：本业务的表头与录入值都按 Trim 后比较
        private static string ReadText(IXLCell cell) => cell.HasFormula ? string.Empty : cell.GetString().Trim();
        private static string? ReadOptionalText(IXLCell cell) => string.IsNullOrWhiteSpace(ReadText(cell)) ? null : ReadText(cell);
    }
}
