using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Model.Tech.Cloud;
using Model.Tech.Cloud.Webgis;
using Tool;

namespace Service.Wind.TimeWorker
{
    public class WeatherWorkerService
    {
        private static readonly string ShiPXYKey = AppSettingUtils.GetSetting("AppSettings:PXY:PXYKeyExt") ?? "";

        private readonly CloudWind_KingBase_UnitOfWorkFactory _cloudWindKingBaseUowFactory;

        public WeatherWorkerService(CloudWind_KingBase_UnitOfWorkFactory cloudWindKingBaseUowFactory)
        {
            _cloudWindKingBaseUowFactory = cloudWindKingBaseUowFactory;
        }

        /// <summary>
        /// 获取指定经纬度的气象数据
        /// </summary>
        public async Task<(PXYWeather data, string msg)> GetWeatherByPoint(string lon, string lat)
        {
            PXYWeather se = new PXYWeather();
            string msg = "";

            try
            {
                var PostUrl = @"http://api.shipxy.com/apicall/GetWeatherByPoint?k=" + ShiPXYKey + "&lon=" + lon + "&lat=" + lat;
                string JsonStr = await HttpUtils.GetAsync(PostUrl);
                var result = JsonUtils.Deserialize<PXYRet<List<PXYWeatherJson>>>(JsonStr);

                if (result.status == 0 && result.data != null && result.data.Count > 0)
                {
                    var item = result.data[0];

                    se.bm500 = item.bm500 < -30000 || item.bm500 > 30000 ? "" : item.bm500.ToString();
                    se.humidity = item.humidity < -30000 || item.humidity > 30000 ? "" : item.humidity.ToString();
                    se.pressure = item.pressure < -30000 || item.pressure > 30000 ? "" : item.pressure.ToString();
                    se.swelldir = item.swelldir < -30000 || item.swelldir > 30000 ? "" : item.swelldir.ToString();
                    se.swellheight = item.swellheight < -30000 || item.swellheight > 30000 ? "" : item.swellheight.ToString();
                    se.swellperiod = item.swellperiod < -30000 || item.swellperiod > 30000 ? "" : item.swellperiod.ToString();
                    se.temperature = item.temperature < -30000 || item.temperature > 30000 ? "" : item.temperature.ToString();
                    se.visibility = item.visibility < -30000 || item.visibility > 30000 ? "" : item.visibility.ToString();
                    se.waveheight = item.waveheight < -30000 || item.waveheight > 30000 ? "" : item.waveheight.ToString();
                    se.winddir = item.winddir < -30000 || item.winddir > 30000 ? "" : item.winddir.ToString();
                    se.windspeed = item.windspeed < -30000 || item.windspeed > 30000 ? "" : item.windspeed.ToString();
                    //划分风向
                    se.winddirStr = ConvertDirection(item.winddir.ToString());
                    //划分风级
                    var WindSpeedLevel = ConvertWeatherWindLevel(se.windspeed);
                    se.windlevel = WindSpeedLevel.WindSpeedLevel;
                    se.windlevelStr = WindSpeedLevel.WindSpeedLevelStr;
                    se.windwarninglevel = WindSpeedLevel.WindSpeedWarningLevel;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WeatherWorkerService));
                se = new PXYWeather();
                msg = "发生错误";
            }

            return (se, msg);
        }

        /// <summary>
        /// 定时存储船讯网气象数据
        /// </summary>
        public async Task SavePXYWeatherByPoint()
        {
            try
            {
                using (var uow = _cloudWindKingBaseUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<View_Wind_TaskInfo_QXSJ>();
                    var points = taskRepo.Find(a =>
                        a.FlowStatus == (int)CloudWindTaskFlowStatus.完成 &&
                        a.SoftwareID == (int)CloudWindSoftware.海况与气象数据收集
                    ).ToList();

                    if (points.Count == 0)
                    {
                        return;
                    }

                    List<Data_PXYWeather> addLists = new List<Data_PXYWeather>();
                    var now = DateTime.Now;

                    foreach (var item in points)
                    {
                        if (string.IsNullOrEmpty(item.Points))
                        {
                            continue;
                        }

                        //1号_122.12345_31.554,2号_123.554_30.5544
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

                        if (now >= item.CollectEndTime || now <= item.CollectStartTime)
                        {
                            continue;
                        }

                        foreach (var point in list)
                        {
                            if (string.IsNullOrEmpty(point.PointName) || point.PointLon == null || point.PointLat == null)
                            {
                                continue;
                            }

                            var (data, msg) = await GetWeatherByPoint(point.PointLon.ToString(), point.PointLat.ToString());
                            if (!string.IsNullOrEmpty(msg))
                            {
                                LoggerUtils.Error($"获取气象数据失败，点位: {point.PointName}, 错误: {msg}", typeof(WeatherWorkerService));
                                continue;
                            }

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
                            se.time = now;
                            addLists.Add(se);
                        }
                    }

                    if (addLists.Count > 0)
                    {
                        var weatherRepo = uow.GetRepository<Data_PXYWeather>();
                        weatherRepo.AddList(addLists);
                        uow.Save();
                        LoggerUtils.Info($"气象数据保存成功，共 {addLists.Count} 条", typeof(WeatherWorkerService));
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WeatherWorkerService));
            }
        }

        #region 私有工具方法

