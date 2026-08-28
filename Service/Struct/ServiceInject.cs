using DataFactory.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.Base.Data;
using Service.TechCenter;
using Service.Test;
using Service.Towing;
using Service.Wind;

namespace Service.Struct
{
    public static class ServiceInject  // 改为静态类，方便直接调用
    {
        /// <summary>
        /// 配置所有服务依赖（封装注册逻辑）
        /// </summary>
        /// <param name="services">服务集合</param>
        public static void ConfigureServices(IServiceCollection services)
        {
            // 1. 注册配置
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            services.AddSingleton<IConfiguration>(config);

            // 2. 注册通用多库工厂
            services.AddScoped<MultiDbRepositoryFactory>();

            // 注册工作单元工厂
            //正式库
            //技术中心集成平台kingbase
            services.AddScoped<TechCenter_KingBase_UnitOfWorkFactory>(sp =>
                new TechCenter_KingBase_UnitOfWorkFactory(
                    connectionName: "KingBase_TechCenterDBConnection",
                    multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
                )
            );
            //云服务平台kingbase
            services.AddScoped<CloudWind_KingBase_UnitOfWorkFactory>(sp =>
                new CloudWind_KingBase_UnitOfWorkFactory(
                    connectionName: "KingBase_CloudWindConnection",
                    multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
                )
            );
            //桥梁监测kingbase
            services.AddScoped<QLJC_KingBase_UnitOfWorkFactory>(sp =>
                new QLJC_KingBase_UnitOfWorkFactory(
                    connectionName: "KingBase_QLJCConnection",
                    multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
                )
            );
            //深基坑监测监测kingbase
            services.AddScoped<SJKJC_KingBase_UnitOfWorkFactory>(sp =>
                new SJKJC_KingBase_UnitOfWorkFactory(
                    connectionName: "KingBase_SJKJCConnection",
                    multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
                )
            );

            //云服务平台sql
            services.AddScoped<CloudWind_Sql_UnitOfWorkFactory>(sp =>
                new CloudWind_Sql_UnitOfWorkFactory(
                    connectionName: "SqlServer_CloudWindConnection",
                    multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
                )
            );
            //拖航sql
            services.AddScoped<Towing_Sql_UnitOfWorkFactory>(sp =>
                new Towing_Sql_UnitOfWorkFactory(
                    connectionName: "SqlServer_TowingDbConnection",
                    multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
                )
            );

            //测试库
            //KingBase测试库
            services.AddScoped<TestDB_KingBase_Test_UnitOfWorkFactory>(sp =>
                new TestDB_KingBase_Test_UnitOfWorkFactory(
                    connectionName: "KingBase_TestDBConnection",
                    multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
                )
            );
            //风电测试数据库
            services.AddScoped<CloudWind_Sql_Test_UnitOfWorkFactory>(sp =>
                new CloudWind_Sql_Test_UnitOfWorkFactory(
                    connectionName: "SqlServer_TestCloudWindConnection",
                    multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
                )
            );
            //拖航测试库
            services.AddScoped<Towing_Sql_Test_UnitOfWorkFactory>(sp =>
                new Towing_Sql_Test_UnitOfWorkFactory(
                    connectionName: "SqlServer_TestTowingDbConnection",
                    multiDbFactory: sp.GetRequiredService<MultiDbRepositoryFactory>()
                )
            );


            // 4. 注册业务服务
            services.AddScoped<UserService>();
            services.AddScoped<ProjectAreaService>();
            services.AddScoped<DataLoginService>();
            services.AddScoped<SSOService>();
            services.AddScoped<DataCopyService>();
            services.AddScoped<CloudWindInfoService>();


        }
    }
}
