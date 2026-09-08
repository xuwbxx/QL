namespace Model.tech.QL.DTO.BizProject
{
    /// <summary>
    /// 混凝土梁管理列表 DTO
    /// </summary>
    public class ConcreteBeamItemDTO
    {
        public int ID { set; get; }
        public int ProjID { set; get; }
        public int GirderID { set; get; }
        public string ProjectName { set; get; } = string.Empty;
        public string BridgeName { set; get; } = string.Empty;
        public string CastingGroupName { set; get; } = string.Empty;
        public string ManagerName { set; get; } = string.Empty;
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

    public class ConcreteBeamConfigDTO
    {
        public int CastingGroupID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string BridgeName { get; set; } = string.Empty;
        public string CastingGroupName { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public decimal? OffsetA { get; set; }
        public decimal? OffsetB { get; set; }
        public decimal? OffsetC { get; set; }
        public decimal? ReferenceElevation { get; set; }
        public bool IsConfigured { get; set; }
        public bool ElevationIncludesCamber { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }

    public class ConcreteBeamConfigSaveDTO
    {
        public int CastingGroupID { get; set; }
        public decimal OffsetA { get; set; }
        public decimal OffsetB { get; set; }
        public decimal OffsetC { get; set; }
        public decimal? ReferenceElevation { get; set; }
    }

    public class ConcreteBeamDataQueryDTO
    {
        public int CastingGroupID { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool UseCalculation { get; set; }
    }

    public class ConcreteBeamSegmentDTO
    {
        public string SegmentName { get; set; } = string.Empty;
        public string? Downstream { get; set; }
        public string? Upstream { get; set; }
        public string Node1 { get; set; } = string.Empty;
        public string Node2 { get; set; } = string.Empty;
        public DateTime? PouringDate { get; set; }
    }

    public class ConcreteBeamDesignCoordinateDTO
    {
        public string NodeName { get; set; } = string.Empty;
        public decimal XE { get; set; }
        public decimal YE { get; set; }
        public decimal ZE { get; set; }
        public decimal XC { get; set; }
        public decimal YC { get; set; }
        public decimal ZC { get; set; }
        public decimal XW { get; set; }
        public decimal YW { get; set; }
        public decimal ZW { get; set; }
    }
    public class ConcreteBeamCamberDTO
    {
        public string NodeName { get; set; } = string.Empty;
        public decimal ZE { get; set; } public decimal ZC { get; set; } public decimal ZW { get; set; }
    }

    public class ConcreteBeamPagedResultDTO<T>
    {
        public List<T> List { get; set; } = new();
        public int Total { get; set; }
    }

    public class ConcreteBeamImportErrorDTO
    {
        public int ExcelRow { get; set; }
        public string RecordKey { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string OriginalValue { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class ConcreteBeamImportResultDTO
    {
        public bool Success { get; set; }
        public bool RequiresOverwriteConfirmation { get; set; }
        public int ImportedCount { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ConcreteBeamImportErrorDTO> Errors { get; set; } = new();
        public byte[]? ErrorFile { get; set; }
        public string? ErrorFileName { get; set; }
    }
}
