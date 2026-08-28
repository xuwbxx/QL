using System.ComponentModel.DataAnnotations;

namespace Model.tech.QL.DTO.BizProject
{
    /// <summary>
    /// 项目桥梁子项 DTO
    /// </summary>
    public class BizProjectBridgeItemDTO
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        public int ID { set; get; }

        /// <summary>
        /// 所属项目ID（biz_project 外键）
        /// </summary>
        public int ProjID { set; get; }

        /// <summary>
        /// 桥梁名称
        /// </summary>
        [MaxLength(100)]
        [Required(ErrorMessage = "桥梁名称不能为空")]
        public string Name { set; get; } = string.Empty;

        /// <summary>
        /// 梁类型：0=钢梁, 1=混凝土梁
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
        /// 创建时间
        /// </summary>
        public DateTime? CreatedTime { set; get; }

        /// <summary>
        /// 更新人
        /// </summary>
        [MaxLength(100)]
        public string? UpdatedBy { set; get; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedTime { set; get; }
    }
}
