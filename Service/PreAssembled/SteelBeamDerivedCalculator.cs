using Service.PreAssembled.BridgeSegmentAlignment;

namespace Service.PreAssembled;

/// <summary>
/// 钢梁「已推出胎架梁段」当批坐标推算器。
/// <para>
/// 业务背景：钢梁顶推是单向的。第 k 次顶推时，已推出胎架的梁段无法再实测，
/// 但计算第 k+1 段待拼装数据需要「同一批次、同一姿态下的完整实测点集 Q」。
/// 解决办法是用仍在胎架上的公共测点求出「上一批次 → 本批次」的刚体变换 T，
/// 再把上一批次的完整点集里本批次没测到的点整体变换过来，即本批次的推算坐标。
/// </para>
/// <para>
/// 递推式：完整集(k) = 实测(k) ∪ 推算(k)；
/// 推算(k) = T_{k-1→k} ( 完整集(k-1) − 实测(k) )，不区分来源是实测还是推算。
/// 配准公共点只用「实测(k-1) ∩ 实测(k)」，推算值不参与配准，避免累积误差被二次吸收。
/// </para>
/// </summary>
public static class SteelBeamDerivedCalculator
{
    /// <summary>刚体配准所需的最少公共点数量（3 点确定一个不共线的刚体姿态）。</summary>
    public const int MinCommonPoints = 3;

    /// <summary>参与推算的一个测点。</summary>
    public sealed record Point(int TheoreticalID, string PointCode, string SegmentNo, decimal X, decimal Y, decimal Z, decimal Weight);

    /// <summary>推算结果。</summary>
    public sealed class Result
    {
        /// <summary>是否推算成功。</summary>
        public bool Success { get; init; }
        /// <summary>失败原因（已双语）；成功时为 null。</summary>
        public string? ErrorMessage { get; init; }
        /// <summary>推算出的本批次坐标行。</summary>
        public List<Point> Rows { get; init; } = [];

        public static Result Fail(string message) => new() { Success = false, ErrorMessage = message };
        public static Result Ok(List<Point> rows) => new() { Success = true, Rows = rows };
    }

    /// <summary>
    /// 推算本批次的已推出梁段坐标。
    /// </summary>
    /// <param name="prevFullSet">上一批次的完整点集（实测 ∪ 推算）。</param>
    /// <param name="prevMeasured">上一批次的实测点集（只用于配准取公共点）。</param>
    /// <param name="currMeasured">本批次的实测点集（只用于配准取公共点）。</param>
    /// <param name="pushCount">本批次顶推次数，仅用于错误文案。</param>
    /// <param name="lang">当前请求语言。</param>
    /// <returns>推算结果；公共点不足时返回失败，由调用方决定回滚。</returns>
    public static Result Derive(
        IReadOnlyList<Point> prevFullSet,
        IReadOnlyList<Point> prevMeasured,
        IReadOnlyList<Point> currMeasured,
        int pushCount,
        string? lang)
    {
        ArgumentNullException.ThrowIfNull(prevFullSet);
        ArgumentNullException.ThrowIfNull(prevMeasured);
        ArgumentNullException.ThrowIfNull(currMeasured);

        // 1. 配准公共点：上一批次实测 ∩ 本批次实测（推算值不参与配准）
        var prevMeasuredMap = prevMeasured
            .GroupBy(a => a.TheoreticalID)
            .ToDictionary(g => g.Key, g => g.First());
        var currMeasuredMap = currMeasured
            .GroupBy(a => a.TheoreticalID)
            .ToDictionary(g => g.Key, g => g.First());
        var commonIds = currMeasuredMap.Keys
            .Where(prevMeasuredMap.ContainsKey)
            .OrderBy(a => a)
            .ToList();
        if (commonIds.Count < MinCommonPoints)
            return Result.Fail(BizMessages.SteelDerivedCommonPointsInsufficient(lang, pushCount, commonIds.Count));

        // 2. 待变换点集：上一批次完整集 − 本批次实测
        var prevFullMap = prevFullSet
            .GroupBy(a => a.TheoreticalID)
            .ToDictionary(g => g.Key, g => g.First());
        var targetIds = prevFullMap.Keys
            .Where(a => !currMeasuredMap.ContainsKey(a))
            .OrderBy(a => a)
            .ToList();
        if (targetIds.Count == 0) return Result.Ok([]);

        // 3. 组装 P（上一批次公共点坐标）/ Q（本批次公共点坐标）/ R（逐点权重，XYZ 共用）/ D（待变换点）
        var p = new List<List<decimal>>() { new(), new(), new() };
        var q = new List<List<decimal>>() { new(), new(), new() };
        var r = new List<List<decimal>>() { new(), new(), new() };
        foreach (var id in commonIds)
        {
            var prev = prevMeasuredMap[id];
            var curr = currMeasuredMap[id];
            p[0].Add(prev.X); p[1].Add(prev.Y); p[2].Add(prev.Z);
            q[0].Add(curr.X); q[1].Add(curr.Y); q[2].Add(curr.Z);
            r[0].Add(curr.Weight); r[1].Add(curr.Weight); r[2].Add(curr.Weight);
        }
        var d = new List<List<decimal>>() { new(), new(), new() };
        foreach (var id in targetIds)
        {
            var src = prevFullMap[id];
            d[0].Add(src.X); d[1].Add(src.Y); d[2].Add(src.Z);
        }

        // 4. 刚体变换：顶推是纯刚体运动，scale 固定 1.0
        var transformed = SteelGirderAlignment.Gescha(P: p, Q: q, D: d, R: r, scaleS: 1.0m);

        var rows = new List<Point>(targetIds.Count);
        for (var i = 0; i < targetIds.Count; i++)
        {
            var src = prevFullMap[targetIds[i]];
            rows.Add(new Point(
                src.TheoreticalID,
                src.PointCode,
                src.SegmentNo,
                Round4(transformed[0][i]),
                Round4(transformed[1][i]),
                Round4(transformed[2][i]),
                src.Weight));
        }
        return Result.Ok(rows);
    }

    private static decimal Round4(decimal v) => Math.Round(v, 4, MidpointRounding.AwayFromZero);
}
