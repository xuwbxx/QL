using Microsoft.AspNetCore.Mvc;
using Model.tech.QL;
using Model.tech.QL.DTO.BizProject;
using Service.PreAssembled;

namespace QL.PreAssembled.Areas.Biz.Controllers
{
    [Area("Biz")]
    public class ProjectController : Controller
    {
        private BizProjectService _projectService;
        private BizProjectBridgeService _bridgeService;
        private BizProjectBridgeCastingGroupService _castingGroupService;
        public ProjectController(BizProjectService bizProjectService, BizProjectBridgeService bridgeService, BizProjectBridgeCastingGroupService castingGroupService)
        {
            _projectService = bizProjectService;
            _bridgeService = bridgeService;
            _castingGroupService = castingGroupService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> List([FromBody] BizProjectQueryDTO req)
        {
            var list = await _projectService.List(req);
            var res = EPApiResult.Success(list);// new EPApiResult<List<SysMenuItemDTO>>();
            res.Data = list;
            return Json(res);
        }

        /// <summary>
        /// 获取启用的用户列表（用于项目负责人下拉选择）
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> UserList()
        {
            var list = await _projectService.GetUserList();
            var res = EPApiResult.Success(list);
            res.Data = list;
            return Json(res);
        }

        [HttpPost]
        public async Task<JsonResult> Save([FromBody] BizProjectItemDTO proj)
        {
            var res = await _projectService.Save(proj);
            return Json(EPApiResult.Success(res));
        }

        [HttpPost]
        public async Task<JsonResult> Delete([FromBody] BizProjectItemDTO proj)
        {
            try
            {
                var res = await _projectService.Delete(proj);
                return Json(EPApiResult.Success(res));
            }
            catch (Exception ex)
            {
                return Json(EPApiResult.Fail(ex.Message));
            }
        }

        // ==================== 桥梁子项管理 ====================

        [HttpPost]
        public async Task<JsonResult> BridgeList([FromBody] BizProjectBridgeQueryDTO req)
        {
            var list = await _bridgeService.BridgeList(req);
            var res = EPApiResult.Success(list);
            res.Data = list;
            return Json(res);
        }

        [HttpPost]
        public async Task<JsonResult> BridgeSave([FromBody] BizProjectBridgeItemDTO bridge)
        {
            var res = await _bridgeService.BridgeSave(bridge);
            return Json(EPApiResult.Success(res));

        }

        [HttpPost]
        public async Task<JsonResult> BridgeDelete([FromBody] BizProjectBridgeItemDTO bridge)
        {
            try
            {
                var res = await _bridgeService.BridgeDelete(bridge);
                return Json(EPApiResult.Success(res));
            }
            catch (Exception ex)
            {
                return Json(EPApiResult.Fail(ex.Message));
            }
        }


        // ==================== 浇筑分组管理 ====================

        [HttpPost]
        public async Task<JsonResult> CastingGroupList([FromBody] BizProjectBridgeCastingGroupQueryDTO req)
        {
            var list = await _castingGroupService.CastingGroupList(req);
            var res = EPApiResult.Success(list);
            res.Data = list;
            return Json(res);
        }

        [HttpPost]
        public async Task<JsonResult> CastingGroupSave([FromBody] BizProjectBridgeCastingGroupItemDTO group)
        {
            var res = await _castingGroupService.CastingGroupSave(group);
            return Json(EPApiResult.Success(res));
        }

        [HttpPost]
        public async Task<JsonResult> CastingGroupDelete([FromBody] BizProjectBridgeCastingGroupItemDTO group)
        {
            try
            {
                var res = await _castingGroupService.CastingGroupDelete(group);
                return Json(EPApiResult.Success(res));
            }
            catch (Exception ex)
            {
                return Json(EPApiResult.Fail(ex.Message));
            }
        }
    }
}
