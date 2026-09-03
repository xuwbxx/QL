using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.tech.QL;
using Model.tech.QL.DTO.BizProject;
using Service.PreAssembled;

namespace QL.PreAssembled.Areas.Biz.Controllers
{
    [Area("Biz")]
    public class ConcreteBeamController : Controller
    {
        private readonly ConcreteBeamService _concreteBeamService;

        public ConcreteBeamController(ConcreteBeamService concreteBeamService)
        {
            _concreteBeamService = concreteBeamService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> List([FromBody] ConcreteBeamQueryDTO req)
        {
            try
            {
                var res = await _concreteBeamService.List(req ?? new ConcreteBeamQueryDTO());
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
                var res = await _concreteBeamService.ProjectOptions();
                return Json(EPApiResult.Success(res));
            }
            catch (Exception ex)
            {
                return Json(EPApiResult.Fail(ex.Message));
            }
        }

        [HttpGet]
        public async Task<JsonResult> CastingGroupInfo(int id)
        {
            try
            {
                var res = await _concreteBeamService.GetCastingGroupInfo(id);
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
            ViewBag.GroupID = id;
            return View();
        }
    }
}
