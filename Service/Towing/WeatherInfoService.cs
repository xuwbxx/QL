using Business.Towing;
using DataFactory.Factory;
using Model.Towing;
using Tool;

namespace Service.Towing
{
    public class WeatherInfo
    {
        public int DeviceID { set; get; }
        public decimal? WindSpeed { set; get; }
        public string WindSpeedStr => Utils.getStr(WindSpeed, 2);
        public decimal WindSpeedLv1 { set; get; }
        public decimal WindSpeedLv2 { set; get; }
        public string WindSpeedLvl => GetAlermLevel(WindSpeed ?? 0, WindSpeedLv1, WindSpeedLv2);
        public decimal? WindDirection { set; get; }
        public string WindDirectionStr => Utils.getStr(WindDirection, 2);
        public DateTime UpdateTime { set; get; }
        public string UpdateTimeStr => UpdateTime.ToString("HH:mm:ss");
        public string UpdateTimeFullStr => UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");

        public static string GetAlermLevel(decimal val, decimal lv1, decimal lv2)
        {
            return val > lv2 ? "red" : (val > lv1 ? "alerm" : "normal");
        }
    }

    public class WeatherInfoService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;
        private readonly PXYShipService _pxyShipService;

        private static int TowingWindDirectionOffset = Convert.ToInt32(AppSettingUtils.GetSetting("THScan:TowingWindDirection") ?? "0");

