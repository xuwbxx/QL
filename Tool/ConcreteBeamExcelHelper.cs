using ClosedXML.Excel;
using System.Globalization;

namespace Tool;

public sealed record ConcreteBeamSegmentImportRow(
    int ExcelRow, string SegmentName, string? Downstream, string? Upstream,
    string Node1, string Node2, DateTime? PouringDate);

public sealed record ConcreteBeamDesignCoordinateImportRow(
    int ExcelRow, string NodeName,
    decimal XE, decimal YE, decimal ZE,
    decimal XC, decimal YC, decimal ZC,
    decimal XW, decimal YW, decimal ZW);
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
    public const int MaxDataRows = 50_000;
    public const long MaxFileBytes = 20L * 1024 * 1024;

    public static readonly string[] SegmentHeaders =
        ["梁段名称", "下游", "上游", "节点1", "节点2", "浇筑日期"];

    public static readonly string[] DesignCoordinateHeaders =
        ["节点名称", "XE", "YE", "ZE", "XC", "YC", "ZC", "XW", "YW", "ZW"];
    public static readonly string[] CamberHeaders = ["节点名称", "ZE", "ZC", "ZW"];

    public static ConcreteBeamParseResult<ConcreteBeamSegmentImportRow> ParseSegments(Stream stream)
    {
        var result = new ConcreteBeamParseResult<ConcreteBeamSegmentImportRow>();
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheet(1);
        var columns = ValidateHeaders(sheet, SegmentHeaders, result.Errors);
        if (result.Errors.Count > 0) return result;
        var lastRow = ValidateDataCount(sheet, columns.Values, result.Errors);
        if (result.Errors.Count > 0) return result;

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var row = 2; row <= lastRow; row++)
        {
            if (IsEmptyRow(sheet, row, columns.Values)) continue;
            var name = ReadText(sheet.Cell(row, columns["梁段名称"]));
            ValidateNoFormula(sheet, row, columns, result.Errors, name);
            ValidateRequiredText(row, name, "梁段名称", name, 100, result.Errors);
            if (!string.IsNullOrWhiteSpace(name) && !names.Add(name))
                result.Errors.Add(new(row, name, "梁段名称", name, "同一文件内梁段名称重复（忽略大小写）"));
            var downstream = ReadOptionalText(sheet.Cell(row, columns["下游"]));
            var upstream = ReadOptionalText(sheet.Cell(row, columns["上游"]));
            var node1 = ReadText(sheet.Cell(row, columns["节点1"]));
            var node2 = ReadText(sheet.Cell(row, columns["节点2"]));
            ValidateRequiredText(row, name, "节点1", node1, 100, result.Errors);
            ValidateRequiredText(row, name, "节点2", node2, 100, result.Errors);
            if (!string.IsNullOrWhiteSpace(node1) && string.Equals(node1, node2, StringComparison.OrdinalIgnoreCase))
                result.Errors.Add(new(row, name, "节点2", node2, "节点2不得与节点1相同"));
            var pouringDate = ReadOptionalDate(sheet.Cell(row, columns["浇筑日期"]), row, name, result.Errors);
            if (!result.Errors.Any(a => a.ExcelRow == row))
                result.Rows.Add(new(row, name, downstream, upstream, node1, node2, pouringDate));
        }
        return result;
    }

    public static ConcreteBeamParseResult<ConcreteBeamDesignCoordinateImportRow> ParseDesignCoordinates(Stream stream)
    {
        var result = new ConcreteBeamParseResult<ConcreteBeamDesignCoordinateImportRow>();
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheet(1);
        var columns = ValidateHeaders(sheet, DesignCoordinateHeaders, result.Errors);
        if (result.Errors.Count > 0) return result;
        var lastRow = ValidateDataCount(sheet, columns.Values, result.Errors);
        if (result.Errors.Count > 0) return result;

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var xcValues = new List<(int ExcelRow, string NodeName, decimal Value, string RawValue)>();
        for (var row = 2; row <= lastRow; row++)
        {
            if (IsEmptyRow(sheet, row, columns.Values)) continue;
            var name = ReadText(sheet.Cell(row, columns["节点名称"]));
            ValidateNoFormula(sheet, row, columns, result.Errors, name);
            ValidateRequiredText(row, name, "节点名称", name, 100, result.Errors);
            if (!string.IsNullOrWhiteSpace(name) && !names.Add(name))
                result.Errors.Add(new(row, name, "节点名称", name, "同一文件内节点名称重复（忽略大小写）"));
            var values = DesignCoordinateHeaders.Skip(1)
                .Select(field => ReadRequiredDecimal(sheet.Cell(row, columns[field]), row, name, field, result.Errors))
                .ToArray();
            if (values[3] is decimal xcValue)
                xcValues.Add((row, name, xcValue,
                    sheet.Cell(row, columns["XC"]).GetFormattedString()));
            if (!result.Errors.Any(a => a.ExcelRow == row))
                result.Rows.Add(new(row, name, values[0]!.Value, values[1]!.Value, values[2]!.Value,
                    values[3]!.Value, values[4]!.Value, values[5]!.Value,
                    values[6]!.Value, values[7]!.Value, values[8]!.Value));
        }
        ValidateXcMonotonicity(xcValues, result.Errors);
        return result;
    }

    public static ConcreteBeamParseResult<ConcreteBeamCamberImportRow> ParseCamber(Stream stream)
    {
        var result = new ConcreteBeamParseResult<ConcreteBeamCamberImportRow>();
        using var workbook = new XLWorkbook(stream); var sheet = workbook.Worksheet(1);
        var columns = ValidateHeaders(sheet, CamberHeaders, result.Errors);
        if (result.Errors.Count > 0) return result;
        var lastRow = ValidateDataCount(sheet, columns.Values, result.Errors);
        var names = new HashSet<string>(StringComparer.Ordinal);
        for (var row = 2; row <= lastRow; row++)
        {
            if (IsEmptyRow(sheet, row, columns.Values)) continue;
            var name = ReadText(sheet.Cell(row, columns["节点名称"]));
            ValidateNoFormula(sheet, row, columns, result.Errors, name);
            ValidateRequiredText(row, name, "节点名称", name, 100, result.Errors);
            if (!string.IsNullOrWhiteSpace(name) && !names.Add(name)) result.Errors.Add(new(row, name, "节点名称", name, "同一文件内节点名称重复"));
            var ze = ReadOptionalDecimal(sheet.Cell(row, columns["ZE"]), row, name, "ZE", result.Errors);
            var zc = ReadRequiredDecimal(sheet.Cell(row, columns["ZC"]), row, name, "ZC", result.Errors);
            var zw = ReadOptionalDecimal(sheet.Cell(row, columns["ZW"]), row, name, "ZW", result.Errors);
            if (!result.Errors.Any(a => a.ExcelRow == row)) result.Rows.Add(new(row, name, ze, zc!.Value, zw));
        }
        return result;
    }

    public static byte[] CreateSegmentTemplate() => CreateTemplate(SegmentHeaders,
    [
        ["梁段名称", "是", "同一浇筑组内唯一，最多100个字符", "KF-P2-1U"],
        ["下游/上游", "否", "允许为空，不做内容校验", "KF-P2-0U"],
        ["节点1/节点2", "是", "最多100个字符，两个节点不得相同", "1 / 2"],
        ["浇筑日期", "否", "格式：yyyy-MM-dd", "2026-08-31"]
    ]);

    public static byte[] CreateDesignCoordinateTemplate() => CreateTemplate(DesignCoordinateHeaders,
    [
        ["节点名称", "是", "同一浇筑组内唯一，最多100个字符", "KF-P2-1U"],
        ["XE/YE/ZE", "是", "东侧控制点坐标，单位m，最多4位小数", "713038.0931"],
        ["XC", "是", "单位m，最多4位小数；按数据行顺序必须严格递增或严格递减，不允许相等", "713038.0931"],
        ["YC/ZC", "是", "中间控制点坐标，单位m，最多4位小数", "713038.0931"],
        ["XW/YW/ZW", "是", "西侧控制点坐标，单位m，最多4位小数", "713038.0931"]
    ]);
    public static byte[] CreateCamberTemplate() => CreateTemplate(CamberHeaders,
    [
        ["节点名称", "是", "名称和数量必须与当前浇筑组的设计坐标完全一致", "KF-P2-1U"],
        ["ZE/ZW", "否", "允许为空；为空时计算表使用ZC填充；有值时最多4位小数", "0.0150"],
        ["ZC", "是", "不能为空，最多4位小数", "0.0120"]
    ]);

    public static byte[] CreateErrorFile(IEnumerable<ConcreteBeamImportError> errors)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("错误明细");
        var headers = new[] { "行号", "测点编号", "字段名称", "原始值", "失败原因" };
        for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
        var row = 2;
        foreach (var error in errors)
        {
            sheet.Cell(row, 1).Value = error.ExcelRow;
            sheet.Cell(row, 2).Value = error.RecordKey;
            sheet.Cell(row, 3).Value = error.FieldName;
            sheet.Cell(row, 4).Value = error.OriginalValue;
            sheet.Cell(row, 5).Value = error.Reason;
            row++;
        }
        Style(sheet, headers.Length);
        sheet.SheetView.FreezeRows(1);
        using var output = new MemoryStream();
        workbook.SaveAs(output);
        return output.ToArray();
    }

    public static bool HasScale(decimal value, int maximumScale)
    {
        var bits = decimal.GetBits(value);
        return ((bits[3] >> 16) & 0x7F) <= maximumScale;
    }

    private static void ValidateXcMonotonicity(
        IReadOnlyList<(int ExcelRow, string NodeName, decimal Value, string RawValue)> values,
        List<ConcreteBeamImportError> errors)
    {
        var direction = 0;
        for (var index = 1; index < values.Count; index++)
        {
            var current = values[index];
            var comparison = current.Value.CompareTo(values[index - 1].Value);
            if (comparison == 0)
            {
                errors.Add(new(current.ExcelRow, current.NodeName, "XC", current.RawValue,
                    "XC必须按数据行顺序严格递增或严格递减，不允许相邻行相等"));
                continue;
            }

            if (direction == 0)
            {
                direction = comparison;
                continue;
            }

            if (comparison != direction)
                errors.Add(new(current.ExcelRow, current.NodeName, "XC", current.RawValue,
                    "XC必须按数据行顺序保持同一递增或递减方向"));
        }
    }

    private static int ValidateDataCount(IXLWorksheet sheet, IEnumerable<int> columns, List<ConcreteBeamImportError> errors)
    {
        var columnList = columns.ToArray();
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        var count = Enumerable.Range(2, Math.Max(lastRow - 1, 0)).Count(row => !IsEmptyRow(sheet, row, columnList));
        if (count == 0) errors.Add(new(0, string.Empty, "文件", string.Empty, "第一个工作表没有数据"));
        else if (count > MaxDataRows) errors.Add(new(0, string.Empty, "文件", count.ToString(), $"数据行数不能超过{MaxDataRows}条"));
        return lastRow;
    }

    private static Dictionary<string, int> ValidateHeaders(IXLWorksheet sheet, string[] required, List<ConcreteBeamImportError> errors)
    {
        var actual = new Dictionary<string, int>(StringComparer.Ordinal);
        var lastColumn = sheet.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;
        for (var column = 1; column <= lastColumn; column++)
        {
            var name = sheet.Cell(1, column).GetString().Trim();
            if (!string.IsNullOrEmpty(name) && !actual.ContainsKey(name)) actual[name] = column;
        }
        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var name in required)
        {
            if (actual.TryGetValue(name, out var column)) result[name] = column;
            else errors.Add(new(1, string.Empty, name, string.Empty, $"缺少必需列：{name}"));
        }
        return result;
    }

    private static void ValidateNoFormula(IXLWorksheet sheet, int row, Dictionary<string, int> columns,
        List<ConcreteBeamImportError> errors, string key)
    {
        foreach (var column in columns)
        {
            var cell = sheet.Cell(row, column.Value);
            if (cell.HasFormula) errors.Add(new(row, key, column.Key, cell.FormulaA1, "导入字段不允许使用公式"));
        }
    }

    private static decimal? ReadRequiredDecimal(IXLCell cell, int row, string key, string field,
        List<ConcreteBeamImportError> errors)
    {
        var raw = cell.GetFormattedString();
        if (cell.IsEmpty())
        {
            errors.Add(new(row, key, field, raw, $"{field}不能为空"));
            return null;
        }
        decimal value;
        if (cell.DataType == XLDataType.Number) value = Convert.ToDecimal(cell.GetDouble(), CultureInfo.InvariantCulture);
        else if (!decimal.TryParse(cell.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out value))
        {
            errors.Add(new(row, key, field, raw, $"{field}必须是有效数字"));
            return null;
        }
        if (!HasScale(value, 4)) errors.Add(new(row, key, field, raw, $"{field}最多保留4位小数"));
        return value;
    }
    private static decimal? ReadOptionalDecimal(IXLCell cell, int row, string key, string field, List<ConcreteBeamImportError> errors)
    {
        if (cell.IsEmpty() || string.IsNullOrWhiteSpace(cell.GetFormattedString())) return null;
        var raw = cell.GetFormattedString(); decimal value;
        if (cell.DataType == XLDataType.Number) value = Convert.ToDecimal(cell.GetDouble(), CultureInfo.InvariantCulture);
        else if (!decimal.TryParse(cell.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out value))
        { errors.Add(new(row, key, field, raw, $"{field}必须是有效数字或为空")); return null; }
        if (!HasScale(value, 4)) errors.Add(new(row, key, field, raw, $"{field}最多保留4位小数"));
        return value;
    }

    private static DateTime? ReadOptionalDate(IXLCell cell, int row, string key, List<ConcreteBeamImportError> errors)
    {
        if (cell.IsEmpty()) return null;
        var raw = cell.GetFormattedString();
        if (cell.DataType == XLDataType.DateTime && cell.TryGetValue<DateTime>(out var date)) return date.Date;
        if (DateTime.TryParseExact(cell.GetString().Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out date)) return date.Date;
        errors.Add(new(row, key, "浇筑日期", raw, "浇筑日期格式必须为yyyy-MM-dd"));
        return null;
    }

    private static void ValidateRequiredText(int row, string key, string field, string value, int maxLength,
        List<ConcreteBeamImportError> errors)
    {
        if (string.IsNullOrWhiteSpace(value)) errors.Add(new(row, key, field, value, $"{field}不能为空"));
        else if (value.Length > maxLength) errors.Add(new(row, key, field, value, $"{field}不能超过{maxLength}个字符"));
    }

    private static string ReadText(IXLCell cell) => cell.HasFormula ? string.Empty : cell.GetString().Trim();
    private static string? ReadOptionalText(IXLCell cell) => string.IsNullOrWhiteSpace(ReadText(cell)) ? null : ReadText(cell);
    private static bool IsEmptyRow(IXLWorksheet sheet, int row, IEnumerable<int> columns) => columns.All(c => sheet.Cell(row, c).IsEmpty());

    private static byte[] CreateTemplate(string[] headers, string[][] instructions)
    {
        using var workbook = new XLWorkbook();
        var data = workbook.AddWorksheet("数据");
        for (var i = 0; i < headers.Length; i++) data.Cell(1, i + 1).Value = headers[i];
        Style(data, headers.Length);
        data.SheetView.FreezeRows(1);
        var help = workbook.AddWorksheet("填写说明");
        var helpHeaders = new[] { "字段", "必填", "规则", "示例" };
        for (var i = 0; i < helpHeaders.Length; i++) help.Cell(1, i + 1).Value = helpHeaders[i];
        for (var row = 0; row < instructions.Length; row++)
            for (var column = 0; column < instructions[row].Length; column++)
                help.Cell(row + 2, column + 1).Value = instructions[row][column];
        Style(help, helpHeaders.Length);
        using var output = new MemoryStream();
        workbook.SaveAs(output);
        return output.ToArray();
    }

    private static void Style(IXLWorksheet sheet, int columns)
    {
        var header = sheet.Range(1, 1, 1, columns);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF2FF");
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Columns(1, columns).AdjustToContents(12, 50);
        sheet.RangeUsed()?.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        sheet.RangeUsed()?.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
    }
}
