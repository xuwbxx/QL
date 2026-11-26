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
        public int? GirderID { set; get; }
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
        public int GirderID { get; set; }
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
        public int GirderID { get; set; }
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
        /// <summary>数据是否来自当前选中批次；false = 由同顶推次数下更早批次最近一次上传值补齐（前端标注来源时间且不可编辑/删除）。</summary>
        public bool IsCurrentBatch { get; set; }
        public int PointRowSpan { get; set; }
    }

    public class SteelBeamCoordinateUpdateDTO
    {
        public int ID { get; set; }
        public int GirderID { get; set; }
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public decimal Z { get; set; }
        public int Version { get; set; }
    }

    /// <summary>实测数据批量删除请求（口径与单行删除一致：仅最新批次的可编辑行）。</summary>
    public class SteelBeamMeasuredBatchDeleteDTO
    {
        public int GirderID { get; set; }
        /// <summary>待删除的实测行ID集合（前端勾选结果）。</summary>
        public System.Collections.Generic.List<int> IDs { get; set; } = new();
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
