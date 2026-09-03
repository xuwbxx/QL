using Business.Towing;
using DataFactory.Factory;
using Tool;

namespace Service.Towing
{
    public class MRUStatusInfo
    {
        public int DeviceID { set; get; }
        public decimal DangX { set; get; }
        public decimal DangXMax { set; get; }
        public decimal DangXAbs { set; get; }
        public decimal DangY { set; get; }
        public decimal DangYMax { set; get; }
        public decimal DangYAbs { set; get; }
        public decimal DangZ { set; get; }
        public decimal DangZMax { set; get; }
        public decimal DangZAbs { set; get; }
        public decimal YaoX { set; get; }
        public decimal YaoXMax { set; get; }
        public decimal YaoXAbs { set; get; }
        public decimal YaoY { set; get; }
        public decimal YaoYMax { set; get; }
        public decimal YaoYAbs { set; get; }
        public decimal YaoZ { set; get; }
        public decimal YaoZMax { set; get; }
        public decimal YaoZAbs { set; get; }
        public decimal ValidDangX => LimitValues.MaxDangX;
        public decimal ValidDangY => LimitValues.MaxDangY;
        public decimal ValidDangZ => LimitValues.MaxDangX;
        public decimal ValidYaoX => LimitValues.MaxYaoX;
        public decimal ValidYaoY => LimitValues.MaxYaoY;
        public decimal ValidYaoZ => LimitValues.MaxYaoX;
        public decimal YaoXLv1 { set; get; }
        public decimal YaoYLv1 { set; get; }
        public decimal YaoZLv1 { set; get; }
        public decimal YaoXLv2 { set; get; }
        public decimal YaoYLv2 { set; get; }
        public decimal YaoZLv2 { set; get; }
        public string YaoXLvl => GetAlermLevel(YaoX, YaoXLv1, YaoXLv2);
        public string YaoYLvl => GetAlermLevel(YaoY, YaoYLv1, YaoYLv2);
        public string YaoZLvl => GetAlermLevel(YaoZ, YaoZLv1, YaoZLv2);
        public string YaoXMaxLvl => GetAlermLevel(YaoXMax, YaoXLv1, YaoXLv2);
        public string YaoYMaxLvl => GetAlermLevel(YaoYMax, YaoYLv1, YaoYLv2);
        public string YaoZMaxLvl => GetAlermLevel(YaoZMax, YaoZLv1, YaoZLv2);
        public string YaoXAbsLvl => GetAlermLevel(YaoXAbs, YaoXLv1, YaoXLv2);
        public string YaoYAbsLvl => GetAlermLevel(YaoYAbs, YaoYLv1, YaoYLv2);
        public string YaoZAbsLvl => GetAlermLevel(YaoZAbs, YaoZLv1, YaoZLv2);

        public static string GetAlermLevel(decimal val, decimal lv1, decimal lv2)
        {
            return val > lv2 ? "red" : (val > lv1 ? "alerm" : "normal");
        }

        public bool IsValidDangX { get { return Math.Abs(DangX) > ValidDangX; } }
        public bool IsValidDangY { get { return Math.Abs(DangY) > ValidDangY; } }
        public bool IsValidDangZ { get { return Math.Abs(DangZ) > ValidDangZ; } }
        public bool IsValidDangXMax { get { return Math.Abs(DangXMax) > ValidDangX; } }
        public bool IsValidDangYMax { get { return Math.Abs(DangYMax) > ValidDangY; } }
        public bool IsValidDangZMax { get { return Math.Abs(DangZMax) > ValidDangZ; } }
        public bool IsValidDangXAbs { get { return Math.Abs(DangXAbs) > ValidDangX; } }
        public bool IsValidDangYAbs { get { return Math.Abs(DangYAbs) > ValidDangY; } }
        public bool IsValidDangZAbs { get { return Math.Abs(DangZAbs) > ValidDangZ; } }
        public bool IsValidYaoX { get { return Math.Abs(YaoX) > ValidYaoX; } }
        public bool IsValidYaoY { get { return Math.Abs(YaoY) > ValidYaoY; } }
        public bool IsValidYaoZ { get { return Math.Abs(YaoZ) > ValidYaoZ; } }
        public bool IsValidYaoXMax { get { return Math.Abs(YaoXMax) > ValidYaoX; } }
        public bool IsValidYaoYMax { get { return Math.Abs(YaoYMax) > ValidYaoY; } }
        public bool IsValidYaoZMax { get { return Math.Abs(YaoZMax) > ValidYaoZ; } }
        public bool IsValidYaoXAbs { get { return Math.Abs(YaoXAbs) > ValidYaoX; } }
        public bool IsValidYaoYAbs { get { return Math.Abs(YaoYAbs) > ValidYaoY; } }
        public bool IsValidYaoZAbs { get { return Math.Abs(YaoZAbs) > ValidYaoZ; } }