        private static string ConvertDirection(string DirectionValue)
        {
            string ret = string.Empty;
            if (string.IsNullOrEmpty(DirectionValue))
            {
                ret = "未知";
            }
            else
            {
                double Value = Convert.ToDouble(DirectionValue);
                if (Value < 0 || Value > 360)
                {
                    ret = "未知";
                }
                else if (Value >= 67.5 && Value < 112.5) { ret = "东"; }
                else if (Value >= 112.5 && Value < 157.5) { ret = "东南"; }
                else if (Value >= 157.5 && Value < 202.5) { ret = "南"; }
                else if (Value >= 202.5 && Value < 247.5) { ret = "西南"; }
                else if (Value >= 247.5 && Value < 292.5) { ret = "西"; }
                else if (Value >= 292.5 && Value < 337.5) { ret = "西北"; }
                else if (Value >= 337.5 || Value < 22.5) { ret = "北"; }
                else if (Value >= 22.5 && Value < 67.5) { ret = "东北"; }
                else { ret = "未知"; }
            }
            return ret;
        }

        private static WeatherWindSpeedLevel ConvertWeatherWindLevel(string WindSpeedValue)
        {
            WeatherWindSpeedLevel ret = new WeatherWindSpeedLevel();
            if (string.IsNullOrEmpty(WindSpeedValue))
            {
                ret.WindSpeedLevel = "";
                ret.WindSpeedLevelStr = "";
                ret.WindSpeedWarningLevel = "";
            }
            else
            {
                var WindSpeed = Convert.ToDouble(WindSpeedValue);
                if (WindSpeed >= 0 && WindSpeed <= 0.2) { ret.WindSpeedLevel = ((int)WindSpeedEnum.无风).ToString(); ret.WindSpeedLevelStr = WindSpeedEnum.无风.ToString(); ret.WindSpeedWarningLevel = "1"; }
                else if (WindSpeed > 0.2 && WindSpeed <= 1.5) { ret.WindSpeedLevel = ((int)WindSpeedEnum.软风).ToString(); ret.WindSpeedLevelStr = WindSpeedEnum.软风.ToString(); ret.WindSpeedWarningLevel = "1"; }
                else if (WindSpeed > 1.5 && WindSpeed <= 3.3) { ret.WindSpeedLevel = ((int)WindSpeedEnum.轻风).ToString(); ret.WindSpeedLevelStr = WindSpeedEnum.轻风.ToString(); ret.WindSpeedWarningLevel = "1"; }
                else if (WindSpeed > 3.3 && WindSpeed <= 5.4) { ret.WindSpeedLevel = ((int)WindSpeedEnum.微风).ToString(); ret.WindSpeedLevelStr = WindSpeedEnum.微风.ToString(); ret.WindSpeedWarningLevel = "1"; }
                else if (WindSpeed > 5.4 && WindSpeed <= 7.9) { ret.WindSpeedLevel = ((int)WindSpeedEnum.和风).ToString(); ret.WindSpeedLevelStr = WindSpeedEnum.和风.ToString(); ret.WindSpeedWarningLevel = "1"; }
                else if (WindSpeed > 7.9 && WindSpeed <= 10.7) { ret.WindSpeedLevel = ((int)WindSpeedEnum.劲风).ToString(); ret.WindSpeedLevelStr = WindSpeedEnum.劲风.ToString(); ret.WindSpeedWarningLevel = "1"; }
                else if (WindSpeed > 10.7 && WindSpeed <= 13.8) { ret.WindSpeedLevel = ((int)WindSpeedEnum.强风).ToString(); ret.WindSpeedLevelStr = WindSpeedEnum.强风.ToString(); ret.WindSpeedWarningLevel = "1"; }
                else if (WindSpeed > 13.8 && WindSpeed <= 17.1) { ret.WindSpeedLevel = ((int)WindSpeedEnum.疾风).ToString(); ret.WindSpeedLevelStr = WindSpeedEnum.疾风.ToString(); ret.WindSpeedWarningLevel = "2"; }
                else if (WindSpeed > 17.1 && WindSpeed <= 20.7) { ret.WindSpeedLevel = ((int)WindSpeedEnum.大风).ToString(); ret.WindSpeedLevelStr = WindSpeedEnum.大风.ToString(); ret.WindSpeedWarningLevel = "2"; }
                else if (WindSpeed > 20.7 && WindSpeed <= 24.4) { ret.WindSpeedLevel = ((int)WindSpeedEnum.烈风).ToString(); ret.WindSpeedLevelStr = WindSpeedEnum.烈风.ToString(); ret.WindSpeedWarningLevel = "2"; }
                else if (WindSpeed > 24.4 && WindSpeed <= 28.4) { ret.WindSpeedLevel = ((int)WindSpeedEnum.狂风).ToString(); ret.WindSpeedLevelStr = WindSpeedEnum.狂风.ToString(); ret.WindSpeedWarningLevel = "2"; }
                else if (WindSpeed > 28.4 && WindSpeed <= 32.6) { ret.WindSpeedLevel = ((int)WindSpeedEnum.暴风).ToString(); ret.WindSpeedLevelStr = WindSpeedEnum.暴风.ToString(); ret.WindSpeedWarningLevel = "3"; }
                else if (WindSpeed > 32.6 && WindSpeed <= 41.5) { ret.WindSpeedLevel = ((int)WindSpeedEnum.台风).ToString(); ret.WindSpeedLevelStr = WindSpeedEnum.台风.ToString(); ret.WindSpeedWarningLevel = "3"; }
                else if (WindSpeed > 41.5) { ret.WindSpeedLevel = ((int)WindSpeedEnum.超强台风).ToString(); ret.WindSpeedLevelStr = WindSpeedEnum.超强台风.ToString(); ret.WindSpeedWarningLevel = "3"; }
                else { ret.WindSpeedLevel = ""; ret.WindSpeedLevelStr = ""; ret.WindSpeedWarningLevel = ""; }
            }
            return ret;
        }

        #endregion
    }
}
