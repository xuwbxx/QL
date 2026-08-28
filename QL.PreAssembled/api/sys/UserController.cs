using Microsoft.AspNetCore.Mvc;

using Service.PreAssembled;

namespace QL.PreAssembled.api.sys
{
    [Route("api/sys/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private SysUserService _userService;
        public UserController(SysUserService userService)
        {
            _userService = userService;
        }
        [HttpPost]
        public JsonResult Menu()
        {

            return new JsonResult(_userService.GetMenu("admin"));
        }
    }
}