        public WeatherInfoService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory, PXYShipService pxyShipService)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
            _pxyShipService = pxyShipService;
        }

        private static decimal WindDirectionConvert(decimal value)
        {
            //0 北风 箭头向下  180南风 箭头向上
            //数值要加偏差值  还要加180是反方向
            decimal ret = value + TowingWindDirectionOffset;
            if (ret >= 360)
            {
                ret = ret - 360;
            }
            return ret;
        }

        /// <summary>
        /// 最新的信息
        /// </summary>
        public ResponseBase<WeatherInfo> GetLastWindInfo()
        {
            var res = new ResponseBase<WeatherInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var windRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Wind>();
                    // 取最新的一条记录
                    var data = windRepo.Find(a => a.IsDelete != true)
                        .OrderByDescending(a => a.CreateTime)
                        .FirstOrDefault();

                    res.Data = new WeatherInfo();
                    if (data != null)
                    {
                        res.Data.DeviceID = data.DeviceID ?? 0;
                        res.Data.WindSpeed = data.WindSpeed ?? 0;
                        res.Data.WindDirection = data.WindDirection ?? 0;
                        res.Data.UpdateTime = data.CreateTime ?? DateTime.Now;

                        var alarmRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_Alarm>();
                        var alerm = alarmRepo.Find(a => a.IsDelete != true && a.Type == "WindSpeed").FirstOrDefault();
                        if (alerm != null)
                        {
                            res.Data.WindSpeedLv1 = alerm.AlarmValue1 ?? 99;
                            res.Data.WindSpeedLv2 = alerm.AlarmValue2 ?? 99;
                        }
                    }
                    else
                    {
                        res.SetError("尚未有风向信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WeatherInfoService));
                res.SetError(ex);
            }
            return res;
        }

        /// <summary>
        /// 10分钟内才有效
        /// </summary>
        public ResponseBase<WeatherInfo> GetLastWindInfoV2()
        {
            var res = new ResponseBase<WeatherInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var windRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Wind>();
                    // 取最新的一条记录
                    var data = windRepo.Find(a => a.IsDelete != true && a.CreateTime > LimitValues.DeviceOfflineTime)
                        .OrderByDescending(a => a.CreateTime)
                        .FirstOrDefault();

                    res.Data = new WeatherInfo();
                    if (data != null)
                    {
                        res.Data.DeviceID = data.DeviceID ?? 0;
                        res.Data.WindSpeed = data.WindSpeed ?? 0;
                        res.Data.WindDirection = data.WindDirection == null ? 0 : WindDirectionConvert(data.WindDirection.Value);
                        res.Data.UpdateTime = data.CreateTime ?? DateTime.Now;

                        var alarmRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_Alarm>();
                        var alerm = alarmRepo.Find(a => a.IsDelete != true && a.Type == "WindSpeed").FirstOrDefault();
                        if (alerm != null)
                        {
                            res.Data.WindSpeedLv1 = alerm.AlarmValue1 ?? 99;
                            res.Data.WindSpeedLv2 = alerm.AlarmValue2 ?? 99;
                        }
                    }
                    else
                    {
                        res.SetError("尚未有风向信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WeatherInfoService));
                res.SetError(ex);
            }
            return res;
        }

        public ResponseBase<HFWeatherInfo> GetWeatherForecastList(string lon, string lat)
            => _pxyShipService.GetWeatherForecastList(lon, lat);

        public ResponseBase<EChartBase> GetChart(DateTime? startTime = null, DateTime? endTime = null, int size = 0, string legend = "")
        {
            var res = new ResponseBase<EChartBase>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var windRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Wind>();
                    if (startTime == null) startTime = DateTime.MinValue;
                    if (endTime == null) endTime = DateTime.Now;
                    if (size == 0) size = LimitValues.EChartDataSize;
                    if (string.IsNullOrEmpty(legend)) legend = "风向,风速";

                    var query = windRepo.Find(a => a.IsDelete != true && a.CreateTime >= startTime && a.CreateTime < endTime)
                        .OrderByDescending(a => a.CreateTime);

                    List<DataFactory.KingBase.Towing.Data_Wind> list;
                    // 仅当size为0时才不做数量限制
                    if (size > 0)
                    {
                        list = query.Take(size).OrderBy(a => a.CreateTime).ToList();
                    }
                    else
                    {
                        list = query.OrderBy(a => a.CreateTime).ToList();
                    }

                    if (list != null && list.Count > 0)
                    {
                        foreach (var data in list.OrderBy(a => a.CreateTime))
                        {
                            if (data.WindSpeed != null && data.WindDirection != null)
                            {
                                var item = new WeatherInfo();
                                item.DeviceID = data.DeviceID ?? 0;
                                item.WindSpeed = data.WindSpeed ?? 0;
                                item.WindDirection = data.WindDirection == null ? 0 : WindDirectionConvert(data.WindDirection.Value);
                                item.UpdateTime = data.CreateTime ?? DateTime.Now;

                                var legendArr = legend.Split(',');
                                if (legendArr.Contains("风速"))
                                {
                                    res.Data.AddData("风速", item.UpdateTimeFullStr.Replace(" ", " \r"), item.WindSpeed);
                                }
                                if (legendArr.Contains("风向"))
                                {
                                    res.Data.AddData("风向", item.UpdateTimeFullStr.Replace(" ", " \r"), item.WindDirection);
                                }
                            }
                        }
                    }
                    else
                    {
                        res.SetError("尚未有风向信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WeatherInfoService));
                res.SetError(ex);
            }
            return res;
        }

        public ResponseBase<EChartBase> GetWindEChartDataByNum(int size = 0)
        {
            var res = new ResponseBase<EChartBase>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var windRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Wind>();
                    if (size == 0) size = LimitValues.EChartDataSize;

                    var query = windRepo.Find(a => a.IsDelete != true)
                        .OrderByDescending(a => a.CreateTime);

                    List<DataFactory.KingBase.Towing.Data_Wind> list;
                    // 仅当size为0时才不做数量限制
                    if (size > 0)
                    {
                        list = query.Take(size).ToList();
                    }
                    else
                    {
                        list = query.ToList();
                    }

                    if (list != null && list.Count > 0)
                    {
                        foreach (var data in list.OrderBy(a => a.CreateTime))
                        {
                            var item = new WeatherInfo();
                            item.DeviceID = data.DeviceID ?? 0;
                            item.WindSpeed = data.WindSpeed ?? 0;
                            item.WindDirection = data.WindDirection ?? 0;
                            item.UpdateTime = data.CreateTime ?? DateTime.Now;

                            res.Data.AddData("风速", item.UpdateTimeFullStr, item.WindSpeed);
                        }
                    }
                    else
                    {
                        res.SetError("尚未有风向信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WeatherInfoService));
                res.SetError(ex);
            }
            return res;
        }

        public ResponseBase<WeatherInfo> GetWindEChartDataByNumV2(int size = 0)
        {
            var res = new ResponseBase<WeatherInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var windRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Wind>();
                    if (size == 0) size = LimitValues.EChartDataSize;

                    var query = windRepo.Find(a => a.IsDelete != true)
                        .OrderByDescending(a => a.CreateTime);

                    List<DataFactory.KingBase.Towing.Data_Wind> list;
                    // 仅当size为0时才不做数量限制
                    if (size > 0)
                    {
                        list = query.Take(size).ToList();
                    }
                    else
                    {
                        list = query.ToList();
                    }

                    if (list != null && list.Count > 0)
                    {
                        foreach (var data in list.OrderBy(a => a.CreateTime))
                        {
                            var item = new WeatherInfo();
                            item.DeviceID = data.DeviceID ?? 0;
                            item.WindSpeed = data.WindSpeed ?? 0;
                            item.WindDirection = data.WindDirection ?? 0;
                            item.UpdateTime = data.CreateTime ?? DateTime.Now;

                            res.List.Add(item);
                        }
                    }
                    else
                    {
                        res.SetError("尚未有风向信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WeatherInfoService));
                res.SetError(ex);
            }
            return res;
        }
    }
}
