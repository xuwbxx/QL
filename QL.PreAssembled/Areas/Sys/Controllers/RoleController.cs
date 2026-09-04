using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Model.tech.QL;
using Model.tech.QL.DTO.SysRole;
using Service.PreAssembled;
using System.Security.Claims;

namespace QL.PreAssembled.Areas.Sys.Controllers;

[Area("Sys")]
[Authorize]
public class RoleController(SysRoleService service) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (!await Authorized()) return StatusCode(StatusCodes.Status403Forbidden);
        return View();
    }

    [HttpPost] public Task<IActionResult> List([FromBody] SysRoleQueryDTO req) => Run(() => service.List(req));
    [HttpPost] public Task<IActionResult> Save([FromBody] SysRoleItemDTO req) => Run(() => service.Save(req, Actor()));
    [HttpPost] public Task<IActionResult> SetStatus([FromBody] SysRoleStatusDTO req) => Run(async () => { await service.SetStatus(req, Actor()); return true; });
    [HttpPost] public Task<IActionResult> Delete([FromBody] SysRoleIdDTO req) => Run(() => service.Delete(req, Actor()));
    [HttpPost] public Task<IActionResult> Menus([FromBody] SysRoleIdDTO req) => Run(() => service.Menus(req.ID));
    [HttpPost] public Task<IActionResult> SaveMenus([FromBody] SysRoleMenuDTO req) => Run(async () => { await service.SaveMenus(req, Actor()); return true; });
    [HttpPost] public Task<IActionResult> Users([FromBody] SysRoleIdDTO req) => Run(() => service.Users(req.ID));
    [HttpPost] public Task<IActionResult> SaveUsers([FromBody] SysRoleUserDTO req) => Run(async () => { await service.SaveUsers(req, Actor()); return true; });

    private async Task<IActionResult> Run<T>(Func<Task<T>> action)
    {
        if (!await Authorized()) return StatusCode(StatusCodes.Status403Forbidden, EPApiResult.Fail("无角色管理权限"));
        try { return Json(EPApiResult.Success(await action())); }
        catch (InvalidOperationException ex) { return BadRequest(EPApiResult.Fail(ex.Message)); }
    }

    private Task<bool> Authorized() => service.CanManage(Actor());
    private string Actor()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
}
