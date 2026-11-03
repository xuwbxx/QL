using Microsoft.AspNetCore.Mvc;
using Model.tech.QL;
using Model.tech.QL.DTO.BizProject;
using Service.PreAssembled;
using Tool;

namespace QL.PreAssembled.Areas.Biz.Controllers;

[Area("Biz")]
public class ConcreteBeamController(ConcreteBeamService concreteBeamService) : Controller
{
    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult LinearControl(int id)
    {
        ViewBag.GroupID = id;
        return View();
    }

    [HttpPost]
    public async Task<JsonResult> List([FromBody] ConcreteBeamQueryDTO? req) =>
        await JsonCall(() => concreteBeamService.List(req ?? new()));

    [HttpGet]
    public async Task<JsonResult> ProjectOptions() => await JsonCall(concreteBeamService.ProjectOptions);

    [HttpGet]
    public async Task<JsonResult> Config(int id) => await JsonCall(() => concreteBeamService.GetConfig(id));

    // ==================== 误差分析与报警 ====================

    /// <summary>误差分析与报警页面数据：三张折线图（东/中/西）+ 预警记录列表。</summary>
    [HttpGet]
    public async Task<JsonResult> AlarmData(int castingGroupID) =>
        await JsonCall(() => concreteBeamService.GetAlarmPageData(castingGroupID));

    /// <summary>预警配置读取（弹框初始展示）。</summary>
    [HttpGet]
    public async Task<JsonResult> AlarmConfig(int castingGroupID) =>
        await JsonCall(() => concreteBeamService.GetAlarmConfig(castingGroupID));

    /// <summary>预警配置保存（upsert，每浇筑组一行）。</summary>
    [HttpPost]
    public async Task<JsonResult> SaveAlarmConfig([FromBody] ConcreteBeamAlarmConfigSaveDTO req) =>
        await JsonCall(() => concreteBeamService.SaveAlarmConfig(req));

    [HttpPost]
    public async Task<JsonResult> SaveConfig([FromBody] ConcreteBeamConfigSaveDTO req) =>
        await JsonCall(() => concreteBeamService.SaveConfig(req));

    [HttpPost]
    public async Task<JsonResult> SegmentList([FromBody] ConcreteBeamDataQueryDTO req) =>
        await JsonCall(() => concreteBeamService.SegmentList(req));

    [HttpPost]
    public async Task<JsonResult> DesignCoordinateList([FromBody] ConcreteBeamDataQueryDTO req) =>
        await JsonCall(() => concreteBeamService.DesignCoordinateList(req));
    [HttpPost]
    public async Task<JsonResult> CamberList([FromBody] ConcreteBeamDataQueryDTO req) =>
        await JsonCall(() => concreteBeamService.CamberList(req));
    /// <summary>
    /// 设计坐标「核对与预览」：已计算坐标（含预拱度）+ PrintReport 几何报告，供【General Coordinates】页签预览。
    /// </summary>
    [HttpGet]
    public async Task<JsonResult> DesignOrientationPreview(int castingGroupID) =>
        await JsonCall(() => concreteBeamService.DesignOrientationPreview(castingGroupID));

