using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFactory.KingBase.Towing
{
    public class TowingDbContext : BaseDbContext
    {
        public TowingDbContext(string connectionString, DatabaseType databaseType)
            : base(connectionString, databaseType) { }

        public DbSet<Data_Backup> Data_Backup { set; get; }

        /// <summary>
        /// 数据备份（MRU）表
        /// </summary>
        public DbSet<Data_Backup_Mru> Data_Backup_Mru { set; get; }

        /// <summary>
        /// 气象数据备份表
        /// </summary>
        public DbSet<Data_Backup_Weather> Data_Backup_Weather { set; get; }

        /// <summary>
        /// 电池数据表
        /// </summary>
        public DbSet<Data_Battery> Data_Battery { set; get; }

        /// <summary>
        /// 设备数据表
        /// </summary>
        public DbSet<Data_Device> Data_Device { set; get; }

        /// <summary>
        /// 陀螺仪数据表
        /// </summary>
        public DbSet<Data_EGYRO> Data_EGYRO { set; get; }

        /// <summary>
        /// 文件数据表
        /// </summary>
        public DbSet<Data_File> Data_File { set; get; }

        /// <summary>
        /// 预报数据表
        /// </summary>
        public DbSet<Data_Forecast> Data_Forecast { set; get; }

        /// <summary>
        /// 海康本地图片表
        /// </summary>
        public DbSet<Data_HikLocalPic> Data_HikLocalPic { set; get; }

        /// <summary>
        /// MRU传感器数据表
        /// </summary>
        public DbSet<Data_MRU> Data_MRU { set; get; }

        /// <summary>
        /// 消息数据表
        /// </summary>
        public DbSet<Data_Message> Data_Message { set; get; }

        /// <summary>
        /// 卫星定位数据表
        /// </summary>
        public DbSet<Data_SATELLITE> Data_SATELLITE { set; get; }

        /// <summary>
        /// 航速数据表
        /// </summary>
        public DbSet<Data_SailSpeed> Data_SailSpeed { set; get; }

        /// <summary>
        /// 台风数据表
        /// </summary>
        public DbSet<Data_Typhoon> Data_Typhoon { set; get; }

        /// <summary>
        /// 风数据表
        /// </summary>
        public DbSet<Data_Wind> Data_Wind { set; get; }

        /// <summary>
        /// 报警配置表
        /// </summary>
        public DbSet<Manage_Alarm> Manage_Alarm { set; get; }

        /// <summary>
        /// 区域配置表
        /// </summary>
        public DbSet<Manage_Area> Manage_Area { set; get; }

        /// <summary>
        /// 基础信息表
        /// </summary>
        public DbSet<Manage_Basic> Manage_Basic { set; get; }

        /// <summary>
        /// 登录日志表
        /// </summary>
        public DbSet<Manage_LoginLog> Manage_LoginLog { set; get; }

        /// <summary>
        /// 计划航线表
        /// </summary>
        public DbSet<Manage_PlanRoute> Manage_PlanRoute { set; get; }

        /// <summary>
        /// 项目表
        /// </summary>
        public DbSet<Manage_Project> Manage_Project { set; get; }

        /// <summary>
        /// 船舶表
        /// </summary>
        public DbSet<Manage_Ship> Manage_Ship { set; get; }

        /// <summary>
        /// 船舶锚地表
        /// </summary>
        public DbSet<Manage_ShipAnchorage> Manage_ShipAnchorage { set; get; }

        /// <summary>
        /// 用户表
        /// </summary>
        public DbSet<Manage_User> Manage_User { set; get; }

        /// <summary>
        /// 消息内容表
        /// </summary>
        public DbSet<Message_Body> Message_Body { set; get; }

    }

    public class Data_Backup
    {
        public int id { set; get; }
        public string? DataString { set; get; }
        public DateTime? CreateTime { set; get; }
    }

    /// <summary>
    /// 数据备份（MRU）实体，映射 public.Data_Backup_Mru 表
    /// </summary>
    [Table("Data_Backup_Mru")]
    public class Data_Backup_Mru
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 备份数据内容（文本）
        /// </summary>
        public string? DataString { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }
    }

    /// <summary>
    /// 气象数据备份实体，映射 public.Data_Backup_Weather 表
    /// </summary>
    [Table("Data_Backup_Weather")]
    public class Data_Backup_Weather
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 站点编号
        /// </summary>
        public string? bm500 { set; get; }

        /// <summary>
        /// 湿度
        /// </summary>
        public string? humidity { set; get; }

        /// <summary>
        /// 气压
        /// </summary>
        public string? pressure { set; get; }

        /// <summary>
        /// 涌向（角度）
        /// </summary>
        public string? swelldir { set; get; }

        /// <summary>
        /// 涌高
        /// </summary>
        public string? swellheight { set; get; }

        /// <summary>
        /// 涌周期
        /// </summary>
        public string? swellperiod { set; get; }

        /// <summary>
        /// 温度
        /// </summary>
        public string? temperature { set; get; }

        /// <summary>
        /// 能见度
        /// </summary>
        public string? visibility { set; get; }

        /// <summary>
        /// 浪高
        /// </summary>
        public string? waveheight { set; get; }

        /// <summary>
        /// 风向（角度）
        /// </summary>
        public string? winddir { set; get; }

        /// <summary>
        /// 风向（文字描述）
        /// </summary>
        public string? winddirStr { set; get; }

        /// <summary>
        /// 风速
        /// </summary>
        public string? windspeed { set; get; }

        /// <summary>
        /// 风力等级
        /// </summary>
        public string? windlevel { set; get; }

        /// <summary>
        /// 风力等级（文字描述）
        /// </summary>
        public string? windlevelStr { set; get; }

        /// <summary>
        /// 风预警等级
        /// </summary>
        public string? windwarninglevel { set; get; }

        /// <summary>
        /// 风报警
        /// </summary>
        public bool? windalarm { set; get; }

        /// <summary>
        /// 观测时间
        /// </summary>
        public string? time { set; get; }

        /// <summary>
        /// 观测时间（字符串格式）
        /// </summary>
        public string? timeStr { set; get; }

        /// <summary>
        /// 流向（角度）
        /// </summary>
        public string? oceandir { set; get; }

        /// <summary>
        /// 流向（文字描述）
        /// </summary>
        public string? oceandirStr { set; get; }

        /// <summary>
        /// 流速
        /// </summary>
        public string? oceanspeed { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }
    }

    /// <summary>
    /// 电池数据实体，映射 public.Data_Battery 表
    /// </summary>
    [Table("Data_Battery")]
    public class Data_Battery
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public int? ProjectID { set; get; }

        /// <summary>
        /// 剩余电量1
        /// </summary>
        public double? RemainEle { set; get; }

        /// <summary>
        /// 剩余电量2
        /// </summary>
        public double? RemainEle2 { set; get; }

        /// <summary>
        /// 发动机1状态
        /// </summary>
        public double? Engine1 { set; get; }

        /// <summary>
        /// 发动机2状态
        /// </summary>
        public double? Engine2 { set; get; }

        /// <summary>
        /// 电压
        /// </summary>
        public double? Voltage { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 创建时间2
        /// </summary>
        public DateTime? CreateTime2 { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 设备数据实体，映射 public.Data_Device 表
    /// </summary>
    [Table("Data_Device")]
    public class Data_Device
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public int? ProjectID { set; get; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string? Type { set; get; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string? DeviceName { set; get; }

        /// <summary>
        /// 最近更新时间
        /// </summary>
        public DateTime? LatestUpdateTime { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 陀螺仪数据实体，映射 public.Data_EGYRO 表
    /// </summary>
    [Table("Data_EGYRO")]
    public class Data_EGYRO
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public int? DeviceID { set; get; }

        /// <summary>
        /// X轴角度
        /// </summary>
        public double? AngleX { set; get; }

        /// <summary>
        /// Y轴角度
        /// </summary>
        public double? AngleY { set; get; }

        /// <summary>
        /// 艏向角
        /// </summary>
        public double? AzAngle { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 创建时间2
        /// </summary>
        public DateTime? CreateTime2 { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 文件数据实体，映射 public.Data_File 表
    /// </summary>
    [Table("Data_File")]
    public class Data_File
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string? FileName { set; get; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string? FilePath { set; get; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public int? Type { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 预报数据实体，映射 public.Data_Forecast 表
    /// </summary>
    [Table("Data_Forecast")]
    public class Data_Forecast
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 预报编号
        /// </summary>
        public int? No { set; get; }

        /// <summary>
        /// 预报时间
        /// </summary>
        public DateTime? ForecastTime { set; get; }

        /// <summary>
        /// 纬度
        /// </summary>
        public double? Lat { set; get; }

        /// <summary>
        /// 经度
        /// </summary>
        public double? Lon { set; get; }

        /// <summary>
        /// 最大摇X
        /// </summary>
        public double? MaxYaoX { set; get; }

        /// <summary>
        /// 最大摇Y
        /// </summary>
        public double? MaxYaoY { set; get; }

        /// <summary>
        /// 最大荡Z
        /// </summary>
        public double? MaxDangZ { set; get; }

        /// <summary>
        /// 总报警数
        /// </summary>
        public int? TotalAlarm { set; get; }

        /// <summary>
        /// 风报警
        /// </summary>
        public int? WindAlarm { set; get; }

        /// <summary>
        /// 浪报警
        /// </summary>
        public int? WaveAlarm { set; get; }

        /// <summary>
        /// 摇晃报警
        /// </summary>
        public int? ShakeAlarm { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 海康本地图片实体，映射 public.Data_HikLocalPic 表
    /// </summary>
    [Table("Data_HikLocalPic")]
    public class Data_HikLocalPic
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public int? ProjID { set; get; }

        /// <summary>
        /// 抓拍时间
        /// </summary>
        public DateTime? SnapTime { set; get; }

        /// <summary>
        /// 抓拍图片URL
        /// </summary>
        public string? SnapPicUrl { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// MRU传感器数据实体，映射 public.Data_MRU 表
    /// </summary>
    [Table("Data_MRU")]
    public class Data_MRU
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public int? DeviceID { set; get; }

        /// <summary>
        /// 荡X
        /// </summary>
        public double? DangX { set; get; }

        /// <summary>
        /// 荡Y
        /// </summary>
        public double? DangY { set; get; }

        /// <summary>
        /// 荡Z
        /// </summary>
        public double? DangZ { set; get; }

        /// <summary>
        /// 摇X
        /// </summary>
        public double? YaoX { set; get; }

        /// <summary>
        /// 摇Y
        /// </summary>
        public double? YaoY { set; get; }

        /// <summary>
        /// 摇Z
        /// </summary>
        public double? YaoZ { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 创建时间2
        /// </summary>
        public DateTime? CreateTime2 { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 消息数据实体，映射 public.Data_Message 表
    /// </summary>
    [Table("Data_Message")]
    public class Data_Message
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string? Message { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 卫星定位数据实体，映射 public.Data_SATELLITE 表
    /// </summary>
    [Table("Data_SATELLITE")]
    public class Data_SATELLITE
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public int? DeviceID { set; get; }

        /// <summary>
        /// 经度
        /// </summary>
        public double? Lon { set; get; }

        /// <summary>
        /// 经度方向（E/W）
        /// </summary>
        public string? LonDirection { set; get; }

        /// <summary>
        /// 纬度
        /// </summary>
        public double? Lat { set; get; }

        /// <summary>
        /// 纬度方向（N/S）
        /// </summary>
        public string? LatDirection { set; get; }

        /// <summary>
        /// 海拔高度
        /// </summary>
        public double? Altitude { set; get; }

        /// <summary>
        /// 艏向
        /// </summary>
        public double? Heading { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 创建时间2
        /// </summary>
        public DateTime? CreateTime2 { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 航速数据实体，映射 public.Data_SailSpeed 表
    /// </summary>
    [Table("Data_SailSpeed")]
    public class Data_SailSpeed
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public int? ProjectID { set; get; }

        /// <summary>
        /// 航速
        /// </summary>
        public double? SailSpeed { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 创建时间2
        /// </summary>
        public DateTime? CreateTime2 { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 台风数据实体，映射 public.Data_Typhoon 表
    /// </summary>
    [Table("Data_Typhoon")]
    public class Data_Typhoon
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 中国编号
        /// </summary>
        public string? chnCode { set; get; }

        /// <summary>
        /// 中国名称
        /// </summary>
        public string? chnName { set; get; }

        /// <summary>
        /// 当前年份
        /// </summary>
        public int? currentyear { set; get; }

        /// <summary>
        /// 序号
        /// </summary>
        public string? xuHao { set; get; }

        /// <summary>
        /// 时间（字符串格式）
        /// </summary>
        public string? time { set; get; }

        /// <summary>
        /// 日期时间
        /// </summary>
        public DateTime? dateTime { set; get; }

        /// <summary>
        /// 预报信息
        /// </summary>
        public string? forecast { set; get; }

        /// <summary>
        /// 预报时效（小时）
        /// </summary>
        public double? fhour { set; get; }

        /// <summary>
        /// 纬度
        /// </summary>
        public double? lat { set; get; }

        /// <summary>
        /// 经度
        /// </summary>
        public double? lon { set; get; }

        /// <summary>
        /// 台风等级
        /// </summary>
        public double? grade { set; get; }

        /// <summary>
        /// 最大风速
        /// </summary>
        public double? mspeed { set; get; }

        /// <summary>
        /// 气压
        /// </summary>
        public double? pressure { set; get; }

        /// <summary>
        /// 移动速度
        /// </summary>
        public double? kspeed { set; get; }

        /// <summary>
        /// 移动方向
        /// </summary>
        public string? direction { set; get; }

        /// <summary>
        /// 7级风圈半径
        /// </summary>
        public double? radius7 { set; get; }

        /// <summary>
        /// 10级风圈半径
        /// </summary>
        public double? radius10 { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 风数据实体，映射 public.Data_Wind 表
    /// </summary>
    [Table("Data_Wind")]
    public class Data_Wind
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public int? DeviceID { set; get; }

        /// <summary>
        /// 风速
        /// </summary>
        public double? WindSpeed { set; get; }

        /// <summary>
        /// 风向
        /// </summary>
        public double? WindDirection { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 创建时间2
        /// </summary>
        public DateTime? CreateTime2 { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 报警配置实体，映射 public.Manage_Alarm 表
    /// </summary>
    [Table("Manage_Alarm")]
    public class Manage_Alarm
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 报警类型
        /// </summary>
        public string? Type { set; get; }

        /// <summary>
        /// 报警值1
        /// </summary>
        public double? AlarmValue1 { set; get; }

        /// <summary>
        /// 报警值2
        /// </summary>
        public double? AlarmValue2 { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 区域配置实体，映射 public.Manage_Area 表
    /// </summary>
    [Table("Manage_Area")]
    public class Manage_Area
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public int? ProjectID { set; get; }

        /// <summary>
        /// 经度
        /// </summary>
        public double? Lon { set; get; }

        /// <summary>
        /// 纬度
        /// </summary>
        public double? Lat { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 基础信息实体，映射 public.Manage_Basic 表
    /// </summary>
    [Table("Manage_Basic")]
    public class Manage_Basic
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public int? ProjectID { set; get; }

        /// <summary>
        /// 项目开始时间
        /// </summary>
        public DateTime? ProjectStartTime { set; get; }

        /// <summary>
        /// 起点经度
        /// </summary>
        public double? StartLon { set; get; }

        /// <summary>
        /// 起点纬度
        /// </summary>
        public double? StartLat { set; get; }

        /// <summary>
        /// 终点经度
        /// </summary>
        public double? EndLon { set; get; }

        /// <summary>
        /// 终点纬度
        /// </summary>
        public double? EndLat { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 登录日志实体，映射 public.Manage_LoginLog 表
    /// </summary>
    [Table("Manage_LoginLog")]
    public class Manage_LoginLog
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 用户编号
        /// </summary>
        public string? UserCode { set; get; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string? RealName { set; get; }

        /// <summary>
        /// 部门
        /// </summary>
        public string? Depart { set; get; }

        /// <summary>
        /// 岗位
        /// </summary>
        public string? Job { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 计划航线实体，映射 public.Manage_PlanRoute 表
    /// </summary>
    [Table("Manage_PlanRoute")]
    public class Manage_PlanRoute
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public int? ProjectID { set; get; }

        /// <summary>
        /// 经度
        /// </summary>
        public double? Lon { set; get; }

        /// <summary>
        /// 纬度
        /// </summary>
        public double? Lat { set; get; }

        /// <summary>
        /// 航线时间
        /// </summary>
        public DateTime? RouteTime { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 项目实体，映射 public.Manage_Project 表
    /// </summary>
    [Table("Manage_Project")]
    public class Manage_Project
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 云平台项目ID
        /// </summary>
        public int? CloudProjectID { set; get; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string? ProjectName { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 船舶实体，映射 public.Manage_Ship 表
    /// </summary>
    [Table("Manage_Ship")]
    public class Manage_Ship
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// MMSI编号
        /// </summary>
        public string? mmsi { set; get; }

        /// <summary>
        /// 船舶名称
        /// </summary>
        public string? ShipName { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 船舶锚地实体，映射 public.Manage_ShipAnchorage 表
    /// </summary>
    [Table("Manage_ShipAnchorage")]
    public class Manage_ShipAnchorage
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 锚地类型
        /// </summary>
        public string? Type { set; get; }

        /// <summary>
        /// 分组
        /// </summary>
        public string? Group { set; get; }

        /// <summary>
        /// 锚地名称
        /// </summary>
        public string? Name { set; get; }

        /// <summary>
        /// 经度
        /// </summary>
        public double? Lon { set; get; }

        /// <summary>
        /// 纬度
        /// </summary>
        public double? Lat { set; get; }

        /// <summary>
        /// 半径
        /// </summary>
        public double? Radius { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 用户实体，映射 public.Manage_User 表
    /// </summary>
    [Table("Manage_User")]
    public class Manage_User
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { set; get; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string? RealName { set; get; }

        /// <summary>
        /// 部门
        /// </summary>
        public string? Depart { set; get; }

        /// <summary>
        /// 是否已确认
        /// </summary>
        public bool? IsConfirm { set; get; }

        /// <summary>
        /// 角色
        /// </summary>
        public int? Role { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }

    /// <summary>
    /// 消息内容实体，映射 public.Message_Body 表
    /// </summary>
    [Table("Message_Body")]
    public class Message_Body
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public string? MsgType { set; get; }

        /// <summary>
        /// 消息级别
        /// </summary>
        public int? MsgLevel { set; get; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string? MsgContent { set; get; }

        /// <summary>
        /// 发送者
        /// </summary>
        public string? Sender { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { set; get; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { set; get; }
    }



}
