using Business.Towing;
using DataFactory.Factory;
using Tool;

namespace Service.Towing
{
    /// <summary>
    /// 电磁罗经信息
    /// </summary>
    public class EGYROInfo
    {
        /// <summary>
        /// 方位角
        /// </summary>
        public decimal? AzAngle { set; get; }
        public string AzAngleDirectionStr
            => Utils.GetDirectionStr16(AzAngle ?? 0m);

        /// <summary>
        /// 横倾
        /// </summary>
        public decimal? AngleX { set; get; }

        /// <summary>
        /// 纵倾
        /// </summary>
        public decimal? AngleY { set; get; }

        /// <summary>
        /// 状态
        /// </summary>
        public string StatusStr { set; get; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { set; get; }
        public string UpdateTimeStr => UpdateTime.ToString("HH:mm:ss");
        public string UpdateTimeFullStr => UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");

        public decimal SailSpeed { set; get; }
    }

    public class EGYROInfoService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        public EGYROInfoService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        /// <summary>
        /// 获取当前电磁罗经信息
        /// </summary>
        public ResponseBase<EGYROInfo> GetInfo()
        {
            var res = new ResponseBase<EGYROInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_EGYRO>();
                    // 取最新的一条记录
                    var egyro = repo.Find(a => a.IsDelete == false)
                        .OrderByDescending(a => a.CreateTime)
                        .FirstOrDefault();

                    res.Data = new EGYROInfo();
                    if (egyro != null)
                    {
                        res.Data.AngleX = egyro.AngleX ?? 0;
                        res.Data.AngleY = egyro.AngleY ?? 0;
                        res.Data.AzAngle = egyro.AzAngle ?? 0;
                        res.Data.UpdateTime = egyro.CreateTime ?? DateTime.Now;
                    }
                    else
                    {
                        res.SetError("尚未有电磁罗经信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(EGYROInfoService));
                res.SetError(ex);
            }
            return res;
        }

        /// <summary>
        /// 获取当前电磁罗经信息，仅10分钟内才有效
        /// </summary>
        public ResponseBase<EGYROInfo> GetInfoV2()
        {
            var res = new ResponseBase<EGYROInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var egyroRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_EGYRO>();
                    // 取最新的一条记录
                    var egyro = egyroRepo.Find(a => a.IsDelete == false && a.CreateTime > LimitValues.DeviceOfflineTime)
                        .OrderByDescending(a => a.CreateTime)
                        .FirstOrDefault();

                    res.Data = new EGYROInfo();
                    if (egyro != null)
                    {
                        res.Data.AngleX = egyro.AngleX ?? 0;
                        res.Data.AngleY = egyro.AngleY ?? 0;
                        res.Data.AzAngle = egyro.AzAngle ?? 0;
                        res.Data.UpdateTime = egyro.CreateTime ?? DateTime.Now;
                    }
                    else
                    {
                        res.SetError("尚未有电磁罗经信息。");
                    }

                    //计算航速
                    var satRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_SATELLITE>();
                    var LastPositions = satRepo.Find(a => !a.IsDelete)
                        .OrderByDescending(a => a.CreateTime)
                        .Take(2)
                        .ToList();

                    res.Data.SailSpeed = 0;
                    if (LastPositions.Count == 2
                        && LastPositions[0].Lon != null && LastPositions[1].Lon != null
                        && LastPositions[0].Lat != null && LastPositions[1].Lat != null
                        && LastPositions[0].CreateTime != LastPositions[1].CreateTime)
                    {
                        var LonDiffMeter = (LastPositions[0].Lon - LastPositions[1].Lon) * 31544206 / 360;
                        var LatDiffMeter = (LastPositions[0].Lat - LastPositions[1].Lat) * 40030173 / 360;

                        double hourDiff = (LastPositions[0].CreateTime.Value - LastPositions[1].CreateTime.Value).TotalHours;

                        var Speed = Math.Abs(Math.Sqrt(Math.Pow(Convert.ToDouble(LonDiffMeter), 2) + Math.Pow(Convert.ToDouble(LatDiffMeter), 2)) / hourDiff);

                        var SailSpeed = Math.Round(Speed / 1852, 2);

                        res.Data.SailSpeed = Convert.ToDecimal(SailSpeed);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(EGYROInfoService));
                res.SetError(ex);
            }
            return res;
        }

