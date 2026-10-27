using ClosedXML.Excel;
using DataFactory.Factory;
using DataFactory.KingBase;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Configuration;
using Model.tech.QL.DTO.BizProject;
using Model.tech.QL.DTO.BizSteelBeam;
using NPOI.POIFS.Crypt.Agile;
using Service.PreAssembled.BridgeSegmentAlignment;
using Service.PreAssembled.Reports;
using System.Security.Claims;
using Tool;

namespace Service.PreAssembled;

public class SteelBeamService : ServiceBase<biz_project_bridge>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    /// <summary>当前请求语言。来源：前端 axios 拦截器写入的 i18n 请求头，见 <see cref="RequestLanguage"/>。</summary>
    private string Lang => RequestLanguage.Resolve(_httpContextAccessor);

    public SteelBeamService(
        QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration) : base(qlUowFactory, httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public async Task<SteelBeamPagedResultDTO<SteelBeamItemDTO>> List(SteelBeamQueryDTO req)
    {
        var allowed = await GetAllowedProjectIds();
        var query = from bridge in Db.Query<biz_project_bridge>().Where(a => a.Status != -1 && a.BeamType == 0)
                    join proj in Db.Query<biz_project>().Where(a => a.Status != -1) on bridge.ProjID equals proj.ID
                    select new SteelBeamItemDTO
                    {
                        ID = bridge.ID,
                        ProjID = bridge.ProjID,
                        ProjectName = proj.Name,
                        BridgeName = bridge.Name,
                        BeamType = bridge.BeamType
                    };
        if (allowed != null) query = query.Where(a => allowed.Contains(a.ProjID));
        if (req.ProjID is > 0) query = query.Where(a => a.ProjID == req.ProjID);
        if (req.GirderID is > 0) query = query.Where(a => a.ID == req.GirderID);
        var total = await query.CountAsync();
        var page = Math.Max(req.PageIndex, 1);
        var size = NormalizePageSize(req.PageSize);
        var list = await query.OrderBy(a => a.ID)
            .Skip((page - 1) * size).Take(size).ToListAsync();
        return new() { List = list, Total = total };
    }

    public async Task<List<object>> ProjectOptions()
    {
        var allowed = await GetAllowedProjectIds();
        var query = from bridge in Db.Query<biz_project_bridge>().Where(a => a.Status != -1 && a.BeamType == 0)
                    join proj in Db.Query<biz_project>().Where(a => a.Status != -1) on bridge.ProjID equals proj.ID
                    select new { proj.ID, proj.Name };
        if (allowed != null) query = query.Where(a => allowed.Contains(a.ID));
        var rows = await query.Distinct().OrderBy(a => a.Name).ToListAsync();
        return rows.Select(a => (object)new { id = a.ID, name = a.Name }).ToList();
    }

    public async Task<List<object>> BridgeOptions(int projID)
    {
        await EnsureProjectAccess(projID);
        var rows = await Db.Query<biz_project_bridge>()
            .Where(a => a.Status != -1 && a.BeamType == 0 && a.ProjID == projID)
            .OrderBy(a => a.Name).Select(a => new { id = a.ID, name = a.Name }).ToListAsync();
        return rows.Select(a => (object)a).ToList();
    }

    public async Task<object> GetGirderInfo(int girderID)
    {
        var context = await EnsureGirderAccess(girderID);
        return new { projectName = context.ProjectName, bridgeName = context.BridgeName, projID = context.ProjID, beamType = 0 };
    }

    public async Task<SteelBeamImportStateDTO> GetImportState(int girderID)
    {
        await EnsureGirderAccess(girderID);
        var hasTheory = await Db.Query<biz_steel_beam_theoretical>().AnyAsync(a => a.GirderID == girderID && a.Status != -1);
        var activeBatches = Db.Query<biz_steel_beam_measure_batch>().Where(a => a.GirderID == girderID && a.Status != -1);
        var hasMeasured = await activeBatches.AnyAsync();
        var max = hasMeasured ? await activeBatches.MaxAsync(a => a.PushCount) : (int?)null;
        return new()
        {
            HasTheoretical = hasTheory,
            HasMeasured = hasMeasured,
            MaxPushCount = max,
            ImportPushCounts = max.HasValue ? [max.Value, max.Value + 1] : [0],
            QueryPushCounts = max.HasValue ? Enumerable.Range(0, max.Value + 1).ToList() : []
        };
    }

    public async Task<SteelBeamPagedResultDTO<SteelBeamTheoreticalItemDTO>> TheoreticalList(SteelBeamTheoreticalQueryDTO req)
    {
        await EnsureGirderAccess(req.GirderID);
        var locked = await Db.Query<biz_steel_beam_measure_batch>().AnyAsync(a => a.GirderID == req.GirderID && a.Status != -1);
        var query = Db.Query<biz_steel_beam_theoretical>().Where(a => a.GirderID == req.GirderID && a.Status != -1);
        if (!string.IsNullOrWhiteSpace(req.PointCode)) query = query.Where(a => a.PointCode == req.PointCode);
        if (!string.IsNullOrWhiteSpace(req.SegmentNo)) query = query.Where(a => a.SegmentNo == req.SegmentNo);
        var total = await query.CountAsync();
        var page = Math.Max(req.PageIndex, 1);
        var size = NormalizePageSize(req.PageSize);
        // [Shen, Lin] 因为pointcode并未规定格式，所以排序不可靠，先用PositionOrder（即excel中行的顺序）
        var rows = await query.OrderBy(a => a.ID) // .OrderBy(a => a.PointCode.ToLower())
            .Skip((page - 1) * size).Take(size)
            .Select(a => new SteelBeamTheoreticalItemDTO
            {
                ID = a.ID,
                PointCode = a.PointCode,
                DesignX = a.DesignX,
                DesignY = a.DesignY,
                DesignZ = a.DesignZ,
                PreCamber = a.PreCamber,
                Weight = a.Weight,
                SegmentNo = a.SegmentNo,
                PositionName = a.PositionName,
                Version = a.Version,
                CanEdit = !locked
            }).ToListAsync();
        return new() { List = rows, Total = total };
    }

    public async Task<object> TheoreticalOptions(int girderID)
    {
        await EnsureGirderAccess(girderID);
        var query = Db.Query<biz_steel_beam_theoretical>().Where(a => a.GirderID == girderID && a.Status != -1);
        var points = await query.Select(a => a.PointCode).Distinct().OrderBy(a => a.ToLower()).ToListAsync();
        var segments = await query.Select(a => a.SegmentNo).Distinct().OrderBy(a => a).ToListAsync();
        return new { points, segments };
    }

    public async Task<SteelBeamPagedResultDTO<SteelBeamMeasuredItemDTO>> MeasuredList(SteelBeamMeasuredQueryDTO req)
    {
        await EnsureGirderAccess(req.GirderID);
        var batches = Db.Query<biz_steel_beam_measure_batch>().Where(a => a.GirderID == req.GirderID && a.Status != -1);
        // 可编辑/删除的仅「最大顶推次数 + 最新测量时间」的最新批次（与 LoadLatestCompleteSetAsync 同口径）
        var latestBatchId = await batches
            .OrderByDescending(a => a.PushCount).ThenByDescending(a => a.MeasureTime).ThenByDescending(a => a.ID)
            .Select(a => (int?)a.ID).FirstOrDefaultAsync();
        var query = from measured in Db.Query<biz_steel_beam_measured>().Where(a => a.GirderID == req.GirderID && a.Status != -1)
                    join batch in batches on measured.BatchID equals batch.ID
                    select new { measured, batch };
        if (req.PushCount.HasValue) query = query.Where(a => a.batch.PushCount == req.PushCount.Value);
        if (req.MeasureTime.HasValue) query = query.Where(a => a.batch.MeasureTime == req.MeasureTime.Value);

        var pointQuery = query.Select(a => a.measured.PointCode).Distinct();
        var total = await pointQuery.CountAsync();
        var page = Math.Max(req.PageIndex, 1);
        var size = NormalizePageSize(req.PageSize);
        var pointCodes = await pointQuery.OrderBy(a => a.ToLower()).Skip((page - 1) * size).Take(size).ToListAsync();
        if (pointCodes.Count == 0) return new() { Total = total };

        var data = await query.Where(a => pointCodes.Contains(a.measured.PointCode))
            .Select(a => new SteelBeamMeasuredItemDTO
            {
                ID = a.measured.ID,
                PointCode = a.measured.PointCode,
                MeasuredX = a.measured.MeasuredX,
                MeasuredY = a.measured.MeasuredY,
                MeasuredZ = a.measured.MeasuredZ,
                PushCount = a.batch.PushCount,
                MeasureTime = a.batch.MeasureTime,
                ImportTime = a.measured.ImportTime,
                Version = a.measured.Version,
                CanEdit = a.batch.ID == latestBatchId
            }).ToListAsync();
        var ordered = data.OrderBy(a => a.ID).ToList();
        foreach (var group in ordered.GroupBy(a => a.PointCode, StringComparer.OrdinalIgnoreCase))
            group.First().PointRowSpan = group.Count();
        return new() { List = ordered, Total = total };
    }

    public async Task<List<DateTime>> MeasureTimeOptions(int girderID, int pushCount)
    {
        await EnsureGirderAccess(girderID);
        return await Db.Query<biz_steel_beam_measure_batch>()
            .Where(a => a.GirderID == girderID && a.Status != -1 && a.PushCount == pushCount)
            .Select(a => a.MeasureTime).Distinct().OrderByDescending(a => a).ToListAsync();
    }

    public async Task<SteelBeamTheoreticalItemDTO> UpdateTheoretical(SteelBeamCoordinateUpdateDTO req)
    {
        await EnsureGirderAccess(req.GirderID);
        // [Shen, Lin] 不再验证，改为四舍五入
        // ValidateCoordinate(req.X, req.Y, req.Z);
        req.X = Math.Round(req.X, 4, MidpointRounding.AwayFromZero);
        req.Y = Math.Round(req.Y, 4, MidpointRounding.AwayFromZero);
        req.Z = Math.Round(req.Z, 4, MidpointRounding.AwayFromZero);
        if (await Db.Query<biz_steel_beam_measure_batch>().AnyAsync(a => a.GirderID == req.GirderID && a.Status != -1))
            throw new InvalidOperationException(BizMessages.SteelTheoryLockedForEdit(Lang));
        var row = await Db.Query<biz_steel_beam_theoretical>()
            .FirstOrDefaultAsync(a => a.ID == req.ID && a.GirderID == req.GirderID && a.Status != -1 && a.Version == req.Version)
            ?? throw new InvalidOperationException(BizMessages.DataUpdatedRefresh(Lang));
        row.DesignX = req.X; row.DesignY = req.Y; row.DesignZ = req.Z;
        row.Version++; row.UpdatedBy = GetCurrentAccount(); row.UpdatedTime = DateTime.Now;
        await Db.SaveAsync();
        return new()
        {
            ID = row.ID,
            PointCode = row.PointCode,
            DesignX = row.DesignX,
            DesignY = row.DesignY,
            DesignZ = row.DesignZ,
            PreCamber = row.PreCamber,
            Weight = row.Weight,
            SegmentNo = row.SegmentNo,
            PositionName = row.PositionName,
            Version = row.Version,
            CanEdit = true
        };
    }

    public async Task<SteelBeamMeasuredItemDTO> UpdateMeasured(SteelBeamCoordinateUpdateDTO req)
    {
        var context = await EnsureGirderAccess(req.GirderID);
        // [Shen, Lin] 不再验证，改为四舍五入
        // ValidateCoordinate(req.X, req.Y, req.Z);
        req.X = Math.Round(req.X, 4, MidpointRounding.AwayFromZero);
        req.Y = Math.Round(req.Y, 4, MidpointRounding.AwayFromZero);
        req.Z = Math.Round(req.Z, 4, MidpointRounding.AwayFromZero);
        var latestBatchId = await Db.Query<biz_steel_beam_measure_batch>()
            .Where(a => a.GirderID == req.GirderID && a.Status != -1)
            .OrderByDescending(a => a.PushCount).ThenByDescending(a => a.MeasureTime).ThenByDescending(a => a.ID)
            .Select(a => (int?)a.ID).FirstOrDefaultAsync();
        var row = await (from measured in Db.Query<biz_steel_beam_measured>()
                         join batch in Db.Query<biz_steel_beam_measure_batch>().Where(a => a.Status != -1) on measured.BatchID equals batch.ID
                         where measured.ID == req.ID && measured.GirderID == req.GirderID && measured.Status != -1 && measured.Version == req.Version
                         select new { measured, batch }).FirstOrDefaultAsync()
                         ?? throw new InvalidOperationException(BizMessages.DataUpdatedRefresh(Lang));
        if (row.batch.ID != latestBatchId) throw new InvalidOperationException(BizMessages.SteelPushCountNotEditable(Lang));
        var account = GetCurrentAccount();
        var timestamp = DateTime.Now;
        Db.BeginTransaction();
        try
        {
            row.measured.MeasuredX = req.X; row.measured.MeasuredY = req.Y; row.measured.MeasuredZ = req.Z;
            row.measured.Version++; row.measured.UpdatedBy = account; row.measured.UpdatedTime = timestamp;
            await Db.SaveAsync();
            // 可编辑的只能是最大顶推次数批次（即最后一批），改完立即 replay 重建滚动快照；
            // 重算失败整体回滚
            await RebuildDerivedCoreAsync(context.ProjID, req.GirderID, account, timestamp);
            Db.CommitTransaction();
        }
        catch
        {
            Db.RollbackTransaction();
            throw;
        }
        return new()
        {
            ID = row.measured.ID,
            PointCode = row.measured.PointCode,
            MeasuredX = row.measured.MeasuredX,
            MeasuredY = row.measured.MeasuredY,
            MeasuredZ = row.measured.MeasuredZ,
            PushCount = row.batch.PushCount,
            MeasureTime = row.batch.MeasureTime,
            ImportTime = row.measured.ImportTime,
            Version = row.measured.Version,
            CanEdit = true
        };
    }

    public async Task<SteelBeamMeasuredItemDTO> DeleteMeasured(SteelBeamCoordinateUpdateDTO req)
    {
        var context = await EnsureGirderAccess(req.GirderID);
        var latestBatchId = await Db.Query<biz_steel_beam_measure_batch>()
            .Where(a => a.GirderID == req.GirderID && a.Status != -1)
            .OrderByDescending(a => a.PushCount).ThenByDescending(a => a.MeasureTime).ThenByDescending(a => a.ID)
            .Select(a => (int?)a.ID).FirstOrDefaultAsync();
        var row = await (from measured in Db.Query<biz_steel_beam_measured>()
                         join batch in Db.Query<biz_steel_beam_measure_batch>().Where(a => a.Status != -1) on measured.BatchID equals batch.ID
                         where measured.ID == req.ID && measured.GirderID == req.GirderID && measured.Status != -1 && measured.Version == req.Version
                         select new { measured, batch }).FirstOrDefaultAsync()
                         ?? throw new InvalidOperationException(BizMessages.DataUpdatedRefresh(Lang));
        if (row.batch.ID != latestBatchId) throw new InvalidOperationException(BizMessages.SteelPushCountNotEditable(Lang));
        var account = GetCurrentAccount();
        var timestamp = DateTime.Now;
        Db.BeginTransaction();
        try
        {
            row.measured.Status = -1; row.measured.Version++; row.measured.UpdatedBy = account; row.measured.UpdatedTime = timestamp;
            await Db.SaveAsync();
            // 删除后与编辑保存一致：同一事务内 replay 重建滚动快照，失败整体回滚
            await RebuildDerivedCoreAsync(context.ProjID, req.GirderID, account, timestamp);
            Db.CommitTransaction();
        }
        catch
        {
            Db.RollbackTransaction();
            throw;
        }
        return new()
        {
            ID = row.measured.ID,
            PointCode = row.measured.PointCode,
            MeasuredX = row.measured.MeasuredX,
            MeasuredY = row.measured.MeasuredY,
            MeasuredZ = row.measured.MeasuredZ,
            PushCount = row.batch.PushCount,
            MeasureTime = row.batch.MeasureTime,
            ImportTime = row.measured.ImportTime,
            Version = row.measured.Version,
            CanEdit = false
        };
    }

    public async Task<SteelBeamImportOutcome> ImportTheoretical(int girderID, byte[] bytes, bool confirmOverwrite)
    {
        var context = await EnsureGirderAccess(girderID);
        var timestamp = DateTime.Now;
        SteelBeamParseResult<SteelBeamTheoreticalImportRow> parsed;
        try { using var input = new MemoryStream(bytes); parsed = SteelBeamExcelHelper.ParseTheoretical(input, Lang); }
        catch (Exception ex) { return SaveFailure(context, "理论数据", null, null, timestamp, bytes, [new(0, "", BizMessages.FieldFile(Lang), "", BizMessages.ExcelParseFailed(Lang, ex.Message))]); }

        if (await Db.Query<biz_steel_beam_measure_batch>().AnyAsync(a => a.GirderID == girderID && a.Status != -1))
            parsed.Errors.Add(new(0, "", BizMessages.FieldBusinessRule(Lang), "", BizMessages.SteelTheoryLockedForImport(Lang)));
        var hasExisting = await Db.Query<biz_steel_beam_theoretical>().AnyAsync(a => a.GirderID == girderID && a.Status != -1);
        if (hasExisting && !confirmOverwrite) parsed.Errors.Add(new(0, "", BizMessages.FieldBusinessRule(Lang), "", BizMessages.SteelTheoryOverwriteConfirm(Lang)));
        if (parsed.Errors.Count == 0)
        {
            var duplicateSet = (await Db.Query<biz_steel_beam_theoretical>()
                .Where(a => a.ProjID == context.ProjID && a.GirderID != girderID && a.Status != -1)
                .Select(a => a.PointCode).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var row in parsed.SourcePoints.Where(a => duplicateSet.Contains(a.PointCode)))
                parsed.Errors.Add(new(row.ExcelRow, row.PointCode, BizMessages.FieldPointCode(Lang), row.PointCode, BizMessages.SteelPointCodeUsedElsewhere(Lang)));
        }
        if (!parsed.Success) return SaveFailure(context, "理论数据", null, null, timestamp, bytes, parsed.Errors);
        // TODO [Shen, Lin] 使用实际数据发现，毕竟不是依次固定方向的，需要询问下是否需要做点位方向验证，以及如何验证
        // SteelGirderAlignment.ValidateSequenceDirection(parsed);
        if (!parsed.Success) return SaveFailure(context, "理论数据", null, null, timestamp, bytes, parsed.Errors);
        var successPath = SaveOriginal(context, "理论数据", null, null, timestamp, bytes, false);

        try
        {
            Db.BeginTransaction();
            if (await Db.Query<biz_steel_beam_measure_batch>().AnyAsync(a => a.GirderID == girderID && a.Status != -1))
                throw new InvalidOperationException(BizMessages.SteelTheoryLockedForImport(Lang));
            var oldRows = await Db.Query<biz_steel_beam_theoretical>().Where(a => a.GirderID == girderID && a.Status != -1).ToListAsync();
            if (oldRows.Count > 0 && !confirmOverwrite) throw new InvalidOperationException(BizMessages.SteelTheoryChangedConfirm(Lang));
            foreach (var old in oldRows) { old.Status = -1; old.UpdatedBy = GetCurrentAccount(); old.UpdatedTime = timestamp; }
            await Db.SaveAsync();
            var account = GetCurrentAccount();
            // [Shen, Lin] 增加索引值，用来做排序，从0开始
            int idx = 0;
            var entities = parsed.Rows.Select(a => new biz_steel_beam_theoretical
            {
                ProjID = context.ProjID,
                GirderID = girderID,
                PointCode = a.PointCode,
                DesignX = a.DesignX,
                DesignY = a.DesignY,
                DesignZ = a.DesignZ,
                PreCamber = a.PreCamber,
                Weight = a.Weight,
                SegmentNo = a.SegmentNo,
                PositionName = a.PositionName,
                IsFirstCoordinate = null,
                PositionOrder = idx++,
                DistanceFromStart = null,
                Version = 1,
                Status = 0,
                CreatedBy = account,
                CreatedTime = timestamp
            }).ToList();
            // DistanceFromStart及PositionOrder
            SteelGirderAlignment.CalcPointPositionOrderAndDistanceFromStart(entities);
            Db.GetRepository<biz_steel_beam_theoretical>().AddList(entities);
            await Db.SaveAsync();
            Db.CommitTransaction();
            return new() { Success = true, ImportedCount = entities.Count, Message = BizMessages.SteelTheoryImportSuccess(Lang, entities.Count) };
        }
        catch (Exception ex)
        {
            Db.RollbackTransaction();
            TryDelete(successPath);
            return SaveFailure(context, "理论数据", null, null, timestamp, bytes, [new(0, "", BizMessages.FieldDatabase(Lang), "", BizMessages.SaveFailed(Lang, ex.Message))]);
        }
    }

    public async Task<SteelBeamImportOutcome> ImportMeasured(int girderID, int pushCount, DateTime measureTime, byte[] bytes, bool confirmOverwrite)
    {
        var context = await EnsureGirderAccess(girderID);
        var timestamp = DateTime.Now;
        measureTime = new DateTime(measureTime.Year, measureTime.Month, measureTime.Day, measureTime.Hour, 0, 0, DateTimeKind.Unspecified);
        SteelBeamParseResult<SteelBeamMeasuredImportRow> parsed;
        try { using var input = new MemoryStream(bytes); parsed = SteelBeamExcelHelper.ParseMeasured(input, Lang); }
        catch (Exception ex) { return SaveFailure(context, "实测数据", pushCount, measureTime, timestamp, bytes, [new(0, "", BizMessages.FieldFile(Lang), "", BizMessages.ExcelParseFailed(Lang, ex.Message))]); }

        var theory = await Db.Query<biz_steel_beam_theoretical>()
            .Where(a => a.GirderID == girderID && a.Status != -1).ToListAsync();
        var theoryMap = theory.ToDictionary(a => a.PointCode, StringComparer.OrdinalIgnoreCase);
        if (theory.Count == 0) parsed.Errors.Add(new(0, "", BizMessages.FieldBusinessRule(Lang), "", BizMessages.SteelTheoryRequired(Lang)));
        foreach (var row in parsed.Rows.Where(a => !theoryMap.ContainsKey(a.PointCode)))
            parsed.Errors.Add(new(row.ExcelRow, row.PointCode, BizMessages.FieldPointCode(Lang), row.PointCode, BizMessages.SteelPointCodeNotInTheory(Lang)));
        parsed.Errors.AddRange(SteelBeamExcelHelper.ValidateMeasuredSegmentMinPoints(
            theory.Select(a => new SteelBeamTheoreticalPoint(a.PointCode, a.SegmentNo)),
            parsed.Rows.Select(a => a.PointCode), Lang));
        var activeBatches = Db.Query<biz_steel_beam_measure_batch>().Where(a => a.GirderID == girderID && a.Status != -1);
        var hasBatch = await activeBatches.AnyAsync();
        var maxPush = hasBatch ? await activeBatches.MaxAsync(a => a.PushCount) : (int?)null;
        var allowedPushes = maxPush.HasValue ? new[] { maxPush.Value, maxPush.Value + 1 } : new[] { 0 };
        if (!allowedPushes.Contains(pushCount)) parsed.Errors.Add(new(0, "", "顶推次数", pushCount.ToString(), BizMessages.SteelPushCountOutOfRange(Lang)));
        var currentHour = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0);
        if (measureTime > currentHour) parsed.Errors.Add(new(0, "", "测量时间", measureTime.ToString("yyyy-MM-dd HH:00"), BizMessages.SteelMeasureTimeInFuture(Lang)));
        var oldBatch = await activeBatches.FirstOrDefaultAsync(a => a.PushCount == pushCount && a.MeasureTime == measureTime);
        if (oldBatch != null && !confirmOverwrite) parsed.Errors.Add(new(0, "", BizMessages.FieldBusinessRule(Lang), "", BizMessages.SteelMeasuredOverwriteConfirm(Lang)));
        // 同一 PushCount 允许多个 batch，但以 MeasureTime 最新为准；更早的批次不能覆盖已有的最新批次
        var samePushLatestTime = await activeBatches
            .Where(a => a.PushCount == pushCount)
            .OrderByDescending(a => a.MeasureTime)
            .Select(a => (DateTime?)a.MeasureTime)
            .FirstOrDefaultAsync();
        if (samePushLatestTime.HasValue && measureTime < samePushLatestTime.Value)
            parsed.Errors.Add(new(0, "", "测量时间", measureTime.ToString("yyyy-MM-dd HH:00"), BizMessages.SteelMeasureTimeOlderThanLatest(Lang)));
        if (parsed.Rows.Count == 0 && parsed.Errors.Count == 0)
            parsed.Errors.Add(new(0, "", BizMessages.FieldBusinessRule(Lang), "", BizMessages.SteelMeasuredNoValidPoints(Lang)));
        if (!parsed.Success) return SaveFailure(context, "实测数据", pushCount, measureTime, timestamp, bytes, parsed.Errors);
        // TODO [Shen, Lin] 使用实际数据发现，坐标点位在同一个梁段内的时候，方向是不确定的，先不再做方向验证
        // SteelGirderAlignment.ValidateSequenceDirection(parsed);
        var successPath = SaveOriginal(context, "实测数据", pushCount, measureTime, timestamp, bytes, false);
        try
        {
            Db.BeginTransaction();
            var account = GetCurrentAccount();
            var currentBatches = Db.Query<biz_steel_beam_measure_batch>().Where(a => a.GirderID == girderID && a.Status != -1);
            var currentHasBatch = await currentBatches.AnyAsync();
            var currentMaxPush = currentHasBatch ? await currentBatches.MaxAsync(a => a.PushCount) : (int?)null;
            var currentAllowed = currentMaxPush.HasValue ? new[] { currentMaxPush.Value, currentMaxPush.Value + 1 } : new[] { 0 };
            if (!currentAllowed.Contains(pushCount)) throw new InvalidOperationException(BizMessages.SteelPushCountChanged(Lang));
            oldBatch = await currentBatches.FirstOrDefaultAsync(a => a.PushCount == pushCount && a.MeasureTime == measureTime);
            if (oldBatch != null && !confirmOverwrite) throw new InvalidOperationException(BizMessages.SteelMeasuredChangedConfirm(Lang));
            List<biz_steel_beam_measured> oldRows = [];
            if (oldBatch != null)
            {
                oldBatch.Status = -1; oldBatch.UpdatedBy = account; oldBatch.UpdatedTime = timestamp;
                oldRows = await Db.Query<biz_steel_beam_measured>().Where(a => a.BatchID == oldBatch.ID && a.Status != -1).ToListAsync();
                foreach (var old in oldRows) { old.Status = -1; old.UpdatedBy = account; old.UpdatedTime = timestamp; }
                // 被覆盖批次的推算坐标一并作废，随后按新批次重新推算
                var oldDerived = await Db.Query<biz_steel_beam_derived_coordinate>().Where(a => a.BatchID == oldBatch.ID && a.Status != -1).ToListAsync();
                foreach (var old in oldDerived) { old.Status = -1; old.UpdatedBy = account; old.UpdatedTime = timestamp; }
                await Db.SaveAsync();
            }
            var batch = new biz_steel_beam_measure_batch
            {
                ProjID = context.ProjID,
                GirderID = girderID,
                PushCount = pushCount,
                MeasureTime = measureTime,
                ImportCount = parsed.Rows.Count,
                Status = 0,
                CreatedBy = account,
                CreatedTime = timestamp
            };
            Db.GetRepository<biz_steel_beam_measure_batch>().Add(batch);
            await Db.SaveAsync();
            if (oldBatch != null) oldBatch.ReplacedByBatchID = batch.ID;
            var rows = parsed.Rows.Select(a =>
            {
                var point = theoryMap[a.PointCode];
                return new biz_steel_beam_measured
                {
                    ProjID = context.ProjID,
                    GirderID = girderID,
                    BatchID = batch.ID,
                    TheoreticalID = point.ID,
                    PointCode = point.PointCode,
                    MeasuredX = a.MeasuredX,
                    MeasuredY = a.MeasuredY,
                    MeasuredZ = a.MeasuredZ,
                    ImportTime = timestamp,
                    Version = 1,
                    Status = 0,
                    CreatedBy = account,
                    CreatedTime = timestamp
                };
            }).ToList();
            Db.GetRepository<biz_steel_beam_measured>().AddList(rows);
            await Db.SaveAsync();

            // 同一 PushCount 允许多个 batch：本次为最新，之前同 PushCount 的 batch 视为被覆盖。
            // 旧 batch 的实测行一并作废，避免 replay 时被当成有效数据。
            var supersededBatches = await Db.Query<biz_steel_beam_measure_batch>()
                .Where(a => a.GirderID == girderID && a.PushCount == pushCount && a.ID != batch.ID && a.Status != StatusDelete)
                .ToListAsync();
            foreach (var old in supersededBatches)
            {
                old.Status = StatusDelete;
                old.ReplacedByBatchID = batch.ID;
                old.UpdatedBy = account;
                old.UpdatedTime = timestamp;
            }
            var supersededIds = supersededBatches.Select(a => a.ID).ToList();
            if (supersededIds.Count > 0)
            {
                var supersededMeasured = await Db.Query<biz_steel_beam_measured>()
                    .Where(a => supersededIds.Contains(a.BatchID) && a.Status != StatusDelete)
                    .ToListAsync();
                foreach (var old in supersededMeasured)
                {
                    old.Status = StatusDelete;
                    old.UpdatedBy = account;
                    old.UpdatedTime = timestamp;
                }
            }
            await Db.SaveAsync();

            // replay 0..当前 PushCount：按最新快照重建 derived（唯一滚动快照）。
            // 之前批次不可修改，因此 replay 是确定性的；失败直接回滚整个导入事务。
            var derived = await RebuildDerivedCoreAsync(context.ProjID, girderID, account, timestamp);
            Db.CommitTransaction();
            var successMessage = derived.PointCount > 0
                ? BizMessages.SteelMeasuredImportSuccessWithDerived(Lang, rows.Count, derived.PointCount)
                : BizMessages.SteelMeasuredImportSuccess(Lang, rows.Count);
            if (parsed.IncompleteSkipped.Count > 0)
                successMessage += "；" + BizMessages.SteelMeasuredIncompletePointsSkipped(
                    Lang, parsed.IncompleteSkipped.Select(a => a.PointCode));
            return new()
            {
                Success = true,
                ImportedCount = rows.Count,
                Message = successMessage
            };
        }
        catch (InvalidOperationException ex)
        {
            // 业务规则类失败（顶推次数已变化、需确认覆盖、推算公共点不足等）：原样透出双语文案
            Db.RollbackTransaction();
            TryDelete(successPath);
            return SaveFailure(context, "实测数据", pushCount, measureTime, timestamp, bytes, [new(0, "", BizMessages.FieldBusinessRule(Lang), "", ex.Message)]);
        }
        catch (Exception ex)
        {
            Db.RollbackTransaction();
            TryDelete(successPath);
            return SaveFailure(context, "实测数据", pushCount, measureTime, timestamp, bytes, [new(0, "", BizMessages.FieldDatabase(Lang), "", BizMessages.SaveFailed(Lang, ex.Message))]);
        }
    }

    public byte[] TheoreticalTemplate() => SteelBeamExcelHelper.CreateTheoreticalTemplate();
    public byte[] MeasuredTemplate() => SteelBeamExcelHelper.CreateMeasuredTemplate();
    public void EnsureAuthenticated() => _ = GetCurrentAccount();

    public async Task<(byte[] Bytes, string FileName)> ExportMeasured(SteelBeamMeasuredQueryDTO req)
    {
        var context = await EnsureGirderAccess(req.GirderID);
        var query = from measured in Db.Query<biz_steel_beam_measured>().Where(a => a.GirderID == req.GirderID && a.Status != -1)
                    join batch in Db.Query<biz_steel_beam_measure_batch>().Where(a => a.Status != -1) on measured.BatchID equals batch.ID
                    select new { measured, batch };
        if (req.PushCount.HasValue) query = query.Where(a => a.batch.PushCount == req.PushCount.Value);
        if (req.MeasureTime.HasValue) query = query.Where(a => a.batch.MeasureTime == req.MeasureTime.Value);
        var data = await query.Select(a => new
        {
            a.measured.PointCode,
            a.measured.MeasuredX,
            a.measured.MeasuredY,
            a.measured.MeasuredZ,
            a.measured.TheoreticalID,
            a.batch.PushCount,
            a.batch.MeasureTime
        }).ToListAsync();
        if (data.Count == 0) throw new InvalidOperationException(BizMessages.NoDownloadData(Lang));
        var ordered = data.OrderBy(a => a.TheoreticalID)
            .ThenBy(a => a.MeasureTime).ThenBy(a => a.PushCount).ToList();
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("实测数据");
        var headers = new[] { "测点编号", "实测坐标X(m)", "实测坐标Y(m)", "实测坐标Z(m)", "顶推次数", "测量时间" };
        for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
        for (var i = 0; i < ordered.Count; i++)
        {
            var row = i + 2; var item = ordered[i];
            sheet.Cell(row, 1).Value = item.PointCode;
            sheet.Cell(row, 2).Value = item.MeasuredX; sheet.Cell(row, 3).Value = item.MeasuredY; sheet.Cell(row, 4).Value = item.MeasuredZ;
            sheet.Cell(row, 5).Value = item.PushCount; sheet.Cell(row, 6).Value = item.MeasureTime.ToString("yyyy-MM-dd HH:00");
        }
        var start = 2;
        while (start <= ordered.Count + 1)
        {
            var code = sheet.Cell(start, 1).GetString(); var end = start;
            while (end + 1 <= ordered.Count + 1 && string.Equals(sheet.Cell(end + 1, 1).GetString(), code, StringComparison.OrdinalIgnoreCase)) end++;
            if (end > start) sheet.Range(start, 1, end, 1).Merge();
            start = end + 1;
        }
        var used = sheet.RangeUsed();
        if (used != null)
        {
            used.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            used.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            used.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
        sheet.Row(1).Style.Font.Bold = true; sheet.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF2FF");
        sheet.Columns().AdjustToContents(); sheet.SheetView.FreezeRows(1);
        using var output = new MemoryStream(); workbook.SaveAs(output);
        var name = $"{ExcelUtils.SanitizePathPart(context.BridgeName)}_实测数据_{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
        return (output.ToArray(), name);
    }

    private SteelBeamImportOutcome SaveFailure(GirderContext context, string type, int? pushCount, DateTime? measureTime,
        DateTime timestamp, byte[] original, IEnumerable<SteelBeamImportError> errors)
    {
        var errorList = errors.ToList();
        var errorBytes = SteelBeamExcelHelper.CreateErrorFile(errorList, Lang);
        var originalPath = SaveOriginal(context, type, pushCount, measureTime, timestamp, original, true);
        var baseName = Path.GetFileNameWithoutExtension(originalPath).Replace("_失败", string.Empty, StringComparison.Ordinal);
        var errorName = $"{baseName}_错误明细.xlsx";
        File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(originalPath)!, errorName), errorBytes);
        return new() { Success = false, Message = BizMessages.ImportFailedSeeDetail(Lang), ErrorFile = errorBytes, ErrorFileName = errorName };
    }

    private string SaveOriginal(GirderContext context, string type, int? pushCount, DateTime? measureTime, DateTime timestamp, byte[] bytes, bool failed)
    {
        var root = _configuration["SteelBeam:FileRoot"];
        if (string.IsNullOrWhiteSpace(root)) root = Path.Combine(AppContext.BaseDirectory, "App_Data");
        var project = ExcelUtils.SanitizePathPart(context.ProjectName);
        var bridge = ExcelUtils.SanitizePathPart(context.BridgeName);
        var directory = Path.Combine(root, "UploadFile", project, bridge, type);
        Directory.CreateDirectory(directory);
        var timestampText = timestamp.ToString("yyyyMMddHHmmssfff");
        var fileName = type == "理论数据"
            ? $"{bridge}_理论数据_{timestampText}{(failed ? "_失败" : string.Empty)}.xlsx"
            : $"{bridge}_{measureTime:yyyyMMddHHmm}_{pushCount}_{timestampText}{(failed ? "_失败" : string.Empty)}.xlsx";
        var path = Path.Combine(directory, fileName);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    private async Task<HashSet<int>?> GetAllowedProjectIds()
    {
        var account = GetCurrentAccount().Trim().ToLower();
        var users = Db.Query<sys_userinfo>()
            .Where(a => a.Status == 1 && a.Account.ToLower() == account);
        var isAdmin = await (from user in users
                             join userRole in Db.Query<sys_user_role>().Where(a => a.Status == 1)
                                 on user.ID equals userRole.UserID
                             join role in Db.Query<sys_role>().Where(a => a.Status == 1)
                                 on userRole.RoleID equals role.ID
                             where role.Code != null &&
                                   (role.Code.ToLower() == "role_admin" || role.Code.ToLower() == "role_biz_admin")
                             select role.ID).AnyAsync();
        if (isAdmin) return null;

        var projectIds = await (from user in users
                                join projectUser in Db.Query<biz_project_user>().Where(a => a.Status == 1)
                                    on user.ID equals projectUser.UserID
                                select projectUser.ProjID).Distinct().ToListAsync();
        return projectIds.ToHashSet();
    }

    private async Task EnsureProjectAccess(int projID)
    {
        var allowed = await GetAllowedProjectIds();
        if (allowed != null && !allowed.Contains(projID)) throw new UnauthorizedAccessException(BizMessages.NoProjectPermission(Lang));
    }

    private async Task<GirderContext> EnsureGirderAccess(int girderID)
    {
        var bridge = await (from b in Db.Query<biz_project_bridge>().Where(a => a.Status != -1 && a.BeamType == 0 && a.ID == girderID)
                            join p in Db.Query<biz_project>().Where(a => a.Status != -1) on b.ProjID equals p.ID
                            select new GirderContext(b.ProjID, p.Name, b.Name)).FirstOrDefaultAsync()
                     ?? throw new InvalidOperationException(BizMessages.SteelGirderMissing(Lang));
        await EnsureProjectAccess(bridge.ProjID);
        return bridge;
    }

    private string GetCurrentAccount()
    {
        var account = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(account)) throw new UnauthorizedAccessException(BizMessages.AccountMissing(Lang));
        return account;
    }

    private static void ValidateCoordinate(decimal x, decimal y, decimal z)
    {
        if (!ExcelUtils.HasScale(x, 4) || !ExcelUtils.HasScale(y, 4) || !ExcelUtils.HasScale(z, 4))
            throw new InvalidOperationException("坐标最多保留4位小数");
    }

    private static int NormalizePageSize(int size) => size is 10 or 20 or 50 or 100 ? size : 10;
    private sealed record GirderContext(int ProjID, string ProjectName, string BridgeName);

    /// <summary>
    /// 计算页签的梁段下拉：返回全部梁段，并标记是否已拼装。
    /// <para>已拼装判定：该段在 derived 快照中至少有一个 MeasuredID &gt; 0 的点；
    /// 从未实测、只有补全值的段不算已拼装。</para>
    /// <para>排序：按同一测位下梁段的最小 DistanceFromStart（缺失时退回 PositionOrder），
    /// 这样前端可以按梁段顺序判断“连续”。</para>
    /// </summary>
    public async Task<List<SteelBeamCalcSegmentOptionDTO>> GetCalcSegmentNoByGirderId(int girderId)
    {
        await EnsureGirderAccess(girderId);
        var snapshot = await LoadSnapshotAsync(girderId);
        var assembled = snapshot
            .Where(a => a.MeasuredID != 0)
            .Select(a => a.SegmentNo)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var theory = await Db.Query<biz_steel_beam_theoretical>()
            .Where(a => a.GirderID == girderId && a.Status != StatusDelete)
            .Select(a => new { a.SegmentNo, a.PositionOrder, a.DistanceFromStart })
            .ToListAsync();

        return theory
            .GroupBy(a => a.SegmentNo, StringComparer.OrdinalIgnoreCase)
            .Select(g => new
            {
                SegmentNo = g.Key,
                Order = g.Min(x => x.DistanceFromStart ?? (decimal?)x.PositionOrder ?? decimal.MaxValue),
                IsAssembled = assembled.Contains(g.Key)
            })
            .OrderBy(a => a.Order)
            .ThenBy(a => a.SegmentNo, StringComparer.OrdinalIgnoreCase)
            .Select(a => new SteelBeamCalcSegmentOptionDTO
            {
                SegmentNo = a.SegmentNo,
                IsAssembled = a.IsAssembled
            })
            .ToList();
    }

    /// <summary>
    /// 计算所选梁段的定位坐标。
    /// <para>已拼装段（至少一个 MeasuredID&gt;0 的点）：直接读 derived 快照，不再重算；</para>
    /// <para>待拼装段：用快照中 MeasuredID&gt;0 的点做 P/Q 配准，D=待拼装段设计坐标，Gescha 预测；</para>
    /// <para>允许混合选择，但所选梁段必须按梁段顺序连续（不跳段）。</para>
    /// </summary>
    public async Task<CalcResultDTO> GetCalcResultBySegment(int girderId, List<string> segmentNos)
    {
        if (segmentNos == null || segmentNos.Count == 0)
            throw new InvalidOperationException(BizMessages.ParameterRequired(Lang));

        // 项目信息
        var projQuery = from gird in Db.Query<biz_project_bridge>().Where(a => a.ID == girderId)
                        from proj in Db.Query<biz_project>().Where(a => a.ID == gird.ProjID)
                        select proj;
        var projInfo = projQuery.FirstOrDefault()
            ?? throw new Exception(BizMessages.ProjectInfoNotFound(Lang));

        // 理论点：用于段顺序、待拼装 D、输出顺序
        var theory = await Db.Query<biz_steel_beam_theoretical>()
            .Where(a => a.GirderID == girderId && a.Status != StatusDelete)
            .ToListAsync();
        if (theory.Count == 0)
            throw new InvalidOperationException(BizMessages.SteelTheoryRequired(Lang));

        var segmentOrder = theory
            .GroupBy(a => a.SegmentNo, StringComparer.OrdinalIgnoreCase)
            .Select(g => new
            {
                SegmentNo = g.Key,
                Order = g.Min(x => x.DistanceFromStart ?? (decimal?)x.PositionOrder ?? decimal.MaxValue)
            })
            .OrderBy(a => a.Order)
            .ThenBy(a => a.SegmentNo, StringComparer.OrdinalIgnoreCase)
            .Select(a => a.SegmentNo)
            .ToList();
        EnsureSegmentsContinuous(segmentNos, segmentOrder);

        var snapshot = await LoadSnapshotAsync(girderId);
        var snapshotBySegment = snapshot
            .GroupBy(a => a.SegmentNo, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        // 已拼装 = 该段在实测表中至少有一个点；即使 derived 快照缺失也能识别，
        // 从而给出「请先 RebuildDerived」的明确提示，而不是静默当成待拼装。
        var measuredTheoryIds = (await Db.Query<biz_steel_beam_measured>()
            .Where(a => a.GirderID == girderId && a.Status != StatusDelete)
            .Select(a => a.TheoreticalID).Distinct().ToListAsync()).ToHashSet();
        var theoryBySegment = theory
            .GroupBy(a => a.SegmentNo, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
        var assembledSegments = segmentNos
            .Where(s => theoryBySegment.TryGetValue(s, out var pts) && pts.Any(t => measuredTheoryIds.Contains(t.ID)))
            .ToList();
        var pendingSegments = segmentNos
            .Where(s => !assembledSegments.Contains(s, StringComparer.OrdinalIgnoreCase))
            .ToList();

        // 已拼装段缺快照：拒绝计算，提示先 RebuildDerived（不要静默回退，避免口径不一致）
        foreach (var s in assembledSegments)
        {
            if (!snapshotBySegment.TryGetValue(s, out var rows) || rows.Count == 0)
                throw new InvalidOperationException(BizMessages.SteelDerivedSnapshotMissing(Lang, s));
        }

        var theoryById = theory.ToDictionary(a => a.ID);
        var rowsWithOrder = new List<(CalcResultRowDTO Row, decimal Order)>();

        // 1) 已拼装段：直接读 derived 快照（含 MeasuredID=0 的补全点）
        foreach (var s in assembledSegments)
        {
            foreach (var point in snapshotBySegment[s])
            {
                var t = theoryById.GetValueOrDefault(point.TheoreticalID);
                rowsWithOrder.Add((new CalcResultRowDTO
                {
                    PointCode = point.PointCode,
                    DesignX = point.X,
                    DesignY = point.Y,
                    DesignZ = point.Z,
                    SegmentNo = point.SegmentNo,
                    PositionName = t?.PositionName ?? string.Empty
                }, t?.DistanceFromStart ?? (decimal?)t?.PositionOrder ?? decimal.MaxValue));
            }
        }

        // 2) 待拼装段：P/Q 只用 MeasuredID>0 的点；D=待拼装段设计坐标
        if (pendingSegments.Count > 0)
        {
            var p = new List<List<decimal>>() { new(), new(), new() };
            var q = new List<List<decimal>>() { new(), new(), new() };
            var r = new List<List<decimal>>() { new(), new(), new() };
            foreach (var row in snapshot.Where(a => a.MeasuredID != 0))
            {
                if (!theoryById.TryGetValue(row.TheoreticalID, out var t)) continue;
                p[0].Add(t.DesignX); p[1].Add(t.DesignY); p[2].Add(t.DesignZ);
                q[0].Add(row.X); q[1].Add(row.Y); q[2].Add(row.Z);
                r[0].Add(t.Weight); r[1].Add(t.Weight); r[2].Add(t.Weight);
            }
            if (p[0].Count < 3)
                throw new InvalidOperationException(BizMessages.SteelCalcRegistrationPointsInsufficient(Lang, p[0].Count));

            var pendingTheory = theory
                .Where(a => pendingSegments.Contains(a.SegmentNo, StringComparer.OrdinalIgnoreCase))
                .OrderBy(a => a.DistanceFromStart ?? (decimal?)a.PositionOrder ?? decimal.MaxValue)
                .ThenBy(a => a.ID)
                .ToList();
            var d = new List<List<decimal>>() { new(), new(), new() };
            foreach (var t in pendingTheory)
            {
                d[0].Add(t.DesignX); d[1].Add(t.DesignY); d[2].Add(t.DesignZ);
            }

            var gescha = SteelGirderAlignment.Gescha(P: p, Q: q, D: d, R: r);
            for (var i = 0; i < pendingTheory.Count; i++)
            {
                var t = pendingTheory[i];
                rowsWithOrder.Add((new CalcResultRowDTO
                {
                    PointCode = t.PointCode,
                    DesignX = Math.Round(gescha[0][i], 4, MidpointRounding.AwayFromZero),
                    DesignY = Math.Round(gescha[1][i], 4, MidpointRounding.AwayFromZero),
                    DesignZ = Math.Round(gescha[2][i], 4, MidpointRounding.AwayFromZero),
                    SegmentNo = t.SegmentNo,
                    PositionName = t.PositionName
                }, t.DistanceFromStart ?? (decimal?)t.PositionOrder ?? decimal.MaxValue));
            }
        }

        return new CalcResultDTO
        {
            ProjName = projInfo.Name,
            SheetNo = "请填写",
            MeasuredSegments = string.Join(",", assembledSegments),
            PendingSegments = string.Join(",", pendingSegments),
            Rows = rowsWithOrder
                .OrderBy(a => a.Order)
                .ThenBy(a => a.Row.PointCode, StringComparer.OrdinalIgnoreCase)
                .Select(a => a.Row)
                .ToList()
        };
    }

    /// <summary>
    /// 校验所选梁段在梁段顺序上连续（不跳段）。
    /// <para>顺序来自同一测位下各梁段的最小 DistanceFromStart，缺失时退回 PositionOrder。</para>
    /// </summary>
    private void EnsureSegmentsContinuous(List<string> selected, List<string> segmentOrder)
    {
        var indices = new List<int>();
        foreach (var s in selected)
        {
            var idx = segmentOrder.FindIndex(x => string.Equals(x, s, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) indices.Add(idx);
        }
        if (indices.Count != selected.Count)
            throw new InvalidOperationException(BizMessages.SteelSegmentNotInTheory(Lang, selected.FirstOrDefault() ?? string.Empty));

        indices.Sort();
        for (var i = 1; i < indices.Count; i++)
            if (indices[i] != indices[i - 1] + 1)
                throw new InvalidOperationException(BizMessages.SteelCalcSegmentsNotContinuous(Lang));
    }

    /// <summary>
    /// 重算本钢梁全部顶推批次的推算坐标（滚动快照）。
    /// <para>业务：按 PushCount 升序 replay，每个 PushCount 取 MeasureTime 最新的 batch；
    /// 每次 replay 在上一次快照基础上，按「本批实测 / 历史实测点搬运 / 从未实测点补全」生成新快照，
    /// 最终只保留一份按 TheoreticalID 逻辑唯一的最新坐标。</para>
    /// <para>用途：①迁移后回填；②人工修改实测数据后刷新；③同一 PushCount 多 batch 的最新覆盖。</para>
    /// </summary>
    public async Task<string> RebuildDerived(int girderID)
    {
        var context = await EnsureGirderAccess(girderID);
        var account = GetCurrentAccount();
        var timestamp = DateTime.Now;
        Db.BeginTransaction();
        try
        {
            var (batchCount, pointCount) = await RebuildDerivedCoreAsync(context.ProjID, girderID, account, timestamp);
            Db.CommitTransaction();
            return BizMessages.SteelDerivedRebuildSuccess(Lang, batchCount, pointCount);
        }
        catch
        {
            Db.RollbackTransaction();
            throw;
        }
    }

    /// <summary>
    /// 批量重建推算坐标：按项目或指定梁列表逐根 replay。
    /// <para>每根梁独立事务：某根失败只记录该根失败，不影响其它梁；返回逐根结果便于排查。</para>
    /// <para>只处理钢梁（BeamType=0），已删除/非钢梁不计入。</para>
    /// </summary>
    public async Task<List<SteelBeamRebuildResultDTO>> RebuildDerivedBatch(SteelBeamRebuildBatchDTO req)
    {
        if (req == null) throw new InvalidOperationException(BizMessages.ParameterRequired(Lang));
        if (!req.ProjID.HasValue && (req.GirderIDs == null || req.GirderIDs.Count == 0))
            throw new InvalidOperationException(BizMessages.SteelRebuildBatchFilterRequired(Lang));

        var query = Db.Query<biz_project_bridge>()
            .Where(a => a.Status != StatusDelete && a.BeamType == 0);
        if (req.ProjID.HasValue) query = query.Where(a => a.ProjID == req.ProjID.Value);
        if (req.GirderIDs != null && req.GirderIDs.Count > 0)
            query = query.Where(a => req.GirderIDs.Contains(a.ID));

        var girders = await query
            .OrderBy(a => a.ID)
            .Select(a => new { a.ID, a.Name })
            .ToListAsync();
        if (girders.Count == 0)
            throw new InvalidOperationException(BizMessages.SteelRebuildBatchNoGirder(Lang));

        var results = new List<SteelBeamRebuildResultDTO>(girders.Count);
        foreach (var g in girders)
        {
            try
            {
                var message = await RebuildDerived(g.ID);
                results.Add(new SteelBeamRebuildResultDTO
                {
                    GirderID = g.ID,
                    GirderName = g.Name ?? string.Empty,
                    Success = true,
                    Message = message
                });
            }
            catch (Exception ex)
            {
                results.Add(new SteelBeamRebuildResultDTO
                {
                    GirderID = g.ID,
                    GirderName = g.Name ?? string.Empty,
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        return results;
    }

    /// <summary>
    /// replay 核心：从 PushCount=0 到最大批次逐批重建滚动快照，最终 upsert 到 derived 表。
    /// <para>必须升序：第 k 批依赖第 k-1 批的快照；同一 PushCount 只取 MeasureTime 最新的 batch。
    /// 旧 batch 独有的点不会进入快照，天然满足「同一次顶推以最新 batch 为准」。</para>
    /// </summary>
    private async Task<(int BatchCount, int PointCount)> RebuildDerivedCoreAsync(
        int projID, int girderID, string account, DateTime timestamp)
    {
        var batches = await Db.Query<biz_steel_beam_measure_batch>()
            .Where(a => a.GirderID == girderID && a.Status != StatusDelete)
            .ToListAsync();
        if (batches.Count == 0)
            throw new InvalidOperationException(BizMessages.SteelDerivedNothingToRebuild(Lang));

        // 同一 PushCount 允许多个 batch：以 MeasureTime 最新的为准，旧 batch 视为被覆盖
        var latestByPush = batches
            .GroupBy(a => a.PushCount)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.MeasureTime).ThenByDescending(a => a.ID).First());
        var maxPush = latestByPush.Keys.Max();

        var theory = await LoadAllTheoryPointsAsync(girderID);
        var snapshot = new List<SteelBeamDerivedCalculator.SnapshotPoint>();
        var batchCount = 0;

        for (var k = 0; k <= maxPush; k++)
        {
            if (!latestByPush.TryGetValue(k, out var batch)) continue;
            var measured = await LoadMeasuredWithTheoryAsync(batch.ID);
            var result = SteelBeamDerivedCalculator.BuildSnapshot(k, batch.ID, measured, snapshot, theory, Lang);
            if (!result.Success) throw new InvalidOperationException(result.ErrorMessage);
            snapshot = result.Rows;
            batchCount++;
        }

        await ReplaceSnapshotAsync(projID, girderID, maxPush, snapshot, account, timestamp);
        return (batchCount, snapshot.Count);
    }

    /// <summary>
    /// 把滚动快照 upsert 到 derived 表：按 TheoreticalID 逻辑唯一。
    /// <para>已存在的点更新坐标/来源；新点插入；本快照不再包含的 active 行标记 Status=-1。
    /// 同一 TheoreticalID 若有多条历史 active 行，只保留 ID 最大的一条。</para>
    /// </summary>
    private async Task ReplaceSnapshotAsync(int projID, int girderID, int pushCount,
        List<SteelBeamDerivedCalculator.SnapshotPoint> snapshot, string account, DateTime timestamp)
    {
        var existing = await Db.Query<biz_steel_beam_derived_coordinate>()
            .Where(a => a.GirderID == girderID && a.Status != StatusDelete)
            .ToListAsync();
        var groups = existing
            .GroupBy(a => a.TheoreticalID)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.ID).ToList());
        var keep = snapshot.Select(a => a.TheoreticalID).ToHashSet();

        foreach (var row in snapshot)
        {
            if (groups.TryGetValue(row.TheoreticalID, out var olds) && olds.Count > 0)
            {
                var entity = olds[0];
                entity.ProjID = projID;
                entity.BatchID = row.BatchID;
                entity.PushCount = pushCount;
                entity.SourcePushCount = row.SourcePushCount;
                entity.MeasuredID = row.MeasuredID;
                entity.PointCode = row.PointCode;
                entity.SegmentNo = row.SegmentNo;
                entity.DerivedX = row.X;
                entity.DerivedY = row.Y;
                entity.DerivedZ = row.Z;
                entity.Status = 0;
                entity.UpdatedBy = account;
                entity.UpdatedTime = timestamp;
                // 同一测点的历史重复 active 行一并作废，保证逻辑唯一
                foreach (var extra in olds.Skip(1))
                {
                    extra.Status = StatusDelete;
                    extra.UpdatedBy = account;
                    extra.UpdatedTime = timestamp;
                }
            }
            else
            {
                Db.GetRepository<biz_steel_beam_derived_coordinate>().Add(new biz_steel_beam_derived_coordinate
                {
                    ProjID = projID,
                    GirderID = girderID,
                    BatchID = row.BatchID,
                    PushCount = pushCount,
                    SourcePushCount = row.SourcePushCount,
                    MeasuredID = row.MeasuredID,
                    TheoreticalID = row.TheoreticalID,
                    PointCode = row.PointCode,
                    SegmentNo = row.SegmentNo,
                    DerivedX = row.X,
                    DerivedY = row.Y,
                    DerivedZ = row.Z,
                    Version = 1,
                    Status = 0,
                    CreatedBy = account,
                    CreatedTime = timestamp
                });
            }
        }

        // 快照不再包含的 active 行：标记删除，避免旧 batch 独有的点残留
        foreach (var (theoreticalID, olds) in groups)
        {
            if (keep.Contains(theoreticalID)) continue;
            foreach (var old in olds)
            {
                old.Status = StatusDelete;
                old.UpdatedBy = account;
                old.UpdatedTime = timestamp;
            }
        }

        await Db.SaveAsync();
    }

    /// <summary>
    /// 载入某批次上传的实测点（带理论坐标 + 实测行ID）。
    /// <para>MeasuredID 写回 derived，供误差分析区分「实测来源」与「纯补全」。</para>
    /// </summary>
    private async Task<List<SteelBeamDerivedCalculator.MeasuredPoint>> LoadMeasuredWithTheoryAsync(int batchID)
    {
        var rows = await (from m in Db.Query<biz_steel_beam_measured>().Where(a => a.BatchID == batchID && a.Status != StatusDelete)
                          join t in Db.Query<biz_steel_beam_theoretical>().Where(a => a.Status != StatusDelete) on m.TheoreticalID equals t.ID
                          orderby m.TheoreticalID
                          select new
                          {
                              m.ID,
                              m.TheoreticalID,
                              m.PointCode,
                              t.SegmentNo,
                              t.DesignX,
                              t.DesignY,
                              t.DesignZ,
                              m.MeasuredX,
                              m.MeasuredY,
                              m.MeasuredZ,
                              t.Weight
                          }).ToListAsync();
        return rows.Select(a => new SteelBeamDerivedCalculator.MeasuredPoint(
            a.TheoreticalID, a.PointCode, a.SegmentNo,
            a.DesignX, a.DesignY, a.DesignZ,
            a.MeasuredX, a.MeasuredY, a.MeasuredZ,
            a.Weight, a.ID)).ToList();
    }

    /// <summary>载入本钢梁全部理论测点（逐段补全与出胎段识别共用）。</summary>
    private async Task<List<SteelBeamDerivedCalculator.TheoryPoint>> LoadAllTheoryPointsAsync(int girderID)
    {
        var rows = await Db.Query<biz_steel_beam_theoretical>()
            .Where(a => a.GirderID == girderID && a.Status != StatusDelete)
            .ToListAsync();
        return rows.Select(a => new SteelBeamDerivedCalculator.TheoryPoint(
            a.ID, a.PointCode, a.SegmentNo, a.DesignX, a.DesignY, a.DesignZ, a.Weight)).ToList();
    }

    /// <summary>读取当前滚动快照（derived 表 active 行，按 TheoreticalID 逻辑唯一）。</summary>
    private async Task<List<SteelBeamDerivedCalculator.SnapshotPoint>> LoadSnapshotAsync(int girderID)
    {
        var rows = await (from d in Db.Query<biz_steel_beam_derived_coordinate>().Where(a => a.GirderID == girderID && a.Status != StatusDelete)
                          join t in Db.Query<biz_steel_beam_theoretical>().Where(a => a.Status != StatusDelete) on d.TheoreticalID equals t.ID
                          select new
                          {
                              d.TheoreticalID,
                              d.PointCode,
                              d.SegmentNo,
                              d.DerivedX,
                              d.DerivedY,
                              d.DerivedZ,
                              d.MeasuredID,
                              d.BatchID,
                              t.Weight
                          }).ToListAsync();
        return rows.Select(a => new SteelBeamDerivedCalculator.SnapshotPoint(
            a.TheoreticalID, a.PointCode, a.SegmentNo,
            a.DerivedX, a.DerivedY, a.DerivedZ, a.Weight,
            a.MeasuredID, a.BatchID, 0)).ToList();
    }

    /// <summary>
    /// 取当前滚动快照的完整点集，作为待拼装计算的 Q。
    /// <para>口径：只含 derived active 行（每点一条）；MeasuredID&gt;0 表示有实测来源，
    /// MeasuredID=0 表示段内补全，不应该参与 P/Q 配准。</para>
    /// </summary>
    private async Task<List<(biz_steel_beam_theoretical Theory, decimal X, decimal Y, decimal Z, int MeasuredID)>> LoadLatestCompleteSetAsync(int girderID)
    {
        var rows = await (from d in Db.Query<biz_steel_beam_derived_coordinate>().Where(a => a.GirderID == girderID && a.Status != StatusDelete)
                          join t in Db.Query<biz_steel_beam_theoretical>().Where(a => a.Status != StatusDelete) on d.TheoreticalID equals t.ID
                          select new
                          {
                              t,
                              d.DerivedX,
                              d.DerivedY,
                              d.DerivedZ,
                              d.MeasuredID
                          }).ToListAsync();
        return rows.Select(a => (a.t, a.DerivedX, a.DerivedY, a.DerivedZ, a.MeasuredID)).ToList();
    }

    /// <summary>
    /// 根据girderId获取测位列表
    /// </summary>
    /// <param name="girderId"></param>
    /// <returns></returns>
    public async Task<List<string>> GetPositionNameByGirderId(int girderId)
    {
        var query = from proj in Db.Query<biz_project>().Where(a => a.Status != StatusDelete)
                    from gird in Db.Query<biz_project_bridge>().Where(a => a.Status != StatusDelete && a.ProjID == proj.ID)
                    from theo in Db.Query<biz_steel_beam_theoretical>().Where(a => a.Status != StatusDelete && a.ProjID == proj.ID && a.GirderID == gird.ID)
                    where theo.GirderID == girderId
                    select theo.PositionName;
        var result = await query.Distinct().OrderBy(a => a).ToListAsync();
        result = result.Where(a => a != null).ToList();
        return result;
    }

    /// <summary>
    /// 计算梁段Z向线形偏差（误差分析数据源）。
    /// 数据来源：biz_steel_beam_derived_coordinate（推算坐标表）中「最大顶推次数的最新批次」。
    /// 该表按批次保存「完整梁段」当批坐标（在架段实测/补全 ∪ 出胎段搬运），
    /// 每个测点每批次仅一行，因此天然不存在「同一点位跨顶推重复」的问题。
    /// 口径说明：Z偏差 = 当批推算坐标 DerivedZ − 完工设计 Z（成桥组装态残差）；
    /// 与早期「相对首段单次刚体变换」的实时算法语义不同，属既定口径变更。
    /// </summary>
    /// <param name="girderId">梁ID</param>
    /// <param name="positionName">测位筛选，不传则全部</param>
    /// <param name="offsetType">偏差类型预留</param>
    /// <param name="sort">按 X 轴（点位距离）排序 asc/desc（距离恒为正值）</param>
    /// <summary>
    /// 误差分析：读取当前滚动快照（derived 表 active 行）并计算 Z 偏差。
    /// <para>MeasuredID=0 的补全点：ZError 返回 null，前端折线图留空位（不画成 0）。</para>
    /// <para>这样示例中「push0 上传 01/02/03，push1 上传 01/03/04/05」时，
    /// 01-05 是有效点；06-10 从未实测，保留为空位。</para>
    /// </summary>
    public async Task<CalcSteelBeamOffsetDTO> GetSteelBeamOffset(int girderId, string positionName = "", string offsetType = "", string sort = "asc")
    {
        var result = new CalcSteelBeamOffsetDTO { OffsetType = offsetType };

        // 滚动快照：derived 表 active 行（每点一条），不再按批次过滤
        var query = from d in Db.Query<biz_steel_beam_derived_coordinate>()
                        .Where(a => a.GirderID == girderId && a.Status != StatusDelete)
                    join t in Db.Query<biz_steel_beam_theoretical>()
                        .Where(a => a.Status != StatusDelete && a.GirderID == girderId) on d.TheoreticalID equals t.ID
                    select new { d, t };
        var list = await query.ToListAsync();
        if (list.Count == 0) return result;

        var rows = list
            .Where(a => string.IsNullOrWhiteSpace(positionName) || a.t.PositionName == positionName)
            .Select(a => new CalcSteelBeamOffsetRow
            {
                PushCount = a.d.PushCount,
                SegmentNo = a.t.SegmentNo,
                PointCode = a.t.PointCode,
                PositionName = a.t.PositionName,
                PositionOrder = a.t.PositionOrder,
                PointDistance = a.t.DistanceFromStart,
                X = a.t.DesignX,
                Y = a.t.DesignY,
                Z = a.t.DesignZ,
                PredictZ = a.d.DerivedZ,
                // 只有实测来源的点才画误差；纯补全点返回 null，折线图留空位
                ZError = a.d.MeasuredID != 0 ? a.d.DerivedZ - a.t.DesignZ : (decimal?)null,
                Value = a.d.MeasuredID != 0 ? a.d.DerivedZ - a.t.DesignZ : (decimal?)null
            })
            .ToList();

        // 排序：按 X 轴（点位距离）升/降序；PositionOrder 作稳定次级键
        if (sort.Equals("desc", StringComparison.OrdinalIgnoreCase))
            rows = rows.OrderByDescending(a => a.PointDistance).ThenBy(a => a.PositionOrder).ToList();
        else
            rows = rows.OrderBy(a => a.PointDistance).ThenBy(a => a.PositionOrder).ToList();

        result.Rows = rows;
        return result;
    }

}
