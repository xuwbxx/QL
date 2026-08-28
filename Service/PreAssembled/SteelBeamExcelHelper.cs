using ClosedXML.Excel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Service.PreAssembled;

public sealed record SteelBeamTheoreticalImportRow(
    int ExcelRow, string PointCode, decimal DesignX, decimal DesignY, decimal DesignZ,
    decimal PreCamber, decimal Weight, string SegmentNo, string PositionName);

public sealed record SteelBeamMeasuredImportRow(
    int ExcelRow, string PointCode, decimal MeasuredX, decimal MeasuredY, decimal MeasuredZ);

public sealed record SteelBeamImportError(
    int ExcelRow, string PointCode, string FieldName, string OriginalValue, string Reason);

public sealed class SteelBeamParseResult<T>
{
    public List<T> Rows { get; } = new();
    public List<(int ExcelRow, string PointCode)> SourcePoints { get; } = new();
    public List<SteelBeamImportError> Errors { get; } = new();
    public bool Success => Errors.Count == 0 && Rows.Count > 0;
}

public static class SteelBeamExcelHelper
{
    public const int MaxDataRows = 50_000;
    public const long MaxFileBytes = 20L * 1024 * 1024;

    public static readonly string[] TheoreticalHeaders =
    [
        "测点编号", "设计坐标X(m)", "设计坐标Y(m)", "设计坐标Z(m)",
        "预拱度(m)", "权值", "所属梁段号", "测位名称"
    ];

    public static readonly string[] MeasuredHeaders =
    [
        "测点编号", "实测坐标X(m)", "实测坐标Y(m)", "实测坐标Z(m)"
    ];

    public static SteelBeamParseResult<SteelBeamTheoreticalImportRow> ParseTheoretical(Stream stream)
    {
        var result = new SteelBeamParseResult<SteelBeamTheoreticalImportRow>();
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheet(1);
        var columns = ValidateHeaders(sheet, TheoreticalHeaders, result.Errors);
        if (result.Errors.Count > 0) return result;

        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        var dataCount = CountNonEmptyRows(sheet, 2, lastRow, columns.Values);
        if (dataCount == 0)
        {
            result.Errors.Add(new(0, string.Empty, "文件", string.Empty, "第一个工作表没有数据"));
            return result;
        }
        if (dataCount > MaxDataRows)
        {
            result.Errors.Add(new(0, string.Empty, "文件", dataCount.ToString(), $"数据行数不能超过{MaxDataRows}条"));
            return result;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            if (IsEmptyRow(sheet, rowNumber, columns.Values)) continue;
            var point = ReadText(sheet.Cell(rowNumber, columns["测点编号"]));
            result.SourcePoints.Add((rowNumber, point));
            ValidateNoFormula(sheet, rowNumber, columns, result.Errors, point);
            ValidatePointCode(rowNumber, point, result.Errors);
            if (!string.IsNullOrEmpty(point) && !seen.Add(point))
                result.Errors.Add(new(rowNumber, point, "测点编号", point, "文件内测点编号重复（忽略大小写）"));

            var x = ReadDecimal(sheet.Cell(rowNumber, columns["设计坐标X(m)"]), rowNumber, point, "设计坐标X(m)", 6, null, null, result.Errors);
            var y = ReadDecimal(sheet.Cell(rowNumber, columns["设计坐标Y(m)"]), rowNumber, point, "设计坐标Y(m)", 6, null, null, result.Errors);
            var z = ReadDecimal(sheet.Cell(rowNumber, columns["设计坐标Z(m)"]), rowNumber, point, "设计坐标Z(m)", 6, null, null, result.Errors);
            var camber = ReadDecimal(sheet.Cell(rowNumber, columns["预拱度(m)"]), rowNumber, point, "预拱度(m)", 6, null, null, result.Errors);
            var weight = ReadDecimal(sheet.Cell(rowNumber, columns["权值"]), rowNumber, point, "权值", 4, 0, 1, result.Errors);
            var segment = ReadText(sheet.Cell(rowNumber, columns["所属梁段号"])).Trim();
            var position = ReadText(sheet.Cell(rowNumber, columns["测位名称"])).Trim();
            ValidateRequiredText(rowNumber, point, "所属梁段号", segment, 50, result.Errors);
            ValidateRequiredText(rowNumber, point, "测位名称", position, 100, result.Errors);

            if (!result.Errors.Any(e => e.ExcelRow == rowNumber))
                result.Rows.Add(new(rowNumber, point, x!.Value, y!.Value, z!.Value, camber!.Value, weight!.Value, segment, position));
        }
        return result;
    }

