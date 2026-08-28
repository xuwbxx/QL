using DataFactory.Factory;
using Microsoft.AspNetCore.Authentication.Cookies;
using QL.PreAssembled.Middleware;
using Service.Base;
using Service.PreAssembled;
using Tool;

var builder = WebApplication.CreateBuilder(args);

// 注册日志
var logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // ✅ LoginPath：用户未认证时跳转的页面（不是 API）
        options.LoginPath = "/Home/Login";           // MVC 登录页面
        // 或者
        // options.LoginPath = "/login.html";        // 静态登录页面

        // ✅ LogoutPath：退出登录的路径
        options.LogoutPath = "/Home/Logout";

        // ✅ AccessDeniedPath：无权限时的跳转路径
        options.AccessDeniedPath = "/Home/AccessDenied";

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;

        // ⚠️ 如果是 API 模式（前后端分离），需要拦截重定向
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                // 如果是 API 请求（Accept: application/json），返回 401
                if (IsApiRequest(context.Request))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
                else
                {
                    // 如果是页面请求，重定向到登录页面
                    context.Response.Redirect(context.RedirectUri);
                }
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                if (IsApiRequest(context.Request))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                }
                else
                {
                    context.Response.Redirect(context.RedirectUri);
                }
                return Task.CompletedTask;
            }
        };
    });
// 辅助方法：判断是否为 API 请求
static bool IsApiRequest(HttpRequest request)
{
    // 方法1：根据路径判断
    if (request.Path.StartsWithSegments("/api"))
        return true;

    // 方法2：根据 Accept Header 判断
    if (request.Headers["Accept"].ToString().Contains("application/json"))
        return true;

    return false;
}

builder.Services.AddAuthorization();


//// 读取 JWT 配置
//var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("Jwt:Key not configured");
//var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MyApp";
//var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MyApp_API";
//var key = Encoding.UTF8.GetBytes(jwtKey);

// [TODO] jwt

// 1. 注册通用多库工厂
builder.Services.AddScoped<MultiDbRepositoryFactory>();

//正式库
builder.Services.AddScoped<QlPreAssembled_KingBase_UnitOfWorkFactory>(sp =>
    new QlPreAssembled_KingBase_UnitOfWorkFactory(
        connectionName: "KingBase_QlPreAssembledDBConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);

// 2. 注册服务
builder.Services.AddMemoryCache();
builder.Services.AddScoped<CookieService>();
builder.Services.AddScoped<testTableService>();

builder.Services.AddScoped<SysUserService>();
builder.Services.AddScoped<SysRoleService>();
builder.Services.AddScoped<SysMenuService>();
builder.Services.AddScoped<BizProjectService>();
builder.Services.AddScoped<BizProjectBridgeService>();
builder.Services.AddScoped<BizProjectBridgeCastingGroupService>();
builder.Services.AddScoped<SteelBeamService>();

// 3.全局配置 JSON 序列化选项
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        var settings = options.SerializerSettings;
        settings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(); // 驼峰命名（前端JS习惯）
        settings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
        settings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
        settings.Formatting = Newtonsoft.Json.Formatting.None;
        settings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; // 忽略null字段
        // settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; // 解决导航属性循环引用（EF实体直接返回时用）
    });

// 4. 可选：注册 HTTP 客户端（用于调用外部 API）
builder.Services.AddHttpClient();
// 5. 关键：注册 IHttpContextAccessor 服务
// （封装当前 HTTP 请求和响应信息的核心对象，包含了请求头、响应头、会话、用户身份、请求路径等关键信息）
builder.Services.AddHttpContextAccessor();

// 在 builder.Services.AddControllers() 之后加入
builder.Services.AddControllers(options =>
{
    // 注册全局异常过滤器
    options.Filters.Add<GlobalExceptionFilter>();
});


var app = builder.Build();

// 初始化 CookieUtils（注册静态类，因为里面有需要注册的空间）
CookieUtils.Initialize(app.Services);

// 6. 开发环境专用中间件：显示详细异常页（生产环境需移除，避免泄露敏感信息）
// 开发环境禁用静态文件缓存
if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            // 设置缓存控制头
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers["Pragma"] = "no-cache";
            ctx.Context.Response.Headers["Expires"] = "0";
        }
    });
}
else
{
    // 生产环境设置合理的缓存策略
    app.UseStaticFiles();
}

// 7. 启用路由功能（必须在 UseEndpoints 前）
app.UseRouting();

// 8. 认证与授权中间件（按需启用，需配合 Identity 等库）
//    示例：如果应用需要登录功能，需先注册认证服务（如 JWT、Cookie）
app.UseAuthentication();
app.UseAuthorization();

// 特性开关
//app.MapControllers();


// 9. 合并所有端点配置到 UseEndpoints 中
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
});

//app.MapControllers();





// 10. 可选：映射 Razor Pages（如果项目包含 Razor Pages）
//app.MapRazorPages();

app.Run();