    /// <summary>
    /// 设计坐标「核对与预览」PDF：上半 General Coordinates，下半 ORIENTATION REPORT。
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DesignOrientationPreviewPdf(int castingGroupID, int? timezoneOffsetMinutes = null)
    {
        concreteBeamService.EnsureAuthenticated();
        try
        {
            var bytes = await concreteBeamService.BuildDesignOrientationPreviewPdf(castingGroupID, timezoneOffsetMinutes);
            return File(bytes, "application/pdf", "orientation-preview.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpPost]
    public async Task<JsonResult> DesignImportPreviewList([FromBody] ConcreteBeamImportPreviewQueryDTO req) =>
        await JsonCall(() => concreteBeamService.DesignImportPreviewList(req));
    [HttpPost]
    public async Task<JsonResult> CamberImportPreviewList([FromBody] ConcreteBeamImportPreviewQueryDTO req) =>
        await JsonCall(() => concreteBeamService.CamberImportPreviewList(req));
    [HttpPost]
    public async Task<JsonResult> ConfirmDesignImport([FromBody] ConcreteBeamImportConfirmDTO req) =>
        await JsonCall(() => concreteBeamService.ConfirmDesignImport(req));
    /// <summary>
    /// 仅切换【包含预拱度】并重算设计坐标（设计源已存在，不走上传预览）。
    /// </summary>
    [HttpPost]
    public async Task<JsonResult> ConfirmDesignCamberSetting([FromBody] ConcreteBeamDesignCamberSettingDTO req) =>
        await JsonCall(() => concreteBeamService.ConfirmDesignCamberSetting(req));
    [HttpPost]
    public async Task<JsonResult> ConfirmCamberImport([FromBody] ConcreteBeamImportConfirmDTO req) =>
        await JsonCall(() => concreteBeamService.ConfirmCamberImport(req));
    [HttpPost]
    public async Task<JsonResult> CancelImportPreview([FromBody] ConcreteBeamImportCancelDTO req) =>
        await JsonCall(async () => { await concreteBeamService.CancelImportPreview(req); return true; });
    /// <summary>
    /// 取全部梁段（不分页），每段带 IsFirst / IsFilled / CanEdit / LockedReason 标记，
    /// 前端据此判断哪一段可填报、哪一段只读、是否按首段表单渲染。
    /// </summary>
    /// <param name="castingGroupID">浇筑组ID（梁ID由浇筑组推导，无需另传）。</param>
    [HttpGet]
    public async Task<JsonResult> GetSegmentSurveyList(int castingGroupID) =>
        await JsonCall(() => concreteBeamService.GetSegmentSurveyList(castingGroupID));

    /// <summary>
    /// 只取当前可操作的梁段：最后一个已填报的梁段（下一段填报前仍可修改）+ 下一个待填报的梁段。
    /// 一次都没填报时只返回首个梁段；全部填报完毕时只返回最后一个。
    /// </summary>
    /// <param name="castingGroupID">浇筑组ID（梁ID由浇筑组推导，无需另传）。</param>
    [HttpGet]
    public async Task<JsonResult> GetSegmentToSurvey(int castingGroupID) =>
        await JsonCall(() => concreteBeamService.GetSegmentToSurvey(castingGroupID));

    /// <summary>
    /// 读取某个梁段已填报的实测数据（未填报返回空表单 + CanEdit 状态）。
    /// </summary>
    /// <param name="castingGroupID">浇筑组ID。</param>
    /// <param name="segmentNo">梁段名称，格式 {节点1}-{节点2}。</param>
    [HttpGet]
    public async Task<JsonResult> GetSurvey(int castingGroupID, string segmentNo) =>
        await JsonCall(() => concreteBeamService.GetSurvey(castingGroupID, segmentNo ?? string.Empty));

    /// <summary>
    /// 保存梁段实测填报：写梁段录入数据，跑线形算法并落库浇筑端面/测点坐标。
    /// </summary>
    [HttpPost]
    public async Task<JsonResult> SaveSurvey([FromBody] SetupNSurveyDTO req) =>
        await JsonCall(() => concreteBeamService.SaveSurvey(req));

    /// <summary>
    /// 仅更新梁段浇筑日期：往期已锁定段也允许修改，且只允许改这一个字段。
    /// </summary>
    [HttpPost]
    public async Task<JsonResult> UpdatePouringDate([FromBody] ConcreteBeamPouringDateDTO req) =>
        await JsonCall(() => concreteBeamService.UpdatePouringDate(req));

    /// <summary>
    /// 立模参数计算（PerfectCast）：以 St=0 计算"浇筑时测量应该是什么"的理论目标值，
    /// 用于页面只读区（起始段=活动端/固定端模板，标准段=匹配梁段设置参数）。
    /// 基于上一梁段已落库实测数据 + 当前梁段设计数据，不含本段实测修正。
    /// </summary>
    /// <param name="castingGroupID">浇筑组ID（梁ID由浇筑组推导）。</param>
    /// <param name="segmentNo">梁段名称，格式 {节点1}-{节点2}。</param>
    [HttpGet]
    public async Task<JsonResult> PerfectCast(int castingGroupID, string segmentNo)
    {
        var req = new PerfectCastReqDTO() { CastingGroupID = castingGroupID, SegmentNo = segmentNo ?? string.Empty };
        return await JsonCall(async () => await concreteBeamService.PerfectCast(req));
    }

    /// <summary>
    /// 浇筑坐标页签：取某梁段的端面理论坐标/浇筑坐标 + 定位点信息（高程测点、轴柱测点、中线长度）。
    /// 数据全部读自已落库的坐标表（design_coordinate_calculation / ascast_coordinate / ascast_measure），
    /// 不重跑线形算法；仅中线长度 4 个值按算法口径现算。
    /// 起始段返回两组端面（活动端 joint 0 + 固定端 joint 1），标准段返回一组（固定端 joint m+1）。
    /// </summary>
    /// <param name="castingGroupID">浇筑组ID（梁ID由浇筑组推导）。</param>
    /// <param name="segmentName">梁段名称，格式 {节点1}-{节点2}。</param>
    [HttpGet]
    public async Task<JsonResult> GetPouringCoords(int castingGroupID, string segmentName) =>
        await JsonCall(() => concreteBeamService.GetPouringCoords(castingGroupID, segmentName ?? string.Empty));

    /// <summary>
    /// 单梁段报告 PDF（【报告】页签 · 单梁段报告）。
    /// <paramref name="reportType"/>：setting = 梁段设置，baogao = 已浇筑梁段。
    /// </summary>
    /// <remarks>
    /// 预览（PDF.js 取字节）与导出（浏览器据此下载）共用本接口，靠 Content-Disposition 兼顾两者。
    /// 出错时返回 400 + JSON（而不是 HTML 错误页），前端按 Content-Type 判断后把 message 弹出来——
    /// 否则 PDF.js 对着一段 HTML 只会抛一句无法理解的解析失败。
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> SegmentReportPdf(int castingGroupID, string segmentName, string reportType,
        int? timezoneOffsetMinutes = null)
    {
        concreteBeamService.EnsureAuthenticated();
        try
        {
            var bytes = await concreteBeamService.BuildSegmentReportPdf(
                castingGroupID, segmentName ?? string.Empty, reportType ?? string.Empty, timezoneOffsetMinutes);
            var suffix = string.Equals(reportType, ConcreteBeamService.ReportTypeAsCast, StringComparison.OrdinalIgnoreCase)
                ? ConcreteBeamService.ReportTypeAsCast
                : ConcreteBeamService.ReportTypeSetup;
            return File(bytes, "application/pdf", $"{SanitizeFileName(segmentName)}-{suffix}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 整体报告 PDF（【报告】页签 · 整体报告）。
    /// <paramref name="reportType"/>：project = 项目信息，casttheory = 浇筑和理论坐标，theory = 理论坐标。
    /// <paramref name="includeAll"/> 只对 theory 生效：true = 列出全部 joint。
    /// </summary>
    /// <remarks>
    /// 与单梁段报告一样：预览（PDF.js 取字节）与导出共用本接口；
    /// 出错时返回 400 + JSON（而不是 HTML 错误页），前端据此把 message 直接弹出来。
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> OverallReportPdf(int castingGroupID, string reportType, bool includeAll,
        int? timezoneOffsetMinutes = null)
    {
        concreteBeamService.EnsureAuthenticated();
        try
        {
            var bytes = await concreteBeamService.BuildOverallReportPdf(
                castingGroupID, reportType ?? string.Empty, includeAll, timezoneOffsetMinutes);
            var suffix = reportType switch
            {
                ConcreteBeamService.ReportTypeProject => ConcreteBeamService.ReportTypeProject,
                ConcreteBeamService.ReportTypeCastTheory => ConcreteBeamService.ReportTypeCastTheory,
                _ => includeAll ? "theory-all" : "theory"
            };
            return File(bytes, "application/pdf", $"Overall-{suffix}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>去掉文件名里的非法字符，避免中文梁段名带出的路径字符破坏 Content-Disposition。</summary>
    private static string SanitizeFileName(string? name)
    {
        var value = string.IsNullOrWhiteSpace(name) ? "segment" : name.Trim();
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(c, '_');
        }

        return value;
    }

    [HttpGet]
    public IActionResult DesignCoordinateTemplate()
    {
        concreteBeamService.EnsureAuthenticated();
        return File(concreteBeamService.DesignCoordinateTemplate(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "混凝土梁_设计坐标导入模板.xlsx");
    }
    [HttpGet]
    public IActionResult CamberTemplate()
    {
        concreteBeamService.EnsureAuthenticated();
        return File(concreteBeamService.CamberTemplate(), XlsxContentType, "混凝土梁_预拱度导入模板.xlsx");
    }

    [HttpPost]
    [RequestSizeLimit(ExcelUtils.MaxFileBytes + 1024 * 1024)]
    public async Task<IActionResult> ImportDesignCoordinates(int castingGroupID,
        bool elevationIncludesCamber, IFormFile? file) =>
        await Import(file, bytes => concreteBeamService.PreviewDesignImport(
            castingGroupID, bytes, elevationIncludesCamber));
    [HttpPost]
    [RequestSizeLimit(ExcelUtils.MaxFileBytes + 1024 * 1024)]
    public async Task<IActionResult> ImportCamber(int castingGroupID, IFormFile? file) =>
        await Import(file, bytes => concreteBeamService.PreviewCamberImport(castingGroupID, bytes));

    private async Task<IActionResult> Import(IFormFile? file, Func<byte[], Task<ConcreteBeamImportResultDTO>> action)
    {
        try
        {
            var validation = ValidateUpload(file);
            if (validation != null) return Json(EPApiResult.Fail(validation));
            await using var input = file!.OpenReadStream();
            using var output = new MemoryStream();
            await input.CopyToAsync(output);
            var result = await action(output.ToArray());
            if (result.Success || result.RequiresOverwriteConfirmation)
                return Json(EPApiResult.Success(result));
            Response.Headers["X-Import-Success"] = "false";
            Response.Headers["X-Import-Message"] = Uri.EscapeDataString(result.Message);
            return File(result.ErrorFile!, XlsxContentType, result.ErrorFileName);
        }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }

    private static string? ValidateUpload(IFormFile? file)
    {
        if (file == null || file.Length == 0) return "请选择需要导入的Excel文件";
        if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase)) return "只支持.xlsx文件";
        if (file.Length > ExcelUtils.MaxFileBytes) return "文件大小不能超过20 MB";
        return null;
    }

    private async Task<JsonResult> JsonCall<T>(Func<Task<T>> action)
    {
        try { return Json(EPApiResult.Success(await action())); }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }
}