        /// <summary>
        /// 获取历史电磁罗经信息
        /// </summary>
        public ResponseBase<EGYROInfo> GetHistoryList(DateTime? startTime = null, DateTime? endTime = null)
        {
            var res = new ResponseBase<EGYROInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_EGYRO>();
                    startTime = startTime ?? DateTime.MinValue;
                    endTime = endTime ?? DateTime.Now;

                    var list = repo.Find(a => a.IsDelete == false
                            && a.CreateTime >= startTime
                            && a.CreateTime < endTime)
                        .OrderBy(a => a.CreateTime)
                        .ToList();

                    res.List = new List<EGYROInfo>();
                    if (list != null && list.Count > 0)
                    {
                        foreach (var egyro in list)
                        {
                            var itm = new EGYROInfo();
                            itm.AngleX = egyro.AngleX ?? 0;
                            itm.AngleY = egyro.AngleY ?? 0;
                            itm.AzAngle = egyro.AzAngle ?? 0;
                            itm.UpdateTime = egyro.CreateTime ?? DateTime.Now;
                            res.List.Add(itm);
                        }
                    }
                    else
                    {
                        res.SetError("尚未有电磁罗经信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(EGYROInfoService));
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
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_EGYRO>();
                    if (size == 0) size = LimitValues.EChartDataSize;

                    var query = repo.Find(a => a.IsDelete == false)
                        .OrderByDescending(a => a.CreateTime);

                    List<DataFactory.KingBase.Towing.Data_EGYRO> list;
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
                        foreach (var egyro in list.OrderBy(a => a.CreateTime))
                        {
                            var itm = new EGYROInfo();
                            itm.AngleX = egyro.AngleX ?? 0;
                            itm.AngleY = egyro.AngleY ?? 0;
                            itm.AzAngle = egyro.AzAngle ?? 0;
                            itm.UpdateTime = egyro.CreateTime ?? DateTime.Now;
                            res.Data.AddData("船首角度", itm.UpdateTimeStr, itm.AzAngle ?? 0);
                            res.Data.AddData("船首指向", itm.UpdateTimeStr, itm.AzAngleDirectionStr);
                        }
                    }
                    else
                    {
                        res.SetError("尚未有电磁罗经信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(EGYROInfoService));
                res.SetError(ex);
            }
            return res;
        }

        public ResponseBase<EGYROInfo> GetEChartDataByNumV2(int size = 0)
        {
            var res = new ResponseBase<EGYROInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_EGYRO>();
                    if (size == 0) size = LimitValues.EChartDataSize;

                    var query = repo.Find(a => a.IsDelete == false)
                        .OrderByDescending(a => a.CreateTime);

                    List<DataFactory.KingBase.Towing.Data_EGYRO> list;
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
                        foreach (var egyro in list.OrderBy(a => a.CreateTime))
                        {
                            var itm = new EGYROInfo();
                            itm.AngleX = egyro.AngleX ?? 0;
                            itm.AngleY = egyro.AngleY ?? 0;
                            itm.AzAngle = egyro.AzAngle ?? 0;
                            itm.UpdateTime = egyro.CreateTime ?? DateTime.Now;
                            res.List.Add(itm);
                        }
                    }
                    else
                    {
                        res.SetError("尚未有电磁罗经信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(EGYROInfoService));
                res.SetError(ex);
            }
            return res;
        }

        public ResponseBase<EChartBase> GetChart(DateTime? startTime = null, DateTime? endTime = null, int size = 0, string legend = "")
        {
            var res = new ResponseBase<EChartBase>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_EGYRO>();
                    startTime = startTime ?? DateTime.MinValue;
                    endTime = endTime ?? DateTime.Now;

                    if (size == 0) size = LimitValues.EChartDataSize;

                    var query = repo.Find(a => a.IsDelete == false && a.CreateTime >= startTime && a.CreateTime < endTime)
                        .OrderByDescending(a => a.CreateTime);

                    List<DataFactory.KingBase.Towing.Data_EGYRO> list;
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

                    if (list != null && list.Count > 0)
                    {
                        foreach (var egyro in list.OrderBy(a => a.CreateTime))
                        {
                            var itm = new EGYROInfo();
                            itm.AngleX = egyro.AngleX ?? 0;
                            itm.AngleY = egyro.AngleY ?? 0;
                            itm.AzAngle = egyro.AzAngle == null ? 0 : NavigationalStatusService.AZangleConvert(egyro.AzAngle.Value);
                            itm.UpdateTime = egyro.CreateTime ?? DateTime.Now;

                            var legendArr = legend.Split(',');
                            if (legendArr.Contains("航向"))
                            {
                                res.Data.AddData("航向", itm.UpdateTimeFullStr, itm.AzAngle ?? 0);
                            }
                            if (legendArr.Contains("航向指向"))
                            {
                                res.Data.AddData("航向指向", itm.UpdateTimeFullStr, itm.AzAngleDirectionStr);
                            }
                        }
                    }
                    else
                    {
                        res.SetError("尚未有电磁罗经信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(EGYROInfoService));
                res.SetError(ex);
            }
            return res;
        }
    }
}
