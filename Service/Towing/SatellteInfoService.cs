using Business.Towing;
using DataFactory.Factory;
using Tool;

namespace Service.Towing
{
    public class SatellteInfo
    {
        public int ID { set; get; }
        public int DeviceID { set; get; }

        /// <summary>
        /// 实时经度
        /// </summary>
        public decimal? Longitude { set; get; }
        public string LonStr => Utils.getStr(Longitude, 5);
        public string LonDirection { set; get; }

        /// <summary>
        /// 实时纬度
        /// </summary>
        public decimal? Latitude { set; get; }
        public string LatStr => Utils.getStr(Latitude, 5);
        public string LatDirection { set; get; }

        /// <summary>
        /// 实时海拔
        /// </summary>
        public decimal? Altitude { set; get; }
        public string AltStr => Utils.getStr(Altitude);

        /// <summary>
        /// 最新航向角度
        /// </summary>
        public decimal? LatestDirectionAngle { set; get; }
        public string LatestDirectionAngleStr => Utils.getStr(LatestDirectionAngle);

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { set; get; }
    }

    public class SatellteInfoService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        public SatellteInfoService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        /// <summary>
        /// 获取当前信息
        /// </summary>
        public ResponseBase<SatellteInfo> GetInfo()
        {
            var res = new ResponseBase<SatellteInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var satRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_SATELLITE>();
                    var data = satRepo.FindAll()
                        .Where(a => a.IsDelete != true)
                        .OrderByDescending(a => a.CreateTime)
                        .FirstOrDefault();

                    res.Data = new SatellteInfo();
                    if (data != null)
                    {
                        res.Data.DeviceID = data.DeviceID ?? 0;
                        res.Data.Longitude = data.Lon ?? 0;
                        res.Data.LonDirection = data.LonDirection;
                        res.Data.Latitude = data.Lat ?? 0;
                        res.Data.LatDirection = data.LatDirection;
                        res.Data.Altitude = data.Altitude ?? 0;
                        res.Data.UpdateTime = data.CreateTime ?? DateTime.Now;

                        var egyroRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_EGYRO>();
                        var egyro = egyroRepo.FindAll().OrderByDescending(a => a.CreateTime).FirstOrDefault();
                        if (egyro != null)
                        {
                            res.Data.LatestDirectionAngle = egyro.AzAngle ?? 0;
                        }
                    }
                    else
                    {
                        res.SetError("尚未有卫星定位信息记录。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(SatellteInfoService));
                res.SetError(ex);
            }
            return res;
        }

        /// <summary>
        /// 获取当前信息，最近10分钟内的才有效
        /// </summary>
        public ResponseBase<SatellteInfo> GetInfoV2()
        {
            var res = new ResponseBase<SatellteInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var satRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_SATELLITE>();
                    var data = satRepo.FindAll()
                        .Where(a => a.IsDelete != true && a.CreateTime > LimitValues.DeviceOfflineTime)
                        .OrderByDescending(a => a.CreateTime)
                        .FirstOrDefault();

                    res.Data = new SatellteInfo();
                    if (data != null)
                    {
                        res.Data.DeviceID = data.DeviceID ?? 0;
                        res.Data.Longitude = data.Lon ?? 0;
                        res.Data.LonDirection = data.LonDirection;
                        res.Data.Latitude = data.Lat ?? 0;
                        res.Data.LatDirection = data.LatDirection;
                        res.Data.Altitude = data.Altitude ?? 0;
                        res.Data.UpdateTime = data.CreateTime ?? DateTime.Now;

                        var egyroRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_EGYRO>();
                        var egyro = egyroRepo.FindAll().OrderByDescending(a => a.CreateTime).FirstOrDefault();
                        if (egyro != null)
                        {
                            res.Data.LatestDirectionAngle = egyro.AzAngle ?? 0;
                        }
                    }
                    else
                    {
                        res.SetError("尚未有卫星定位信息记录。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(SatellteInfoService));
                res.SetError(ex);
            }
            return res;
        }

        /// <summary>
        /// 获取历史信息
        /// </summary>
        public ResponseBase<SatellteInfo> GetHistoryList(DateTime? startTime = null, DateTime? endTime = null, int size = 0)
        {
            var res = new ResponseBase<SatellteInfo>();
            try
            {
                if (size == 0) size = LimitValues.SatelltePointSize;
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var satRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_SATELLITE>();
                    startTime = startTime ?? DateTime.MinValue;
                    endTime = endTime ?? DateTime.Now;

                    var list = satRepo.FindAll()
                        .Where(a => a.IsDelete != true
                            && a.CreateTime > startTime
                            && a.CreateTime <= endTime)
                        .OrderBy(a => a.CreateTime)
                        .ToList();

                    if (startTime == DateTime.MinValue)
                    {
                        // 若开始时间未设置，则包含项目起点
                        var basicRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_Basic>();
                        var prjInfo = basicRepo.FindAll().FirstOrDefault(a => a.IsDelete != true);
                        if (prjInfo != null)
                        {
                            var item = new SatellteInfo()
                            {
                                Longitude = prjInfo.StartLon ?? 0,
                                Latitude = prjInfo.StartLat ?? 0,
                                UpdateTime = startTime ?? DateTime.MinValue
                            };
                            res.List.Add(item);
                        }
                    }
                    if (list != null && list.Count > 0)
                    {
                        // 为0时不做稀释
                        if (size > 0)
                        {
                            list = Utils.GetUniformSample(list, size);
                        }
                        foreach (var data in list)
                        {
                            var item = new SatellteInfo();
                            item.ID = data.id;
                            item.DeviceID = data.DeviceID ?? 0;
                            item.Longitude = data.Lon ?? 0;
                            item.LonDirection = data.LonDirection;
                            item.Latitude = data.Lat ?? 0;
                            item.LatDirection = data.LatDirection;
                            item.Altitude = data.Altitude ?? 0;
                            item.UpdateTime = data.CreateTime ?? DateTime.Now;
                            res.List.Add(item);
                        }
                        if (res.List.Count > 0)
                        {
                            if (res.List.Count >= 2)
                            {
                                Location point1 = new Location()
                                {
                                    Lon = res.List[res.List.Count - 2].Longitude == null ? 0 : res.List[res.List.Count - 2].Longitude.Value,
                                    Lat = res.List[res.List.Count - 2].Latitude == null ? 0 : res.List[res.List.Count - 2].Latitude.Value,
                                };
                                Location point2 = new Location()
                                {
                                    Lon = res.List[res.List.Count - 1].Longitude == null ? 0 : res.List[res.List.Count - 1].Longitude.Value,
                                    Lat = res.List[res.List.Count - 1].Latitude == null ? 0 : res.List[res.List.Count - 1].Latitude.Value,
                                };
                                res.List[res.List.Count - 1].LatestDirectionAngle = NavigationCalculator.CalculateBearing(point1, point2);
                            }
                            else
                            {
                                var egyroRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_EGYRO>();
                                var egyro = egyroRepo.FindAll().OrderByDescending(a => a.CreateTime).FirstOrDefault();
                                if (egyro != null)
                                {
                                    res.List[res.List.Count - 1].LatestDirectionAngle = egyro.AzAngle == null ? 0 : NavigationalStatusService.AZangleConvert(egyro.AzAngle.Value);
                                }
                            }
                        }
                    }
                    else
                    {
                        res.SetError($"尚未有时间范围内的卫星定位信息记录:({startTime},{endTime}]");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(SatellteInfoService));
                res.SetError(ex);
            }
            return res;
        }

        /// <summary>
        /// 获取历史信息（去重版本）
        /// </summary>
        public ResponseBase<SatellteInfo> GetHistoryListV2(DateTime? startTime = null, DateTime? endTime = null, int size = 0)
        {
            var res = new ResponseBase<SatellteInfo>();
            try
            {
                if (size == 0) size = LimitValues.SatelltePointSize;
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var satRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_SATELLITE>();
                    startTime = startTime ?? DateTime.MinValue;
                    endTime = endTime ?? DateTime.Now;

                    var list = satRepo.FindAll()
                        .Where(a => a.IsDelete != true
                            && a.CreateTime > startTime
                            && a.CreateTime <= endTime)
                        .OrderBy(a => a.CreateTime)
                        .GroupBy(a => new { a.Lon, a.Lat, a.Altitude })
                        .Select(a => new { a.Key.Lon, a.Key.Lat, a.Key.Altitude, CreateTime = a.Max(g => g.CreateTime) })
                        .ToList();

                    if (startTime == DateTime.MinValue)
                    {
                        // 若开始时间未设置，则包含项目起点
                        var basicRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_Basic>();
                        var prjInfo = basicRepo.FindAll().FirstOrDefault(a => a.IsDelete != true);
                        if (prjInfo != null)
                        {
                            var item = new SatellteInfo()
                            {
                                Longitude = prjInfo.StartLon ?? 0,
                                Latitude = prjInfo.StartLat ?? 0,
                                UpdateTime = startTime ?? DateTime.MinValue
                            };
                            res.List.Add(item);
                        }
                    }
                    if (list != null && list.Count > 0)
                    {
                        // 为0时不做稀释
                        if (size > 0)
                        {
                            list = Utils.GetUniformSample(list, size);
                        }
                        foreach (var data in list)
                        {
                            var item = new SatellteInfo();
                            item.Longitude = data.Lon ?? 0;
                            item.Latitude = data.Lat ?? 0;
                            item.Altitude = data.Altitude ?? 0;
                            item.UpdateTime = data.CreateTime ?? DateTime.Now;
                            res.List.Add(item);
                        }
                        if (res.List.Count > 0)
                        {
                            var egyroRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_EGYRO>();
                            var egyro = egyroRepo.FindAll().OrderByDescending(a => a.CreateTime).FirstOrDefault();
                            if (egyro != null)
                            {
                                res.List[res.List.Count - 1].LatestDirectionAngle = egyro.AzAngle ?? 0;
                            }
                        }
                    }
                    else
                    {
                        res.SetError($"尚未有时间范围内的卫星定位信息记录:({startTime},{endTime}]");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(SatellteInfoService));
                res.SetError(ex);
            }
            return res;
        }
    }
}
