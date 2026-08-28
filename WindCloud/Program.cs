// 引入业务层CCSHJ WebApi命名空间
using BIM.Business.CCSHJWebApi;
// 引入数据工厂（含仓储工厂、UnitOfWork工厂等）
using DataFactory.Factory;
// 引入基础Service（Cookie、SSO、登录等）
using Service.Base;
// 引入技术中心Service
using Service.TechCenter;
// 引入Wind主模块Service
using Service.Wind;
// 引入Wind后台管理Service
using Service.Wind.BackManage;
// 引入Wind布局Service
using Service.Wind.Layout;
// 引入工具类库
using Tool;

// 创建Web应用构建器，加载配置和中间件管道
var builder = WebApplication.CreateBuilder(args);

// 注册多数据库仓储工厂，用于生成不同数据库的Repository实例
builder.Services.AddScoped<MultiDbRepositoryFactory>();

// 注册技术中心集成平台的KingBaseUnitOfWork工厂，连接KingBase技术中心数据库
builder.Services.AddScoped<TechCenter_KingBase_UnitOfWorkFactory>(sp =>
    new TechCenter_KingBase_UnitOfWorkFactory(
        connectionName: "KingBase_TechCenterDBConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);
// 注册云平台的KingBaseUnitOfWork工厂，连接KingBase云平台数据库
builder.Services.AddScoped<CloudWind_KingBase_UnitOfWorkFactory>(sp =>
    new CloudWind_KingBase_UnitOfWorkFactory(
        connectionName: "KingBase_CloudWindConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);

// 注册拖航模块的KingBaseUnitOfWork工厂，连接KingBase拖航数据库
builder.Services.AddScoped<Towing_KingBase_UnitOfWorkFactory>(sp =>
    new Towing_KingBase_UnitOfWorkFactory(
        connectionName: "KingBase_TowingDbConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);

// 注册云平台的SQL ServerUnitOfWork工厂，连接SQL Server云平台数据库
builder.Services.AddScoped<CloudWind_Sql_UnitOfWorkFactory>(sp =>
    new CloudWind_Sql_UnitOfWorkFactory(
        connectionName: "SqlServer_CloudWindConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);
// 注册拖航模块的SQL ServerUnitOfWork工厂，连接SQL Server拖航数据库
builder.Services.AddScoped<Towing_Sql_UnitOfWorkFactory>(sp =>
    new Towing_Sql_UnitOfWorkFactory(
        connectionName: "SqlServer_TowingDbConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);

// 注册测试库的KingBaseUnitOfWork工厂，连接KingBase测试数据库
builder.Services.AddScoped<TestDB_KingBase_Test_UnitOfWorkFactory>(sp =>
    new TestDB_KingBase_Test_UnitOfWorkFactory(
        connectionName: "KingBase_TestDBConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);
// 注册云平台测试库的SQL ServerUnitOfWork工厂，连接SQL Server测试云平台数据库
builder.Services.AddScoped<CloudWind_Sql_Test_UnitOfWorkFactory>(sp =>
    new CloudWind_Sql_Test_UnitOfWorkFactory(
        connectionName: "SqlServer_TestCloudWindConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);
// 注册拖航测试库的SQL ServerUnitOfWork工厂，连接SQL Server测试拖航数据库
builder.Services.AddScoped<Towing_Sql_Test_UnitOfWorkFactory>(sp =>
    new Towing_Sql_Test_UnitOfWorkFactory(
        connectionName: "SqlServer_TestTowingDbConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);

// 注册Cookie服务，用于读写Cookie
builder.Services.AddScoped<CookieService>();
// 注册SSO单点登录服务
builder.Services.AddScoped<SSOService>();
// 注册登录记录服务，记录用户登录日志
builder.Services.AddScoped<ManageLoginRecordService>();
// 注册技术中心服务，处理技术中心业务逻辑
builder.Services.AddScoped<TechCenterService>();
// 注册Web验证服务，用于权限和登录校验
builder.Services.AddScoped<WebValidateService>();
// 注册项目服务，处理项目管理相关业务
builder.Services.AddScoped<ProjectService>();
// 注册后台项目管理服务
builder.Services.AddScoped<BackProjectService>();
// 注册后台管理服务
builder.Services.AddScoped<BackManageService>();
// 注册后台平台服务
builder.Services.AddScoped<BackPlatformService>();
// 注册后台审批流程服务
builder.Services.AddScoped<BackFlowService>();
// 注册后台资料库服务
builder.Services.AddScoped<BackLibraryService>();
// 注册后台平台角色服务
builder.Services.AddScoped<BackPlatRoleService>();
// 注册后台抄送通知服务
builder.Services.AddScoped<BackCopyInformService>();
// 注册后台设置服务
builder.Services.AddScoped<BackSettingService>();
// 注册后台项目角色服务
builder.Services.AddScoped<BackProjectRoleService>();
// 注册后台勘察院报告服务
builder.Services.AddScoped<BackKZYReportService>();
// 注册后台拖航管理服务
builder.Services.AddScoped<BackTowingManageService>();
// 注册后台拖航报告服务
builder.Services.AddScoped<BackTowingReportService>();
// 注册登录服务，处理用户登录登出逻辑
builder.Services.AddScoped<LoginService>();
// 注册云平台信息服务，处理云平台数据查询
builder.Services.AddScoped<CloudWindInfoService>();
// 注册云中心服务
builder.Services.AddScoped<CloudCenterService>();
// 注册看板头部服务（接口+实现），用于看板数据展示
builder.Services.AddScoped<IBoardHeaderService, BoardHeaderService>();

// 注册MVC控制器和视图服务
builder.Services.AddControllersWithViews();

// 配置JSON序列化选项，属性名保持原样（PascalCase），不做驼峰转换
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        
    });

// 注册HttpClient工厂，支持通过IHttpClientFactory发起HTTP请求
builder.Services.AddHttpClient();

// 注册HttpContext访问器，使非Controller层也能获取当前HTTP请求上下文
builder.Services.AddHttpContextAccessor();



// 构建应用实例，完成中间件管道的组装
var app = builder.Build();

// 初始化工具类中的CookieUtils，注入IServiceProvider供静态方法使用
CookieUtils.Initialize(app.Services);

// 启用静态文件中间件（wwwroot目录下的css/js/图片等）
app.UseStaticFiles(); 


// 启用路由中间件，匹配请求URL到对应的Controller Action
app.UseRouting();

// 启用授权中间件，执行权限校验
app.UseAuthorization();

// 配置Area区域路由模板，优先匹配带Area的请求
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// 配置默认路由模板，无Area时走此路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// 启动应用，监听端口并开始接收请求
app.Run();
