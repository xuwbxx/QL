using Service.PreAssembled.BridgeSegmentAlignment;

namespace Service.PreAssembled;

/// <summary>
/// 混凝土梁设计坐标、预拱度与浇筑态数据之间的流程规则。
/// </summary>
/// <remarks>
/// 统一收纳"什么情况下必须作废下游数据"的判断，避免规则散落在各个 Service 方法里。
/// 下游数据指：biz_concrete_beam_segment（逐段填报的实测数据）、
/// biz_concrete_beam_ascast_coordinate（浇筑端面坐标）、biz_concrete_beam_ascast_measure（浇筑测点坐标）。
/// 三者是同一条计算链上的产物，作废时必须在同一事务内整体作废，不能只清一部分。
/// </remarks>
public static class ConcreteBeamWorkflowRules
{
    /// <summary>
    /// 判断确认设计坐标后是否需要清除不再匹配的独立预拱度数据。
    /// 已含预拱度的设计坐标只会停用独立预拱度，不删除已有数据。
    /// </summary>
    public static bool ShouldClearCamberAfterDesignConfirmation(
        bool elevationIncludesCamber,
        bool canReuseCamber)
    {
        return !elevationIncludesCamber && !canReuseCamber;
    }

    /// <summary>
    /// 判断浇筑组基础参数变更是否会让已生成的浇筑态数据失效。
    /// A/B/C/H 四个参数全部进入线形算法（Calc / ZeroErr / OneErr），
    /// 任一变化都会让既往算出的浇筑坐标失去基准，必须作废下游。
    /// 首次保存配置（previous 为 null）时返回 false。
    /// </summary>
    public static bool ShouldInvalidateAsCastAfterConfigChange(
        decimal? previousA, decimal? previousB, decimal? previousC, decimal? previousH,
        decimal nextA, decimal nextB, decimal nextC, decimal nextH)
    {
        if (!previousA.HasValue || !previousB.HasValue || !previousC.HasValue || !previousH.HasValue)
            return false;
        return previousA.Value != nextA
            || previousB.Value != nextB
            || previousC.Value != nextC
            || previousH.Value != nextH;
    }

    /// <summary>设计坐标覆盖导入的确认文案。</summary>
    public static string DesignOverwriteMessage(bool hasAsCastData, string? lang) =>
        BizMessages.DesignOverwriteConfirm(lang, hasAsCastData);

    /// <summary>预拱度覆盖导入的确认文案。</summary>
    public static string CamberOverwriteMessage(bool hasAsCastData, string? lang) =>
        BizMessages.CamberOverwriteConfirm(lang, hasAsCastData);

    /// <summary>下游数据被作废后的提示文案。</summary>
    public static string AsCastInvalidatedMessage(int invalidatedCount, string? lang) =>
        BizMessages.AsCastInvalidated(lang, invalidatedCount);

    #region 逐段填报（Survey）的填报顺序规则

    /// <summary>填报决策：允许 / 跳段 / 已锁定。</summary>
    public enum SegmentFillDecision
    {
        /// <summary>允许填报（新增或更新本段）。</summary>
        Allowed,
        /// <summary>跳段：该段之前尚有未填报的梁段。</summary>
        SkipAhead,
        /// <summary>已锁定：该段之后已有梁段填报，不允许返工。</summary>
        Locked
    }

