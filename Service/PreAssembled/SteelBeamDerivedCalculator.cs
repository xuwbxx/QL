using Service.PreAssembled.BridgeSegmentAlignment;

namespace Service.PreAssembled;

/// <summary>
/// 钢梁顶推「滚动快照」推算器（2026-10-21 版）。
/// <para>业务规则（按顺序理解）：</para>
/// <list type="number">
/// <item><description>
/// derived 表按 <c>TheoreticalID</c> 逻辑唯一：每次新顶推上传后，同一测点只保留一行最新坐标，
/// 不按批次累积。BatchID/MeasuredID 记录这行坐标最近一次来源。
/// </description></item>
/// <item><description>
/// 同一 PushCount 允许多个 batch，以 MeasureTime 最新的 batch 为准；旧 batch 独有的测点直接忽略，
/// 不参与快照。该规则由调用方 replay（0..当前 PushCount）保证，本推算器只处理「当前 batch」。
/// </description></item>
/// <item><description>
/// 本批上传的测点：取真实实测坐标，<c>MeasuredID</c> = 本批 measured.ID，<c>BatchID</c> = 本批 batch.ID。
/// </description></item>
/// <item><description>
/// 本批未上传、但以前实测过的测点：不能直接用上次坐标，必须用刚体变换 T 从上次快照搬到当前位置；
/// <c>MeasuredID</c> 保留原来那次实测 ID，<c>BatchID</c> 更新为本批 batch.ID。
/// </description></item>
/// <item><description>
/// 从未实测过的测点：用本段设计坐标 + 本段本次上传点做 Gescha 补全（逐段，不是整批）；
/// <c>MeasuredID = 0</c>，<c>BatchID = 0</c>，表示这是补全值，不是实测值。
/// 后续该点有实测上传时，MeasuredID 会由 0 更新为最新的实测 ID。
/// </description></item>
/// <item><description>
/// T 的算法：先按段用设计坐标补全出「上次完整集」和「本次完整集」，再用两份完整集的同名测点
/// 拟合一个全局 T（多个重复梁段合并为一个大 T）；push0 没有上一次，T 不存在，只做段内补全。
/// </description></item>
/// </list>
/// </summary>
public static class SteelBeamDerivedCalculator
{
    /// <summary>一个梁段做刚体配准所需的最少上传点数（≥3 时用完整刚体拟合）。</summary>
    public const int MinSegmentPoints = 3;

    /// <summary>
    /// 跨批次配准（上一批→本批全局 T）所需的最少公共点数。
    /// 2026-11-06 放宽：3 → 2；2 个公共点的绕连线轴旋转不唯一，SVD 取确定性最小旋转解，精度下降但流程可走通。
    /// </summary>
    public const int MinRegistrationPoints = 2;

    /// <summary>本批上传的实测点，带其对应的理论（设计）坐标，供逐段补全使用。</summary>
    /// <remarks><c>BatchID</c>：实测行所属批次。同一 PushCount 多批次按点合并时，
    /// 上传点需保留各自真实批次，避免全部误标为最新批。</remarks>
    public sealed record MeasuredPoint(
        int TheoreticalID, string PointCode, string SegmentNo,
        decimal DesignX, decimal DesignY, decimal DesignZ,
        decimal MeasuredX, decimal MeasuredY, decimal MeasuredZ,
        decimal Weight, int MeasuredID, int BatchID);

    /// <summary>理论（设计）测点，用于识别段内缺失点。</summary>
    public sealed record TheoryPoint(
        int TheoreticalID, string PointCode, string SegmentNo,
        decimal DesignX, decimal DesignY, decimal DesignZ, decimal Weight);

    /// <summary>
    /// 滚动快照中的一行：一个测点的最新坐标 + 来源标记。
    /// <para><c>MeasuredID &gt; 0</c>：该点至少被实测过一次；<c>MeasuredID = 0</c>：纯补全值。</para>
    /// <para><c>BatchID</c>：实测/搬运点为本批 batch.ID；纯补全点为 0。</para>
    /// </summary>
    public sealed record SnapshotPoint(
        int TheoreticalID, string PointCode, string SegmentNo,
        decimal X, decimal Y, decimal Z, decimal Weight,
        int MeasuredID, int BatchID, int SourcePushCount);

    /// <summary>推算结果。</summary>
    public sealed class Result
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public List<SnapshotPoint> Rows { get; init; } = [];

