using DataFactory.Factory;
using Microsoft.EntityFrameworkCore;

namespace DataFactory.SqlServer
{
    /// <summary>
    /// 拖航数据库
    /// </summary>
    public class TowingDbContext : BaseDbContext
    {
        public TowingDbContext(string connectionString, DatabaseType databaseType)
            : base(connectionString, databaseType) { }

        // SQL Server的实体定义（示例）
        public DbSet<Manage_Area> Manage_Area { get; set; }
        public DbSet<View_ProjectArea> View_ProjectArea { get; set; }


    }

    // SQL Server实体类（示例）
    public class Manage_Area
    {
        public int ID { get; set; }
        public int ProjectID { get; set; }

        public decimal? Lon { set; get; }

        public decimal? Lat { set; get; }

        public DateTime? CreateTime { set; get; }

        public bool IsDelete { set; get; }

        // 其他字段...
    }

    public class View_ProjectArea
    {
        public int ID { get; set; }
        public int ProjectID { get; set; }

        public decimal? Lon { set; get; }

        public decimal? Lat { set; get; }

        public DateTime? CreateTime { set; get; }

        public bool IsDelete { set; get; }

        public int CloudProjectID { set; get; }

        public string? ProjectName { set; get; }
    }
}
