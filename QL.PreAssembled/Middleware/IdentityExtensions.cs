using System.Security.Claims;
using System.Security.Principal;

namespace QL.PreAssembled.Middleware
{
    public static class IdentityExtensions
    {
        /// <summary>
        /// 获取账号（封装从Claim读取，替代每次写 FindFirstValue）
        /// </summary>
        public static string? GetAccount(this IIdentity identity)
        {
            if (identity is not ClaimsIdentity ci)
                return null;

            // 这里用你存Claim的Type，例如自定义类型 "Account"
            return ci.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static string? GetDeptOID(this IIdentity identity)
        {
            if (identity is not ClaimsIdentity ci)
                return null;

            return ci.FindFirst("DeptOID")?.Value;
        }

        public static string? GetDeptName(this IIdentity identity)
        {
            if (identity is not ClaimsIdentity ci)
                return null;

            return ci.FindFirst("DeptName")?.Value;
        }
    }
}