        public static Result Fail(string message) => new() { Success = false, ErrorMessage = message };
        public static Result Ok(List<SnapshotPoint> rows) => new() { Success = true, Rows = rows };
    }

    /// <summary>
    /// 计算「本批顶推之后」的滚动快照。
    /// </summary>
    /// <param name="pushCount">本批顶推次数（0 起）。</param>
    /// <param name="batchID">本批 batch.ID，写到实测/搬运行的 BatchID。</param>
    /// <param name="currentMeasured">本批真实上传的实测点（只含当前 batch）。</param>
    /// <param name="previousSnapshot">上一次顶推后的滚动快照（push0 传空集合）。</param>
    /// <param name="allTheory">本钢梁全部理论测点（用于逐段补全与出胎段识别）。</param>
    /// <param name="lang">当前请求语言，用于双语错误文案。</param>
    public static Result BuildSnapshot(
        int pushCount,
        int batchID,
        IReadOnlyList<MeasuredPoint> currentMeasured,
        IReadOnlyList<SnapshotPoint> previousSnapshot,
        IReadOnlyList<TheoryPoint> allTheory,
        string? lang)
    {
        ArgumentNullException.ThrowIfNull(currentMeasured);
        ArgumentNullException.ThrowIfNull(previousSnapshot);
        ArgumentNullException.ThrowIfNull(allTheory);

        var theoryBySegment = allTheory
            .GroupBy(a => a.SegmentNo, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
        var currentBySegment = currentMeasured
            .GroupBy(a => a.SegmentNo, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
        var previousBySegment = previousSnapshot
            .GroupBy(a => a.SegmentNo, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
        // 是否首次实测：无上一批快照（push=0）。首次段内补全每段至少 2 点，后续至少 1 点
        var hasPrevious = previousSnapshot.Count > 0;

        // 1. 逐段补全：对每个本批 touched 段，产出「本批完整集」
        var currentCompleteBySegment = new Dictionary<string, List<SnapshotPoint>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (segmentNo, uploaded) in currentBySegment)
        {
            if (!theoryBySegment.TryGetValue(segmentNo, out var theory))
                return Result.Fail(BizMessages.SteelDerivedSegmentTheoryMissing(lang, segmentNo));

            var uploadedIds = uploaded.Select(a => a.TheoreticalID).ToHashSet();
            var missing = theory.Where(a => !uploadedIds.Contains(a.TheoreticalID)).ToList();

            var complete = new List<SnapshotPoint>(uploaded.Count + missing.Count);

            // 本批上传点：真实实测，MeasuredID = 本批 measured.ID
            foreach (var u in uploaded)
            {
                complete.Add(new SnapshotPoint(
                    u.TheoreticalID, u.PointCode, segmentNo,
                    Round4(u.MeasuredX), Round4(u.MeasuredY), Round4(u.MeasuredZ),
                    u.Weight, u.MeasuredID, u.BatchID, pushCount));
            }

            // 段内缺失点：按本段上传点推算「本次完整集」中的补全坐标。
            // 注意：这里用本段自己的 P/Q，不用整批其他段的点。
            // 2026-11-06 放宽：首次实测（无上一批快照）每段至少 2 点——Gescha SVD 对 2 点给出
            // 确定性最小旋转解（绕连线轴旋转不唯一，精度下降但结果可复现）；
            // 后续批次每段至少 1 点——2 点仍走 Gescha，1 点退化为平移补全（假设段内无旋转）。
            if (missing.Count > 0)
            {
                var minPoints = hasPrevious ? 1 : 2;
                if (uploaded.Count < minPoints)
                    return Result.Fail(BizMessages.SteelDerivedSegmentPointsInsufficient(lang, segmentNo, uploaded.Count, minPoints));

                var predicted = uploaded.Count >= 2
                    ? CompleteBySegment(uploaded, missing)
                    : CompleteByTranslation(uploaded, missing);
                for (var i = 0; i < missing.Count; i++)
                {
                    var m = missing[i];
                    complete.Add(new SnapshotPoint(
                        m.TheoreticalID, m.PointCode, segmentNo,
                        predicted[i].X, predicted[i].Y, predicted[i].Z,
                        m.Weight, 0, 0, pushCount));
                }
            }

            currentCompleteBySegment[segmentNo] = complete
                .OrderBy(a => a.TheoreticalID)
                .ToList();
        }

        // 2. 找重复梁段，用「上次完整集 vs 本次完整集」的同名测点拟合一个全局 T。
        //    重复段判定（2026-11-06 放宽）：上一次该段 MeasuredID != 0 的点 >= 2，且本次该段上传点 >= 2。
        var fitP = NewMatrix();
        var fitQ = NewMatrix();
        var fitR = NewMatrix();
        foreach (var (segmentNo, currentComplete) in currentCompleteBySegment)
        {
            if (!previousBySegment.TryGetValue(segmentNo, out var previousComplete)) continue;

            var previousMeasuredCount = previousComplete.Count(a => a.MeasuredID != 0);
            var currentUploadedCount = currentBySegment[segmentNo].Count;
            if (previousMeasuredCount < MinRegistrationPoints || currentUploadedCount < MinRegistrationPoints) continue;

            var previousMap = previousComplete
                .GroupBy(a => a.TheoreticalID)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var curr in currentComplete)
            {
                if (!previousMap.TryGetValue(curr.TheoreticalID, out var prev)) continue;
                AddPoint(fitP, fitQ, fitR, prev.X, prev.Y, prev.Z, curr.X, curr.Y, curr.Z, curr.Weight);
            }
        }

        // 3. 收集需要搬运的点：
        //    ① touched 段里「以前实测过、本批未上传」的缺失点；
        //    ② 本批完全未上传的出胎段（上一次快照里、但不在本批 touched 段中）。
        var touchedSegments = currentBySegment.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var targets = new List<SnapshotPoint>();
        foreach (var (segmentNo, previousPoints) in previousBySegment)
        {
            if (touchedSegments.Contains(segmentNo))
            {
                var uploadedIds = currentBySegment[segmentNo].Select(a => a.TheoreticalID).ToHashSet();
                foreach (var p in previousPoints)
                {
                    if (uploadedIds.Contains(p.TheoreticalID)) continue;
                    if (p.MeasuredID != 0) targets.Add(p);
                }
            }
            else
            {
                targets.AddRange(previousPoints);
            }
        }

        // 4. 用 T 搬运：P/Q 是重复段的完整集，D 是待搬运点在上一次快照中的坐标。
        var transported = new Dictionary<int, (decimal X, decimal Y, decimal Z)>();
        if (targets.Count > 0)
        {
            if (fitP[0].Count < MinRegistrationPoints)
                return Result.Fail(BizMessages.SteelDerivedNoRepeatedSegment(lang, pushCount));

            var d = NewMatrix();
            foreach (var t in targets)
            {
                d[0].Add(t.X);
                d[1].Add(t.Y);
                d[2].Add(t.Z);
            }

            var transformed = SteelGirderAlignment.Gescha(P: fitP, Q: fitQ, D: d, R: fitR, scaleS: 1.0m);
            for (var i = 0; i < targets.Count; i++)
            {
                transported[targets[i].TheoreticalID] = (
                    Round4(transformed[0][i]),
                    Round4(transformed[1][i]),
                    Round4(transformed[2][i]));
            }
        }

        // 5. 组装本批之后的滚动快照
        var rows = new List<SnapshotPoint>();

        // 5.1 touched 段：上传点取实测；缺失点按「以前实测过 -> 搬运，否则 -> 本次补全」二选一
        foreach (var (segmentNo, currentComplete) in currentCompleteBySegment)
        {
            var previousMap = previousBySegment.TryGetValue(segmentNo, out var previousPoints)
                ? previousPoints.GroupBy(a => a.TheoreticalID).ToDictionary(g => g.Key, g => g.First())
                : new Dictionary<int, SnapshotPoint>();
            var uploadedIds = currentBySegment[segmentNo].Select(a => a.TheoreticalID).ToHashSet();

            // 本批上传点
            foreach (var u in currentBySegment[segmentNo])
            {
                rows.Add(new SnapshotPoint(
                    u.TheoreticalID, u.PointCode, segmentNo,
                    Round4(u.MeasuredX), Round4(u.MeasuredY), Round4(u.MeasuredZ),
                    u.Weight, u.MeasuredID, u.BatchID, pushCount));
            }

            // 段内缺失点
            foreach (var curr in currentComplete)
            {
                if (uploadedIds.Contains(curr.TheoreticalID)) continue;

                if (previousMap.TryGetValue(curr.TheoreticalID, out var prev) && prev.MeasuredID != 0)
                {
                    var xyz = transported[curr.TheoreticalID];
                    rows.Add(new SnapshotPoint(
                        curr.TheoreticalID, curr.PointCode, segmentNo,
                        xyz.X, xyz.Y, xyz.Z,
                        curr.Weight, prev.MeasuredID, batchID, pushCount - 1));
                }
                else
                {
                    rows.Add(new SnapshotPoint(
                        curr.TheoreticalID, curr.PointCode, segmentNo,
                        curr.X, curr.Y, curr.Z,
                        curr.Weight, 0, 0, pushCount));
                }
            }
        }

        // 5.2 出胎段：上一次快照里的全部点用 T 搬运；MeasuredID 保留，BatchID 按是否有实测来源决定
        foreach (var (segmentNo, previousPoints) in previousBySegment)
        {
            if (touchedSegments.Contains(segmentNo)) continue;

            foreach (var p in previousPoints)
            {
                var xyz = transported[p.TheoreticalID];
                rows.Add(new SnapshotPoint(
                    p.TheoreticalID, p.PointCode, segmentNo,
                    xyz.X, xyz.Y, xyz.Z,
                    p.Weight, p.MeasuredID,
                    p.MeasuredID != 0 ? batchID : 0,
                    pushCount - 1));
            }
        }

        // 6. 按 TheoreticalID 去重（同一测点只保留一行），并按 ID 排序，便于落库/对比
        var final = rows
            .GroupBy(a => a.TheoreticalID)
            .Select(g => g.Last())
            .OrderBy(a => a.TheoreticalID)
            .ToList();
        return Result.Ok(final);
    }

    /// <summary>
    /// 逐段补全：P=本段本次上传点的设计坐标，Q=本段本次上传点的实测坐标，
    /// D=本段缺失点的设计坐标，R=权重；Gescha 得到缺失点的推测实测坐标。
    /// </summary>
    private static List<(decimal X, decimal Y, decimal Z)> CompleteBySegment(
        IReadOnlyList<MeasuredPoint> uploaded,
        IReadOnlyList<TheoryPoint> missing)
    {
        var p = NewMatrix();
        var q = NewMatrix();
        var r = NewMatrix();
        foreach (var u in uploaded.OrderBy(a => a.TheoreticalID))
        {
            AddPoint(p, q, r, u.DesignX, u.DesignY, u.DesignZ,
                u.MeasuredX, u.MeasuredY, u.MeasuredZ, u.Weight);
        }

        var d = NewMatrix();
        foreach (var m in missing)
        {
            d[0].Add(m.DesignX);
            d[1].Add(m.DesignY);
            d[2].Add(m.DesignZ);
        }

        var transformed = SteelGirderAlignment.Gescha(P: p, Q: q, D: d, R: r, scaleS: 1.0m);
        var result = new List<(decimal X, decimal Y, decimal Z)>(missing.Count);
        for (var i = 0; i < missing.Count; i++)
        {
            result.Add((
                Round4(transformed[0][i]),
                Round4(transformed[1][i]),
                Round4(transformed[2][i])));
        }
        return result;
    }

    /// <summary>
    /// 单点平移补全（2026-11-06 新增，仅后续批次每段只上传 1 点时使用）：
    /// 假设段内无旋转，用唯一上传点「实测−设计」的平移量直接推算本段缺失点。
    /// 精度最低，仅用于推进顶推流程；待该段有更多实测点后由滚动快照自然覆盖。
    /// </summary>
    private static List<(decimal X, decimal Y, decimal Z)> CompleteByTranslation(
        IReadOnlyList<MeasuredPoint> uploaded, IReadOnlyList<TheoryPoint> missing)
    {
        var anchor = uploaded.OrderBy(a => a.TheoreticalID).First();
        var dx = anchor.MeasuredX - anchor.DesignX;
        var dy = anchor.MeasuredY - anchor.DesignY;
        var dz = anchor.MeasuredZ - anchor.DesignZ;
        // 保持与传入 missing 相同的顺序：调用方按下标一一配对
        return missing
            .Select(m => (Round4(m.DesignX + dx), Round4(m.DesignY + dy), Round4(m.DesignZ + dz)))
            .ToList();
    }

    private static List<List<decimal>> NewMatrix() => [new(), new(), new()];

    private static void AddPoint(
        List<List<decimal>> p, List<List<decimal>> q, List<List<decimal>> r,
        decimal px, decimal py, decimal pz,
        decimal qx, decimal qy, decimal qz,
        decimal weight)
    {
        p[0].Add(px); p[1].Add(py); p[2].Add(pz);
        q[0].Add(qx); q[1].Add(qy); q[2].Add(qz);
        r[0].Add(weight); r[1].Add(weight); r[2].Add(weight);
    }

    private static decimal Round4(decimal v) => Math.Round(v, 4, MidpointRounding.AwayFromZero);
}
