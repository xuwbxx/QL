using DataFactory.Factory;
using DataFactory.KingBase;
using Tool;

namespace Service.Test
{

    public class UserService
    {
        // 依赖注入：仓储工厂（核心）+ 日志（可选，用于异常追踪）
        private readonly TestDB_KingBase_Test_UnitOfWorkFactory _testDbFactory;

        public UserService(TestDB_KingBase_Test_UnitOfWorkFactory testDbFactory)
        {
            _testDbFactory = testDbFactory;

        }

        // 使用示例
        public async Task<List<TestDbUsers>?> GetDataAsync()
        {
            try
            {
                using (var uow = _testDbFactory.Create())
                {
                    // 获取TestEntity的仓储
                    var repo = uow.GetRepository<TestDbUsers>();

                    // 调用异步查询所有数据的方法，并转换为List
                    var result = await repo.FindAllAsync();

                    return result.ToList(); // 确保返回List<TestEntity>
                }




            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(UserService));
                return null; // 异常时返回null
            }
        }


    }
}
