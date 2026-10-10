using ClosedXML.Excel;
using Service.PreAssembled.BridgeSegmentAlignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tool;

namespace Service.PreAssembled.Reports
{
    // 说明：Excel 的打开/读取/写值/样式/模板骨架等与业务无关的能力已下沉到 Tool.ExcelUtils，
    // 本文件只保留钢梁（SteelBeam）业务的数据契约、表头定义与业务校验规则。

    public interface ISteelBeamPointRow
    {
        /// <summary>
        /// ExcelRow索引（兼容record写法，所以这里用了 {get;init;}）
        /// </summary>
        int ExcelRow { get; init; }
        string PointCode { get; init; }
        string PositionName { get; init; }
        string RowType { get; init; }
        decimal X { get; }
        decimal Y { get; }
        decimal Z { get; }
        //decimal PreCamber { get; init; }
        //string SegmentNo { get; init; }
        //string PositionName { get; init; }
    }

    public sealed record SteelBeamTheoreticalImportRow(
        int ExcelRow, string PointCode, decimal DesignX, decimal DesignY, decimal DesignZ,
        decimal PreCamber, decimal Weight, string SegmentNo, string PositionName, string RowType = "theoretical") : ISteelBeamPointRow
    {
        private decimal? _x { set; get; }
        private decimal? _y { set; get; }
        private decimal? _z { set; get; }
        public decimal X
        {
            get
            {
                if (_x == null) _x = DesignX;
                return _x ?? DesignX;
            }
            set { _x = value; }
        }
        public decimal Y
        {
            get
            {
                // 原实现此处误写为 _x = DesignY，会污染 X 的缓存值，已修正为 _y
                if (_y == null) _y = DesignY;
                return _y ?? DesignY;
            }
            set { _y = value; }
        }
        public decimal Z
        {
            get
            {
                if (_z == null) _z = DesignZ;
                return _z ?? DesignZ;
            }
            set { _z = value; }
        }
    }

    // [Shen, Lin] 注意：实测数据Excel文档里不带测位数据
    public sealed record SteelBeamMeasuredImportRow(
        int ExcelRow, string PointCode, decimal MeasuredX, decimal MeasuredY, decimal MeasuredZ,
        string PositionName, string RowType = "measured") : ISteelBeamPointRow
    {
        public decimal X => MeasuredX;
        public decimal Y => MeasuredY;
        public decimal Z => MeasuredZ;
    }

    public sealed record SteelBeamImportError(
        int? ExcelRow, string PointCode, string FieldName, string OriginalValue, string Reason);

    public sealed record SteelBeamTheoreticalPoint(
        string PointCode, string SegmentNo);

    public sealed class SteelBeamParseResult<T>
        where T : ISteelBeamPointRow
    {
        public List<T> Rows { get; } = new();
        public List<(int ExcelRow, string PointCode)> SourcePoints { get; } = new();
        public List<SteelBeamImportError> Errors { get; } = new();
        public bool Success => Errors.Count == 0 && Rows.Count > 0;
    }

    public static class SteelBeamExcelHelper
    {
        public static readonly string[] TheoreticalHeaders =
        [
            "测点编号", "设计坐标X(m)", "设计坐标Y(m)", "设计坐标Z(m)",
            "预拱度(m)", "权值", "所属梁段号", "测位名称"
        ];

        public static readonly string[] MeasuredHeaders =
        [
            "测点编号", "实测坐标X(m)", "实测坐标Y(m)", "实测坐标Z(m)"
        ];

