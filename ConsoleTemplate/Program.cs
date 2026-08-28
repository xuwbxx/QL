// See https://aka.ms/new-console-template for more information
// 1. 创建服务集合
using ConsoleTemplate;
using Microsoft.Extensions.DependencyInjection;
using Service.Struct;
using Service.Test;
using Service.Towing;

//这个是手动调用服务
ManualService.ManualTest();

//这个是服务注入的方法来调用

var services = new ServiceCollection();

// 2. 调用封装好的服务注册方法（核心：一行代码搞定所有配置）
ServiceInject.ConfigureServices(services);

// 3. 构建服务提供器
using var serviceProvider = services.BuildServiceProvider();


try
{
    // 4. 从容器中获取业务服务并调用方法
    var testDbService = serviceProvider.GetRequiredService<UserService>();
    var dataList = await testDbService.GetDataAsync();


    var towingDbService = serviceProvider.GetRequiredService<ProjectAreaService>();
    var dataList2 = await towingDbService.GetListAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"出错了：{ex.Message}");
}

Console.WriteLine("按任意键退出...");
Console.ReadKey();