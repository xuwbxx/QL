using Business.Towing;
using DataFactory.Factory;
using Model.Towing;
using Tool;

namespace Service.Towing
{
    public class PXYShipService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        private static string ShiPXYBasicKey = AppSettingUtils.GetSetting("PXY:PXYKey") ?? "";
        private static string ShiPXYKey = AppSettingUtils.GetSetting("PXY:PXYKeyExt") ?? "";
        private static string ShiPXYHttpUrl = AppSettingUtils.GetSetting("PXY:PXYHttpUrl") ?? "";

        public PXYShipService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        #region 船讯网

        public static List<PXYShipInfo> GetPXYShips(List<PXYShipInfo> ships, out string msg)
        {
            List<PXYShipInfo> list = new List<PXYShipInfo>();
            msg = "";
            try
            {
                if (ships == null || ships.Count == 0)
                {
                    msg = "船舶数据为空";
                    return list;
                }
                int PostCount = 20;
                var ArrayCount = Math.Ceiling(Convert.ToDecimal(ships.Count) / Convert.ToDecimal(PostCount));
                for (int i = 0; i < ArrayCount; i++)
                {
                    int FirstIndex = i * PostCount;
                    int LastIndex = (FirstIndex + PostCount) > ships.Count ? ships.Count : (FirstIndex + PostCount);

                    string TempShipIDs = "";
                    for (int j = FirstIndex; j < LastIndex; j++)
                    {
                        TempShipIDs += ships[j].MMSI + ",";
                    }

                    var PostUrl = ShiPXYHttpUrl + @"GetManyShip?v=2&k=" + ShiPXYKey + "&enc=1&id=" + TempShipIDs.Trim(',');

                    string JsonStr = HttpUtils.GetSync(PostUrl);

                    var result = JsonUtils.Deserialize<PXYRet<List<PXYShipInfoJson>>>(JsonStr);
                    if (result.status == 0 && result.data != null && result.data.Count > 0)
                    {
                        ships.ForEach(a =>
                        {
                            long MMSI = Convert.ToInt64(a.MMSI);
                            var Ship = result.data.FirstOrDefault(b => b.mmsi == MMSI);
                            if (Ship != null)
                            {
                                PXYShipInfo se = new PXYShipInfo();
                                se.ID = a.ID;
                                se.ShipName = a.ShipName;
                                se.ShipShortName = Ship.name;
                                se.ShipType = a.ShipType ?? "";
                                se.MMSI = a.MMSI;
                                se.CallSign = Ship.callsign;
                                se.Dest = Ship.dest;
                                se.LastTime = DateTimeOffset.FromUnixTimeSeconds(Ship.lasttime).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                                se.Lat = ConvertLat(Ship.lat);
                                se.LatValue = Math.Round(Convert.ToDouble(Ship.lat) / 1000000, 6);
                                se.Lon = ConvertLon(Ship.lon);
                                se.LonValue = Math.Round(Convert.ToDouble(Ship.lon) / 1000000, 6);

                                list.Add(se);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PXYShipService));
                list = new List<PXYShipInfo>();
                msg = "发生错误";
            }
            return list;
        }

        /// <summary>
        /// 查询单艘船信息
        /// </summary>
        public static PXYShipInfo GetSingleShip(string ShipID)
        {
            PXYShipInfo se = new PXYShipInfo();
            try
            {
                var PostUrl = @"http://api.shipxy.com/apicall/GetSingleShip?v=2&k=" + ShiPXYBasicKey + "&enc=1&id=" + ShipID;
                string JsonStr = HttpUtils.GetSync(PostUrl);
                var result = JsonUtils.Deserialize<PXYRet<List<PXYShipInfoJson>>>(JsonStr);
                if (result.status == 0 && result.data != null && result.data.Count > 0)
                {
                    var item = result.data[0];
                    se.ShipName = item.name;
                    se.MMSI = item.mmsi.ToString();
                    se.CallSign = item.callsign;
                    se.Draught = Math.Round(Convert.ToDouble(item.draught / 1000), 2);
                    se.Dest = item.dest;
                    se.Eta = item.eta_std;
                    se.IMO = item.imo;
                    se.LastTime = item.lasttime.ToString();
                    se.Lat = ConvertLat(item.lat);
                    se.LatValue = Math.Round(Convert.ToDouble(item.lat) / 1000000, 6);
                    se.Lon = ConvertLon(item.lon);
                    se.LonValue = Math.Round(Convert.ToDouble(item.lon) / 1000000, 6);
                    se.Left = Math.Round(Convert.ToDouble(item.left) / 10, 2);
                    se.Length = Math.Round(Convert.ToDouble(item.length) / 10, 2);
                    se.Width = Math.Round(Convert.ToDouble(item.width) / 10, 2);
                    se.Trail = Math.Round(Convert.ToDouble(item.trail) / 10, 2);
                    se.NaviStat = ConvertShipStat(item.navistat);
                    se.Rot = Math.Round(Convert.ToDouble(item.rot) / 100, 2);
                    se.ShipType = ConvertShipType(item.shiptype);
                    se.Speed1 = Math.Round(Convert.ToDouble(item.sog) * 3600 / 1000000, 3);
                    se.Speed2 = Math.Round(Convert.ToDouble(item.sog) * 3600 / 1000000 / 1.852, 3);
                    se.ShipDegreeValue = item.hdg > 35900 ? 0 : (item.hdg / 100);
                }
                else
                {
                    se = null;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PXYShipService));
                se = null;
            }
            return se;
        }

        /// <summary>
        /// 获取区域内船舶
        /// </summary>
        public static ResponseBase<PXYShipInfo> GetAreaShips(string area)
        {
            var res = new ResponseBase<PXYShipInfo>();
            try
            {
                var PostUrl = $"http://api.shipxy.com/apicall/GetAreaShip?v=2&k={ShiPXYKey}&enc=1&jsf=&scode=0&xy={area}";
                string JsonStr = HttpUtils.GetSync(PostUrl);
                var result = JsonUtils.Deserialize<PXYRet<List<PXYShipInfoJson>>>(JsonStr);
                if (result.status == 0 && result.data != null && result.data.Count > 0)
                {
                    var se = new PXYShipInfo();
                    var item = result.data[0];
                    se.ShipName = item.name;
                    se.MMSI = item.mmsi.ToString();
                    se.CallSign = item.callsign;
                    se.Draught = Math.Round(Convert.ToDouble(item.draught / 1000), 2);
                    se.Dest = item.dest;
                    se.Eta = item.eta_std;
                    se.IMO = item.imo;
                    se.LastTime = item.lasttime.ToString();
                    se.Lat = ConvertLat(item.lat);
                    se.LatValue = Math.Round(Convert.ToDouble(item.lat) / 1000000, 6);
                    se.Lon = ConvertLon(item.lon);
                    se.LonValue = Math.Round(Convert.ToDouble(item.lon) / 1000000, 6);
                    se.Left = Math.Round(Convert.ToDouble(item.left) / 10, 2);
                    se.Length = Math.Round(Convert.ToDouble(item.length) / 10, 2);
                    se.Width = Math.Round(Convert.ToDouble(item.width) / 10, 2);
                    se.Trail = Math.Round(Convert.ToDouble(item.trail) / 10, 2);
                    se.NaviStat = ConvertShipStat(item.navistat);
                    se.Rot = Math.Round(Convert.ToDouble(item.rot) / 100, 2);
                    se.ShipType = ConvertShipType(item.shiptype);
                    se.Speed1 = Math.Round(Convert.ToDouble(item.sog) * 3600 / 1000000, 3);
                    se.Speed2 = Math.Round(Convert.ToDouble(item.sog) * 3600 / 1000000 / 1.852, 3);
                    se.ShipDegreeValue = item.hdg > 35900 ? 0 : (item.hdg / 100);

                    res.List.Add(se);
                }
                else
                {
                    res = null;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PXYShipService));
                res = null;
            }
            return res;
        }

        /// <summary>
        /// 查询轨迹
        /// </summary>
        public static List<PXYShipTrack> GetShipTrack(string ShipID, string StartTime, string EndTime)
        {
            List<PXYShipTrack> list = new List<PXYShipTrack>();
            try
            {
                var PostUrl = ShiPXYHttpUrl + @"GetShipTrack?v=2&k=" + ShiPXYKey + "&enc=1&cut=1&id=" + ShipID + "&btm=" + StartTime + "&etm=" + EndTime;
                string JsonStr = HttpUtils.GetSync(PostUrl);
                var result = JsonUtils.Deserialize<PXYRetTrack<List<PXYShipTrackJson>>>(JsonStr);
                if (result.status == 0 && result.points != null && result.points.Count > 0)
                {
                    foreach (var item in result.points)
                    {
                        PXYShipTrack se = new PXYShipTrack();
                        se.Lat = ConvertLat(item.lat);
                        se.LatValue = Math.Round(Convert.ToDouble(item.lat) / 1000000, 6);
                        se.Lon = ConvertLon(item.lon);
                        se.LonValue = Math.Round(Convert.ToDouble(item.lon) / 1000000, 6);
                        se.Speed = Math.Round(Convert.ToDouble(item.sog) * 3600 / 1000000 / 1.852, 3);
                        se.Cog = Math.Round(Convert.ToDouble(item.cog) / 100, 1);
                        se.Time = item.utc.ToString();
                        list.Add(se);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PXYShipService));
                list = new List<PXYShipTrack>();
            }
            return list;
        }

        /// <summary>
        /// 获取周边船舶信息
        /// </summary>
        public static ResponseBase<PXYShipInfo> GetSurRoudingShips(string mmsi)
        {
            var enc = "1"; // 0: base64, 1: json
            var action = $"http://api.shipxy.com/apicall/GetSurRoundingShip?k={ShiPXYKey}&mmsi={mmsi}&enc={enc}";
            var res = new ResponseBase<PXYShipInfo>();
            try
            {
                string JsonStr = HttpUtils.GetSync(action);
                var result = JsonUtils.Deserialize<PXYRet<List<PXYShipInfoSurroundJson>>>(JsonStr);
                if (result.status == 0 && result.data != null && result.data.Count > 0)
                {
                    foreach (var item in result.data)
                    {
                        var se = new PXYShipInfo();
                        se.ShipName = item.name;
                        se.MMSI = item.mmsi.ToString();
                        se.CallSign = item.callsign;
                        se.Draught = Math.Round(Convert.ToDouble(item.draught / 1000), 2);
                        se.Dest = item.dest;
                        se.Eta = item.eta_std;
                        se.IMO = item.imo;
                        se.LastTime = item.lasttime.ToString();
                        se.Lat = ConvertLat(item.lat);
                        se.LatValue = Math.Round(Convert.ToDouble(item.lat) / 1000000, 6);
                        se.Lon = ConvertLon(item.lon);
                        se.LonValue = Math.Round(Convert.ToDouble(item.lon) / 1000000, 6);
                        se.Left = Math.Round(Convert.ToDouble(item.left) / 10, 2);
                        se.Length = Math.Round(Convert.ToDouble(item.length) / 10, 2);
                        se.Width = Math.Round(Convert.ToDouble(item.width) / 10, 2);
                        se.Trail = Math.Round(Convert.ToDouble(item.trail) / 10, 2);
                        se.NaviStat = ConvertShipStat(item.navistat);
                        se.Rot = Math.Round(Convert.ToDouble(item.rot) / 100, 2);
                        se.ShipType = ConvertShipType(item.shiptype);
                        se.Speed1 = Math.Round(Convert.ToDouble(item.sog) * 3600 / 1000000, 3);
                        se.Speed2 = Math.Round(Convert.ToDouble(item.sog) * 3600 / 1000000 / 1.852, 3);
                        se.ShipDegreeValue = item.hdg > 35900 ? 0 : (item.hdg / 100);

                        res.List.Add(se);
                    }
                    res.SetOK();
                }
                else
                {
                    res.SetError("未查询到船舶");
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PXYShipService));
                res.SetError(ex);
            }
            return res;
        }

