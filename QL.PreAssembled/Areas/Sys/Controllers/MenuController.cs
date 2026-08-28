using Microsoft.AspNetCore.Mvc;
using Model.tech.QL;
using Model.tech.QL.DTO.SysMenu;
using Service.PreAssembled;

namespace QL.PreAssembled.Areas.Sys.Controllers
{
    [Area("Sys")]
    public class MenuController : Controller
    {
        private SysMenuService _menuService;
        public MenuController(SysMenuService sysMenuService)
        {
            _menuService = sysMenuService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> List([FromBody] SysMenuQueryDTO req)
        {
            var list = await _menuService.List(req);
            var res = EPApiResult.Success(list);// new EPApiResult<List<SysMenuItemDTO>>();
            res.Data = list;
            return Json(res);
        }

        [HttpPost]
        public async Task<JsonResult> Save([FromBody] SysMenuItemDTO menu)
        {
            var res = await _menuService.Save(menu);
            return Json(EPApiResult.Success(res));
        }

        [HttpPost]
        public async Task<JsonResult> Delete([FromBody] SysMenuItemDTO menu)
        {
            var res = await _menuService.Delete(menu);
            return Json(EPApiResult.Success(res));
        }
    }
}
