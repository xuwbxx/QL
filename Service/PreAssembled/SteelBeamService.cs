using ClosedXML.Excel;
using DataFactory.Factory;
using DataFactory.KingBase;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Model.tech.QL.DTO.BizProject;
using System.Security.Claims;

namespace Service.PreAssembled;

public class SteelBeamService : ServiceBase<biz_project_bridge>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public SteelBeamService(
        QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration) : base(qlUowFactory)
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
                        ID = bridge.ID, ProjID = bridge.ProjID, ProjectName = proj.Name,
                        BridgeName = bridge.Name, BeamType = bridge.BeamType
                    };
        if (allowed != null) query = query.Where(a => allowed.Contains(a.ProjID));
        if (req.ProjID is > 0) query = query.Where(a => a.ProjID == req.ProjID);
        if (req.BridgeID is > 0) query = query.Where(a => a.ID == req.BridgeID);
        var total = await query.CountAsync();
        var page = Math.Max(req.PageIndex, 1);
        var size = NormalizePageSize(req.PageSize);
        var list = await query.OrderBy(a => a.ProjectName).ThenBy(a => a.BridgeName)
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

    public async Task<object> GetBridgeInfo(int bridgeID)
    {
        var context = await EnsureBridgeAccess(bridgeID);
        return new { projectName = context.ProjectName, bridgeName = context.BridgeName, projID = context.ProjID, beamType = 0 };
    }

    public async Task<SteelBeamImportStateDTO> GetImportState(int bridgeID)
    {
        await EnsureBridgeAccess(bridgeID);
        var hasTheory = await Db.Query<biz_steel_beam_theoretical>().AnyAsync(a => a.BridgeID == bridgeID && a.Status != -1);
        var activeBatches = Db.Query<biz_steel_beam_measure_batch>().Where(a => a.BridgeID == bridgeID && a.Status != -1);
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
        await EnsureBridgeAccess(req.BridgeID);
        var locked = await Db.Query<biz_steel_beam_measure_batch>().AnyAsync(a => a.BridgeID == req.BridgeID && a.Status != -1);
        var query = Db.Query<biz_steel_beam_theoretical>().Where(a => a.BridgeID == req.BridgeID && a.Status != -1);
        if (!string.IsNullOrWhiteSpace(req.PointCode)) query = query.Where(a => a.PointCode == req.PointCode);
        if (!string.IsNullOrWhiteSpace(req.SegmentNo)) query = query.Where(a => a.SegmentNo == req.SegmentNo);
        var total = await query.CountAsync();
        var page = Math.Max(req.PageIndex, 1);
        var size = NormalizePageSize(req.PageSize);
        var rows = await query.OrderBy(a => a.PointCode.ToLower()).Skip((page - 1) * size).Take(size)
            .Select(a => new SteelBeamTheoreticalItemDTO
            {
                ID = a.ID, PointCode = a.PointCode, DesignX = a.DesignX, DesignY = a.DesignY, DesignZ = a.DesignZ,
                PreCamber = a.PreCamber, Weight = a.Weight, SegmentNo = a.SegmentNo,
                PositionName = a.PositionName, Version = a.Version, CanEdit = !locked
            }).ToListAsync();
        return new() { List = rows, Total = total };
    }

    public async Task<object> TheoreticalOptions(int bridgeID)
    {
        await EnsureBridgeAccess(bridgeID);
        var query = Db.Query<biz_steel_beam_theoretical>().Where(a => a.BridgeID == bridgeID && a.Status != -1);
        var points = await query.Select(a => a.PointCode).Distinct().OrderBy(a => a.ToLower()).ToListAsync();
        var segments = await query.Select(a => a.SegmentNo).Distinct().OrderBy(a => a).ToListAsync();
        return new { points, segments };
    }

    public async Task<SteelBeamPagedResultDTO<SteelBeamMeasuredItemDTO>> MeasuredList(SteelBeamMeasuredQueryDTO req)
    {
        await EnsureBridgeAccess(req.BridgeID);
        var batches = Db.Query<biz_steel_beam_measure_batch>().Where(a => a.BridgeID == req.BridgeID && a.Status != -1);
        var maxPush = await batches.AnyAsync() ? await batches.MaxAsync(a => a.PushCount) : -1;
        var query = from measured in Db.Query<biz_steel_beam_measured>().Where(a => a.BridgeID == req.BridgeID && a.Status != -1)
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
                ID = a.measured.ID, PointCode = a.measured.PointCode,
                MeasuredX = a.measured.MeasuredX, MeasuredY = a.measured.MeasuredY, MeasuredZ = a.measured.MeasuredZ,
                PushCount = a.batch.PushCount, MeasureTime = a.batch.MeasureTime, ImportTime = a.measured.ImportTime,
                Version = a.measured.Version, CanEdit = a.batch.PushCount == maxPush
            }).ToListAsync();
        var ordered = data.OrderBy(a => a.PointCode, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(a => a.ImportTime).ThenByDescending(a => a.ID).ToList();
        foreach (var group in ordered.GroupBy(a => a.PointCode, StringComparer.OrdinalIgnoreCase))
            group.First().PointRowSpan = group.Count();
        return new() { List = ordered, Total = total };
    }

    public async Task<List<DateTime>> MeasureTimeOptions(int bridgeID, int pushCount)
    {
        await EnsureBridgeAccess(bridgeID);
        return await Db.Query<biz_steel_beam_measure_batch>()
            .Where(a => a.BridgeID == bridgeID && a.Status != -1 && a.PushCount == pushCount)
            .Select(a => a.MeasureTime).Distinct().OrderByDescending(a => a).ToListAsync();
    }

    public async Task<SteelBeamTheoreticalItemDTO> UpdateTheoretical(SteelBeamCoordinateUpdateDTO req)
    {
        await EnsureBridgeAccess(req.BridgeID);
        ValidateCoordinate(req.X, req.Y, req.Z);
        if (await Db.Query<biz_steel_beam_measure_batch>().AnyAsync(a => a.BridgeID == req.BridgeID && a.Status != -1))
            throw new InvalidOperationException("已存在实测数据，理论数据不可修改");
        var row = await Db.Query<biz_steel_beam_theoretical>()
            .FirstOrDefaultAsync(a => a.ID == req.ID && a.BridgeID == req.BridgeID && a.Status != -1 && a.Version == req.Version)
            ?? throw new InvalidOperationException("数据已更新，请刷新后重试");
        row.DesignX = req.X; row.DesignY = req.Y; row.DesignZ = req.Z;
        row.Version++; row.UpdatedBy = GetCurrentAccount(); row.UpdatedTime = DateTime.Now;
        await Db.SaveAsync();
        return new()
        {
            ID = row.ID, PointCode = row.PointCode, DesignX = row.DesignX, DesignY = row.DesignY, DesignZ = row.DesignZ,
            PreCamber = row.PreCamber, Weight = row.Weight, SegmentNo = row.SegmentNo,
            PositionName = row.PositionName, Version = row.Version, CanEdit = true
        };
    }

    public async Task<SteelBeamMeasuredItemDTO> UpdateMeasured(SteelBeamCoordinateUpdateDTO req)
    {
        await EnsureBridgeAccess(req.BridgeID);
        ValidateCoordinate(req.X, req.Y, req.Z);
        var maxPush = await Db.Query<biz_steel_beam_measure_batch>()
            .Where(a => a.BridgeID == req.BridgeID && a.Status != -1).MaxAsync(a => a.PushCount);
        var row = await (from measured in Db.Query<biz_steel_beam_measured>()
                         join batch in Db.Query<biz_steel_beam_measure_batch>().Where(a => a.Status != -1) on measured.BatchID equals batch.ID
                         where measured.ID == req.ID && measured.BridgeID == req.BridgeID && measured.Status != -1 && measured.Version == req.Version
                         select new { measured, batch }).FirstOrDefaultAsync()
                  ?? throw new InvalidOperationException("数据已更新，请刷新后重试");
        if (row.batch.PushCount != maxPush) throw new InvalidOperationException("非最大顶推次数的数据不可编辑");
        row.measured.MeasuredX = req.X; row.measured.MeasuredY = req.Y; row.measured.MeasuredZ = req.Z;
        row.measured.Version++; row.measured.UpdatedBy = GetCurrentAccount(); row.measured.UpdatedTime = DateTime.Now;
        await Db.SaveAsync();
        return new()
        {
            ID = row.measured.ID, PointCode = row.measured.PointCode,
            MeasuredX = row.measured.MeasuredX, MeasuredY = row.measured.MeasuredY, MeasuredZ = row.measured.MeasuredZ,
            PushCount = row.batch.PushCount, MeasureTime = row.batch.MeasureTime, ImportTime = row.measured.ImportTime,
            Version = row.measured.Version, CanEdit = true
        };
    }

    public async Task<SteelBeamImportOutcome> ImportTheoretical(int bridgeID, byte[] bytes, bool confirmOverwrite)
    {
        var context = await EnsureBridgeAccess(bridgeID);
        var timestamp = DateTime.Now;
        SteelBeamParseResult<SteelBeamTheoreticalImportRow> parsed;
        try { using var input = new MemoryStream(bytes); parsed = SteelBeamExcelHelper.ParseTheoretical(input); }
        catch (Exception ex) { return SaveFailure(context, "理论数据", null, null, timestamp, bytes, [new(0, "", "文件", "", $"Excel解析失败：{ex.Message}")]); }

        if (await Db.Query<biz_steel_beam_measure_batch>().AnyAsync(a => a.BridgeID == bridgeID && a.Status != -1))
            parsed.Errors.Add(new(0, "", "业务规则", "", "已存在实测数据，理论数据不可导入"));
        var hasExisting = await Db.Query<biz_steel_beam_theoretical>().AnyAsync(a => a.BridgeID == bridgeID && a.Status != -1);
        if (hasExisting && !confirmOverwrite) parsed.Errors.Add(new(0, "", "业务规则", "", "本次导入将覆盖当前全部理论数据，请确认后继续"));
        if (parsed.Errors.Count == 0)
        {
            var duplicateSet = (await Db.Query<biz_steel_beam_theoretical>()
                .Where(a => a.ProjID == context.ProjID && a.BridgeID != bridgeID && a.Status != -1)
                .Select(a => a.PointCode).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var row in parsed.SourcePoints.Where(a => duplicateSet.Contains(a.PointCode)))
                parsed.Errors.Add(new(row.ExcelRow, row.PointCode, "测点编号", row.PointCode, "测点编号已在同一项目的其他桥梁中使用"));
        }
        if (!parsed.Success) return SaveFailure(context, "理论数据", null, null, timestamp, bytes, parsed.Errors);

        var successPath = SaveOriginal(context, "理论数据", null, null, timestamp, bytes, false);
        try
        {
            Db.BeginTransaction();
            if (await Db.Query<biz_steel_beam_measure_batch>().AnyAsync(a => a.BridgeID == bridgeID && a.Status != -1))
                throw new InvalidOperationException("已存在实测数据，理论数据不可导入");
            var oldRows = await Db.Query<biz_steel_beam_theoretical>().Where(a => a.BridgeID == bridgeID && a.Status != -1).ToListAsync();
            if (oldRows.Count > 0 && !confirmOverwrite) throw new InvalidOperationException("理论数据已变化，请确认覆盖后重试");
            foreach (var old in oldRows) { old.Status = -1; old.UpdatedBy = GetCurrentAccount(); old.UpdatedTime = timestamp; }
            await Db.SaveAsync();
            var account = GetCurrentAccount();
            var entities = parsed.Rows.Select(a => new biz_steel_beam_theoretical
            {
                ProjID = context.ProjID, BridgeID = bridgeID, PointCode = a.PointCode,
                DesignX = a.DesignX, DesignY = a.DesignY, DesignZ = a.DesignZ, PreCamber = a.PreCamber,
                Weight = a.Weight, SegmentNo = a.SegmentNo, PositionName = a.PositionName,
                IsFirstCoordinate = null, PositionOrder = null, DistanceFromStart = null,
                Version = 1, Status = 0, CreatedBy = account, CreatedTime = timestamp
            }).ToList();
            Db.GetRepository<biz_steel_beam_theoretical>().AddList(entities);
            await Db.SaveAsync();
            Db.CommitTransaction();
            return new() { Success = true, ImportedCount = entities.Count, Message = $"成功导入{entities.Count}条理论数据" };
        }
        catch (Exception ex)
        {
            Db.RollbackTransaction();
            TryDelete(successPath);
            return SaveFailure(context, "理论数据", null, null, timestamp, bytes, [new(0, "", "数据库", "", $"保存失败：{ex.Message}")]);
        }
    }

    public async Task<SteelBeamImportOutcome> ImportMeasured(int bridgeID, int pushCount, DateTime measureTime, byte[] bytes, bool confirmOverwrite)
    {
        var context = await EnsureBridgeAccess(bridgeID);
        var timestamp = DateTime.Now;
        measureTime = new DateTime(measureTime.Year, measureTime.Month, measureTime.Day, measureTime.Hour, 0, 0, DateTimeKind.Unspecified);
        SteelBeamParseResult<SteelBeamMeasuredImportRow> parsed;
        try { using var input = new MemoryStream(bytes); parsed = SteelBeamExcelHelper.ParseMeasured(input); }
        catch (Exception ex) { return SaveFailure(context, "实测数据", pushCount, measureTime, timestamp, bytes, [new(0, "", "文件", "", $"Excel解析失败：{ex.Message}")]); }

        var theory = await Db.Query<biz_steel_beam_theoretical>()
            .Where(a => a.BridgeID == bridgeID && a.Status != -1).ToListAsync();
        var theoryMap = theory.ToDictionary(a => a.PointCode, StringComparer.OrdinalIgnoreCase);
        if (theory.Count == 0) parsed.Errors.Add(new(0, "", "业务规则", "", "请先导入理论数据"));
        foreach (var row in parsed.SourcePoints.Where(a => !string.IsNullOrEmpty(a.PointCode) && !theoryMap.ContainsKey(a.PointCode)))
            parsed.Errors.Add(new(row.ExcelRow, row.PointCode, "测点编号", row.PointCode, "测点编号不存在于当前桥梁理论数据"));
        var activeBatches = Db.Query<biz_steel_beam_measure_batch>().Where(a => a.BridgeID == bridgeID && a.Status != -1);
        var hasBatch = await activeBatches.AnyAsync();
        var maxPush = hasBatch ? await activeBatches.MaxAsync(a => a.PushCount) : (int?)null;
        var allowedPushes = maxPush.HasValue ? new[] { maxPush.Value, maxPush.Value + 1 } : new[] { 0 };
        if (!allowedPushes.Contains(pushCount)) parsed.Errors.Add(new(0, "", "顶推次数", pushCount.ToString(), "顶推次数只能选择当前最大次数或最大次数加一"));
        var currentHour = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0);
        if (measureTime > currentHour) parsed.Errors.Add(new(0, "", "测量时间", measureTime.ToString("yyyy-MM-dd HH:00"), "测量时间不能晚于当前小时"));
        var oldBatch = await activeBatches.FirstOrDefaultAsync(a => a.PushCount == pushCount && a.MeasureTime == measureTime);
        if (oldBatch != null && !confirmOverwrite) parsed.Errors.Add(new(0, "", "业务规则", "", "本次导入将覆盖原数据，请确认后继续"));
        if (!parsed.Success) return SaveFailure(context, "实测数据", pushCount, measureTime, timestamp, bytes, parsed.Errors);

        var successPath = SaveOriginal(context, "实测数据", pushCount, measureTime, timestamp, bytes, false);
        try
        {
            Db.BeginTransaction();
            var account = GetCurrentAccount();
            var currentBatches = Db.Query<biz_steel_beam_measure_batch>().Where(a => a.BridgeID == bridgeID && a.Status != -1);
            var currentHasBatch = await currentBatches.AnyAsync();
            var currentMaxPush = currentHasBatch ? await currentBatches.MaxAsync(a => a.PushCount) : (int?)null;
            var currentAllowed = currentMaxPush.HasValue ? new[] { currentMaxPush.Value, currentMaxPush.Value + 1 } : new[] { 0 };
            if (!currentAllowed.Contains(pushCount)) throw new InvalidOperationException("顶推次数已变化，请刷新后重试");
            oldBatch = await currentBatches.FirstOrDefaultAsync(a => a.PushCount == pushCount && a.MeasureTime == measureTime);
            if (oldBatch != null && !confirmOverwrite) throw new InvalidOperationException("实测数据已变化，请确认覆盖后重试");
            List<biz_steel_beam_measured> oldRows = [];
            if (oldBatch != null)
            {
                oldBatch.Status = -1; oldBatch.UpdatedBy = account; oldBatch.UpdatedTime = timestamp;
                oldRows = await Db.Query<biz_steel_beam_measured>().Where(a => a.BatchID == oldBatch.ID && a.Status != -1).ToListAsync();
                foreach (var old in oldRows) { old.Status = -1; old.UpdatedBy = account; old.UpdatedTime = timestamp; }
                await Db.SaveAsync();
            }
            var batch = new biz_steel_beam_measure_batch
            {
                ProjID = context.ProjID, BridgeID = bridgeID, PushCount = pushCount, MeasureTime = measureTime,
                ImportCount = parsed.Rows.Count, Status = 0, CreatedBy = account, CreatedTime = timestamp
            };
            Db.GetRepository<biz_steel_beam_measure_batch>().Add(batch);
            await Db.SaveAsync();
            if (oldBatch != null) oldBatch.ReplacedByBatchID = batch.ID;
            var rows = parsed.Rows.Select(a =>
            {
                var point = theoryMap[a.PointCode];
                return new biz_steel_beam_measured
                {
                    ProjID = context.ProjID, BridgeID = bridgeID, BatchID = batch.ID, TheoreticalID = point.ID,
                    PointCode = point.PointCode, MeasuredX = a.MeasuredX, MeasuredY = a.MeasuredY, MeasuredZ = a.MeasuredZ,
                    ImportTime = timestamp, Version = 1, Status = 0, CreatedBy = account, CreatedTime = timestamp
                };
            }).ToList();
            Db.GetRepository<biz_steel_beam_measured>().AddList(rows);
            await Db.SaveAsync();
            Db.CommitTransaction();
            return new() { Success = true, ImportedCount = rows.Count, Message = $"成功导入{rows.Count}条实测数据" };
        }
        catch (Exception ex)
        {
            Db.RollbackTransaction();
            TryDelete(successPath);
            return SaveFailure(context, "实测数据", pushCount, measureTime, timestamp, bytes, [new(0, "", "数据库", "", $"保存失败：{ex.Message}")]);
        }
    }

    public byte[] TheoreticalTemplate() => SteelBeamExcelHelper.CreateTheoreticalTemplate();
    public byte[] MeasuredTemplate() => SteelBeamExcelHelper.CreateMeasuredTemplate();
    public void EnsureAuthenticated() => _ = GetCurrentAccount();

    public async Task<(byte[] Bytes, string FileName)> ExportMeasured(SteelBeamMeasuredQueryDTO req)
    {
        var context = await EnsureBridgeAccess(req.BridgeID);
        var query = from measured in Db.Query<biz_steel_beam_measured>().Where(a => a.BridgeID == req.BridgeID && a.Status != -1)
                    join batch in Db.Query<biz_steel_beam_measure_batch>().Where(a => a.Status != -1) on measured.BatchID equals batch.ID
                    select new { measured, batch };
        if (req.PushCount.HasValue) query = query.Where(a => a.batch.PushCount == req.PushCount.Value);
        if (req.MeasureTime.HasValue) query = query.Where(a => a.batch.MeasureTime == req.MeasureTime.Value);
        var data = await query.Select(a => new
        {
            a.measured.PointCode, a.measured.MeasuredX, a.measured.MeasuredY, a.measured.MeasuredZ,
            a.batch.PushCount, a.batch.MeasureTime
        }).ToListAsync();
        if (data.Count == 0) throw new InvalidOperationException("当前筛选条件下无可下载数据");
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
        var name = $"{SteelBeamExcelHelper.SanitizePathPart(context.BridgeName)}_实测数据_{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
        return (output.ToArray(), name);
    }

    private SteelBeamImportOutcome SaveFailure(BridgeContext context, string type, int? pushCount, DateTime? measureTime,
        DateTime timestamp, byte[] original, IEnumerable<SteelBeamImportError> errors)
    {
        var errorList = errors.ToList();
        var errorBytes = SteelBeamExcelHelper.CreateErrorFile(errorList);
        var originalPath = SaveOriginal(context, type, pushCount, measureTime, timestamp, original, true);
        var baseName = Path.GetFileNameWithoutExtension(originalPath).Replace("_失败", string.Empty, StringComparison.Ordinal);
        var errorName = $"{baseName}_错误明细.xlsx";
        File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(originalPath)!, errorName), errorBytes);
        return new() { Success = false, Message = "导入失败，请查看错误明细文件", ErrorFile = errorBytes, ErrorFileName = errorName };
    }

    private string SaveOriginal(BridgeContext context, string type, int? pushCount, DateTime? measureTime, DateTime timestamp, byte[] bytes, bool failed)
    {
        var root = _configuration["SteelBeam:FileRoot"];
        if (string.IsNullOrWhiteSpace(root)) root = Path.Combine(AppContext.BaseDirectory, "App_Data");
        var project = SteelBeamExcelHelper.SanitizePathPart(context.ProjectName);
        var bridge = SteelBeamExcelHelper.SanitizePathPart(context.BridgeName);
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
        var account = GetCurrentAccount();
        var user = await Db.Query<sys_userinfo>().FirstOrDefaultAsync(a => a.Status != -1 && a.Account == account)
                   ?? throw new UnauthorizedAccessException("当前登录用户不存在");
        var isAdmin = await (from ur in Db.Query<sys_user_role>().Where(a => a.Status != -1 && a.UserID == user.ID)
                             join role in Db.Query<sys_role>().Where(a => a.Status != -1 && a.Code == "ADMIN") on ur.RoleID equals role.ID
                             select role.ID).AnyAsync();
        if (isAdmin) return null;
        return (await Db.Query<biz_project_user>().Where(a => a.Status != -1 && a.UserID == user.ID)
            .Select(a => a.ProjID).Distinct().ToListAsync()).ToHashSet();
    }

    private async Task EnsureProjectAccess(int projID)
    {
        var allowed = await GetAllowedProjectIds();
        if (allowed != null && !allowed.Contains(projID)) throw new UnauthorizedAccessException("无该项目的数据权限");
    }

    private async Task<BridgeContext> EnsureBridgeAccess(int bridgeID)
    {
        var bridge = await (from b in Db.Query<biz_project_bridge>().Where(a => a.Status != -1 && a.BeamType == 0 && a.ID == bridgeID)
                            join p in Db.Query<biz_project>().Where(a => a.Status != -1) on b.ProjID equals p.ID
                            select new BridgeContext(b.ProjID, p.Name, b.Name)).FirstOrDefaultAsync()
                     ?? throw new InvalidOperationException("钢梁不存在或已失效");
        await EnsureProjectAccess(bridge.ProjID);
        return bridge;
    }

    private string GetCurrentAccount()
    {
        var account = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(account)) throw new UnauthorizedAccessException("未获取到当前登录用户");
        return account;
    }

    private static void ValidateCoordinate(decimal x, decimal y, decimal z)
    {
        if (!SteelBeamExcelHelper.HasScale(x, 6) || !SteelBeamExcelHelper.HasScale(y, 6) || !SteelBeamExcelHelper.HasScale(z, 6))
            throw new InvalidOperationException("坐标最多保留6位小数");
    }

    private static int NormalizePageSize(int size) => size is 20 or 50 or 100 ? size : 20;
    private sealed record BridgeContext(int ProjID, string ProjectName, string BridgeName);
}