    public static SteelBeamParseResult<SteelBeamMeasuredImportRow> ParseMeasured(Stream stream)
    {
        var result = new SteelBeamParseResult<SteelBeamMeasuredImportRow>();
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheet(1);
        var columns = ValidateHeaders(sheet, MeasuredHeaders, result.Errors);
        if (result.Errors.Count > 0) return result;

        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        var dataCount = CountNonEmptyRows(sheet, 2, lastRow, columns.Values);
        if (dataCount == 0)
        {
            result.Errors.Add(new(0, string.Empty, "文件", string.Empty, "第一个工作表没有数据"));
            return result;
        }
        if (dataCount > MaxDataRows)
        {
            result.Errors.Add(new(0, string.Empty, "文件", dataCount.ToString(), $"数据行数不能超过{MaxDataRows}条"));
            return result;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            if (IsEmptyRow(sheet, rowNumber, columns.Values)) continue;
            var point = ReadText(sheet.Cell(rowNumber, columns["测点编号"]));
            result.SourcePoints.Add((rowNumber, point));
            ValidateNoFormula(sheet, rowNumber, columns, result.Errors, point);
            ValidatePointCode(rowNumber, point, result.Errors);
            if (!string.IsNullOrEmpty(point) && !seen.Add(point))
                result.Errors.Add(new(rowNumber, point, "测点编号", point, "文件内测点编号重复（忽略大小写）"));
            var x = ReadDecimal(sheet.Cell(rowNumber, columns["实测坐标X(m)"]), rowNumber, point, "实测坐标X(m)", 6, null, null, result.Errors);
            var y = ReadDecimal(sheet.Cell(rowNumber, columns["实测坐标Y(m)"]), rowNumber, point, "实测坐标Y(m)", 6, null, null, result.Errors);
            var z = ReadDecimal(sheet.Cell(rowNumber, columns["实测坐标Z(m)"]), rowNumber, point, "实测坐标Z(m)", 6, null, null, result.Errors);
            if (!result.Errors.Any(e => e.ExcelRow == rowNumber))
                result.Rows.Add(new(rowNumber, point, x!.Value, y!.Value, z!.Value));
        }
        return result;
    }

    public static byte[] CreateTheoreticalTemplate() => CreateTemplate(TheoreticalHeaders, new[]
    {
        new[] { "测点编号", "是", "1-50位可见半角字符；禁止空格、中文、换行及以=、+、-、@开头", "GS-Z-1-4" },
        new[] { "设计坐标X/Y/Z(m)", "是", "允许正负数和0，最多6位小数", "713033.449300" },
        new[] { "预拱度(m)", "是", "允许正负数和0，最多6位小数", "0.015000" },
        new[] { "权值", "是", "0到1，最多4位小数", "1" },
        new[] { "所属梁段号", "是", "1-50个字符", "A" },
        new[] { "测位名称", "是", "1-100个字符", "测位1" }
    });

    public static byte[] CreateMeasuredTemplate() => CreateTemplate(MeasuredHeaders, new[]
    {
        new[] { "测点编号", "是", "必须匹配当前桥梁理论测点，忽略大小写", "GS-Z-1-4" },
        new[] { "实测坐标X/Y/Z(m)", "是", "允许正负数和0，最多6位小数", "713033.449300" },
        new[] { "说明", "-", "只录入未发生变形的测点", "" }
    });

