using System;
using System.Collections.Generic;

namespace Service.PreAssembled.Reports
{
    /// <summary>
    /// 预拱度「打印/预览」PDF 的中间模型：
    /// 页首 Project Title / Casting Set / Date，其下 section 标题 Camber: 与界面一致的预拱度表格
    /// （No. / Name / ZE / ZC / ZW）。
    /// </summary>
    public sealed class CamberReportModel
    {
        /// <summary>项目名称。</summary>
        public string ProjectTitle { get; set; } = string.Empty;

        /// <summary>浇筑组名称。</summary>
        public string CastingSet { get; set; } = string.Empty;

        /// <summary>报告生成时间（已按客户端时区偏移还原）。</summary>
        public DateTime ReportDate { get; set; } = DateTime.Now;

        /// <summary>客户端时区偏移（分钟，东八区=480）。</summary>
        public int? TimezoneOffsetMinutes { get; set; }

        /// <summary>表头（No. / Name / ZE / ZC / ZW）。</summary>
        public List<string> Headers { get; set; } = new();

        /// <summary>数据行（已格式化为字符串，列序与 Headers 一致）。</summary>
        public List<List<string>> Rows { get; set; } = new();
    }
}
