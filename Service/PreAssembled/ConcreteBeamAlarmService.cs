using DataFactory.KingBase;
using Microsoft.EntityFrameworkCore;
using Model.tech.QL.DTO.BizProject;

namespace Service.PreAssembled;

/// <summary>
/// 混凝土梁【误差分析与报警】（ConcreteBeamService 分部）。
/// 偏差口径：设站及测量（St=1 实测修正）提交落库后的 as-cast 端面坐标 Z − 设计计算坐标 Z，
/// 按 节点 × E/C/W（东/中/西）维度输出；偏差值落库后固定不变。
/// 预警记录幂等键：浇筑组 + 节点(design_coordinate) + 测线 + 等级。
/// </summary>
public partial class ConcreteBeamService
{
    /// <summary>
    /// 判级规则（需求 2.3.7）：
    /// 1) v 落入某已配置区间 [Min, Max) → 该级；
    /// 2) v 超出已配置档上限 → 判其上一级（只升一级，封顶一级；上一级阈值未配置也判上一级）；
    /// 3) v 小于最低已配置档下限 → 不预警（null）；
    /// 4) 未配置档（min/max 均空）不参与匹配。
    /// </summary>
    internal static int? JudgeAlarmLevel(decimal absDeviation, biz_concrete_beam_alarm_config cfg)
    {
        if (cfg.Level3Min.HasValue && cfg.Level3Max.HasValue
            && absDeviation >= cfg.Level3Min && absDeviation < cfg.Level3Max) return 3;
        if (cfg.Level2Min.HasValue && cfg.Level2Max.HasValue
            && absDeviation >= cfg.Level2Min && absDeviation < cfg.Level2Max) return 2;
        if (cfg.Level1Min.HasValue && cfg.Level1Max.HasValue
            && absDeviation >= cfg.Level1Min && absDeviation < cfg.Level1Max) return 1;

        if (cfg.Level3Min.HasValue && cfg.Level3Max.HasValue && absDeviation >= cfg.Level3Max)
        {
            if (cfg.Level2Min.HasValue && cfg.Level2Max.HasValue && absDeviation >= cfg.Level2Max) return 1;
            return 2;
        }
        if (cfg.Level2Min.HasValue && cfg.Level2Max.HasValue && absDeviation >= cfg.Level2Max) return 1;
        return null;
    }

    /// <summary>读取预警配置（弹框初始展示）。</summary>
    public async Task<ConcreteBeamAlarmConfigDTO> GetAlarmConfig(int castingGroupID)
    {
        await EnsureCastingGroupAccess(castingGroupID);
        var cfg = await Db.Query<biz_concrete_beam_alarm_config>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
            .FirstOrDefaultAsync();
        return new ConcreteBeamAlarmConfigDTO
        {
            CastingGroupID = castingGroupID,
            Level1Min = cfg?.Level1Min, Level1Max = cfg?.Level1Max,
            Level2Min = cfg?.Level2Min, Level2Max = cfg?.Level2Max,
            Level3Min = cfg?.Level3Min, Level3Max = cfg?.Level3Max,
            HasConfig = cfg != null
        };
    }

    /// <summary>
    /// 保存预警配置（upsert，每浇筑组一行）。
    /// 校验：成对填写（min/max 同时空或同时有值）、min &lt; max、非负。
    /// </summary>
    public async Task<ConcreteBeamAlarmConfigDTO> SaveAlarmConfig(ConcreteBeamAlarmConfigSaveDTO req)
    {
        var ctx = await EnsureCastingGroupAccess(req.CastingGroupID);
        ValidateAlarmConfig(req);

        var account = GetCurrentAccount();
        var now = DateTime.Now;
        var cfg = await Db.Query<biz_concrete_beam_alarm_config>()
            .Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1)
            .FirstOrDefaultAsync();
        if (cfg == null)
        {
            cfg = new biz_concrete_beam_alarm_config
            {
                ProjID = ctx.ProjID,
                GirderID = ctx.GirderID,
                CastingGroupID = req.CastingGroupID,
                Status = 0,
                CreatedBy = account,
                CreatedTime = now
            };
            Db.GetRepository<biz_concrete_beam_alarm_config>().Add(cfg);
        }
        cfg.Level1Min = req.Level1Min; cfg.Level1Max = req.Level1Max;
        cfg.Level2Min = req.Level2Min; cfg.Level2Max = req.Level2Max;
        cfg.Level3Min = req.Level3Min; cfg.Level3Max = req.Level3Max;
        cfg.UpdatedBy = account;
        cfg.UpdatedTime = now;
        await Db.SaveAsync();

