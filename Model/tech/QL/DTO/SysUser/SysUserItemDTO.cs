namespace Model.tech.QL.DTO.SysUser
{
    /// <summary>
    /// 用于查询的SysUserInfo
    /// </summary>
    public class SysUserItemDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public string? EmpNo { get; set; }
        public string? Mobile { get; set; }
        public int Status { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string UpdatedBy { get; set; }
    }
}
