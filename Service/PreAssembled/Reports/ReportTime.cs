using System;

namespace Service.PreAssembled.Reports
{
    /// <summary>
    /// PDF 报告时间格式化：统一在日期/时间后附加时区标识（如 UTC+08:00）。
    /// </summary>
    public static class ReportTime
    {
        /// <summary>把分钟偏移格式化为 UTC±HH:mm；无偏移时返回空串。</summary>
        public static string FormatOffset(int? offsetMinutes)
        {
            if (!offsetMinutes.HasValue) return string.Empty;
            var minutes = offsetMinutes.Value;
            var sign = minutes >= 0 ? "+" : "-";
            var abs = Math.Abs(minutes);
            return $"UTC{sign}{abs / 60:00}:{abs % 60:00}";
        }

        /// <summary>在已格式化的时间文本后附加时区；无偏移时原样返回。</summary>
        public static string WithOffset(string text, int? offsetMinutes)
        {
            var label = FormatOffset(offsetMinutes);
            return string.IsNullOrEmpty(label) ? text : $"{text} ({label})";
        }
    }
}
