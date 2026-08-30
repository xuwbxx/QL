using DataFactory.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataFactory.KingBase
{
    /// <summary>
    /// 云平台数据库
    /// </summary>
    public class QlPreAssembledDbContext : BaseDbContext
    {
        public QlPreAssembledDbContext(string connectionString, DatabaseType databaseType)
            : base(connectionString, databaseType) { }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // 移除这个约定，表名就不会自动变成复数了
            configurationBuilder.Conventions.Remove(typeof(TableNameFromDbSetConvention));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 默认取dbo
            modelBuilder.HasDefaultSchema("dbo");

            // 必须先调用 base，执行父上下文内部的模型配置
            base.OnModelCreating(modelBuilder);

            // 动态转换实现表名与class名/字段名与property名的映射（当数据库使用了蛇形命名规范时）
            // ApplySnakeCaseConversion(modelBuilder);

            // 你还可以在这里继续配置当前DbContext的实体
            ApplyCustomConfigurations(modelBuilder);
        }

        #region 动态转换

        /// <summary>
        /// 自动将 PascalCase 转换为 snake_case（只转换未配置的表和字段）
        /// </summary>
        private void ApplySnakeCaseConversion(ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                string? currentTableName = entityType.GetTableName();

                // 如果表名还是实体类名（即没有手动 ToTable），自动转换
                if (currentTableName == entityType.Name)
                {
                    if (!currentTableName!.Contains('_'))
                    {
                        string snakeTableName = GetSnakeCase(currentTableName!);
                        entityType.SetTableName(snakeTableName);
                    }
                }

                foreach (IMutableProperty property in entityType.GetProperties())
                {
                    string currentColumnName = property.GetColumnName();

                    if (!string.Equals(currentColumnName, property.Name, StringComparison.Ordinal)
                        || property.Name.Contains('_'))
                    {
                        continue;
                    }

                    string snakeName = GetSnakeCase(property.Name);
                    property.SetColumnName(snakeName);
                }
            }
        }

        /// <summary>
        /// PascalCase → snake_case 转换
        /// </summary>
        private static string GetSnakeCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            // 常见缩写保留
            var abbreviations = new HashSet<string> { "ID", "OID", "GUID", "URL", "URI", "HTML", "XML", "JSON" };
            if (abbreviations.Contains(name)) return name;

            return string.Concat(name.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

        #endregion

        #region 特殊配置

        /// <summary>
        /// 特殊情况手动配置（覆盖动态转换）
        /// </summary>
        private void ApplyCustomConfigurations(ModelBuilder modelBuilder)
        {
            // 如果需要特殊配置，在这里添加
            // 例如：
            // modelBuilder.Entity<sys_userinfo>(entity =>
            // {
            //     entity.HasIndex(e => e.Account).IsUnique();
            //     entity.HasIndex(e => e.EmpNo).IsUnique();
            // });
        }

        #endregion

        /// <summary>
        /// 测试用
        /// </summary>
        public DbSet<testTable> testTable { get; set; }

        #region sys tables - DbSet

        /// <summary>
        /// 部门表，存储组织结构信息
        /// </summary>
        public DbSet<sys_dept> sys_dept { get; set; }

        /// <summary>
        /// 用户表，存储系统用户信息
        /// </summary>
        public DbSet<sys_userinfo> sys_userinfo { get; set; }

        /// <summary>
        /// 角色表，存储系统角色信息
        /// </summary>
        public DbSet<sys_role> sys_role { get; set; }

        /// <summary>
        /// 用户角色关联表，实现用户与角色的多对多关系
        /// </summary>
        public DbSet<sys_user_role> sys_user_role { get; set; }

        /// <summary>
        /// 系统菜单表，存储菜单/权限配置
        /// </summary>
        public DbSet<sys_menu> sys_menu { get; set; }

        /// <summary>
        /// 角色菜单关联表，实现角色与菜单的多对多关系
        /// </summary>
        public DbSet<sys_role_menu> sys_role_menu { get; set; }

        /// <summary>
        /// 系统字典主表，定义字典分类
        /// </summary>
        public DbSet<sys_dic_data> sys_dic_data { get; set; }

        /// <summary>
        /// 字典明细表，存储字典的具体选项
        /// </summary>
        public DbSet<sys_dic_data_item> sys_dic_data_item { get; set; }

        #endregion

        #region biz tables

        public DbSet<biz_project> biz_project { get; set; }
        public DbSet<biz_project_bridge> biz_project_bridge { get; set; }

        public DbSet<biz_project_bridge_castingGroup> biz_project_bridge_castingGroup { get; set; }
        public DbSet<biz_steel_beam_theoretical> biz_steel_beam_theoretical { get; set; }
        public DbSet<biz_steel_beam_measure_batch> biz_steel_beam_measure_batch { get; set; }
        public DbSet<biz_steel_beam_measured> biz_steel_beam_measured { get; set; }

        #endregion
    }

    public class testTable
    {
        public int ID { set; get; }

        public string? Name { set; get; }

    }


    #region sys tables

    /// <summary>
    /// 部门表，存储组织结构信息
    /// </summary>
    public class sys_dept : IIDTable
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { set; get; }

        /// <summary>
        /// 外部系统部门唯一标识（对接企业内部系统）
        /// </summary>
        [Column("OID")]
        public long OID { set; get; }

        /// <summary>
        /// 部门名称
        /// </summary>
        [MaxLength(200)]
        [Required]
        public string Name { set; get; }

        /// <summary>
        /// 父部门ID（本系统自关联）
        /// </summary>
        public int? ParentID { set; get; }

        /// <summary>
        /// 父部门OID（外部系统关联）
        /// </summary>
        public long? ParentOID { set; get; }

        /// <summary>
        /// 父部门名称（冗余字段，便于展示）
        /// </summary>
        [MaxLength(200)]
        public string? ParentName { set; get; }

        /// <summary>
        /// 部门全称/路径（如：总公司/技术部/研发组）
        /// </summary>
        [MaxLength(500)]
        public string FullName { set; get; }

        /// <summary>
        /// 状态：0-保存，1-提交，-1-删除
        /// </summary>
        public int Status { set; get; }

        /// <summary>
        /// 创建人
        /// </summary>
        [MaxLength(100)]
        public string? CreatedBy { set; get; }

        /// <summary>
        /// 创建时间（业务时区时间）
        /// </summary>
        public DateTime? CreatedTime { set; get; }

        /// <summary>
        /// 更新人
        /// </summary>
        [MaxLength(100)]
        public string? UpdatedBy { set; get; }

        /// <summary>
        /// 更新时间（业务时区时间）
        /// </summary>
        public DateTime? UpdatedTime { set; get; }
    }


    /// <summary>
    /// 用户表，存储系统用户信息
    /// </summary>
    public class sys_userinfo : IIDTable
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { set; get; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string Name { set; get; }

        /// <summary>
        /// 员工编号（对接企业内部系统，唯一标识，非内部用户可为空）
        /// </summary>
        [MaxLength(32)]
        public string? EmpNo { set; get; }

        /// <summary>
        /// 登录账号（唯一）
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string Account { set; get; }

        /// <summary>
        /// 手机号
        /// </summary>
        [MaxLength(20)]
        public string? Mobile { set; get; }

        /// <summary>
        /// 部门OID（关联sys_dept.OID，用于对接外部系统）
        /// </summary>
        public long? DeptOID { set; get; }

        /// <summary>
        /// 最后登录时间（业务时区时间）
        /// </summary>
        public DateTime? LastLoginTime { set; get; }

        /// <summary>
        /// 访问令牌（JWT/OAuth Token）
        /// </summary>
        [MaxLength(256)]
        public string? AccessToken { set; get; }

        /// <summary>
        /// 访问令牌密钥（用于刷新或验证）
        /// </summary>
        [MaxLength(256)]
        public string? AccessTokenSecret { set; get; }

        /// <summary>
        /// 令牌过期时间（业务时区时间）
        /// </summary>
        public DateTime? TokenExpiredTime { set; get; }

        /// <summary>
        /// 密码哈希值（MD5加密）
        /// </summary>
        [MaxLength(255)]
        public string? PasswordHash { set; get; }

        /// <summary>
        /// 状态：0-保存，1-提交，-1-删除
        /// </summary>
        public int Status { set; get; }

        /// <summary>
        /// 创建人
        /// </summary>
        [MaxLength(100)]
        public string? CreatedBy { set; get; }

        /// <summary>
        /// 创建时间（业务时区时间）
        /// </summary>
        public DateTime? CreatedTime { set; get; }

        /// <summary>
        /// 更新人
        /// </summary>
        [MaxLength(100)]
        public string? UpdatedBy { set; get; }

        /// <summary>
        /// 更新时间（业务时区时间）
        /// </summary>
        public DateTime? UpdatedTime { set; get; }
    }


    /// <summary>
    /// 角色表，存储系统角色信息
    /// </summary>
    public class sys_role : IIDTable
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { set; get; }

        /// <summary>
        /// 角色名称（如：管理员、普通用户）
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string Name { set; get; }

        /// <summary>
        /// 角色编码（如：ADMIN、USER，便于权限判断）
        /// </summary>
        [MaxLength(50)]
        public string? Code { set; get; }

        /// <summary>
        /// 状态：0-保存，1-提交，-1-删除
        /// </summary>
        public int Status { set; get; }

        /// <summary>
        /// 创建人
        /// </summary>
        [MaxLength(100)]
        public string? CreatedBy { set; get; }

        /// <summary>
        /// 创建时间（业务时区时间）
        /// </summary>
        public DateTime? CreatedTime { set; get; }

        /// <summary>
        /// 更新人
        /// </summary>
        [MaxLength(100)]
        public string? UpdatedBy { set; get; }

        /// <summary>
        /// 更新时间（业务时区时间）
        /// </summary>
        public DateTime? UpdatedTime { set; get; }
    }


    /// <summary>
    /// 用户角色关联表，实现用户与角色的多对多关系
    /// </summary>
    public class sys_user_role : IIDTable
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { set; get; }

        /// <summary>
        /// 用户ID（关联sys_userinfo.ID）
        /// </summary>
        public int UserID { set; get; }

        /// <summary>
        /// 角色ID（关联sys_role.ID）
        /// </summary>
        public int RoleID { set; get; }

        /// <summary>
        /// 状态：0-保存，1-提交，-1-删除
        /// </summary>
        public int Status { set; get; }

        /// <summary>
        /// 创建人
        /// </summary>
        [MaxLength(100)]
        public string? CreatedBy { set; get; }

        /// <summary>
        /// 创建时间（业务时区时间）
        /// </summary>
        public DateTime? CreatedTime { set; get; }

        /// <summary>
        /// 更新人
        /// </summary>
        [MaxLength(100)]
        public string? UpdatedBy { set; get; }

        /// <summary>
        /// 更新时间（业务时区时间）
        /// </summary>
        public DateTime? UpdatedTime { set; get; }
    }


    /// <summary>
    /// 系统菜单表，存储菜单/权限配置
    /// </summary>
    public class sys_menu : IIDTable
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { set; get; }

        /// <summary>
        /// 父菜单ID（用于构建树形结构）
        /// </summary>
        public int? ParentID { set; get; }

        /// <summary>
        /// 菜单名称
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string Name { set; get; }

        /// <summary>
        /// 菜单名称(En)
        /// </summary>
        [MaxLength(100)]
        public string? EnName { set; get; }

        /// <summary>
        /// 菜单全称/路径名称
        /// </summary>
        [MaxLength(200)]
        [Required]
        public string FullName { set; get; }

        /// <summary>
        /// 菜单全称/路径名称(En)
        /// </summary>
        [MaxLength(200)]
        public string? EnFullName { set; get; }

        /// <summary>
        /// 菜单图标（CSS类名）
        /// </summary>
        [MaxLength(100)]
        public string? Icon { set; get; }

        /// <summary>
        /// 菜单描述
        /// </summary>
        [MaxLength(500)]
        public string? Description { set; get; }

        /// <summary>
        /// 排序号（数值越小越靠前）
        /// </summary>
        public int Sort { set; get; }

        /// <summary>
        /// 权限标识（如：user:add，用于权限控制）
        /// </summary>
        [MaxLength(100)]
        public string? PermissionFlag { set; get; }

        /// <summary>
        /// 菜单行为：目录节点可为空，支持HTTP开头的跳转路径或站点相对路径
        /// </summary>
        [MaxLength(500)]
        public string? Action { set; get; }

        /// <summary>
        /// 菜单类型：directory-目录，menu-菜单，button-操作按钮
        /// </summary>
        [MaxLength(20)]
        public string? MenuType { set; get; }

        /// <summary>
        /// 状态：0-保存，1-提交，-1-删除
        /// </summary>
        public int Status { set; get; }

        /// <summary>
        /// 创建人
        /// </summary>
        [MaxLength(100)]
        public string? CreatedBy { set; get; }

        /// <summary>
        /// 创建时间（业务时区时间）
        /// </summary>
        public DateTime? CreatedTime { set; get; }

        /// <summary>
        /// 更新人
        /// </summary>
        [MaxLength(100)]
        public string? UpdatedBy { set; get; }

        /// <summary>
        /// 更新时间（业务时区时间）
        /// </summary>
        public DateTime? UpdatedTime { set; get; }
    }


    /// <summary>
    /// 角色菜单关联表，实现角色与菜单的多对多关系
    /// </summary>
    public class sys_role_menu : IIDTable
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { set; get; }

        /// <summary>
        /// 角色ID（关联sys_role.ID）
        /// </summary>
        public int RoleID { set; get; }

        /// <summary>
        /// 菜单ID（关联sys_menu.ID）
        /// </summary>
        public int MenuID { set; get; }

        /// <summary>
        /// 状态：0-保存，1-提交，-1-删除
        /// </summary>
        public int Status { set; get; }

        /// <summary>
        /// 创建人
        /// </summary>
        [MaxLength(100)]
        public string? CreatedBy { set; get; }

        /// <summary>
        /// 创建时间（业务时区时间）
        /// </summary>
        public DateTime? CreatedTime { set; get; }

        /// <summary>
        /// 更新人
        /// </summary>
        [MaxLength(100)]
        public string? UpdatedBy { set; get; }

        /// <summary>
        /// 更新时间（业务时区时间）
        /// </summary>
        public DateTime? UpdatedTime { set; get; }
    }


    /// <summary>
    /// 系统字典主表，定义字典分类
    /// </summary>
    public class sys_dic_data : IIDTable
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { set; get; }

        /// <summary>
        /// 字典分类（唯一标识，如：GENDER、USER_STATUS）
        /// </summary>
        [MaxLength(50)]
        [Required]
        public string Category { set; get; }

        /// <summary>
        /// 字典描述（如：性别、用户状态）
        /// </summary>
        [MaxLength(200)]
        public string? Description { set; get; }

        /// <summary>
        /// 状态：0-保存，1-提交，-1-删除
        /// </summary>
        public int Status { set; get; }

        /// <summary>
        /// 创建人
        /// </summary>
        [MaxLength(100)]
        public string? CreatedBy { set; get; }

        /// <summary>
        /// 创建时间（业务时区时间）
        /// </summary>
        public DateTime? CreatedTime { set; get; }

        /// <summary>
        /// 更新人
        /// </summary>
        [MaxLength(100)]
        public string? UpdatedBy { set; get; }

        /// <summary>
        /// 更新时间（业务时区时间）
        /// </summary>
        public DateTime? UpdatedTime { set; get; }
    }


    /// <summary>
    /// 字典明细表，存储字典的具体选项
    /// </summary>
    public class sys_dic_data_item : IIDTable
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { set; get; }

        /// <summary>
        /// 字典主表ID（关联sys_dic_data.ID）
        /// </summary>
        public int DicDataID { set; get; }

        /// <summary>
        /// 字典分类（冗余字段，便于直接查询）
        /// </summary>
        [MaxLength(50)]
        [Required]
        public string Category { set; get; }

        /// <summary>
        /// 字典项编码（如：M、F）
        /// </summary>
        [MaxLength(50)]
        [Required]
        public string Code { set; get; }

        /// <summary>
        /// 字典项名称（如：男、女）
        /// </summary>
        [MaxLength(200)]
        [Required]
        public string Name { set; get; }

        /// <summary>
        /// 生效开始时间（业务时区时间，可用于时间范围控制）
        /// </summary>
        public DateTime? EffectStart { set; get; }

        /// <summary>
        /// 生效结束时间（业务时区时间，可用于时间范围控制）
        /// </summary>
        public DateTime? EffectEnd { set; get; }

        /// <summary>
        /// 状态：0-保存，1-提交，-1-删除
        /// </summary>
        public int Status { set; get; }

        /// <summary>
        /// 创建人
        /// </summary>
        [MaxLength(100)]
        public string? CreatedBy { set; get; }

        /// <summary>
        /// 创建时间（业务时区时间）
        /// </summary>
        public DateTime? CreatedTime { set; get; }

        /// <summary>
        /// 更新人
        /// </summary>
        [MaxLength(100)]
        public string? UpdatedBy { set; get; }

        /// <summary>
        /// 更新时间（业务时区时间）
        /// </summary>
        public DateTime? UpdatedTime { set; get; }
    }

    #endregion

    #region biz tables
    /// <summary>
    /// 业务项目表
    /// </summary>
    public class biz_project : IIDTable
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { set; get; }

        /// <summary>
        /// 项目名称
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string Name { set; get; } = string.Empty;

        /// <summary>
        /// 项目描述
        /// </summary>
        [MaxLength(2000)]
        public string? Description { set; get; }

        /// <summary>
        /// 负责人id
        /// </summary>
        public int? ManagerId { set; get; }

        /// <summary>
        /// 负责人
        /// </summary>
        [MaxLength(100)]
        public string? ManagerName { set; get; } = string.Empty;

        /// <summary>
        /// 项目状态：0=在建, 1=完工
        /// </summary>
        public int? ProgressStatus { set; get; } = 0;

        /// <summary>
        /// 状态：0-保存，1-提交，-1-删除
        /// </summary>
        public int Status { set; get; }

        /// <summary>
        /// 创建人
        /// </summary>
        [MaxLength(100)]
        public string? CreatedBy { set; get; }

        /// <summary>
        /// 创建时间（业务时区时间）
        /// </summary>
        public DateTime? CreatedTime { set; get; }

        /// <summary>
        /// 更新人
        /// </summary>
        [MaxLength(100)]
        public string? UpdatedBy { set; get; }

        /// <summary>
        /// 更新时间（业务时区时间）
        /// </summary>
        public DateTime? UpdatedTime { set; get; }
    }

    /// <summary>
    /// 项目桥梁子表
    /// </summary>
    public class biz_project_bridge : IIDTable
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { set; get; }

        /// <summary>
        /// 项目id，biz_project 外键
        /// </summary>
        [ForeignKey(nameof(Project))]
        public int ProjID { set; get; }

        // 导航属性
        public biz_project Project { set; get; } = null!;

        public string Name { set; get; }
        public string? Code { set; get; }
        public string? Description { set; get; }
        /// <summary>
        /// 项目状态：0=钢梁, 1=混凝土梁
        /// </summary>
        public int BeamType { set; get; } = 0;

        /// <summary>
        /// 状态：0-保存，1-提交，-1-删除
        /// </summary>
        public int Status { set; get; }

        /// <summary>
        /// 创建人
        /// </summary>
        [MaxLength(100)]
        public string? CreatedBy { set; get; }

        /// <summary>
        /// 创建时间（业务时区时间）
        /// </summary>
        public DateTime? CreatedTime { set; get; }

        /// <summary>
        /// 更新人
        /// </summary>
        [MaxLength(100)]
        public string? UpdatedBy { set; get; }

        /// <summary>
        /// 更新时间（业务时区时间）
        /// </summary>
        public DateTime? UpdatedTime { set; get; }
    }



    /// <summary>
    /// 桥梁浇筑分组表
    /// </summary>
    public class biz_project_bridge_castingGroup : IIDTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { set; get; }

        [ForeignKey(nameof(Bridge))]
        public int BridgeID { set; get; }

        public biz_project_bridge Bridge { set; get; } = null!;

        [MaxLength(100)]
        [Required]
        public string Name { set; get; } = string.Empty;

        public int Status { set; get; }

        [MaxLength(100)]
        public string? CreatedBy { set; get; }

        public DateTime? CreatedTime { set; get; }

        [MaxLength(100)]
        public string? UpdatedBy { set; get; }

        public DateTime? UpdatedTime { set; get; }
    }
    /// <summary>
    ///// 项目桥梁浇筑分组表
    ///// </summary>
    //public class biz_project_casting_set
    //{
    //    public int ID { set; get; }
    //    public string Name { set; get; }
    //    public string? Code { set; get; }
    //    public int? AuthorID { set; get; }
    //    public decimal DefaOffsetDimA { set; get; }
    //    public decimal DefaOffsetDimB { set; get; }
    //    public decimal DefaOffsetDimC { set; get; }
    //    public decimal CellRefElevation { set; get; }
    //    public int Revision { set; get; }
    //    public int Status { set; get; }
    //    public string? CreatedBy { set; get; }
    //    public DateTime? CreatedTime { set; get; }
    //    public string? UpdatedBy { set; get; }
    //    public DateTime? UpdatedTime { set; get; }
    //}

    public class biz_project_segment
    {
        public int ID { set; get; }
        public string Name { set; get; }
        public string? Code { set; get; }
        public DateTime? CastDate { set; get; }
        public int Joint1 { set; get; }
        public int Joint2 { set; get; }
        public int Status { set; get; }
        public string? CreatedBy { set; get; }
        public DateTime? CreatedTime { set; get; }
        public string? UpdatedBy { set; get; }
        public DateTime? UpdatedTime { set; get; }
    }
    public class biz_project_joint
    {
        public int ID { set; get; }
        public string Name { set; get; }
        public string? Code { set; get; }
        /// <summary>
        /// 在同一个casting set内的index
        /// </summary>
        public int IndexInSet { set; get; }
        public int Status { set; get; }
        public string? CreatedBy { set; get; }
        public DateTime? CreatedTime { set; get; }
        public string? UpdatedBy { set; get; }
        public DateTime? UpdatedTime { set; get; }
    }

    #endregion
}
