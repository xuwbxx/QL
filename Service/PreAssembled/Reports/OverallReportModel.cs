using System;
using System.Collections.Generic;

namespace Service.PreAssembled.Reports
{
    /// <summary>
    /// 整体报告的三种形态，对应【报告】页签「整体报告」区的三个单选项。
    /// </summary>
    public enum OverallReportKind
    {
        /// <summary>项目信息：浇筑组基础参数与设计坐标/梁段填报的现状汇总。</summary>
        Project,

        /// <summary>浇筑和理论坐标：逐梁段给出端面「理论值 / 浇筑值」对照。</summary>
        CastAndTheory,

        /// <summary>理论坐标：按 joint 给出设计坐标（理论值）；范围受「包含所有」控制。</summary>
        Theory
    }

    /// <summary>
    /// 整体报告的中间模型。与 <see cref="SegmentReportModel"/> 同构（头部三行 + 正文行流），
    /// 只是正文语义不同，因此复用同一套 <see cref="ReportLine"/> 与渲染器。
    /// </summary>
    /// <remarks>
    /// 同样纯数据、不参与 JSON 序列化——前端只取最终的 PDF 字节流。
    /// <para>
    /// 报告正文固定为英文（与单梁段报告一致）：面向监理归档，与原件口径对齐，
    /// 但同样<b>不输出原软件厂商横幅</b>。
    /// </para>
    /// </remarks>
    public sealed class OverallReportModel
    {
        /// <summary>报告类型。</summary>
        public OverallReportKind Kind { get; set; }

        /// <summary>项目名称，对应原报告的 Project Title。</summary>
        public string ProjectTitle { get; set; } = string.Empty;

        /// <summary>浇筑组名称，对应原报告的 Casting Set。</summary>
        public string CastingSet { get; set; } = string.Empty;

        /// <summary>报告生成日期，对应原报告的 Date。</summary>
        public DateTime ReportDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否输出全部 joint。仅对 <see cref="OverallReportKind.Theory"/> 生效：
        /// 未勾选时只列到「已浇筑梁段的最大 joint」，勾选后列出全部 joint。
        /// </summary>
        public bool IncludeAll { get; set; }

        /// <summary>正文行。</summary>
        public List<ReportLine> Lines { get; set; } = new List<ReportLine>();

        /// <summary>PDF 文档信息里的标题（与单梁段报告区分，便于在阅读器里辨认）。</summary>
        public string DocumentTitle => Kind switch
        {
            OverallReportKind.Project => "Overall Report - Project Information",
            OverallReportKind.CastAndTheory => "Overall Report - Cast and Theoretical Coordinates",
            _ => IncludeAll ? "Overall Report - Theoretical Coordinates (All Joints)"
                            : "Overall Report - Theoretical Coordinates"
        };

        /// <summary>建议的文件名后缀（不含扩展名）。</summary>
        public string SuggestedFileName => Kind switch
        {
            OverallReportKind.Project => "project-information",
            OverallReportKind.CastAndTheory => "cast-and-theory",
            _ => IncludeAll ? "theory-all" : "theory"
        };
    }
}