        return new ConcreteBeamAlarmConfigDTO
        {
            CastingGroupID = req.CastingGroupID,
            Level1Min = cfg.Level1Min, Level1Max = cfg.Level1Max,
            Level2Min = cfg.Level2Min, Level2Max = cfg.Level2Max,
            Level3Min = cfg.Level3Min, Level3Max = cfg.Level3Max,
            HasConfig = true
        };
    }

    private void ValidateAlarmConfig(ConcreteBeamAlarmConfigSaveDTO req)
    {
        void CheckPair(decimal? min, decimal? max, string name, string lang)
        {
            if (min.HasValue != max.HasValue)
                throw new InvalidOperationException(lang.StartsWith("zh")
                    ? $"{name}预警区间必须同时填写最小值和最大值"
                    : $"Level {name} alarm range requires both min and max values");
            if (min.HasValue && max.HasValue)
            {
                if (min.Value < 0 || max.Value <= 0)
                    throw new InvalidOperationException(lang.StartsWith("zh")
                        ? $"{name}预警区间不允许负数"
                        : $"Level {name} alarm range must be non-negative");
                if (min.Value >= max.Value)
                    throw new InvalidOperationException(lang.StartsWith("zh")
                        ? $"{name}预警区间要求 最小值 < 最大值"
                        : $"Level {name} alarm range requires min < max");
            }
        }
        CheckPair(req.Level1Min, req.Level1Max, "一级", Lang);
        CheckPair(req.Level2Min, req.Level2Max, "二级", Lang);
        CheckPair(req.Level3Min, req.Level3Max, "三级", Lang);
    }

    /// <summary>
    /// 误差分析与报警页面数据：三张折线图（E/C/W）+ 预警记录列表。
    /// 偏差 = as-cast 端面坐标 Z（实测修正后） − 设计计算坐标 Z；缺失节点为 null（图表断开）。
    /// </summary>
    public async Task<ConcreteBeamAlarmPageDTO> GetAlarmPageData(int castingGroupID)
    {
        await EnsureCastingGroupAccess(castingGroupID);
        var cfg = await Db.Query<biz_concrete_beam_alarm_config>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
            .FirstOrDefaultAsync();

        // 节点（source 顺序即导入节点顺序）
        var joints = await Db.Query<biz_concrete_beam_design_coordinate_source>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
            .OrderBy(a => a.ID)
            .ToListAsync();
        // 设计计算坐标（按节点名匹配理论 Z）
        var calcByNode = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
            .ToDictionaryAsync(a => a.NodeName);
        // as-cast 端面坐标（按节点锚点）
        var ascastByNode = (await Db.Query<biz_concrete_beam_ascast_coordinate>()
                .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
                .ToListAsync())
            .GroupBy(a => a.DesignCoordinateID)
            .ToDictionary(g => g.Key, g => g.First());

        var isZh = Lang.StartsWith("zh");
        string[] titles = isZh
            ? new[] { "东侧预制偏差", "中间预制偏差", "西侧预制偏差" }
            : new[] { "East side deviation", "Center deviation", "West side deviation" };
        string[] lines = { "E", "C", "W" };

        var page = new ConcreteBeamAlarmPageDTO { HasConfig = cfg != null };
        for (var li = 0; li < 3; li++)
        {
            var chart = new ConcreteBeamAlarmChartDTO { LineType = lines[li], Title = titles[li] };
            foreach (var node in joints)
            {
                chart.Nodes.Add(node.NodeName);
                decimal? dev = null;
                if (calcByNode.TryGetValue(node.NodeName, out var calc)
                    && ascastByNode.TryGetValue(node.ID, out var asc))
                {
                    decimal? ascZ = li switch
                    {
                        0 => asc.EastZ,
                        1 => asc.CenterZ,
                        _ => asc.WestZ
                    };
                    decimal calcZ = li switch
                    {
                        0 => calc.ZE,
                        1 => calc.ZC,
                        _ => calc.ZW
                    };
                    if (ascZ.HasValue) dev = Math.Round(ascZ.Value - calcZ, 4, MidpointRounding.AwayFromZero);
                }
                chart.Deviations.Add(dev);
            }
            page.Charts.Add(chart);
        }

        page.Records = await Db.Query<biz_concrete_beam_alarm_record>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
            .OrderByDescending(a => a.AlarmTime)
            .ThenBy(a => a.ID)
            .Select(a => new ConcreteBeamAlarmRecordDTO
            {
                ID = a.ID,
                AlarmTime = a.AlarmTime,
                Position = a.NodeName + " " + a.LineType,
                Deviation = a.Deviation,
                Level = a.Level
            })
            .ToListAsync();
        return page;
    }

    /// <summary>
    /// 设站及测量提交成功后生成预警记录（在 SaveSurvey 事务内调用）。
    /// 幂等键：浇筑组 + 节点(DesignCoordinateID) + 测线 + 等级；未配置阈值时静默跳过。
    /// </summary>
    /// <param name="projID">项目ID</param>
    /// <param name="girderID">梁ID</param>
    /// <param name="castingGroupID">浇筑组ID</param>
    /// <param name="segmentID">本次提交的梁段ID</param>
    /// <param name="jointIndexes">本次产出的节点下标（对应 joints/KT）</param>
    /// <param name="joints">设计坐标 source 节点列表（导入顺序）</param>
    /// <param name="calcRows">设计计算坐标（理论 Z 基准）</param>
    /// <param name="kt">as-cast 端面坐标矩阵（double[9]），KT[ji] = [EastX,EastY,EastZ, CenterX,CenterY,CenterZ, WestX,WestY,WestZ]</param>
    /// <param name="alarmTime">预警时间（测量提交时间）</param>
    internal async Task EnsureAlarmRecordsAsync(int projID, int girderID, int castingGroupID, int segmentID,
        List<int> jointIndexes, List<biz_concrete_beam_design_coordinate_source> joints,
        List<biz_concrete_beam_design_coordinate_calculation> calcRows, List<double[]> kt, DateTime alarmTime)
    {
        var cfg = await Db.Query<biz_concrete_beam_alarm_config>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
            .FirstOrDefaultAsync();
        if (cfg == null) return;

        var calcByNode = calcRows.ToDictionary(a => a.NodeName);
        var account = GetCurrentAccount();
        var now = DateTime.Now;

        foreach (var ji in jointIndexes)
        {
            if (ji < 0 || ji >= joints.Count || ji >= kt.Count) continue;
            var src = joints[ji];
            if (!calcByNode.TryGetValue(src.NodeName, out var calc)) continue;

            // 与 ApplyJointValues 落库口径一致：先 Round4 再作差
            var eastZ = Math.Round((decimal)kt[ji][2], 4, MidpointRounding.AwayFromZero);
            var centerZ = Math.Round((decimal)kt[ji][5], 4, MidpointRounding.AwayFromZero);
            var westZ = Math.Round((decimal)kt[ji][8], 4, MidpointRounding.AwayFromZero);
            (string Line, decimal Dev)[] devs =
            {
                ("E", eastZ - calc.ZE),
                ("C", centerZ - calc.ZC),
                ("W", westZ - calc.ZW)
            };

            foreach (var (line, dev) in devs)
            {
                var rounded = Math.Round(dev, 4, MidpointRounding.AwayFromZero);
                var level = JudgeAlarmLevel(Math.Abs(rounded), cfg);
                if (level == null) continue;

                var existing = await Db.Query<biz_concrete_beam_alarm_record>()
                    .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1
                                && a.DesignCoordinateID == src.ID && a.LineType == line && a.Level == level.Value)
                    .FirstOrDefaultAsync();
                if (existing == null)
                {
                    Db.GetRepository<biz_concrete_beam_alarm_record>().Add(new biz_concrete_beam_alarm_record
                    {
                        ProjID = projID,
                        GirderID = girderID,
                        CastingGroupID = castingGroupID,
                        SegmentID = segmentID,
                        DesignCoordinateID = src.ID,
                        NodeName = src.NodeName,
                        LineType = line,
                        Deviation = rounded,
                        Level = level.Value,
                        AlarmTime = alarmTime,
                        Status = 0,
                        CreatedBy = account,
                        CreatedTime = now
                    });
                }
                else
                {
                    // 幂等：偏差值固定，重复提交同段时仅刷新审计字段
                    existing.SegmentID = segmentID;
                    existing.Deviation = rounded;
                    existing.UpdatedBy = account;
                    existing.UpdatedTime = now;
                }
            }
        }
        await Db.SaveAsync();
    }
}