        public DateTime UpdateTime { set; get; }
        public DateTime CreatedTime { set; get; }
        public string CreatedTimeStr => CreatedTime.ToString("HH:mm:ss");
        public string CreatedTimeFullStr => CreatedTime.ToString("yyyy-MM-dd HH:mm:ss");
        public bool IsDeleted { set; get; }
    }

    public class MRUStatusInfoService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        private static decimal TowingWindYaoY = Convert.ToDecimal(AppSettingUtils.GetSetting("TowingWindYaoY") ?? "0");

        public MRUStatusInfoService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        public static decimal MruYaoConvert(decimal value)
        {
            decimal ret = value + TowingWindYaoY;
            return ret;
        }

        public ResponseBase<MRUStatusInfo> GetInfo()
        {
            var res = new ResponseBase<MRUStatusInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_MRU>();
                    // 取最新的一条记录
                    var data = repo.FindAll()
                        .Where(a => a.IsDelete != true)
                        .OrderByDescending(a => a.CreateTime)
                        .FirstOrDefault();

                    res.Data = new MRUStatusInfo();
                    if (data != null)
                    {
                        res.Data.DangX = data.DangX ?? 0;
                        res.Data.DangY = data.DangY ?? 0;
                        res.Data.DangZ = data.DangZ ?? 0;
                        res.Data.YaoX = data.YaoX ?? 0;
                        res.Data.YaoY = data.YaoY ?? 0;
                        res.Data.YaoZ = data.YaoZ ?? 0;
                        res.Data.DeviceID = data.DeviceID ?? 0;
                    }
                    else
                    {
                        res.SetError("尚未有MRU信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MRUStatusInfoService));
                res.SetError(ex);
            }
            return res;
        }

        public ResponseBase<MRUStatusInfo> GetHistoryList(DateTime? startTime = null, DateTime? endTime = null, int size = 0)
        {
            var res = new ResponseBase<MRUStatusInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_MRU>();
                    startTime = startTime ?? DateTime.MinValue;
                    endTime = endTime ?? DateTime.Now;
                    var list = repo.FindAll()
                        .Where(a => a.IsDelete != true
                            && a.CreateTime >= startTime
                            && a.CreateTime < endTime)
                        .OrderBy(a => a.CreateTime)
                        .ToList();

                    res.List = new List<MRUStatusInfo>();
                    if (list != null && list.Count > 0)
                    {
                        foreach (var data in list)
                        {
                            var item = new MRUStatusInfo();
                            item.DangX = data.DangX ?? 0;
                            item.DangY = data.DangY ?? 0;
                            item.DangZ = data.DangZ ?? 0;
                            item.YaoX = data.YaoX ?? 0;
                            item.YaoY = data.YaoY ?? 0;
                            item.YaoZ = data.YaoZ ?? 0;
                            item.DeviceID = data.DeviceID ?? 0;
                            res.List.Add(item);
                        }

                        res.List = Utils.GetUniformSample(res.List, size);
                    }
                    else
                    {
                        res.SetError("尚未有MRU信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MRUStatusInfoService));
                res.SetError(ex);
            }
            return res;
        }

        /// <summary>
        /// 获取最近的历史信息，并进行统计。
        /// </summary>
        /// <param name="size">最近条数，默认300条</param>
        /// <param name="t">0只返回统计结果，1同时返回记录</param>
        public ResponseBase<MRUStatusInfo> GetHistoryListByNum(int size = 0, int t = 0)
        {
            var res = new ResponseBase<MRUStatusInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_MRU>();
                    if (size == 0) size = LimitValues.EChartDataSize;

                    var query = repo.FindAll()
                        .Where(a => a.IsDelete != true)
                        .OrderByDescending(a => a.CreateTime);

                    List<DataFactory.KingBase.Towing.Data_MRU> list;
                    // 仅当size为0时才不做数量限制
                    if (size > 0)
                    {
                        list = query.Take(size).ToList();
                    }
                    else
                    {
                        double offlineSeconds = (double)(0 - LimitValues.DeviceOfflineSeconds);
                        list = query.Where(a => a.CreateTime >= DateTime.Now.AddSeconds(offlineSeconds)).ToList();
                    }

                    res.List = new List<MRUStatusInfo>();
                    if (list != null && list.Count > 0)
                    {
                        foreach (var data in list.OrderBy(a => a.CreateTime))
                        {
                            var item = new MRUStatusInfo();
                            item.DangX = data.DangX ?? 0;
                            item.DangY = data.DangY ?? 0;
                            item.DangZ = data.DangZ ?? 0;
                            item.YaoX = data.YaoX ?? 0;
                            item.YaoY = data.YaoY ?? 0;
                            item.YaoZ = data.YaoZ ?? 0;
                            item.DeviceID = data.DeviceID ?? 0;
                            res.List.Add(item);
                        }
                        res.Data = new MRUStatusInfo();
                        // 最大值
                        res.Data.DangXMax = res.List.Select(a => a.DangX).Max();
                        res.Data.DangYMax = res.List.Select(a => a.DangY).Max();
                        res.Data.DangZMax = res.List.Select(a => a.DangZ).Max();
                        res.Data.YaoXMax = res.List.Select(a => a.YaoX).Max();
                        res.Data.YaoYMax = res.List.Select(a => a.YaoY).Max();
                        res.Data.YaoZMax = res.List.Select(a => a.YaoZ).Max();
                        // 绝对值后的最大值
                        res.Data.DangXAbs = res.List.Select(a => Math.Abs(a.DangX)).Max();
                        res.Data.DangYAbs = res.List.Select(a => Math.Abs(a.DangY)).Max();
                        res.Data.DangZAbs = res.List.Select(a => Math.Abs(a.DangZ)).Max();
                        res.Data.YaoXAbs = res.List.Select(a => Math.Abs(a.YaoX)).Max();
                        res.Data.YaoYAbs = res.List.Select(a => Math.Abs(a.YaoY)).Max();
                        res.Data.YaoZAbs = res.List.Select(a => Math.Abs(a.YaoZ)).Max();

                        // 获取alerm值
                        var alarmRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_Alarm>();
                        var alerm = alarmRepo.FindAll().FirstOrDefault(a => a.IsDelete != true && a.Type == "MRU");
                        if (alerm != null)
                        {
                            // 只有度数的需要验证
                            res.Data.YaoXLv1 = alerm.AlarmValue1 ?? 90;
                            res.Data.YaoYLv1 = alerm.AlarmValue1 ?? 90;
                            res.Data.YaoZLv1 = alerm.AlarmValue1 ?? 90;
                            res.Data.YaoXLv2 = alerm.AlarmValue2 ?? 90;
                            res.Data.YaoYLv2 = alerm.AlarmValue2 ?? 90;
                            res.Data.YaoZLv2 = alerm.AlarmValue2 ?? 90;
                        }

                        // 若只需总结信息，则清空List。
                        if (t == 0)
                        {
                            res.List.Clear();
                        }
                    }
                    else
                    {
                        res.SetError("尚未有MRU信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MRUStatusInfoService));
                res.SetError(ex);
            }
            return res;
        }

        /// <summary>
        /// 获取最近的历史信息，并进行统计。仅供Main左侧用，取最近10分钟的
        /// </summary>
        public ResponseBase<MRUStatusInfo> GetHistoryListByNumV2(DateTime? startTime = null)
        {
            var res = new ResponseBase<MRUStatusInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_MRU>();
                    // 每次获取上次刷新时间到现在的数据，若第一次打开，无上次刷新时间，则取最近10分钟的
                    if (startTime == null || startTime == DateTime.MinValue)
                    {
                        startTime = LimitValues.DeviceOfflineTime;
                    }

                    //2026-4-10 王衔  只取最近的120秒的数据,防止传感器时间和服务器时间冲突，
                    //采用取最近的60条数据（每2秒一条数据，即120秒内）
                    int takeCount = 60;

                    res.Data.UpdateTime = DateTime.Now;

                    var query = repo.FindAll()
                        .Where(a => a.IsDelete != true && a.CreateTime > startTime)
                        .OrderByDescending(a => a.CreateTime)
                        .Take(takeCount);
                    var list = query.ToList();

                    res.List = new List<MRUStatusInfo>();
                    if (list != null && list.Count > 0)
                    {
                        foreach (var data in list.OrderBy(a => a.CreateTime))
                        {
                            var item = new MRUStatusInfo();
                            item.DangX = data.DangX ?? 0;
                            item.DangY = data.DangY ?? 0;
                            item.DangZ = data.DangZ ?? 0;
                            item.YaoX = data.YaoX ?? 0;
                            item.YaoY = data.YaoY ?? 0;
                            item.YaoZ = data.YaoZ ?? 0;
                            item.DeviceID = data.DeviceID ?? 0;
                            item.CreatedTime = data.CreateTime.Value;
                            res.List.Add(item);
                        }
                        res.Data = new MRUStatusInfo();
                        // 返回当前时间
                        //res.Data.UpdateTime = DateTime.Now;

                        //最新值
                        var latestData = res.List.OrderByDescending(a => a.CreatedTime).FirstOrDefault();
                        res.Data.DangXMax = latestData.DangX;
                        res.Data.DangYMax = latestData.DangY;
                        res.Data.DangZMax = latestData.DangZ;

                        res.Data.YaoXMax = latestData.YaoX;
                        res.Data.YaoYMax = MruYaoConvert(latestData.YaoY);
                        res.Data.YaoZMax = latestData.YaoZ;

                        //最大值减去最小值
                        res.Data.DangXAbs = res.List.Select(a => a.DangX).Max() - res.List.Select(a => a.DangX).Min();
                        res.Data.DangYAbs = res.List.Select(a => a.DangY).Max() - res.List.Select(a => a.DangY).Min();
                        res.Data.DangZAbs = res.List.Select(a => a.DangZ).Max() - res.List.Select(a => a.DangZ).Min();
                        res.Data.YaoXAbs = res.List.Select(a => a.YaoX).Max() - res.List.Select(a => a.YaoX).Min();
                        res.Data.YaoYAbs = res.List.Select(a => a.YaoY).Max() - res.List.Select(a => a.YaoY).Min();
                        res.Data.YaoZAbs = res.List.Select(a => a.YaoZ).Max() - res.List.Select(a => a.YaoZ).Min();

                        // 获取alerm值
                        var alarmRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_Alarm>();
                        var alerm = alarmRepo.FindAll().FirstOrDefault(a => a.IsDelete != true && a.Type == "MRU");
                        if (alerm != null)
                        {
                            // 只有度数的需要验证
                            res.Data.YaoXLv1 = alerm.AlarmValue1 ?? 90;
                            res.Data.YaoYLv1 = alerm.AlarmValue1 ?? 90;
                            res.Data.YaoZLv1 = alerm.AlarmValue1 ?? 90;
                            res.Data.YaoXLv2 = alerm.AlarmValue2 ?? 90;
                            res.Data.YaoYLv2 = alerm.AlarmValue2 ?? 90;
                            res.Data.YaoZLv2 = alerm.AlarmValue2 ?? 90;
                        }

                        // 若只需总结信息，则清空List。
                        res.List.Clear();
                    }
                    else
                    {
                        res.SetError("尚未有MRU信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MRUStatusInfoService));
                res.SetError(ex);
            }
            return res;
        }

        public ResponseBase<EChartBase> GetEChartDataByNum(int size = 0)
        {
            var res = new ResponseBase<EChartBase>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_MRU>();
                    if (size == 0) size = LimitValues.EChartDataSize;

                    var query = repo.FindAll()
                        .Where(a => a.IsDelete != true)
                        .OrderByDescending(a => a.CreateTime);

                    List<DataFactory.KingBase.Towing.Data_MRU> list;
                    // 仅当size为0时才不做数量限制
                    if (size > 0)
                    {
                        list = query.Take(size).ToList();
                    }
                    else
                    {
                        double offlineSeconds = (double)(0 - LimitValues.DeviceOfflineSeconds);
                        list = query.Where(a => a.CreateTime >= DateTime.Now.AddSeconds(offlineSeconds)).ToList();
                    }

                    if (list != null && list.Count > 0)
                    {
                        foreach (var data in list.OrderBy(a => a.CreateTime))
                        {
                            var item = new MRUStatusInfo();
                            item.DangX = data.DangX ?? 0;
                            item.DangY = data.DangY ?? 0;
                            item.DangZ = data.DangZ ?? 0;
                            item.YaoX = data.YaoX ?? 0;
                            item.YaoY = data.YaoY ?? 0;
                            item.YaoZ = data.YaoZ ?? 0;
                            item.CreatedTime = data.CreateTime ?? DateTime.Now;
                            item.DeviceID = data.DeviceID ?? 0;
                            res.Data.AddData("横荡", item.CreatedTimeFullStr, item.DangX);
                            res.Data.AddData("纵荡", item.CreatedTimeFullStr, item.DangY);
                            res.Data.AddData("垂荡", item.CreatedTimeFullStr, item.DangZ);
                            res.Data.AddData("横摇", item.CreatedTimeFullStr, item.YaoX);
                            res.Data.AddData("纵摇", item.CreatedTimeFullStr, item.YaoY);
                            res.Data.AddData("艏摇", item.CreatedTimeFullStr, item.YaoZ);
                        }
                    }
                    else
                    {
                        res.SetError("尚未有MRU信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MRUStatusInfoService));
                res.SetError(ex);
            }
            return res;
        }

        /// <summary>
        /// 获取垂向运动（垂荡、横摇、纵摇）
        /// </summary>
        public ResponseBase<EChartBase> GetChartZMotion(DateTime? startTime = null, DateTime? endTime = null, int size = 0)
            => GetChartData(startTime, endTime, size, "垂荡,横摇,纵摇");

        /// <summary>
        /// 获取横向运动（横荡、纵荡、艏摇）
        /// </summary>
        public ResponseBase<EChartBase> GetChartXMotion(DateTime? startTime = null, DateTime? endTime = null, int size = 0)
            => GetChartData(startTime, endTime, size, "横荡,纵荡,艏摇");

        public ResponseBase<EChartBase> GetChartData(DateTime? startTime = null, DateTime? endTime = null, int size = 0, string legend = "")
        {
            var res = new ResponseBase<EChartBase>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_MRU>();
                    if (size == 0) size = LimitValues.EChartDataSize;
                    startTime = startTime ?? DateTime.MinValue;
                    endTime = endTime ?? DateTime.Now;

                    var query = repo.FindAll()
                        .Where(a => a.IsDelete != true
                            && a.CreateTime >= startTime
                            && a.CreateTime < endTime)
                        .OrderByDescending(a => a.CreateTime);

                    List<DataFactory.KingBase.Towing.Data_MRU> list;
                    // 仅当size为0时才不做数量限制
                    if (size > 0)
                    {
                        list = query.Take(size).OrderBy(a => a.CreateTime).ToList();
                    }
                    else
                    {
                        double offlineSeconds = (double)(0 - LimitValues.DeviceOfflineSeconds);
                        list = query.Where(a => a.CreateTime >= DateTime.Now.AddSeconds(offlineSeconds)).OrderBy(a => a.CreateTime).ToList();
                    }

                    res.List = new List<EChartBase>();
                    if (list != null && list.Count > 0)
                    {
                        foreach (var data in list)
                        {
                            var item = new MRUStatusInfo();
                            item.DangX = data.DangX ?? 0;
                            item.DangY = data.DangY ?? 0;
                            item.DangZ = data.DangZ ?? 0;
                            item.YaoX = data.YaoX ?? 0;
                            item.YaoY = MruYaoConvert(data.YaoY ?? 0);
                            item.YaoZ = data.YaoZ ?? 0;
                            item.DeviceID = data.DeviceID ?? 0;
                            item.CreatedTime = data.CreateTime ?? DateTime.Now;

                            if (string.IsNullOrEmpty(legend)) legend = "横荡,纵荡,垂荡,横摇,纵摇,艏摇";
                            var legArr = legend.Split(',');
                            if (legArr.Contains("横荡"))
                            {
                                res.Data.AddData("横荡", item.CreatedTimeFullStr, item.DangX);
                            }
                            if (legArr.Contains("纵荡"))
                            {
                                res.Data.AddData("纵荡", item.CreatedTimeFullStr, item.DangY);
                            }
                            if (legArr.Contains("垂荡"))
                            {
                                res.Data.AddData("垂荡", item.CreatedTimeFullStr, item.DangZ);
                            }
                            if (legArr.Contains("横摇"))
                            {
                                res.Data.AddData("横摇", item.CreatedTimeFullStr, item.YaoX);
                            }
                            if (legArr.Contains("纵摇"))
                            {
                                res.Data.AddData("纵摇", item.CreatedTimeFullStr, item.YaoY);
                            }
                            if (legArr.Contains("艏摇"))
                            {
                                res.Data.AddData("艏摇", item.CreatedTimeStr, item.YaoZ);
                            }
                        }
                    }
                    else
                    {
                        res.SetError("尚未有MRU信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MRUStatusInfoService));
                res.SetError(ex);
            }
            return res;
        }
    }
}
