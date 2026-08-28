namespace Model.tech.QL.DTO.BizProject
{
    /// <summary>
    /// 钢梁管理列表 DTO
    /// </summary>
    public class SteelBeamItemDTO
    {
        public int ID { set; get; }
        public int ProjID { set; get; }
        public string ProjectName { set; get; } = string.Empty;
        public string BridgeName { set; get; } = string.Empty;
        public int BeamType { set; get; }
    }

    /// <summary>
    /// 钢梁管理查询 DTO
    /// </summary>
    public class SteelBeamQueryDTO
    {
        public int? ProjID { set; get; }
        public int? BridgeID { set; get; }
        public int PageIndex { set; get; } = 1;
        public int PageSize { set; get; } = 10;
    }

    /// <summary>
    /// 钢梁管理分页结果 DTO
    /// </summary>
    public class SteelBeamPagedResultDTO
    {
        public List<SteelBeamItemDTO> List { set; get; } = new();
        public int Total { set; get; }
    }
}
