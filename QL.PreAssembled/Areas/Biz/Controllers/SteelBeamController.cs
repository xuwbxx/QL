using Humanizer;
using MathNet.Numerics.Distributions;
using Microsoft.AspNetCore.Mvc;
using Model.tech.QL;
using Model.tech.QL.DTO.BizProject;
using Model.tech.QL.DTO.BizSteelBeam;
using Service.PreAssembled;
using Tool;

namespace QL.PreAssembled.Areas.Biz.Controllers;

[Area("Biz")]
public class SteelBeamController : Controller
{
    private readonly SteelBeamService _service;
    private readonly IWebHostEnvironment _webEnv;
    public SteelBeamController(SteelBeamService service, IWebHostEnvironment webEnv)
    {
        _service = service;
        _webEnv = webEnv;
    }

    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult LinearControl(int girderID)
    {
        ViewBag.GirderID = girderID;
        return View();
    }

    [HttpPost]
    public Task<JsonResult> List([FromBody] SteelBeamQueryDTO req) => JsonCall(() => _service.List(req ?? new()));

    [HttpGet]
    public Task<JsonResult> ProjectOptions() => JsonCall(_service.ProjectOptions);

    [HttpGet]
    public Task<JsonResult> BridgeOptions(int projID) => JsonCall(() => _service.BridgeOptions(projID));

    [HttpGet]
    public Task<JsonResult> GirderInfo(int girderID) => JsonCall(() => _service.GetGirderInfo(girderID));

    [HttpGet]
    public Task<JsonResult> ImportState(int girderID) => JsonCall(() => _service.GetImportState(girderID));

    [HttpPost]
    public Task<JsonResult> TheoreticalList([FromBody] SteelBeamTheoreticalQueryDTO req) => JsonCall(() => _service.TheoreticalList(req));

    [HttpGet]
    public Task<JsonResult> TheoreticalOptions(int girderID) => JsonCall(() => _service.TheoreticalOptions(girderID));

    [HttpPost]
    public Task<JsonResult> MeasuredList([FromBody] SteelBeamMeasuredQueryDTO req) => JsonCall(() => _service.MeasuredList(req));

    [HttpGet]
    public Task<JsonResult> MeasureTimeOptions(int girderID, int pushCount) => JsonCall(() => _service.MeasureTimeOptions(girderID, pushCount));

    [HttpPost]
    public Task<JsonResult> UpdateTheoretical([FromBody] SteelBeamCoordinateUpdateDTO req) => JsonCall(() => _service.UpdateTheoretical(req));

    [HttpPost]
    public Task<JsonResult> UpdateMeasured([FromBody] SteelBeamCoordinateUpdateDTO req) => JsonCall(() => _service.UpdateMeasured(req));

    [HttpPost]
    public Task<JsonResult> DeleteMeasured([FromBody] SteelBeamCoordinateUpdateDTO req) => JsonCall(() => _service.DeleteMeasured(req));

    /// <summary>实测数据批量删除（仅最新批次可删，返回实际删除行数）。</summary>
    [HttpPost]
    public Task<JsonResult> DeleteMeasuredBatch([FromBody] SteelBeamMeasuredBatchDeleteDTO req) => JsonCall(() => _service.DeleteMeasuredBatch(req));

    [HttpPost]
    [RequestSizeLimit(ExcelUtils.MaxFileBytes + 1024 * 1024)]
    public async Task<IActionResult> ImportTheoretical(int girderID, bool confirmOverwrite, IFormFile file)
    {
        var invalid = ValidateUpload(file);
        if (invalid != null) return Json(EPApiResult.Fail(invalid));
        try
        {
            var bytes = await ReadFile(file);
            var outcome = await _service.ImportTheoretical(girderID, bytes, confirmOverwrite);
            return ImportResponse(outcome);
        }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }

    [HttpPost]
    [RequestSizeLimit(ExcelUtils.MaxFileBytes + 1024 * 1024)]
    public async Task<IActionResult> ImportMeasured(int girderID, int pushCount, DateTime measureTime, bool confirmOverwrite, IFormFile file)
    {
        var invalid = ValidateUpload(file);
        if (invalid != null) return Json(EPApiResult.Fail(invalid));
        try
        {
            var bytes = await ReadFile(file);
            var outcome = await _service.ImportMeasured(girderID, pushCount, measureTime, bytes, confirmOverwrite);
            return ImportResponse(outcome);
        }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }

    [HttpGet]
    public IActionResult TheoreticalTemplate()
    {
        try { _service.EnsureAuthenticated(); return File(_service.TheoreticalTemplate(), XlsxContentType, "钢梁理论数据导入模板.xlsx"); }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }

    [HttpGet]
    public IActionResult MeasuredTemplate()
    {
        try { _service.EnsureAuthenticated(); return File(_service.MeasuredTemplate(), XlsxContentType, "钢梁实测数据导入模板.xlsx"); }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }

    [HttpPost]
    public async Task<IActionResult> DownloadMeasured([FromBody] SteelBeamMeasuredQueryDTO req)
    {
        try
        {
            var result = await _service.ExportMeasured(req);
            return File(result.Bytes, XlsxContentType, result.FileName);
        }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }

    [HttpGet]
    public async Task<IActionResult> GetCalcSegmentNo(int girderId)
    {
        try
        {
            var result = await _service.GetCalcSegmentNoByGirderId(girderId);
            return Ok(EPApiResult.Success(result));

        }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }

    /// <summary>
    /// 重算推算坐标（已推出胎架梁段在各顶推批次下的坐标）。
    /// 导入实测会自动推算本批次；本接口用于回填历史批次或在人工修改实测数据后整体刷新。
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RebuildDerived(int girderId)
    {
        try
        {
            var message = await _service.RebuildDerived(girderId);
            return Ok(EPApiResult.Success(message));
        }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }

    /// <summary>
    /// 批量重建推算坐标：按项目或指定梁列表逐根 replay。
    /// 每根梁独立事务，某根失败不影响其它梁；返回逐根结果。
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RebuildDerivedBatch([FromBody] SteelBeamRebuildBatchDTO req)
    {
        try
        {
            var result = await _service.RebuildDerivedBatch(req);
            return Ok(EPApiResult.Success(result));
        }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }

    [HttpGet]
    public async Task<IActionResult> GetCalcResultBySegmentNos(int girderId, List<string> segmentNos)
    {
        try
        {
            var result = await _service.GetCalcResultBySegment(girderId, segmentNos);
            return Ok(EPApiResult.Success(result));
        }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }

    [HttpPost]
    public async Task<IActionResult> DownloadCalcResult([FromBody] CalcResultDTO calcReq)
    {
        try
        {
            calcReq.EnvFolder = _webEnv.ContentRootPath;
            // 1.读取模板文件（放在项目根目录，和dll同级，不是wwwroot）
            var templatePath = Path.Combine(_webEnv.ContentRootPath, "siteconf", "cccc_3he_techcenter_construction_monitoring_comm_note_template.docx");
            if (!System.IO.File.Exists(templatePath))
            {
                return NotFound("Word模板不存在");
            }
            byte[] templateBytes;
            using (var wpf = Tool.NPOIWordUtils.LoadTemplate(templatePath))
            {
                var replaceDic = new Dictionary<string, string>();
                replaceDic.Add("projName", calcReq.ProjName);
                replaceDic.Add("sheetNo", calcReq.SheetNo);
                replaceDic.Add("reason", calcReq.Reason);
                replaceDic.Add("preDocument", calcReq.PreDocument);
                replaceDic.Add("measuredSegments", calcReq.MeasuredSegments);
                replaceDic.Add("pendingSegments", calcReq.PendingSegments);
                replaceDic.Add("pointMap", string.Empty);
                replaceDic.Add("remark", calcReq.Remark);
                replaceDic.Add("author", calcReq.Author);
                replaceDic.Add("issuedBy", calcReq.IssuedBy);
                replaceDic.Add("issuedDate", calcReq.IssuedDate?.ToString("yyyy-MM-dd") ?? "");
                replaceDic.Add("receivedBy", calcReq.ReceivedBy);
                replaceDic.Add("receivedDate", calcReq.ReceivedDate?.ToString("yyyy-MM-dd") ?? "");


                wpf.ReplacePlaceholders(replaceDic)
                    .FillTableByTemplateRow(0, 8, calcReq.Rows, (tr, row, fontSnap) =>
                {
                    tr.GetCell(1).SetCellTextPreserveStyle(row.PointCode, fontSnap[0]);
                    tr.GetCell(2).SetCellTextPreserveStyle(row.DesignX.ToString(), fontSnap[0]);
                    tr.GetCell(3).SetCellTextPreserveStyle(row.DesignY.ToString(), fontSnap[0]);
                    tr.GetCell(4).SetCellTextPreserveStyle(row.DesignZ.ToString(), fontSnap[4]);
                });
                templateBytes = wpf.ToByteArray();
            }
            if (templateBytes == null)
            {
                templateBytes = await System.IO.File.ReadAllBytesAsync(templatePath);
            }

            // ========== 这里是你的业务：用NPOI/ApachePOI填充模板内容 ==========
            // 伪代码示例：
            // using var ms = new MemoryStream(templateBytes);
            // XWPFDocument doc = new XWPFDocument(ms);
            // 替换书签/占位符，填充dto里面的单据信息 DocNo、Subject、PendingSegments...
            // 写完到新MemoryStream
            // MemoryStream outputMs = new MemoryStream();
            // doc.Write(outputMs);
            // byte[] resultBytes = outputMs.ToArray();
            // ================================================================

            // 临时：先直接返回模板，用于前端联调，替换成上面填充后的resultBytes
            byte[] resultBytes = templateBytes;

            string fileName = $"监控联系单_{calcReq.SheetNo}.docx";
            // 返回文件流
            return File(resultBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }

    [HttpPost]
    public async Task<IActionResult> GetPositionNames(int girderId)
    {
        try
        {
            var result = await _service.GetPositionNameByGirderId(girderId);
            return Ok(EPApiResult.Success(result));
        }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }

    [HttpPost]
    public async Task<IActionResult> GetSteelBeamOffset(int girderId, string positionName, string offsetType, string sort = "asc")
    {
        try
        {
            var result = await _service.GetSteelBeamOffset(girderId, positionName, offsetType, sort);
            return Ok(EPApiResult.Success(result));
        }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }


    private IActionResult ImportResponse(SteelBeamImportOutcome outcome)
    {
        if (outcome.Success) return Json(EPApiResult.Success(new { outcome.ImportedCount, outcome.Message }));
        Response.Headers["X-Import-Success"] = "false";
        Response.Headers["X-Import-Message"] = Uri.EscapeDataString(outcome.Message);
        return File(outcome.ErrorFile!, XlsxContentType, outcome.ErrorFileName);
    }

    private static string? ValidateUpload(IFormFile? file)
    {
        if (file == null || file.Length == 0) return "请选择要导入的文件";
        if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase)) return "仅支持.xlsx文件";
        if (file.Length > ExcelUtils.MaxFileBytes) return "文件大小不能超过20 MB";
        return null;
    }

    private static async Task<byte[]> ReadFile(IFormFile file)
    {
        await using var input = file.OpenReadStream();
        using var output = new MemoryStream();
        await input.CopyToAsync(output);
        return output.ToArray();
    }

    private async Task<JsonResult> JsonCall<T>(Func<Task<T>> action)
    {
        try { return Json(EPApiResult.Success(await action())); }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }

    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}
