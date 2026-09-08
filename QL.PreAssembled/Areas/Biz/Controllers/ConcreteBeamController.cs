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
    [RequestSizeLimit(ConcreteBeamExcelHelper.MaxFileBytes + 1024 * 1024)]
    public async Task<IActionResult> ImportDesignCoordinates(int castingGroupID, bool confirmOverwrite,
        bool elevationIncludesCamber, IFormFile? file) =>
        await Import(file, bytes => concreteBeamService.ImportDesignCoordinates(
            castingGroupID, bytes, confirmOverwrite, elevationIncludesCamber));
    [HttpPost]
    [RequestSizeLimit(ConcreteBeamExcelHelper.MaxFileBytes + 1024 * 1024)]
    public async Task<IActionResult> ImportCamber(int castingGroupID, bool confirmOverwrite, IFormFile? file) =>
        await Import(file, bytes => concreteBeamService.ImportCamber(castingGroupID, bytes, confirmOverwrite));

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
        if (file.Length > ConcreteBeamExcelHelper.MaxFileBytes) return "文件大小不能超过20 MB";
        return null;
    }

    private async Task<JsonResult> JsonCall<T>(Func<Task<T>> action)
    {
        try { return Json(EPApiResult.Success(await action())); }
        catch (Exception ex) { return Json(EPApiResult.Fail(ex.Message)); }
    }
}
