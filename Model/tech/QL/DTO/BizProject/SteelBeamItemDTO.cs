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
        public int PageSize { set; get; } = 20;
    }

    /// <summary>
    /// 钢梁管理分页结果 DTO
    /// </summary>
    public class SteelBeamPagedResultDTO<T>
    {
        public List<T> List { set; get; } = new();
        public int Total { set; get; }
    }

    public class SteelBeamTheoreticalQueryDTO
    {
        public int BridgeID { get; set; }
        public string? PointCode { get; set; }
        public string? SegmentNo { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class SteelBeamTheoreticalItemDTO
    {
        public int ID { get; set; }
        public string PointCode { get; set; } = string.Empty;
        public decimal DesignX { get; set; }
        public decimal DesignY { get; set; }
        public decimal DesignZ { get; set; }
        public decimal PreCamber { get; set; }
        public decimal Weight { get; set; }
        public string SegmentNo { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public int Version { get; set; }
        public bool CanEdit { get; set; }
    }

    public class SteelBeamMeasuredQueryDTO
    {
        public int BridgeID { get; set; }
        public int? PushCount { get; set; }
        public DateTime? MeasureTime { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class SteelBeamMeasuredItemDTO
    {
        public int ID { get; set; }
        public string PointCode { get; set; } = string.Empty;
        public decimal MeasuredX { get; set; }
        public decimal MeasuredY { get; set; }
        public decimal MeasuredZ { get; set; }
        public int PushCount { get; set; }
        public DateTime MeasureTime { get; set; }
        public DateTime ImportTime { get; set; }
        public int Version { get; set; }
        public bool CanEdit { get; set; }
        public int PointRowSpan { get; set; }
    }

    public class SteelBeamCoordinateUpdateDTO
    {
        public int ID { get; set; }
        public int BridgeID { get; set; }
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public decimal Z { get; set; }
        public int Version { get; set; }
    }

    public class SteelBeamImportStateDTO
    {
        public bool HasTheoretical { get; set; }
        public bool HasMeasured { get; set; }
        public int? MaxPushCount { get; set; }
        public List<int> ImportPushCounts { get; set; } = new();
        public List<int> QueryPushCounts { get; set; } = new();
    }

    public class SteelBeamImportOutcome
    {
        public bool Success { get; set; }
        public int ImportedCount { get; set; }
        public string Message { get; set; } = string.Empty;
        public byte[]? ErrorFile { get; set; }
        public string? ErrorFileName { get; set; }
    }
}