    public static byte[] CreateErrorFile(IEnumerable<SteelBeamImportError> errors)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("错误明细");
        var headers = new[] { "Excel行号", "测点编号", "字段名称", "原始值", "失败原因" };
        for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
        var row = 2;
        foreach (var error in errors)
        {
            sheet.Cell(row, 1).Value = error.ExcelRow;
            sheet.Cell(row, 2).Value = error.PointCode;
            sheet.Cell(row, 3).Value = error.FieldName;
            sheet.Cell(row, 4).Value = error.OriginalValue;
            sheet.Cell(row, 5).Value = error.Reason;
            row++;
        }
        StyleSheet(sheet, headers.Length);
        return Save(workbook);
    }

    public static string SanitizePathPart(string value)
    {
        var sanitized = Regex.Replace(value ?? string.Empty, @"[\x00-\x1F<>:""/\\|?*]", "_").Trim().TrimEnd('.');
        return string.IsNullOrWhiteSpace(sanitized) ? "未命名" : sanitized;
    }

    public static bool HasValidPointCode(string value) =>
        !string.IsNullOrEmpty(value) && value.Length <= 50 &&
        value[0] is not ('=' or '+' or '-' or '@') && Regex.IsMatch(value, "^[\\x21-\\x7E]+$");

    public static bool HasScale(decimal value, int maximumScale)
    {
        var bits = decimal.GetBits(value);
        var scale = (bits[3] >> 16) & 0x7F;
        return scale <= maximumScale;
    }

    private static Dictionary<string, int> ValidateHeaders(IXLWorksheet sheet, string[] required, List<SteelBeamImportError> errors)
    {
        var allColumns = new Dictionary<string, int>(StringComparer.Ordinal);
        var lastColumn = sheet.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;
        for (var column = 1; column <= lastColumn; column++)
        {
            var name = sheet.Cell(1, column).GetString();
            if (!string.IsNullOrEmpty(name) && !allColumns.ContainsKey(name)) allColumns[name] = column;
        }
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var name in required)
        {
            if (!allColumns.TryGetValue(name, out var column)) errors.Add(new(1, string.Empty, name, string.Empty, $"缺少必需列：{name}"));
            else map[name] = column;
        }
        return map;
    }

    private static void ValidateNoFormula(IXLWorksheet sheet, int row, Dictionary<string, int> columns, List<SteelBeamImportError> errors, string point)
    {
        foreach (var pair in columns)
        {
            var cell = sheet.Cell(row, pair.Value);
            if (cell.HasFormula) errors.Add(new(row, point, pair.Key, cell.FormulaA1, "导入字段不允许使用公式"));
        }
    }

    private static void ValidatePointCode(int row, string point, List<SteelBeamImportError> errors)
    {
        if (string.IsNullOrEmpty(point)) errors.Add(new(row, point, "测点编号", point, "测点编号不能为空"));
        else if (!HasValidPointCode(point)) errors.Add(new(row, point, "测点编号", point, "仅允许1-50位可见半角字符，禁止空格、中文、换行及以=、+、-、@开头"));
    }

    private static decimal? ReadDecimal(IXLCell cell, int row, string point, string field, int scale, decimal? min, decimal? max, List<SteelBeamImportError> errors)
    {
        var raw = cell.GetFormattedString();
        if (cell.IsEmpty())
        {
            errors.Add(new(row, point, field, raw, $"{field}不能为空"));
            return null;
        }
        decimal value;
        if (cell.DataType == XLDataType.Number) value = Convert.ToDecimal(cell.GetDouble(), CultureInfo.InvariantCulture);
        else if (!decimal.TryParse(cell.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out value))
        {
            errors.Add(new(row, point, field, raw, $"{field}必须是有效数字"));
            return null;
        }
        if (!HasScale(value, scale)) errors.Add(new(row, point, field, raw, $"{field}最多保留{scale}位小数"));
        if (min.HasValue && value < min.Value || max.HasValue && value > max.Value)
            errors.Add(new(row, point, field, raw, $"{field}必须在{min}到{max}之间"));
        return value;
    }

    private static void ValidateRequiredText(int row, string point, string field, string value, int maxLength, List<SteelBeamImportError> errors)
    {
        if (string.IsNullOrWhiteSpace(value)) errors.Add(new(row, point, field, value, $"{field}不能为空"));
        else if (value.Length > maxLength) errors.Add(new(row, point, field, value, $"{field}不能超过{maxLength}个字符"));
    }

    private static string ReadText(IXLCell cell) => cell.HasFormula ? string.Empty : cell.GetString();
    private static bool IsEmptyRow(IXLWorksheet sheet, int row, IEnumerable<int> columns) => columns.All(c => sheet.Cell(row, c).IsEmpty());
    private static int CountNonEmptyRows(IXLWorksheet sheet, int first, int last, IEnumerable<int> columns)
    {
        var count = 0;
        for (var row = first; row <= last; row++) if (!IsEmptyRow(sheet, row, columns)) count++;
        return count;
    }

    private static byte[] CreateTemplate(string[] headers, string[][] instructions)
    {
        using var workbook = new XLWorkbook();
        var data = workbook.AddWorksheet("数据");
        for (var i = 0; i < headers.Length; i++) data.Cell(1, i + 1).Value = headers[i];
        StyleSheet(data, headers.Length);
        data.SheetView.FreezeRows(1);
        var help = workbook.AddWorksheet("填写说明");
        var helpHeaders = new[] { "字段", "必填", "规则", "示例" };
        for (var i = 0; i < helpHeaders.Length; i++) help.Cell(1, i + 1).Value = helpHeaders[i];
        for (var r = 0; r < instructions.Length; r++)
            for (var c = 0; c < instructions[r].Length; c++) help.Cell(r + 2, c + 1).Value = instructions[r][c];
        StyleSheet(help, helpHeaders.Length);
        return Save(workbook);
    }

    private static void StyleSheet(IXLWorksheet sheet, int columns)
    {
        var header = sheet.Range(1, 1, 1, columns);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF2FF");
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Columns(1, columns).AdjustToContents(12, 50);
        sheet.RangeUsed()?.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        sheet.RangeUsed()?.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
    }

    private static byte[] Save(XLWorkbook workbook)
    {
        using var output = new MemoryStream();
        workbook.SaveAs(output);
        return output.ToArray();
    }
}
