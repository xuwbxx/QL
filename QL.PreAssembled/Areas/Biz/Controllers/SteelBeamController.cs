using Microsoft.AspNetCore.Mvc;
using Model.tech.QL;
using Model.tech.QL.DTO.BizProject;
using Service.PreAssembled;

namespace QL.PreAssembled.Areas.Biz.Controllers
{
    [Area("Biz")]
    public class SteelBeamController : Controller
    {
        private readonly SteelBeamService _steelBeamService;

        public SteelBeamController(SteelBeamService steelBeamService)
        {
            _steelBeamService = steelBeamService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> List([FromBody] SteelBeamQueryDTO req)
        {
            try
            {
                var res = await _steelBeamService.List(req ?? new SteelBeamQueryDTO());
                return Json(EPApiResult.Success(res));
            }
            catch (Exception ex)
            {
                return Json(EPApiResult.Fail(ex.Message));
            }
        }

        [HttpGet]
        public async Task<JsonResult> ProjectOptions()
        {
            try
            {
                var res = await _steelBeamService.ProjectOptions();
                return Json(EPApiResult.Success(res));
            }
            catch (Exception ex)
            {
                return Json(EPApiResult.Fail(ex.Message));
            }
        }

        [HttpGet]
        public async Task<JsonResult> BridgeOptions(int projID)
        {
            try
            {
                var res = await _steelBeamService.BridgeOptions(projID);
                return Json(EPApiResult.Success(res));
            }
            catch (Exception ex)
            {
                return Json(EPApiResult.Fail(ex.Message));
            }
        }

        [HttpGet]
        public async Task<JsonResult> BridgeInfo(int id)
        {
            try
            {
                var res = await _steelBeamService.GetBridgeInfo(id);
                return Json(EPApiResult.Success(res));
            }
            catch (Exception ex)
            {
                return Json(EPApiResult.Fail(ex.Message));
            }
        }

        [HttpGet]
        public IActionResult LinearControl(int id)
        {
            ViewBag.BridgeID = id;
            return View();
        }
    }
}
