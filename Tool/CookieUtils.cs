using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Tool
{
    // 静态工具类：Cookie操作基础方法
    public static class CookieUtils
    {
        // 使用服务定位器模式获取 IHttpContextAccessor
        private static IHttpContextAccessor HttpContextAccessor =>
            GetServiceFromProvider<IHttpContextAccessor>();

        // 服务提供器（在应用启动时初始化）
        private static IServiceProvider? _serviceProvider;

        /// <summary>
        /// 初始化服务提供器（在 Program.cs 中调用）
        /// </summary>
        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // 从服务提供器获取服务
        private static TService GetServiceFromProvider<TService>()
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("CookieUtils 尚未初始化，请调用 Initialize 方法");
            }

            var service = _serviceProvider.GetService<TService>();
            if (service == null)
            {
                throw new InvalidOperationException($"无法获取服务: {typeof(TService).Name}");
            }

            return service;
        }

        /// <summary>
        /// 静态方法：保存 Cookie 到响应中
        /// </summary>
        public static void SetCookie(string key, string value, DateTimeOffset? expires = null, bool httpOnly = true, bool secure = false)
        {
            var context = HttpContextAccessor?.HttpContext;
            if (context == null)
            {
                throw new InvalidOperationException("当前不在 HTTP 请求上下文中");
            }

            var options = new CookieOptions
            {
                HttpOnly = httpOnly,
                Secure = secure,
                SameSite = SameSiteMode.Lax,
                Expires = expires ?? DateTime.Now.AddHours(10)
            };

            context.Response.Cookies.Append(key, value, options);
        }

        public static string? GetCookie(string key)
        {
            var context = HttpContextAccessor?.HttpContext;
            if (context?.Request.Cookies.TryGetValue(key, out var value) == true)
            {
                return value;
            }
            return null;
        }

        public static void DeleteCookie(string key)
        {
            var context = HttpContextAccessor?.HttpContext;
            context?.Response.Cookies.Delete(key);
        }
    }
}
