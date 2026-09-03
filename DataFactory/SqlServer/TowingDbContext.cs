using DataFactory.Factory;
using Microsoft.EntityFrameworkCore;

namespace DataFactory.SqlServer.Towing
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
        public DbSet<Data_Backup> Data_Backup { get; set; }
        public DbSet<Data_Backup_Mru> Data_Backup_Mru { get; set; }
        public DbSet<Data_Backup_Weather> Data_Backup_Weather { get; set; }
        public DbSet<Data_Battery> Data_Battery { get; set; }
        public DbSet<Data_Device> Data_Device { get; set; }
        public DbSet<Data_EGYRO> Data_EGYRO { get; set; }
        public DbSet<Data_Forecast> Data_Forecast { get; set; }
        public DbSet<Data_HikLocalPic> Data_HikLocalPic { get; set; }
        public DbSet<Data_MRU> Data_MRU { get; set; }
        public DbSet<Data_SailSpeed> Data_SailSpeed { get; set; }
        public DbSet<Data_SATELLITE> Data_SATELLITE { get; set; }
        public DbSet<Data_Wind> Data_Wind { get; set; }
        public DbSet<Manage_Basic> Manage_Basic { get; set; }
        public DbSet<Manage_PlanRoute> Manage_PlanRoute { get; set; }
        public DbSet<Manage_ShipAnchorage> Manage_ShipAnchorage { get; set; }
        public DbSet<Manage_User> Manage_User { get; set; }
        public DbSet<Message_Body> Message_Body { get; set; }


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

    public class Data_Backup
    {
        public int id { set; get; }
        public string? DataString { set; get; }
        public DateTime? CreateTime { set; get; }
    }

    public class Data_Backup_Mru
    {
        public int id { set; get; }
        public string? DataString { set; get; }
        public DateTime? CreateTime { set; get; }
    }

    public class Data_Backup_Weather
    {
        public int id { set; get; }
        public string? bm500 { set; get; }
        public string? humidity { set; get; }
        public string? pressure { set; get; }
        public string? swelldir { set; get; }
        public string? swellheight { set; get; }
        public string? swellperiod { set; get; }
        public string? temperature { set; get; }
        public string? visibility { set; get; }
        public string? waveheight { set; get; }
        public string? winddir { set; get; }
        public string? winddirStr { set; get; }
        public string? windspeed { set; get; }
        public string? windlevel { set; get; }
        public string? windlevelStr { set; get; }
        public string? windwarninglevel { set; get; }
        public bool? windalarm { set; get; }
        public string? time { set; get; }
        public string? timeStr { set; get; }
        public string? oceandir { set; get; }
        public string? oceandirStr { set; get; }
        public string? oceanspeed { set; get; }
        public DateTime? CreateTime { set; get; }
    }

    public class Data_Battery
    {
        public int id { set; get; }
        public int? ProjectID { set; get; }
        public decimal? RemainEle { set; get; }
        public decimal? RemainEle2 { set; get; }
        public decimal? Engine1 { set; get; }
        public decimal? Engine2 { set; get; }
        public decimal? Voltage { set; get; }
        public DateTime? CreateTime { set; get; }
        public DateTime? CreateTime2 { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Data_Device
    {
        public int id { set; get; }
        public int? ProjectID { set; get; }
        public string? Type { set; get; }
        public string? DeviceName { set; get; }
        public DateTime? LatestUpdateTime { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Data_EGYRO
    {
        public int id { set; get; }
        public int? DeviceID { set; get; }
        public decimal? AngleX { set; get; }
        public decimal? AngleY { set; get; }
        public decimal? AzAngle { set; get; }
        public DateTime? CreateTime { set; get; }
        public DateTime? CreateTime2 { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Data_Forecast
    {
        public int id { set; get; }
        public int? No { set; get; }
        public DateTime? ForecastTime { set; get; }
        public decimal? Lat { set; get; }
        public decimal? Lon { set; get; }
        public decimal? MaxYaoX { set; get; }
        public decimal? MaxYaoY { set; get; }
        public decimal? MaxDangZ { set; get; }
        public int? TotalAlarm { set; get; }
        public int? WindAlarm { set; get; }
        public int? WaveAlarm { set; get; }
        public int? ShakeAlarm { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Data_HikLocalPic
    {
        public int id { set; get; }
        public int? ProjID { set; get; }
        public DateTime? SnapTime { set; get; }
        public string? SnapPicUrl { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Data_MRU
    {
        public int id { set; get; }
        public int? DeviceID { set; get; }
        public decimal? DangX { set; get; }
        public decimal? DangY { set; get; }
        public decimal? DangZ { set; get; }
        public decimal? YaoX { set; get; }
        public decimal? YaoY { set; get; }
        public decimal? YaoZ { set; get; }
        public DateTime? CreateTime { set; get; }
        public DateTime? CreateTime2 { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Data_SailSpeed
    {
        public int id { set; get; }
        public int? ProjectID { set; get; }
        public decimal? SailSpeed { set; get; }
        public DateTime? CreateTime { set; get; }
        public DateTime? CreateTime2 { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Data_SATELLITE
    {
        public int id { set; get; }
        public int? DeviceID { set; get; }
        public decimal? Lon { set; get; }
        public string? LonDirection { set; get; }
        public decimal? Lat { set; get; }
        public string? LatDirection { set; get; }
        public decimal? Altitude { set; get; }
        public decimal? Heading { set; get; }
        public DateTime? CreateTime { set; get; }
        public DateTime? CreateTime2 { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Data_Wind
    {
        public int id { set; get; }
        public int? DeviceID { set; get; }
        public decimal? WindSpeed { set; get; }
        public decimal? WindDirection { set; get; }
        public DateTime? CreateTime { set; get; }
        public DateTime? CreateTime2 { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Manage_Basic
    {
        public int id { set; get; }
        public int? ProjectID { set; get; }
        public DateTime? ProjectStartTime { set; get; }
        public decimal? StartLon { set; get; }
        public decimal? StartLat { set; get; }
        public decimal? EndLon { set; get; }
        public decimal? EndLat { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Manage_PlanRoute
    {
        public int id { set; get; }
        public int? ProjectID { set; get; }
        public decimal? Lon { set; get; }
        public decimal? Lat { set; get; }
        public DateTime? RouteTime { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Manage_ShipAnchorage
    {
        public int id { set; get; }
        public string? Type { set; get; }
        public string? Group { set; get; }
        public string? Name { set; get; }
        public decimal? Lon { set; get; }
        public decimal? Lat { set; get; }
        public decimal? Radius { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Manage_User
    {
        public int id { set; get; }
        public string? UserName { set; get; }
        public string? RealName { set; get; }
        public string? Depart { set; get; }
        public bool? IsConfirm { set; get; }
        public int? Role { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Message_Body
    {
        public int id { set; get; }
        public string? MsgType { set; get; }
        public int? MsgLevel { set; get; }
        public string? MsgContent { set; get; }
        public string? Sender { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
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
