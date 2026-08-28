using DataFactory.Factory;
using DataFactory.KingBase;
using Microsoft.Extensions.Configuration;

namespace ConsoleTemplate
{
    public class ManualService
    {
        //手动创建服务
        public static void ManualTest()
        {
            // 1. 手动读取配置（获取数据库连接字符串）
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // 读取项目根目录的 appsettings.json
                .AddJsonFile("appsettings.json")
                .Build();
            // 替换为你的连接名（比如 KingBase_CloudWindConnection）
            var connectionString = config.GetConnectionString("KingBase_TestDBConnection");

            // 2. 手动创建仓储工厂实例（无需 DI）
            var multiDbFactory = new MultiDbRepositoryFactory(config);

            // 3. 获取仓储（使用 using 确保资源释放，推荐）
            using (var testRepo = multiDbFactory.GetRepository<TestDbUsers>("KingBase_TestDBConnection"))
            {
                // 4. 调用异步方法并返回结果
                var data = testRepo.FindAll();
                //return data; // 此时返回类型匹配（IEnumerable<TestDbUsers>）
            }


        }
    }
}
