using Microsoft.AspNetCore.Http;
using System;

namespace Service.PreAssembled.BridgeSegmentAlignment
{
    // ============================================================
    // 消息接口
    // ============================================================

    /// <summary>
    /// 算法输出消息提供者。每个请求独立持有一个实例，保证多语言并发安全。
    /// </summary>
    public interface IAlignmentMessages
    {
        // ---- 通用提示 ----
        string JudgmentPassed { get; }
        string JudgmentFailed(int allowedSegment);
        string ExitRun { get; }
        string CoordinateCorrection { get; }
        string NoCoordinateCorrection { get; }
        string SiteCoordinatesUpdated { get; }
        string MeasurementInputValid { get; }
        string MeasurementInputInvalid(int expected, int actual);
        string InsufficientSegments(int m);
        string InitialSegmentRequiresEntryParamsInitial { get; }
        string StandardSegmentRequiresEntryParamsStandard { get; }

        // ---- 节段匹配参数 ----
        string FloatingBulkheadSetup(int m);
        string MatchCastSetup(int m);
        string EastElev { get; }
        string WestElev { get; }
        string CenterOffset { get; }
        string EastLength { get; }
        string WestLength { get; }
        string Length { get; }
        string South { get; }
        string North { get; }
        string SouthEastElev { get; }
        string SouthWestElev { get; }
        string NorthEastElev { get; }
        string NorthWestElev { get; }

        // ---- 浇筑坐标 ----
        string AsCastCoordinates(int m);
        string BoltsAndHairpins { get; }
        string JointsEastCenterWest { get; }
        string SouthJoint { get; }
        string NorthJoint { get; }
        string TheoreticalCoordinates(int m);
        string BulkheadJoint { get; }

        // ---- 长度 ----
        string LengthOnCenter { get; }
        string Theoretical { get; }
        string Total { get; }
        string AsCast { get; }

        // ---- 偏差曲线 ----
        string SegmentLengthDeviation { get; }
        string SegmentOffsetDeviation { get; }
        string SegmentElevationDeviation { get; }
    }

    // ============================================================
    // 中文实现
    // ============================================================

    /// <summary>中文消息实现（zh-CN）。</summary>
    public class ZhAlignmentMessages : IAlignmentMessages
    {
        public string JudgmentPassed => "判定通过！";
        public string JudgmentFailed(int allowedSegment)
            => $"判定不通过！当前记录允许计算第{allowedSegment}节段";
        public string ExitRun => "退出运行";
        public string CoordinateCorrection => "进行坐标修正";
        public string NoCoordinateCorrection => "不进行坐标修正";
        public string SiteCoordinatesUpdated => "现场拼装坐标已更新";
        public string MeasurementInputValid => "实测数据输入正确";
        public string MeasurementInputInvalid(int expected, int actual)
            => $"请检查实测数据输入格式！期望字段数 {expected}，实际 {actual}";
        public string InsufficientSegments(int m)
            => $"K 中节段数不足，无法计算第 {m} 节段";
        public string InitialSegmentRequiresEntryParamsInitial
            => "0 号段需传入 EntryParamsInitial";
        public string StandardSegmentRequiresEntryParamsStandard
            => "匹配段需传入 EntryParamsStandard";

        public string FloatingBulkheadSetup(int m) => $"No.{m} 浮式模板设置：";
        public string MatchCastSetup(int m) => $"No.{m} 匹配浇筑设置：";
        public string EastElev => "东侧高程";
        public string WestElev => "西侧高程";
        public string CenterOffset => "中心偏距";
        public string EastLength => "东侧长度";
        public string WestLength => "西侧长度";
        public string Length => "长度";
        public string South => "南侧";
        public string North => "北侧";
        public string SouthEastElev => "南东高程";
        public string SouthWestElev => "南西高程";
        public string NorthEastElev => "北东高程";
        public string NorthWestElev => "北西高程";

        public string AsCastCoordinates(int m) => $"No.{m} 浇筑坐标：";
        public string BoltsAndHairpins => "螺栓与钢筋（东螺栓、钢筋、西螺栓）：";
        public string JointsEastCenterWest => "节点（东、中、西）";
        public string SouthJoint => "南侧节点";
        public string NorthJoint => "北侧节点";
        public string TheoreticalCoordinates(int m) => $"No.{m} 理论坐标：";
        public string BulkheadJoint => "模板节点（东、中、西）";

        public string LengthOnCenter => "中心线长度：";
        public string Theoretical => "理论";
        public string Total => "累计";
        public string AsCast => "浇筑";

        public string SegmentLengthDeviation => "节段长度偏差曲线：";
        public string SegmentOffsetDeviation => "节段偏距偏差曲线：";
        public string SegmentElevationDeviation => "节段高程偏差曲线：";
    }

    // ============================================================
    // 英文实现
    // ============================================================

