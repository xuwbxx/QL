namespace Model.tech.QL.DTO.BizProject
{
    /// <summary>
    /// 混凝土梁管理列表 DTO
    /// </summary>
    public class ConcreteBeamItemDTO
    {
        public int ID { set; get; }
        public int ProjID { set; get; }
        public int BridgeID { set; get; }
        public string ProjectName { set; get; } = string.Empty;
        public string BridgeName { set; get; } = string.Empty;
        public string CastingGroupName { set; get; } = string.Empty;
    }

    /// <summary>
    /// 混凝土梁管理查询 DTO
    /// </summary>
    public class ConcreteBeamQueryDTO
    {
        public int? ProjID { set; get; }
        public string? CastingGroupName { set; get; }
        public int PageIndex { set; get; } = 1;
        public int PageSize { set; get; } = 10;
    }

    /// <summary>
    /// 混凝土梁管理分页结果 DTO
    /// </summary>
    public class ConcreteBeamPagedResultDTO
    {
        public List<ConcreteBeamItemDTO> List { set; get; } = new();
        public int Total { set; get; }
    }
}
