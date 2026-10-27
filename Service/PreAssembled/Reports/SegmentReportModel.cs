using System;
using System.Collections.Generic;

namespace Service.PreAssembled.Reports
{
    /// <summary>
    /// 单梁段报告的两种形态，对应【报告】页签「梁段设置 / 已浇筑梁段」两个单选项。
    /// </summary>
    public enum SegmentReportKind
    {
        /// <summary>梁段设置（浇筑前）：只有 MATCH-CAST SETUP 一章。</summary>
        Setup,

        /// <summary>已浇筑梁段（浇筑后）：MATCH-CAST SETUP + SURVEY + AS-CAST COORDINATES 三章。</summary>
        AsCast
    }

    /// <summary>
    /// 报告里的一行。原报告是纯文本流 + 等宽字体的版式（无表格边框），
    /// 所以这里也用「前导空格 + 文本」表达缩进与列的相对位置，由 Builder 原样铺到 PDF 上。
    /// </summary>
    public sealed class ReportLine
    {
        /// <summary>行首空格数。等宽字体下用它做缩进与列定位。</summary>
        public int LeadingSpaces { get; init; }

        /// <summary>行文本（不含前导空格）。空串表示空行。</summary>
        public string Text { get; init; } = string.Empty;

        /// <summary>是否加粗（章标题、块标题用）。</summary>
        public bool Bold { get; init; }

        /// <summary>是否属于头部信息区（Project Title / Casting Set / Date 三行）。</summary>
        public bool IsHeader { get; init; }

        /// <summary>构造一个空行。</summary>
        public static ReportLine Blank() => new ReportLine();

        /// <summary>
        /// 构造「标签 : 值」型数据行——标签左对齐到固定列，值右对齐到固定列，
        /// 这样不同长度的标签也能得到整齐的数值列（等宽字体保证左右都真正对齐）。
        /// </summary>
        public static ReportLine Pair(int leadingSpaces, string label, string value, bool bold = false)
        {
            var pad = LabelColumnWidth - leadingSpaces - label.Length;
            if (pad < 1)
            {
                pad = 1;
            }

            return new ReportLine
            {
                LeadingSpaces = leadingSpaces,
                Text = label + new string(' ', pad) + (value ?? string.Empty).PadLeft(ValueColumnWidth),
                Bold = bold
            };
        }

        /// <summary>构造标题行（章 / 块标题）。</summary>
        public static ReportLine Title(int leadingSpaces, string text, bool bold = true)
            => new ReportLine { LeadingSpaces = leadingSpaces, Text = text, Bold = bold };

        /// <summary>标签列总宽（字符数，含左侧缩进）。</summary>
        public const int LabelColumnWidth = 46;

        /// <summary>数值列宽（字符数，右对齐）。</summary>
        public const int ValueColumnWidth = 12;
    }

    /// <summary>
    /// 单梁段报告的完整中间模型。纯数据，不参与 JSON 序列化——
    /// 前端只拿最终的 PDF 字节流，因此放在 Service 内部而不是 Model 项目的 DTO 里。
    /// </summary>
    /// <remarks>
    /// 与原报告（MC3D Unlimited 生成）的对应关系：
    /// <list type="bullet">
    /// <item>头部 = Project Title / Casting Set / Date + Wet Cast 与 Match Cast 两行；</item>
    /// <item>正文 = <see cref="Lines"/>，按原件的章（MATCH-CAST SETUP / SURVEY / AS-CAST COORDINATES）顺序铺开；</item>
    /// <item>页脚 = Page x of y + 生成时间戳，由 Builder 直接生成，不在此模型内。</item>
    /// </list>
    /// </remarks>
    public sealed class SegmentReportModel
    {
        /// <summary>报告类型。</summary>
        public SegmentReportKind Kind { get; set; }

        /// <summary>
        /// 是否起始段（m=0）。起始段没有匹配段，报告里不显示“上一梁段浇筑时间”（避免 1970 哨兵）。
        /// </summary>
        public bool IsFirstSegment { get; set; }

        /// <summary>项目名称，对应原报告的 Project Title。</summary>
        public string ProjectTitle { get; set; } = string.Empty;

        /// <summary>浇筑组名称，对应原报告的 Casting Set。</summary>
        public string CastingSet { get; set; } = string.Empty;

        /// <summary>报告生成日期，对应原报告的 Date。</summary>
        public DateTime ReportDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 客户端时区偏移（分钟，东八区=480）。
        /// 报告里的日期/时间按此时区还原，并在格式化时附加 UTC±HH:mm 标识。
        /// </summary>
        public int? TimezoneOffsetMinutes { get; set; }

        /// <summary>本段（浇筑段）名称。</summary>
        public string WetCastSegmentName { get; set; } = string.Empty;

        /// <summary>
        /// 本段浇筑日期。未浇筑时为 <see cref="NotCastSentinel"/>——
        /// 这是原系统的既定行为（原件 setting.pdf 对未浇筑段就显示 Thursday, January 01, 1970），
        /// 保持与原件一致，避免监理对格式产生疑问。
        /// </summary>
        public DateTime? WetCastCastOn { get; set; }

        /// <summary>匹配段名称。起始段无匹配段，为空串（报告里渲染为 —）。</summary>
        public string MatchCastSegmentName { get; set; } = string.Empty;

        /// <summary>匹配段浇筑日期。</summary>
        public DateTime? MatchCastCastOn { get; set; }

        /// <summary>
        /// 未浇筑梁段的日期哨兵值（Unix 纪元），与原系统输出一致。
        /// </summary>
        public static readonly DateTime NotCastSentinel = new DateTime(1970, 1, 1);

        /// <summary>正文行，按原报告的章 / 块顺序排列。</summary>
        public List<ReportLine> Lines { get; set; } = new List<ReportLine>();

        /// <summary>建议的 PDF 文件名（不含扩展名）。</summary>
        public string SuggestedFileName =>
            $"{WetCastSegmentName}-{(Kind == SegmentReportKind.Setup ? "setting" : "baogao")}".Trim('-');
    }
}