        public static SteelBeamParseResult<SteelBeamTheoreticalImportRow> ParseTheoretical(Stream stream, string? lang)
        {
            var result = new SteelBeamParseResult<SteelBeamTheoreticalImportRow>();
            ExcelUtils.ExcelErrorSink addError = (row, rowKey, field, originalValue, reason)
                => result.Errors.Add(new(row, rowKey, field, originalValue, reason));

            using var workbook = ExcelUtils.OpenWorkbook(stream);
            var sheet = ExcelUtils.FirstWorksheet(workbook);
            var columns = ExcelUtils.ReadHeaderMap(sheet, TheoreticalHeaders, lang, addError);
            if (result.Errors.Count > 0) return result;

            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
            var dataCount = ExcelUtils.CountNonEmptyRows(sheet, 2, lastRow, columns.Values);
            if (dataCount == 0)
            {
                result.Errors.Add(new(0, string.Empty, BizMessages.FieldFile(lang), string.Empty, BizMessages.NoDataInFirstSheet(lang)));
                return result;
            }
            if (dataCount > ExcelUtils.MaxDataRows)
            {
                result.Errors.Add(new(0, string.Empty, BizMessages.FieldFile(lang), dataCount.ToString(), BizMessages.TooManyRows(lang, ExcelUtils.MaxDataRows)));
                return result;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
            {
                if (ExcelUtils.IsEmptyRow(sheet, rowNumber, columns.Values)) continue;
                var point = ExcelUtils.ReadText(sheet.Cell(rowNumber, columns["测点编号"]));
                result.SourcePoints.Add((rowNumber, point));
                ExcelUtils.ValidateNoFormula(sheet, rowNumber, columns, point, lang, addError);
                ValidatePointCode(rowNumber, point, result.Errors, lang);
                if (!string.IsNullOrEmpty(point) && !seen.Add(point))
                    result.Errors.Add(new(rowNumber, point, BizMessages.FieldPointCode(lang), point, BizMessages.DuplicatePointCode(lang, "测点编号")));

                var x = ExcelUtils.ReadDecimal(sheet.Cell(rowNumber, columns["设计坐标X(m)"]), rowNumber, point, "设计坐标X(m)", 4, null, null, lang, addError);
                var y = ExcelUtils.ReadDecimal(sheet.Cell(rowNumber, columns["设计坐标Y(m)"]), rowNumber, point, "设计坐标Y(m)", 4, null, null, lang, addError);
                var z = ExcelUtils.ReadDecimal(sheet.Cell(rowNumber, columns["设计坐标Z(m)"]), rowNumber, point, "设计坐标Z(m)", 4, null, null, lang, addError);
                var camber = ExcelUtils.ReadDecimal(sheet.Cell(rowNumber, columns["预拱度(m)"]), rowNumber, point, "预拱度(m)", 4, null, null, lang, addError);
                var weight = ExcelUtils.ReadDecimal(sheet.Cell(rowNumber, columns["权值"]), rowNumber, point, "权值", 4, 0, 1, lang, addError);
                var segment = ExcelUtils.ReadText(sheet.Cell(rowNumber, columns["所属梁段号"])).Trim();
                var position = ExcelUtils.ReadText(sheet.Cell(rowNumber, columns["测位名称"])).Trim();
                ExcelUtils.ValidateRequiredText(rowNumber, point, "所属梁段号", segment, 50, lang, addError);
                ExcelUtils.ValidateRequiredText(rowNumber, point, "测位名称", position, 100, lang, addError);

                if (!result.Errors.Any(e => e.ExcelRow == rowNumber))
                    result.Rows.Add(new(rowNumber, point, x!.Value, y!.Value, z!.Value, camber!.Value, weight!.Value, segment, position));
            }
            return result;
        }

        public static SteelBeamParseResult<SteelBeamMeasuredImportRow> ParseMeasured(Stream stream, string? lang)
        {
            var result = new SteelBeamParseResult<SteelBeamMeasuredImportRow>();
            ExcelUtils.ExcelErrorSink addError = (row, rowKey, field, originalValue, reason)
                => result.Errors.Add(new(row, rowKey, field, originalValue, reason));

            using var workbook = ExcelUtils.OpenWorkbook(stream);
            var sheet = ExcelUtils.FirstWorksheet(workbook);
            var columns = ExcelUtils.ReadHeaderMap(sheet, MeasuredHeaders, lang, addError);
            if (result.Errors.Count > 0) return result;

            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
            var dataCount = ExcelUtils.CountNonEmptyRows(sheet, 2, lastRow, columns.Values);
            if (dataCount == 0)
            {
                result.Errors.Add(new(0, string.Empty, BizMessages.FieldFile(lang), string.Empty, BizMessages.NoDataInFirstSheet(lang)));
                return result;
            }
            if (dataCount > ExcelUtils.MaxDataRows)
            {
                result.Errors.Add(new(0, string.Empty, BizMessages.FieldFile(lang), dataCount.ToString(), BizMessages.TooManyRows(lang, ExcelUtils.MaxDataRows)));
                return result;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
            {
                if (ExcelUtils.IsEmptyRow(sheet, rowNumber, columns.Values)) continue;
                var point = ExcelUtils.ReadText(sheet.Cell(rowNumber, columns["测点编号"]));
                result.SourcePoints.Add((rowNumber, point));
                ExcelUtils.ValidateNoFormula(sheet, rowNumber, columns, point, lang, addError);
                ValidatePointCode(rowNumber, point, result.Errors, lang);
                if (!string.IsNullOrEmpty(point) && !seen.Add(point))
                    result.Errors.Add(new(rowNumber, point, BizMessages.FieldPointCode(lang), point, BizMessages.DuplicatePointCode(lang, "测点编号")));
                var x = ExcelUtils.ReadDecimal(sheet.Cell(rowNumber, columns["实测坐标X(m)"]), rowNumber, point, "实测坐标X(m)", 4, null, null, lang, addError);
                var y = ExcelUtils.ReadDecimal(sheet.Cell(rowNumber, columns["实测坐标Y(m)"]), rowNumber, point, "实测坐标Y(m)", 4, null, null, lang, addError);
                var z = ExcelUtils.ReadDecimal(sheet.Cell(rowNumber, columns["实测坐标Z(m)"]), rowNumber, point, "实测坐标Z(m)", 4, null, null, lang, addError);
                if (!result.Errors.Any(e => e.ExcelRow == rowNumber))
                    result.Rows.Add(new(rowNumber, point, x!.Value, y!.Value, z!.Value, ""));
            }
            return result;
        }

        public static byte[] CreateTheoreticalTemplate() => ExcelUtils.CreateTemplate(TheoreticalHeaders, new[]
        {
            new[] { "测点编号", "是", "1-50位可见半角字符；禁止空格、中文、换行及以=、+、-、@开头", "GS-Z-1-4" },
            new[] { "设计坐标X/Y/Z(m)", "是", "允许正负数和0，最多4位小数", "713033.4493" },
            new[] { "预拱度(m)", "是", "允许正负数和0，最多4位小数", "0.0150" },
            new[] { "权值", "是", "0到1，最多4位小数", "1" },
            new[] { "所属梁段号", "是", "1-50个字符", "A" },
            new[] { "测位名称", "是", "1-100个字符", "测位1" }
        });

        public static byte[] CreateMeasuredTemplate() => ExcelUtils.CreateTemplate(MeasuredHeaders, new[]
        {
            new[] { "测点编号", "是", "必须匹配当前桥梁理论测点，忽略大小写", "GS-Z-1-4" },
            new[] { "实测坐标X/Y/Z(m)", "是", "允许正负数和0，最多4位小数", "713033.4493" },
            new[] { "说明", "-", "只录入未发生变形的测点", "" }
        });

        public static byte[] CreateErrorFile(IEnumerable<SteelBeamImportError> errors, string? lang)
        {
            var headers = BizMessages.SteelErrorFileHeaders(lang);
            var rows = errors.Select(error => new[]
            {
                error.ExcelRow?.ToString() ?? string.Empty,
                error.PointCode ?? string.Empty,
                error.FieldName ?? string.Empty,
                error.OriginalValue ?? string.Empty,
                error.Reason ?? string.Empty
            });
            return ExcelUtils.CreateTableWorkbook(BizMessages.ErrorSheetName(lang), headers, rows);
        }

        public static bool HasValidPointCode(string value) =>
            !string.IsNullOrEmpty(value) && value.Length <= 50 &&
            value[0] is not ('=' or '+' or '-' or '@') && Regex.IsMatch(value, "^[\\x21-\\x7E]+$");

        public static List<SteelBeamImportError> ValidateMeasuredSegmentCompleteness(
            IEnumerable<SteelBeamTheoreticalPoint> theoreticalPoints,
            IEnumerable<string> uploadedPointCodes,
            string? lang)
        {
            var theory = theoreticalPoints.ToList();
            var uploaded = new HashSet<string>(
                uploadedPointCodes.Where(a => !string.IsNullOrEmpty(a)),
                StringComparer.OrdinalIgnoreCase);
            var uploadedTheory = new HashSet<string>(
                theory.Where(a => uploaded.Contains(a.PointCode)).Select(a => a.PointCode),
                StringComparer.OrdinalIgnoreCase);
            var touchedSegments = new HashSet<string>(
                theory.Where(a => uploadedTheory.Contains(a.PointCode)).Select(a => a.SegmentNo),
                StringComparer.OrdinalIgnoreCase);

            return theory
                .Where(a => touchedSegments.Contains(a.SegmentNo) && !uploadedTheory.Contains(a.PointCode))
                .OrderBy(a => a.SegmentNo, StringComparer.OrdinalIgnoreCase)
                .ThenBy(a => a.PointCode, StringComparer.OrdinalIgnoreCase)
                .Select(a => new SteelBeamImportError(
                    0,
                    a.PointCode,
                    BizMessages.FieldSegmentNo(lang),
                    a.SegmentNo,
                    BizMessages.SegmentIncomplete(lang)))
                .ToList();
        }

        private static void ValidatePointCode(int row, string point, List<SteelBeamImportError> errors, string? lang)
        {
            if (string.IsNullOrEmpty(point)) errors.Add(new(row, point, BizMessages.FieldPointCode(lang), point, BizMessages.PointCodeRequired(lang)));
            else if (!HasValidPointCode(point)) errors.Add(new(row, point, BizMessages.FieldPointCode(lang), point, BizMessages.PointCodeInvalid(lang)));
        }
    }
}
