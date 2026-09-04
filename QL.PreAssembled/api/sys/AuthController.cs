using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.tech.QL;
using Model.tech.QL.DTO.SysUser;
using QL.PreAssembled.Middleware.Auth;
using QL.PreAssembled.Middleware;
using Service.PreAssembled;
using System.Security.Claims;
using System.Security.Cryptography;

namespace QL.PreAssembled.api.sys
{
    [Route("api/sys/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private SysUserService _userService;
        public AuthController(SysUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] SysMenuItemDTO dto, string? returnUrl = null)
        {
            // 1. 验证用户（使用你之前的 ValidUserAsync）
            var user = await _userService.ValidUser(dto.Account, dto.Password);
            //var user = new LoginUserInfoDTO(); 
            //user.Name = "用户名";
            //user.DeptName = "部门名";
            //user.Account = "admin";
            //user.EmpNo = "yyyynnnnnn";
            //user.DeptOID = "0000000";
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "账号或密码错误" });
            }

            // 2. 创建 Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Account),
                new Claim(ClaimTypes.Name, user.Name ?? user.Account),
                new Claim("DeptName", user.DeptName ?? ""),
                new Claim("DeptOID", user.DeptOID ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // 3. 登录（创建 Cookie）
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            // 4. 返回 JSON
            return Ok(new
            {
                code = 200,
                success = true,
                message = "登录成功",
                user = new
                {
                    account = user.Account,
                    name = user.Name,
                    deptName = user.DeptName
                },
                redirectUrl = "/"
            });
        }

        [HttpPost("logout")]
        [Authorize]
        [Permission("auth:logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(EPApiResult.Success("", "已完成登录"));
        }

        [HttpPost]
        [Authorize]
        public IActionResult GetUserInfo()
        {
            return Ok(new
            {
                code = 200,
                success = true,
                user = new
                {
                    account = User.Identity?.GetAccount(),
                    name = User.Identity?.Name,
                    deptName = User.Identity?.GetDeptName()
                },
            });
        }

        [HttpPost]
        [Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var account = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(account))
                return Ok(new { code = 200, success = false, message = "未登录" });

            try
            {
                _userService.ChangePassword(account, dto.OldPassword, dto.NewPassword);
                return Ok(new { code = 200, success = true, message = "密码修改成功" });
            }
            catch (Exception ex)
            {
                return Ok(new { code = 500, success = false, message = ex.Message });
            }
        }

        private string GenerateRefreshToken()
        {
            // 生成足够随机的 Refresh Token
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
