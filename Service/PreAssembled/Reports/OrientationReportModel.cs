using System;
using System.Collections.Generic;

namespace Service.PreAssembled.Reports
{
    /// <summary>
    /// 设计坐标「核对与预览」PDF 的中间模型：
    /// 上半表 General Coordinates（已计算坐标，含预拱度），下半表 PrintReport 几何报告。
    /// </summary>
    public sealed class OrientationReportModel
    {
        /// <summary>项目名称。</summary>
        public string ProjectTitle { get; set; } = string.Empty;

        /// <summary>浇筑组名称。</summary>
        public string CastingSet { get; set; } = string.Empty;

        /// <summary>报告生成时间（已按客户端时区偏移还原）。</summary>
        public DateTime ReportDate { get; set; } = DateTime.Now;

        /// <summary>客户端时区偏移（分钟，东八区=480）。</summary>
        public int? TimezoneOffsetMinutes { get; set; }

        /// <summary>上半表表头。</summary>
        public List<string> JointHeaders { get; set; } = new();

        /// <summary>上半表数据行（已格式化为字符串）。</summary>
        public List<List<string>> JointRows { get; set; } = new();

        /// <summary>下半表表头（ReportRow.GetHeader()）。</summary>
        public List<string> ReportHeaders { get; set; } = new();

        /// <summary>下半表数据行（已格式化为字符串）。</summary>
        public List<List<string>> ReportRows { get; set; } = new();
    }
}
