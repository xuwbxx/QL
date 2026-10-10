using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Model.Tech.Cloud;
using Model.Tech.Cloud.Webgis;
using Service.Base;
using Service.Towing;
using Tool;

namespace Service.Wind
{
    public class CloudWindTyphoonService : WindBaseService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;
        private readonly WebGISService _webGisService;

        public CloudWindTyphoonService(
            CloudWind_KingBase_UnitOfWorkFactory techCenterUowFactory,
            Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory,
            WebGISService webGisService,
            CookieService cookieService)
            : base(techCenterUowFactory, cookieService)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
            _webGisService = webGisService;
        }

        public void SaveTyphoonData()
        {
            string msg = "";
            var list = _webGisService.GetTyphoonList(out msg);

            if (!string.IsNullOrEmpty(msg))
            {
                LoggerUtils.Error(msg, typeof(CloudWindTyphoonService));
            }

            List<DataFactory.KingBase.CloudWind.Data_Typhoon> Typhoons = new List<Data_Typhoon>();

            foreach (var item in list)
            {
                if (!string.IsNullOrEmpty(item.dataMark))
                {
                    string msg2 = "";
                    var data = _webGisService.GetSingleTyphoon(item.xuHao, out msg2);
                    foreach (var item2 in data)
                    {
                        Data_Typhoon se = new Data_Typhoon();
                        se.chnCode = item.chnCode ?? "";
                        se.chnName = item.chnName ?? "";
                        se.currentyear = string.IsNullOrEmpty(item.currentyear) ? default(int?) : int.Parse(item.currentyear);
                        se.xuHao = item.xuHao ?? "";
                        se.time = item2.time ?? "";
                        se.dateTime = item2.dateTime;
                        se.forecast = item2.forecast ?? "";
                        se.fhour = string.IsNullOrEmpty(item2.fhour) ? default(decimal?) : Convert.ToDecimal(item2.fhour);
                        se.lat = string.IsNullOrEmpty(item2.lat) ? default(decimal?) : Convert.ToDecimal(item2.lat);
                        se.lon = string.IsNullOrEmpty(item2.lon) ? default(decimal?) : Convert.ToDecimal(item2.lon);
                        se.grade = string.IsNullOrEmpty(item2.grade) ? default(decimal?) : Convert.ToDecimal(item2.grade);
                        se.mspeed = string.IsNullOrEmpty(item2.mspeed) ? default(decimal?) : Convert.ToDecimal(item2.mspeed);
                        se.pressure = string.IsNullOrEmpty(item2.pressure) ? default(decimal?) : Convert.ToDecimal(item2.pressure);
                        se.kspeed = string.IsNullOrEmpty(item2.kspeed) ? default(decimal?) : Convert.ToDecimal(item2.kspeed);
                        se.radius7 = string.IsNullOrEmpty(item2.radius7) ? default(decimal?) : Convert.ToDecimal(item2.radius7);
                        se.radius10 = string.IsNullOrEmpty(item2.radius10) ? default(decimal?) : Convert.ToDecimal(item2.radius10);
                        se.direction = item2.direction ?? "";
                        se.CreateTime = DateTime.Now;
                        se.IsDelete = false;

                        Typhoons.Add(se);
                    }
                }
            }

            if (Typhoons.Count > 0)
            {
                using var uow = _towingKingBaseUowFactory.Create();
                var repo = uow.GetRepository<Data_Typhoon>();
                repo.AddList(Typhoons);
                uow.Save();
            }
        }

        public void SaveWeatherData()
        {
            try
            {
                // 1.获取船舶经纬度
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var shipRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_Ship>();
                    var Ship = shipRepo.Find(a => !a.IsDelete).FirstOrDefault();
                    if (Ship == null || string.IsNullOrEmpty(Ship.mmsi))
                    {
                        return;
                    }

                    var ShipInfo = PXYShipService.GetSingleShip(Ship.mmsi);
                    if (ShipInfo == null)
                    {
                        return;
                    }

                    // 2.获取当前天气数据
                    var weather = PXYShipService.GetWeatherByPoint(ShipInfo.LonValue.ToString(), ShipInfo.LatValue.ToString());

                    DataFactory.KingBase.Towing.Data_Backup_Weather data = new DataFactory.KingBase.Towing.Data_Backup_Weather();
                    data.bm500 = weather.bm500;
                    data.humidity = weather.humidity;
                    data.pressure = weather.pressure;
                    data.swelldir = weather.swelldir;
                    data.swellheight = weather.swellheight;
                    data.swellperiod = weather.swellperiod;
                    data.temperature = weather.temperature;
                    data.visibility = weather.visibility;
                    data.waveheight = weather.waveheight;
                    data.winddir = weather.winddir;
                    data.winddirStr = weather.winddirStr;
                    data.windspeed = weather.windspeed;
                    data.windlevel = weather.windlevel;
                    data.windlevelStr = weather.windlevelStr;
                    data.windwarninglevel = weather.windwarninglevel;
                    data.windalarm = weather.windalarm;
                    data.time = weather.time;
                    data.timeStr = weather.timeStr;
                    data.oceandir = weather.oceandir;
                    data.oceandirStr = weather.oceandirStr;
                    data.oceanspeed = weather.oceanspeed;
                    data.CreateTime = DateTime.Now;

                    var weatherRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Backup_Weather>();
                    weatherRepo.Add(data);
                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWindTyphoonService));
            }
        }

        /// <summary>
        /// 台风分析
        /// </summary>
        public List<PXYTyphoonAnalyzeModel> TyphoonAnalyze()
        {
            List<PXYTyphoonAnalyzeModel> ret = new List<PXYTyphoonAnalyzeModel>();

            try
            {
                DateTime limitTime = DateTime.Now.AddDays(-3);

                using var towingUow = _towingKingBaseUowFactory.Create();
                var typhoonRepo = towingUow.GetRepository<Data_Typhoon>();
                var TyphoonList = typhoonRepo.Find(a => !a.IsDelete && a.CreateTime >= limitTime)
                    .ToList();

                List<PXYTyphoonAnalyzeModel> list = new List<PXYTyphoonAnalyzeModel>();

                TyphoonList.GroupBy(a => a.xuHao).ToList().ForEach(a =>
                {
                    string ChnCode = a.FirstOrDefault()!.chnCode ?? "";
                    string XuHao = a.FirstOrDefault()!.xuHao ?? "";
                    string ChnName = a.FirstOrDefault()!.chnName ?? "";

                    var SingleTyphoonList = TyphoonList.Where(b => b.xuHao.Equals(XuHao)).ToList();

                    DateTime maxTime = SingleTyphoonList.Max(b => b.dateTime.Value);
                    DateTime maxCreateTime = SingleTyphoonList.Max(b => b.CreateTime.Value);

                    PXYTyphoonAnalyzeModel model = new PXYTyphoonAnalyzeModel();
                    model.chnCode = ChnCode;
                    model.chnName = ChnName;
                    model.xuHao = XuHao;

                    SingleTyphoonList.Where(b => b.dateTime == maxTime && b.CreateTime == maxCreateTime).ToList().ForEach(b =>
                    {
                        // 筛选出预测时间范围之内的数据
                        int hour = b.fhour == null ? 0 : Convert.ToInt32(b.fhour.Value);
                        DateTime ForecastTime = b.dateTime.Value.AddHours(hour);

                        // 筛选出预测时间大于昨天
                        if (ForecastTime >= DateTime.Now.AddDays(-1))
                        {
                            model.Typhoons.Add(new PXYTyphoonInfo()
                            {
                                ID = b.id,
                                forecastTime = ForecastTime,
                                lon = b.lon.Value,
                                lat = b.lat.Value,
                                grade = b.grade.Value,
                                speed = b.mspeed.Value
                            });
                        }
                    });

                    list.Add(model);
                });

                // 分析经纬度是否在区域范围内
                if (list.Count > 0)
                {
                    List<PXYArea> areas = new List<PXYArea>();
                    var areaRepo = towingUow.GetRepository<DataFactory.KingBase.Towing.Manage_Area>();
                    areaRepo.Find(a => !a.IsDelete).ToList().ForEach(a =>
                    {
                        areas.Add(new PXYArea()
                        {
                            Lon = a.Lon ?? 0,
                            Lat = a.Lat ?? 0
                        });
                    });

                    if (areas.Count < 3)
                    {
                        return ret;
                    }

                    list.ForEach(a =>
                    {
                        if (a.Typhoons.Count > 0)
                        {
                            List<PXYTyphoonInfo> models = new List<PXYTyphoonInfo>();
                            a.Typhoons.OrderBy(b => b.forecastTime).ToList().ForEach(b =>
                            {
                                PXYArea point = new PXYArea();
                                point.Lon = b.lon;
                                point.Lat = b.lat;

                                bool IsIn = IsPointInArea(areas, point);

                                if (IsIn)
                                {
                                    models.Add(b);
                                }
                            });

                            if (models.Count > 0)
                            {
                                ret.Add(new PXYTyphoonAnalyzeModel()
                                {
                                    chnCode = a.chnCode,
                                    chnName = a.chnName,
                                    xuHao = a.xuHao,
                                    Typhoons = models
                                });
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWindTyphoonService));
                ret = new List<PXYTyphoonAnalyzeModel>();
            }

            return ret;
        }

        /// <summary>
        /// 判断一个经纬度点是否在多边形区域内
        /// </summary>
        private bool IsPointInArea(List<PXYArea> areaPoints, PXYArea point)
        {
            if (areaPoints == null || areaPoints.Count < 3)
                return false;

            bool isInside = false;
            int count = areaPoints.Count;

            double pointLon = (double)point.Lon;
            double pointLat = (double)point.Lat;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                double iLat = (double)areaPoints[i].Lat;
                double jLat = (double)areaPoints[j].Lat;
                double iLon = (double)areaPoints[i].Lon;
                double jLon = (double)areaPoints[j].Lon;

                if ((iLon == pointLon && iLat == pointLat) ||
                    (jLon == pointLon && jLat == pointLat))
                {
                    return true;
                }

                bool isYInRange = (iLat > pointLat) != (jLat > pointLat);
                if (!isYInRange)
                    continue;

                double xIntersect = (pointLat - iLat) * (jLon - iLon) / (jLat - iLat) + iLon;

                if (pointLon < xIntersect)
                {
                    isInside = !isInside;
                }
            }

            return isInside;
        }

        /// <summary>
        /// 定时存储船讯网气象数据
        /// </summary>
        public void SavePXYWeatherByPoint()
        {
            try
            {
                using var cloudWindUow = _techCenterUowFactory.Create();
                var taskRepo = cloudWindUow.GetRepository<View_Wind_TaskInfo_QXSJ>();
                var points = taskRepo.Find(a => a.FlowStatus == (int)CloudWindTaskFlowStatus.完成 && a.SoftwareID == (int)CloudWindSoftware.海况与气象数据收集)
                    .ToList();

                if (points.Count == 0)
                {
                    return;
                }

                List<Data_PXYWeather> addLists = new List<Data_PXYWeather>();
                foreach (var item in points)
                {
                    if (string.IsNullOrEmpty(item.Points))
                    {
                        continue;
                    }
                    // 1号_122.12345_31.554,2号_123.554_30.5544
                    List<CloudWindCollectPoint_QXSJ> list = new List<CloudWindCollectPoint_QXSJ>();
                    string[] pointArr = item.Points.Split(',');
                    foreach (string str in pointArr)
                    {
                        string pointName = str.Split('_')[0];
                        double pointLon = Convert.ToDouble(str.Split('_')[1]);
                        double pointLat = Convert.ToDouble(str.Split('_')[2]);
                        list.Add(new CloudWindCollectPoint_QXSJ()
                        {
                            PointName = pointName,
                            PointLon = pointLon.ToString(),
                            PointLat = pointLat.ToString()
                        });
                    }
                    if (list.Count == 0)
                    {
                        continue;
                    }
                    if (DateTime.Now >= item.CollectEndTime || DateTime.Now <= item.CollectStartTime)
                    {
                        continue;
                    }

                    foreach (var point in list)
                    {
                        if (string.IsNullOrEmpty(point.PointName) || point.PointLon == null || point.PointLat == null)
                        {
                            continue;
                        }

                        var data = _webGisService.GetWeatherByPoint(point.PointLon.ToString(), point.PointLat.ToString());

                        if (data != null)
                        {
                            Data_PXYWeather se = new Data_PXYWeather();
                            se.TaskID = item.id;
                            se.PointName = point.PointName;
                            se.LonLat = point.PointLon + "-" + point.PointLat;
                            se.bm500 = data.bm500 ?? "";
                            se.humidity = data.humidity ?? "";
                            se.pressure = data.pressure ?? "";
                            se.swelldir = data.swelldir ?? "";
                            se.swellheight = data.swellheight ?? "";
                            se.swellperiod = data.swellperiod ?? "";
                            se.temperature = data.temperature ?? "";
                            se.visibility = data.visibility ?? "";
                            se.waveheight = data.waveheight ?? "";
                            se.winddir = data.winddir ?? "";
                            se.windspeed = data.windspeed ?? "";
                            se.windlevel = data.windlevel ?? "";
                            se.windwarninglevel = data.windwarninglevel ?? "";
                            se.windalarm = data.windalarm;
                            se.time = DateTime.Now;
                            addLists.Add(se);
                        }
                    }
                }

                if (addLists.Count > 0)
                {
                    var weatherRepo = cloudWindUow.GetRepository<Data_PXYWeather>();
                    weatherRepo.AddList(addLists);
                    cloudWindUow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWindTyphoonService));
            }
        }
    }
}
