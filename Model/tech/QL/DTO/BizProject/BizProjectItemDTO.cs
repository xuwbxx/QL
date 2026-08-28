using System.ComponentModel.DataAnnotations;

namespace Model.tech.QL.DTO.BizProject
{
    public class BizProjectItemDTO
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        public int ID { set; get; }

        /// <summary>
        /// 所属项目ID
        /// </summary>
        public long ProjectId { set; get; }

        /// <summary>
        /// 分项名称
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string Name { set; get; } = string.Empty;

        /// <summary>
        /// 分项描述
        /// </summary>
        [MaxLength(2000)]
        public string? Description { set; get; }

        /// <summary>
        /// 分项负责人id
        /// </summary>
        public int? ManagerId { set; get; }

        /// <summary>
        /// 分项负责人名称
        /// </summary>
        [MaxLength(100)]
        public string? ManagerName { set; get; } = string.Empty;


        /// <summary>
        /// 分项状态：0=在建, 1=完工
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
}
