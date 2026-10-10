namespace Model.tech.QL.DTO.BizProject
{
    /// <summary>
    /// 混凝土梁管理列表 DTO
    /// </summary>
    public class ConcreteBeamItemDTO
    {
        public int ID { set; get; }
        public int ProjID { set; get; }
        public int GirderID { set; get; }
        public string ProjectName { set; get; } = string.Empty;
        public string BridgeName { set; get; } = string.Empty;
        public string CastingGroupName { set; get; } = string.Empty;
        public string ManagerName { set; get; } = string.Empty;
        public string CreatedBy { set; get; } = string.Empty;
        public DateTime? CreatedTime { set; get; }
    }

    /// <summary>
    /// 混凝土梁管理查询 DTO
    /// </summary>
    public class ConcreteBeamQueryDTO
    {
        public int? ProjID { set; get; }
        public string? CastingGroupName { set; get; }
        public int PageIndex { set; get; } = 1;
        public int PageSize { set; get; } = 10;
    }

    /// <summary>
    /// 混凝土梁管理分页结果 DTO
    /// </summary>
    public class ConcreteBeamPagedResultDTO
    {
        public List<ConcreteBeamItemDTO> List { set; get; } = new();
        public int Total { set; get; }
    }

    public class ConcreteBeamConfigDTO
    {
        public int CastingGroupID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string BridgeName { get; set; } = string.Empty;
        public string CastingGroupName { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public decimal? OffsetA { get; set; }
        public decimal? OffsetB { get; set; }
        public decimal? OffsetC { get; set; }
        public decimal? ReferenceElevation { get; set; }
        public bool IsConfigured { get; set; }
        /// <summary>
        /// 是否存在已确认保存的设计坐标源数据
        /// </summary>
        public bool HasConfirmedDesignCoordinates { get; set; }
        /// <summary>
        /// 是否已经生成可展示的归一化设计坐标
        /// </summary>
        public bool HasCalculatedDesignCoordinates { get; set; }
        /// <summary>
        /// 该浇筑组是否已存在梁段实测填报数据（biz_concrete_beam_segment 有有效行）。
        /// 为 true 时【项目信息】与【设计坐标】的内容锁定，前端禁用相关录入与按钮，
        /// 服务端也会拒绝 SaveConfig / 设计坐标导入 / 预拱度导入。
        /// </summary>
        public bool HasSurveyData { get; set; }
        public bool ElevationIncludesCamber { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string CalculationMessage { get; set; } = string.Empty;
    }

    public class ConcreteBeamConfigSaveDTO
    {
        public int CastingGroupID { get; set; }
        public decimal OffsetA { get; set; }
        public decimal OffsetB { get; set; }
        public decimal OffsetC { get; set; }
        public decimal? ReferenceElevation { get; set; }
    }

    public class ConcreteBeamDataQueryDTO
    {
        public int CastingGroupID { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool UseCalculation { get; set; }
        public bool DisplayIncludesCamber { get; set; }
    }

    public class ConcreteBeamSegmentDTO
    {
        public string SegmentName { get; set; } = string.Empty;
        public string? Downstream { get; set; }
        public string? Upstream { get; set; }
        public int SegmentIndex { set; get; }
        public string Node1 { get; set; } = string.Empty;
        public string Node2 { get; set; } = string.Empty;
        public DateTime? PouringDate { get; set; }
        /// <summary>
        /// 是否首个梁段（SegmentIndex == 0）。首段与其他梁段的录入字段不同：
        /// 首段录固定端模（Bulkhead）三项 + 东西侧浇筑长度，无匹配面实测值、无扭转值。
        /// </summary>
        public bool IsFirst { get; set; }
        /// <summary>该梁段是否已填报过 Survey（已存在填报记录）。</summary>
        public bool IsFilled { get; set; }
        /// <summary>
        /// 该梁段当前是否允许填报/修改。
        /// 规则：不跳段（新填只能是紧邻的下一节）、不返工（只有最后一节可改）。
        /// </summary>
        public bool CanEdit { get; set; }
        /// <summary>不允许填报/修改的原因文案；CanEdit 为 true 时为空字符串。</summary>
        public string LockedReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// 梁段列表（供填报页面使用）：每一段都带 IsFirst / IsFilled / CanEdit / LockedReason 标记，
    /// 由前端自行判断哪一段可填报、哪一段只读。
    /// </summary>
    /// <remarks>
    /// 两个接口共用本 DTO，只是 List 的范围不同：
    /// GetSegmentSurveyList 返回全部梁段；GetSegmentToSurvey 只返回可操作的 1~2 段。
    /// 判断口径始终只有一条：CanEdit == true 才可填报/修改，其余按 LockedReason 置灰。
    /// </remarks>
    public class ConcreteBeamSegmentSurveyDTO
    {
        /// <summary>梁段列表，按 SegmentIndex 升序（全量接口为全部，快捷接口为可操作项）。</summary>
        public List<ConcreteBeamSegmentDTO> List { get; set; } = new();
        /// <summary>所属梁ID（由浇筑组推导，前端无需另行传入）。</summary>
        public int GirderID { get; set; }
        /// <summary>已填报梁段的最大几何序号（0 起）；一次都没填报时为 -1。</summary>
        public int FilledMaxIndex { get; set; } = -1;
        /// <summary>下一个待填报的梁段几何序号；全部填报完毕时为 -1。</summary>
        public int NextSegmentIndex { get; set; } = -1;
        /// <summary>全部梁段是否已填报完毕。</summary>
        public bool AllCompleted { get; set; }
        /// <summary>梁段总数（= 端面数 - 1）。</summary>
        public int TotalSegmentCount { get; set; }
    }

    /// <summary>
    /// 设计坐标「核对与预览」（手册 Coordinate Check and Preview）：
    /// Joints = 已计算坐标（design_coordinate_calculation，含预拱度，不取 source）；
    /// Report = ConstructionGirderAlignment.PrintReport 输出，首项为表头字符串，其余为数据行
    /// （double.NaN 已在服务端替换为 "NaN"，避免 JSON 序列化失败）。
    /// </summary>
    public class ConcreteBeamOrientationPreviewDTO
    {
        public List<ConcreteBeamDesignCoordinateDTO> Joints { get; set; } = new();
        public List<List<object>> Report { get; set; } = new();
    }

    public class ConcreteBeamDesignCoordinateDTO
    {
        public string NodeName { get; set; } = string.Empty;
        public decimal XE { get; set; }
        public decimal YE { get; set; }
        public decimal ZE { get; set; }
        public decimal XC { get; set; }
        public decimal YC { get; set; }
        public decimal ZC { get; set; }
        public decimal XW { get; set; }
        public decimal YW { get; set; }
        public decimal ZW { get; set; }
    }
    public class ConcreteBeamCamberDTO
    {
        public string NodeName { get; set; } = string.Empty;
        public decimal? ZE { get; set; }
        public decimal ZC { get; set; }
        public decimal? ZW { get; set; }
    }

    public class ConcreteBeamPagedResultDTO<T>
    {
        public List<T> List { get; set; } = new();
        public int Total { get; set; }
    }

    public class ConcreteBeamImportErrorDTO
    {
        public int ExcelRow { get; set; }
        public string RecordKey { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string OriginalValue { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class ConcreteBeamImportResultDTO
    {
        public bool Success { get; set; }
        public bool RequiresOverwriteConfirmation { get; set; }
        public int ImportedCount { get; set; }
        public string PreviewToken { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<ConcreteBeamImportErrorDTO> Errors { get; set; } = new();
        public byte[]? ErrorFile { get; set; }
        public string? ErrorFileName { get; set; }
    }

    public class ConcreteBeamImportPreviewQueryDTO : ConcreteBeamDataQueryDTO
    {
        public string PreviewToken { get; set; } = string.Empty;
    }

    public class ConcreteBeamImportConfirmDTO
    {
        public int CastingGroupID { get; set; }
        public string PreviewToken { get; set; } = string.Empty;
        public bool ConfirmOverwrite { get; set; }
    }

    public class ConcreteBeamImportCancelDTO
    {
        public int CastingGroupID { get; set; }
        public string PreviewToken { get; set; } = string.Empty;
        public string ImportType { get; set; } = string.Empty;
    }

    /// <summary>
    /// 立模参数计算（PerfectCast）请求：基于上一梁段实测数据与当前梁段设计数据，
    /// 算出"浇筑时测量应该是什么"的理论目标值（St=0，不做实测修正）。
    /// </summary>
    public class PerfectCastReqDTO
    {
        public int GirderID { set; get; }
        public int CastingGroupID { set; get; }
        public int SegmentID { set; get; }
        /// <summary>梁段名称，格式 {节点1}-{节点2}；以此定位梁段。</summary>
        public string SegmentNo { set; get; } = string.Empty;
        public string JointName1 { set; get; } = string.Empty;
        public string JointName2 { set; get; } = string.Empty;
        public int JointID1 { set; get; }
        public int JointID2 { set; get; }
    }

    /// <summary>
    /// 立模参数计算（PerfectCast）结果：以 St=0 基于「上一梁段实测 + 当前梁段设计」推算，
    /// 即「没有偏差、完美符合设计坐标时，本段的测量值应该是多少」。
    /// 字段名与算法 CalcResult 一一对应，按归属端面分四组，与 MeasuredE* 实测实体的词表一致：
    ///   Match*     —— 匹配端（上一梁段）的 as-cast 端面；首段无匹配段，算法取本段设计端面
    ///   WetCast*   —— 本段（浇筑段）目标端面
    ///   Floating*  —— 活动端模板（首段 South 侧浮动端头）
    ///   Fixed*     —— 固定端模板（North 侧固定端头）
    /// 首段只读区用 Floating* + WetCastEast/WestLength，标准段只读区用 Match* + Fixed*。
    /// </summary>
    public class ConcreteBeamPerfectCastDTO
    {
        public int CastingGroupID { get; set; }
        public int GirderID { get; set; }
        public string SegmentName { get; set; } = string.Empty;
        public int SegmentIndex { get; set; }
        /// <summary>是否首个梁段：true 时前端按起始段布局渲染。</summary>
        public bool IsFirst { get; set; }

        #region 活动端模板（Floating Bulkhead，South 侧浮动端头）—— 起始段只读区
        /// <summary>活动端（South）东侧高程目标值。</summary>
        public decimal FloatingBulkheadEastElev { get; set; }
        /// <summary>活动端（South）西侧高程目标值。</summary>
        public decimal FloatingBulkheadWestElev { get; set; }
        /// <summary>活动端（South）中心偏移量目标值。</summary>
        public decimal FloatingBulkheadCenterOffset { get; set; }
        /// <summary>东侧长度（局部系下南端面东点纵向坐标）。</summary>
        public decimal WetCastEastLength { get; set; }
        /// <summary>西侧长度（局部系下南端面西点纵向坐标）。</summary>
        public decimal WetCastWestLength { get; set; }
        /// <summary>
        /// 中线长度（局部系下南端面中心点纵向坐标）= 本段设计中线长。
        /// 与 WetCastEast/WestLength 同源（同取南端面三点的 X 分量）；仅标准段实测区「浇筑梁段 · 梁段长度」使用。
        /// </summary>
        public decimal WetCastCenterLength { get; set; }
        #endregion

        #region 匹配端（Match，上一梁段 as-cast 端面）—— 标准段只读区「匹配梁段设置参数」
        public decimal MatchNorthEastElev { get; set; }
        public decimal MatchNorthWestElev { get; set; }
        public decimal MatchSouthEastElev { get; set; }
        public decimal MatchSouthWestElev { get; set; }
        public decimal MatchNorthCenterOffset { get; set; }
        public decimal MatchSouthCenterOffset { get; set; }
        public decimal MatchCenterLength { get; set; }
        /// <summary>
        /// 匹配浇筑扭转：匹配端与当前浇筑端之间的扭转量（跨端面耦合量，故不归入 Match/WetCast 任一组）。
        /// 首段与 St=0 时恒为 0（算法只在 OneErr 中产出）。
        /// </summary>
        public decimal MatchCastTwist { get; set; }
        #endregion

        #region 浇筑段（WetCast，本段目标端面）—— 实测区「浇筑梁段」的理论值
        public decimal WetCastSouthEastElev { get; set; }
        public decimal WetCastSouthWestElev { get; set; }
        public decimal WetCastNorthEastElev { get; set; }
        public decimal WetCastNorthWestElev { get; set; }
        public decimal WetCastSouthCenterOffset { get; set; }
        public decimal WetCastNorthCenterOffset { get; set; }
        #endregion

        #region 固定端模板（Fixed Bulkhead，North 侧）
        public decimal FixedBulkheadEastElev { get; set; }
        public decimal FixedBulkheadWestElev { get; set; }
        public decimal FixedBulkheadCenterOffset { get; set; }
        #endregion
    }

    public class PerfectMatchResDTO
    {

    }

    public class SetupNSurveyDTO
    {
        /// <summary>
        /// biz_concrete_beam_segment.ID，已填报时回填；前端新增填报时可传 0。
        /// </summary>
        public int SegmentID { set; get; }
        public int GriderID { set; get; }
        public int CastingGroupID { set; get; }
        /// <summary>
        /// 梁段名称，格式 {节点1}-{节点2}；SaveSurvey 以此定位梁段（查到=更新，查不到=新增）。
        /// </summary>
        public string SegmentName { get; set; } = string.Empty;
        /// <summary>
        /// 梁段几何序号（0 起），等于算法入参 M，由 SegmentName 在设计端面序列中的位置反推后回填。
        /// </summary>
        public int SegmentIndex { get; set; }
        /// <summary>本段南侧（活动端）端面节点名。</summary>
        public string Node1 { get; set; } = string.Empty;
        /// <summary>本段北侧（固定端）端面节点名。</summary>
        public string Node2 { get; set; } = string.Empty;
        /// <summary>
        /// 是否首个梁段（SegmentIndex == 0）。true 时前端按"首段"表单渲染：
        /// 需录入固定端模（Bulkhead）三项与东西侧浇筑长度，且没有匹配面实测值与扭转值。
        /// </summary>
        public bool IsFirst { get; set; }
        /// <summary>浇筑日期，可为空。</summary>
        public DateTime? PouringDate { get; set; }
        #region 活动端（匹配端）
        /// <summary>
        /// 活动端（匹配端）模板东侧高程
        /// </summary>
        public decimal MatchNorthEastElev { get; set; }
        /// <summary>
        /// 活动端（匹配端）模板西侧高程
        /// </summary>
        public decimal MatchNorthWestElev { get; set; }
        /// <summary>
        /// 活动端（匹配端）模板中心偏移量
        /// </summary>
        public decimal MatchNorthCenterOffset { get; set; }

        /// <summary>
        /// 固定端模板东侧高程
        /// </summary>
        public decimal MatchSouthEastElev { get; set; }
        /// <summary>
        /// 固定端模板西侧高程
        /// </summary>
        public decimal MatchSouthWestElev { get; set; }
        /// <summary>
        /// 固定端模板中心偏移量
        /// </summary>
        public decimal MatchSouthCenterOffset { get; set; }

        /// <summary>
        /// 活动端（匹配端）西侧长度
        /// </summary>
        public decimal MatchWestLength { get; set; }
        /// <summary>
        /// 活动端（匹配端）东侧长度
        /// </summary>
        public decimal MatchEastLength { get; set; }
        /// <summary>
        /// 活动端（匹配端）中线长度
        /// </summary>
        public decimal MatchCenterLength { get; set; }

        #endregion

        #region 测点偏位
        /// <summary>
        /// 测点偏位北侧a值（Bolt Offsets's aN）
        /// </summary>
        public decimal BoltOffsetAN { get; set; }
        /// <summary>
        /// 测点偏位北侧c值（Bolt Offsets's cN）
        /// </summary>
        public decimal BoltOffsetCN { get; set; }
        /// <summary>
        /// 测点偏位南侧a值（Bolt Offsets's aS）
        /// </summary>
        public decimal BoltOffsetAS { get; set; }
        /// <summary>
        /// 测点偏位南侧c值（Bolt Offsets's cS）
        /// </summary>
        public decimal BoltOffsetCS { get; set; }
        #endregion

        #region 实测
        #region 实测-活动端（匹配端）
        /// <summary>
        /// 实测匹配端东北角高程
        /// </summary>
        public decimal SurveyMatchNorthEastElev { set; get; }
        /// <summary>
        /// 实测固定端西北高程
        /// </summary>
        public decimal SurveyMatchNorthWestElev { set; get; }
        /// <summary>
        /// 实测活动端（匹配端）东南高程
        /// </summary>
        public decimal SurveyMatchSouthEastElev { set; get; }
        /// <summary>
        /// 实测活动端（匹配端）西南高程
        /// </summary>
        public decimal SurveyMatchSouthWestElev { set; get; }
        /// <summary>
        /// 实测活动端（匹配端）北中偏移
        /// </summary>
        public decimal SurveyMatchNorthCenterOffset { set; get; }
        /// <summary>
        /// 实测活动端（匹配端）南中偏移
        /// </summary>
        public decimal SurveyMatchSouthCenterOffset { set; get; }
        #endregion

        #region 实测-固定端(Bulkhead)
        /// <summary>
        /// 实测固定端东部高程
        /// </summary>
        public decimal SurveyBulkheadEastElev { set; get; }
        /// <summary>
        /// 实测固定端西部高程
        /// </summary>
        public decimal SurveyBulkheadWestElev { set; get; }
        /// <summary>
        /// 实测固定端中心偏移
        /// </summary>
        public decimal SurveyBulkheadCenterOffset { set; get; }

        #endregion

        #region 实测-浇筑梁段(Wet-Cast)
        /// <summary>
        /// 实测浇筑东北高程
        /// </summary>
        public decimal SurveyWetNorthEastElev { set; get; }
        /// <summary>
        /// 实测浇筑西北高程
        /// </summary>
        public decimal SurveyWetNorthWestElev { set; get; }
        /// <summary>
        /// 实测浇筑东南高程
        /// </summary>
        public decimal SurveyWetSouthEastElev { set; get; }
        /// <summary>
        /// 实测浇筑西南高程
        /// </summary>
        public decimal SurveyWetSouthWestElev { set; get; }
        /// <summary>
        /// 实测浇筑北中偏移
        /// </summary>
        public decimal SurveyWetNorthCenterOffset { set; get; }
        /// <summary>
        /// 实测浇筑南中偏移
        /// </summary>
        public decimal SurveyWetSouthCenterOffset { set; get; }
        /// <summary>
        /// 实测浇筑中线长度
        /// </summary>
        public decimal SurveyWetCenterLength { set; get; }
        /// <summary>
        /// 实测浇筑东侧长度，仅 0 号段录入（对应算法首段 E 的 WetCastEastLength）。
        /// </summary>
        public decimal SurveyWetEastLength { set; get; }
        /// <summary>
        /// 实测浇筑西侧长度，仅 0 号段录入（对应算法首段 E 的 WetCastWestLength）。
        /// </summary>
        public decimal SurveyWetWestLength { set; get; }
        #endregion
        /// <summary>
        /// 匹配浇筑扭转：不是录入值，由算法产出（ConstructionGirderAlignment.OneErr 中的 rerr[9]
        /// = (东南误差+西北误差) - (西南误差+东北误差)），Service 计算后装配到此字段返回前端。
        /// 0 号段无匹配段，恒为 0；St=0（PerfectMatch 预览）同样为 0。
        /// </summary>
        public decimal SurveyMatchCastTwist { get; set; }
        #endregion

        #region 回显/状态（仅输出）
        /// <summary>
        /// 本段是否仍可修改：仅当它是"最后一个已填报梁段"时为 true（下一梁段填报后即锁定）。
        /// </summary>
        public bool CanEdit { get; set; }
        /// <summary>不可修改的原因（已锁定/已跳段等），可为空。</summary>
        public string LockedReason { get; set; } = string.Empty;
        #endregion
    }

    /// <summary>SaveSurvey 返回：回填后的录入数据 + 算法立模参数 + 计算消息。</summary>
    public class ConcreteBeamSurveySaveResultDTO
    {
        /// <summary>已落库的填报数据（含 SegmentID/SegmentIndex/Match* 与扭转等算法产出）。</summary>
        public SetupNSurveyDTO Survey { get; set; } = new();
        /// <summary>算法原始结果中的关键标量（Model 层不引用 Service 类型，故平铺在此）。</summary>
        public decimal TheoreticalCenterLength { get; set; }
        /// <summary>本段浇筑中心长度（KT[m] 与 KT[m+1] 的距离）。</summary>
        public decimal AsCastCenterLength { get; set; }
        /// <summary>自首段起算的浇筑中心总长。</summary>
        public decimal AsCastTotalCenterLength { get; set; }
        /// <summary>提示文案（如"已重算并覆盖本段浇筑坐标"）。</summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>本次是否执行了坐标修正（St=1）。false 表示仅理论放样。</summary>
        public bool CorrectionApplied { get; set; }
    }

    /// <summary>
    /// 浇筑坐标页签：表格的一行 = 一个点位（西侧控制点 / 中心线 / 东侧控制点 / 东北侧点位…）。
    /// 坐标为大地坐标系，单位米，统一 4 位小数。
    /// </summary>
    public class ConcreteBeamCoordRowDTO
    {
        /// <summary>行名，如"西侧控制点"、"东北侧点位"、"北侧点位"。</summary>
        public string Label { get; set; } = string.Empty;
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public decimal Z { get; set; }
    }

    /// <summary>
    /// 浇筑坐标页签：一个端面的「理论坐标 + 浇筑坐标」两张表。
    /// 理论坐标取 design_coordinate_calculation（算法 K），浇筑坐标取 ascast_coordinate（算法 KT）。
    /// </summary>
    public class ConcreteBeamEndFaceCoordDTO
    {
        /// <summary>端头称谓："活动端"（起始段南侧 joint 0）/"固定端"（北侧 joint）。</summary>
        public string FaceName { get; set; } = string.Empty;
        /// <summary>端面节点名称（如 KF-P2-1U）。</summary>
        public string NodeName { get; set; } = string.Empty;
        /// <summary>端面序号（0 起），等于算法里的 joint 下标。</summary>
        public int JointIndex { get; set; }
        /// <summary>
        /// 该端面是否已产出浇筑坐标。未填报梁段所在端面为 false，
        /// 此时 AsCast 三行为 0，前端应显示空态而不是 0.0000。
        /// </summary>
        public bool HasAsCast { get; set; }
        /// <summary>理论坐标，固定 3 行：西侧控制点 / 中心线 / 东侧控制点。</summary>
        public List<ConcreteBeamCoordRowDTO> Theoretical { get; set; } = new();
        /// <summary>浇筑坐标，固定 3 行，行序与 Theoretical 一致。</summary>
        public List<ConcreteBeamCoordRowDTO> AsCast { get; set; } = new();
    }

    /// <summary>
    /// 浇筑坐标页签（GetPouringCoords）返回：某个梁段的端面坐标 + 定位点信息。
    /// 全部数据来自已落库的三张表（design_coordinate_calculation / ascast_coordinate /
    /// ascast_measure），不重跑算法；仅中线长度 4 个值按算法口径用 Dist3D 现算。
    /// </summary>
    public class ConcreteBeamPouringCoordDTO
    {
        public int CastingGroupID { get; set; }
        public int GirderID { get; set; }
        /// <summary>梁段名称，格式 {节点1}-{节点2}。</summary>
        public string SegmentName { get; set; } = string.Empty;
        /// <summary>梁段序号（0 起），等于算法入参 M。</summary>
        public int SegmentIndex { get; set; }
        /// <summary>是否起始段（0 号段）。决定端面是两组还是一组。</summary>
        public bool IsFirst { get; set; }
        /// <summary>上游（匹配）梁段名称，起始段为空。对应界面上的"匹配浇筑"。</summary>
        public string UpstreamSegmentName { get; set; } = string.Empty;
        /// <summary>浇筑日期，取自已填报的梁段记录；未填报为 null。</summary>
        public DateTime? PouringDate { get; set; }

        /// <summary>
        /// 端面组。顺序固定：
        /// 起始段 = [活动端(joint 0)，固定端(joint 1)]；标准段 = [固定端(joint M+1)]。
        /// </summary>
        public List<ConcreteBeamEndFaceCoordDTO> Faces { get; set; } = new();

        /// <summary>本段是否已填报（= 是否已有本段浇筑测点/坐标）。未填报时定位点信息为空态。</summary>
        public bool IsFilled { get; set; }
        /// <summary>高程测点坐标，固定 4 行：东北侧 / 西北侧 / 东南侧 / 西南侧。来源 ascast_measure。</summary>
        public List<ConcreteBeamCoordRowDTO> ElevationPoints { get; set; } = new();
        /// <summary>轴柱测点坐标，固定 2 行：北侧点位 / 南侧点位。来源 ascast_measure 的 Center 点。</summary>
        public List<ConcreteBeamCoordRowDTO> AxisPoints { get; set; } = new();

        /// <summary>中线长度-理论值：设计端面中心点 K[M] 与 K[M+1] 的三维距离。</summary>
        public decimal TheoreticalCenterLength { get; set; }
        /// <summary>中线长度-实测值：浇筑端面中心点 KT[M] 与 KT[M+1] 的三维距离。未填报段为 0。</summary>
        public decimal AsCastCenterLength { get; set; }
        /// <summary>中线长度-理论值总长度：K[0] 与 K[M+1] 的三维距离。</summary>
        public decimal TheoreticalTotalCenterLength { get; set; }
        /// <summary>中线长度-实测值总长度：KT[0] 与 KT[M+1] 的三维距离。未填报段为 0。</summary>
        public decimal AsCastTotalCenterLength { get; set; }
    }
}
