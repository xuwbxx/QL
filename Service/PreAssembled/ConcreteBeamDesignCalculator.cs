using Service.PreAssembled.Reports;

namespace Service.PreAssembled;

public sealed class ConcreteBeamDesignCalculationResult
{
    public List<ConcreteBeamDesignCoordinateImportRow> Rows { get; } = [];
    public List<ConcreteBeamImportError> Errors { get; } = [];
}

public static class ConcreteBeamDesignCalculator
{
    public static List<ConcreteBeamCamberImportRow> FillCamberRows(
        IEnumerable<ConcreteBeamCamberImportRow> sourceRows) =>
        sourceRows.Select(row => row with
        {
            ZE = Round(row.ZE ?? row.ZC),
            ZC = Round(row.ZC),
            ZW = Round(row.ZW ?? row.ZC)
        }).ToList();

    /// <summary>
    /// 验证节点名称是否重复
    /// </summary>
    /// <param name="rows"></param>
    /// <returns></returns>
    public static List<ConcreteBeamImportError> ValidateNodeNames(
        IReadOnlyList<ConcreteBeamDesignCoordinateImportRow> rows)
    {
        var errors = new List<ConcreteBeamImportError>();
        foreach (var r in rows)
            if (string.IsNullOrWhiteSpace(r.NodeName))
                errors.Add(new(r.ExcelRow, string.Empty, "节点名称", string.Empty, "节点名称不能为空"));

        foreach (var g in rows.Where(r => !string.IsNullOrWhiteSpace(r.NodeName))
                              .GroupBy(r => r.NodeName, StringComparer.Ordinal)
                              .Where(g => g.Count() > 1))
            errors.Add(new(g.First().ExcelRow, g.Key, "节点名称", g.Key,
                $"节点名称重复，共 {g.Count()} 条"));
        return errors;
    }


    public static List<ConcreteBeamImportError> ValidateNodeSequence(
        IReadOnlyList<ConcreteBeamDesignCoordinateImportRow> designRows,
        IReadOnlyList<ConcreteBeamCamberImportRow> camberRows)
    {
        var errors = new List<ConcreteBeamImportError>();
        if (designRows.Count != camberRows.Count)
        {
            errors.Add(new(0, string.Empty, "节点数量", camberRows.Count.ToString(),
                $"预拱度节点数量必须与设计坐标一致，应为{designRows.Count}条"));
            return errors;
        }

        for (var index = 0; index < designRows.Count; index++)
        {
            if (string.Equals(designRows[index].NodeName, camberRows[index].NodeName,
                    StringComparison.Ordinal)) continue;
            errors.Add(new(camberRows[index].ExcelRow, camberRows[index].NodeName, "节点名称",
                camberRows[index].NodeName,
                $"节点名称及排列顺序必须与设计坐标一致，应为{designRows[index].NodeName}"));
        }
        return errors;
    }

    public static ConcreteBeamDesignCalculationResult Calculate(
        IReadOnlyList<ConcreteBeamDesignCoordinateImportRow> sourceRows,
        decimal targetDistance,
        IReadOnlyList<ConcreteBeamCamberImportRow>? camberRows,
        bool elevationIncludesCamber)
    {
        var result = new ConcreteBeamDesignCalculationResult();
        // 验证NodeName是否唯一在CastingGroup下
        result.Errors.AddRange(ValidateNodeNames(sourceRows));   
        if (result.Errors.Count > 0) return result;

        IReadOnlyList<ConcreteBeamCamberImportRow>? filledCamber = null;
        if (!elevationIncludesCamber)
        {
            if (camberRows == null)
            {
                result.Errors.Add(new(0, string.Empty, "预拱度", string.Empty, "请先导入预拱度"));
                return result;
            }

            result.Errors.AddRange(ValidateNodeSequence(sourceRows, camberRows));
            if (result.Errors.Count > 0) return result;
            filledCamber = FillCamberRows(camberRows);
        }

        for (var index = 0; index < sourceRows.Count; index++)
        {
            var source = sourceRows[index];
            var camber = filledCamber?[index];
            var ze = source.ZE + (camber?.ZE ?? 0m);
            var zc = source.ZC + (camber?.ZC ?? 0m);
            var zw = source.ZW + (camber?.ZW ?? 0m);

            var eastWithoutCamber = NormalizeEndpoint(source, "E", source.XE, source.YE, source.ZE,
                source.XC, source.YC, source.ZC, targetDistance, result.Errors);
            var westWithoutCamber = NormalizeEndpoint(source, "W", source.XW, source.YW, source.ZW,
                source.XC, source.YC, source.ZC, targetDistance, result.Errors);
            if (!eastWithoutCamber.HasValue || !westWithoutCamber.HasValue) continue;

            var east = NormalizeEndpoint(source, "E", source.XE, source.YE, ze,
                source.XC, source.YC, zc, targetDistance, result.Errors);
            var west = NormalizeEndpoint(source, "W", source.XW, source.YW, zw,
                source.XC, source.YC, zc, targetDistance, result.Errors);
            if (!east.HasValue || !west.HasValue) continue;

            result.Rows.Add(new(source.ExcelRow, source.NodeName,
                east.Value.X, east.Value.Y, east.Value.Z,
                Round(source.XC), Round(source.YC), Round(zc),
                west.Value.X, west.Value.Y, west.Value.Z)
            {
                ZEWithoutCamber = elevationIncludesCamber ? east.Value.Z : eastWithoutCamber.Value.Z,
                ZCWithoutCamber = elevationIncludesCamber ? Round(zc) : Round(source.ZC),
                ZWWithoutCamber = elevationIncludesCamber ? west.Value.Z : westWithoutCamber.Value.Z
            });
        }
        return result;
    }

    private static (decimal X, decimal Y, decimal Z)? NormalizeEndpoint(
        ConcreteBeamDesignCoordinateImportRow row, string endpoint,
        decimal x, decimal y, decimal z,
        decimal cx, decimal cy, decimal cz,
        decimal targetDistance,
        List<ConcreteBeamImportError> errors)
    {
        var vx = x - cx;
        var vy = y - cy;
        var vz = z - cz;
        if (vx == 0m && vy == 0m && vz == 0m)
        {
            errors.Add(new(row.ExcelRow, row.NodeName, $"{endpoint}点坐标",
                FormattableString.Invariant($"X={x}; Y={y}; Z={z}"),
                $"{endpoint}点与C点重合，距离b为0，无法归一化"));
            return null;
        }

        var dx = (double)vx;
        var dy = (double)vy;
        var dz = (double)vz;
        var distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        var scale = (double)targetDistance / distance;
        return (
            Round((double)cx + scale * dx),
            Round((double)cy + scale * dy),
            Round((double)cz + scale * dz));
    }

    private static decimal Round(decimal value) =>
        Math.Round(value, 4, MidpointRounding.AwayFromZero);

    private static decimal Round(double value) =>
        Round(Convert.ToDecimal(value));
}
