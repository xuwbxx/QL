using DataFactory.Factory;
using Microsoft.EntityFrameworkCore;

namespace DataFactory.KingBase.CloudWind
{
    /// <summary>
    /// 云平台数据库
    /// </summary>
    public class CloudWindDbContext : BaseDbContext
    {
        public CloudWindDbContext(string connectionString, DatabaseType databaseType)
            : base(connectionString, databaseType) { }

        // 仅定义CloudWind数据库的表（实体类带CloudWind_前缀）
        public DbSet<WindDbUsers> Users { get; set; } // 映射到CloudWind的Users表
        public DbSet<Base_LoginRecord> Base_LoginRecord { get; set; } // 映射到CloudWind的Users表
        public DbSet<Manage_LoginRecord> Manage_LoginRecord { get; set; } // 映射到CloudWind的Manage_LoginRecord表
        public DbSet<Wind_ProjectContacter> Wind_ProjectContacter { set; get; }
        public DbSet<Manage_Admin> Manage_Admin { set; get; }
        public DbSet<Wind_Project> Wind_Project { set; get; }
        public DbSet<Manage_Platform> Manage_Platform { set; get; }
        public DbSet<Manage_Company> Manage_Company { set; get; }
        public DbSet<Manage_CompanyRole> Manage_CompanyRole { set; get; }
        public DbSet<Manage_Viewer> Manage_Viewer { set; get; }
        public DbSet<Wind_ProjectRole> Wind_ProjectRole { set; get; }
        public DbSet<Flow_ProjectApply> Flow_ProjectApply { set; get; }
        public DbSet<Manage_Copyer> Manage_Copyer { set; get; }
        public DbSet<Wind_TaskFileDeliver> Wind_TaskFileDeliver { set; get; }
        public DbSet<View_Wind_ProjectFlow> View_Wind_ProjectFlow { set; get; }

        public DbSet<Wind_ProjectInfo> Wind_ProjectInfo { set; get; }
        public DbSet<Flow_Node> Flow_Node { set; get; }
        public DbSet<Wind_ProjectArea> Wind_ProjectArea { set; get; }
        public DbSet<Wind_ProjectFan> Wind_ProjectFan { set; get; }
        public DbSet<Wind_ProjectFile> Wind_ProjectFile { set; get; }

        public DbSet<Library_Geology> Library_Geology { set; get; }
        public DbSet<Library_Geology_DK> Library_Geology_DK { set; get; }
        public DbSet<Library_Geology_Data> Library_Geology_Data { set; get; }
        public DbSet<Library_Ship> Library_Ship { set; get; }
        public DbSet<Library_Ship_Data> Library_Ship_Data { set; get; }

        public DbSet<Wind_Task> Wind_Task { set; get; }
        public DbSet<Wind_Project_Copyer> Wind_Project_Copyer { set; get; }
        public DbSet<View_Manage_Copyer> View_Manage_Copyer { set; get; }

        public DbSet<Manage_Software> Manage_Software { set; get; }

        public DbSet<View_NodeManageUser> View_NodeManageUser { set; get; }

        public DbSet<Flow_Task_ShipFile> Flow_Task_ShipFile { set; get; }
        public DbSet<Flow_Task_DKFile> Flow_Task_DKFile { set; get; }
        public DbSet<Flow_Task_CommentFile> Flow_Task_CommentFile { set; get; }
        public DbSet<Library_Pile> Library_Pile { set; get; }
        public DbSet<Library_Pile_Data> Library_Pile_Data { set; get; }
        public DbSet<Wind_TaskFile> Wind_TaskFile { set; get; }
        public DbSet<Wind_TaskInfoImg_ZJCZ> Wind_TaskInfoImg_ZJCZ { set; get; }
        public DbSet<Wind_TaskReport> Wind_TaskReport { set; get; }

        public DbSet<Flow_NodeManageUser> Flow_NodeManageUser { set; get; }
        
        public DbSet<View_Wind_ProjectContacter> View_Wind_ProjectContacter { set; get; }

        public DbSet<View_Wind_ProjectRole> View_Wind_ProjectRole { set; get; }

        public DbSet<Manage_Role> Manage_Role { set; get; }