    /// <summary>英文消息实现（en）。</summary>
    public class EnAlignmentMessages : IAlignmentMessages
    {
        public string JudgmentPassed => "Validation passed.";
        public string JudgmentFailed(int allowedSegment)
            => $"Validation failed! Current record allows computing segment {allowedSegment}.";
        public string ExitRun => "Exit.";
        public string CoordinateCorrection => "Performing coordinate correction.";
        public string NoCoordinateCorrection => "No coordinate correction.";
        public string SiteCoordinatesUpdated => "Site assembly coordinates updated.";
        public string MeasurementInputValid => "Measurement data is valid.";
        public string MeasurementInputInvalid(int expected, int actual)
            => $"Invalid measurement data format! Expected {expected} fields, actual {actual}.";
        public string InsufficientSegments(int m)
            => $"Not enough segments in K to compute segment {m}.";
        public string InitialSegmentRequiresEntryParamsInitial
            => "Segment 0 requires EntryParamsInitial.";
        public string StandardSegmentRequiresEntryParamsStandard
            => "Matched segment requires EntryParamsStandard.";

        public string FloatingBulkheadSetup(int m) => $"No.{m} Floating Bulkhead Setup:";
        public string MatchCastSetup(int m) => $"No.{m} Match-Cast Set-up:";
        public string EastElev => "East Elev";
        public string WestElev => "West Elev";
        public string CenterOffset => "Center Offset";
        public string EastLength => "East Length";
        public string WestLength => "West Length";
        public string Length => "Length";
        public string South => "South";
        public string North => "North";
        public string SouthEastElev => "South East Elev";
        public string SouthWestElev => "South West Elev";
        public string NorthEastElev => "North East Elev";
        public string NorthWestElev => "North West Elev";

        public string AsCastCoordinates(int m) => $"No.{m} AS-CAST COORDINATES:";
        public string BoltsAndHairpins => "Bolts and Hairpins (East Bolt, Hairpin, West Bolt):";
        public string JointsEastCenterWest => "Joints (East, Center, West)";
        public string SouthJoint => "South Joint";
        public string NorthJoint => "North Joint";
        public string TheoreticalCoordinates(int m) => $"No.{m} Theoretical COORDINATES:";
        public string BulkheadJoint => "Bulkhead Joint (East, Center, West)";

        public string LengthOnCenter => "Length on Center:";
        public string Theoretical => "Theoretical";
        public string Total => "Total";
        public string AsCast => "As-Cast";

        public string SegmentLengthDeviation => "Segment length deviation curve:";
        public string SegmentOffsetDeviation => "Segment offset deviation curve:";
        public string SegmentElevationDeviation => "Segment elevation deviation curve:";
    }

    // ============================================================
    // 工厂
    // ============================================================

    // ============================================================
    // 工厂
    // ============================================================

    /// <summary>
    /// 消息提供者工厂：按语言标识返回对应实现。
    /// </summary>
    public static class AlignmentMessagesFactory
    {
        /// <summary>
        /// 根据语言标识返回消息实现。
        /// </summary>
        /// <param name="lang">"zh" / "zh-CN" 返回中文；其他返回英文。</param>
        public static IAlignmentMessages Create(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang))
                return new ZhAlignmentMessages();