        /// <summary>
        /// 获取预报时间
        /// </summary>
        public static List<string> GetWeatherTime()
        {
            List<string> list = new List<string>();
            try
            {
                var PostUrl = ShiPXYHttpUrl + @"GetWeatherTime?k=" + ShiPXYKey;
                string JsonStr = HttpUtils.GetSync(PostUrl);
                var result = JsonUtils.Deserialize<PXYRet<List<string>>>(JsonStr);
                if (result.status == 0)
                {
                    list = result.data;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PXYShipService));
                list = new List<string>();
            }
            return list;
        }

        public ResponseBase<HFWeatherInfo> GetWeatherForecastList(string lon, string lat)
        {
            var res = new ResponseBase<HFWeatherInfo>();
            try
            {
                var weatherTime = GetWeatherTime();
                // 若未传入，则获取船舶当前位置
                if (string.IsNullOrEmpty(lon) || string.IsNullOrEmpty(lat))
                {
                    using (var uow = _towingKingBaseUowFactory.Create())
                    {
                        var satRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_SATELLITE>();
                        var satelliteInfo = satRepo.FindAll().OrderByDescending(a => a.CreateTime).FirstOrDefault();
                        if (satelliteInfo != null)
                        {
                            lon = (satelliteInfo.Lon ?? 0).ToString();
                            lat = (satelliteInfo.Lat ?? 0).ToString();
                        }
                        if (string.IsNullOrEmpty(lon) || string.IsNullOrEmpty(lat)
                            || lon == "0" || lat == "0")
                        {
                            res.SetError("未输入经纬度");
                            return res;
                        }
                    }
                }
                foreach (var wt in weatherTime)
                {
                    var PostUrl = $"http://api.shipxy.com/apicall/GetWeatherByPoint?k=" + ShiPXYKey + "&lon=" + lon + "&lat=" + lat + "&time=" + wt;
                    string JsonStr = HttpUtils.GetSync(PostUrl);
                    var result = JsonUtils.Deserialize<PXYRet<List<PXYWeatherJson>>>(JsonStr);
                    if (result.status == 0 && result.data != null && result.data.Count > 0)
                    {
                        foreach (var item in result.data)
                        {
                            var se = HFWeatherFromJson(item);
                            se.Time = wt;
                            se.Date = wt.Split(' ')[0];
                            se.ShortTime = wt.Split(' ')[1];
                            res.List.Add(se);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PXYShipService));
                res.SetError(ex);
            }
            return res;
        }

        /// <summary>
        /// 天气信息格式化
        /// </summary>
        public static PXYWeather WeatherFromJson(PXYWeatherJson item)
        {
            var se = new PXYWeather();
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
            se.winddirStr = ConvertDirection(se.winddirStr);
            var WindSpeedLevel = ConvertWeatherWindLevel(se.windspeed);
            se.windlevel = WindSpeedLevel.WindSpeedLevel;
            se.windlevelStr = WindSpeedLevel.WindSpeedLevelStr;
            se.windwarninglevel = WindSpeedLevel.WindSpeedWarningLevel;
            return se;
        }

        /// <summary>
        /// 天气信息格式化
        /// </summary>
        public static HFWeatherInfo HFWeatherFromJson(PXYWeatherJson data)
        {
            var se = new HFWeatherInfo();
            se.Bm500 = data.bm500 == -32767 ? "" : data.bm500.ToString();
            se.Humidity = data.humidity == -32767 ? "" : data.humidity.ToString();
            se.OceanDir = data.oceandir == -32767 ? "" : data.oceandir.ToString();
            se.OceanSpeed = data.oceanspeed == -32767 ? "" : data.oceanspeed.ToString();
            se.Pressure = data.pressure == -32767 ? "" : data.pressure.ToString();
            se.SwellDir = data.swelldir == -32767 ? "" : data.swelldir.ToString();
            se.SwellHeight = data.swellheight == -32767 ? "" : data.swellheight.ToString();
            se.SwellPeriod = data.swellperiod == -32767 ? "" : data.swellperiod.ToString();
            se.Temperature = data.temperature == -32767 ? "" : data.temperature.ToString();
            se.Visibility = (data.visibility == -32767 || data.visibility > 1000) ? "" : data.visibility.ToString();
            se.WaveHeight = data.waveheight == -32767 ? "" : data.waveheight.ToString();
            se.WindDir = data.winddir == -32767 ? "" : data.winddir.ToString();
            se.WindSpeed = data.windspeed == -32767 ? "" : data.windspeed.ToString();
            se.WindDirStr = ConvertDirection(se.WindDir);
            se.OceanDirStr = ConvertDirection(se.OceanDir);

            var WindSpeedLevel = ConvertWeatherWindLevel(se.WindSpeed);
            se.WindSpeedLevel = WindSpeedLevel.WindSpeedLevel;
            se.WindSpeedLevelStr = WindSpeedLevel.WindSpeedLevelStr;
            se.WindSpeedWarningLevel = WindSpeedLevel.WindSpeedWarningLevel;

            var SwellLevel = ConvertWeatherSwellLevel(se.SwellHeight);
            se.SwellLevel = SwellLevel.SwellLevel;
            se.SwellLevelStr = SwellLevel.SwellLevelStr;
            se.SwellWarningLevel = SwellLevel.SwellWarningLevel;
            return se;
        }

        /// <summary>
        /// 获取某经纬度的气象信息
        /// </summary>
        public static PXYWeather GetWeatherByPoint(string lon, string lat)
        {
            PXYWeather se = new PXYWeather();
            try
            {
                var PostUrl = @"http://api.shipxy.com/apicall/GetWeatherByPoint?k=" + ShiPXYBasicKey + "&lon=" + lon + "&lat=" + lat;
                string JsonStr = HttpUtils.GetSync(PostUrl);
                var result = JsonUtils.Deserialize<PXYRet<List<PXYWeatherJson>>>(JsonStr);
                if (result.status == 0 && result.data != null && result.data.Count > 0)
                {
                    var item = result.data[0];
                    se = WeatherFromJson(item);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PXYShipService));
                se = new PXYWeather();
            }
            return se;
        }

        /// <summary>
        /// 获取某经纬度的预报气象信息
        /// </summary>
        public static HFWeatherInfo GetPreWeatherByPoint(string lon, string lat, string time)
        {
            HFWeatherInfo se = new HFWeatherInfo();
            try
            {
                if (string.IsNullOrEmpty(time) || time.Split(' ').Length != 2)
                {
                    time = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");
                }
                var PostUrl = @"http://api.shipxy.com/apicall/GetWeatherByPoint?k=" + ShiPXYKey + "&time=" + time + "&lon=" + lon + "&lat=" + lat;
                string JsonStr = HttpUtils.GetSync(PostUrl);
                var result = JsonUtils.Deserialize<PXYRet<List<PXYWeatherJson>>>(JsonStr);
                if (result.status == 0 && result.data != null && result.data.Count > 0)
                {
                    var data = result.data[0];
                    se.Time = time;
                    se.Date = time.Split(' ')[0];
                    se.ShortTime = time.Split(' ')[1];
                    se.Bm500 = data.bm500 == -32767 ? "" : data.bm500.ToString();
                    se.Humidity = data.humidity == -32767 ? "" : data.humidity.ToString();
                    se.OceanDir = data.oceandir == -32767 ? "" : data.oceandir.ToString();
                    se.OceanSpeed = data.oceanspeed == -32767 ? "" : data.oceanspeed.ToString();
                    se.Pressure = data.pressure == -32767 ? "" : data.pressure.ToString();
                    se.SwellDir = data.swelldir == -32767 ? "" : data.swelldir.ToString();
                    se.SwellHeight = data.swellheight == -32767 ? "" : data.swellheight.ToString();
                    se.SwellPeriod = data.swellperiod == -32767 ? "" : data.swellperiod.ToString();
                    se.Temperature = data.temperature == -32767 ? "" : data.temperature.ToString();
                    se.Visibility = (data.visibility == -32767 || data.visibility > 1000) ? "" : data.visibility.ToString();
                    se.WaveHeight = data.waveheight == -32767 ? "" : data.waveheight.ToString();
                    se.WindDir = data.winddir == -32767 ? "" : data.winddir.ToString();
                    se.WindSpeed = data.windspeed == -32767 ? "" : data.windspeed.ToString();
                    se.WindDirStr = ConvertDirection(se.WindDir);
                    se.OceanDirStr = ConvertDirection(se.OceanDir);

                    var WindSpeedLevel = ConvertWeatherWindLevel(se.WindSpeed);
                    se.WindSpeedLevel = WindSpeedLevel.WindSpeedLevel;
                    se.WindSpeedLevelStr = WindSpeedLevel.WindSpeedLevelStr;
                    se.WindSpeedWarningLevel = WindSpeedLevel.WindSpeedWarningLevel;

                    var SwellLevel = ConvertWeatherSwellLevel(se.SwellHeight);
                    se.SwellLevel = SwellLevel.SwellLevel;
                    se.SwellLevelStr = SwellLevel.SwellLevelStr;
                    se.SwellWarningLevel = SwellLevel.SwellWarningLevel;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PXYShipService));
                se = new HFWeatherInfo();
            }
            return se;
        }

        public static void GetTyphoonList()
        {
            try
            {
                var PostUrl = @"http://api.shipxy.com/apicall/GetTyphoonList?k=" + ShiPXYKey;
                string JsonStr = HttpUtils.GetSync(PostUrl);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PXYShipService));
            }
        }

        public static void GetSingleTyphoon()
        {
            try
            {
                var PostUrl = @"http://api.shipxy.com/apicall/GetSingleTyphoon?k=" + ShiPXYKey + "&tid=2929285";
                string JsonStr = HttpUtils.GetSync(PostUrl);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PXYShipService));
            }
        }

        #endregion

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
                else if (Value >= 337.5 && Value < 22.5) { ret = "北"; }
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

        private static WeatherSwellLevel ConvertWeatherSwellLevel(string SwellValue)
        {
            WeatherSwellLevel ret = new WeatherSwellLevel();
            if (string.IsNullOrEmpty(SwellValue))
            {
                ret.SwellLevel = "";
                ret.SwellLevelStr = "";
                ret.SwellWarningLevel = "";
            }
            else
            {
                var Swell = Convert.ToDouble(SwellValue);
                if (Swell == 0) { ret.SwellLevel = ((int)SwellLevelEnum.无浪).ToString(); ret.SwellLevelStr = SwellLevelEnum.无浪.ToString(); ret.SwellWarningLevel = "1"; }
                else if (Swell > 0 && Swell <= 0.1) { ret.SwellLevel = ((int)SwellLevelEnum.微浪).ToString(); ret.SwellLevelStr = SwellLevelEnum.微浪.ToString(); ret.SwellWarningLevel = "1"; }
                else if (Swell > 0.1 && Swell <= 0.5) { ret.SwellLevel = ((int)SwellLevelEnum.小浪).ToString(); ret.SwellLevelStr = SwellLevelEnum.小浪.ToString(); ret.SwellWarningLevel = "1"; }
                else if (Swell > 0.5 && Swell <= 1.25) { ret.SwellLevel = ((int)SwellLevelEnum.轻浪).ToString(); ret.SwellLevelStr = SwellLevelEnum.轻浪.ToString(); ret.SwellWarningLevel = "1"; }
                else if (Swell > 1.25 && Swell <= 2.5) { ret.SwellLevel = ((int)SwellLevelEnum.中浪).ToString(); ret.SwellLevelStr = SwellLevelEnum.中浪.ToString(); ret.SwellWarningLevel = "1"; }
                else if (Swell > 2.5 && Swell <= 4.0) { ret.SwellLevel = ((int)SwellLevelEnum.大浪).ToString(); ret.SwellLevelStr = SwellLevelEnum.大浪.ToString(); ret.SwellWarningLevel = "2"; }
                else if (Swell > 4.0 && Swell <= 6.0) { ret.SwellLevel = ((int)SwellLevelEnum.巨浪).ToString(); ret.SwellLevelStr = SwellLevelEnum.巨浪.ToString(); ret.SwellWarningLevel = "2"; }
                else if (Swell > 6.0 && Swell <= 9.0) { ret.SwellLevel = ((int)SwellLevelEnum.狂浪).ToString(); ret.SwellLevelStr = SwellLevelEnum.狂浪.ToString(); ret.SwellWarningLevel = "3"; }
                else if (Swell > 9.0 && Swell <= 14.0) { ret.SwellLevel = ((int)SwellLevelEnum.狂涛).ToString(); ret.SwellLevelStr = SwellLevelEnum.狂涛.ToString(); ret.SwellWarningLevel = "3"; }
                else if (Swell > 14.0) { ret.SwellLevel = ((int)SwellLevelEnum.怒涛).ToString(); ret.SwellLevelStr = SwellLevelEnum.怒涛.ToString(); ret.SwellWarningLevel = "3"; }
                else { ret.SwellLevel = ""; ret.SwellLevelStr = ""; ret.SwellWarningLevel = ""; }
            }
            return ret;
        }

        private static string ConvertLat(double lat)
        {
            string latStr = "未知";
            if (lat >= 0) { latStr = "N" + CoorConvert(lat); }
            else { latStr = "S" + CoorConvert(-lat); }
            return latStr;
        }

        private static string ConvertLon(double lon)
        {
            string lonStr = "未知";
            if (lon >= 0) { lonStr = "E" + CoorConvert(lon); }
            else { lonStr = "W" + CoorConvert(-lon); }
            return lonStr;
        }

        public static string CoorConvert(double value)
        {
            try
            {
                value = value / 1000000;
                string degree = value.ToString().Split('.')[0].ToString();
                string minute = "";
                string second = "";

                if (!string.IsNullOrEmpty(value.ToString().Split('.')[1]))
                {
                    double degreeOther = Convert.ToDouble("0." + value.ToString().Split('.')[1].ToString());
                    minute = (degreeOther * 60).ToString().Split('.')[0].ToString();
                    if (!string.IsNullOrEmpty((degreeOther * 60).ToString().Split('.')[1]))
                    {
                        double minuteOther = Convert.ToDouble("0." + (degreeOther * 60).ToString().Split('.')[1].ToString());
                        second = (minuteOther * 60).ToString();
                    }
                }
                return degree + "°" + minute + "'" + second + "\"";
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PXYShipService));
                return "";
            }
        }

        public static double CoorConvertReverse(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            if (!value.Contains("E") && !value.Contains("W") && !value.Contains("S") && !value.Contains("N")) return 0;
            if (!value.Contains("°") || !value.Contains("'") || !value.Contains("\"")) return 0;
            try
            {
                string Symbol = "";
                if (value.Contains("S") || value.Contains("W")) { Symbol = "-"; }

                value = value.Substring(1);
                var SecondStr = value.Substring(value.IndexOf("'") + 1).Replace("\"", "");
                var Second = Convert.ToDouble(SecondStr) / 60;

                var MinuteStr = value.Substring(value.IndexOf("°") + 1, value.IndexOf("'") - value.IndexOf("°") - 1);
                var Minute = (Convert.ToDouble(MinuteStr) + Second) / 60;

                var Degree = Convert.ToDouble(value.Substring(0, value.IndexOf("°")));

                return Convert.ToDouble(Symbol + (Degree + Minute).ToString());
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(PXYShipService));
                return 0;
            }
        }

        private static string ConvertShipType(int type)
        {
            string ShipType = "未知";
            if (type >= 20 && type <= 29) { ShipType = "地效应船"; }
            else if (type >= 40 && type <= 49) { ShipType = "高速船"; }
            else if (type >= 60 && type <= 69) { ShipType = "客船"; }
            else if (type >= 70 && type <= 79) { ShipType = "货船"; }
            else if (type >= 80 && type <= 89) { ShipType = "油轮"; }
            else if (type >= 90 && type <= 99) { ShipType = "其他类型的船舶"; }
            else
            {
                ShipType = Enum.GetName(typeof(PXYShipTypeEnum), type);
                ShipType = ShipType ?? "其他类型的船舶";
            }
            return ShipType;
        }

        private static string ConvertShipStat(int stat)
        {
            string ShipStat = Enum.GetName(typeof(PXYNaviStat), stat);
            ShipStat = ShipStat ?? "未知";
            return ShipStat;
        }

        private static string ConvertShipDegree(double degree)
        {
            string ShipDegree = "ship1";
            if (degree >= 33750 || degree < 2250) { ShipDegree = "ship1"; }
            else if (degree >= 2250 || degree < 6750) { ShipDegree = "ship2"; }
            else if (degree >= 6750 || degree < 11250) { ShipDegree = "ship3"; }
            else if (degree >= 11250 || degree < 15750) { ShipDegree = "ship4"; }
            else if (degree >= 15750 || degree < 20250) { ShipDegree = "ship5"; }
            else if (degree >= 20250 || degree < 24750) { ShipDegree = "ship6"; }
            else if (degree >= 24750 || degree < 29250) { ShipDegree = "ship7"; }
            else if (degree >= 29250 || degree < 33750) { ShipDegree = "ship8"; }
            return ShipDegree;
        }

        #endregion
    }
}
