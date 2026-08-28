using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataFactory.Factory;

namespace DataFactory.KingBase;

public class biz_steel_beam_theoretical : IIDTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }
    public int ProjID { get; set; }
    public int BridgeID { get; set; }
    [Required, MaxLength(50)] public string PointCode { get; set; } = string.Empty;
    [Column(TypeName = "decimal(20,6)")] public decimal DesignX { get; set; }
    [Column(TypeName = "decimal(20,6)")] public decimal DesignY { get; set; }
    [Column(TypeName = "decimal(20,6)")] public decimal DesignZ { get; set; }
    [Column(TypeName = "decimal(20,6)")] public decimal PreCamber { get; set; }
    [Column(TypeName = "decimal(5,4)")] public decimal Weight { get; set; }
    [Required, MaxLength(50)] public string SegmentNo { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string PositionName { get; set; } = string.Empty;
    public bool? IsFirstCoordinate { get; set; }
    public int? PositionOrder { get; set; }
    [Column(TypeName = "decimal(20,6)")] public decimal? DistanceFromStart { get; set; }
    public int Version { get; set; } = 1;
    public int Status { get; set; }
    [MaxLength(100)] public string? CreatedBy { get; set; }
    public DateTime? CreatedTime { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }
    public DateTime? UpdatedTime { get; set; }
}

public class biz_steel_beam_measure_batch : IIDTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }
    public int ProjID { get; set; }
    public int BridgeID { get; set; }
    public int PushCount { get; set; }
    public DateTime MeasureTime { get; set; }
    public int ImportCount { get; set; }
    public int? ReplacedByBatchID { get; set; }
    public int Status { get; set; }
    [MaxLength(100)] public string? CreatedBy { get; set; }
    public DateTime? CreatedTime { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }
    public DateTime? UpdatedTime { get; set; }
}

public class biz_steel_beam_measured : IIDTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }
    public int ProjID { get; set; }
    public int BridgeID { get; set; }
    public int BatchID { get; set; }
    public int TheoreticalID { get; set; }
    [Required, MaxLength(50)] public string PointCode { get; set; } = string.Empty;
    [Column(TypeName = "decimal(20,6)")] public decimal MeasuredX { get; set; }
    [Column(TypeName = "decimal(20,6)")] public decimal MeasuredY { get; set; }
    [Column(TypeName = "decimal(20,6)")] public decimal MeasuredZ { get; set; }
    public DateTime ImportTime { get; set; }
    public int Version { get; set; } = 1;
    public int Status { get; set; }
    [MaxLength(100)] public string? CreatedBy { get; set; }
    public DateTime? CreatedTime { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }
    public DateTime? UpdatedTime { get; set; }
}
