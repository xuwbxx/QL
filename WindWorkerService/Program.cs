using DataFactory.Factory;
using Service.Wind.TimeWorker;
using WindWorkerService;

var builder = Host.CreateApplicationBuilder(args);

// 注册多数据库工厂
builder.Services.AddScoped<MultiDbRepositoryFactory>();

// 注册云平台的 KingBase UoW 工厂
builder.Services.AddScoped<CloudWind_KingBase_UnitOfWorkFactory>(sp =>
    new CloudWind_KingBase_UnitOfWorkFactory(
        connectionName: "KingBase_CloudWindConnection",
        multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
    )
);

// 注册台风 Worker 服务
builder.Services.AddScoped<TyphoonWorkerService>();

// 注册气象 Worker 服务
builder.Services.AddScoped<WeatherWorkerService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