        public DbSet<Wind_TaskInfo_KZY> Wind_TaskInfo_KZY { set; get; }
        public DbSet<View_Wind_ProjectTask> View_Wind_ProjectTask { set; get; }
    }

    public class WindDbUsers
    {
        public int id { set; get; }
        public string? name { set; get; }
        public string? sex { set; get; }
    }

    public class Manage_LoginRecord
    {
        public int ID { set; get; }
        public string? UserCode { set; get; }
        public string? Name { set; get; }
        public string? Depart { set; get; }
        public string? LoginType { set; get; }
        public DateTime? LoginTime { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }

    }

    public class Base_LoginRecord
    {
        public int ID { set; get; }
        public string? UserName { set; get; }
        public string? UserCode { set; get; }
        public string? Name { set; get; }
        public DateTime? LoginTime { set; get; }
        public bool LoginResult { set; get; }
        public int? SoftwareID { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }

    }

    public class Manage_Admin
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

    public class Wind_ProjectContacter
    {
        public int ID { set; get; }
        public int ProjectID { set; get; }
        public string? Applyer { set; get; }
        public string? ApplyerCode { set; get; }
        public string? ApplyerDepart { set; get; }
        public string? ApplyerPhone { set; get; }
        public string? ApplyerJobName { set; get; }
        public string? Director { set; get; }
        public string? DirectorCode { set; get; }
        public string? DirectorDepart { set; get; }
        public string? DirectorPhone { set; get; }
        public string? DirectorJobName { set; get; }
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

    public class Manage_CompanyRole
    {
        public int ID { set; get; }
        public int? CompanyID { set; get; }
        public int? RoleID { set; get; }
        public string? UserName { set; get; }
        public string? UserCode { set; get; }
        public string? UserDepartName { set; get; }
        public string? UserPhone { set; get; }
        public string? UserJobName { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }

    }

    public class Wind_Project
    {
        public int ID { set; get; }
        public string? ProjectCode { set; get; }
        public int? ProjectCodeIndex { set; get; }
        public string? ProjectName { set; get; }
        public int? CompanyID { set; get; }
        public string? Lon { set; get; }
        public string? Lat { set; get; }
        public int? Status { set; get; }
        public int? FlowStatus { set; get; }
        public DateTime? ProjectStartTime { set; get; }
        public DateTime? ProjectEndTime { set; get; }
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

    public class Flow_ProjectApply
    {
        public int ID { set; get; }
        public int? ProjectID { set; get; }
        public int? NodeID { set; get; }
        public int? FlowOrder { set; get; }
        public string? NodeUserName { set; get; }
        public string? NodeUserCode { set; get; }
        public string? NodeUserDepart { set; get; }
        public string? NodeUserPhone { set; get; }
        public string? NodeUserJobName { set; get; }
        public int? FlowHandle { set; get; }
        public DateTime? ApprovalTime { set; get; }
        public string? Comment { set; get; }
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

    public class View_Wind_ProjectFlow
    {
        public int ID { set; get; }
        public int? ProjectID { set; get; }
        public int? NodeID { set; get; }
        public int? FlowOrder { set; get; }
        public string? NodeUserName { set; get; }
        public string? NodeUserCode { set; get; }
        public string? NodeUserDepart { set; get; }
        public string? NodeUserPhone { set; get; }
        public string? NodeUserJobName { set; get; }
        public int? FlowHandle { set; get; }
        public DateTime? ApprovalTime { set; get; }
        public string? Comment { set; get; }
        public DateTime CreateTime { set; get; }
        public string? ProjectCode { set; get; }
        public string? ProjectName { set; get; }
        public int FlowStatus { set; get; }
        public DateTime? ProjectStartTime { set; get; }
        public DateTime? ProjectEndTime { set; get; }
        public string? Applyer { set; get; }
        public string? ApplyerCode { set; get; }
        public string? ApplyerDepart { set; get; }
        public string? ApplyerPhone { set; get; }
        public string? ApplyerJobName { set; get; }
        public string? Director { set; get; }
        public string? DirectorCode { set; get; }
        public string? DirectorDepart { set; get; }
        public string? DirectorPhone { set; get; }
        public string? DirectorJobName { set; get; }
        public string? NodeName { set; get; }
        public int? RoleID { set; get; }
        public string? roleusercode { set; get; }
        public string? copyerusercode { set; get; }
        public int? projectstatus { set; get; }
        public bool? isroleexist { set; get; }
        public bool? iscopyexist { set; get; }
    }

