namespace Model.tech.QL.DTO.BizProject;

/// <summary>预警配置保存 DTO。六个阈值均可空，空 = 该等级未配置。</summary>
public class ConcreteBeamAlarmConfigSaveDTO
{
    /// <summary>浇筑组ID。</summary>
    public int CastingGroupID { set; get; }
    public decimal? Level1Min { set; get; }
    public decimal? Level1Max { set; get; }
    public decimal? Level2Min { set; get; }
    public decimal? Level2Max { set; get; }
    public decimal? Level3Min { set; get; }
    public decimal? Level3Max { set; get; }
}

/// <summary>预警配置读取 DTO。</summary>
public class ConcreteBeamAlarmConfigDTO
{
    public int CastingGroupID { set; get; }
    public decimal? Level1Min { set; get; }
    public decimal? Level1Max { set; get; }
    public decimal? Level2Min { set; get; }
    public decimal? Level2Max { set; get; }
    public decimal? Level3Min { set; get; }
    public decimal? Level3Max { set; get; }
    /// <summary>是否已存在配置（决定弹框初始只读/可编辑）。</summary>
    public bool HasConfig { set; get; }
}

/// <summary>单条预警记录。</summary>
public class ConcreteBeamAlarmRecordDTO
{
    public int ID { set; get; }
    /// <summary>预警时间（测量提交时间）。</summary>
    public DateTime AlarmTime { set; get; }
    /// <summary>部位：节点名 + 测线（如 Joint1 E）。</summary>
    public string Position { set; get; } = string.Empty;
    /// <summary>偏差值（米，带符号）。</summary>
    public decimal Deviation { set; get; }
    /// <summary>预警等级：1=一级，2=二级，3=三级。</summary>
    public int Level { set; get; }
}

/// <summary>单测线折线图数据。</summary>
public class ConcreteBeamAlarmChartDTO
{
    /// <summary>测线：E=东，C=中，W=西。</summary>
    public string LineType { set; get; } = string.Empty;
    /// <summary>图标题。</summary>
    public string Title { set; get; } = string.Empty;
    /// <summary>X 轴节点名序列。</summary>
    public List<string> Nodes { set; get; } = new();
    /// <summary>各节点偏差值（米，带符号；与 Nodes 等长，缺失为 null）。</summary>
    public List<decimal?> Deviations { set; get; } = new();
}

/// <summary>误差分析与报警页面数据。</summary>
public class ConcreteBeamAlarmPageDTO
{
    /// <summary>三张折线图（东/中/西）。</summary>
    public List<ConcreteBeamAlarmChartDTO> Charts { set; get; } = new();
    /// <summary>预警记录列表。</summary>
    public List<ConcreteBeamAlarmRecordDTO> Records { set; get; } = new();
    /// <summary>是否已存在预警配置。</summary>
    public bool HasConfig { set; get; }
}
