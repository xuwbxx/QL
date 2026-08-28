using DataFactory.Factory;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataFactory.KingBase
{
    /// <summary>
    /// 深基坑监测数据库
    /// </summary>
    public class SJKJCDbContext : BaseDbContext
    {
        public SJKJCDbContext(string connectionString, DatabaseType databaseType)
            : base(connectionString, databaseType) { }

        // 仅定义CloudWind数据库的表（实体类带CloudWind_前缀）
        public DbSet<QL_Users> Users { get; set; } // 映射到CloudWind的Users表

        public DbSet<ZJSHJ_Project> project { get; set; }

        public DbSet<ZJSHJ_Monitor_Point> monitor_pointnumber_datas { get; set; }


        public DbSet<ZJSHJ_Monitor_Record> monitor_records { get; set; }
    }

    [Table("project", Schema = "zjshj")]
    public class ZJSHJ_Project
    {
        public long id { set; get; }

        public string? projectType { set; get; }

        public string? project_no { set; get; }

        public string? project_name { set; get; }

        public DateTime add_time { set; get; }

        /// <summary>
        /// 0:未开始 1：进行中 2：已完成
        /// </summary>
        public int status { set; get; }
    }

    [Table("monitor_pointnumber_datas", Schema = "zjshj")]
    public class ZJSHJ_Monitor_Point
    {
        public long id { set; get; }

        public int pid { set; get; }

        public string? pointNumber { set; get; }

        public string? monitoringType { set; get; }

        public DateTime add_time { set; get; }

    }

    [Table("monitor_records", Schema = "zjshj")]
    public class ZJSHJ_Monitor_Record
    {
        public long id { set; get; }

        public int pid { set; get; }

        public int pointNumberId { set; get; }

        public string? monitoringType { set; get; }

        public string? datacontent { set; get; }

        public DateTime add_time { set; get; }

    }
}
