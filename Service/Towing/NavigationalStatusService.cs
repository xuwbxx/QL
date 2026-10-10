using Business.Towing;
using DataFactory.Factory;
using Tool;

namespace Service.Towing
{
    public class NavigationalStatus
    {
        /// <summary>
        /// 已航行时间
        /// </summary>
        public decimal? SpentTimeHours { set; get; }
        public string SpentTimeHoursStr { set; get; }
        public decimal? SpentDistance { set; get; }

        /// <summary>
        /// 预计剩余航行时间
        /// </summary>
        public decimal? RemainingTimeHours { set; get; }
        public string RemainingTimeHoursStr { set; get; }
        public decimal? RemainingDistance { set; get; }

        /// <summary>
        /// 当前航速
        /// </summary>
        public decimal? Speed { set; get; }
        public string SpeedStr => Utils.getStr(Speed, 2);

        /// <summary>
        /// 实时航向
        /// </summary>
        public decimal? HeadingDegree { set; get; }
        public string HeadingDegreeStr { set; get; }
    }

    public class NavigationalStatusService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        private static int TowingWindAzAngleOffset = Convert.ToInt32(AppSettingUtils.GetSetting("TowingWindAzAngle") ?? "0");

        public NavigationalStatusService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        public static decimal AZangleConvert(decimal value)
        {
            //正东+140数值
            decimal ret = value + TowingWindAzAngleOffset;
            if (ret >= 360)
            {
                ret = ret - 360;
            }
            return ret;
        }

        /// <summary>
        /// 以最近10分钟内为有效，并不是取现有最新的
        /// </summary>
        public ResponseBase<NavigationalStatus> GetInfoV2()
        {
            var res = new ResponseBase<NavigationalStatus>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var satRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_SATELLITE>();
                    var egyroRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_EGYRO>();
                    var basicRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_Basic>();
                    var speedRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_SailSpeed>();
                    var planRouteRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_PlanRoute>();

                    var firstData = satRepo.Find(a => a.IsDelete != true)
                        .OrderBy(a => a.CreateTime)
                        .FirstOrDefault();

                    var SatelliteData = satRepo.Find(a => a.IsDelete != true && a.CreateTime > LimitValues.DeviceOfflineTime)
                        .OrderBy(a => a.CreateTime)
                        .ToList();

                    var lastData = SatelliteData.OrderByDescending(a => a.CreateTime).FirstOrDefault();

                    var lastEGYRO = egyroRepo.Find(a => a.IsDelete != true && a.CreateTime > LimitValues.DeviceOfflineTime)
                        .OrderByDescending(a => a.CreateTime)
                        .FirstOrDefault();

                    var proj = basicRepo.Find(a => a.IsDelete != true).FirstOrDefault();

                    var speed = speedRepo.Find(a => a.IsDelete != true && a.CreateTime > LimitValues.DeviceOfflineTime)
                        .OrderByDescending(a => a.CreateTime)
                        .FirstOrDefault();

                    // 获取最新的卫星定位信息，计算与目标距离，估算预计航行时间
                    res.Data = new NavigationalStatus();

                    if (lastEGYRO != null)
                    {
                        //取最近的两个点
                        if (SatelliteData.Count >= 2)
                        {
                            Location point1 = new Location()
                            {
                                Lon = SatelliteData[SatelliteData.Count - 2].Lon == null ? 0 : SatelliteData[SatelliteData.Count - 2].Lon.Value,
                                Lat = SatelliteData[SatelliteData.Count - 2].Lat == null ? 0 : SatelliteData[SatelliteData.Count - 2].Lat.Value,
                            };
                            Location point2 = new Location()
                            {
                                Lon = SatelliteData[SatelliteData.Count - 1].Lon == null ? 0 : SatelliteData[SatelliteData.Count - 1].Lon.Value,
                                Lat = SatelliteData[SatelliteData.Count - 1].Lat == null ? 0 : SatelliteData[SatelliteData.Count - 1].Lat.Value,
                            };

                            res.Data.HeadingDegree = NavigationCalculator.CalculateBearing(point1, point2);
                        }
                        else
                        {
                            res.Data.HeadingDegree = lastEGYRO.AzAngle == null ? 0 : AZangleConvert(lastEGYRO.AzAngle.Value);
                        }

                        //数值
                        res.Data.HeadingDegreeStr = Utils.getStr(res.Data.HeadingDegree, 2);
                    }

                    if (proj != null && firstData != null && lastData != null)
                    {
                        var PlanRoutes = planRouteRepo.Find(a => !a.IsDelete).ToList();
                        List<Location> PlanLocations = new List<Location>();
                        foreach (var item in PlanRoutes)
                        {
                            PlanLocations.Add(new Location()
                            {
                                Lon = item.Lon.Value,
                                Lat = item.Lat.Value
                            });
                        }
                        Location nowPos = new Location()
                        {
                            Lon = lastData.Lon.Value,
                            Lat = lastData.Lat.Value
                        };

                        //剩余路程
                        decimal remindingPath = NavigationCalculator.CalculateRemainingNauticalMiles(PlanLocations, nowPos);

                        if (speed != null)
                        {
                            res.Data.Speed = speed.SailSpeed ?? 0;

                            // 估算剩余时间
                            var remindingHours = res.Data.Speed == 0 ? null : remindingPath / res.Data.Speed;
                            res.Data.RemainingTimeHours = remindingHours;
                            res.Data.RemainingTimeHoursStr = remindingHours == null ? "-" : (remindingHours >= 999 ? "-" : remindingHours.Value.ToString("F2"));
                        }

                        //剩余航行距离
                        res.Data.RemainingDistance = Math.Round(remindingPath, 2);

                        var timeDiff = lastData.CreateTime - firstData.CreateTime;

                        if (proj.ProjectStartTime >= lastData.CreateTime)
                        {
                            res.Data.SpentTimeHours = null;
                            res.Data.SpentTimeHoursStr = "未开始";
                            res.Data.SpentDistance = 0;
                        }
                        else
                        {
                            res.Data.SpentTimeHours = ((decimal)timeDiff.Value.TotalMinutes / 60);
                            res.Data.SpentTimeHoursStr = Utils.getStr(res.Data.SpentTimeHours, 2);

                            //已航行距离
                            decimal spentPath = NavigationCalculator.CalculateTraveledNauticalMiles(PlanLocations, nowPos);
                            res.Data.SpentDistance = Math.Round(spentPath, 2);
                        }
                    }
                    else
                    {
                        res.SetError("尚未有航行状态信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(NavigationalStatusService));
                res.SetError(ex);
            }
            return res;
        }

        public ResponseBase<EChartBase> GetSpeedChartData(DateTime? startTime = null, DateTime? endTime = null, int size = 0)
        {
            var res = new ResponseBase<EChartBase>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var speedRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_SailSpeed>();
                    if (size == 0) size = LimitValues.EChartDataSize;
                    startTime = startTime ?? DateTime.MinValue;
                    endTime = endTime ?? DateTime.Now;

                    var query = speedRepo.Find(a => a.IsDelete != true
                            && a.CreateTime >= startTime
                            && a.CreateTime < endTime)
                        .OrderByDescending(a => a.CreateTime);

                    List<DataFactory.KingBase.Towing.Data_SailSpeed> list;
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
                            res.Data.AddData("航速",
                                data.CreateTime == null ? "" : data.CreateTime.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                                data.SailSpeed ?? 0);
                        }
                    }
                    else
                    {
                        res.SetError("尚未有船速信息。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(NavigationalStatusService));
                res.SetError(ex);
            }
            return res;
        }
    }
}
