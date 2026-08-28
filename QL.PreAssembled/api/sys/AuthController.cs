using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.tech.QL.DTO.SysUser;
using Service.PreAssembled;
using System.Security.Claims;
using System.Security.Cryptography;

namespace QL.PreAssembled.api.sys
{
    [Route("api/sys/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private SysUserService _userService;
        public AuthController(SysUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] SysMenuItemDTO dto, string? returnUrl = null)
        {
            // 1. 验证用户（使用你之前的 ValidUserAsync）
            var user = await _userService.ValidUser(dto.Account, dto.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "账号或密码错误" });
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
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { success = true, message = "已退出登录" });
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
