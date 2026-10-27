using DataFactory.Factory;
using DataFactory.KingBase;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Model.tech.QL.DTO.BizProject;
using Service.PreAssembled.BridgeSegmentAlignment;
using Service.PreAssembled.Reports;
using System.Collections;
using System.Globalization;
using System.Security.Claims;
using Tool;

namespace Service.PreAssembled;

public class ConcreteBeamService : ServiceBase<biz_project_bridge_castingGroup>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ConcreteBeamImportPreviewStore _previewStore;

    public ConcreteBeamService(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory,
        IHttpContextAccessor httpContextAccessor,
        ConcreteBeamImportPreviewStore previewStore) : base(qlUowFactory, httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _previewStore = previewStore;
    }

    /// <summary>
    /// 当前请求的语言（zh-CN / en），供所有返回给前端的文案取用。
    /// 来源：前端 axios 拦截器写入的 i18n 请求头，见 <see cref="RequestLanguage"/>。
    /// </summary>
    private string Lang => RequestLanguage.Resolve(_httpContextAccessor);

    public async Task<ConcreteBeamPagedResultDTO> List(ConcreteBeamQueryDTO req)
    {
        var allowed = await GetAllowedProjectIds();
        var query = from cg in Db.Query<biz_project_bridge_castingGroup>().Where(a => a.Status != -1)
                    join bridge in Db.Query<biz_project_bridge>().Where(a => a.Status != -1 && a.BeamType == 1) on cg.BridgeID equals bridge.ID
                    join proj in Db.Query<biz_project>().Where(a => a.Status != -1) on bridge.ProjID equals proj.ID
                    select new ConcreteBeamItemDTO
                    {
                        ID = cg.ID,
                        GirderID = cg.BridgeID,
                        ProjID = bridge.ProjID,
                        ProjectName = proj.Name,
                        BridgeName = bridge.Name,
                        CastingGroupName = cg.Name,
                        ManagerName = proj.ManagerName ?? string.Empty,
                        CreatedBy = cg.CreatedBy ?? string.Empty,
                        CreatedTime = cg.CreatedTime
                    };
        if (allowed != null) query = query.Where(a => allowed.Contains(a.ProjID));
        if (req.ProjID is > 0) query = query.Where(a => a.ProjID == req.ProjID);
        if (!string.IsNullOrWhiteSpace(req.CastingGroupName)) query = query.Where(a => a.CastingGroupName.Contains(req.CastingGroupName.Trim()));
        var total = await query.CountAsync();
        var page = Math.Max(req.PageIndex, 1); var size = NormalizePageSize(req.PageSize);
        var list = await query.OrderBy(a => a.ProjectName).ThenBy(a => a.BridgeName).ThenBy(a => a.CastingGroupName)
            .Skip((page - 1) * size).Take(size).ToListAsync();
        return new() { List = list, Total = total };
    }

    public async Task<List<object>> ProjectOptions()
    {
        var allowed = await GetAllowedProjectIds();
        var query = from cg in Db.Query<biz_project_bridge_castingGroup>().Where(a => a.Status != -1)
                    join bridge in Db.Query<biz_project_bridge>().Where(a => a.Status != -1 && a.BeamType == 1) on cg.BridgeID equals bridge.ID
                    join proj in Db.Query<biz_project>().Where(a => a.Status != -1) on bridge.ProjID equals proj.ID
                    select new { proj.ID, proj.Name };
        if (allowed != null) query = query.Where(a => allowed.Contains(a.ID));
        var rows = await query.Distinct().OrderBy(a => a.Name).ToListAsync();
        return rows.Select(a => (object)new { id = a.ID, name = a.Name }).ToList();
    }

    public async Task<ConcreteBeamConfigDTO> GetConfig(int castingGroupID)
    {
        var context = await EnsureCastingGroupAccess(castingGroupID);
        var hasSurveyData = await HasSurveyData(castingGroupID);
        var config = await Db.Query<biz_concrete_beam_config>().Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
            .OrderByDescending(a => a.ID).FirstOrDefaultAsync();
        var hasConfirmedDesignCoordinates = await Db.Query<biz_concrete_beam_design_coordinate_source>()
            .AnyAsync(a => a.CastingGroupID == castingGroupID && a.Status != -1);
        var hasCalculatedDesignCoordinates = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .AnyAsync(a => a.CastingGroupID == castingGroupID && a.Status != -1);
        return new()
        {
            CastingGroupID = castingGroupID,
            ProjectName = context.ProjectName,
            BridgeName = context.BridgeName,
            CastingGroupName = context.CastingGroupName,
            ManagerName = context.ManagerName,
            OffsetA = config?.OffsetA,
            OffsetB = config?.OffsetB,
            OffsetC = config?.OffsetC,
            ReferenceElevation = config?.OffsetH,
            IsConfigured = config != null && config.OffsetA > 0 && config.OffsetB > 0 && config.OffsetC > 0,
            HasConfirmedDesignCoordinates = hasConfirmedDesignCoordinates,
            HasCalculatedDesignCoordinates = hasCalculatedDesignCoordinates,
            HasSurveyData = hasSurveyData,
            ElevationIncludesCamber = config?.ElevationIncludesCamber ?? false,
            Normalize = config?.Normalize ?? false,
            CreatedBy = context.CreatedBy,
            CreatedTime = context.CreatedTime,
            UpdatedTime = config?.UpdatedTime
        };
    }

    public async Task<ConcreteBeamConfigDTO> SaveConfig(ConcreteBeamConfigSaveDTO req)
    {
        if (req.OffsetA <= 0) throw new InvalidOperationException(BizMessages.OffsetMustBePositive(Lang, "A"));
        if (req.OffsetB <= 0) throw new InvalidOperationException(BizMessages.OffsetMustBePositive(Lang, "B"));
        if (req.OffsetC <= 0) throw new InvalidOperationException(BizMessages.OffsetMustBePositive(Lang, "C"));
        if (!req.ReferenceElevation.HasValue) throw new InvalidOperationException(BizMessages.ReferenceElevationRequired(Lang));
        req.OffsetA = Math.Round(req.OffsetA, 4, MidpointRounding.AwayFromZero);
        req.OffsetB = Math.Round(req.OffsetB, 4, MidpointRounding.AwayFromZero);
        req.OffsetC = Math.Round(req.OffsetC, 4, MidpointRounding.AwayFromZero);
        req.ReferenceElevation = Math.Round(req.ReferenceElevation.Value, 4, MidpointRounding.AwayFromZero);
        // [Shen, Lin] 不再验证，而是转化为4位小数
        //ValidateFourDecimals(req.OffsetA, "A"); ValidateFourDecimals(req.OffsetB, "B");
        //ValidateFourDecimals(req.OffsetC, "C"); ValidateFourDecimals(req.ReferenceElevation.Value, "浇筑参考高程");
        var context = await EnsureCastingGroupAccess(req.CastingGroupID);
        var entity = await Db.Query<biz_concrete_beam_config>().Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1)
            .OrderByDescending(a => a.ID).FirstOrDefaultAsync();
        var previousA = entity?.OffsetA;
        var previousB = entity?.OffsetB;
        var previousC = entity?.OffsetC;
        var previousH = entity?.OffsetH;
        // 已有梁段实测填报后，A/B/C/H 不再允许修改：四个参数全部进入线形算法，
        // 一旦改动，既往梁段"理论值"的口径会被静默改写，已签字归档的实测数据将失去可比性
        // （原系统是"改了就作废下游重填"，业务上不可接受，故改为直接拒绝）。
        // 值完全一致的提交不算修改，直接返回当前配置，避免"没改任何内容却被拒绝"的误报。
        if (await HasSurveyData(req.CastingGroupID))
        {
            var unchanged = previousA.HasValue && previousB.HasValue && previousC.HasValue && previousH.HasValue
                && previousA.Value == req.OffsetA && previousB.Value == req.OffsetB
                && previousC.Value == req.OffsetC && previousH.Value == req.ReferenceElevation.Value;
            if (!unchanged) throw new InvalidOperationException(ConcreteBeamWorkflowRules.SurveyLockedConfigMessage(Lang));
            return await GetConfig(req.CastingGroupID);
        }
        var now = DateTime.Now; var account = GetCurrentAccount();
        if (entity == null)
        {
            entity = new()
            {
                ProjID = context.ProjID,
                GirderID = context.GirderID,
                CastingGroupID = req.CastingGroupID,
                Status = 0,
                CreatedBy = account,
                CreatedTime = now
            };
            Db.GetRepository<biz_concrete_beam_config>().Add(entity);
        }
        else { entity.UpdatedBy = account; entity.UpdatedTime = now; }
        entity.OffsetA = req.OffsetA; entity.OffsetB = req.OffsetB; entity.OffsetC = req.OffsetC;
        entity.OffsetH = req.ReferenceElevation.Value;
        // A/B/C/H 任一变化都会让已算出的浇筑坐标失去基准（四个参数全部进入线形算法），必须作废下游
        var invalidateAsCast = ConcreteBeamWorkflowRules.ShouldInvalidateAsCastAfterConfigChange(
            previousA, previousB, previousC, previousH,
            req.OffsetA, req.OffsetB, req.OffsetC, req.ReferenceElevation.Value);
        var invalidatedCount = 0;
        var messages = new List<string>();
        try
        {
            Db.BeginTransaction();
            if (invalidateAsCast)
                invalidatedCount = await InvalidateAsCastData(req.CastingGroupID, account, now);
            if (previousB.HasValue && previousB.Value != req.OffsetB)
            {
                var sourceRows = await LoadDesignSourceRows(req.CastingGroupID);
                var oldCalculations = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
                    .Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1).ToListAsync();
                SoftDelete(oldCalculations, account, now);
                if (sourceRows.Count > 0)
                {
                    var include = entity.ElevationIncludesCamber;
                    var camberRows = include ? null : await LoadCamberCalculationRows(req.CastingGroupID);
                    var canReuse = !include && camberRows != null && camberRows.Count > 0 &&
                        ConcreteBeamDesignCalculator.ValidateNodeSequence(sourceRows, camberRows).Count == 0;
                    List<ConcreteBeamDesignCoordinateImportRow> designCalculationRows;
                    if (entity.Normalize)
                    {
                        var camberForCalc = include ? null : (canReuse ? camberRows : ZeroCamberRows(sourceRows));
                        var calculation = ConcreteBeamDesignCalculator.Calculate(sourceRows, req.OffsetB, camberForCalc, include);
                        if (calculation.Errors.Count > 0)
                            throw new InvalidOperationException(calculation.Errors[0].Reason);
                        designCalculationRows = calculation.Rows;
                    }
                    else
                    {
                        designCalculationRows = !include && canReuse
                            ? ConcreteBeamDesignCalculator.ApplyCamberToSource(sourceRows, camberRows!)
                            : sourceRows.ToList();
                    }
                    Db.GetRepository<biz_concrete_beam_design_coordinate_calculation>().AddList(
                        designCalculationRows.Select(a => ToDesignCalculationEntity(a, context,
                            req.CastingGroupID, account, now)).ToList());
                }
            }
            await Db.SaveAsync();
            Db.CommitTransaction();
        }
        catch
        {
            Db.RollbackTransaction();
            throw;
        }
        if (invalidatedCount > 0) messages.Add(ConcreteBeamWorkflowRules.AsCastInvalidatedMessage(invalidatedCount, Lang));
        var response = await GetConfig(req.CastingGroupID);
        response.CalculationMessage = string.Join("；", messages);
        return response;
    }

    public async Task<ConcreteBeamPagedResultDTO<ConcreteBeamSegmentDTO>> SegmentList(ConcreteBeamDataQueryDTO req)
    {
        await EnsureCastingGroupAccess(req.CastingGroupID);
        // 梁段本身不由系统单独维护，而是由相邻端面派生：N 个端面 -> N-1 个梁段。
        // 端面顺序 = 导入时 Excel 行序 = 按 ID 升序；梁段序号即算法入参 M（节 m 跨端面 m..m+1）。
        // 先取全量再分页，避免"按页取"导致的 SegmentIndex 与实际几何序号错位。
        var all = await LoadAllSegments(req.CastingGroupID, req.UseCalculation);
        var total = all.Count;
        var page = Math.Max(req.PageIndex, 1);
        var size = NormalizePageSize(req.PageSize);
        var list = all.Skip((page - 1) * size).Take(size).ToList();
        return new() { List = list, Total = total };
    }

    /// <summary>
    /// 校验叠加预拱度后的设计坐标（calculation）与源数据（source）的端面数量与顺序完全一致。
    /// 算法入参 K 取 calculation 表，而梁段列表若按 source 派生，两表错位会导致静默算错。
    /// </summary>
    private async Task EnsureCalculationMatchesSource(int castingGroupID, List<string> calculationNodeNames)
    {
        if (calculationNodeNames.Count == 0)
            throw new InvalidOperationException(BizMessages.CalculationRequired(Lang));
        var sourceNodeNames = await Db.Query<biz_concrete_beam_design_coordinate_source>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete)
            .OrderBy(a => a.ID).Select(a => a.NodeName).ToListAsync();
        if (sourceNodeNames.Count != calculationNodeNames.Count)
            throw new InvalidOperationException(
                BizMessages.DesignSourceCountMismatch(Lang, sourceNodeNames.Count, calculationNodeNames.Count));
        for (var i = 0; i < sourceNodeNames.Count; i++)
            if (!string.Equals(sourceNodeNames[i], calculationNodeNames[i], StringComparison.Ordinal))
                throw new InvalidOperationException(
                    BizMessages.DesignSourceJointMismatch(Lang, i + 1, sourceNodeNames[i], calculationNodeNames[i]));
    }

    public async Task<ConcreteBeamPagedResultDTO<ConcreteBeamDesignCoordinateDTO>> DesignCoordinateList(ConcreteBeamDataQueryDTO req)
    {
        await EnsureCastingGroupAccess(req.CastingGroupID);
        var config = await GetSubmittedProjectConfig(req.CastingGroupID);
        var page = Math.Max(req.PageIndex, 1); var size = NormalizePageSize(req.PageSize);

        // 归一化展示：只用 source 做 K 归一化，不叠加 camber，结果不落库
        if (req.Normalized)
        {
            var query = Db.Query<biz_concrete_beam_design_coordinate_source>()
                .Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1);
            var total = await query.CountAsync();
            var sourceEntities = await query.OrderBy(a => a.ID)
                .Skip((page - 1) * size).Take(size).ToListAsync();
            var sourceRows = sourceEntities.Select((a, i) => new ConcreteBeamDesignCoordinateImportRow(
                i + 2, a.NodeName, a.XE, a.YE, a.ZE, a.XC, a.YC, a.ZC, a.XW, a.YW, a.ZW)).ToList();
            var calculation = ConcreteBeamDesignCalculator.Calculate(sourceRows, config.OffsetB, null, true);
            if (calculation.Errors.Count > 0)
                throw new InvalidOperationException(calculation.Errors[0].Reason);
            return new()
            {
                Total = total,
                List = calculation.Rows.Select(ToDesignDto).ToList()
            };
        }

        if (req.UseCalculation)
        {
            var query = Db.Query<biz_concrete_beam_design_coordinate_calculation>()
                .Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1);
            var total = await query.CountAsync();
            var list = await query.OrderBy(a => a.ID).Skip((page - 1) * size).Take(size).Select(a => new ConcreteBeamDesignCoordinateDTO
            { NodeName = a.NodeName, XE = a.XE, YE = a.YE, ZE = a.ZE, XC = a.XC, YC = a.YC, ZC = a.ZC, XW = a.XW, YW = a.YW, ZW = a.ZW }).ToListAsync();
            return new() { List = list, Total = total };
        }
        else
        {
            var query = Db.Query<biz_concrete_beam_design_coordinate_source>()
                .Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1);
            var total = await query.CountAsync();
            var list = await query.OrderBy(a => a.ID).Skip((page - 1) * size).Take(size).Select(a => new ConcreteBeamDesignCoordinateDTO
            { NodeName = a.NodeName, XE = a.XE, YE = a.YE, ZE = a.ZE, XC = a.XC, YC = a.YC, ZC = a.ZC, XW = a.XW, YW = a.YW, ZW = a.ZW }).ToListAsync();
            return new() { List = list, Total = total };
        }
    }

    /// <summary>
    /// 设计坐标「核对与预览」（手册 Coordinate Check and Preview）。
    /// 上半表：biz_concrete_beam_design_coordinate_calculation 的已计算坐标（含预拱度，与算法 K 同源，**不取 source**）；
    /// 下半表：ConstructionGirderAlignment.PrintReport 对同一批坐标输出的几何报告
    /// （B_East / B_West / 端面折角 / 纵向偏距 / 横坡 / Z 轴趋势）。
    /// </summary>
    public async Task<ConcreteBeamOrientationPreviewDTO> DesignOrientationPreview(int castingGroupID)
    {
        await EnsureCastingGroupAccess(castingGroupID);
        _ = await GetSubmittedProjectConfig(castingGroupID);

        var rows = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
            .OrderBy(a => a.ID)
            .Select(a => new ConcreteBeamDesignCoordinateDTO
            {
                NodeName = a.NodeName,
                XE = a.XE, YE = a.YE, ZE = a.ZE,
                XC = a.XC, YC = a.YC, ZC = a.ZC,
                XW = a.XW, YW = a.YW, ZW = a.ZW
            }).ToListAsync();
        if (rows.Count == 0)
            throw new InvalidOperationException(BizMessages.NoCalculatedDesign(Lang));

        var k = rows.Select(a => new double[9]
        {
            (double)a.XE, (double)a.YE, (double)a.ZE,
            (double)a.XC, (double)a.YC, (double)a.ZC,
            (double)a.XW, (double)a.YW, (double)a.ZW
        }).ToArray();
        var report = new ConstructionGirderAlignment().PrintReport(k);

        // double.NaN 无法走默认 JSON 序列化（会抛异常），统一替换为 "NaN"，与原版界面口径一致。
        // 注意：PrintReport 返回 List<object>，其中表头是 List<string>、数据行是 List<object>；
        // 直接 Cast<List<object>>() 会因泛型不变性抛 InvalidCastException，故按非泛型 IEnumerable 逐元素转换。
        var sanitized = report
            .Select(row => (row is IEnumerable items ? items.Cast<object>() : new[] { row })
                .Select(v => v is double d && double.IsNaN(d) ? (object)"NaN" : v)
                .ToList())
            .ToList();
        return new ConcreteBeamOrientationPreviewDTO { Joints = rows, Report = sanitized };
    }

    /// <summary>
    /// 设计坐标「核对与预览」PDF：
    /// 上半 General Coordinates（已计算坐标，含预拱度），下半 ORIENTATION REPORT（PrintReport 几何报告）。
    /// </summary>
    public async Task<byte[]> BuildDesignOrientationPreviewPdf(int castingGroupID, int? timezoneOffsetMinutes = null)
    {
        var context = await EnsureCastingGroupAccess(castingGroupID);
        var config = await GetSubmittedProjectConfig(castingGroupID);

        var rows = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
            .OrderBy(a => a.ID)
            .Select(a => new ConcreteBeamDesignCoordinateDTO
            {
                NodeName = a.NodeName,
                XE = a.XE, YE = a.YE, ZE = a.ZE,
                XC = a.XC, YC = a.YC, ZC = a.ZC,
                XW = a.XW, YW = a.YW, ZW = a.ZW
            }).ToListAsync();
        if (rows.Count == 0)
            throw new InvalidOperationException(BizMessages.NoCalculatedDesign(Lang));

        var k = rows.Select(a => new double[9]
        {
            (double)a.XE, (double)a.YE, (double)a.ZE,
            (double)a.XC, (double)a.YC, (double)a.ZC,
            (double)a.XW, (double)a.YW, (double)a.ZW
        }).ToArray();
        var report = new ConstructionGirderAlignment().PrintReport(k);

        var model = new OrientationReportModel
        {
            ProjectTitle = context.ProjectName,
            CastingSet = context.CastingGroupName,
            ReportDate = NowForClient(timezoneOffsetMinutes),
            TimezoneOffsetMinutes = timezoneOffsetMinutes,
            JointHeaders = new List<string>
            {
                "Joint#", "East X", "East Y", "East Z",
                "Center X", "Center Y", "Center Z",
                "West X", "West Y", "West Z"
            },
            JointRows = rows.Select((a, i) => new List<string>
            {
                (i + 1).ToString(CultureInfo.InvariantCulture),
                Fmt4(a.XE), Fmt4(a.YE), Fmt4(a.ZE),
                Fmt4(a.XC), Fmt4(a.YC), Fmt4(a.ZC),
                Fmt4(a.XW), Fmt4(a.YW), Fmt4(a.ZW)
            }).ToList(),
            ReportHeaders = ReportRow.GetHeader(),
            ReportRows = report.Skip(1)
                .Select(row =>
                {
                    var cells = (row is IEnumerable items ? items.Cast<object>() : new[] { row }).ToList();
                    var result = new List<string>(cells.Count);
                    for (var i = 0; i < cells.Count; i++)
                    {
                        // 列序与 ReportRow.GetHeader() 一致：
                        // 1=B_East, 2=B_West → 偏差对项目 B；3=Bulkhead Angle(East), 4=(West) → 偏差对 90°
                        if (i == 1 || i == 2)
                            result.Add(CellTextWithDeviation(cells[i], (double)config.OffsetB));
                        else if (i == 3 || i == 4)
                            result.Add(CellTextWithDeviation(cells[i], 90d));
                        else
                            result.Add(CellText(cells[i]));
                    }
                    return result;
                })
                .ToList()
        };
        return ConcreteBeamOrientationReportBuilder.Build(model);
    }

    /// <summary>decimal → 4 位小数字符串（不变文化）。</summary>
    private static string Fmt4(decimal v) => v.ToString("0.0000", CultureInfo.InvariantCulture);

    /// <summary>PrintReport 单元格：double/NaN/decimal/字符串统一转可打印文本。</summary>
    private static string CellText(object v)
    {
        if (v == null) return "NaN";
        if (v is double d) return double.IsNaN(d) ? "NaN" : d.ToString("0.0000", CultureInfo.InvariantCulture);
        if (v is decimal m) return m.ToString("0.0000", CultureInfo.InvariantCulture);
        return Convert.ToString(v, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    /// <summary>
    /// 带偏差的单元格：数值与偏差分两行（value 换行 (value - baseline)），如
    /// 2.9000
    /// (0.0000)
    /// 非数值（null / "NaN"）不加偏差。
    /// </summary>
    private static string CellTextWithDeviation(object v, double baseline)
    {
        if (v is double d && !double.IsNaN(d) && !double.IsInfinity(d))
            return $"{d.ToString("0.0000", CultureInfo.InvariantCulture)}\n({(d - baseline).ToString("0.0000", CultureInfo.InvariantCulture)})";
        return CellText(v);
    }

    public async Task<ConcreteBeamPagedResultDTO<ConcreteBeamCamberDTO>> CamberList(ConcreteBeamDataQueryDTO req)
    {
        await EnsureCastingGroupAccess(req.CastingGroupID);
        var query = Db.Query<biz_concrete_beam_camber_calculation>().Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1);
        var total = await query.CountAsync();
        var page = Math.Max(req.PageIndex, 1);
        var size = NormalizePageSize(req.PageSize);
        var list = await query.OrderBy(a => a.ID).Skip((page - 1) * size).Take(size)
            .Select(a => new ConcreteBeamCamberDTO { NodeName = a.NodeName, ZE = a.ZE, ZC = a.ZC, ZW = a.ZW }).ToListAsync();
        return new() { List = list, Total = total };
    }

    public byte[] DesignCoordinateTemplate() => ConcreteBeamExcelHelper.CreateDesignCoordinateTemplate();
    public byte[] CamberTemplate() => ConcreteBeamExcelHelper.CreateCamberTemplate();
    public void EnsureAuthenticated() => _ = GetCurrentAccount();

    public async Task<ConcreteBeamImportResultDTO> PreviewDesignImport(int castingGroupID,
        byte[] bytes, bool elevationIncludesCamber)
    {
        await EnsureCastingGroupAccess(castingGroupID);
        // 已开始填报后不允许再动设计坐标，故在预览阶段就拒绝，而不是等用户走完流程到保存时才报错
        await EnsureSurveyEditable(castingGroupID, ConcreteBeamWorkflowRules.SurveyLockedDesignMessage(Lang));
        var config = await GetSubmittedProjectConfig(castingGroupID);
        ConcreteBeamParseResult<ConcreteBeamDesignCoordinateImportRow> parsed;
        try
        {
            using var stream = new MemoryStream(bytes);
            parsed = ConcreteBeamExcelHelper.ParseDesignCoordinates(stream, Lang);
        }
        catch (Exception ex)
        {
            return Failure([new(0, string.Empty, BizMessages.FieldFile(Lang), string.Empty, BizMessages.ExcelParseFailed(Lang, ex.Message))], Lang);
        }
        if (!parsed.Success) return Failure(parsed.Errors, Lang);
        // 设计坐标自身的完整性校验，与 elevationIncludesCamber 无关
        var nameErrors = ConcreteBeamDesignCalculator.ValidateNodeNames(parsed.Rows);
        if (nameErrors.Count > 0) return Failure(nameErrors, Lang);

        if (elevationIncludesCamber)
        {
            var dryRun = ConcreteBeamDesignCalculator.Calculate(parsed.Rows, config.OffsetB, null, true);
            if (dryRun.Errors.Count > 0) return Failure(dryRun.Errors, Lang);
        }
        else
        {
            var existingCamber = await LoadCamberCalculationRows(castingGroupID);
            if (existingCamber.Count > 0 &&
                ConcreteBeamDesignCalculator.ValidateNodeSequence(parsed.Rows, existingCamber).Count == 0)
            {
                var dryRun = ConcreteBeamDesignCalculator.Calculate(parsed.Rows, config.OffsetB,
                    existingCamber, false);
                if (dryRun.Errors.Count > 0) return Failure(dryRun.Errors, Lang);
            }
        }

        var token = _previewStore.Create(GetCurrentAccount(), castingGroupID,
            ConcreteBeamImportKind.Design, parsed.Rows, elevationIncludesCamber);
        return new()
        {
            Success = true,
            ImportedCount = parsed.Rows.Count,
            PreviewToken = token,
            Message = BizMessages.DesignReadSuccess(Lang, parsed.Rows.Count)
        };
    }

    public async Task<ConcreteBeamImportResultDTO> PreviewCamberImport(int castingGroupID, byte[] bytes)
    {
        await EnsureCastingGroupAccess(castingGroupID);
        await EnsureSurveyEditable(castingGroupID, ConcreteBeamWorkflowRules.SurveyLockedCamberMessage(Lang));
        var config = await GetSubmittedProjectConfig(castingGroupID);
        if (config.ElevationIncludesCamber)
            return Failure([new(0, string.Empty, BizMessages.FieldCamber(Lang), string.Empty, BizMessages.CamberAlreadyIncluded(Lang))], Lang);

        ConcreteBeamParseResult<ConcreteBeamCamberImportRow> parsed;
        try
        {
            using var stream = new MemoryStream(bytes);
            parsed = ConcreteBeamExcelHelper.ParseCamber(stream, Lang);
        }
        catch (Exception ex)
        {
            return Failure([new(0, string.Empty, BizMessages.FieldFile(Lang), string.Empty, BizMessages.ExcelParseFailed(Lang, ex.Message))], Lang);
        }
        if (!parsed.Success) return Failure(parsed.Errors, Lang);

        var designRows = await LoadDesignSourceRows(castingGroupID);
        if (designRows.Count == 0)
            return Failure([new(0, string.Empty, BizMessages.FieldDesign(Lang), string.Empty, BizMessages.DesignRequired(Lang))], Lang);
        var nodeErrors = ConcreteBeamDesignCalculator.ValidateNodeSequence(designRows, parsed.Rows);
        if (nodeErrors.Count > 0) return Failure(nodeErrors, Lang);

        var dryRun = ConcreteBeamDesignCalculator.Calculate(designRows, config.OffsetB,
            ConcreteBeamDesignCalculator.FillCamberRows(parsed.Rows), false);
        if (dryRun.Errors.Count > 0) return Failure(dryRun.Errors, Lang);

        var token = _previewStore.Create(GetCurrentAccount(), castingGroupID,
            ConcreteBeamImportKind.Camber, parsed.Rows);
        return new()
        {
            Success = true,
            ImportedCount = parsed.Rows.Count,
            PreviewToken = token,
            Message = BizMessages.CamberReadSuccess(Lang, parsed.Rows.Count)
        };
    }

    public async Task<ConcreteBeamPagedResultDTO<ConcreteBeamDesignCoordinateDTO>> DesignImportPreviewList(
        ConcreteBeamImportPreviewQueryDTO req)
    {
        await EnsureCastingGroupAccess(req.CastingGroupID);
        var preview = _previewStore.GetPage<ConcreteBeamDesignCoordinateImportRow>(req.PreviewToken,
            GetCurrentAccount(), req.CastingGroupID, ConcreteBeamImportKind.Design,
            req.PageIndex, NormalizePageSize(req.PageSize));
        // 归一化展示：用内存预览行现算，不叠加 camber，不落库
        if (req.Normalized)
        {
            var config = await GetSubmittedProjectConfig(req.CastingGroupID);
            var calculation = ConcreteBeamDesignCalculator.Calculate(preview.List, config.OffsetB, null, true);
            if (calculation.Errors.Count > 0)
                throw new InvalidOperationException(calculation.Errors[0].Reason);
            return new() { Total = preview.Total, List = calculation.Rows.Select(ToDesignDto).ToList() };
        }
        return new()
        {
            Total = preview.Total,
            List = preview.List.Select(ToDesignDto).ToList()
        };
    }

    public async Task<ConcreteBeamPagedResultDTO<ConcreteBeamCamberDTO>> CamberImportPreviewList(
        ConcreteBeamImportPreviewQueryDTO req)
    {
        await EnsureCastingGroupAccess(req.CastingGroupID);
        var preview = _previewStore.GetPage<ConcreteBeamCamberImportRow>(req.PreviewToken,
            GetCurrentAccount(), req.CastingGroupID, ConcreteBeamImportKind.Camber,
            req.PageIndex, NormalizePageSize(req.PageSize));
        return new()
        {
            Total = preview.Total,
            List = preview.List.Select(a => new ConcreteBeamCamberDTO
            { NodeName = a.NodeName, ZE = a.ZE, ZC = a.ZC, ZW = a.ZW }).ToList()
        };
    }

    /// <summary>构造与 source 节点同名、Z 全 0 的 camber 行，用于“无 camber 时按 0 计算”。</summary>
    private static List<ConcreteBeamCamberImportRow> ZeroCamberRows(
        IReadOnlyList<ConcreteBeamDesignCoordinateImportRow> rows) =>
        rows.Select((r, i) => new ConcreteBeamCamberImportRow(i + 2, r.NodeName, 0m, 0m, 0m)).ToList();

    /// <summary>
    /// 确认设计坐标导入：写 source 与 calculation（不再清空已有预拱度数据）。
    /// <para>calculation 规则：
    /// normalize=true：K 归一化；include=false 时叠加 camber，缺 camber 按 0 行补齐，不报错。
    /// normalize=false：不归一化；include=true 复制 source，include=false 为 source 的 Z 加 camber 的 Z（缺 +0）。</para>
    /// <para>缺 camber 只置 RequiresCamber，保存后提示，不阻断、不跳页签。</para>
    /// </summary>
    public async Task<ConcreteBeamImportResultDTO> ConfirmDesignImport(ConcreteBeamImportConfirmDTO req)
    {
        var context = await EnsureCastingGroupAccess(req.CastingGroupID);
        await EnsureSurveyEditable(req.CastingGroupID, ConcreteBeamWorkflowRules.SurveyLockedDesignMessage(Lang));
        var config = await GetSubmittedProjectConfig(req.CastingGroupID);
        var preview = _previewStore.GetForConfirm<ConcreteBeamDesignCoordinateImportRow>(
            req.PreviewToken, GetCurrentAccount(), req.CastingGroupID, ConcreteBeamImportKind.Design);
        var oldSources = await Db.Query<biz_concrete_beam_design_coordinate_source>()
            .Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1).ToListAsync();
        var oldCalculations = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1).ToListAsync();
        var hasAsCastData = await HasAsCastData(req.CastingGroupID);
        if ((oldSources.Count > 0 || oldCalculations.Count > 0) && !req.ConfirmOverwrite)
            return new()
            {
                RequiresOverwriteConfirmation = true,
                PreviewToken = req.PreviewToken,
                Message = ConcreteBeamWorkflowRules.DesignOverwriteMessage(hasAsCastData, Lang)
            };

        var camberCalculations = await Db.Query<biz_concrete_beam_camber_calculation>()
            .Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1).OrderBy(a => a.ID).ToListAsync();
        var camberRows = camberCalculations.Select((a, index) =>
            new ConcreteBeamCamberImportRow(index + 2, a.NodeName, a.ZE, a.ZC, a.ZW)).ToList();
        var elevationIncludesCamber = req.ElevationIncludesCamber ?? preview.ElevationIncludesCamber;
        var canReuseCamber = !elevationIncludesCamber && camberRows.Count > 0 &&
            ConcreteBeamDesignCalculator.ValidateNodeSequence(preview.Rows, camberRows).Count == 0;
        var normalize = req.Normalize;
        config.Normalize = normalize;

        List<ConcreteBeamDesignCoordinateImportRow> designCalculationRows;
        if (normalize)
        {
            var camberForCalc = elevationIncludesCamber
                ? null
                : (canReuseCamber ? camberRows : ZeroCamberRows(preview.Rows));
            var calculation = ConcreteBeamDesignCalculator.Calculate(preview.Rows, config.OffsetB,
                camberForCalc, elevationIncludesCamber);
            if (calculation.Errors.Count > 0)
                throw new InvalidOperationException(calculation.Errors[0].Reason);
            designCalculationRows = calculation.Rows;
        }
        else
        {
            designCalculationRows = !elevationIncludesCamber && canReuseCamber
                ? ConcreteBeamDesignCalculator.ApplyCamberToSource(preview.Rows, camberRows)
                : preview.Rows.ToList();
        }
        var requiresCamber = !elevationIncludesCamber && !canReuseCamber;

        var now = DateTime.Now;
        var account = GetCurrentAccount();
        var invalidatedCount = 0;
        try
        {
            Db.BeginTransaction();
            SoftDelete(oldSources, account, now);
            SoftDelete(oldCalculations, account, now);
            invalidatedCount = await InvalidateAsCastData(req.CastingGroupID, account, now);
            // 重新上传设计坐标时保留已有 camber，供后续复用
            Db.GetRepository<biz_concrete_beam_design_coordinate_source>().AddList(
                preview.Rows.Select(a => ToDesignSourceEntity(a, context, req.CastingGroupID, account, now)).ToList());
            Db.GetRepository<biz_concrete_beam_design_coordinate_calculation>().AddList(
                designCalculationRows.Select(a => ToDesignCalculationEntity(a, context,
                    req.CastingGroupID, account, now)).ToList());
            config.ElevationIncludesCamber = elevationIncludesCamber;
            config.UpdatedBy = account;
            config.UpdatedTime = now;
            await Db.SaveAsync();
            Db.CommitTransaction();
        }
        catch (Exception ex)
        {
            Db.RollbackTransaction();
            throw new InvalidOperationException(BizMessages.SaveFailed(Lang, ex.Message), ex);
        }
        _previewStore.RemoveConfirmed(req.PreviewToken);
        var designMessages = new List<string> { BizMessages.DesignSaveSuccess(Lang, preview.Rows.Count) };
        if (requiresCamber) designMessages.Add(BizMessages.CamberZeroApplied(Lang));
        if (invalidatedCount > 0) designMessages.Add(ConcreteBeamWorkflowRules.AsCastInvalidatedMessage(invalidatedCount, Lang));
        return new()
        {
            Success = true,
            ImportedCount = preview.Rows.Count,
            RequiresCamber = requiresCamber,
            Message = string.Join("；", designMessages)
        };
    }

    /// <summary>
    /// 确认后修改【归一化坐标】/【包含预拱度】：基于已保存 source 重新生成 calculation。
    /// <para>规则同 ConfirmDesignImport：normalize=true 走 K；normalize=false 走 source(+camber/0)。
    /// 缺 camber 按 0 补齐，不报错；只提示 RequiresCamber，不跳页签。</para>
    /// <para>不清空已有 camber；有设站数据时由 EnsureSurveyEditable 拒绝。</para>
    /// </summary>
    public async Task<ConcreteBeamImportResultDTO> ConfirmDesignCamberSetting(ConcreteBeamDesignCamberSettingDTO req)
    {
        if (req == null) throw new InvalidOperationException(BizMessages.ParameterRequired(Lang));
        var context = await EnsureCastingGroupAccess(req.CastingGroupID);
        await EnsureSurveyEditable(req.CastingGroupID, ConcreteBeamWorkflowRules.SurveyLockedDesignMessage(Lang));
        var config = await GetSubmittedProjectConfig(req.CastingGroupID);

        var designRows = await LoadDesignSourceRows(req.CastingGroupID);
        if (designRows.Count == 0)
            return new() { Success = true, Message = BizMessages.DesignCamberNoDesignData(Lang) };

        var camberCalculations = await Db.Query<biz_concrete_beam_camber_calculation>()
            .Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1).OrderBy(a => a.ID).ToListAsync();
        var camberRows = camberCalculations.Select((a, index) =>
            new ConcreteBeamCamberImportRow(index + 2, a.NodeName, a.ZE, a.ZC, a.ZW)).ToList();
        var canReuseCamber = !req.ElevationIncludesCamber && camberRows.Count > 0 &&
            ConcreteBeamDesignCalculator.ValidateNodeSequence(designRows, camberRows).Count == 0;
        var normalize = req.Normalize;
        config.Normalize = normalize;

        List<ConcreteBeamDesignCoordinateImportRow> designCalculationRows;
        if (normalize)
        {
            var camberForCalc = req.ElevationIncludesCamber
                ? null
                : (canReuseCamber ? camberRows : ZeroCamberRows(designRows));
            var calculation = ConcreteBeamDesignCalculator.Calculate(designRows, config.OffsetB,
                camberForCalc, req.ElevationIncludesCamber);
            if (calculation.Errors.Count > 0)
                throw new InvalidOperationException(calculation.Errors[0].Reason);
            designCalculationRows = calculation.Rows;
        }
        else
        {
            designCalculationRows = !req.ElevationIncludesCamber && canReuseCamber
                ? ConcreteBeamDesignCalculator.ApplyCamberToSource(designRows, camberRows)
                : designRows.ToList();
        }
        var requiresCamber = !req.ElevationIncludesCamber && !canReuseCamber;

        var hasAsCastData = await HasAsCastData(req.CastingGroupID);
        if (!req.ConfirmOverwrite)
            return new()
            {
                RequiresOverwriteConfirmation = true,
                Message = BizMessages.DesignCamberSettingConfirm(Lang, normalize, hasAsCastData, requiresCamber)
            };

        var oldCalculations = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1).ToListAsync();
        var now = DateTime.Now;
        var account = GetCurrentAccount();
        var invalidatedCount = 0;
        try
        {
            Db.BeginTransaction();
            SoftDelete(oldCalculations, account, now);
            invalidatedCount = await InvalidateAsCastData(req.CastingGroupID, account, now);
            Db.GetRepository<biz_concrete_beam_design_coordinate_calculation>().AddList(
                designCalculationRows.Select(a => ToDesignCalculationEntity(a, context,
                    req.CastingGroupID, account, now)).ToList());
            config.ElevationIncludesCamber = req.ElevationIncludesCamber;
            config.UpdatedBy = account;
            config.UpdatedTime = now;
            await Db.SaveAsync();
            Db.CommitTransaction();
        }
        catch (Exception ex)
        {
            Db.RollbackTransaction();
            throw new InvalidOperationException(BizMessages.SaveFailed(Lang, ex.Message), ex);
        }

        var messages = new List<string>
        {
            normalize
                ? BizMessages.DesignCamberSettingUpdated(Lang)
                : BizMessages.DesignCamberSettingUpdatedNoNormalize(Lang)
        };
        if (requiresCamber) messages.Add(BizMessages.CamberZeroApplied(Lang));
        if (invalidatedCount > 0) messages.Add(ConcreteBeamWorkflowRules.AsCastInvalidatedMessage(invalidatedCount, Lang));
        return new()
        {
            Success = true,
            ImportedCount = designRows.Count,
            RequiresCamber = requiresCamber,
            Message = string.Join("；", messages)
        };
    }

    public async Task<ConcreteBeamImportResultDTO> ConfirmCamberImport(ConcreteBeamImportConfirmDTO req)
    {
        var context = await EnsureCastingGroupAccess(req.CastingGroupID);
        await EnsureSurveyEditable(req.CastingGroupID, ConcreteBeamWorkflowRules.SurveyLockedCamberMessage(Lang));
        var config = await GetSubmittedProjectConfig(req.CastingGroupID);
        if (config.ElevationIncludesCamber)
            throw new InvalidOperationException(BizMessages.CamberAlreadyIncluded(Lang));
        var preview = _previewStore.GetForConfirm<ConcreteBeamCamberImportRow>(
            req.PreviewToken, GetCurrentAccount(), req.CastingGroupID, ConcreteBeamImportKind.Camber);
        var designRows = await LoadDesignSourceRows(req.CastingGroupID);
        var nodeErrors = ConcreteBeamDesignCalculator.ValidateNodeSequence(designRows, preview.Rows);
        if (nodeErrors.Count > 0) throw new InvalidOperationException(BizMessages.DesignChangedReimportCamber(Lang));
        var filledCamber = ConcreteBeamDesignCalculator.FillCamberRows(preview.Rows);
        var normalize = req.Normalize;
        config.Normalize = normalize;

        List<ConcreteBeamDesignCoordinateImportRow> designCalculationRows;
        if (normalize)
        {
            var calculation = ConcreteBeamDesignCalculator.Calculate(designRows, config.OffsetB, filledCamber, false);
            if (calculation.Errors.Count > 0) throw new InvalidOperationException(calculation.Errors[0].Reason);
            designCalculationRows = calculation.Rows;
        }
        else
        {
            designCalculationRows = ConcreteBeamDesignCalculator.ApplyCamberToSource(designRows, filledCamber);
        }

        var oldCamberSources = await Db.Query<biz_concrete_beam_camber_source>()
            .Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1).ToListAsync();
        var oldCamberCalculations = await Db.Query<biz_concrete_beam_camber_calculation>()
            .Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1).ToListAsync();
        var hasAsCastData = await HasAsCastData(req.CastingGroupID);
        if ((oldCamberSources.Count > 0 || oldCamberCalculations.Count > 0) && !req.ConfirmOverwrite)
            return new()
            {
                RequiresOverwriteConfirmation = true,
                PreviewToken = req.PreviewToken,
                Message = ConcreteBeamWorkflowRules.CamberOverwriteMessage(hasAsCastData, Lang)
            };

        var oldDesignCalculations = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1).ToListAsync();
        var now = DateTime.Now;
        var account = GetCurrentAccount();
        var invalidatedCount = 0;
        try
        {
            Db.BeginTransaction();
            SoftDelete(oldCamberSources, account, now);
            SoftDelete(oldCamberCalculations, account, now);
            SoftDelete(oldDesignCalculations, account, now);
            // 预拱度改变了端面高程基准，下游（梁段填报/浇筑坐标/浇筑测点）必须整体作废重做
            invalidatedCount = await InvalidateAsCastData(req.CastingGroupID, account, now);
            Db.GetRepository<biz_concrete_beam_camber_source>().AddList(preview.Rows.Select(a =>
                new biz_concrete_beam_camber_source
                {
                    ProjID = context.ProjID,
                    GirderID = context.GirderID,
                    CastingGroupID = req.CastingGroupID,
                    NodeName = a.NodeName,
                    ZE = a.ZE,
                    ZC = a.ZC,
                    ZW = a.ZW,
                    Status = 0,
                    CreatedBy = account,
                    CreatedTime = now
                }).ToList());
            Db.GetRepository<biz_concrete_beam_camber_calculation>().AddList(filledCamber.Select(a =>
                new biz_concrete_beam_camber_calculation
                {
                    ProjID = context.ProjID,
                    GirderID = context.GirderID,
                    CastingGroupID = req.CastingGroupID,
                    NodeName = a.NodeName,
                    ZE = a.ZE!.Value,
                    ZC = a.ZC,
                    ZW = a.ZW!.Value,
                    Status = 0,
                    CreatedBy = account,
                    CreatedTime = now
                }).ToList());
            Db.GetRepository<biz_concrete_beam_design_coordinate_calculation>().AddList(
                designCalculationRows.Select(a => ToDesignCalculationEntity(a, context,
                    req.CastingGroupID, account, now)).ToList());
            await Db.SaveAsync();
            Db.CommitTransaction();
        }
        catch (Exception ex)
        {
            Db.RollbackTransaction();
            throw new InvalidOperationException(BizMessages.SaveFailed(Lang, ex.Message), ex);
        }
        _previewStore.RemoveConfirmed(req.PreviewToken);
        var camberMessages = new List<string> { BizMessages.CamberSaveSuccess(Lang, preview.Rows.Count) };
        if (invalidatedCount > 0) camberMessages.Add(ConcreteBeamWorkflowRules.AsCastInvalidatedMessage(invalidatedCount, Lang));
        return new()
        {
            Success = true,
            ImportedCount = preview.Rows.Count,
            Message = string.Join("；", camberMessages)
        };
    }

    public async Task CancelImportPreview(ConcreteBeamImportCancelDTO req)
    {
        await EnsureCastingGroupAccess(req.CastingGroupID);
        var kind = string.Equals(req.ImportType, "design", StringComparison.OrdinalIgnoreCase)
            ? ConcreteBeamImportKind.Design
            : string.Equals(req.ImportType, "camber", StringComparison.OrdinalIgnoreCase)
                ? ConcreteBeamImportKind.Camber
                : throw new InvalidOperationException(BizMessages.InvalidImportType(Lang));
        _previewStore.Remove(req.PreviewToken, GetCurrentAccount(), req.CastingGroupID, kind);
    }

    private async Task<List<ConcreteBeamDesignCoordinateImportRow>> LoadDesignSourceRows(int castingGroupID)
    {
        var entities = await Db.Query<biz_concrete_beam_design_coordinate_source>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
            .OrderBy(a => a.ID).ToListAsync();
        return entities.Select((a, index) => new ConcreteBeamDesignCoordinateImportRow(index + 2,
            a.NodeName, a.XE, a.YE, a.ZE, a.XC, a.YC, a.ZC, a.XW, a.YW, a.ZW)).ToList();
    }

    private async Task<List<ConcreteBeamCamberImportRow>> LoadCamberCalculationRows(int castingGroupID)
    {
        var entities = await Db.Query<biz_concrete_beam_camber_calculation>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
            .OrderBy(a => a.ID).ToListAsync();
        return entities.Select((a, index) =>
            new ConcreteBeamCamberImportRow(index + 2, a.NodeName, a.ZE, a.ZC, a.ZW)).ToList();
    }

    private static ConcreteBeamDesignCoordinateDTO ToDesignDto(ConcreteBeamDesignCoordinateImportRow a) => new()
    {
        NodeName = a.NodeName,
        XE = a.XE,
        YE = a.YE,
        ZE = a.ZE,
        XC = a.XC,
        YC = a.YC,
        ZC = a.ZC,
        XW = a.XW,
        YW = a.YW,
        ZW = a.ZW
    };

    private static biz_concrete_beam_design_coordinate_source ToDesignSourceEntity(
        ConcreteBeamDesignCoordinateImportRow a, CastingGroupContext context,
        int castingGroupID, string account, DateTime now) => new()
        {
            ProjID = context.ProjID,
            GirderID = context.GirderID,
            CastingGroupID = castingGroupID,
            NodeName = a.NodeName,
            XE = a.XE,
            YE = a.YE,
            ZE = a.ZE,
            XC = a.XC,
            YC = a.YC,
            ZC = a.ZC,
            XW = a.XW,
            YW = a.YW,
            ZW = a.ZW,
            Status = 0,
            CreatedBy = account,
            CreatedTime = now
        };

    private static biz_concrete_beam_design_coordinate_calculation ToDesignCalculationEntity(
        ConcreteBeamDesignCoordinateImportRow a, CastingGroupContext context,
        int castingGroupID, string account, DateTime now) => new()
        {
            ProjID = context.ProjID,
            GirderID = context.GirderID,
            CastingGroupID = castingGroupID,
            NodeName = a.NodeName,
            XE = a.XE,
            YE = a.YE,
            ZE = a.ZE,
            XC = a.XC,
            YC = a.YC,
            ZC = a.ZC,
            XW = a.XW,
            YW = a.YW,
            ZW = a.ZW,
            Status = 0,
            CreatedBy = account,
            CreatedTime = now
        };

    private static void SoftDelete(IEnumerable<biz_concrete_beam_design_coordinate_source> rows,
        string account, DateTime now)
    {
        foreach (var row in rows) { row.Status = -1; row.UpdatedBy = account; row.UpdatedTime = now; }
    }

    private static void SoftDelete(IEnumerable<biz_concrete_beam_design_coordinate_calculation> rows,
        string account, DateTime now)
    {
        foreach (var row in rows) { row.Status = -1; row.UpdatedBy = account; row.UpdatedTime = now; }
    }

    private static void SoftDelete(IEnumerable<biz_concrete_beam_camber_source> rows,
        string account, DateTime now)
    {
        foreach (var row in rows) { row.Status = -1; row.UpdatedBy = account; row.UpdatedTime = now; }
    }

    private static void SoftDelete(IEnumerable<biz_concrete_beam_camber_calculation> rows,
        string account, DateTime now)
    {
        foreach (var row in rows) { row.Status = -1; row.UpdatedBy = account; row.UpdatedTime = now; }
    }

    private static void SoftDelete(IEnumerable<biz_concrete_beam_segment> rows,
        string account, DateTime now)
    {
        foreach (var row in rows) { row.Status = -1; row.UpdatedBy = account; row.UpdatedTime = now; }
    }

    private static void SoftDelete(IEnumerable<biz_concrete_beam_ascast_coordinate> rows,
        string account, DateTime now)
    {
        foreach (var row in rows) { row.Status = -1; row.UpdatedBy = account; row.UpdatedTime = now; }
    }

    private static void SoftDelete(IEnumerable<biz_concrete_beam_ascast_measure> rows,
        string account, DateTime now)
    {
        foreach (var row in rows) { row.Status = -1; row.UpdatedBy = account; row.UpdatedTime = now; }
    }

    /// <summary>
    /// 判断该浇筑组是否已开始逐段实测填报（biz_concrete_beam_segment 存在有效行）。
    /// 与 <see cref="HasAsCastData"/> 的区别：后者还包含浇筑坐标/测点，二者在同一事务内
    /// 同生共死，但本方法语义更贴近"测量员是否已开工"，用于前端锁定与服务端兜底校验。
    /// </summary>
    private async Task<bool> HasSurveyData(int castingGroupID) =>
        await Db.Query<biz_concrete_beam_segment>()
            .AnyAsync(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete);

    /// <summary>
    /// 上游输入（项目信息 / 设计坐标 / 预拱度）的可改前置校验：已开始填报则拒绝。
    /// </summary>
    /// <param name="castingGroupID">浇筑组。</param>
    /// <param name="message">被锁定时的提示语，取 <see cref="ConcreteBeamWorkflowRules"/> 的上游锁定文案常量。</param>
    private async Task EnsureSurveyEditable(int castingGroupID, string message)
    {
        if (ConcreteBeamWorkflowRules.IsUpstreamLocked(await HasSurveyData(castingGroupID)))
            throw new InvalidOperationException(message);
    }

    /// <summary>
    /// 判断该浇筑组是否已存在浇筑态数据（逐段填报的实测数据 / 浇筑坐标 / 浇筑测点）。
    /// </summary>
    private async Task<bool> HasAsCastData(int castingGroupID)
    {
        return await Db.Query<biz_concrete_beam_segment>()
                   .AnyAsync(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete)
            || await Db.Query<biz_concrete_beam_ascast_coordinate>()
                   .AnyAsync(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete)
            || await Db.Query<biz_concrete_beam_ascast_measure>()
                   .AnyAsync(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete);
    }

    /// <summary>
    /// 作废该浇筑组下全部浇筑态数据：梁段填报（segment）+ 浇筑端面坐标（ascast_coordinate）
    /// + 浇筑测点坐标（ascast_measure）。
    /// 必须在调用方已开启的事务内执行：三者是同一条计算链上的产物，只允许整体作废，
    /// 只清其中一部分会留下指向已删设计端面的孤儿数据，且不报错。
    /// </summary>
    /// <returns>被作废的记录条数。</returns>
    private async Task<int> InvalidateAsCastData(int castingGroupID, string account, DateTime now)
    {
        var segments = await Db.Query<biz_concrete_beam_segment>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).ToListAsync();
        var ascastList = await Db.Query<biz_concrete_beam_ascast_coordinate>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).ToListAsync();
        var measureList = await Db.Query<biz_concrete_beam_ascast_measure>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).ToListAsync();
        SoftDelete(segments, account, now);
        SoftDelete(ascastList, account, now);
        SoftDelete(measureList, account, now);
        return segments.Count + ascastList.Count + measureList.Count;
    }

    private static ConcreteBeamImportResultDTO Failure(IEnumerable<ConcreteBeamImportError> errors, string? lang)
    {
        var errorList = errors.ToList();
        return new()
        {
            Message = BizMessages.ImportFailedSeeDetail(lang),
            Errors = errorList.Select(a => new ConcreteBeamImportErrorDTO
            { ExcelRow = a.ExcelRow, RecordKey = a.RecordKey, FieldName = a.FieldName, OriginalValue = a.OriginalValue, Reason = a.Reason }).ToList(),
            ErrorFile = ConcreteBeamExcelHelper.CreateErrorFile(errorList, lang),
            ErrorFileName = BizMessages.ErrorFileName(lang, DateTime.Now)
        };
    }

    private async Task<HashSet<int>?> GetAllowedProjectIds()
    {
        var account = GetCurrentAccount();
        var userID = await Db.Query<sys_userinfo>()
            .Where(a => a.Status == 1 && a.Account == account)
            .Select(a => (int?)a.ID)
            .FirstOrDefaultAsync()
            ?? throw new UnauthorizedAccessException(BizMessages.AccountDisabled(Lang));

        var adminRoleCodes = new[] { "role_admin", "role_biz_admin" };
        var isAdmin = await (from userRole in Db.Query<sys_user_role>().Where(a => a.Status == 1 && a.UserID == userID)
                             join role in Db.Query<sys_role>().Where(a => a.Status == 1)
                                 on userRole.RoleID equals role.ID
                             where role.Code != null && adminRoleCodes.Contains(role.Code)
                             select role.ID).AnyAsync();
        if (isAdmin) return null;

        var projectIDs = await Db.Query<biz_project_user>()
            .Where(a => a.Status == 1 && a.UserID == userID)
            .Select(a => a.ProjID)
            .Distinct()
            .ToListAsync();
        return projectIDs.ToHashSet();
    }

    private async Task EnsureProjectAccess(int projID)
    {
        var allowed = await GetAllowedProjectIds();
        if (allowed != null && !allowed.Contains(projID)) throw new UnauthorizedAccessException(BizMessages.NoProjectPermission(Lang));
    }

    private async Task<CastingGroupContext> EnsureCastingGroupAccess(int castingGroupID)
    {
        var context = await (from cg in Db.Query<biz_project_bridge_castingGroup>().Where(a => a.Status != -1 && a.ID == castingGroupID)
                             join bridge in Db.Query<biz_project_bridge>().Where(a => a.Status != -1 && a.BeamType == 1) on cg.BridgeID equals bridge.ID
                             join project in Db.Query<biz_project>().Where(a => a.Status != -1) on bridge.ProjID equals project.ID
                             select new CastingGroupContext(project.ID, bridge.ID, project.Name, bridge.Name,
                                 cg.Name, project.ManagerName ?? string.Empty,
                                 cg.CreatedBy ?? string.Empty, cg.CreatedTime))
                             .FirstOrDefaultAsync() ?? throw new InvalidOperationException(BizMessages.CastingGroupMissing(Lang));
        await EnsureProjectAccess(context.ProjID); return context;
    }

    private string GetCurrentAccount()
    {
        var account = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(account)) throw new UnauthorizedAccessException(BizMessages.AccountMissing(Lang));
        return account;
    }

    private static void ValidateFourDecimals(decimal value, string field, string? lang)
    { if (!ExcelUtils.HasScale(value, 4)) throw new InvalidOperationException(BizMessages.MaxFourDecimals(lang, field)); }
    private async Task<biz_concrete_beam_config> GetSubmittedProjectConfig(int castingGroupID)
    {
        var config = await Db.Query<biz_concrete_beam_config>().Where(a =>
            a.CastingGroupID == castingGroupID && a.Status != -1 &&
            a.OffsetA > 0 && a.OffsetB > 0 && a.OffsetC > 0)
            .OrderByDescending(a => a.ID).FirstOrDefaultAsync();
        return config ?? throw new InvalidOperationException(BizMessages.ProjectConfigRequired(Lang));
    }
    private static int NormalizePageSize(int size) => size is 10 or 20 or 50 or 100 ? size : 10;
    private sealed record CastingGroupContext(int ProjID, int GirderID, string ProjectName,
        string BridgeName, string CastingGroupName, string ManagerName,
        string CreatedBy, DateTime? CreatedTime);

    public static double[] ToDoubleArray(biz_concrete_beam_design_coordinate_calculation item)
    {
        var arr = new double[9] {
                    (double?)item.XE ?? 0, (double?)item.YE ?? 0, (double?)item.ZE ?? 0,
                    (double?)item.XC ?? 0, (double?)item.YC ?? 0, (double?)item.ZC ?? 0,
                    (double?)item.XW ?? 0, (double?)item.YW ?? 0, (double?)item.ZW ?? 0 };
        return arr;
    }
    public static double[] ToDoubleArray(biz_concrete_beam_ascast_coordinate item)
    {
        var arr = new double[9] {
                    (double?)item.EastX ?? 0, (double?)item.EastY ?? 0, (double?)item.EastZ ?? 0,
                    (double?)item.CenterX ?? 0, (double?)item.CenterY ?? 0, (double?)item.CenterZ ?? 0,
                    (double?)item.WestX ?? 0, (double?)item.WestY ?? 0, (double?)item.WestZ ?? 0 };
        return arr;
    }
    public static double[] ToDoubleArray(biz_concrete_beam_ascast_measure item)
    {
        var arr = new double[9] {
                    (double?)item.EastX ?? 0, (double?)item.EastY ?? 0, (double?)item.EastZ ?? 0,
                    (double?)item.CenterX ?? 0, (double?)item.CenterY ?? 0, (double?)item.CenterZ ?? 0,
                    (double?)item.WestX ?? 0, (double?)item.WestY ?? 0, (double?)item.WestZ ?? 0 };
        return arr;
    }


    #region Survey 浇筑梁段

    /// <summary>
    /// 取全部梁段（不分页），每段带 IsFirst / IsFilled / CanEdit / LockedReason 标记，
    /// 由前端判断哪一段可填报、哪一段只读。
    /// </summary>
    /// <param name="castingGroupID">浇筑组ID（梁ID由浇筑组推导，无需另传）。</param>
    /// <remarks>
    /// 不走 SegmentList 分页：NormalizePageSize 只认 10/20/50/100，
    /// 梁段数超过 100 时前端无法一次取全，标记判断会失效。
    /// </remarks>
    public async Task<ConcreteBeamSegmentSurveyDTO> GetSegmentSurveyList(int castingGroupID)
    {
        var context = await EnsureCastingGroupAccess(castingGroupID);
        var allSegments = await LoadAllSegments(castingGroupID, true);
        return ToSegmentSurveyDto(context.GirderID, allSegments, allSegments);
    }

    /// <summary>
    /// 只取当前可操作的梁段：最后一个已填报的梁段（下一段填报前仍可改）+ 下一个待填报的梁段。
    /// 一次都没填报时只返回首个梁段；全部填报完毕时只返回最后一个。
    /// 与 GetSegmentSurveyList 共用一套标记，只是这里已按 CanEdit 过滤。
    /// </summary>
    /// <param name="castingGroupID">浇筑组ID（梁ID由浇筑组推导，无需另传）。</param>
    public async Task<ConcreteBeamSegmentSurveyDTO> GetSegmentToSurvey(int castingGroupID)
    {
        var context = await EnsureCastingGroupAccess(castingGroupID);
        var allSegments = await LoadAllSegments(castingGroupID, true);
        if (allSegments.Count == 0)
            throw new InvalidOperationException(BizMessages.NoSegmentGenerated(Lang));
        return ToSegmentSurveyDto(context.GirderID, allSegments, allSegments.Where(a => a.CanEdit).ToList());
    }

    /// <summary>组装梁段列表返回体（全量列表与可操作快捷列表共用）。</summary>
    private static ConcreteBeamSegmentSurveyDTO ToSegmentSurveyDto(
        int girderID, List<ConcreteBeamSegmentDTO> allSegments, List<ConcreteBeamSegmentDTO> list)
    {
        var filledMax = FilledMaxIndex(allSegments);
        var nextIndex = filledMax + 1;
        return new ConcreteBeamSegmentSurveyDTO
        {
            List = list,
            GirderID = girderID,
            FilledMaxIndex = filledMax,
            NextSegmentIndex = nextIndex < allSegments.Count ? nextIndex : -1,
            AllCompleted = filledMax >= allSegments.Count - 1,
            TotalSegmentCount = allSegments.Count
        };
    }

    /// <summary>
    /// 读取某个梁段的实测填报数据（Survey）。未填报过则返回带默认值与 CanEdit 状态的 DTO。
    /// </summary>
    /// <param name="castingGroupID">浇筑组ID。</param>
    /// <param name="segmentName">梁段名称，格式 {节点1}-{节点2}。</param>
    public async Task<SetupNSurveyDTO> GetSurvey(int castingGroupID, string segmentName)
    {
        if (string.IsNullOrWhiteSpace(segmentName)) throw new InvalidOperationException(BizMessages.SegmentNameRequired(Lang));
        var context = await EnsureCastingGroupAccess(castingGroupID);
        var allSegments = await LoadAllSegments(castingGroupID, true);
        var target = FindSegment(allSegments, segmentName);
        var filledMax = FilledMaxIndex(allSegments);
        var entity = await Db.Query<biz_concrete_beam_segment>().FirstOrDefaultAsync(a =>
            a.CastingGroupID == castingGroupID && a.SegmentName == target.SegmentName && a.Status != StatusDelete);

        var dto = entity == null ? new SetupNSurveyDTO() : ToSurveyDto(entity);
        dto.CastingGroupID = castingGroupID;
        dto.GriderID = context.GirderID;
        dto.SegmentName = target.SegmentName;
        dto.SegmentIndex = target.SegmentIndex;
        dto.Node1 = target.Node1;
        dto.Node2 = target.Node2;
        dto.IsFirst = target.IsFirst;
        var (canEdit, lockedReason) =
            ConcreteBeamWorkflowRules.DescribeSegmentFill(filledMax, target.SegmentIndex, entity != null, Lang);
        dto.CanEdit = canEdit;
        dto.LockedReason = lockedReason;
        return dto;
    }

    /// <summary>
    /// 仅更新梁段浇筑日期（往期已锁定段也允许）。
    /// <para>只写 PouringDate，不触碰实测值、立模参数、浇筑坐标，避免绕过逐段填报规则。</para>
    /// </summary>
    public async Task<ConcreteBeamSurveySaveResultDTO> UpdatePouringDate(ConcreteBeamPouringDateDTO req)
    {
        if (req == null) throw new InvalidOperationException(BizMessages.ParameterRequired(Lang));
        if (string.IsNullOrWhiteSpace(req.SegmentName))
            throw new InvalidOperationException(BizMessages.SegmentNameRequiredForSurvey(Lang));

        var context = await EnsureCastingGroupAccess(req.CastingGroupID);
        var entity = await Db.Query<biz_concrete_beam_segment>().FirstOrDefaultAsync(a =>
            a.CastingGroupID == req.CastingGroupID
            && a.SegmentName == req.SegmentName
            && a.Status != StatusDelete);
        if (entity == null)
            throw new InvalidOperationException(BizMessages.SegmentSurveyNotFound(Lang, req.SegmentName));

        entity.PouringDate = req.PouringDate;
        entity.UpdatedBy = GetCurrentAccount();
        entity.UpdatedTime = DateTime.Now;
        await Db.SaveAsync();

        var dto = ToSurveyDto(entity);
        dto.CastingGroupID = req.CastingGroupID;
        dto.GriderID = context.GirderID;
        return new ConcreteBeamSurveySaveResultDTO
        {
            Survey = dto,
            Message = BizMessages.PouringDateUpdated(Lang, req.SegmentName)
        };
    }

    /// <summary>
    /// Survey点击Apply（确认）：保存本梁段录入的实测数据，跑线形算法（St=1）并把
    /// 浇筑端面坐标（ascast_coordinate）与浇筑测点坐标（ascast_measure）落库。
    /// </summary>
    /// <remarks>
    /// 业务流程与关键约定：
    /// 1. 以 SegmentName 定位梁段：查到=更新（仅最后一个已填报段可改），查不到=新增（必须紧邻已填报段）；
    /// 2. 算法 K 取 design_coordinate_calculation，ascast 的 DesignCoordinateID 统一指回 source；
    /// 3. KT/KCT 按端面数 N / 段数 2(N-1) 预分配后回填已落库值，避免首段数组越界；
    /// 4. 一次 Run(St=1) 即可同时得到立模参数（CalcResult，写回 segment.Match*）与修正后坐标；
    /// 5. 全部写入在同一事务内，段/ascast/measure 三者要么同时成功要么整体回滚。
    /// </remarks>
    public async Task<ConcreteBeamSurveySaveResultDTO> SaveSurvey(SetupNSurveyDTO req)
    {
        if (req == null) throw new InvalidOperationException(BizMessages.ParameterRequired(Lang));
        if (string.IsNullOrWhiteSpace(req.SegmentName)) throw new InvalidOperationException(BizMessages.SegmentNameRequiredForSurvey(Lang));
        var castingGroupID = req.CastingGroupID;
        var context = await EnsureCastingGroupAccess(castingGroupID);
        var config = await GetSubmittedProjectConfig(castingGroupID);
        var account = GetCurrentAccount();
        var now = DateTime.Now;

        // 1. 定位梁段 + 填报顺序校验（不跳段 / 不返工 / 本段可改）
        var allSegments = await LoadAllSegments(castingGroupID, true);
        var target = FindSegment(allSegments, req.SegmentName);
        var m = target.SegmentIndex;
        var segments = await Db.Query<biz_concrete_beam_segment>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).ToListAsync();
        var existing = segments.FirstOrDefault(a => string.Equals(a.SegmentName, target.SegmentName, StringComparison.Ordinal));
        var filledMax = FilledMaxIndex(allSegments);
        var decision = ConcreteBeamWorkflowRules.DecideSegmentFill(filledMax, m, existing != null);
        if (decision == ConcreteBeamWorkflowRules.SegmentFillDecision.SkipAhead)
            throw new InvalidOperationException(ConcreteBeamWorkflowRules.SkipSegmentMessage(filledMax, m, Lang));
        if (decision == ConcreteBeamWorkflowRules.SegmentFillDecision.Locked)
            throw new InvalidOperationException(ConcreteBeamWorkflowRules.LockedSegmentMessage(filledMax, m, Lang));

        // 2. 设计坐标：calculation 供算法 K 使用，source 作为 ascast 的 DesignCoordinateID 锚点
        var calculation = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).OrderBy(a => a.ID).ToListAsync();
        var source = await Db.Query<biz_concrete_beam_design_coordinate_source>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).OrderBy(a => a.ID).ToListAsync();
        if (calculation.Count != allSegments.Count + 1 || source.Count != calculation.Count)
            throw new InvalidOperationException(BizMessages.DesignSourceInconsistent(Lang));

        var jointCount = calculation.Count;
        var segmentCount = jointCount - 1;
        var kArray = calculation.Select(ToDoubleArray).ToArray();

        // 3. KT/KCT 预分配 + 回填已落库的浇筑坐标
        var ascastList = await Db.Query<biz_concrete_beam_ascast_coordinate>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).ToListAsync();
        var measureList = await Db.Query<biz_concrete_beam_ascast_measure>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).ToListAsync();

        var ktArray = NewMatrix(jointCount);
        var kctArray = NewMatrix(segmentCount * 2);
        var jointIndexBySourceId = new Dictionary<int, int>();
        for (var i = 0; i < source.Count; i++) jointIndexBySourceId[source[i].ID] = i;
        foreach (var row in ascastList)
            if (jointIndexBySourceId.TryGetValue(row.DesignCoordinateID, out var ji) && ji < jointCount)
                ktArray[ji] = ToDoubleArray(row);

        var segmentIndexByName = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var s in allSegments) segmentIndexByName[s.SegmentName] = s.SegmentIndex;
        var segmentNameById = new Dictionary<int, string>();
        foreach (var s in segments) segmentNameById[s.ID] = s.SegmentName;
        foreach (var row in measureList)
        {
            if (!segmentNameById.TryGetValue(row.SegmentID, out var sName)) continue;
            if (!segmentIndexByName.TryGetValue(sName, out var si)) continue;
            // 测点顺序固定：南端 = 2m（Side=1），北端 = 2m+1（Side=0）
            var idx = si * 2 + (row.Side == 1 ? 0 : 1);
            if (idx >= 0 && idx < kctArray.Length) kctArray[idx] = ToDoubleArray(row);
        }

        // 4. 测点偏位 Bo（固定 10/13 项，不随 UseB 变化）：
        //    0 号段 10 项 [aNE,aNW,aSE,aSW,cN,cS,bNE,bNW,bSE,bSW]；
        //    标准段 13 项 [aNE1,aNW1,cN1,aNE2,aNW2,aSE2,aSW2,cN2,cS2,bNE2,bNW2,bSE2,bSW2]。
        //    a 始终取落库值；b 是否参与计算由 UseB 决定（false 时算法走旧表达式，b 值可填 0）。
        double[] bo;
        if (m == 0)
        {
            bo = new[]
            {
                (double)req.BoltOffsetANE, (double)req.BoltOffsetANW,
                (double)req.BoltOffsetASE, (double)req.BoltOffsetASW,
                (double)req.BoltOffsetCN, (double)req.BoltOffsetCS,
                (double)req.BoltOffsetBNE, (double)req.BoltOffsetBNW,
                (double)req.BoltOffsetBSE, (double)req.BoltOffsetBSW
            };
        }
        else
        {
            var prev = segments.FirstOrDefault(a => string.Equals(a.SegmentName, allSegments[m - 1].SegmentName, StringComparison.Ordinal))
                ?? throw new InvalidOperationException(BizMessages.PreviousSegmentNotFilled(Lang, allSegments[m - 1].SegmentName));
            bo = new[]
            {
                (double)prev.BoltOffsetANE, (double)prev.BoltOffsetANW, (double)prev.BoltOffsetCN,
                (double)req.BoltOffsetANE, (double)req.BoltOffsetANW,
                (double)req.BoltOffsetASE, (double)req.BoltOffsetASW,
                (double)req.BoltOffsetCN, (double)req.BoltOffsetCS,
                (double)req.BoltOffsetBNE, (double)req.BoltOffsetBNW,
                (double)req.BoltOffsetBSE, (double)req.BoltOffsetBSW
            };
        }

        // 4.1 UseB=true 时四个 b 值必须全部填写。DTO 为 decimal，无法区分“空”和 0，
        //     故以“四个 b 全为 0”作为兜底校验；正常 b 为横向半宽/坐标，不应全为 0。
        if (req.BoltOffsetBUseMeasured
            && req.BoltOffsetBNE == 0m && req.BoltOffsetBNW == 0m
            && req.BoltOffsetBSE == 0m && req.BoltOffsetBSW == 0m)
        {
            throw new InvalidOperationException(BizMessages.BoltOffsetBRequired(Lang));
        }

        // 5. 实测数据 E：0 号段 14 项 / 标准段 16 项，统一由 SetupNSurveyDTO 转换而来
        var input = new ComputeInput
        {
            M = m,
            St = 1,                        // 1=做实测修正并输出浇筑坐标；PerfectMatch 才用 0
            A = (double)config.OffsetA,
            B = (double)config.OffsetB,
            C = (double)config.OffsetC,
            H = (double)config.OffsetH,
            K = kArray,
            KT = ktArray,
            KCT = kctArray,
            Bo = bo,
            UseB = req.BoltOffsetBUseMeasured
        };
        if (m == 0) input.ZerothMeasuredE = ToZerothMeasuredE(req);
        else input.StandardMeasuredE = ToStandardMeasuredE(req);

        var measuredE = m == 0
            ? input.ZerothMeasuredE?.ToArray()
            : input.StandardMeasuredE?.ToArray();
        if (measuredE == null || measuredE.Any(v => double.IsNaN(v) || double.IsInfinity(v)) || measuredE.All(v => Math.Abs(v) < 1e-12))
            throw new InvalidOperationException(BizMessages.MeasuredDataRequired(Lang));

        var output = new ConstructionGirderAlignment().Run(input);
        if (!output.JudgePass)
            throw new InvalidOperationException(BizMessages.SurveyDataIncomplete(Lang));
        if (output.KT == null || output.KCT == null)
            throw new InvalidOperationException(BizMessages.NoCorrectedCoordinates(Lang));

        var jointIndexes = m == 0 ? new[] { 0, 1 } : new[] { m + 1 };
        foreach (var ji in jointIndexes)
            if (!AllFinite(output.KT[ji]))
                throw new InvalidOperationException(BizMessages.AlgorithmResultInvalid(Lang));
        if (!AllFinite(output.KCT[2 * m]) || !AllFinite(output.KCT[2 * m + 1]))
            throw new InvalidOperationException(BizMessages.AlgorithmResultInvalid(Lang));

        // 6. 落库：segment（upsert）→ ascast_coordinate（按 DesignCoordinateID）→ ascast_measure（按 SegmentID+Side）
        biz_concrete_beam_segment entity;
        try
        {
            Db.BeginTransaction();
            var isNew = existing == null;
            entity = existing ?? new biz_concrete_beam_segment
            {
                ProjID = context.ProjID,
                GirderID = context.GirderID,
                CastingGroupID = castingGroupID,
                Status = 0,
                CreatedBy = account,
                CreatedTime = now
            };
            if (isNew) Db.GetRepository<biz_concrete_beam_segment>().Add(entity);
            else { entity.UpdatedBy = account; entity.UpdatedTime = now; }
            ApplySurveyInput(entity, req, target);
            ApplyCalcResult(entity, output.CalcResult);
            // 先落 segment 拿到主键，再写以其为外键的测点行
            await Db.SaveAsync();

            // 0 号段一次产出两个端面（KT[0]、KT[1]）；标准段只产出北端面 KT[m+1]
            foreach (var ji in jointIndexes)
            {
                var designId = source[ji].ID;
                var row = ascastList.FirstOrDefault(a => a.DesignCoordinateID == designId);
                if (row == null)
                {
                    row = new biz_concrete_beam_ascast_coordinate
                    {
                        ProjID = context.ProjID,
                        GirderID = context.GirderID,
                        CastingGroupID = castingGroupID,
                        DesignCoordinateID = designId,
                        Status = 0,
                        CreatedBy = account,
                        CreatedTime = now
                    };
                    Db.GetRepository<biz_concrete_beam_ascast_coordinate>().Add(row);
                }
                else { row.UpdatedBy = account; row.UpdatedTime = now; }
                row.SegmentID = entity.ID;
                ApplyJointValues(row, output.KT[ji]);
            }

            // 本节两行测点：南端 KCT[2m] → Side=1，北端 KCT[2m+1] → Side=0
            UpsertMeasure(context, castingGroupID, entity.ID, 1, output.KCT[2 * m], measureList, account, now);
            UpsertMeasure(context, castingGroupID, entity.ID, 0, output.KCT[2 * m + 1], measureList, account, now);

            await Db.SaveAsync();
            Db.CommitTransaction();
        }
        catch (Exception ex)
        {
            Db.RollbackTransaction();
            throw ex;
        }

        var dto = ToSurveyDto(entity);
        ApplyCalcResultToDto(dto, output.CalcResult);
        dto.CastingGroupID = castingGroupID;
        dto.GriderID = context.GirderID;
        dto.CanEdit = true;
        return new ConcreteBeamSurveySaveResultDTO
        {
            Survey = dto,
            CorrectionApplied = true,
            Message = existing == null
                ? BizMessages.SurveySaved(Lang, m + 1)
                : BizMessages.SurveyRecalculated(Lang, m + 1),
            TheoreticalCenterLength = ToDecimal4(output.TheoreticalCenterLength),
            AsCastCenterLength = ToDecimal4(output.AsCastCenterLength),
            AsCastTotalCenterLength = ToDecimal4(output.AsCastTotalCenterLength)
        };
    }

    /// <summary>
    /// 立模参数计算（PerfectCast）：以 St=0 跑线形算法，得到"完美符合设计坐标时，本段的测量值应该是多少"，
    /// 即零偏差基准值。一次调用即可同时供两处使用：
    ///   ① 页面只读区（起始段=活动端模板，标准段=匹配梁段设置参数）—— 选/切梁段时填充；
    ///   ② 页面实测录入区的预填（勾选「精度达标」时按理论值覆盖）—— 现场在此基准上录入偏差。
    /// </summary>
    /// <remarks>
    /// 语义：基于<b>上一梁段已落库的实测数据</b>（as-cast 端面/测点）+ <b>当前梁段的设计数据</b>推算，
    /// 不做本段实测修正（St=0），因此不读本段的 Survey 录入值，Bo 中属于本段的偏位常数一律为 0。
    /// 关键：KT/KCT 必须按端面数 N / 段数 2(N-1) 预分配后再按定位键回填，
    /// 否则 St=0 时 KK[3]/KK[4]（上一节段南/北测点）取不到值，算出的立模参数会退化为纯理论值。
    /// 返回字段按归属端面分组（Match* / WetCast* / Floating* / Fixed*），与算法 CalcResult 同名。
    /// </remarks>
    public async Task<ConcreteBeamPerfectCastDTO> PerfectCast(PerfectCastReqDTO req)
    {
        if (req == null) throw new InvalidOperationException(BizMessages.ParameterRequired(Lang));
        if (string.IsNullOrWhiteSpace(req.SegmentNo)) throw new InvalidOperationException(BizMessages.SegmentNameRequiredForCalc(Lang));
        var castingGroupID = req.CastingGroupID;
        var context = await EnsureCastingGroupAccess(castingGroupID);
        var config = await GetSubmittedProjectConfig(castingGroupID);

        var allSegments = await LoadAllSegments(castingGroupID, true);
        var target = FindSegment(allSegments, req.SegmentNo);
        var m = target.SegmentIndex;

        var calculation = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).OrderBy(a => a.ID).ToListAsync();
        var source = await Db.Query<biz_concrete_beam_design_coordinate_source>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).OrderBy(a => a.ID).ToListAsync();
        if (calculation.Count != allSegments.Count + 1 || source.Count != calculation.Count)
            throw new InvalidOperationException(BizMessages.DesignSourceInconsistent(Lang));

        var jointCount = calculation.Count;
        var segmentCount = jointCount - 1;
        var kArray = calculation.Select(ToDoubleArray).ToArray();

        // KT/KCT 预分配后回填已落库的浇筑坐标与测点（定位键与 SaveSurvey 一致）
        var ascastList = await Db.Query<biz_concrete_beam_ascast_coordinate>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).ToListAsync();
        var measureList = await Db.Query<biz_concrete_beam_ascast_measure>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).ToListAsync();
        var segments = await Db.Query<biz_concrete_beam_segment>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).ToListAsync();

        var ktArray = NewMatrix(jointCount);
        var kctArray = NewMatrix(segmentCount * 2);
        var jointIndexBySourceId = new Dictionary<int, int>();
        for (var i = 0; i < source.Count; i++) jointIndexBySourceId[source[i].ID] = i;
        foreach (var row in ascastList)
            if (jointIndexBySourceId.TryGetValue(row.DesignCoordinateID, out var ji) && ji < jointCount)
                ktArray[ji] = ToDoubleArray(row);

        var segmentIndexByName = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var s in allSegments) segmentIndexByName[s.SegmentName] = s.SegmentIndex;
        var segmentNameById = new Dictionary<int, string>();
        foreach (var s in segments) segmentNameById[s.ID] = s.SegmentName;
        foreach (var row in measureList)
        {
            if (!segmentNameById.TryGetValue(row.SegmentID, out var sName)) continue;
            if (!segmentIndexByName.TryGetValue(sName, out var si)) continue;
            var idx = si * 2 + (row.Side == 1 ? 0 : 1);   // 南端 = 2m（Side=1），北端 = 2m+1（Side=0）
            if (idx >= 0 && idx < kctArray.Length) kctArray[idx] = ToDoubleArray(row);
        }

        // Bo：本段实测尚未录入，本段 a/b/c 一律 0；标准段带上一段的北侧 a（aNE1/aNW1）与 cN1，
        // 供上一节段测点烘焙。aNE1/aNW1 直接取上一段落库值（保存时已按“两侧不一致”归一化）。
        double[] bo;
        if (m == 0)
        {
            bo = new double[10];
        }
        else
        {
            var prev = segments.FirstOrDefault(a => string.Equals(a.SegmentName, allSegments[m - 1].SegmentName, StringComparison.Ordinal))
                ?? throw new InvalidOperationException(BizMessages.PreviousSegmentNotFilledForSetup(Lang, allSegments[m - 1].SegmentName));
            bo = new[]
            {
                (double)prev.BoltOffsetANE, (double)prev.BoltOffsetANW, (double)prev.BoltOffsetCN,
                0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d
            };
        }

        var input = new ComputeInput
        {
            M = m,
            St = 0,                     // 0=仅理论放样，不做实测修正
            A = (double)config.OffsetA,
            B = (double)config.OffsetB,
            C = (double)config.OffsetC,
            H = (double)config.OffsetH,
            K = kArray,
            KT = ktArray,
            KCT = kctArray,
            Bo = bo,
            UseB = false                // St=0 不走 ZeroErr/OneErr，b 不参与
        };

        var output = new ConstructionGirderAlignment().Run(input);
        if (!output.JudgePass)
            throw new InvalidOperationException(BizMessages.AlgorithmValidationFailed(Lang));
        var r = output.CalcResult;

        return new ConcreteBeamPerfectCastDTO
        {
            CastingGroupID = castingGroupID,
            GirderID = context.GirderID,
            SegmentName = target.SegmentName,
            SegmentIndex = m,
            IsFirst = m == 0,
            // 活动端模板（Floating Bulkhead，South 侧）：起始段只读区
            FloatingBulkheadEastElev = ToDecimal4(r.FloatingBulkheadEastElev),
            FloatingBulkheadWestElev = ToDecimal4(r.FloatingBulkheadWestElev),
            FloatingBulkheadCenterOffset = ToDecimal4(r.FloatingBulkheadCenterOffset),
            WetCastEastLength = ToDecimal4(r.WetCastEastLength),
            WetCastWestLength = ToDecimal4(r.WetCastWestLength),
            WetCastCenterLength = ToDecimal4(r.WetCastCenterLength),
            // 匹配端（上一梁段 as-cast 端面）：标准段只读区「匹配梁段设置参数」
            MatchNorthEastElev = ToDecimal4(r.MatchNorthEastElev),
            MatchNorthWestElev = ToDecimal4(r.MatchNorthWestElev),
            MatchSouthEastElev = ToDecimal4(r.MatchSouthEastElev),
            MatchSouthWestElev = ToDecimal4(r.MatchSouthWestElev),
            MatchNorthCenterOffset = ToDecimal4(r.MatchNorthCenterOffset),
            MatchSouthCenterOffset = ToDecimal4(r.MatchSouthCenterOffset),
            MatchCenterLength = ToDecimal4(r.MatchCenterLength),
            MatchCastTwist = ToDecimal4(r.MatchCastTwist),
            // 浇筑段（本段目标端面）：实测区「浇筑梁段」的理论值（首段为 0，前端按 IsFirst 走 Match*）
            WetCastSouthEastElev = ToDecimal4(r.WetCastSouthEastElev),
            WetCastSouthWestElev = ToDecimal4(r.WetCastSouthWestElev),
            WetCastNorthEastElev = ToDecimal4(r.WetCastNorthEastElev),
            WetCastNorthWestElev = ToDecimal4(r.WetCastNorthWestElev),
            WetCastSouthCenterOffset = ToDecimal4(r.WetCastSouthCenterOffset),
            WetCastNorthCenterOffset = ToDecimal4(r.WetCastNorthCenterOffset),
            // 固定端模板（Fixed Bulkhead，North 侧）
            FixedBulkheadEastElev = ToDecimal4(r.FixedBulkheadEastElev),
            FixedBulkheadWestElev = ToDecimal4(r.FixedBulkheadWestElev),
            FixedBulkheadCenterOffset = ToDecimal4(r.FixedBulkheadCenterOffset)
        };
    }

    /// <summary>
    /// 浇筑坐标页签：取某个梁段的端面坐标与定位点信息。
    /// </summary>
    /// <remarks>
    /// <b>全部读自已落库的坐标表，不重跑算法</b>：
    /// ① 理论坐标 ← design_coordinate_calculation（算法 K，含预拱度）；
    /// ② 浇筑坐标 ← ascast_coordinate（算法 KT，由 SaveSurvey 以 St=1 落库）；
    /// ③ 测点坐标 ← ascast_measure（算法 KCT，定位键 = SegmentID + Side；
    ///    注意该表两行都挂本段的 SegmentID，故任意段都能同时给出南/北两侧测点）。
    ///
    /// 唯一没有落库列的是「中线长度」4 个值（SaveSurvey 只把它们放进响应报文），
    /// 故在此按算法同一口径用 Dist3D 现算 —— 是<b>端面中心点的三维距离</b>，
    /// 与 WetCastCenterLength = C.X 那套局部系口径不是一回事，不可混用。
    ///
    /// 未填报的梁段在 ascast_coordinate / ascast_measure 里没有记录，
    /// 此时端面浇筑坐标与定位点信息返回 0 并由前端按 IsFilled 显示空态；理论坐标不受影响。
    /// </remarks>
    /// <param name="castingGroupID">浇筑组ID。</param>
    /// <param name="segmentName">梁段名称，格式 {节点1}-{节点2}。</param>
    public async Task<ConcreteBeamPouringCoordDTO> GetPouringCoords(int castingGroupID, string segmentName)
    {
        if (string.IsNullOrWhiteSpace(segmentName)) throw new InvalidOperationException(BizMessages.SegmentNameRequired(Lang));
        var context = await EnsureCastingGroupAccess(castingGroupID);
        var allSegments = await LoadAllSegments(castingGroupID, true);
        var target = FindSegment(allSegments, segmentName);
        var m = target.SegmentIndex;

        // 设计端面坐标：查询顺序即 joint 下标（SaveSurvey 取算例 K 也是这个顺序）
        var calculation = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete)
            .OrderBy(a => a.ID).ToListAsync();
        if (calculation.Count < m + 2)
            throw new InvalidOperationException(BizMessages.DesignCountNotMatchSegment(Lang));

        // 浇筑端面坐标：按 DesignCoordinateID → source.ID → joint 下标 定位（与 SaveSurvey 同键）
        var source = await Db.Query<biz_concrete_beam_design_coordinate_source>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete)
            .OrderBy(a => a.ID).ToListAsync();
        var jointIndexBySourceId = new Dictionary<int, int>();
        for (var i = 0; i < source.Count; i++) jointIndexBySourceId[source[i].ID] = i;
        var ascastList = await Db.Query<biz_concrete_beam_ascast_coordinate>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).ToListAsync();
        var ktByJoint = new Dictionary<int, double[]>();
        foreach (var row in ascastList)
            if (jointIndexBySourceId.TryGetValue(row.DesignCoordinateID, out var ji) && ji <= m + 1)
                ktByJoint[ji] = ToDoubleArray(row);

        // 本段填报记录：没有它说明本段尚未 SaveSurvey，浇筑坐标/测点一律为空
        var segments = await Db.Query<biz_concrete_beam_segment>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).ToListAsync();
        var thisSegment = segments.FirstOrDefault(a => string.Equals(a.SegmentName, target.SegmentName, StringComparison.Ordinal));

        // 本段测点：Side=0 北端、Side=1 南端，两行都挂本段 SegmentID
        var measureBySide = new Dictionary<int, double[]>();
        if (thisSegment != null)
        {
            var measureList = await Db.Query<biz_concrete_beam_ascast_measure>()
                .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete
                            && a.SegmentID == thisSegment.ID).ToListAsync();
            foreach (var row in measureList) measureBySide[row.Side] = ToDoubleArray(row);
        }

        var dto = new ConcreteBeamPouringCoordDTO
        {
            CastingGroupID = castingGroupID,
            GirderID = context.GirderID,
            SegmentName = target.SegmentName,
            SegmentIndex = m,
            IsFirst = m == 0,
            UpstreamSegmentName = target.Upstream ?? string.Empty,
            PouringDate = target.PouringDate,
            IsFilled = thisSegment != null
        };

        // 端面组：起始段 = 活动端(joint 0，南) + 固定端(joint 1，北)；标准段 = 固定端(joint M+1)
        // 实体注释印证：0 号段一次产出端面 0 与端面 1，第 m(≥1) 段只产出端面 m+1。
        var faceJoints = m == 0 ? new[] { 0, 1 } : new[] { m + 1 };
        foreach (var ji in faceJoints)
        {
            var k = ToDoubleArray(calculation[ji]);
            ktByJoint.TryGetValue(ji, out var kt);
            dto.Faces.Add(new ConcreteBeamEndFaceCoordDTO
            {
                FaceName = m == 0 && ji == 0 ? "活动端" : "固定端",
                NodeName = calculation[ji].NodeName ?? string.Empty,
                JointIndex = ji,
                HasAsCast = kt != null,
                Theoretical = EndFaceRows(k),
                AsCast = EndFaceRows(kt)
            });
        }

        // 定位点信息：一行测点自带 E/C/W 三点，南北两行摊成 4 个角点 + 2 个轴柱点
        measureBySide.TryGetValue(0, out var measureNorth);   // Side=0 北端
        measureBySide.TryGetValue(1, out var measureSouth);   // Side=1 南端
        dto.ElevationPoints = new List<ConcreteBeamCoordRowDTO>
        {
            CoordRow("东北侧点位", measureNorth, 0),
            CoordRow("西北侧点位", measureNorth, 6),
            CoordRow("东南侧点位", measureSouth, 0),
            CoordRow("西南侧点位", measureSouth, 6)
        };
        dto.AxisPoints = new List<ConcreteBeamCoordRowDTO>
        {
            CoordRow("北侧点位", measureNorth, 3),
            CoordRow("南侧点位", measureSouth, 3)
        };

        // 中线长度：口径同算法（ConstructionGirderAlignment L159-182）——端面中心点的三维距离
        var align = new ConstructionGirderAlignment();
        var kFirst = ToDoubleArray(calculation[0]);
        var kCur = ToDoubleArray(calculation[m]);
        var kLast = ToDoubleArray(calculation[m + 1]);
        dto.TheoreticalCenterLength = ToDecimal4(
            align.Dist3D(kCur[3], kCur[4], kCur[5], kLast[3], kLast[4], kLast[5]));
        dto.TheoreticalTotalCenterLength = ToDecimal4(
            align.Dist3D(kFirst[3], kFirst[4], kFirst[5], kLast[3], kLast[4], kLast[5]));
        if (ktByJoint.TryGetValue(m, out var ktCur) && ktByJoint.TryGetValue(m + 1, out var ktEnd))
            dto.AsCastCenterLength = ToDecimal4(
                align.Dist3D(ktCur[3], ktCur[4], ktCur[5], ktEnd[3], ktEnd[4], ktEnd[5]));
        if (ktByJoint.TryGetValue(0, out var ktStart) && ktByJoint.TryGetValue(m + 1, out var ktTail))
            dto.AsCastTotalCenterLength = ToDecimal4(
                align.Dist3D(ktStart[3], ktStart[4], ktStart[5], ktTail[3], ktTail[4], ktTail[5]));

        return dto;
    }

    #region 单梁段报告（【报告】页签）

    /// <summary>报告类型标识：梁段设置（浇筑前，只有 MATCH-CAST SETUP 一章）。</summary>
    public const string ReportTypeSetup = "setting";

    /// <summary>报告类型标识：已浇筑梁段（浇筑后，含 SURVEY 与 AS-CAST COORDINATES）。</summary>
    public const string ReportTypeAsCast = "baogao";

    /// <summary>
    /// 生成单梁段报告的 PDF 字节流，供【报告】页签的预览（PDF.js）与导出共用。
    /// </summary>
    /// <param name="castingGroupID">浇筑组ID。</param>
    /// <param name="segmentName">梁段名称，格式 {节点1}-{节点2}。</param>
    /// <param name="reportType"><see cref="ReportTypeSetup"/> / <see cref="ReportTypeAsCast"/>；其余值一律按梁段设置处理。</param>
    public async Task<byte[]> BuildSegmentReportPdf(int castingGroupID, string segmentName, string reportType,
        int? timezoneOffsetMinutes = null)
    {
        var kind = string.Equals(reportType, ReportTypeAsCast, StringComparison.OrdinalIgnoreCase)
            ? SegmentReportKind.AsCast
            : SegmentReportKind.Setup;
        var model = await GetSegmentReportModel(castingGroupID, segmentName, kind, timezoneOffsetMinutes);
        return ConcreteBeamSegmentReportBuilder.Build(model);
    }

    /// <summary>
    /// 组装单梁段报告的中间模型（不渲染）。
    /// </summary>
    /// <remarks>
    /// 四块数据的来源各自独立，需要注意：
    /// <list type="number">
    /// <item>头部 ← 浇筑组上下文（项目名 / 浇筑组名）+ 本段与上游段的浇筑日期；</item>
    /// <item>MATCH-CAST SETUP 章 ← <b>现算 PerfectCast(St=0)</b>：这 7 项从来没有落库列，
    ///       但结果稳定（只依赖已冻结的 KT[m] 与设计 K），故报告期重算 ≡ 浇筑当时的值；</item>
    /// <item>SURVEY 章 ← <c>biz_concrete_beam_segment</c> 的 Survey* 录入列（原始录入值）；</item>
    /// <item>AS-CAST COORDINATES 章 ← <c>ascast_coordinate</c> / <c>ascast_measure</c>，
    ///       长度按算法口径用 Dist3D 现算 —— 与「浇筑坐标」页签「中线长度 · 实测值」同源。</item>
    /// </list>
    /// 「已浇筑梁段」报告要求本段已填报；「梁段设置」报告不要求（它描述的是浇筑目标）。
    /// </remarks>
    public async Task<SegmentReportModel> GetSegmentReportModel(int castingGroupID, string segmentName, SegmentReportKind kind,
        int? timezoneOffsetMinutes = null)
    {
        if (string.IsNullOrWhiteSpace(segmentName)) throw new InvalidOperationException(BizMessages.SegmentNameRequired(Lang));

        var context = await EnsureCastingGroupAccess(castingGroupID);
        var allSegments = await LoadAllSegments(castingGroupID, true);
        var target = FindSegment(allSegments, segmentName);
        var m = target.SegmentIndex;
        var isFirst = m == 0;

        var records = await Db.Query<biz_concrete_beam_segment>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).ToListAsync();
        var thisRecord = records.FirstOrDefault(a =>
            string.Equals(a.SegmentName, target.SegmentName, StringComparison.Ordinal));

        if (kind == SegmentReportKind.AsCast && thisRecord == null)
            throw new InvalidOperationException($"梁段「{target.SegmentName}」尚未填报实测数据，无法生成「已浇筑梁段」报告");

        // 上游（匹配）段：名称取梁段列表给出的 Upstream，浇筑日期回查 segment 表
        var matchName = isFirst
            ? string.Empty
            : (target.Upstream ?? (m - 1 >= 0 && m - 1 < allSegments.Count ? allSegments[m - 1].SegmentName : string.Empty));
        DateTime? matchCastOn = null;
        if (!isFirst && !string.IsNullOrEmpty(matchName))
        {
            var upstreamRecord = records.FirstOrDefault(a =>
                string.Equals(a.SegmentName, matchName, StringComparison.Ordinal));
            matchCastOn = upstreamRecord?.PouringDate;
        }

        var model = new SegmentReportModel
        {
            Kind = kind,
            IsFirstSegment = isFirst,
            ProjectTitle = context.ProjectName,
            CastingSet = context.CastingGroupName,
            ReportDate = NowForClient(timezoneOffsetMinutes),
            TimezoneOffsetMinutes = timezoneOffsetMinutes,
            WetCastSegmentName = target.SegmentName,
            WetCastCastOn = ToClientLocal(thisRecord?.PouringDate, timezoneOffsetMinutes),
            MatchCastSegmentName = matchName,
            MatchCastCastOn = ToClientLocal(matchCastOn, timezoneOffsetMinutes)
        };

        // Setup 章：现算 St=0 的立模参数（与本段是否已填报无关）
        var setup = await PerfectCast(new PerfectCastReqDTO
        {
            CastingGroupID = castingGroupID,
            SegmentNo = target.SegmentName
        });
        AppendSetupChapter(model, setup, isFirst);

        if (kind == SegmentReportKind.AsCast)
        {
            AppendSurveyChapter(model, thisRecord!, isFirst);
            await AppendAsCastChapter(model, castingGroupID, thisRecord!, m);
        }

        return model;
    }

    /// <summary>
    /// MATCH-CAST SETUP 章。
    /// 标准段给出匹配端 4 角高程 + 北/南 Hairpin 偏移 + 所需浇筑中线长（共 7 项，与设站页
    /// 只读卡「匹配梁段设置参数」一一对应）；起始段无匹配段，改用活动端模板 + 固定端模板两组
    /// 目标值（共 8 项，与设站页首段只读区一一对应）。
    /// </summary>
    private static void AppendSetupChapter(SegmentReportModel model, ConcreteBeamPerfectCastDTO setup, bool isFirst)
    {
        var lines = model.Lines;
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, "MATCH-CAST SETUP ======================"));

        if (isFirst)
        {
            lines.Add(ReportLine.Title(8, "Floating Bulkhead (South End)", false));
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Pair(14, "East Elevation:", Num(setup.FloatingBulkheadEastElev)));
            lines.Add(ReportLine.Pair(14, "West Elevation:", Num(setup.FloatingBulkheadWestElev)));
            lines.Add(ReportLine.Pair(14, "Center Offset:", Num(setup.FloatingBulkheadCenterOffset)));
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Title(8, "Fixed Bulkhead (North End)", false));
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Pair(14, "East Elevation:", Num(setup.FixedBulkheadEastElev)));
            lines.Add(ReportLine.Pair(14, "West Elevation:", Num(setup.FixedBulkheadWestElev)));
            lines.Add(ReportLine.Pair(14, "Center Offset:", Num(setup.FixedBulkheadCenterOffset)));
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Pair(14, "East Side Length:", Num(setup.WetCastEastLength)));
            lines.Add(ReportLine.Pair(14, "West Side Length:", Num(setup.WetCastWestLength)));
            return;
        }

        lines.Add(ReportLine.Title(8, "Match-Cast Segment Elevations", false));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Pair(14, "North East Elevation:", Num(setup.MatchNorthEastElev)));
        lines.Add(ReportLine.Pair(14, "North West Elevation:", Num(setup.MatchNorthWestElev)));
        lines.Add(ReportLine.Pair(14, "South East Elevation:", Num(setup.MatchSouthEastElev)));
        lines.Add(ReportLine.Pair(14, "South West Elevation:", Num(setup.MatchSouthWestElev)));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, "Match-Cast Segment Center Offset", false));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Pair(14, "North Hairpin:", Num(setup.MatchNorthCenterOffset)));
        lines.Add(ReportLine.Pair(14, "South Hairpin:", Num(setup.MatchSouthCenterOffset)));
        lines.Add(ReportLine.Blank());
        // 取 MatchCenterLength（匹配端 as-built 的中线长），不是设计值 WetCastCenterLength
        lines.Add(ReportLine.Pair(14, "Required Wet-Cast Segment Length:", Num(setup.MatchCenterLength)));
    }

    /// <summary>
    /// SURVEY 章。全部取自 segment 表的录入列（原始测量记录，不做任何换算）。
    /// 标准段 = 固定端模板 3 + 匹配段 7（4 高程 + 扭转 + 2 偏移）+ 浇筑段 7；
    /// 起始段 = 固定端模板 3 + 活动端模板 3 + 浇筑段 8（长度用东西侧，无中线长、无扭转）。
    /// </summary>
    private static void AppendSurveyChapter(SegmentReportModel model, biz_concrete_beam_segment r, bool isFirst)
    {
        var lines = model.Lines;
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, "SURVEY ================================"));
        lines.Add(ReportLine.Title(8,
            $"Segment was Cast on : {ReportTime.WithOffset(FormatLongDate(model.WetCastCastOn ?? SegmentReportModel.NotCastSentinel), model.TimezoneOffsetMinutes)}", false));
        lines.Add(ReportLine.Blank());

        // 固定端模板位移（南北两段都有）
        lines.Add(ReportLine.Title(8, "Fixed Bulkhead Movement:", false));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Pair(18, "East Elevation:", Num(r.SurveyBulkheadEastElev)));
        lines.Add(ReportLine.Pair(18, "West Elevation:", Num(r.SurveyBulkheadWestElev)));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Pair(18, "Center Offset:", Num(r.SurveyBulkheadCenterOffset)));
        lines.Add(ReportLine.Blank());

        if (isFirst)
        {
            // 起始段无匹配段：本块换成活动端模板实测（对应设站页首段「活动端模板」三项）
            lines.Add(ReportLine.Title(8, "Floating Bulkhead Survey:", false));
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Title(8, "Floating Bulkhead Bolt Elevations", false));
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Pair(18, "South East:", Num(r.SurveyMatchSouthEastElev)));
            lines.Add(ReportLine.Pair(18, "South West:", Num(r.SurveyMatchSouthWestElev)));
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Title(8, "Floating Bulkhead Hairpin Offsets", false));
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Pair(18, "South Hairpin:", Num(r.SurveyMatchSouthCenterOffset)));
            lines.Add(ReportLine.Blank());
        }
        else
        {
            // 匹配段实测（对应设站页「匹配梁段」六项 + 扭转）
            lines.Add(ReportLine.Title(8, "Match-Cast Survey:", false));
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Title(8, "Match-Cast Bolt Elevations", false));
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Pair(18, "North East:", Num(r.SurveyMatchNorthEastElev)));
            lines.Add(ReportLine.Pair(18, "North West:", Num(r.SurveyMatchNorthWestElev)));
            lines.Add(ReportLine.Pair(18, "South East:", Num(r.SurveyMatchSouthEastElev)));
            lines.Add(ReportLine.Pair(18, "South West:", Num(r.SurveyMatchSouthWestElev)));
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Pair(8, "Recorded Twist Before Correction:", Num(r.MatchCastTwist)));
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Title(8, "Match-Cast Hairpin Offsets", false));
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Pair(18, "North Hairpin:", Num(r.SurveyMatchNorthCenterOffset)));
            lines.Add(ReportLine.Pair(18, "South Hairpin:", Num(r.SurveyMatchSouthCenterOffset)));
            lines.Add(ReportLine.Blank());
        }

        // 浇筑梁段实测（南北两段共有）
        lines.Add(ReportLine.Title(8, "Wet-Cast Bolt Elevations", false));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Pair(18, "North East:", Num(r.SurveyWetNorthEastElev)));
        lines.Add(ReportLine.Pair(18, "North West:", Num(r.SurveyWetNorthWestElev)));
        lines.Add(ReportLine.Pair(18, "South East:", Num(r.SurveyWetSouthEastElev)));
        lines.Add(ReportLine.Pair(18, "South West:", Num(r.SurveyWetSouthWestElev)));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, "Wet-Cast Hairpin Offsets", false));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Pair(18, "North Hairpin:", Num(r.SurveyWetNorthCenterOffset)));
        lines.Add(ReportLine.Pair(18, "South Hairpin:", Num(r.SurveyWetSouthCenterOffset)));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, "Wet-Cast Length", false));
        lines.Add(ReportLine.Blank());
        if (isFirst)
        {
            // 首段录入的是东西侧长度，没有中线长
            lines.Add(ReportLine.Pair(18, "East Side:", Num(r.SurveyWetEastLength)));
            lines.Add(ReportLine.Pair(18, "West Side:", Num(r.SurveyWetWestLength)));
        }
        else
        {
            lines.Add(ReportLine.Pair(18, "On Centerline:", Num(r.SurveyWetCenterLength)));
        }
    }

    /// <summary>
    /// AS-CAST COORDINATES 章：南北两端面的浇筑坐标 + 端面测点 + 实测中线长。
    /// 定位键与 SaveSurvey / GetPouringCoords 完全一致（DesignCoordinateID → source.ID → joint 下标）。
    /// </summary>
    private async Task AppendAsCastChapter(SegmentReportModel model, int castingGroupID,
        biz_concrete_beam_segment record, int m)
    {
        var source = await Db.Query<biz_concrete_beam_design_coordinate_source>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete)
            .OrderBy(a => a.ID).ToListAsync();
        var jointIndexBySourceId = new Dictionary<int, int>();
        for (var i = 0; i < source.Count; i++) jointIndexBySourceId[source[i].ID] = i;

        var ascastList = await Db.Query<biz_concrete_beam_ascast_coordinate>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).ToListAsync();
        var ktByJoint = new Dictionary<int, double[]>();
        foreach (var row in ascastList)
            if (jointIndexBySourceId.TryGetValue(row.DesignCoordinateID, out var ji))
                ktByJoint[ji] = ToDoubleArray(row);

        var measureList = await Db.Query<biz_concrete_beam_ascast_measure>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete
                        && a.SegmentID == record.ID).ToListAsync();
        var measureBySide = new Dictionary<int, double[]>();
        foreach (var row in measureList) measureBySide[row.Side] = ToDoubleArray(row);
        measureBySide.TryGetValue(0, out var measureNorth);   // Side=0 北端
        measureBySide.TryGetValue(1, out var measureSouth);   // Side=1 南端

        // 端面节点名：优先 segment 的 Node1/Node2，缺失时退化到设计坐标表的 NodeName
        var calculation = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete)
            .OrderBy(a => a.ID).ToListAsync();

        var lines = model.Lines;
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, "AS-CAST COORDINATES ================"));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Blank());

        ktByJoint.TryGetValue(m + 1, out var ktNorth);
        var northName = ResolveJointName(record.Node2, calculation, m + 1);
        lines.Add(ReportLine.Title(8, $"North Joint ({northName})", false));
        lines.Add(CoordLine(16, "East", ktNorth, 0));
        lines.Add(CoordLine(16, "Center", ktNorth, 3));
        lines.Add(CoordLine(16, "West", ktNorth, 6));
        lines.Add(ReportLine.Blank());

        ktByJoint.TryGetValue(m, out var ktSouth);
        var southName = ResolveJointName(record.Node1, calculation, m);
        lines.Add(ReportLine.Title(8, $"South Joint ({southName})", false));
        lines.Add(CoordLine(16, "East", ktSouth, 0));
        lines.Add(CoordLine(16, "Center", ktSouth, 3));
        lines.Add(CoordLine(16, "West", ktSouth, 6));
        lines.Add(ReportLine.Blank());

        // 端面测点：北行给 North Bolts，南行给 South Bolts，两者的中心点即 Hairpins
        lines.Add(ReportLine.Title(8, "Bolts and Hairpins", false));
        lines.Add(ReportLine.Title(8, "North Bolts", false));
        lines.Add(CoordLine(16, "East", measureNorth, 0));
        lines.Add(CoordLine(16, "West", measureNorth, 6));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, "South Bolts", false));
        lines.Add(CoordLine(16, "East", measureSouth, 0));
        lines.Add(CoordLine(16, "West", measureSouth, 6));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, "Hairpins", false));
        lines.Add(CoordLine(16, "North", measureNorth, 3));
        lines.Add(CoordLine(16, "South", measureSouth, 3));
        lines.Add(ReportLine.Blank());

        // 实测中线长：与「浇筑坐标」页签「中线长度 · 实测值」同口径（端面中心点三维距离）
        var align = new ConstructionGirderAlignment();
        var asCastLength = ktByJoint.TryGetValue(m, out var a) && ktByJoint.TryGetValue(m + 1, out var b)
            ? align.Dist3D(a[3], a[4], a[5], b[3], b[4], b[5])
            : 0d;
        lines.Add(ReportLine.Title(8, "Length", false));
        lines.Add(ReportLine.Pair(16, "As-Cast:", Num(asCastLength)));
    }

    /// <summary>端面节点名：segment 的 Node1/Node2 优先，缺失时退化到设计坐标表的 NodeName。</summary>
    private static string ResolveJointName(string? recordNodeName, List<biz_concrete_beam_design_coordinate_calculation> calculation, int jointIndex)
    {
        if (!string.IsNullOrWhiteSpace(recordNodeName)) return recordNodeName!;
        return jointIndex >= 0 && jointIndex < calculation.Count
            ? (calculation[jointIndex].NodeName ?? string.Empty)
            : string.Empty;
    }

    /// <summary>坐标行：标签 + 三个 4 位小数分量（等宽字体下按列对齐）。</summary>
    private static ReportLine CoordLine(int leadingSpaces, string label, double[]? v, int idx)
    {
        var arr = v ?? new double[9];
        return new ReportLine
        {
            LeadingSpaces = leadingSpaces,
            Text = label.PadRight(14)
                   + Num(arr[idx]).PadLeft(12) + "  "
                   + Num(arr[idx + 1]).PadLeft(12) + "  "
                   + Num(arr[idx + 2]).PadLeft(12)
        };
    }

    /// <summary>报告数值格式：固定 4 位小数、不变文化（避免服务端区域设置影响小数点）。</summary>
    private static string Num(decimal v) => v.ToString("0.0000", CultureInfo.InvariantCulture);

    /// <summary>报告数值格式：固定 4 位小数、不变文化。</summary>
    private static string Num(double v) => ((decimal)v).ToString("0.0000", CultureInfo.InvariantCulture);

    /// <summary>
    /// 把 DB 里的 UTC 墙钟按客户端时区偏移还原成本地墙钟。
    /// <para>DB 约定为 timestamp without time zone，时区由前端决定；PDF 由服务端渲染，
    /// 因此前端把浏览器偏移（分钟，东八区=480）传进来，这里只做偏移换算，不依赖服务器时区。</para>
    /// </summary>
    private static DateTime? ToClientLocal(DateTime? value, int? timezoneOffsetMinutes)
    {
        if (!value.HasValue) return null;
        if (!timezoneOffsetMinutes.HasValue) return value;
        return DateTime.SpecifyKind(value.Value, DateTimeKind.Utc).AddMinutes(timezoneOffsetMinutes.Value);
    }

    /// <summary>报告生成时间：有客户端偏移时按客户端本地时间，否则用服务器当前时间。</summary>
    private static DateTime NowForClient(int? timezoneOffsetMinutes) =>
        timezoneOffsetMinutes.HasValue
            ? DateTime.UtcNow.AddMinutes(timezoneOffsetMinutes.Value)
            : DateTime.Now;

    /// <summary>英文长日期（Friday, October 06, 2023），与原报告一致。</summary>
    private static string FormatLongDate(DateTime date)
        => date.ToString("dddd, MMMM dd, yyyy", CultureInfo.InvariantCulture);

    #endregion

    #region 整体报告（【报告】页签 · 整体报告）

    /// <summary>整体报告类型标识：项目信息。</summary>
    public const string ReportTypeProject = "project";

    /// <summary>整体报告类型标识：浇筑和理论坐标。</summary>
    public const string ReportTypeCastTheory = "casttheory";

    /// <summary>整体报告类型标识：理论坐标（joint 范围由 includeAll 决定）。</summary>
    public const string ReportTypeTheory = "theory";

    /// <summary>
    /// 生成整体报告的 PDF 字节流，供【报告】页签的预览（PDF.js）与导出共用。
    /// </summary>
    /// <param name="castingGroupID">浇筑组ID。</param>
    /// <param name="reportType">
    /// <see cref="ReportTypeProject"/> / <see cref="ReportTypeCastTheory"/> / <see cref="ReportTypeTheory"/>；
    /// 其余值一律按理论坐标处理。
    /// </param>
    /// <param name="includeAll">仅对理论坐标生效：true = 列出全部 joint，false = 只列到已浇筑梁段的最大 joint。</param>
    public async Task<byte[]> BuildOverallReportPdf(int castingGroupID, string reportType, bool includeAll,
        int? timezoneOffsetMinutes = null)
    {
        var kind = ParseOverallReportKind(reportType);
        var model = await GetOverallReportModel(castingGroupID, kind, includeAll, timezoneOffsetMinutes);
        return ConcreteBeamOverallReportBuilder.Build(model);
    }

    private static OverallReportKind ParseOverallReportKind(string reportType)
    {
        if (string.Equals(reportType, ReportTypeProject, StringComparison.OrdinalIgnoreCase))
            return OverallReportKind.Project;
        if (string.Equals(reportType, ReportTypeCastTheory, StringComparison.OrdinalIgnoreCase))
            return OverallReportKind.CastAndTheory;
        return OverallReportKind.Theory;
    }

    /// <summary>
    /// 组装整体报告的中间模型（不渲染）。
    /// </summary>
    /// <remarks>
    /// 三份报告共用同一批坐标数据，区别只在组织方式：
    /// <list type="bullet">
    /// <item>项目信息 ← 浇筑组上下文 + config 表 + 梁段填报现状（只给统计值，不给逐段明细）；</item>
    /// <item>浇筑和理论坐标 ← 逐梁段、逐端面给出「理论值 / 浇筑值」对照，未浇筑段的浇筑列留空；</item>
    /// <item>理论坐标 ← 按 joint 列设计坐标，「包含所有」时才列到最后一个 joint。</item>
    /// </list>
    /// 坐标全部读表不重算：理论值 ← design_coordinate_calculation，浇筑值 ← ascast_coordinate
    /// （定位键 DesignCoordinateID → source.ID → joint 下标，与 GetPouringCoords / SaveSurvey 同源）。
    /// </remarks>
    public async Task<OverallReportModel> GetOverallReportModel(int castingGroupID, OverallReportKind kind, bool includeAll,
        int? timezoneOffsetMinutes = null)
    {
        var context = await EnsureCastingGroupAccess(castingGroupID);
        var model = new OverallReportModel
        {
            Kind = kind,
            ProjectTitle = context.ProjectName,
            CastingSet = context.CastingGroupName,
            ReportDate = NowForClient(timezoneOffsetMinutes),
            TimezoneOffsetMinutes = timezoneOffsetMinutes,
            IncludeAll = includeAll
        };

        var (calculation, ktByJoint) = await LoadJointCoordinates(castingGroupID);
        var allSegments = await LoadAllSegments(castingGroupID, true);

        switch (kind)
        {
            case OverallReportKind.Project:
                await AppendProjectChapter(model, castingGroupID, context, calculation, allSegments);
                break;
            case OverallReportKind.CastAndTheory:
                AppendCastAndTheoryChapter(model, calculation, ktByJoint, allSegments, timezoneOffsetMinutes);
                break;
            default:
                AppendTheoryChapter(model, calculation, allSegments, includeAll);
                break;
        }

        return model;
    }

    /// <summary>
    /// 一次性取回「设计端面坐标（理论值）」与「浇筑端面坐标（实测修正后的值）」，按 joint 下标索引。
    /// 定位键与 SaveSurvey / GetPouringCoords 完全一致，避免报告与页签取到不同的坐标口径。
    /// </summary>
    private async Task<(List<biz_concrete_beam_design_coordinate_calculation> Calculation,
        Dictionary<int, double[]> KtByJoint)> LoadJointCoordinates(int castingGroupID)
    {
        var calculation = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete)
            .OrderBy(a => a.ID).ToListAsync();

        var source = await Db.Query<biz_concrete_beam_design_coordinate_source>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete)
            .OrderBy(a => a.ID).ToListAsync();
        var jointIndexBySourceId = new Dictionary<int, int>();
        for (var i = 0; i < source.Count; i++) jointIndexBySourceId[source[i].ID] = i;

        var ascastList = await Db.Query<biz_concrete_beam_ascast_coordinate>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete).ToListAsync();
        var ktByJoint = new Dictionary<int, double[]>();
        foreach (var row in ascastList)
            if (jointIndexBySourceId.TryGetValue(row.DesignCoordinateID, out var ji))
                ktByJoint[ji] = ToDoubleArray(row);

        return (calculation, ktByJoint);
    }

    /// <summary>
    /// 项目信息章：浇筑组基础参数 + 设计坐标现状 + 梁段填报统计。
    /// 未提交过项目信息（config 为空）时，四个偏移量显示 "--" 而不是 0，避免被误读成已测值。
    /// </summary>
    private async Task AppendProjectChapter(OverallReportModel model, int castingGroupID,
        CastingGroupContext context, List<biz_concrete_beam_design_coordinate_calculation> calculation,
        List<ConcreteBeamSegmentDTO> allSegments)
    {
        var config = await Db.Query<biz_concrete_beam_config>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
            .OrderByDescending(a => a.ID).FirstOrDefaultAsync();
        var castCount = allSegments.Count(a => a.IsFilled);

        var lines = model.Lines;
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, "PROJECT INFORMATION ===================="));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Pair(14, "Project Name:", context.ProjectName));
        lines.Add(ReportLine.Pair(14, "Bridge Name:", context.BridgeName));
        lines.Add(ReportLine.Pair(14, "Casting Set:", context.CastingGroupName));
        lines.Add(ReportLine.Pair(14, "Manager:", BlankIfEmpty(context.ManagerName)));
        lines.Add(ReportLine.Pair(14, "Created By:", BlankIfEmpty(context.CreatedBy)));
        lines.Add(ReportLine.Pair(14, "Created Time:", context.CreatedTime.HasValue
            ? context.CreatedTime.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            : "--"));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, "Geometry Offsets", false));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Pair(14, "Offset A:", NumOrBlank(config?.OffsetA)));
        lines.Add(ReportLine.Pair(14, "Offset B:", NumOrBlank(config?.OffsetB)));
        lines.Add(ReportLine.Pair(14, "Offset C:", NumOrBlank(config?.OffsetC)));
        lines.Add(ReportLine.Pair(14, "Reference Elevation (H):", NumOrBlank(config?.OffsetH)));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, "Design Coordinates", false));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Pair(14, "Joints:", calculation.Count.ToString(CultureInfo.InvariantCulture)));
        lines.Add(ReportLine.Pair(14, "Elevation Includes Camber:",
            config != null && config.ElevationIncludesCamber ? "Yes" : "No"));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, "Segments", false));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Pair(14, "Total Segments:", allSegments.Count.ToString(CultureInfo.InvariantCulture)));
        lines.Add(ReportLine.Pair(14, "Cast Segments:", castCount.ToString(CultureInfo.InvariantCulture)));
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Pair(14, "Report Generated:",
            ReportTime.WithOffset(model.ReportDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                model.TimezoneOffsetMinutes)));
    }

    /// <summary>
    /// 浇筑和理论坐标章：逐梁段列出南北两端面的「理论值 / 浇筑值」对照，末尾给中线长对照。
    /// 端面归属与 <see cref="GetPouringCoords"/> 一致：North = joint m+1，South = joint m。
    /// 尚未浇筑的梁段没有 as-cast 记录，对应列留空（不是 0，避免被当成实测值）。
    /// </summary>
    private static void AppendCastAndTheoryChapter(OverallReportModel model,
        List<biz_concrete_beam_design_coordinate_calculation> calculation,
        Dictionary<int, double[]> ktByJoint, List<ConcreteBeamSegmentDTO> allSegments,
        int? timezoneOffsetMinutes = null)
    {
        var align = new ConstructionGirderAlignment();
        var lines = model.Lines;
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, "CAST AND THEORETICAL COORDINATES ======="));

        foreach (var seg in allSegments)
        {
            var m = seg.SegmentIndex;
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Title(8, $"Segment {m + 1} : {seg.SegmentName}", false));
            lines.Add(ReportLine.Title(8, "  Cast on : " + (seg.PouringDate.HasValue
                ? ReportTime.WithOffset(FormatLongDate(ToClientLocal(seg.PouringDate, timezoneOffsetMinutes)!.Value), timezoneOffsetMinutes)
                : "Not Cast"), false));

            ktByJoint.TryGetValue(m + 1, out var ktNorth);
            ktByJoint.TryGetValue(m, out var ktSouth);
            AppendFaceBlock(lines, "North Joint", ResolveJointName(seg.Node2, calculation, m + 1),
                ToDoubleArray(calculation[m + 1]), ktNorth);
            AppendFaceBlock(lines, "South Joint", ResolveJointName(seg.Node1, calculation, m),
                ToDoubleArray(calculation[m]), ktSouth);

            // 中线长：端面中心点的三维距离，与「浇筑坐标」页签同源
            var kNorth = ToDoubleArray(calculation[m + 1]);
            var kSouth = ToDoubleArray(calculation[m]);
            var theoryLength = align.Dist3D(kSouth[3], kSouth[4], kSouth[5], kNorth[3], kNorth[4], kNorth[5]);
            var asCastLength = ktNorth != null && ktSouth != null
                ? align.Dist3D(ktSouth[3], ktSouth[4], ktSouth[5], ktNorth[3], ktNorth[4], ktNorth[5])
                : (double?)null;
            lines.Add(ReportLine.Title(8, "Centerline Length", false));
            lines.Add(PairLine(16, "Theory :", Num(theoryLength)));
            lines.Add(PairLine(16, "As-Cast:", asCastLength.HasValue ? Num(asCastLength.Value) : "--"));
        }
    }

    /// <summary>一个端面的三行（东 / 中心 / 西），每行两小行：理论值与浇筑值。</summary>
    private static void AppendFaceBlock(List<ReportLine> lines, string caption, string nodeName,
        double[] theory, double[]? asCast)
    {
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, $"{caption} ({nodeName})", false));
        AppendPointPair(lines, "East", theory, asCast, 0);
        AppendPointPair(lines, "Center", theory, asCast, 3);
        AppendPointPair(lines, "West", theory, asCast, 6);
    }

    private static void AppendPointPair(List<ReportLine> lines, string label,
        double[] theory, double[]? asCast, int idx)
    {
        lines.Add(new ReportLine
        {
            LeadingSpaces = 16,
            Text = label.PadRight(10) + "Theory :" + FormatTriple(theory, idx)
        });
        lines.Add(new ReportLine
        {
            LeadingSpaces = 16,
            Text = new string(' ', 10) + "As-Cast:" + FormatTriple(asCast, idx)
        });
    }

    /// <summary>三元组（X / Y / Z）右对齐；没有浇筑值时用空格占位，保持列不错位。</summary>
    private static string FormatTriple(double[]? v, int idx)
    {
        var blank = new string(' ', 12);
        if (v == null) return "  " + blank + "  " + blank + "  " + blank;
        return "  " + Num(v[idx]).PadLeft(12) + "  " + Num(v[idx + 1]).PadLeft(12) + "  " + Num(v[idx + 2]).PadLeft(12);
    }

    /// <summary>窄版「标签 : 值」行（整体报告不用 ReportLine.Pair 的 46 字符标签列）。</summary>
    private static ReportLine PairLine(int leadingSpaces, string label, string value)
        => new ReportLine { LeadingSpaces = leadingSpaces, Text = label.PadRight(10) + value.PadLeft(12) };

    /// <summary>
    /// 理论坐标章：按 joint 列出设计坐标（含预拱度）。
    /// 「包含所有」未勾选时只列到已浇筑梁段的最大 joint —— 未浇筑段的坐标虽已算好，
    /// 但尚未进入施工，列出来容易与已施工部分混淆；一个都没浇筑时退化为全部列出。
    /// </summary>
    private static void AppendTheoryChapter(OverallReportModel model,
        List<biz_concrete_beam_design_coordinate_calculation> calculation,
        List<ConcreteBeamSegmentDTO> allSegments, bool includeAll)
    {
        var lines = model.Lines;
        lines.Add(ReportLine.Blank());
        lines.Add(ReportLine.Title(8, includeAll
            ? "THEORETICAL COORDINATES (ALL JOINTS) ====="
            : "THEORETICAL COORDINATES ================="));

        var maxFilled = -1;
        for (var i = 0; i < allSegments.Count; i++) if (allSegments[i].IsFilled) maxFilled = i;
        var lastJoint = includeAll || maxFilled < 0
            ? calculation.Count - 1
            : Math.Min(maxFilled + 1, calculation.Count - 1);

        if (maxFilled < 0)
        {
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Title(8, "No segment has been cast yet; all joints are listed.", false));
        }
        else if (!includeAll)
        {
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Title(8, $"Joints are listed up to the last cast segment (joint {lastJoint}).", false));
        }

        for (var j = 0; j <= lastJoint; j++)
        {
            var node = calculation[j];
            var k = ToDoubleArray(node);
            lines.Add(ReportLine.Blank());
            lines.Add(ReportLine.Title(8, $"Joint {j} ({node.NodeName ?? string.Empty})", false));
            lines.Add(CoordLine(16, "East", k, 0));
            lines.Add(CoordLine(16, "Center", k, 3));
            lines.Add(CoordLine(16, "West", k, 6));
        }
    }

    private static string NumOrBlank(decimal? v) => v.HasValue ? Num(v.Value) : "--";

    private static string BlankIfEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? "--" : v!;

    #endregion

    /// <summary>
    /// 端面三行表格：西侧控制点 / 中心线 / 东侧控制点。
    /// 入参为 9 列数组 [E(0-2), C(3-5), W(6-8)]，为 null 时三行全 0（caller 用 HasAsCast 表达空态）。
    /// </summary>
    private List<ConcreteBeamCoordRowDTO> EndFaceRows(double[] v) => new()
    {
        CoordRow("西侧控制点", v, 6),
        CoordRow("中心线", v, 3),
        CoordRow("东侧控制点", v, 0)
    };

    /// <summary>取 9 列数组里第 idx 起的三元组（0=东, 3=中心, 6=西）作为一行点位。</summary>
    private ConcreteBeamCoordRowDTO CoordRow(string label, double[] v, int idx)
    {
        var arr = v ?? new double[9];
        return new ConcreteBeamCoordRowDTO
        {
            Label = label,
            X = ToDecimal4(arr[idx]),
            Y = ToDecimal4(arr[idx + 1]),
            Z = ToDecimal4(arr[idx + 2])
        };
    }

    #region Survey 辅助方法

    /// <summary>
    /// 加载浇筑组下的全部梁段（不分页），并为每段标注填报状态（IsFilled / CanEdit / LockedReason）。
    /// 梁段由相邻端面派生：N 个端面 → N-1 个梁段。
    /// SegmentIndex 为全局几何序号（等于算法入参 M），不是页内下标。
    /// </summary>
    private async Task<List<ConcreteBeamSegmentDTO>> LoadAllSegments(int castingGroupID, bool useCalculation)
    {
        var nodeNames = useCalculation
            ? await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
                .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete)
                .OrderBy(a => a.ID).Select(a => a.NodeName).ToListAsync()
            : await Db.Query<biz_concrete_beam_design_coordinate_source>()
                .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete)
                .OrderBy(a => a.ID).Select(a => a.NodeName).ToListAsync();
        if (useCalculation) await EnsureCalculationMatchesSource(castingGroupID, nodeNames);
        // 一次查询同时拿到已填报的梁段名称与浇筑日期：名称用于判定 IsFilled，日期用于列表展示
        var filled = await Db.Query<biz_concrete_beam_segment>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != StatusDelete)
            .Select(a => new { a.SegmentName, a.PouringDate }).ToListAsync();
        var pouringDates = new Dictionary<string, DateTime?>(StringComparer.Ordinal);
        var filledNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in filled)
        {
            filledNames.Add(row.SegmentName);
            if (row.PouringDate != null) pouringDates[row.SegmentName] = row.PouringDate;
        }
        var list = new List<ConcreteBeamSegmentDTO>();
        for (var i = 0; i + 1 < nodeNames.Count; i++)
        {
            var segmentName = $"{nodeNames[i]}-{nodeNames[i + 1]}";
            list.Add(new ConcreteBeamSegmentDTO
            {
                SegmentIndex = i,
                SegmentName = segmentName,
                // 首段与其他梁段的录入字段不同（首段录固定端模三项 + 东西侧浇筑长度，无匹配面实测值/扭转值），
                // 前端据此切换表单，故显式给出标记而不是让前端去比 SegmentIndex == 0
                IsFirst = i == 0,
                Node1 = nodeNames[i],
                Node2 = nodeNames[i + 1],
                Upstream = i > 0 ? $"{nodeNames[i - 1]}-{nodeNames[i]}" : null,
                Downstream = i + 2 < nodeNames.Count ? $"{nodeNames[i + 1]}-{nodeNames[i + 2]}" : null,
                PouringDate = pouringDates.TryGetValue(segmentName, out var poured) ? poured : null,
                IsFilled = filledNames.Contains(segmentName)
            });
        }
        ApplyFillStatus(list);
        return list;
    }

    private static ConcreteBeamSegmentDTO FindSegment(List<ConcreteBeamSegmentDTO> allSegments, string segmentName)
    {
        var key = segmentName.Trim();
        return allSegments.FirstOrDefault(a => string.Equals(a.SegmentName, key, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"无此梁段（或设计端面已变更）：{key}");
    }

    /// <summary>
    /// 按"不跳段、不返工、下一梁段填报前可改本段"给每段标注 CanEdit 与不可改原因。
    /// </summary>
    private void ApplyFillStatus(List<ConcreteBeamSegmentDTO> allSegments)
    {
        var filledMax = FilledMaxIndex(allSegments);
        foreach (var s in allSegments)
        {
            var (canEdit, reason) = ConcreteBeamWorkflowRules.DescribeSegmentFill(filledMax, s.SegmentIndex, s.IsFilled, Lang);
            s.CanEdit = canEdit;
            s.LockedReason = reason;
        }
    }

    /// <summary>
    /// 已填报梁段的最大几何序号（无任何填报时返回 -1）。
    /// 序号由 SegmentName 在端面序列中的位置反推，不读 segment.SegmentIndex 列——
    /// 该列是后加的，存量行的历史值（默认 0）不可信。
    /// </summary>
    private static int FilledMaxIndex(List<ConcreteBeamSegmentDTO> allSegments)
    {
        var max = -1;
        foreach (var s in allSegments)
            if (s.IsFilled) max = Math.Max(max, s.SegmentIndex);
        return max;
    }

    /// <summary>构造 rows×9 的零矩阵（算法 K/KT/KCT 的行布局）。</summary>
    private static double[][] NewMatrix(int rows)
    {
        var arr = new double[rows][];
        for (var i = 0; i < rows; i++) arr[i] = new double[9];
        return arr;
    }

    /// <summary>
    /// 0 号段（m=0）实测 E，14 项；字段顺序由 MeasuredEZerothSegment.ToArray() 固定，不可调整。
    /// FixedBulkhead=固定端、FloatingBulkhead=活动端/匹配端。
    /// </summary>
    private static MeasuredEZerothSegment ToZerothMeasuredE(SetupNSurveyDTO r) => new()
    {
        FixedBulkheadCenterOffset = (double)r.SurveyBulkheadCenterOffset,        // 0 仅记录
        WetCastWestLength = (double)r.SurveyWetWestLength,                        // 1
        FloatingBulkheadCenterOffset = (double)r.SurveyMatchSouthCenterOffset,    // 2
        WetCastEastLength = (double)r.SurveyWetEastLength,                        // 3
        WetCastSouthEastElev = (double)r.SurveyWetSouthEastElev,                  // 4
        WetCastNorthEastElev = (double)r.SurveyWetNorthEastElev,                  // 5
        WetCastSouthWestElev = (double)r.SurveyWetSouthWestElev,                  // 6
        WetCastNorthWestElev = (double)r.SurveyWetNorthWestElev,                  // 7
        FloatingBulkheadEastElev = (double)r.SurveyMatchSouthEastElev,            // 8
        FloatingBulkheadWestElev = (double)r.SurveyMatchSouthWestElev,            // 9
        FixedBulkheadEastElev = (double)r.SurveyBulkheadEastElev,                 // 10
        FixedBulkheadWestElev = (double)r.SurveyBulkheadWestElev,                 // 11
        WetCastCenterOffsetSouth = (double)r.SurveyWetSouthCenterOffset,          // 12
        WetCastCenterOffsetNorth = (double)r.SurveyWetNorthCenterOffset           // 13
    };

    /// <summary>
    /// 标准段（m≥1）实测 E，16 项；字段顺序由 MeasuredEStandardSegment.ToArray() 固定，不可调整。
    /// </summary>
    private static MeasuredEStandardSegment ToStandardMeasuredE(SetupNSurveyDTO r) => new()
    {
        FixedBulkheadCenterOffset = (double)r.SurveyBulkheadCenterOffset,              // 0 仅记录
        WetCastCenterLength = (double)r.SurveyWetCenterLength,                    // 1
        MatchCastCenterOffsetSouth = (double)r.SurveyMatchSouthCenterOffset,      // 2
        MatchCastCenterOffsetNorth = (double)r.SurveyMatchNorthCenterOffset,      // 3
        FixedBulkheadEastElev = (double)r.SurveyBulkheadEastElev,                      // 4
        FixedBulkheadWestElev = (double)r.SurveyBulkheadWestElev,                      // 5
        MatchCastSouthEastElev = (double)r.SurveyMatchSouthEastElev,              // 6
        MatchCastSouthWestElev = (double)r.SurveyMatchSouthWestElev,              // 7
        MatchCastNorthEastElev = (double)r.SurveyMatchNorthEastElev,              // 8
        MatchCastNorthWestElev = (double)r.SurveyMatchNorthWestElev,              // 9
        WetCastSouthEastElev = (double)r.SurveyWetSouthEastElev,                  // 10
        WetCastSouthWestElev = (double)r.SurveyWetSouthWestElev,                  // 11
        WetCastNorthEastElev = (double)r.SurveyWetNorthEastElev,                  // 12
        WetCastNorthWestElev = (double)r.SurveyWetNorthWestElev,                  // 13
        WetCastCenterOffsetSouth = (double)r.SurveyWetSouthCenterOffset,          // 14
        WetCastCenterOffsetNorth = (double)r.SurveyWetNorthCenterOffset           // 15
    };

    /// <summary>把前端录入（实测 + 偏位常数 + 浇筑日期）写入梁段实体。</summary>
    private static void ApplySurveyInput(biz_concrete_beam_segment entity, SetupNSurveyDTO req,
        ConcreteBeamSegmentDTO target)
    {
        entity.SegmentIndex = target.SegmentIndex;
        entity.SegmentName = target.SegmentName;
        entity.Node1 = target.Node1;
        entity.Node2 = target.Node2;
        entity.Upstream = target.Upstream;
        entity.Downstream = target.Downstream;
        entity.PouringDate = req.PouringDate;

        // 测点偏位：a 四角 + b 四角 + 三个标志。
        // 归一化只发生在保存业务：未勾选“两侧不一致”时，西角复制东角；
        // 算法始终直接读取四角落库值，不做归一化分支。
        var aNE = req.BoltOffsetANE;
        var aNW = req.BoltOffsetAUseNorth ? req.BoltOffsetANW : aNE;
        var aSE = req.BoltOffsetASE;
        var aSW = req.BoltOffsetAUseSouth ? req.BoltOffsetASW : aSE;

        entity.BoltOffsetANE = aNE;
        entity.BoltOffsetANW = aNW;
        entity.BoltOffsetASE = aSE;
        entity.BoltOffsetASW = aSW;
        entity.BoltOffsetBNE = req.BoltOffsetBNE;
        entity.BoltOffsetBNW = req.BoltOffsetBNW;
        entity.BoltOffsetBSE = req.BoltOffsetBSE;
        entity.BoltOffsetBSW = req.BoltOffsetBSW;

        // 旧列同步（兼容历史报表/旧查询）
        entity.BoltOffsetAN = aNE;
        entity.BoltOffsetAS = aSE;
        entity.BoltOffsetCN = req.BoltOffsetCN;
        entity.BoltOffsetCS = req.BoltOffsetCS;

        // 标志列：useAN/useAS 仅用于回显与保存归一化；useB 进入算法分支
        entity.BoltOffsetAUseNorth = req.BoltOffsetAUseNorth ? (short)1 : (short)0;
        entity.BoltOffsetAUseSouth = req.BoltOffsetAUseSouth ? (short)1 : (short)0;
        entity.BoltOffsetBUseMeasured = req.BoltOffsetBUseMeasured ? (short)1 : (short)0;

        entity.SurveyMatchNorthEastElev = req.SurveyMatchNorthEastElev;
        entity.SurveyMatchNorthWestElev = req.SurveyMatchNorthWestElev;
        entity.SurveyMatchSouthEastElev = req.SurveyMatchSouthEastElev;
        entity.SurveyMatchSouthWestElev = req.SurveyMatchSouthWestElev;
        entity.SurveyMatchNorthCenterOffset = req.SurveyMatchNorthCenterOffset;
        entity.SurveyMatchSouthCenterOffset = req.SurveyMatchSouthCenterOffset;
        entity.SurveyWetNorthEastElev = req.SurveyWetNorthEastElev;
        entity.SurveyWetNorthWestElev = req.SurveyWetNorthWestElev;
        entity.SurveyWetSouthEastElev = req.SurveyWetSouthEastElev;
        entity.SurveyWetSouthWestElev = req.SurveyWetSouthWestElev;
        entity.SurveyWetNorthCenterOffset = req.SurveyWetNorthCenterOffset;
        entity.SurveyWetSouthCenterOffset = req.SurveyWetSouthCenterOffset;
        entity.SurveyWetCenterLength = req.SurveyWetCenterLength;
        entity.SurveyWetEastLength = req.SurveyWetEastLength;
        entity.SurveyWetWestLength = req.SurveyWetWestLength;
        entity.SurveyBulkheadEastElev = req.SurveyBulkheadEastElev;
        entity.SurveyBulkheadWestElev = req.SurveyBulkheadWestElev;
        entity.SurveyBulkheadCenterOffset = req.SurveyBulkheadCenterOffset;
    }

    /// <summary>
    /// 把算法算出的立模参数写回梁段实体（Match* 是算法产出，不是录入项）。
    /// 东西长度取局部系下南端面的 E.X / W.X（与算法 0 号段日志 East/West Length 一致）。
    /// </summary>
    /// <summary>
    /// 落库算法产出中"实测修正后才有意义"的值——目前只有匹配浇筑扭转（rerr[9]，依赖实测高程）。
    /// </summary>
    /// <remarks>
    /// <b>Match*（立模参数）不在此写入</b>：它们是 St=0 理论放样的结果，由 PerfectCast 现算供页面只读区使用。
    /// 若用 St=1 的结果覆盖，保存一次后页面上的立模参数就变成了实测修正值，语义被污染，
    /// 且会让"立模参数 = 浇筑前给定的目标值"这一口径失效。
    /// </remarks>
    private void ApplyCalcResult(biz_concrete_beam_segment entity, CalcResult r)
    {
        entity.MatchCastTwist = ToDecimal4(r.MatchCastTwist);
    }

    /// <summary>
    /// 回填算法产出到返回前端的 DTO：与 ApplyCalcResult 口径一致，只回扭转值。
    /// 立模参数（只读区）由前端另行调用 PerfectCast 获取。
    /// </summary>
    private void ApplyCalcResultToDto(SetupNSurveyDTO dto, CalcResult r)
    {
        dto.SurveyMatchCastTwist = ToDecimal4(r.MatchCastTwist);
    }

    /// <summary>把算法输出的一行（9 列 [E,C,W]×XYZ）写入浇筑端面实体。</summary>
    private void ApplyJointValues(biz_concrete_beam_ascast_coordinate row, double[] v)
    {
        row.EastX = ToDecimal4(v[0]); row.EastY = ToDecimal4(v[1]); row.EastZ = ToDecimal4(v[2]);
        row.CenterX = ToDecimal4(v[3]); row.CenterY = ToDecimal4(v[4]); row.CenterZ = ToDecimal4(v[5]);
        row.WestX = ToDecimal4(v[6]); row.WestY = ToDecimal4(v[7]); row.WestZ = ToDecimal4(v[8]);
    }

    private void ApplyJointValues(biz_concrete_beam_ascast_measure row, double[] v)
    {
        row.EastX = ToDecimal4(v[0]); row.EastY = ToDecimal4(v[1]); row.EastZ = ToDecimal4(v[2]);
        row.CenterX = ToDecimal4(v[3]); row.CenterY = ToDecimal4(v[4]); row.CenterZ = ToDecimal4(v[5]);
        row.WestX = ToDecimal4(v[6]); row.WestY = ToDecimal4(v[7]); row.WestZ = ToDecimal4(v[8]);
    }

    /// <summary>
    /// 写入本段的一端测点：有则原地更新（不软删重插，避免顺序错乱），无则新增。
    /// side：1=南端（KCT[2m]），0=北端（KCT[2m+1]）。
    /// </summary>
    private void UpsertMeasure(CastingGroupContext context, int castingGroupID, int segmentID, int side,
        double[] v, List<biz_concrete_beam_ascast_measure> existing, string account, DateTime now)
    {
        var row = existing.FirstOrDefault(a => a.SegmentID == segmentID && a.Side == side);
        if (row == null)
        {
            row = new biz_concrete_beam_ascast_measure
            {
                ProjID = context.ProjID,
                GirderID = context.GirderID,
                CastingGroupID = castingGroupID,
                SegmentID = segmentID,
                Side = side,
                Status = 0,
                CreatedBy = account,
                CreatedTime = now
            };
            Db.GetRepository<biz_concrete_beam_ascast_measure>().Add(row);
        }
        else { row.UpdatedBy = account; row.UpdatedTime = now; }
        ApplyJointValues(row, v);
    }

    private static SetupNSurveyDTO ToSurveyDto(biz_concrete_beam_segment e) => new()
    {
        SegmentID = e.ID,
        CastingGroupID = e.CastingGroupID,
        GriderID = e.GirderID,
        SegmentName = e.SegmentName,
        SegmentIndex = e.SegmentIndex,
        Node1 = e.Node1,
        Node2 = e.Node2,
        PouringDate = e.PouringDate,
        MatchNorthEastElev = e.MatchNorthEastElev,
        MatchNorthWestElev = e.MatchNorthWestElev,
        MatchNorthCenterOffset = e.MatchNorthOffset,
        MatchSouthEastElev = e.MatchSouthEastElev,
        MatchSouthWestElev = e.MatchSouthWestElev,
        MatchSouthCenterOffset = e.MatchSouthOffset,
        MatchWestLength = e.MatchWestLength,
        MatchEastLength = e.MatchEastLength,
        MatchCenterLength = e.MatchCenterLength,
        BoltOffsetANE = e.BoltOffsetANE,
        BoltOffsetANW = e.BoltOffsetANW,
        BoltOffsetASE = e.BoltOffsetASE,
        BoltOffsetASW = e.BoltOffsetASW,
        BoltOffsetBNE = e.BoltOffsetBNE,
        BoltOffsetBNW = e.BoltOffsetBNW,
        BoltOffsetBSE = e.BoltOffsetBSE,
        BoltOffsetBSW = e.BoltOffsetBSW,
        BoltOffsetAUseNorth = e.BoltOffsetAUseNorth != 0,
        BoltOffsetAUseSouth = e.BoltOffsetAUseSouth != 0,
        BoltOffsetBUseMeasured = e.BoltOffsetBUseMeasured != 0,
        // 旧字段（兼容）
        BoltOffsetAN = e.BoltOffsetAN,
        BoltOffsetCN = e.BoltOffsetCN,
        BoltOffsetAS = e.BoltOffsetAS,
        BoltOffsetCS = e.BoltOffsetCS,
        SurveyMatchNorthEastElev = e.SurveyMatchNorthEastElev,
        SurveyMatchNorthWestElev = e.SurveyMatchNorthWestElev,
        SurveyMatchSouthEastElev = e.SurveyMatchSouthEastElev,
        SurveyMatchSouthWestElev = e.SurveyMatchSouthWestElev,
        SurveyMatchNorthCenterOffset = e.SurveyMatchNorthCenterOffset,
        SurveyMatchSouthCenterOffset = e.SurveyMatchSouthCenterOffset,
        SurveyBulkheadEastElev = e.SurveyBulkheadEastElev,
        SurveyBulkheadWestElev = e.SurveyBulkheadWestElev,
        SurveyBulkheadCenterOffset = e.SurveyBulkheadCenterOffset,
        SurveyWetNorthEastElev = e.SurveyWetNorthEastElev,
        SurveyWetNorthWestElev = e.SurveyWetNorthWestElev,
        SurveyWetSouthEastElev = e.SurveyWetSouthEastElev,
        SurveyWetSouthWestElev = e.SurveyWetSouthWestElev,
        SurveyWetNorthCenterOffset = e.SurveyWetNorthCenterOffset,
        SurveyWetSouthCenterOffset = e.SurveyWetSouthCenterOffset,
        SurveyWetCenterLength = e.SurveyWetCenterLength,
        SurveyWetEastLength = e.SurveyWetEastLength,
        SurveyWetWestLength = e.SurveyWetWestLength,
        SurveyMatchCastTwist = e.MatchCastTwist
    };

    private bool AllFinite(double[] v) => v != null && v.All(x => !double.IsNaN(x) && !double.IsInfinity(x));

    /// <summary>double → decimal（4 位小数），并拦截 NaN/Infinity 防止脏值落库。</summary>
    private decimal ToDecimal4(double v)
    {
        if (double.IsNaN(v) || double.IsInfinity(v))
            throw new InvalidOperationException(BizMessages.AlgorithmResultInvalid(Lang));
        if (Math.Abs(v) > 1e12) throw new InvalidOperationException($"线形计算结果超出合理范围：{v}");
        return Math.Round((decimal)v, 4, MidpointRounding.AwayFromZero);
    }

    #endregion

    #endregion
}
