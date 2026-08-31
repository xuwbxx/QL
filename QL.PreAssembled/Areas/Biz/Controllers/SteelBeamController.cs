using Microsoft.AspNetCore.Mvc;
using Model.tech.QL;
using Model.tech.QL.DTO.BizProject;
using Service.PreAssembled;
using Tool;

namespace QL.PreAssembled.Areas.Biz.Controllers;

[Area("Biz")]
public class SteelBeamController : Controller
{
    private readonly SteelBeamService _service;

    public SteelBeamController(SteelBeamService service) => _service = service;

    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult LinearControl(int id)
    {
        ViewBag.BridgeID = id;
        return View();
    }

    [HttpPost]
    public Task<JsonResult> List([FromBody] SteelBeamQueryDTO req) => JsonCall(() => _service.List(req ?? new()));

    [HttpGet]
    public Task<JsonResult> ProjectOptions() => JsonCall(_service.ProjectOptions);

    [HttpGet]
    public Task<JsonResult> BridgeOptions(int projID) => JsonCall(() => _service.BridgeOptions(projID));

    [HttpGet]
    public Task<JsonResult> BridgeInfo(int id) => JsonCall(() => _service.GetBridgeInfo(id));

    [HttpGet]
    public Task<JsonResult> ImportState(int bridgeID) => JsonCall(() => _service.GetImportState(bridgeID));

    [HttpPost]
    public Task<JsonResult> TheoreticalList([FromBody] SteelBeamTheoreticalQueryDTO req) => JsonCall(() => _service.TheoreticalList(req));

    [HttpGet]
    public Task<JsonResult> TheoreticalOptions(int bridgeID) => JsonCall(() => _service.TheoreticalOptions(bridgeID));

    [HttpPost]
    public Task<JsonResult> MeasuredList([FromBody] SteelBeamMeasuredQueryDTO req) => JsonCall(() => _service.MeasuredList(req));

    [HttpGet]
    public Task<JsonResult> MeasureTimeOptions(int bridgeID, int pushCount) => JsonCall(() => _service.MeasureTimeOptions(bridgeID, pushCount));

    [HttpPost]
    public Task<JsonResult> UpdateTheoretical([FromBody] SteelBeamCoordinateUpdateDTO req) => JsonCall(() => _service.UpdateTheoretical(req));

    [HttpPost]
    public Task<JsonResult> UpdateMeasured([FromBody] SteelBeamCoordinateUpdateDTO req) => JsonCall(() => _service.UpdateMeasured(req));

    [HttpPost]
    [RequestSizeLimit(SteelBeamExcelHelper.MaxFileBytes + 1024 * 1024)]
    public async Task<IActionResult> ImportTheoretical(int bridgeID, bool confirmOverwrite, IFormFile file)
    {
        var invalid = ValidateUpload(file);
        if (invalid != null) return Json(EPApiResult.Fail(invalid));
        try
        {
            var bytes = await ReadFile(file);
            var outcome = await _service.ImportTheoretical(bridgeID, bytes, confirmOverwrite);
            return ImportResponse(outcome);
        }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }

    [HttpPost]
    [RequestSizeLimit(SteelBeamExcelHelper.MaxFileBytes + 1024 * 1024)]
    public async Task<IActionResult> ImportMeasured(int bridgeID, int pushCount, DateTime measureTime, bool confirmOverwrite, IFormFile file)
    {
        var invalid = ValidateUpload(file);
        if (invalid != null) return Json(EPApiResult.Fail(invalid));
        try
        {
            var bytes = await ReadFile(file);
            var outcome = await _service.ImportMeasured(bridgeID, pushCount, measureTime, bytes, confirmOverwrite);
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
        if (file.Length > SteelBeamExcelHelper.MaxFileBytes) return "文件大小不能超过20 MB";
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
