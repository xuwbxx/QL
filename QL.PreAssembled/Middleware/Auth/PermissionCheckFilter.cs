using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.JsonWebTokens;

namespace QL.PreAssembled.Middleware.Auth
{
    /// <summary>
    /// 权限校验过滤器，配合 [Permission("project:query")] 使用
    /// 依赖：IPermissionService 获取用户的权限集合
    /// </summary>
    public class PermissionCheckFilter : IAsyncActionFilter
    {
        private readonly IPermissionService _permissionService;

        public PermissionCheckFilter(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. 跳过AllowAnonymous标记的接口
            var isAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(x => x is AllowAnonymousAttribute);
            if (isAnonymous)
            {
                await next();
                return;
            }

            // 2. 获取当前Action上的 [Permission("xxx")] 特性
            var permissionAttr = context.ActionDescriptor.EndpointMetadata
                .FirstOrDefault(x => x is PermissionAttribute) as PermissionAttribute;

            // 如果Action没有标记Permission特性：代表不需要业务权限，只要登录即可访问
            if (permissionAttr == null)
            {
                await next();
                return;
            }

            string requirePerm = permissionAttr.PermissionCode;

            // 3. 获取登录用户UserId（从JWT解析出来的Claim）
            var userIdClaim = context.HttpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            // 4. 获取该用户拥有的全部权限编码集合（实时查库）
            HashSet<string> userPerms = await _permissionService.GetUserPermissionCodes(userId);

            // 5. 判断是否拥有该权限
            if (!userPerms.Contains(requirePerm))
            {
                // 没有权限 返回403 Forbidden
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
