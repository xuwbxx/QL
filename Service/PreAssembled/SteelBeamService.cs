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
        var maxPush = await batches.AnyAsync() ? await batches.MaxAsync(a => a.PushCount) : -1;
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
                CanEdit = a.batch.PushCount == maxPush
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
        await EnsureGirderAccess(req.GirderID);
        // [Shen, Lin] 不再验证，改为四舍五入
        // ValidateCoordinate(req.X, req.Y, req.Z);
        req.X = Math.Round(req.X, 4, MidpointRounding.AwayFromZero);
        req.Y = Math.Round(req.Y, 4, MidpointRounding.AwayFromZero);
        req.Z = Math.Round(req.Z, 4, MidpointRounding.AwayFromZero);
        var maxPush = await Db.Query<biz_steel_beam_measure_batch>()
            .Where(a => a.GirderID == req.GirderID && a.Status != -1).MaxAsync(a => a.PushCount);
        var row = await (from measured in Db.Query<biz_steel_beam_measured>()
                         join batch in Db.Query<biz_steel_beam_measure_batch>().Where(a => a.Status != -1) on measured.BatchID equals batch.ID
                         where measured.ID == req.ID && measured.GirderID == req.GirderID && measured.Status != -1 && measured.Version == req.Version
                         select new { measured, batch }).FirstOrDefaultAsync()
                         ?? throw new InvalidOperationException(BizMessages.DataUpdatedRefresh(Lang));
        if (row.batch.PushCount != maxPush) throw new InvalidOperationException(BizMessages.SteelPushCountNotEditable(Lang));
        row.measured.MeasuredX = req.X; row.measured.MeasuredY = req.Y; row.measured.MeasuredZ = req.Z;
        row.measured.Version++; row.measured.UpdatedBy = GetCurrentAccount(); row.measured.UpdatedTime = DateTime.Now;
        await Db.SaveAsync();
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
        foreach (var row in parsed.SourcePoints.Where(a => !string.IsNullOrEmpty(a.PointCode) && !theoryMap.ContainsKey(a.PointCode)))
            parsed.Errors.Add(new(row.ExcelRow, row.PointCode, BizMessages.FieldPointCode(Lang), row.PointCode, BizMessages.SteelPointCodeNotInTheory(Lang)));
        parsed.Errors.AddRange(SteelBeamExcelHelper.ValidateMeasuredSegmentCompleteness(
            theory.Select(a => new SteelBeamTheoreticalPoint(a.PointCode, a.SegmentNo)),
            parsed.SourcePoints.Select(a => a.PointCode), Lang));
        var activeBatches = Db.Query<biz_steel_beam_measure_batch>().Where(a => a.GirderID == girderID && a.Status != -1);
        var hasBatch = await activeBatches.AnyAsync();
        var maxPush = hasBatch ? await activeBatches.MaxAsync(a => a.PushCount) : (int?)null;
        var allowedPushes = maxPush.HasValue ? new[] { maxPush.Value, maxPush.Value + 1 } : new[] { 0 };
        if (!allowedPushes.Contains(pushCount)) parsed.Errors.Add(new(0, "", "顶推次数", pushCount.ToString(), BizMessages.SteelPushCountOutOfRange(Lang)));
        var currentHour = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0);
        if (measureTime > currentHour) parsed.Errors.Add(new(0, "", "测量时间", measureTime.ToString("yyyy-MM-dd HH:00"), BizMessages.SteelMeasureTimeInFuture(Lang)));
        var oldBatch = await activeBatches.FirstOrDefaultAsync(a => a.PushCount == pushCount && a.MeasureTime == measureTime);
        if (oldBatch != null && !confirmOverwrite) parsed.Errors.Add(new(0, "", BizMessages.FieldBusinessRule(Lang), "", BizMessages.SteelMeasuredOverwriteConfirm(Lang)));
        if (!parsed.Success) return SaveFailure(context, "实测数据", pushCount, measureTime, timestamp, bytes, parsed.Errors);
        // TODO [Shen, Lin] 使用实际数据发现，毕竟不是依次固定方向的，需要询问下是否需要做点位方向验证，以及如何验证
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
            // 顶推0次无「上一批次」，无需推算；其余批次推算已推出胎架梁段的当批坐标，
            // 失败（公共点不足等）直接抛错，整个导入事务回滚，避免 Q 集不完整导致后续算错
            var derivedCount = pushCount > 0
                ? await DeriveForBatchAsync(context.ProjID, girderID, batch.ID, pushCount, account, timestamp)
                : 0;
            Db.CommitTransaction();
            return new()
            {
                Success = true,
                ImportedCount = rows.Count,
                Message = derivedCount > 0
                    ? BizMessages.SteelMeasuredImportSuccessWithDerived(Lang, rows.Count, derivedCount)
                    : BizMessages.SteelMeasuredImportSuccess(Lang, rows.Count)
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
            a.batch.PushCount,
            a.batch.MeasureTime
        }).ToListAsync();
        if (data.Count == 0) throw new InvalidOperationException(BizMessages.NoDownloadData(Lang));
        var ordered = data.OrderBy(a => a.PointCode, StringComparer.OrdinalIgnoreCase)
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
        var directory = Path.Combine(root, "文件上传", project, bridge, type);
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
    /// 根据梁Id获取其下的梁段编号用于监控计算
    /// </summary>
    /// <param name="girderId"></param>
    /// <returns></returns>
    public async Task<List<string>> GetCalcSegmentNoByGirderId(int girderId)
    {
        // 已拼装（已实测/已推算）的梁段 = 最大顶推次数最新批次的完整点集涉及的梁段
        // 注意：不能用「所有批次实测的并集」，否则顶推1次后 L01 只有过期的 push0 数据会被误判为待拼装
        var completeSet = await LoadLatestCompleteSetAsync(girderId);
        var measuredSegmentNo = completeSet.Select(a => a.Theory.SegmentNo).Distinct().ToList();

        var query = from proj in Db.Query<biz_project>().Where(a => a.Status != StatusDelete)
                    from gird in Db.Query<biz_project_bridge>().Where(a => a.Status != StatusDelete && a.ProjID == proj.ID)
                    from theo in Db.Query<biz_steel_beam_theoretical>().Where(a => a.Status != StatusDelete && a.ProjID == proj.ID && a.GirderID == gird.ID)
                    where gird.ID == girderId
                    select theo.SegmentNo;
        var segmentNos = await query.Distinct().OrderBy(a => a).ToListAsync();
        var result = new List<string>();
        foreach (var seg in segmentNos)
        {
            if (!measuredSegmentNo.Contains(seg))
            {
                result.Add(seg);
            }
        }
        return result;
    }

    public async Task<CalcResultDTO> GetCalcResultBySegment(int girderId, List<string> segmentNos)
    {
        // 获取项目信息
        var projQuery = from gird in Db.Query<biz_project_bridge>().Where(a => a.ID == girderId)
                        from proj in Db.Query<biz_project>().Where(a => a.ID == gird.ProjID)
                        select proj;
        var projInfo = projQuery.FirstOrDefault();
        if (projInfo == null)
        {
            throw new Exception(BizMessages.ProjectInfoNotFound(Lang));
        }
        // 完整点集 Q：只取最大顶推次数的最新批次（实测 ∪ 推算），必须是同一批次同一姿态下的坐标，
        // 否则顶推1次之后会把 push0 的过期实测混进来（原实现按 PushCount 分组取最大 MeasureTime，正是这个问题）
        var completeSet = await LoadLatestCompleteSetAsync(girderId);
        // 已拼装完成点位的实测值
        var measuredList = new List<List<decimal>>() { new List<decimal>(), new List<decimal>(), new List<decimal>() };
        // 已拼装点位的设计值（理论值）
        var measuredDesignList = new List<List<decimal>>() { new List<decimal>(), new List<decimal>(), new List<decimal>() };
        // 已拼装点位的权重矩阵
        var measuredWeightList = new List<List<decimal>>() { new List<decimal>(), new List<decimal>(), new List<decimal>() };
        // 待拼装的设计值（理论值）
        var targetDesignList = new List<List<decimal>>() { new List<decimal>(), new List<decimal>(), new List<decimal>() };
        // 获取所选梁段的理论数据
        var measuredSegments = new List<string>();
        foreach (var itm in completeSet)
        {
            if (!measuredSegments.Contains(itm.Theory.SegmentNo)) measuredSegments.Add(itm.Theory.SegmentNo);
            // 已拼装完成点位的坐标（实测优先，缺失时取推算值）
            measuredList[0].Add(itm.X);
            measuredList[1].Add(itm.Y);
            measuredList[2].Add(itm.Z);
            // 已拼装点位的设计值（理论值）
            measuredDesignList[0].Add(itm.Theory.DesignX);
            measuredDesignList[1].Add(itm.Theory.DesignY);
            measuredDesignList[2].Add(itm.Theory.DesignZ);
            // 已拼装点位的权重矩阵（权重只需要已完成拼装的点位，因为点位的权重值只有一个，因此拼接的时候，X/Y/Z的权重都用同一个点位的Weight值）
            measuredWeightList[0].Add(itm.Theory.Weight);
            measuredWeightList[1].Add(itm.Theory.Weight);
            measuredWeightList[2].Add(itm.Theory.Weight);
        }
        var queryTarget = from ther in Db.Query<biz_steel_beam_theoretical>().Where(a => a.Status != StatusDelete)
                          where ther.GirderID == girderId
                                && segmentNos.Contains(ther.SegmentNo)
                          select ther;
        var listTarget = queryTarget.ToList();
        foreach (var itm in listTarget)
        {
            // 待拼装的设计值（理论值）
            targetDesignList[0].Add(itm.DesignX);
            targetDesignList[1].Add(itm.DesignY);
            targetDesignList[2].Add(itm.DesignZ);
        }
        // 执行计算，得到所选梁段的【待拼装数据】
        var gescha = SteelGirderAlignment.Gescha(P: measuredDesignList, Q: measuredList, D: targetDesignList, R: measuredWeightList);
        var result = new CalcResultDTO();
        result.ProjName = projInfo.Name;
        result.SheetNo = "请填写";
        result.MeasuredSegments = string.Join(",", measuredSegments.Distinct());
        result.PendingSegments = string.Join(",", segmentNos);
        result.Rows = new List<CalcResultRowDTO>();
        for (var i = 0; i < listTarget.Count; i++)
        {
            var targ = listTarget[i];
            var item = new CalcResultRowDTO();
            item.PositionName = targ.PositionName;
            item.PointCode = targ.PointCode;
            item.DesignX = gescha[0][i];
            item.DesignY = gescha[1][i];
            item.DesignZ = gescha[2][i];
            item.DesignX = Math.Round(item.DesignX, 4, MidpointRounding.AwayFromZero);
            item.DesignY = Math.Round(item.DesignY, 4, MidpointRounding.AwayFromZero);
            item.DesignZ = Math.Round(item.DesignZ, 4, MidpointRounding.AwayFromZero);
            result.Rows.Add(item);
        }

        return result;
    }

    /// <summary>
    /// 重算本钢梁全部顶推批次的推算坐标。幂等：按顶推次数升序逐批覆盖写。
    /// 用途：①新表上线后回填历史批次；②人工修改实测数据后刷新推算值。
    /// </summary>
    /// <param name="girderID">梁ID</param>
    /// <returns>双语结果描述（重算批次数与条数）</returns>
    public async Task<string> RebuildDerived(int girderID)
    {
        var context = await EnsureGirderAccess(girderID);
        var account = GetCurrentAccount();
        var timestamp = DateTime.Now;
        var batches = await Db.Query<biz_steel_beam_measure_batch>()
            .Where(a => a.GirderID == girderID && a.Status != StatusDelete)
            .ToListAsync();
        var maxPush = batches.Count == 0 ? -1 : batches.Max(a => a.PushCount);
        if (maxPush < 1) throw new InvalidOperationException(BizMessages.SteelDerivedNothingToRebuild(Lang));
        var latestByPush = batches
            .GroupBy(a => a.PushCount)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.MeasureTime).ThenByDescending(a => a.ID).First());
        var batchCount = 0;
        var pointCount = 0;
        Db.BeginTransaction();
        try
        {
            // 必须按顶推次数升序：第 k 批依赖第 k-1 批的完整集
            for (var k = 1; k <= maxPush; k++)
            {
                if (!latestByPush.TryGetValue(k, out var batch)) continue;
                pointCount += await DeriveForBatchAsync(context.ProjID, girderID, batch.ID, k, account, timestamp);
                batchCount++;
            }
            await Db.SaveAsync();
            Db.CommitTransaction();
        }
        catch
        {
            Db.RollbackTransaction();
            throw;
        }
        return BizMessages.SteelDerivedRebuildSuccess(Lang, batchCount, pointCount);
    }

    /// <summary>
    /// 推算某一个批次的「已推出胎架梁段」坐标并落库（覆盖写）。
    /// 推算失败（公共点不足 3 个、上一批次缺失）时抛错，由调用方回滚事务。
    /// </summary>
    private async Task<int> DeriveForBatchAsync(int projID, int girderID, int currBatchID, int pushCount, string account, DateTime timestamp)
    {
        var prevBatch = await Db.Query<biz_steel_beam_measure_batch>()
            .Where(a => a.GirderID == girderID && a.Status != StatusDelete && a.PushCount == pushCount - 1)
            .OrderByDescending(a => a.MeasureTime).ThenByDescending(a => a.ID)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException(BizMessages.SteelDerivedPrevBatchMissing(Lang, pushCount));
        var prevMeasured = await LoadMeasuredPointsAsync(prevBatch.ID);
        var prevDerived = await LoadDerivedPointsAsync(prevBatch.ID);
        var prevFullSet = MergeCompleteSet(prevMeasured, prevDerived);
        var currMeasured = await LoadMeasuredPointsAsync(currBatchID);
        var outcome = SteelBeamDerivedCalculator.Derive(prevFullSet, prevMeasured, currMeasured, pushCount, Lang);
        if (!outcome.Success) throw new InvalidOperationException(outcome.ErrorMessage);
        return await WriteDerivedAsync(projID, girderID, currBatchID, pushCount, pushCount - 1, outcome.Rows, account, timestamp);
    }

    private async Task<int> WriteDerivedAsync(int projID, int girderID, int batchID, int pushCount, int sourcePushCount,
        List<SteelBeamDerivedCalculator.Point> rows, string account, DateTime timestamp)
    {
        var existing = await Db.Query<biz_steel_beam_derived_coordinate>()
            .Where(a => a.BatchID == batchID && a.Status != StatusDelete).ToListAsync();
        foreach (var old in existing) { old.Status = -1; old.UpdatedBy = account; old.UpdatedTime = timestamp; }
        var entities = rows.Select(a => new biz_steel_beam_derived_coordinate
        {
            ProjID = projID,
            GirderID = girderID,
            BatchID = batchID,
            PushCount = pushCount,
            SourcePushCount = sourcePushCount,
            TheoreticalID = a.TheoreticalID,
            PointCode = a.PointCode,
            SegmentNo = a.SegmentNo,
            DerivedX = a.X,
            DerivedY = a.Y,
            DerivedZ = a.Z,
            Version = 1,
            Status = 0,
            CreatedBy = account,
            CreatedTime = timestamp
        }).ToList();
        if (entities.Count > 0) Db.GetRepository<biz_steel_beam_derived_coordinate>().AddList(entities);
        await Db.SaveAsync();
        return entities.Count;
    }

    private async Task<List<SteelBeamDerivedCalculator.Point>> LoadMeasuredPointsAsync(int batchID)
    {
        var rows = await (from m in Db.Query<biz_steel_beam_measured>().Where(a => a.BatchID == batchID && a.Status != StatusDelete)
                          join t in Db.Query<biz_steel_beam_theoretical>().Where(a => a.Status != StatusDelete) on m.TheoreticalID equals t.ID
                          select new { m.TheoreticalID, m.PointCode, m.MeasuredX, m.MeasuredY, m.MeasuredZ, t.SegmentNo, t.Weight })
                          .ToListAsync();
        return rows.Select(a => new SteelBeamDerivedCalculator.Point(
            a.TheoreticalID, a.PointCode, a.SegmentNo, a.MeasuredX, a.MeasuredY, a.MeasuredZ, a.Weight)).ToList();
    }

    private async Task<List<SteelBeamDerivedCalculator.Point>> LoadDerivedPointsAsync(int batchID)
    {
        var rows = await (from d in Db.Query<biz_steel_beam_derived_coordinate>().Where(a => a.BatchID == batchID && a.Status != StatusDelete)
                          join t in Db.Query<biz_steel_beam_theoretical>().Where(a => a.Status != StatusDelete) on d.TheoreticalID equals t.ID
                          select new { d.TheoreticalID, d.PointCode, d.SegmentNo, d.DerivedX, d.DerivedY, d.DerivedZ, t.Weight })
                          .ToListAsync();
        return rows.Select(a => new SteelBeamDerivedCalculator.Point(
            a.TheoreticalID, a.PointCode, a.SegmentNo, a.DerivedX, a.DerivedY, a.DerivedZ, a.Weight)).ToList();
    }

    /// <summary>合并一个批次的完整点集：实测优先，缺失的点用推算值补齐。</summary>
    private static List<SteelBeamDerivedCalculator.Point> MergeCompleteSet(
        IEnumerable<SteelBeamDerivedCalculator.Point> measured, IEnumerable<SteelBeamDerivedCalculator.Point> derived)
    {
        var result = new List<SteelBeamDerivedCalculator.Point>();
        var ids = new HashSet<int>();
        foreach (var p in measured) if (ids.Add(p.TheoreticalID)) result.Add(p);
        foreach (var p in derived) if (ids.Add(p.TheoreticalID)) result.Add(p);
        return result;
    }

    /// <summary>
    /// 取「最大顶推次数的最新批次」的完整点集（实测 ∪ 推算），作为待拼装计算的 Q。
    /// Q 必须是同一批次、同一姿态下的坐标，不能跨批次混用。
    /// </summary>
    private async Task<List<(biz_steel_beam_theoretical Theory, decimal X, decimal Y, decimal Z)>> LoadLatestCompleteSetAsync(int girderID)
    {
        var latestBatch = await Db.Query<biz_steel_beam_measure_batch>()
            .Where(a => a.GirderID == girderID && a.Status != StatusDelete)
            .OrderByDescending(a => a.PushCount).ThenByDescending(a => a.MeasureTime).ThenByDescending(a => a.ID)
            .FirstOrDefaultAsync();
        var result = new List<(biz_steel_beam_theoretical Theory, decimal X, decimal Y, decimal Z)>();
        if (latestBatch == null) return result;
        var ids = new HashSet<int>();
        var measured = await (from m in Db.Query<biz_steel_beam_measured>().Where(a => a.BatchID == latestBatch.ID && a.Status != StatusDelete)
                              join t in Db.Query<biz_steel_beam_theoretical>().Where(a => a.Status != StatusDelete) on m.TheoreticalID equals t.ID
                              select new { m, t }).ToListAsync();
        foreach (var a in measured)
        {
            if (!ids.Add(a.m.TheoreticalID)) continue;
            result.Add((a.t, a.m.MeasuredX, a.m.MeasuredY, a.m.MeasuredZ));
        }
        var derived = await (from d in Db.Query<biz_steel_beam_derived_coordinate>().Where(a => a.BatchID == latestBatch.ID && a.Status != StatusDelete)
                             join t in Db.Query<biz_steel_beam_theoretical>().Where(a => a.Status != StatusDelete) on d.TheoreticalID equals t.ID
                             select new { d, t }).ToListAsync();
        foreach (var a in derived)
        {
            if (!ids.Add(a.d.TheoreticalID)) continue;
            result.Add((a.t, a.d.DerivedX, a.d.DerivedY, a.d.DerivedZ));
        }
        return result;
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
    /// 计算梁段Z向线形偏差（刚体Procrustes/Gescha，scale固定=1.0）
    /// 业务逻辑说明：
    /// 1. 首节PushCount=0：基准预制段，Z偏差=胎架实测Z - 完工设计Z；
    ///    Gescha刚体配准使用首段全部测点求解全局姿态变换矩阵T，供给后续节段预测。
    /// 2. 后续节段PushCount>0：复用首段得到的刚体变换T，将胎架实测坐标预测为拼装完工大地坐标；
    ///    Z偏差=Gescha预测完工Z - 完工设计Z，该偏差包含前段拼装姿态传递效应（线形累积偏差）。
    /// 3. 前提：假定梁为刚体；误差逐段累积；scale固定=1，不启用缩放。
    /// 4. positionName为**输出过滤条件**：刚体拟合使用全量测点，计算完成后，仅返回指定测位数据。
    /// </summary>
    /// <param name="girderId">梁ID</param>
    /// <param name="positionName">测位筛选，不传则全部（仅用于输出过滤，不参与刚体配准计算）</param>
    /// <param name="offsetType">偏差类型预留</param>
    /// <param name="sort">排序 asc/desc</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<CalcSteelBeamOffsetDTO> GetSteelBeamOffset(int girderId, string positionName = "", string offsetType = "", string sort = "asc")
    {
        var result = new CalcSteelBeamOffsetDTO();

        // 获取项目信息
        var projQuery = from gird in Db.Query<biz_project_bridge>().Where(a => a.ID == girderId)
                        from proj in Db.Query<biz_project>().Where(a => a.ID == gird.ProjID)
                        select new { proj, gird };
        var projInfo = projQuery.FirstOrDefault();
        if (projInfo == null)
        {
            throw new Exception($"{BizMessages.ProjectInfoNotFound(Lang)}，GirderId={girderId}");
        }
        // 子查询：按GirderID, PushCount分组，取每组最大MeasureTime
        var maxBatchSubQuery = Db.Query<biz_steel_beam_measure_batch>()
            .Where(a => a.Status != StatusDelete && a.GirderID == girderId)
            .GroupBy(b => new { b.GirderID, b.PushCount })
            .Select(g => new
            {
                g.Key.GirderID,
                g.Key.PushCount,
                MaxMeasureTime = g.Max(x => x.MeasureTime)
            });

        // 主查询：关联batch、measured（胎架实测）、theoretical（完工设计坐标）
        var query = from puch in Db.Query<biz_steel_beam_measure_batch>()
                    from sub in maxBatchSubQuery.Where(a => a.MaxMeasureTime == puch.MeasureTime
                            && a.PushCount == puch.PushCount && a.GirderID == puch.GirderID)
                    from meas in Db.Query<biz_steel_beam_measured>().Where(a => a.BatchID == puch.ID && a.Status != StatusDelete)
                    from ther in Db.Query<biz_steel_beam_theoretical>().Where(a => a.ID == meas.TheoreticalID && a.Status != StatusDelete)
                    select new
                    {
                        puch.PushCount,
                        puch.ImportCount,
                        meas.MeasuredX,
                        meas.MeasuredY,
                        meas.MeasuredZ,
                        ther.PointCode,
                        ther.PositionName, // 测位
                        ther.DistanceFromStart, // 到首个点位距离
                        ther.PositionOrder, // 测位内顺序
                        ther.SegmentNo,     // 梁段编号
                        ther.DesignX,
                        ther.DesignY,
                        ther.DesignZ,
                        ther.Weight
                    };

        var rawList = query.ToList();
        if (!rawList.Any())
        {
            return result;
        }

        // =========【重点改动】positionName过滤移到计算完成之后，这里不再提前过滤！=========

        // 按PushCount排序（顶推批次，PushCount=0为首段）
        var pushGroups = rawList.GroupBy(x => x.PushCount)
            .OrderBy(g => sort.Equals("desc", StringComparison.OrdinalIgnoreCase) ? g.Key * -1 : g.Key)
            .ToList();

        if (!pushGroups.Any())
            return result;

        // ========== 【1.首段PushCount=0，构建理论点对，求刚体变换T（使用首段全部测点，不受positionName限制） ==========
        var firstPushGroup = pushGroups.FirstOrDefault(g => g.Key == 0);
        var firstPoints = firstPushGroup.ToList();

        // P:3×N 目标：完工设计大地坐标 (理论)
        // Q:3×N 源：胎架实测坐标
        int nFirst = firstPoints.Count;
        if (nFirst < 3)
            throw new Exception(BizMessages.FirstSegmentTooFewPoints(Lang, projInfo.gird.Name));

        // P 3行N列：完工设计坐标
        decimal[,] P = new decimal[3, nFirst];
        // Q 3行N列：胎架实测坐标（首段构建T的源点）
        decimal[,] Q = new decimal[3, nFirst];
        // 权重矩阵R：1行N列逐点权重
        decimal[,] R = new decimal[1, nFirst];

        for (int i = 0; i < nFirst; i++)
        {
            var p = firstPoints[i];
            // P：装配完成设计大地坐标
            P[0, i] = p.DesignX;
            P[1, i] = p.DesignY;
            P[2, i] = p.DesignZ;

            // Q：胎架上实测坐标
            Q[0, i] = p.MeasuredX;
            Q[1, i] = p.MeasuredY;
            Q[2, i] = p.MeasuredZ;

            R[0, i] = p.Weight;
        }

        // 回填首段的Z偏差：首段直接使用胎架原始实测值
        for (int i = 0; i < nFirst; i++)
        {
            var row = new CalcSteelBeamOffsetRow
            {
                PushCount = 0,
                SegmentNo = firstPoints[i].SegmentNo,
                PointCode = firstPoints[i].PointCode,
                PointDistance = firstPoints[i].DistanceFromStart,
                PositionName = firstPoints[i].PositionName,
                PositionOrder = firstPoints[i].PositionOrder,
                X = firstPoints[i].DesignX,
                Y = firstPoints[i].DesignY,
                Z = firstPoints[i].DesignZ,
                PredictZ = firstPoints[i].MeasuredZ,
                ZError = firstPoints[i].MeasuredZ - firstPoints[i].DesignZ
            };
            result.Rows.Add(row);
        }

        // ==========【2.后续所有PushCount>0 的梁段，复用上面得到的变换T ==========
        var laterPushGroups = pushGroups.Where(g => g.Key > 0).ToList();
        foreach (var pushGroup in laterPushGroups)
        {
            var curPoints = pushGroup.ToList();
            int m = curPoints.Count;
            if (m < 1) continue;

            // D: 当前这批胎架实测坐标，3×M，待变换
            decimal[,] D = new decimal[3, m];
            for (int i = 0; i < m; i++)
            {
                var pt = curPoints[i];
                D[0, i] = pt.MeasuredX;
                D[1, i] = pt.MeasuredY;
                D[2, i] = pt.MeasuredZ;
            }

            // 复用首段的P,Q,R，scale=1，得到当前段预测完工坐标
            var geschaResult = SteelGirderAlignment.Gescha(P, Q, D, R, 1.0m);

            // 计算Z偏差
            for (int i = 0; i < m; i++)
            {
                var pt = curPoints[i];
                var row = new CalcSteelBeamOffsetRow
                {
                    PushCount = pushGroup.Key,
                    SegmentNo = pt.SegmentNo,
                    PointCode = pt.PointCode,
                    PositionName = pt.PositionName,
                    X = pt.DesignX,
                    Y = pt.DesignY,
                    Z = pt.DesignZ,
                    PredictZ = geschaResult[2, i],
                    ZError = geschaResult[2, i] - pt.DesignZ
                };
                result.Rows.Add(row);
            }
        }

        // =========【后置过滤：全部计算完成之后，才按positionName筛选输出】=========
        if (!string.IsNullOrWhiteSpace(positionName))
        {
            result.Rows = result.Rows.Where(x => x.PositionName == positionName).ToList();
        }

        // 按测位、梁段排序
        if (sort.Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            result.Rows = result.Rows.OrderByDescending(r => r.PositionName)
                .ThenByDescending(r => r.PushCount)
                .ThenByDescending(a => a.PointDistance).ToList();
        }
        else
        {
            result.Rows = result.Rows.OrderBy(r => r.PositionName)
                .ThenBy(r => r.PushCount)
                .ThenBy(a => a.PointDistance).ToList();
        }

        return result;
    }


}