            return lang.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                ? (IAlignmentMessages)new ZhAlignmentMessages()
                : new EnAlignmentMessages();
        }
    }

    // ============================================================
    // 服务端文案：请求语言解析 + 业务文案字典
    // ============================================================
    // 说明：本文件承载两类文案——
    //   ① 算法消息（上方 IAlignmentMessages，每请求一个实例，供算法/报告输出使用）；
    //   ② 业务文案（下方 BizMessages，静态无状态，供接口 message / 异常 / Excel 错误明细使用）。
    // 二者刻意分开：算法消息需要"每请求一实例"以保证并发安全，业务文案是纯静态文本，
    // 强行并入同一接口会逼着所有业务调用点先构造实例；但都放在本文件，避免多处引用。

    /// <summary>
    /// 请求语言解析。语言来源优先级：① 前端 axios 拦截器写入的 <c>i18n</c> 请求头（zh-cn / en）；
    /// ② Accept-Language；③ 兜底中文。
    /// </summary>
    /// <remarks>
    /// 前端侧见 <c>wwwroot/js/httpUtils.js</c> 的请求拦截器：
    /// <c>config.headers['i18n'] = i18nUtils.getCurrentLanguage()</c>，
    /// 因此走 HttpUtils 的请求都自带语言，无需给任何接口加 lang 参数。
    /// 原生 fetch（如报告 PDF 取字节）不走拦截器，但那些请求只要二进制，不需要文案。
    /// </remarks>
    public static class RequestLanguage
    {
        /// <summary>前端写入语言的请求头名。</summary>
        public const string HeaderName = "i18n";

        /// <summary>从当前 HTTP 上下文解析语言；无上下文时兜底中文。</summary>
        public static string Resolve(IHttpContextAccessor? accessor)
        {
            var request = accessor?.HttpContext?.Request;
            if (request == null) return "zh-CN";

            var header = request.Headers[HeaderName].ToString();
            if (!string.IsNullOrWhiteSpace(header)) return header!;

            var accept = request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrWhiteSpace(accept)) return accept!.Split(',')[0].Trim();

            return "zh-CN";
        }

        /// <summary>是否中文（与 <see cref="AlignmentMessagesFactory.Create"/> 同一判定口径）。</summary>
        public static bool IsChinese(string? lang) =>
            string.IsNullOrWhiteSpace(lang) || lang!.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 业务文案字典（服务端返回给前端的 message、异常文案、Excel 错误明细原因）。
    /// 全部为静态方法，首参 lang；判定口径同 <see cref="RequestLanguage.IsChinese"/>。
    /// </summary>
    public static class BizMessages
    {
        // ---- 项目信息（A/B/C/参考高程） ----
        public static string OffsetMustBePositive(string? lang, string field) => Zh(lang)
            ? $"{field}必须大于0"
            : $"{field} must be greater than 0";
        public static string ReferenceElevationRequired(string? lang) => Zh(lang)
            ? "参考高程不能为空且必须是有效数字"
            : "Reference elevation is required and must be a valid number";
        public static string ProjectConfigRequired(string? lang) => Zh(lang)
            ? "请先在项目信息页签提交大于0的A、B、C及有效参考高程"
            : "Please submit A, B, C (greater than 0) and a valid reference elevation on the Project Properties tab first";
        public static string MaxFourDecimals(string? lang, string field) => Zh(lang)
            ? $"{field}最多保留4位小数"
            : $"{field} must have at most 4 decimal places";

        // ---- 设计坐标 / 预拱度 ----
        public static string DesignRequired(string? lang) => Zh(lang) ? "请先导入设计坐标" : "Please import general coordinates first";
        public static string CamberRequired(string? lang) => Zh(lang) ? "请先导入预拱度" : "Please import camber data first";
        public static string CalculationRequired(string? lang) => Zh(lang)
            ? "尚未生成叠加预拱度后的设计坐标，请先完成设计坐标与预拱度导入"
            : "Design coordinates with camber applied are not available yet; import general coordinates and camber first";
        public static string NoCalculatedDesign(string? lang) => Zh(lang)
            ? "暂无已计算的设计坐标，请先导入设计坐标并完成计算"
            : "No calculated general coordinates yet; import general coordinates and finish the calculation first";
        public static string DesignSourceCountMismatch(string? lang, int sourceCount, int calcCount) => Zh(lang)
            ? $"设计坐标源数据（{sourceCount}条）与计算数据（{calcCount}条）数量不一致，请重新导入设计坐标"
            : $"General coordinate source data ({sourceCount}) and calculated data ({calcCount}) differ in row count; please re-import general coordinates";
        public static string DesignSourceJointMismatch(string? lang, int index, string sourceName, string calcName) => Zh(lang)
            ? $"设计坐标源数据与计算数据的第{index}个端面不一致（{sourceName} / {calcName}），请重新导入设计坐标"
            : $"Joint #{index} differs between general coordinate source ({sourceName}) and calculated data ({calcName}); please re-import general coordinates";
        public static string DesignSourceInconsistent(string? lang) => Zh(lang)
            ? "设计坐标源数据与计算数据不一致，请重新导入设计坐标"
            : "General coordinate source and calculated data are inconsistent; please re-import general coordinates";
        public static string DesignCountNotMatchSegment(string? lang) => Zh(lang)
            ? "设计坐标条数与梁段不匹配，请重新导入设计坐标"
            : "General coordinate row count does not match the segments; please re-import general coordinates";
        public static string CamberAlreadyIncluded(string? lang) => Zh(lang)
            ? "设计坐标标高已含预拱度，无需导入预拱度"
            : "Elevations already include camber; importing camber is not needed";
        public static string DesignCamberSettingUnchanged(string? lang) => Zh(lang)
            ? "包含预拱度设置未变化"
            : "The camber-included setting is unchanged";
        public static string DesignCamberNoDesignData(string? lang) => Zh(lang)
            ? "当前没有设计坐标数据，未做任何变更"
            : "There are no general coordinate rows; nothing was changed";
        public static string DesignCamberSettingUpdated(string? lang) => Zh(lang)
            ? "设计计算设置已更新，已按归一化算法重新生成设计计算数据"
            : "The design calculation settings were updated and the design calculation data was regenerated with normalization";
        public static string DesignCamberSettingUpdatedNoNormalize(string? lang) => Zh(lang)
            ? "设计计算设置已更新，已按 source（含预拱度/0）重新生成设计计算数据"
            : "The design calculation settings were updated and the design calculation data was regenerated from the source (with camber or 0)";
        public static string CamberZeroApplied(string? lang) => Zh(lang)
            ? "当前无可用预拱度数据，设计计算已按预拱度 0 生成，请到【预拱度】页签补充"
            : "No usable camber data is available; the design calculation was generated with camber 0. Please fill it in on the Camber tab";
        public static string DesignCamberSettingConfirm(string? lang, bool normalize, bool hasAsCastData, bool requiresCamber) => Zh(lang)
            ? (normalize
                ? "将按归一化算法重新生成设计计算数据，并作废下游梁段填报/浇筑坐标"
                  + (requiresCamber ? "；当前无可用预拱度，将按 0 补齐" : "")
                  + (hasAsCastData ? "；已有浇筑态数据将被作废" : "")
                : "将按 source（含预拱度/0）重新生成设计计算数据，并作废下游梁段填报/浇筑坐标"
                  + (requiresCamber ? "；当前无可用预拱度，将按 0 补齐" : "")
                  + (hasAsCastData ? "；已有浇筑态数据将被作废" : ""))
              + "。是否继续？"
            : (normalize
                ? "The design calculation data will be regenerated with normalization, and downstream segment survey/as-cast data will be invalidated"
                  + (requiresCamber ? "; no usable camber data is available, so camber 0 will be used" : "")
                  + (hasAsCastData ? "; existing as-cast data will be invalidated" : "")
                : "The design calculation data will be regenerated from the source (with camber or 0), and downstream segment survey/as-cast data will be invalidated"
                  + (requiresCamber ? "; no usable camber data is available, so camber 0 will be used" : "")
                  + (hasAsCastData ? "; existing as-cast data will be invalidated" : ""))
              + ". Continue?";
        public static string DesignChangedReimportCamber(string? lang) => Zh(lang)
            ? "设计坐标已变化，请重新导入预拱度"
            : "General coordinates have changed; please re-import camber data";
        public static string DesignReadSuccess(string? lang, int count) => Zh(lang)
            ? $"成功读取{count}条设计坐标，请点击确定保存"
            : $"{count} general coordinate rows read; click Confirm to save";
        public static string CamberReadSuccess(string? lang, int count) => Zh(lang)
            ? $"成功读取{count}条预拱度数据，请点击确定保存"
            : $"{count} camber rows read; click Confirm to save";
        public static string DesignSaveSuccess(string? lang, int count) => Zh(lang)
            ? $"成功保存{count}条设计坐标"
            : $"{count} general coordinate rows saved";
        public static string CamberSaveSuccess(string? lang, int count) => Zh(lang)
            ? $"成功保存{count}条预拱度数据并更新设计坐标"
            : $"{count} camber rows saved and general coordinates updated";
        public static string SaveFailed(string? lang, string reason) => Zh(lang) ? $"保存失败：{reason}" : $"Save failed: {reason}";
        public static string ExcelParseFailed(string? lang, string reason) => Zh(lang) ? $"Excel解析失败：{reason}" : $"Failed to parse the Excel file: {reason}";
        public static string InvalidImportType(string? lang) => Zh(lang) ? "无效的导入类型" : "Invalid import type";
        public static string ImportFailedSeeDetail(string? lang) => Zh(lang) ? "导入失败，请查看错误明细文件" : "Import failed; see the error detail file";

        // ---- 上游输入锁定（来自 ConcreteBeamWorkflowRules，见该文件的 lang 重载） ----
        public static string SurveyLockedReason(string? lang) => Zh(lang) ? "已有梁段完成实测填报" : "Segments have already been surveyed";
        public static string SurveyLockedConfig(string? lang) => Zh(lang)
            ? SurveyLockedReason(lang) + "，项目信息（A/B/C/参考高程）不允许再修改"
            : SurveyLockedReason(lang) + ": project properties (A / B / C / reference elevation) can no longer be modified";
        public static string SurveyLockedDesign(string? lang) => Zh(lang)
            ? SurveyLockedReason(lang) + "，设计坐标不允许再修改"
            : SurveyLockedReason(lang) + ": general coordinates can no longer be modified";
        public static string SurveyLockedCamber(string? lang) => Zh(lang)
            ? SurveyLockedReason(lang) + "，预拱度不允许再修改"
            : SurveyLockedReason(lang) + ": camber can no longer be modified";
        public static string SurveyLockedAlarm(string? lang) => Zh(lang)
            ? SurveyLockedReason(lang) + "，预警配置不允许再修改"
            : SurveyLockedReason(lang) + ": alarm configuration can no longer be modified";

        // ---- 覆盖确认与下游作废 ----
        public static string DesignOverwriteConfirm(string? lang, bool hasAsCastData) => hasAsCastData
            ? (Zh(lang)
                ? "本次导入将整体替换设计坐标，并作废已填报的梁段实测数据与浇筑坐标，请确认后继续"
                : "This import replaces all general coordinates and voids the recorded survey data and as-cast coordinates. Confirm to continue")
            : (Zh(lang)
                ? "本次导入将整体替换当前浇筑组的设计坐标，请确认后继续"
                : "This import replaces all general coordinates of this casting set. Confirm to continue");
        public static string CamberOverwriteConfirm(string? lang, bool hasAsCastData) => hasAsCastData
            ? (Zh(lang)
                ? "本次导入将整体替换预拱度数据，并作废已填报的梁段实测数据与浇筑坐标，请确认后继续"
                : "This import replaces all camber data and voids the recorded survey data and as-cast coordinates. Confirm to continue")
            : (Zh(lang)
                ? "本次导入将整体替换当前浇筑组的预拱度数据，请确认后继续"
                : "This import replaces all camber data of this casting set. Confirm to continue");
        public static string AsCastInvalidated(string? lang, int count) => Zh(lang)
            ? $"已作废{count}条下游数据（梁段实测/浇筑坐标/浇筑测点），需重新逐段填报"
            : $"{count} downstream records voided (segment survey / as-cast coordinates / as-cast bolts and hairpins); re-enter them segment by segment";

        // ---- 逐段填报 ----
        public static string SkipSegment(string? lang, int filledMaxIndex, int segmentIndex) => filledMaxIndex < 0
            ? (Zh(lang)
                ? $"不允许跳段填报：请先填报第1个梁段（当前要填的是第{segmentIndex + 1}个）"
                : $"Segments must be entered in order: start with segment 1 (segment {segmentIndex + 1} was requested)")
            : (Zh(lang)
                ? $"不允许跳段填报：当前已填报到第{filledMaxIndex + 1}个梁段，请先填报第{filledMaxIndex + 2}个梁段"
                : $"Segments must be entered in order: segments up to {filledMaxIndex + 1} are done; enter segment {filledMaxIndex + 2} first");
        public static string LockedSegment(string? lang, int filledMaxIndex, int segmentIndex) => Zh(lang)
            ? $"第{segmentIndex + 1}个梁段之后已填报到第{filledMaxIndex + 1}个梁段，不允许返工修改"
            : $"Segment {segmentIndex + 1} is locked because segments up to {filledMaxIndex + 1} have been entered; rework is not allowed";
        public static string ParameterRequired(string? lang) => Zh(lang) ? "参数不能为空" : "Request payload is required";
        public static string SegmentNameRequired(string? lang) => Zh(lang) ? "缺少梁段名称" : "Segment name is required";
        public static string SegmentNameRequiredForSurvey(string? lang) => Zh(lang)
            ? "缺少梁段名称，无法确定要填报的梁段"
            : "Segment name is required to identify the segment to survey";
        public static string SegmentNameRequiredForCalc(string? lang) => Zh(lang)
            ? "缺少梁段名称，无法确定要计算的梁段"
            : "Segment name is required to identify the segment to calculate";
        public static string NoSegmentGenerated(string? lang) => Zh(lang)
            ? "尚未生成梁段，请先完成设计坐标与预拱度导入"
            : "No segments generated yet; import general coordinates and camber first";
        public static string PreviousSegmentNotFilled(string? lang, string previousName) => Zh(lang)
            ? $"上一梁段（{previousName}）尚未填报，无法计算本段"
            : $"The previous segment ({previousName}) has not been surveyed; this segment cannot be calculated";
        public static string PreviousSegmentNotFilledForSetup(string? lang, string previousName) => Zh(lang)
            ? $"上一梁段（{previousName}）尚未填报，无法计算本段立模参数"
            : $"The previous segment ({previousName}) has not been surveyed; set-up parameters cannot be calculated";
        public static string SurveyDataIncomplete(string? lang) => Zh(lang)
            ? "实测数据未通过算法判定（E 数组项数不符），请检查录入项是否完整"
            : "Survey data rejected by the algorithm (E array length mismatch); check that all inputs are complete";
        public static string NoCorrectedCoordinates(string? lang) => Zh(lang)
            ? "算法未输出修正后的浇筑坐标，请检查录入数据"
            : "The algorithm returned no corrected as-cast coordinates; check the entered data";
        public static string AlgorithmValidationFailed(string? lang) => Zh(lang)
            ? "算法判定未通过，请检查设计坐标与已填报数据"
            : "Algorithm validation failed; check the general coordinates and the recorded survey data";
        public static string SurveySaved(string? lang, int index) => Zh(lang)
            ? $"已保存第{index}个梁段并完成浇筑坐标修正"
            : $"Segment {index} saved and as-cast coordinates corrected";
        public static string SurveyRecalculated(string? lang, int index) => Zh(lang)
            ? $"已重算第{index}个梁段并覆盖其浇筑坐标（未新增记录，原地更新）"
            : $"Segment {index} recalculated and its as-cast coordinates overwritten (updated in place, no new record)";

        // ---- 权限与上下文 ----
        public static string NoProjectPermission(string? lang) => Zh(lang) ? "无该项目的数据权限" : "No data permission for this project";
        public static string AccountMissing(string? lang) => Zh(lang) ? "未获取到当前登录用户" : "The current user could not be resolved";
        public static string AccountDisabled(string? lang) => Zh(lang) ? "当前登录用户不存在或已停用" : "The current user does not exist or is disabled";
        public static string CastingGroupMissing(string? lang) => Zh(lang) ? "混凝土梁浇筑组不存在或已失效" : "The casting set does not exist or is no longer valid";

        public static string AlgorithmResultInvalid(string? lang) => Zh(lang)
            ? "线形计算结果异常（NaN/Infinity），请检查录入的实测数据与设计坐标"
            : "Alignment result is invalid (NaN/Infinity); check the survey data and general coordinates entered";
        public static string MeasuredDataRequired(string? lang) => Zh(lang)
            ? "实测数据未填写或全部为 0，无法计算；请先填写实测数据后再保存"
            : "Survey data is empty or all zeros; please enter the survey data before saving";

        /// <summary>UseB=true（已勾选“已有实测 b 值”）时，四个 b 值必须全部填写。</summary>
        public static string BoltOffsetBRequired(string? lang) => Zh(lang)
            ? "已勾选“已有实测 b 值”，请填写完整的四个 b 值（bNE、bNW、bSE、bSW）"
            : "You selected \"measured b values\"; please fill in all four b values (bNE, bNW, bSE, bSW)";

        /// <summary>仅更新浇筑日期的成功提示。</summary>
        public static string PouringDateUpdated(string? lang, string segmentName) => Zh(lang)
            ? $"已更新梁段「{segmentName}」的浇筑日期"
            : $"Casting date updated for segment \"{segmentName}\"";

        /// <summary>仅更新浇筑日期时，未找到该梁段的填报记录。</summary>
        public static string SegmentSurveyNotFound(string? lang, string segmentName) => Zh(lang)
            ? $"未找到梁段「{segmentName}」的填报记录，无法只更新浇筑日期"
            : $"No survey record found for segment \"{segmentName}\"; the casting date cannot be updated alone";
        public static string ErrorFileName(string? lang, DateTime timestamp) => Zh(lang)
            ? $"混凝土梁_导入错误明细_{timestamp:yyyyMMddHHmmss}.xlsx"
            : $"ConcreteBeam_ImportErrors_{timestamp:yyyyMMddHHmmss}.xlsx";

        // ---- 错误明细（导入失败原因 / 表头 / 字段名） ----
        public static string FieldFile(string? lang) => Zh(lang) ? "文件" : "File";
        public static string FieldDesign(string? lang) => Zh(lang) ? "设计坐标" : "General Coordinates";
        public static string FieldCamber(string? lang) => Zh(lang) ? "预拱度" : "Camber";
        public static string[] ErrorFileHeaders(string? lang) => Zh(lang)
            ? new[] { "行号", "测点编号", "字段名称", "原始值", "失败原因" }
            : new[] { "Row", "Record Key", "Field", "Original Value", "Reason" };

        // ---- 导入解析（Excel 错误明细与模板） ----
        public static string MissingRequiredColumn(string? lang, string column) => Zh(lang)
            ? $"缺少必需列：{column}"
            : $"Missing required column: {column}";
        public static string NoDataInFirstSheet(string? lang) => Zh(lang) ? "第一个工作表没有数据" : "The first worksheet contains no data";
        public static string TooManyRows(string? lang, int max) => Zh(lang)
            ? $"数据行数不能超过{max}条"
            : $"The row count must not exceed {max}";
        public static string FormulaNotAllowed(string? lang) => Zh(lang) ? "导入字段不允许使用公式" : "Formulas are not allowed in imported fields";
        public static string ValueRequired(string? lang, string field) => Zh(lang) ? $"{field}不能为空" : $"{field} is required";
        public static string MustBeNumber(string? lang, string field) => Zh(lang) ? $"{field}必须是有效数字" : $"{field} must be a valid number";
        public static string MustBeNumberOrEmpty(string? lang, string field) => Zh(lang)
            ? $"{field}必须是有效数字或为空"
            : $"{field} must be a valid number or empty";
        public static string TextTooLong(string? lang, string field, int maxLength) => Zh(lang)
            ? $"{field}不能超过{maxLength}个字符"
            : $"{field} must not exceed {maxLength} characters";
        public static string DuplicateSegmentName(string? lang) => Zh(lang)
            ? "同一文件内梁段名称重复（忽略大小写）"
            : "Duplicate segment name in the same file (case-insensitive)";
        public static string DuplicateNodeName(string? lang) => Zh(lang)
            ? "同一文件内节点名称重复（忽略大小写）"
            : "Duplicate joint name in the same file (case-insensitive)";
        public static string Node2SameAsNode1(string? lang) => Zh(lang) ? "节点2不得与节点1相同" : "Node 2 must differ from Node 1";
        public static string PouringDateFormat(string? lang) => Zh(lang) ? "浇筑日期格式必须为yyyy-MM-dd" : "Pouring date must be in yyyy-MM-dd format";
        public static string XcNotMonotonic(string? lang) => Zh(lang)
            ? "XC必须按数据行顺序严格递增或严格递减，不允许相邻行相等"
            : "XC must increase or decrease strictly down the rows; adjacent rows must not be equal";
        public static string XcDirectionInconsistent(string? lang) => Zh(lang)
            ? "XC必须按数据行顺序保持同一递增或递减方向"
            : "XC must keep the same increasing or decreasing direction down the rows";
        public static string DuplicatePointCode(string? lang, string field) => Zh(lang)
            ? $"文件内{field}重复（忽略大小写）"
            : $"Duplicate {field} in the file (case-insensitive)";

        // ---- 钢梁导入（SteelBeamExcelHelper） ----
        public static string PointCodeRequired(string? lang) => Zh(lang) ? "测点编号不能为空" : "Point code is required";
        public static string PointCodeInvalid(string? lang) => Zh(lang)
            ? "仅允许1-50位可见半角字符，禁止空格、中文、换行及以=、+、-、@开头"
            : "Only 1-50 visible half-width characters are allowed; spaces, CJK characters, line breaks and a leading = + - @ are not allowed";
        public static string SegmentIncomplete(string? lang) => Zh(lang) ? "该梁段实测数据不完整" : "Survey data for this segment is incomplete";
        public static string SegmentMinPoints(string? lang, string segmentNo, int current) => Zh(lang)
            ? $"梁段 {segmentNo} 有效测点不足，至少需要3个（当前 {current} 个）"
            : $"Segment {segmentNo} has too few valid survey points; at least 3 are required (current {current})";
        public static string SteelMeasuredIncompletePointsSkipped(string? lang, IEnumerable<string> pointCodes)
        {
            var codes = string.Join("、", pointCodes);
            return Zh(lang)
                ? $"以下测点因 X/Y/Z 坐标不完整，视为未上传已跳过：{codes}"
                : $"The following points were skipped (treated as not uploaded) due to incomplete X/Y/Z coordinates: {codes}";
        }
        public static string SteelMeasuredNoValidPoints(string? lang) => Zh(lang)
            ? "本次导入没有可入库的有效测点（均缺少完整的 X/Y/Z 坐标）"
            : "No valid survey points to import (all rows are missing complete X/Y/Z coordinates)";
        public static string SteelMeasuredNoSelection(string? lang) => Zh(lang)
            ? "未选择要删除的实测数据"
            : "No survey rows selected for deletion";
        public static string FieldSegmentNo(string? lang) => Zh(lang) ? "所属梁段号" : "Segment No.";
        public static string FieldPointCode(string? lang) => Zh(lang) ? "测点编号" : "Point Code";
        public static string[] SteelErrorFileHeaders(string? lang) => Zh(lang)
            ? new[] { "Excel行号", "测点编号", "字段名称", "原始值", "失败原因" }
            : new[] { "Excel Row", "Point Code", "Field", "Original Value", "Reason" };

        // ---- 钢梁业务（SteelBeamService） ----
        public static string FieldBusinessRule(string? lang) => Zh(lang) ? "业务规则" : "Business Rule";
        public static string FieldDatabase(string? lang) => Zh(lang) ? "数据库" : "Database";
        public static string FieldCoordinate(string? lang) => Zh(lang) ? "坐标" : "Coordinates";
        public static string ErrorSheetName(string? lang) => Zh(lang) ? "错误明细" : "Error Details";

        public static string SteelTheoryLockedForImport(string? lang) => Zh(lang)
            ? "已存在实测数据，理论数据不可导入"
            : "Survey data already exists; theoretical data cannot be imported";
        public static string SteelTheoryLockedForEdit(string? lang) => Zh(lang)
            ? "已存在实测数据，理论数据不可修改"
            : "Survey data already exists; theoretical data cannot be modified";
        public static string SteelTheoryRequired(string? lang) => Zh(lang) ? "请先导入理论数据" : "Please import theoretical data first";
        public static string SteelTheoryOverwriteConfirm(string? lang) => Zh(lang)
            ? "本次导入将覆盖当前全部理论数据，请确认后继续"
            : "This import overwrites all existing theoretical data. Confirm to continue";
        public static string SteelTheoryChangedConfirm(string? lang) => Zh(lang)
            ? "理论数据已变化，请确认覆盖后重试"
            : "Theoretical data has changed; confirm the overwrite and retry";
        public static string SteelMeasuredOverwriteConfirm(string? lang) => Zh(lang)
            ? "本次导入将覆盖原数据，请确认后继续"
            : "This import overwrites the existing data. Confirm to continue";
        public static string SteelMeasuredChangedConfirm(string? lang) => Zh(lang)
            ? "实测数据已变化，请确认覆盖后重试"
            : "Survey data has changed; confirm the overwrite and retry";
        public static string SteelPointCodeUsedElsewhere(string? lang) => Zh(lang)
            ? "测点编号已在同一项目的其他桥梁中使用"
            : "The point code is already used by another girder in the same project";
        public static string SteelPointCodeNotInTheory(string? lang) => Zh(lang)
            ? "测点编号不存在于当前桥梁理论数据"
            : "The point code does not exist in the theoretical data of this girder";
        public static string SteelPushCountOutOfRange(string? lang) => Zh(lang)
            ? "顶推次数只能选择当前最大次数或最大次数加一"
            : "Only the current maximum push count or that plus one can be selected";
        public static string SteelPushCountChanged(string? lang) => Zh(lang)
            ? "顶推次数已变化，请刷新后重试"
            : "The push count has changed; refresh and retry";
        public static string SteelPushCountNotEditable(string? lang) => Zh(lang)
            ? "仅最新顶推批次（最新测量时间）的数据可编辑或删除"
            : "Only the data of the latest push batch (latest measure time) can be edited or deleted";
        public static string SteelMeasureTimeInFuture(string? lang) => Zh(lang)
            ? "测量时间不能晚于当前小时"
            : "Measure time must not be later than the current hour";
        public static string SteelMeasureTimeOlderThanLatest(string? lang) => Zh(lang)
            ? "本次测量时间早于该顶推次数已有批次的最新时间；同一顶推次数以最新测量时间的批次为准，不能上传更早的批次"
            : "The measure time is earlier than the latest existing batch for this push count; the latest measure time wins, so an earlier batch cannot be uploaded";
        public static string SteelTheoryImportSuccess(string? lang, int count) => Zh(lang)
            ? $"成功导入{count}条理论数据"
            : $"{count} theoretical rows imported";
        public static string SteelMeasuredImportSuccess(string? lang, int count) => Zh(lang)
            ? $"成功导入{count}条实测数据"
            : $"{count} survey rows imported";
        public static string SteelMeasuredImportSuccessWithDerived(string? lang, int count, int derivedCount) => Zh(lang)
            ? $"成功导入{count}条实测数据，推算快照已更新（共{derivedCount}条坐标，含段内补全与历史点搬运）"
            : $"{count} survey rows imported; derived snapshot updated ({derivedCount} coordinates, including filled-in and transported points)";
        public static string NoDownloadData(string? lang) => Zh(lang) ? "当前筛选条件下无可下载数据" : "No data to download under the current filter";
        public static string DataUpdatedRefresh(string? lang) => Zh(lang) ? "数据已更新，请刷新后重试" : "The data has changed; refresh and retry";
        public static string SteelGirderMissing(string? lang) => Zh(lang) ? "钢梁不存在或已失效" : "The steel girder does not exist or is no longer valid";
        public static string ProjectInfoNotFound(string? lang) => Zh(lang) ? "未找到项目信息" : "Project information could not be found";
        public static string FirstSegmentTooFewPoints(string? lang, string girderName) => Zh(lang)
            ? $"首段控制点数量不足，至少需要3个测点进行刚体配准(桥梁：{girderName})"
            : $"The first segment has too few control points; at least 3 are required for rigid registration (girder: {girderName})";

        // ---- 钢梁推算坐标（derived_coordinate：已推出胎架梁段的当批坐标） ----
        public static string SteelDerivedNoCommonSegments(string? lang, int pushCount) => Zh(lang)
            ? $"第{pushCount}次顶推上传的梁段与第{pushCount - 1}次顶推完全不重叠，无法推算已推出胎架梁段的当批坐标；请在本次上传中至少包含一段上次也上传过的在架梁段，用于配准两次顶推的相对位移"
            : $"The segments uploaded for push {pushCount} do not overlap at all with push {pushCount - 1}; coordinates of the pushed-out segments cannot be derived. Please include at least one in-bed segment that was also uploaded in the previous batch to register the relative displacement between the two pushes";
        public static string SteelDerivedCommonPointsInsufficient(string? lang, int pushCount, int count) => Zh(lang)
            ? $"第{pushCount}次顶推的配准公共点不足：仅有{count}个测点在上一批次与本批次都实测，至少需要3个；请检查本批次导入的测点"
            : $"Registration for push {pushCount} has only {count} common point(s) measured in both the previous and the current batch; at least 3 are required. Check the points imported for this batch";
        public static string SteelDerivedPrevBatchMissing(string? lang, int pushCount) => Zh(lang)
            ? $"未找到第{pushCount - 1}次顶推的实测批次，无法推算第{pushCount}次顶推的已推出梁段坐标"
            : $"The survey batch for push {pushCount - 1} was not found; coordinates of the pushed-out segments for push {pushCount} cannot be derived";
        public static string SteelDerivedCompletionInsufficient(string? lang, int pushCount, int count) => Zh(lang)
            ? $"第{pushCount}次顶推的段内补全上传点不足：至少需要3个本批上传测点作为配准基准，当前仅{count}个；请补充本批次上传的测点"
            : $"Segment completion for push {pushCount} has only {count} uploaded point(s); at least 3 are required as the registration anchor. Add more points to this batch";
        public static string SteelDerivedNothingToRebuild(string? lang) => Zh(lang)
            ? "当前钢梁尚无实测导入批次，无需重算推算坐标"
            : "This girder has no survey batch yet; there is nothing to rebuild";
        public static string SteelDerivedRebuildSuccess(string? lang, int batchCount, int pointCount) => Zh(lang)
            ? $"已重算{batchCount}个顶推批次的推算坐标，共{pointCount}条"
            : $"Derived coordinates rebuilt for {batchCount} push batch(es), {pointCount} row(s) in total";
        public static string SteelDerivedSegmentPointsInsufficient(string? lang, string segmentNo, int count, int minPoints) => Zh(lang)
            ? $"梁段「{segmentNo}」本批仅上传{count}个测点，段内补全至少需要{minPoints}个；请补充该梁段的测点"
            : $"Segment \"{segmentNo}\" has only {count} uploaded point(s) in this batch; at least {minPoints} are required for segment completion";
        public static string SteelDerivedNoRepeatedSegment(string? lang, int pushCount) => Zh(lang)
            ? $"第{pushCount}次顶推没有可用于配准的重复梁段：需要至少一段在上一次、本次各上传不少于2个测点"
            : $"Push {pushCount} has no repeated segment usable for registration: at least one segment must have 2 or more uploaded points in both the previous and the current batch";
        public static string SteelDerivedSegmentTheoryMissing(string? lang, string segmentNo) => Zh(lang)
            ? $"梁段「{segmentNo}」在理论数据中不存在，无法进行段内补全"
            : $"Segment \"{segmentNo}\" does not exist in the theoretical data; segment completion is not possible";
        public static string SteelDerivedSnapshotMissing(string? lang, string segmentName) => Zh(lang)
            ? $"梁段「{segmentName}」已拼装，但推算坐标快照中缺少该段数据；请先执行“重算推算坐标”后再计算"
            : $"Segment \"{segmentName}\" is assembled, but its rows are missing from the derived snapshot. Rebuild the derived coordinates before calculating";
        public static string SteelCalcSegmentsNotContinuous(string? lang) => Zh(lang)
            ? "所选梁段必须按梁段顺序连续，不能跳段；请重新选择"
            : "The selected segments must form a continuous range in segment order; skipping segments is not allowed";
        public static string SteelCalcRegistrationPointsInsufficient(string? lang, int count) => Zh(lang)
            ? $"可用于待拼装配准的实测点不足：当前仅{count}个，至少需要3个"
            : $"Not enough measured-origin points for pending-segment registration: only {count} available, at least 3 required";
        public static string SteelSegmentNotInTheory(string? lang, string segmentNo) => Zh(lang)
            ? $"梁段「{segmentNo}」不在理论数据中，无法计算"
            : $"Segment \"{segmentNo}\" is not present in the theoretical data and cannot be calculated";
        public static string SteelRebuildBatchFilterRequired(string? lang) => Zh(lang)
            ? "批量重建请至少指定项目ID或梁ID列表"
            : "Specify at least a project ID or a list of girder IDs for the batch rebuild";
        public static string SteelRebuildBatchNoGirder(string? lang) => Zh(lang)
            ? "没有找到符合条件的钢梁"
            : "No steel girders matched the rebuild filter";

        private static bool Zh(string? lang) => RequestLanguage.IsChinese(lang);
    }
}