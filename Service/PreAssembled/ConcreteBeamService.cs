using DataFactory.Factory;
using DataFactory.KingBase;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Model.tech.QL.DTO.BizProject;
using System.Security.Claims;
using Tool;

namespace Service.PreAssembled;

public class ConcreteBeamService : ServiceBase<biz_project_bridge_castingGroup>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ConcreteBeamService(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory,
        IHttpContextAccessor httpContextAccessor) : base(qlUowFactory, httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public async Task<ConcreteBeamPagedResultDTO> List(ConcreteBeamQueryDTO req)
    {
        var allowed = await GetAllowedProjectIds();
        var query = from cg in Db.Query<biz_project_bridge_castingGroup>().Where(a => a.Status != -1)
                    join bridge in Db.Query<biz_project_bridge>().Where(a => a.Status != -1 && a.BeamType == 1) on cg.BridgeID equals bridge.ID
                    join proj in Db.Query<biz_project>().Where(a => a.Status != -1) on bridge.ProjID equals proj.ID
                    select new ConcreteBeamItemDTO
                    {
                        ID = cg.ID, GirderID = cg.BridgeID, ProjID = bridge.ProjID,
                        ProjectName = proj.Name, BridgeName = bridge.Name, CastingGroupName = cg.Name,
                        ManagerName = proj.ManagerName ?? string.Empty
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
        var config = await Db.Query<biz_concrete_beam_config>().Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
            .OrderByDescending(a => a.ID).FirstOrDefaultAsync();
        return new()
        {
            CastingGroupID = castingGroupID, ProjectName = context.ProjectName, BridgeName = context.BridgeName,
            CastingGroupName = context.CastingGroupName, ManagerName = context.ManagerName,
            OffsetA = config?.OffsetA, OffsetB = config?.OffsetB, OffsetC = config?.OffsetC,
            ReferenceElevation = config?.OffsetH,
            IsConfigured = config != null && config.OffsetA > 0 && config.OffsetB > 0 && config.OffsetC > 0,
            ElevationIncludesCamber = config?.ElevationIncludesCamber ?? false,
            CreatedTime = config?.CreatedTime, UpdatedTime = config?.UpdatedTime
        };
    }

    public async Task<ConcreteBeamConfigDTO> SaveConfig(ConcreteBeamConfigSaveDTO req)
    {
        if (req.OffsetA <= 0) throw new InvalidOperationException("A必须大于0");
        if (req.OffsetB <= 0) throw new InvalidOperationException("B必须大于0");
        if (req.OffsetC <= 0) throw new InvalidOperationException("C必须大于0");
        if (!req.ReferenceElevation.HasValue) throw new InvalidOperationException("参考高程不能为空且必须是有效数字");
        ValidateFourDecimals(req.OffsetA, "A"); ValidateFourDecimals(req.OffsetB, "B");
        ValidateFourDecimals(req.OffsetC, "C"); ValidateFourDecimals(req.ReferenceElevation.Value, "浇筑参考高程");
        var context = await EnsureCastingGroupAccess(req.CastingGroupID);
        var entity = await Db.Query<biz_concrete_beam_config>().Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1)
            .OrderByDescending(a => a.ID).FirstOrDefaultAsync();
        var now = DateTime.Now; var account = GetCurrentAccount();
        if (entity == null)
        {
            entity = new() { ProjID = context.ProjID, GirderID = context.GirderID, CastingGroupID = req.CastingGroupID,
                Status = 0, CreatedBy = account, CreatedTime = now };
            Db.GetRepository<biz_concrete_beam_config>().Add(entity);
        }
        else { entity.UpdatedBy = account; entity.UpdatedTime = now; }
        entity.OffsetA = req.OffsetA; entity.OffsetB = req.OffsetB; entity.OffsetC = req.OffsetC;
        entity.OffsetH = req.ReferenceElevation.Value;
        await Db.SaveAsync();
        return await GetConfig(req.CastingGroupID);
    }

    public async Task<ConcreteBeamPagedResultDTO<ConcreteBeamSegmentDTO>> SegmentList(ConcreteBeamDataQueryDTO req)
    {
        await EnsureCastingGroupAccess(req.CastingGroupID);
        var query = Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1);
        var nodeCount = await query.CountAsync();
        var total = Math.Max(nodeCount - 1, 0);
        var page = Math.Max(req.PageIndex, 1); var size = NormalizePageSize(req.PageSize);
        var offset = (page - 1) * size;
        var nodeNames = offset >= total
            ? []
            : await query.OrderBy(a => a.ID).Skip(offset).Take(size + 1).Select(a => a.NodeName).ToListAsync();
        var list = nodeNames.Take(Math.Min(size, nodeNames.Count - 1)).Select((nodeName, index) => new ConcreteBeamSegmentDTO
        {
            SegmentName = $"{nodeName}-{nodeNames[index + 1]}",
            Node1 = (offset + index + 1).ToString(),
            Node2 = (offset + index + 2).ToString(),
            Downstream = null,
            Upstream = null,
            PouringDate = null
        }).ToList();
        return new() { List = list, Total = total };
    }

    public async Task<ConcreteBeamPagedResultDTO<ConcreteBeamDesignCoordinateDTO>> DesignCoordinateList(ConcreteBeamDataQueryDTO req)
    {
        await EnsureCastingGroupAccess(req.CastingGroupID);
        await EnsureProjectConfigSubmitted(req.CastingGroupID);
        var page = Math.Max(req.PageIndex, 1); var size = NormalizePageSize(req.PageSize);
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

    public async Task<ConcreteBeamPagedResultDTO<ConcreteBeamCamberDTO>> CamberList(ConcreteBeamDataQueryDTO req)
    {
        await EnsureCastingGroupAccess(req.CastingGroupID);
        var query = Db.Query<biz_concrete_beam_camber_calculation>().Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1);
        var total = await query.CountAsync(); var page = Math.Max(req.PageIndex, 1); var size = NormalizePageSize(req.PageSize);
        var list = await query.OrderBy(a => a.ID).Skip((page - 1) * size).Take(size)
            .Select(a => new ConcreteBeamCamberDTO { NodeName = a.NodeName, ZE = a.ZE, ZC = a.ZC, ZW = a.ZW }).ToListAsync();
        return new() { List = list, Total = total };
    }

    public byte[] DesignCoordinateTemplate() => ConcreteBeamExcelHelper.CreateDesignCoordinateTemplate();
    public byte[] CamberTemplate() => ConcreteBeamExcelHelper.CreateCamberTemplate();
    public void EnsureAuthenticated() => _ = GetCurrentAccount();

    public async Task<ConcreteBeamImportResultDTO> ImportDesignCoordinates(int castingGroupID, byte[] bytes,
        bool confirmOverwrite, bool elevationIncludesCamber)
    {
        var context = await EnsureCastingGroupAccess(castingGroupID);
        await EnsureProjectConfigSubmitted(castingGroupID);
        ConcreteBeamParseResult<ConcreteBeamDesignCoordinateImportRow> parsed;
        try { using var stream = new MemoryStream(bytes); parsed = ConcreteBeamExcelHelper.ParseDesignCoordinates(stream); }
        catch (Exception ex) { return Failure([new(0, "", "文件", "", $"Excel解析失败：{ex.Message}")]); }
        if (!parsed.Success) return Failure(parsed.Errors);
        var hasSource = await Db.Query<biz_concrete_beam_design_coordinate_source>()
            .AnyAsync(a => a.CastingGroupID == castingGroupID && a.Status != -1);
        var hasCalculation = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .AnyAsync(a => a.CastingGroupID == castingGroupID && a.Status != -1);
        if ((hasSource || hasCalculation) && !confirmOverwrite)
            return new() { RequiresOverwriteConfirmation = true, Message = "本次导入将整体替换当前浇筑组的设计坐标，请确认后继续" };
        var now = DateTime.Now; var account = GetCurrentAccount();
        try
        {
            Db.BeginTransaction();
            var oldSources = await Db.Query<biz_concrete_beam_design_coordinate_source>()
                .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1).ToListAsync();
            var oldCalculations = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
                .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1).ToListAsync();
            if ((oldSources.Count > 0 || oldCalculations.Count > 0) && !confirmOverwrite)
                throw new InvalidOperationException("设计坐标已变化，请确认覆盖后重试");
            foreach (var old in oldSources) { old.Status = -1; old.UpdatedBy = account; old.UpdatedTime = now; }
            foreach (var old in oldCalculations) { old.Status = -1; old.UpdatedBy = account; old.UpdatedTime = now; }
            await Db.SaveAsync();
            var sourceRows = parsed.Rows.Select(a => new biz_concrete_beam_design_coordinate_source
            { ProjID = context.ProjID, GirderID = context.GirderID, CastingGroupID = castingGroupID, NodeName = a.NodeName,
                XE = a.XE, YE = a.YE, ZE = a.ZE, XC = a.XC, YC = a.YC, ZC = a.ZC, XW = a.XW, YW = a.YW, ZW = a.ZW,
                Status = 0, CreatedBy = account, CreatedTime = now }).ToList();
            var calculationRows = parsed.Rows.Select(a => new biz_concrete_beam_design_coordinate_calculation
            { ProjID = context.ProjID, GirderID = context.GirderID, CastingGroupID = castingGroupID, NodeName = a.NodeName,
                XE = a.XE, YE = a.YE, ZE = a.ZE, XC = a.XC, YC = a.YC, ZC = a.ZC, XW = a.XW, YW = a.YW, ZW = a.ZW,
                Status = 0, CreatedBy = account, CreatedTime = now }).ToList();
            Db.GetRepository<biz_concrete_beam_design_coordinate_source>().AddList(sourceRows);
            Db.GetRepository<biz_concrete_beam_design_coordinate_calculation>().AddList(calculationRows);

            var config = await Db.Query<biz_concrete_beam_config>()
                .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1)
                .OrderByDescending(a => a.ID).FirstOrDefaultAsync();
            if (config == null)
            {
                config = new biz_concrete_beam_config
                {
                    ProjID = context.ProjID, GirderID = context.GirderID, CastingGroupID = castingGroupID,
                    ElevationIncludesCamber = elevationIncludesCamber,
                    Status = 0, CreatedBy = account, CreatedTime = now
                };
                Db.GetRepository<biz_concrete_beam_config>().Add(config);
            }
            else
            {
                config.ElevationIncludesCamber = elevationIncludesCamber;
                config.UpdatedBy = account;
                config.UpdatedTime = now;
            }
            await Db.SaveAsync(); Db.CommitTransaction();
            return new() { Success = true, ImportedCount = sourceRows.Count, Message = $"成功导入{sourceRows.Count}条设计坐标" };
        }
        catch (Exception ex) { Db.RollbackTransaction(); return Failure([new(0, "", "数据库", "", $"保存失败：{ex.Message}")]); }
    }

    public async Task<ConcreteBeamImportResultDTO> ImportCamber(int castingGroupID, byte[] bytes, bool confirmOverwrite)
    {
        var context = await EnsureCastingGroupAccess(castingGroupID);
        ConcreteBeamParseResult<ConcreteBeamCamberImportRow> parsed;
        try { using var stream = new MemoryStream(bytes); parsed = ConcreteBeamExcelHelper.ParseCamber(stream); }
        catch (Exception ex) { return Failure([new(0, "", "文件", "", $"Excel解析失败：{ex.Message}")]); }
        if (!parsed.Success) return Failure(parsed.Errors);
        var designNames = await Db.Query<biz_concrete_beam_design_coordinate_calculation>()
            .Where(a => a.CastingGroupID == castingGroupID && a.Status != -1).Select(a => a.NodeName).ToListAsync();
        var importedNames = parsed.Rows.Select(a => a.NodeName).ToList();
        var mismatch = importedNames.Count != designNames.Count || !importedNames.OrderBy(a => a, StringComparer.Ordinal)
            .SequenceEqual(designNames.OrderBy(a => a, StringComparer.Ordinal), StringComparer.Ordinal);
        if (mismatch) return Failure([new(0, "", "节点名称", "", "节点名称和数量必须与设计坐标完全一致")]);
        if (await Db.Query<biz_concrete_beam_camber_source>().AnyAsync(a => a.CastingGroupID == castingGroupID && a.Status != -1) && !confirmOverwrite)
            return new() { RequiresOverwriteConfirmation = true, Message = "本次导入将整体替换当前浇筑组的预拱度数据，请确认后继续" };
        var now = DateTime.Now; var account = GetCurrentAccount();
        try
        {
            Db.BeginTransaction();
            var sources = await Db.Query<biz_concrete_beam_camber_source>().Where(a => a.CastingGroupID == castingGroupID && a.Status != -1).ToListAsync();
            var calculations = await Db.Query<biz_concrete_beam_camber_calculation>().Where(a => a.CastingGroupID == castingGroupID && a.Status != -1).ToListAsync();
            if ((sources.Count > 0 || calculations.Count > 0) && !confirmOverwrite) throw new InvalidOperationException("预拱度数据已变化，请确认覆盖后重试");
            foreach (var row in sources) { row.Status = -1; row.UpdatedBy = account; row.UpdatedTime = now; }
            foreach (var row in calculations) { row.Status = -1; row.UpdatedBy = account; row.UpdatedTime = now; }
            await Db.SaveAsync();
            Db.GetRepository<biz_concrete_beam_camber_source>().AddList(parsed.Rows.Select(a => new biz_concrete_beam_camber_source
            { ProjID = context.ProjID, GirderID = context.GirderID, CastingGroupID = castingGroupID, NodeName = a.NodeName,
                ZE = a.ZE, ZC = a.ZC, ZW = a.ZW, Status = 0, CreatedBy = account, CreatedTime = now }).ToList());
            Db.GetRepository<biz_concrete_beam_camber_calculation>().AddList(parsed.Rows.Select(a => new biz_concrete_beam_camber_calculation
            { ProjID = context.ProjID, GirderID = context.GirderID, CastingGroupID = castingGroupID, NodeName = a.NodeName,
                ZE = a.ZE ?? a.ZC, ZC = a.ZC, ZW = a.ZW ?? a.ZC, Status = 0, CreatedBy = account, CreatedTime = now }).ToList());
            await Db.SaveAsync(); Db.CommitTransaction();
            return new() { Success = true, ImportedCount = parsed.Rows.Count, Message = $"成功导入{parsed.Rows.Count}条预拱度数据" };
        }
        catch (Exception ex) { Db.RollbackTransaction(); return Failure([new(0, "", "数据库", "", $"保存失败：{ex.Message}")]); }
    }

    private static ConcreteBeamImportResultDTO Failure(IEnumerable<ConcreteBeamImportError> errors)
    {
        var errorList = errors.ToList();
        return new()
        {
            Message = "导入失败，请查看错误明细文件",
            Errors = errorList.Select(a => new ConcreteBeamImportErrorDTO
            { ExcelRow = a.ExcelRow, RecordKey = a.RecordKey, FieldName = a.FieldName, OriginalValue = a.OriginalValue, Reason = a.Reason }).ToList(),
            ErrorFile = ConcreteBeamExcelHelper.CreateErrorFile(errorList),
            ErrorFileName = $"混凝土梁_导入错误明细_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
        };
    }

    private async Task<HashSet<int>?> GetAllowedProjectIds()
    {
        var account = GetCurrentAccount();
        var userID = await Db.Query<sys_userinfo>()
            .Where(a => a.Status == 1 && a.Account == account)
            .Select(a => (int?)a.ID)
            .FirstOrDefaultAsync()
            ?? throw new UnauthorizedAccessException("当前登录用户不存在或已停用");

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
        if (allowed != null && !allowed.Contains(projID)) throw new UnauthorizedAccessException("无该项目的数据权限");
    }

    private async Task<CastingGroupContext> EnsureCastingGroupAccess(int castingGroupID)
    {
        var context = await (from cg in Db.Query<biz_project_bridge_castingGroup>().Where(a => a.Status != -1 && a.ID == castingGroupID)
                             join bridge in Db.Query<biz_project_bridge>().Where(a => a.Status != -1 && a.BeamType == 1) on cg.BridgeID equals bridge.ID
                             join project in Db.Query<biz_project>().Where(a => a.Status != -1) on bridge.ProjID equals project.ID
                             select new CastingGroupContext(project.ID, bridge.ID, project.Name, bridge.Name, cg.Name, project.ManagerName ?? string.Empty))
                             .FirstOrDefaultAsync() ?? throw new InvalidOperationException("混凝土梁浇筑组不存在或已失效");
        await EnsureProjectAccess(context.ProjID); return context;
    }

    private string GetCurrentAccount()
    {
        var account = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(account)) throw new UnauthorizedAccessException("未获取到当前登录用户");
        return account;
    }

    private static void ValidateFourDecimals(decimal value, string field)
    { if (!ConcreteBeamExcelHelper.HasScale(value, 4)) throw new InvalidOperationException($"{field}最多保留4位小数"); }
    private async Task EnsureProjectConfigSubmitted(int castingGroupID)
    {
        var configured = await Db.Query<biz_concrete_beam_config>().AnyAsync(a =>
            a.CastingGroupID == castingGroupID && a.Status != -1 &&
            a.OffsetA > 0 && a.OffsetB > 0 && a.OffsetC > 0);
        if (!configured)
            throw new InvalidOperationException("请先在项目信息页签提交大于0的A、B、C及有效参考高程");
    }
    private static int NormalizePageSize(int size) => size is 10 or 20 or 50 or 100 ? size : 10;
    private sealed record CastingGroupContext(int ProjID, int GirderID, string ProjectName, string BridgeName, string CastingGroupName, string ManagerName);
}
