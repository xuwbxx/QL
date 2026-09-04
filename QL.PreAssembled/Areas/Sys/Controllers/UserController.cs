using Microsoft.AspNetCore.Mvc;
using Model.tech.QL;
using Model.tech.QL.DTO.SysUser;
using Service.PreAssembled;

namespace QL.PreAssembled.Areas.Sys.Controllers
{
    [Area("Sys")]
    public class UserController : Controller
    {
        private SysUserService _userService;
        public UserController(SysUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult List([FromBody] SysUserQueryDTO req)
        {
            var list = _userService.GetUserList(req ?? new SysUserQueryDTO());
            var res = EPApiResult.Success(list);
            res.Data = list;
            return Json(res);
        }

        [HttpPost]
        public JsonResult Save([FromBody] SysUserSaveDTO dto)
        {
            try
            {
                var id = _userService.SaveUser(dto);
                return Json(EPApiResult.Success(id));
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                if (ex.InnerException != null)
                    msg = ex.InnerException.Message;
                return Json(EPApiResult.Fail(msg));
            }
        }

        [HttpGet]
        public JsonResult Detail(int id)
        {
            var dto = _userService.GetUserById(id);
            var res = EPApiResult.Success(dto);
            res.Data = dto;
            return Json(res);
        }

        [HttpGet]
        public JsonResult RoleList()
        {
            var list = _userService.GetRoleList();
            var res = EPApiResult.Success(list);
            res.Data = list;
            return Json(res);
        }

        [HttpPost]
        public IActionResult QueryMDUser(string userName)
        {
            try
            {
                var json = _userService.QueryMDUser(userName);
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return Json(EPApiResult.Fail(ex.Message));
            }
        }

        [HttpPost]
        public JsonResult Delete([FromBody] SysUserItemDTO user)
        {
            try
            {
                var res = _userService.DeleteUser(user.ID);
                return Json(EPApiResult.Success(res));
            }
            catch (Exception ex)
            {
                return Json(EPApiResult.Fail(ex.Message));
            }
        }

        [HttpPost]
        public JsonResult ResetPassword([FromBody] SysUserItemDTO user)
        {
            try
            {
                var defaultPwd = "e10adc3949ba59abbe56e057f20f883e";
                var ok = _userService.ResetPassword(user.ID, defaultPwd);
                if (ok)
                    return Json(EPApiResult.Success("密码已重置为123456"));
                else
                    return Json(EPApiResult.Fail("用户不存在或已删除"));
            }
            catch (Exception ex)
            {
                return Json(EPApiResult.Fail(ex.Message));
            }
        }
    }
}