    public class Manage_Platform
    {
        public int ID { set; get; }
        public string? Platform { set; get; }

        public string? Url { set; get; }
        public int? DepartID { set; get; }
        public string? Introduction { set; get; }

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

    public class Flow_Node
    {
        public int ID { set; get; }
        public int SoftwareID { set; get; }
        public int NodeNo { set; get; }
        public bool? NodeApprovalType { set; get; }
        public bool? DoEdit { set; get; }
        public string? NodeName { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Flow_NodeManageUser
    {
        public int ID { set; get; }
        public int NodeID { set; get; }
        public string? ManageName { set; get; }
        public string? ManageUserCode { set; get; }
        public string? ManageDepart { set; get; }
        public string? ManagePhone { set; get; }
        public string? ManageJobName { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class View_NodeManageUser
    {
        public int ID { set; get; }
        public int? NodeID { set; get; }

        public string? ManageName { set; get; }
        public string? ManageUserCode { set; get; }
        public string? ManageDepart { set; get; }
        public string? ManagePhone { set; get; }
        public string? ManageJobName { set; get; }

        public DateTime? CreateTime { set; get; }
        public string? NodeName { set; get; }
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

    public class Wind_ProjectFile
    {
        public int ID { set; get; }
        public int ProjectID { set; get; }
        public string? FileName { set; get; }
        public string? FilePath { set; get; }
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

    public class Library_Geology_Data
    {
        public int ID { set; get; }
        public int? DKID { set; get; }

        //序号
        public string? xh { set; get; }

        //地层编号
        public string? dcbh { set; get; }

        //土层名称
        public string? tcbh { set; get; }

        //[层底标高(m)]
        public string? cdbg { set; get; }

        //土层类型
        public string? tclx { set; get; }

        //[不排水抗剪强度cu(kPa)]
        public string? bpskjqd { set; get; }

        //[砂土摩擦角°]
        public string? stmcj { set; get; }

        //有效重度
        public string? yxzd { set; get; }

        //标贯击数
        public string? bgjs { set; get; }
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

    public class Manage_Software
    {
        public int ID { set; get; }
        public string? SoftwareName { set; get; }
        public string? Comment { set; get; }
        public int? DepartID { set; get; }
        public int? FlowType { set; get; }
        public string? SoftwareUrl { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool IsDelete { set; get; }
    }

    public class Library_Ship
    {
        public int ID { set; get; }
        public int? TaskID { set; get; }
        public string? ShipName { set; get; }
        public string? FileName { set; get; }
        public string? FilePath { set; get; }
        public bool? IsConfirm { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool? IsDelete { set; get; }
    }

    public class Library_Ship_Data
    {
        public int ID { set; get; }
        public int? ShipID { set; get; }
        //桩腿直径
        public string? ztzj { set; get; }
        public string? ztzj_unit { set; get; }
        //桩腿周长
        public string? ztzc { set; get; }
        public string? ztzc_unit { set; get; }
        //桩靴长度
        public string? zxcdL { set; get; }
        public string? zxcdL_unit { set; get; }
        //桩靴宽度B
        public string? zxkdB { set; get; }
        public string? zxkdB_unit { set; get; }
        //桩靴高度H
        public string? zxgdH { set; get; }
        public string? zxgdH_unit { set; get; }
        //桩靴面积A
        public string? zxmjA { set; get; }
        public string? zxmjA_unit { set; get; }
        //桩靴最大截面周长
        public string? zxzdjmzc { set; get; }
        public string? zxzdjmzc_unit { set; get; }
        //桩靴体积V
        public string? zxtjV { set; get; }
        public string? zxtjV_unit { set; get; }
        //桩腿、桩靴自重W
        public string? ztzxzzW { set; get; }
        public string? ztzxzzW_unit { set; get; }
        //桩腿预压力
        public string? ztyyl { set; get; }
        public string? ztyyl_unit { set; get; }
        //计算预压荷载
        public string? jsyyhz { set; get; }
        public string? jsyyhz_unit { set; get; }
        //拔桩力
        public string? bzl { set; get; }
        public string? bzl_unit { set; get; }
        //对地比压
        public string? ddby { set; get; }
        public string? ddby_unit { set; get; }
        //有效桩腿长度（船底到靴底）
        public string? yxztcd_cddxd { set; get; }
        public string? yxztcd_cddxd_unit { set; get; }
        //气隙（船底到水面）
        public string? qx_cddsm { set; get; }
        public string? qx_cddsm_unit { set; get; }
        //桩腿有效长度
        public string? ztyxcd { set; get; }
        public string? ztyxcd_unit { set; get; }
        //桩腿截面积
        public string? ztjmj { set; get; }
        public string? ztjmj_unit { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool? IsDelete { set; get; }
    }

    public class Library_Ship_KZY
    {
        public int ID { set; get; }
        public string? ShipName { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool? IsDelete { set; get; }
    }

    public class View_Library_Ship
    {
        public int ID { set; get; }
        public string? ShipName { set; get; }
        public string? FileName { set; get; }
        public string? FilePath { set; get; }
        //桩腿截面积
        public string? ztjmj { set; get; }
        public string? ztjmj_unit { set; get; }
        //桩腿周长
        public string? ztzc { set; get; }
        public string? ztzc_unit { set; get; }
        //桩靴长度
        public string? zxcdL { set; get; }
        public string? zxcdL_unit { set; get; }
        //桩靴宽度B
        public string? zxkdB { set; get; }
        public string? zxkdB_unit { set; get; }
        //桩靴高度H
        public string? zxgdH { set; get; }
        public string? zxgdH_unit { set; get; }
        //桩靴面积A
        public string? zxmjA { set; get; }
        public string? zxmjA_unit { set; get; }
        //桩靴最大截面周长
        public string? zxzdjmzc { set; get; }
        public string? zxzdjmzc_unit { set; get; }
        //桩靴体积V
        public string? zxtjV { set; get; }
        public string? zxtjV_unit { set; get; }
        //桩腿、桩靴自重W
        public string? ztzxzzW { set; get; }
        public string? ztzxzzW_unit { set; get; }
        //桩腿预压力
        public string? ztyyl { set; get; }
        public string? ztyyl_unit { set; get; }
        //计算预压荷载
        public string? jsyyhz { set; get; }
        public string? jsyyhz_unit { set; get; }
        //拔桩力
        public string? bzl { set; get; }
        public string? bzl_unit { set; get; }
        //对地比压
        public string? ddby { set; get; }
        public string? ddby_unit { set; get; }
        //有效桩腿长度（船底到靴底）
        public string? yxztcd_cddxd { set; get; }
        public string? yxztcd_cddxd_unit { set; get; }
        //气隙（船底到水面）
        public string? qx_cddsm { set; get; }
        public string? qx_cddsm_unit { set; get; }
        //桩腿有效长度
        public string? ztyxcd { set; get; }
        public string? ztyxcd_unit { set; get; }
        public bool? IsConfirm { set; get; }
        //桩腿直径
        public string? ztzj { set; get; }
        public string? ztzj_unit { set; get; }
    }

    public class View_Manage_Copyer
    {
        public int ID { set; get; }
        public int? SoftwareID { set; get; }
        public string? UserName { set; get; }
        public string? UserCode { set; get; }
        public string? UserDepart { set; get; }
        public string? UserPhone { set; get; }
        public string? UserJobName { set; get; }
        public string? SoftwareName { set; get; }
    }

    public class Flow_Task_ShipFile
    {
        public int id { set; get; }
        public int? TaskID { set; get; }
        public string? FileName { set; get; }
        public string? FilePath { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool? IsDelete { set; get; }
    }

    public class Library_Pile
    {
        public int id { set; get; }
        public int? TaskID { set; get; }
        public int? Type { set; get; }
        public string? FileName { set; get; }
        public string? FilePath { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool? IsDelete { set; get; }
    }

    public class Library_Pile_Data
    {
        public int id { set; get; }
        public int? PileID { set; get; }
        public string? PileName { set; get; }
        /// <summary>
        /// 每段长度
        /// </summary>
        public string? mdcd { set; get; }
        /// <summary>
        /// 总长
        /// </summary>
        public string? zc { set; get; }
        /// <summary>
        /// [辅助桩外径（m）]
        /// </summary>
        public string? fzzwj { set; get; }
        /// <summary>
        /// [壁厚（m）]
        /// </summary>
        public string? bh { set; get; }
        /// <summary>
        /// [总重量（吨）]
        /// </summary>
        public string? zzl { set; get; }
        /// <summary>
        /// [桩顶高程（m）]
        /// </summary>
        public string? zdgcT { set; get; }
        /// <summary>
        /// [桩长（m）]
        /// </summary>
        public string? zcm { set; get; }
        /// <summary>
        /// [桩底高程（m）]
        /// </summary>
        public string? zdgcB { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool? IsDelete { set; get; }
    }

    public class Wind_TaskFile
    {
        public int id { set; get; }
        public int? TaskID { set; get; }
        public int? TypeID { set; get; }
        public string? FileName { set; get; }
        public string? FilePath { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool? IsDelete { set; get; }
    }

    public class Wind_TaskInfoImg_ZJCZ
    {
        public int id { set; get; }
        public int? TaskID { set; get; }
        public string? FileName { set; get; }
        public string? FilePath { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool? IsDelete { set; get; }
    }

    public class Wind_TaskReport
    {
        public int id { set; get; }
        public int? TaskID { set; get; }
        public int? Type { set; get; }
        public string? ReportName { set; get; }
        public string? ReportPath { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool? IsDelete { set; get; }
    }

    public class Flow_Task_DKFile
    {
        public int id { set; get; }
        public int? TaskID { set; get; }
        public string? FileName { set; get; }
        public string? FilePath { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool? IsDelete { set; get; }
    }

    public class Flow_Task_CommentFile
    {
        public int id { set; get; }
        public int? TaskID { set; get; }
        public int? FlowID { set; get; }
        public string? UserName { set; get; }
        public string? FileName { set; get; }
        public string? FilePath { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool? IsDelete { set; get; }
    }

    public class View_Wind_ProjectContacter
    {
        public int id { set; get; }
        public string? ProjectCode { set; get; }
        public string? ProjectName { set; get; }
        public int? Status { set; get; }
        public int? FlowStatus { set; get; }
        public DateTime? ProjectStartTime { set; get; }
        public DateTime? ProjectEndTime { set; get; }
        public string? DirectorCode { set; get; }
        public DateTime? CreateTime { set; get; }
        public string? Director { set; get; }
    }

    public class Manage_Role
    {
        public int id { set; get; }
        public string? RoleName { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool? IsDelete { set; get; }
    }

    public class View_Wind_ProjectRole
    {
        public int id { set; get; }
        public int? ProjectID { set; get; }
        public int? RoleID { set; get; }
        public string? ProjectCode { set; get; }
        public string? ProjectName { set; get; }
        public string? RoleName { set; get; }
        
        public string? UserName { set; get; }
        public string? UserCode { set; get; }
        public string? UserDepartName { set; get; }
        public string? UserPhone { set; get; }
        public string? UserJobName { set; get; }
    }

    public class View_Wind_ProjectTask
    {
        public int id { set; get; }
        public int? ProjectID { set; get; }
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
        public string? ProjectCode { set; get; }
        public string? ProjectName { set; get; }
        public int? Status { set; get; }
    }

    public class Wind_TaskInfo_KZY
    {
        public int id { set; get; }
        public int? TaskID { set; get; }
        public string? ShipIDs { set; get; }
        public int? TechType { set; get; }
        public DateTime? ForecastStartTime { set; get; }
        public DateTime? ForecastEndTime { set; get; }
        public string? YFYNo { set; get; }
        public string? Balance { set; get; }
        public string? Lon { set; get; }
        public string? Lat { set; get; }
        public DateTime? CreateTime { set; get; }
        public bool? IsDelete { set; get; }
    }

}
