using DataFactory.Factory;
using Microsoft.EntityFrameworkCore;

namespace DataFactory.SqlServer
{
    /// <summary>
    /// 拖航测试数据库
    /// </summary>
    public class TowingTestDbContext : BaseDbContext
    {
        public TowingTestDbContext(string connectionString, DatabaseType databaseType)
            : base(connectionString, databaseType) { }

        // SQL Server的实体定义（示例）
        public DbSet<Manage_Area> Manage_Area { get; set; }
        public DbSet<View_ProjectArea> View_ProjectArea { get; set; }


    }
}
