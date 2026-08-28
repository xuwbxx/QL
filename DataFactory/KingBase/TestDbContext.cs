using DataFactory.Factory;
using Microsoft.EntityFrameworkCore;

namespace DataFactory.KingBase
{
    /// <summary>
    /// 测试数据库
    /// </summary>
    public class TestDbContext : BaseDbContext
    {
        // 新增databaseType参数，传递给基类
        public TestDbContext(string connectionString, DatabaseType databaseType)
            : base(connectionString, databaseType) { }

        // 保持原实体定义不变
        public DbSet<TestDbUsers> Users { get; set; }
    }

    public class TestDbUsers
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Email { get; set; }
    }
}
