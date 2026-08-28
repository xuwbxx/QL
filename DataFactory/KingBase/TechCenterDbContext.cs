using DataFactory.Factory;
using Microsoft.EntityFrameworkCore;
using Model.Base;
using System.ComponentModel.DataAnnotations;


namespace DataFactory.KingBase
{
    /// <summary>
    /// 技术中心集成平台
    /// </summary>
    public class TechCenterDbContext : BaseDbContext
    {
        public TechCenterDbContext(string connectionString, DatabaseType databaseType)
            : base(connectionString, databaseType) { }




        // 表映射，属性名称必须和数据库表名相同
        public DbSet<TechCenter_DataLogin> Data_Login { get; set; }

        public DbSet<TechCenter_DataLoginResult> Data_LoginResult { get; set; }


        public DbSet<TechCenter_Manage_Depart> Manage_Depart { get; set; }

        public DbSet<TechCenter_Manage_Software> Manage_Software { get; set; }

        public DbSet<TechCenter_Manage_UserSoftware> Manage_UserSoftware { get; set; }
    }

    public class TechCenter_DataLogin
    {
        public int ID { set; get; }

        public string? UserCode { set; get; }

        public string? UserName { set; get; }

        public string? Depart { set; get; }

        public DateTime CreateTime { set; get; }

        public bool IsDelete { set; get; }
    }

    public class TechCenter_Manage_Depart
    {
        public int ID { set; get; }

        public string? Name { set; get; }


        public DateTime CreateTime { set; get; }

        public bool IsDelete { set; get; }
    }

    public class TechCenter_Manage_Software
    {
        public int ID { set; get; }

        public string? Name { set; get; }


        public DateTime CreateTime { set; get; }

        public bool IsDelete { set; get; }

        public int DepartID { set; get; }

        public string? Url { set; get; }

        public string? Img { set; get; }

        public string? Info { set; get; }

        public string? Manager { set; get; }

        public string? UseTime { set; get; }

        public int Type { set; get; }
    }

    public class TechCenter_DataLoginResult
    {
        public int ID { set; get; }

        public DateTime CreateTime { set; get; }

        public bool IsDelete { set; get; }

        public string? Token { set; get; }

        public string? UserCode { set; get; }

        public string? GUID { set; get; }

        public string? UserName { set; get; }

        public int? SoftwareID { set; get; }

        public bool Result { set; get; }

    }

    public class TechCenter_Manage_User
    {

    }

    public class TechCenter_Manage_UserSoftware
    {
        public int ID { set; get; }

        public string? UserCode { set; get; }

        public string? SoftwareID { set; get; }

        public DateTime CreateTime { set; get; }

        public bool IsDelete { set; get; }
    }
    /// <summary>
    /// 项目信息
    /// </summary>
    public class Biz_Project
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "项目名称不能为空")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "项目名称长度2-50个字符")]
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// 项目状态：0=在建, 1=完工
        /// </summary>
        public string Status { get; set; } = "0";

        /// <summary>
        /// 桥梁数量
        /// </summary>
        public int BridgeCount { get; set; }

        [Required(ErrorMessage = "项目负责人不能为空")]
        public string Manager { get; set; } = string.Empty;

        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime? UpdatedTime { get; set; }
    }

    /// <summary>
    /// 项目查询参数
    /// </summary>
    public class ProjectQueryRequest : BaseRequest
    {
        public string? ProjectName { get; set; }
        public string? Status { get; set; }
    }
}
