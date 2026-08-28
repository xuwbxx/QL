using System.ComponentModel.DataAnnotations;

namespace Model.tech.QL.DTO.BizProject
{
    /// <summary>
    /// 桥梁浇筑分组子项 DTO
    /// </summary>
    public class BizProjectBridgeCastingGroupItemDTO
    {
        public int ID { set; get; }

        public int BridgeID { set; get; }

        [MaxLength(100)]
        [Required(ErrorMessage = "浇筑分组名称不能为空")]
        public string Name { set; get; } = string.Empty;

        public int Status { set; get; }

        [MaxLength(100)]
        public string? CreatedBy { set; get; }

        public DateTime? CreatedTime { set; get; }

        [MaxLength(100)]
        public string? UpdatedBy { set; get; }

        public DateTime? UpdatedTime { set; get; }
    }
}