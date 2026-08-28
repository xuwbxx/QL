using DataFactory.Factory;
using Microsoft.EntityFrameworkCore;

namespace DataFactory.KingBase
{
    /// <summary>
    /// 桥梁监测
    /// </summary>
    public class QLJCDbContext : BaseDbContext
    {
        public QLJCDbContext(string connectionString, DatabaseType databaseType)
            : base(connectionString, databaseType) { }

        // 仅定义CloudWind数据库的表（实体类带CloudWind_前缀）
        public DbSet<QL_Users> Users { get; set; } // 映射到CloudWind的Users表

    }

    public class QL_Users
    {
        public int id { set; get; }

        public string? name { set; get; }

    }
}
