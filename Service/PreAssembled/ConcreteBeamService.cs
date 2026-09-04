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
            ReferenceElevation = config?.OffsetH, CreatedTime = config?.CreatedTime, UpdatedTime = config?.UpdatedTime
        };
    }

    public async Task<ConcreteBeamConfigDTO> SaveConfig(ConcreteBeamConfigSaveDTO req)
    {
        ValidateFourDecimals(req.OffsetA, "A"); ValidateFourDecimals(req.OffsetB, "B");
        ValidateFourDecimals(req.OffsetC, "C"); ValidateFourDecimals(req.ReferenceElevation, "浇筑参考高程");
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
        entity.OffsetH = req.ReferenceElevation;
        await Db.SaveAsync();
        return await GetConfig(req.CastingGroupID);
    }

    public async Task<ConcreteBeamPagedResultDTO<ConcreteBeamSegmentDTO>> SegmentList(ConcreteBeamDataQueryDTO req)
    {
        await EnsureCastingGroupAccess(req.CastingGroupID);
        var query = Db.Query<biz_concrete_beam_segment>().Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1);
        var total = await query.CountAsync(); var page = Math.Max(req.PageIndex, 1); var size = NormalizePageSize(req.PageSize);
        var list = await query.OrderBy(a => a.ID).Skip((page - 1) * size).Take(size).Select(a => new ConcreteBeamSegmentDTO
        { SegmentName = a.SegmentName, Downstream = a.Downstream, Upstream = a.Upstream, Node1 = a.Node1, Node2 = a.Node2, PouringDate = a.PouringDate }).ToListAsync();
        return new() { List = list, Total = total };
    }

    public async Task<ConcreteBeamPagedResultDTO<ConcreteBeamDesignCoordinateDTO>> DesignCoordinateList(ConcreteBeamDataQueryDTO req)
    {
        await EnsureCastingGroupAccess(req.CastingGroupID);
        var query = Db.Query<biz_concrete_beam_design_coordinate>().Where(a => a.CastingGroupID == req.CastingGroupID && a.Status != -1);
        var total = await query.CountAsync(); var page = Math.Max(req.PageIndex, 1); var size = NormalizePageSize(req.PageSize);
        var list = await query.OrderBy(a => a.ID).Skip((page - 1) * size).Take(size).Select(a => new ConcreteBeamDesignCoordinateDTO
        { NodeName = a.NodeName, XE = a.XE, YE = a.YE, ZE = a.ZE, XC = a.XC, YC = a.YC, ZC = a.ZC, XW = a.XW, YW = a.YW, ZW = a.ZW }).ToListAsync();
        return new() { List = list, Total = total };
    }

    public byte[] SegmentTemplate() => ConcreteBeamExcelHelper.CreateSegmentTemplate();
    public byte[] DesignCoordinateTemplate() => ConcreteBeamExcelHelper.CreateDesignCoordinateTemplate();
    public void EnsureAuthenticated() => _ = GetCurrentAccount();

    public async Task<ConcreteBeamImportResultDTO> ImportSegments(int castingGroupID, byte[] bytes, bool confirmOverwrite)
    {
        var context = await EnsureCastingGroupAccess(castingGroupID);
        ConcreteBeamParseResult<ConcreteBeamSegmentImportRow> parsed;
        try { using var stream = new MemoryStream(bytes); parsed = ConcreteBeamExcelHelper.ParseSegments(stream); }
        catch (Exception ex) { return Failure([new(0, "", "文件", "", $"Excel解析失败：{ex.Message}")]); }
        if (!parsed.Success) return Failure(parsed.Errors);
        if (await Db.Query<biz_concrete_beam_segment>().AnyAsync(a => a.CastingGroupID == castingGroupID && a.Status != -1) && !confirmOverwrite)
            return new() { RequiresOverwriteConfirmation = true, Message = "本次导入将整体替换当前浇筑组的梁段信息，请确认后继续" };
        var now = DateTime.Now; var account = GetCurrentAccount();
        try
        {
            Db.BeginTransaction();
            var oldRows = await Db.Query<biz_concrete_beam_segment>().Where(a => a.CastingGroupID == castingGroupID && a.Status != -1).ToListAsync();
            if (oldRows.Count > 0 && !confirmOverwrite) throw new InvalidOperationException("梁段信息已变化，请确认覆盖后重试");
            foreach (var old in oldRows) { old.Status = -1; old.UpdatedBy = account; old.UpdatedTime = now; }
            await Db.SaveAsync();
            var rows = parsed.Rows.Select(a => new biz_concrete_beam_segment
            { ProjID = context.ProjID, GirderID = context.GirderID, CastingGroupID = castingGroupID, SegmentName = a.SegmentName,
                Downstream = a.Downstream, Upstream = a.Upstream, Node1 = a.Node1, Node2 = a.Node2, PouringDate = a.PouringDate,
                Status = 0, CreatedBy = account, CreatedTime = now }).ToList();
            Db.GetRepository<biz_concrete_beam_segment>().AddList(rows); await Db.SaveAsync(); Db.CommitTransaction();
            return new() { Success = true, ImportedCount = rows.Count, Message = $"成功导入{rows.Count}条梁段信息" };
        }
        catch (Exception ex) { Db.RollbackTransaction(); return Failure([new(0, "", "数据库", "", $"保存失败：{ex.Message}")]); }
    }

    public async Task<ConcreteBeamImportResultDTO> ImportDesignCoordinates(int castingGroupID, byte[] bytes, bool confirmOverwrite)
    {
        var context = await EnsureCastingGroupAccess(castingGroupID);
        ConcreteBeamParseResult<ConcreteBeamDesignCoordinateImportRow> parsed;
        try { using var stream = new MemoryStream(bytes); parsed = ConcreteBeamExcelHelper.ParseDesignCoordinates(stream); }
        catch (Exception ex) { return Failure([new(0, "", "文件", "", $"Excel解析失败：{ex.Message}")]); }
        if (!parsed.Success) return Failure(parsed.Errors);
        if (await Db.Query<biz_concrete_beam_design_coordinate>().AnyAsync(a => a.CastingGroupID == castingGroupID && a.Status != -1) && !confirmOverwrite)
            return new() { RequiresOverwriteConfirmation = true, Message = "本次导入将整体替换当前浇筑组的设计坐标，请确认后继续" };
        var now = DateTime.Now; var account = GetCurrentAccount();
        try
        {
            Db.BeginTransaction();
            var oldRows = await Db.Query<biz_concrete_beam_design_coordinate>().Where(a => a.CastingGroupID == castingGroupID && a.Status != -1).ToListAsync();
            if (oldRows.Count > 0 && !confirmOverwrite) throw new InvalidOperationException("设计坐标已变化，请确认覆盖后重试");
            foreach (var old in oldRows) { old.Status = -1; old.UpdatedBy = account; old.UpdatedTime = now; }
            await Db.SaveAsync();
            var rows = parsed.Rows.Select(a => new biz_concrete_beam_design_coordinate
            { ProjID = context.ProjID, GirderID = context.GirderID, CastingGroupID = castingGroupID, NodeName = a.NodeName,
                XE = a.XE, YE = a.YE, ZE = a.ZE, XC = a.XC, YC = a.YC, ZC = a.ZC, XW = a.XW, YW = a.YW, ZW = a.ZW,
                Status = 0, CreatedBy = account, CreatedTime = now }).ToList();
            Db.GetRepository<biz_concrete_beam_design_coordinate>().AddList(rows); await Db.SaveAsync(); Db.CommitTransaction();
            return new() { Success = true, ImportedCount = rows.Count, Message = $"成功导入{rows.Count}条设计坐标" };
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

    private Task<HashSet<int>?> GetAllowedProjectIds()
    {
        // 与钢梁管理保持一致：菜单负责入口授权，当前库尚无项目-用户关系表。
        GetCurrentAccount();
        return Task.FromResult<HashSet<int>?>(null);
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
    private static int NormalizePageSize(int size) => size is 10 or 20 or 50 or 100 ? size : 10;
    private sealed record CastingGroupContext(int ProjID, int GirderID, string ProjectName, string BridgeName, string CastingGroupName, string ManagerName);
}
