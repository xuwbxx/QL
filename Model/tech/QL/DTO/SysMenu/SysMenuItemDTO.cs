using System.ComponentModel.DataAnnotations;

namespace Model.tech.QL.DTO.SysMenu
{
    public class SysMenuItemDTO
    {
        /// <summary>
        /// 自增主键
        /// </summary>
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
        /// 菜单名称
        /// </summary>
        [MaxLength(100)]
        public string EnName { set; get; }

        /// <summary>
        /// 菜单全称/路径名称
        /// </summary>
        [MaxLength(200)]
        [Required]
        public string FullName { set; get; }

        /// <summary>
        /// 菜单全称/路径名称
        /// </summary>
        [MaxLength(200)]
        public string EnFullName { set; get; }

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
}
