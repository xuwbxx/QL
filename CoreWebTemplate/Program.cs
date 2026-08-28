using DataFactory.Factory;
using Service.Test;
using Service.Towing;
using Service.Wind;

var builder = WebApplication.CreateBuilder(args);
// ==============================================================================
// 1. 注册通用多库工厂

builder.Services.AddScoped<MultiDbRepositoryFactory>();

// 注册工作单元工厂

//正式库
//技术中心集成平台kingbase
builder.Services.AddScoped<TechCenter_KingBase_UnitOfWorkFactory>(sp =>
    new TechCenter_KingBase_UnitOfWorkFactory(
        connectionName: "KingBase_TechCenterDBConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);
//云服务平台kingbase
builder.Services.AddScoped<CloudWind_KingBase_UnitOfWorkFactory>(sp =>
    new CloudWind_KingBase_UnitOfWorkFactory(
        connectionName: "KingBase_CloudWindConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);
//桥梁监测kingbase
builder.Services.AddScoped<QLJC_KingBase_UnitOfWorkFactory>(sp =>
    new QLJC_KingBase_UnitOfWorkFactory(
        connectionName: "KingBase_QLJCConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);
//深基坑监测监测kingbase
builder.Services.AddScoped<SJKJC_KingBase_UnitOfWorkFactory>(sp =>
    new SJKJC_KingBase_UnitOfWorkFactory(
        connectionName: "KingBase_SJKJCConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);

//云服务平台sql
builder.Services.AddScoped<CloudWind_Sql_UnitOfWorkFactory>(sp =>
    new CloudWind_Sql_UnitOfWorkFactory(
        connectionName: "SqlServer_CloudWindConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);
//拖航sql
builder.Services.AddScoped<Towing_Sql_UnitOfWorkFactory>(sp =>
    new Towing_Sql_UnitOfWorkFactory(
        connectionName: "SqlServer_TowingDbConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);

//测试库
//KingBase测试库
builder.Services.AddScoped<TestDB_KingBase_Test_UnitOfWorkFactory>(sp =>
    new TestDB_KingBase_Test_UnitOfWorkFactory(
        connectionName: "KingBase_TestDBConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);
//风电测试数据库
builder.Services.AddScoped<CloudWind_Sql_Test_UnitOfWorkFactory>(sp =>
    new CloudWind_Sql_Test_UnitOfWorkFactory(
        connectionName: "SqlServer_TestCloudWindConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);
//拖航测试库
builder.Services.AddScoped<Towing_Sql_Test_UnitOfWorkFactory>(sp =>
    new Towing_Sql_Test_UnitOfWorkFactory(
        connectionName: "SqlServer_TestTowingDbConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);

// 2. 注册服务
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ManageService>();
builder.Services.AddScoped<ProjectAreaService>();
//builder.Services.AddScoped<IMyService, MyService>();


// 2. 注册 MVC 服务（包含视图、控制器、TempData 等核心功能）
// AddControllersWithViews() 会自动注册 ITempDataDictionaryFactory 等视图依赖的服务
builder.Services.AddControllersWithViews();
//RazorPage风格注册
//builder.Services.AddRazorPages();

// 3.全局配置 JSON 序列化选项
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 保留属性名的原始大小写
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        // 或者显式指定使用PascalCase
        // options.JsonSerializerOptions.PropertyNamingPolicy = new PascalCaseNamingPolicy();
    });

// 4. 可选：注册 HTTP 客户端（用于调用外部 API）
builder.Services.AddHttpClient();
// 5. 关键：注册 IHttpContextAccessor 服务
// （封装当前 HTTP 请求和响应信息的核心对象，包含了请求头、响应头、会话、用户身份、请求路径等关键信息）
builder.Services.AddHttpContextAccessor();



var app = builder.Build();
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

//7.启用静态文件（如 CSS、JS、图片等，默认访问 wwwroot 目录下的文件）
//app.UseStaticFiles();

// 7. 启用路由功能（必须在 UseEndpoints 前）
app.UseRouting();

// 8. 认证与授权中间件（按需启用，需配合 Identity 等库）
//    示例：如果应用需要登录功能，需先注册认证服务（如 JWT、Cookie）
//    app.UseAuthentication();
//    app.UseAuthorization();

// 特性开关
//app.MapControllers();

// 9. 合并所有端点配置到 UseEndpoints 中
app.UseEndpoints(endpoints =>
{

    // 1. Area 路由（匹配带区域前缀的 URL，如 /Tech/Home/Index）
    endpoints.MapControllerRoute(
        name: "areaRoute",
        pattern: "{area}/{controller}/{action}/{id?}",
        defaults: new { action = "Index" },
        constraints: new { area = @"[A-Za-z]+" } // 仅匹配非空 Area
    );

    // 2. 根路径默认路由（访问 / 时，指向 Tech/Home/Index）
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action}/{id?}",
        // 默认值：明确指定 area、controller、action
        defaults: new { area = "Tech", controller = "Manage", action = "Index" }
        // 移除 area = "" 的约束，避免与默认值冲突
    );
});




// 10. 可选：映射 Razor Pages（如果项目包含 Razor Pages）
//app.MapRazorPages();

app.Run();
