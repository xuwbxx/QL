using DataFactory.Factory;
using Microsoft.EntityFrameworkCore;

namespace DataFactory.SqlServer
{
    /// <summary>
    /// 云平台数据库
    /// </summary>
    public class CloudWindDbContext : BaseDbContext
    {
        public CloudWindDbContext(string connectionString, DatabaseType databaseType)
            : base(connectionString, databaseType) { }

        // SQL Server的实体定义（示例）
        public DbSet<Manage_Area> Manage_Area { get; set; }
        public DbSet<View_ProjectArea> View_ProjectArea { get; set; }

        public DbSet<Wind_ProjectRole> Wind_ProjectRole { set; get; }
        public DbSet<Library_Geology> Library_Geology { set; get; }
        public DbSet<Library_Geology_DK> Library_Geology_DK { set; get; }
        public DbSet<Library_Geology_Data> Library_Geology_Data { set; get; }
        public DbSet<Manage_Company> Manage_Company { set; get; }
        public DbSet<Manage_Copyer> Manage_Copyer { set; get; }
        public DbSet<Wind_ProjectFile> Wind_ProjectFile { set; get; }
        public DbSet<Manage_Viewer> Manage_Viewer { set; get; }
        public DbSet<Wind_ProjectInfo> Wind_ProjectInfo { set; get; }
        public DbSet<Wind_ProjectFan> Wind_ProjectFan { set; get; }
        public DbSet<Wind_ProjectArea> Wind_ProjectArea { set; get; }
        public DbSet<Wind_Project_Copyer> Wind_Project_Copyer { set; get; }
        public DbSet<Wind_Task> Wind_Task { set; get; }
        public DbSet<Wind_TaskFileDeliver> Wind_TaskFileDeliver { set; get; }

    }

    public class Wind_ProjectRole
    {
        public int ID { set; get; }
        public int? ProjectID { set; get; }
        public int? RoleID { set; get; }
        public string? UserName { set; get; }
        public string? UserCode { set; get; }
        public string? UserDepartName { set; get; }
        public string? UserPhone { set; get; }
        public string? UserJobName { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Library_Geology
    {
        public int ID { set; get; }
        public int ProjectID { set; get; }
        public int? Type { set; get; }
        public string? FileName { set; get; }
        public string? FilePath { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Library_Geology_DK
    {
        public int ID { set; get; }
        public int? ProjectID { set; get; }
        public int? FanID { set; get; }
        public string? DKName { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Manage_Company
    {
        public int ID { set; get; }
        public string? Company { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Manage_Copyer
    {
        public int ID { set; get; }
        public int? SoftwareID { set; get; }
        public string? UserName { set; get; }
        public string? UserCode { set; get; }
        public string? UserDepart { set; get; }
        public string? UserPhone { set; get; }
        public string? UserJobName { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Wind_ProjectFile
    {
        public int ID { set; get; }
        public int ProjectID { set; get; }
        public string? FileName { set; get; }
        public string? FilePath { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Manage_Viewer
    {
        public int ID { set; get; }
        public string? UserName { set; get; }
        public string? UserCode { set; get; }
        public string? UserDepartName { set; get; }
        public string? UserPhone { set; get; }
        public string? UserJobName { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Library_Geology_Data
    {
        public int ID { set; get; }
        public int? DKID { set; get; }
        public string? 序号 { set; get; }
        public string? 地层编号 { set; get; }
        public string? 土层名称 { set; get; }
        public string? 层底标高 { set; get; }
        public string? 土层类型 { set; get; }
        public string? 不排水抗剪强度 { set; get; }
        public string? 砂土摩擦角 { set; get; }
        public string? 有效重度 { set; get; }
        public string? 标贯击数 { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Wind_ProjectInfo
    {
        public int ID { set; get; }
        public int ProjectID { set; get; }
        public string? WaterDepth { set; get; }
        public string? WaterDepthMin { set; get; }
        public string? WaterDepthMax { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool? IsDelete { set; get; }
    }

    public class Wind_ProjectFan
    {
        public int ID { set; get; }
        public int ProjectID { set; get; }
        public string? FanName { set; get; }
        public int? Status { set; get; }
        public string? Lon { set; get; }
        public string? Lat { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Wind_ProjectArea
    {
        public int ID { set; get; }
        public int ProjectID { set; get; }
        public string? AreaLon { set; get; }
        public string? AreaLat { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Wind_Project_Copyer
    {
        public int ID { set; get; }
        public int ProjectID { set; get; }
        public string? UserName { set; get; }
        public string? UserCode { set; get; }
        public string? UserDepart { set; get; }
        public string? UserPhone { set; get; }
        public string? UserJobName { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Wind_Task
    {
        public int ID { set; get; }
        public int ProjectID { set; get; }
        public string? TaskCode { set; get; }
        public string? TaskName { set; get; }
        public int? FlowStatus { set; get; }
        public int? SoftwareID { set; get; }
        public string? Applyer { set; get; }
        public string? ApplyerCode { set; get; }
        public string? ApplyerDepart { set; get; }
        public string? ApplyerPhone { set; get; }
        public string? ApplyerJobName { set; get; }
        public DateTime? DeliverTime { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Wind_TaskFileDeliver
    {
        public int ID { set; get; }
        public int? TaskID { set; get; }
        public string? DeliverName { set; get; }
        public string? DeliverCode { set; get; }
        public string? DeliverDepart { set; get; }
        public string? DeliverPhone { set; get; }
        public string? DeliverJobName { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

}