    /// <summary>
    /// 逐段填报的顺序判定（业务规则：不跳段、不返工、下一梁段填报前可改本段）。
    /// </summary>
    /// <param name="filledMaxIndex">已填报梁段的最大几何序号（0 起）；无任何填报时为 -1。</param>
    /// <param name="segmentIndex">本次要填报的梁段几何序号（等于算法入参 M）。</param>
    /// <param name="alreadyFilled">该梁段是否已存在填报记录（true=更新，false=新增）。</param>
    /// <remarks>
    /// 新增只允许发生在紧邻的一节（m == filledMaxIndex + 1）；
    /// 更新只允许发生在最后一节（m == filledMaxIndex），因为"下一梁段填报前可改本段"，
    /// 一旦后面又有新段，前面的段就锁定，避免下游已算坐标与上游实测脱节。
    /// </remarks>
    public static SegmentFillDecision DecideSegmentFill(int filledMaxIndex, int segmentIndex, bool alreadyFilled)
    {
        if (alreadyFilled)
            return segmentIndex == filledMaxIndex ? SegmentFillDecision.Allowed : SegmentFillDecision.Locked;
        return segmentIndex == filledMaxIndex + 1
            ? SegmentFillDecision.Allowed
            : SegmentFillDecision.SkipAhead;
    }

    /// <summary>下一个待填报梁段的几何序号。</summary>
    public static int NextSegmentIndex(int filledMaxIndex) => filledMaxIndex + 1;

    /// <summary>梁段是否仍可修改（仅最后一个已填报梁段可改）。</summary>
    public static bool CanEditSegment(int filledMaxIndex, int segmentIndex) => segmentIndex == filledMaxIndex;

    /// <summary>跳段填报的拒绝文案（展示序号用 1 基）。</summary>
    public static string SkipSegmentMessage(int filledMaxIndex, int segmentIndex, string? lang) =>
        BizMessages.SkipSegment(lang, filledMaxIndex, segmentIndex);

    /// <summary>
    /// 一次给出"能否填报/修改"与"不能时的原因"，供列表与详情接口共用，避免两侧判断漂移。
    /// </summary>
    /// <param name="filledMaxIndex">已填报梁段的最大几何序号（0 起）；无任何填报时为 -1。</param>
    /// <param name="segmentIndex">目标梁段的几何序号。</param>
    /// <param name="alreadyFilled">该梁段是否已存在填报记录。</param>
    /// <returns>(CanEdit, LockedReason)；CanEdit 为 true 时 LockedReason 为空字符串。</returns>
    public static (bool CanEdit, string LockedReason) DescribeSegmentFill(
        int filledMaxIndex, int segmentIndex, bool alreadyFilled, string? lang)
    {
        var decision = DecideSegmentFill(filledMaxIndex, segmentIndex, alreadyFilled);
        return decision switch
        {
            SegmentFillDecision.Allowed => (true, string.Empty),
            SegmentFillDecision.SkipAhead => (false, SkipSegmentMessage(filledMaxIndex, segmentIndex, lang)),
            _ => (false, LockedSegmentMessage(filledMaxIndex, segmentIndex, lang))
        };
    }

    /// <summary>已锁定梁段的拒绝文案（展示序号用 1 基）。</summary>
    public static string LockedSegmentMessage(int filledMaxIndex, int segmentIndex, string? lang) =>
        BizMessages.LockedSegment(lang, filledMaxIndex, segmentIndex);

    #endregion

    #region 上游输入锁定（已开始实测填报后）

    /// <summary>
    /// 判断上游输入是否已被锁定：浇筑组下一旦存在梁段实测填报，就不再允许改动上游输入。
    /// 判据由调用方给出（<c>biz_concrete_beam_segment</c> 是否存在有效行），本类不访问数据库。
    /// </summary>
    public static bool IsUpstreamLocked(bool hasSurveyData) => hasSurveyData;

    /// <summary>项目信息（A/B/C/参考高程）被锁定时的拒绝文案。</summary>
    public static string SurveyLockedConfigMessage(string? lang) => BizMessages.SurveyLockedConfig(lang);

    /// <summary>设计坐标被锁定时的拒绝文案。</summary>
    public static string SurveyLockedDesignMessage(string? lang) => BizMessages.SurveyLockedDesign(lang);

    /// <summary>预拱度被锁定时的拒绝文案。</summary>
    public static string SurveyLockedCamberMessage(string? lang) => BizMessages.SurveyLockedCamber(lang);

    #endregion
}
