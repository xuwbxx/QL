using System.IO.Compression;
using System.Text;
using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Model.Tech.Cloud;
using Model.Tech.Cloud.Webgis;
using Service.Base;
using Tool;

namespace Service.Wind
{
    public class WebGISService : WindBaseService
    {
        private readonly string _shiPXYKey;
        private readonly string _shiPXYHttpUrl;

        public WebGISService(CloudWind_KingBase_UnitOfWorkFactory techCenterUowFactory, CookieService cookieService)
        : base(techCenterUowFactory, cookieService)
        {
            _shiPXYKey = AppSettingUtils.GetSetting("PXY:PXYKeyExt");
            _shiPXYHttpUrl = AppSettingUtils.GetSetting("PXY:PXYHttpUrl");
        }

        #region 船讯网

        /// <summary>
        /// 批量查询船讯网船舶信息
        /// </summary>
        public List<PXYShipInfo> GetPXYShips(List<PXYShipInfo> ships, out string msg)
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

                    var PostUrl = _shiPXYHttpUrl + @"GetManyShip?v=2&k=" + _shiPXYKey + "&enc=1&id=" + TempShipIDs.Trim(',');

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
                                se.LastTime = DateTimeOffset.FromUnixTimeSeconds(Ship.lasttime).DateTime.ToString("yyyy-MM-dd HH:mm:ss");
                                se.Lat = ConvertLat(Convert.ToDouble(Ship.lat));
                                se.LatValue = Math.Round(Convert.ToDouble(Ship.lat) / 1000000, 6);
                                se.Lon = ConvertLon(Convert.ToDouble(Ship.lon));
                                se.LonValue = Math.Round(Convert.ToDouble(Ship.lon) / 1000000, 6);

                                list.Add(se);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
                list = new List<PXYShipInfo>();
                msg = "发生错误";
            }
            return list;
        }

        /// <summary>
        /// 查询单艘船信息
        /// </summary>
        public PXYShipInfo GetSingleShip(string ShipID)
        {
            PXYShipInfo se = new PXYShipInfo();
            try
            {
                var PostUrl = @"http://api.shipxy.com/apicall/GetSingleShip?v=2&k=" + _shiPXYKey + "&enc=1&id=" + ShipID;
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
                    se.Lat = ConvertLat(Convert.ToDouble(item.lat));
                    se.LatValue = Math.Round(Convert.ToDouble(item.lat) / 1000000, 6);
                    se.Lon = ConvertLon(Convert.ToDouble(item.lon));
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
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
                se = null;
            }
            return se;
        }

        /// <summary>
        /// 查询轨迹
        /// </summary>
        public List<PXYShipTrack> GetShipTrack(string ShipID, string StartTime, string EndTime)
        {
            List<PXYShipTrack> list = new List<PXYShipTrack>();
            try
            {
                var PostUrl = _shiPXYHttpUrl + @"GetShipTrack?v=2&k=" + _shiPXYKey + "&enc=1&cut=1&id=" + ShipID + "&btm=" + StartTime + "&etm=" + EndTime;
                string JsonStr = HttpUtils.GetSync(PostUrl);
                var result = JsonUtils.Deserialize<PXYRetTrack<List<PXYShipTrackJson>>>(JsonStr);
                if (result.status == 0 && result.points != null && result.points.Count > 0)
                {
                    foreach (var item in result.points)
                    {
                        PXYShipTrack se = new PXYShipTrack();
                        se.Lat = ConvertLat(Convert.ToDouble(item.lat));
                        se.LatValue = Math.Round(Convert.ToDouble(item.lat) / 1000000, 6);
                        se.Lon = ConvertLon(Convert.ToDouble(item.lon));
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
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
                list = new List<PXYShipTrack>();
            }
            return list;
        }

        /// <summary>
        /// 获取预报时间
        /// </summary>
        public List<string> GetWeatherTime()
        {
            List<string> list = new List<string>();
            try
            {
                var PostUrl = _shiPXYHttpUrl + @"GetWeatherTime?k=" + _shiPXYKey;
                string JsonStr = HttpUtils.GetSync(PostUrl);
                var result = JsonUtils.Deserialize<PXYRet<List<string>>>(JsonStr);
                if (result.status == 0)
                {
                    list = result.data;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
                list = new List<string>();
            }
            return list;
        }

        /// <summary>
        /// 获取某经纬度的气象信息
        /// </summary>
        public PXYWeather GetWeatherByPoint(string lon, string lat)
        {
            PXYWeather se = new PXYWeather();
            try
            {
                var PostUrl = @"http://api.shipxy.com/apicall/GetWeatherByPoint?k=" + _shiPXYKey + "&lon=" + lon + "&lat=" + lat;
                string JsonStr = HttpUtils.GetSync(PostUrl);
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
                    se.winddirStr = ConvertDirection(se.winddir);
                    //划分风级
                    var WindSpeedLevel = ConvertWeatherWindLevel(se.windspeed);
                    se.windlevel = WindSpeedLevel.WindSpeedLevel;
                    se.windlevelStr = WindSpeedLevel.WindSpeedLevelStr;
                    se.windwarninglevel = WindSpeedLevel.WindSpeedWarningLevel;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
                se = new PXYWeather();
            }
            return se;
        }

        /// <summary>
        /// 获取某经纬度的预报气象信息
        /// </summary>
        public HFWeatherInfo GetPreWeatherByPoint(string lon, string lat, string time)
        {
            HFWeatherInfo se = new HFWeatherInfo();
            try
            {
                var PostUrl = @"http://api.shipxy.com/apicall/GetWeatherByPoint?k=" + _shiPXYKey + "&time=" + time + "&lon=" + lon + "&lat=" + lat;
                string JsonStr = HttpUtils.GetSync(PostUrl);
                var result = JsonUtils.Deserialize<PXYRet<List<PXYWeatherJson>>>(JsonStr);
                if (result.status == 0 && result.data != null && result.data.Count > 0)
                {
                    var data = result.data[0];
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
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
                se = new HFWeatherInfo();
            }
            return se;
        }

        /// <summary>
        /// 获取台风列表
        /// </summary>
        public List<PXYTyphoon> GetTyphoonList(out string msg)
        {
            List<PXYTyphoon> list = new List<PXYTyphoon>();
            msg = "";
            try
            {
                var PostUrl = @"http://api.shipxy.com/apicall/GetTyphoonList?k=" + _shiPXYKey;
                string JsonStr = HttpUtils.GetSync(PostUrl);
                var result = JsonUtils.Deserialize<PXYRet<List<PXYTyphoon>>>(JsonStr);

                if (result.status == 0 && result.data != null && result.data.Count > 0)
                {
                    list = result.data;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
                list = new List<PXYTyphoon>();
                msg = "发生错误";
            }
            return list;
        }

        /// <summary>
        /// 获取单个台风详情
        /// </summary>
        public List<PXYTyphoonData> GetSingleTyphoon(string xuHao, out string msg)
        {
            List<PXYTyphoonData> list = new List<PXYTyphoonData>();
            msg = "";
            try
            {
                if (string.IsNullOrEmpty(xuHao))
                {
                    return list;
                }

                var PostUrl = @"http://api.shipxy.com/apicall/GetSingleTyphoon?k=" + _shiPXYKey + "&tid=" + xuHao;
                string JsonStr = HttpUtils.GetSync(PostUrl);
                var result = JsonUtils.Deserialize<PXYRet<List<PXYTyphoonData>>>(JsonStr);

                if (result.status == 0 && result.data != null && result.data.Count > 0)
                {
                    list = result.data;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
                list = new List<PXYTyphoonData>();
                msg = "发生错误";
            }
            return list;
        }

        #endregion

        #region 转化工具

        /// <summary>
        /// 纬度转换（1/1000000度 → 度分秒字符串）
        /// </summary>
        private string ConvertLat(double lat)
        {
            string latStr = "未知";
            if (lat >= 0)
            {
                latStr = "N" + CoorConvert(lat);
            }
            else
            {
                latStr = "S" + CoorConvert(-lat);
            }
            return latStr;
        }

        /// <summary>
        /// 经度转换（1/1000000度 → 度分秒字符串）
        /// </summary>
        private string ConvertLon(double lon)
        {
            string lonStr = "未知";
            if (lon >= 0)
            {
                lonStr = "E" + CoorConvert(lon);
            }
            else
            {
                lonStr = "W" + CoorConvert(-lon);
            }
            return lonStr;
        }

        /// <summary>
        /// 转化坐标1
        /// </summary>
        public string CoorConvert(double value)
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
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// 坐标转换2（反向）
        /// </summary>
        public double CoorConvertReverse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }
            if (!value.Contains("E") && !value.Contains("W") && !value.Contains("S") && !value.Contains("N"))
            {
                return 0;
            }
            if (!value.Contains("°") || !value.Contains("'") || !value.Contains("\""))
            {
                return 0;
            }
            try
            {
                string Symbol = "";
                if (value.Contains("S") || value.Contains("W"))
                {
                    Symbol = "-";
                }

                value = value.Substring(1);
                var SecondStr = value.Substring(value.IndexOf("'") + 1).Replace("\"", "");
                var Second = Convert.ToDouble(SecondStr) / 60;

                var MinuteStr = value.Substring(value.IndexOf("°") + 1, value.IndexOf("'") - value.IndexOf("°") - 1);
                var Minute = (Convert.ToDouble(MinuteStr) + Second) / 60;

                var Degree = Convert.ToDouble(value.Substring(0, value.IndexOf("°")));

                return Convert.ToDouble(Symbol + (Degree + Minute).ToString());
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 船舶类型转换
        /// </summary>
        private string ConvertShipType(int type)
        {
            string ShipType = "未知";
            if (type >= 20 && type <= 29)
            {
                ShipType = "地效应船";
            }
            else if (type >= 40 && type <= 49)
            {
                ShipType = "高速船";
            }
            else if (type >= 60 && type <= 69)
            {
                ShipType = "客船";
            }
            else if (type >= 70 && type <= 79)
            {
                ShipType = "货船";
            }
            else if (type >= 80 && type <= 89)
            {
                ShipType = "油轮";
            }
            else if (type >= 90 && type <= 99)
            {
                ShipType = "其他类型的船舶";
            }
            else
            {
                ShipType = Enum.GetName(typeof(PXYShipTypeEnum), type);
                ShipType = ShipType ?? "其他类型的船舶";
            }
            return ShipType;
        }

        /// <summary>
        /// 船舶航行状态转换
        /// </summary>
        private string ConvertShipStat(int stat)
        {
            string ShipStat = Enum.GetName(typeof(PXYNaviStat), stat);
            ShipStat = ShipStat ?? "未知";
            return ShipStat;
        }

        /// <summary>
        /// 船舶航向转图标
        /// </summary>
        private string ConvertShipDegree(double degree)
        {
            string ShipDegree = "ship1";
            if (degree >= 33750 || degree < 2250)
            {
                ShipDegree = "ship1";
            }
            else if (degree >= 2250 || degree < 6750)
            {
                ShipDegree = "ship2";
            }
            else if (degree >= 6750 || degree < 11250)
            {
                ShipDegree = "ship3";
            }
            else if (degree >= 11250 || degree < 15750)
            {
                ShipDegree = "ship4";
            }
            else if (degree >= 15750 || degree < 20250)
            {
                ShipDegree = "ship5";
            }
            else if (degree >= 20250 || degree < 24750)
            {
                ShipDegree = "ship6";
            }
            else if (degree >= 24750 || degree < 29250)
            {
                ShipDegree = "ship7";
            }
            else if (degree >= 29250 || degree < 33750)
            {
                ShipDegree = "ship8";
            }
            return ShipDegree;
        }

        /// <summary>
        /// 风向角度转方位字符串
        /// </summary>
        private string ConvertDirection(string DirectionValue)
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
                else if (Value >= 67.5 && Value < 112.5)
                {
                    ret = "东";
                }
                else if (Value >= 112.5 && Value < 157.5)
                {
                    ret = "东南";
                }
                else if (Value >= 157.5 && Value < 202.5)
                {
                    ret = "南";
                }
                else if (Value >= 202.5 && Value < 247.5)
                {
                    ret = "西南";
                }
                else if (Value >= 247.5 && Value < 292.5)
                {
                    ret = "西";
                }
                else if (Value >= 292.5 && Value < 337.5)
                {
                    ret = "西北";
                }
                else if ((Value >= 337.5 && Value <= 360) || (Value >= 0 && Value < 22.5))
                {
                    ret = "北";
                }
                else if (Value >= 22.5 && Value < 67.5)
                {
                    ret = "东北";
                }
                else
                {
                    ret = "未知";
                }
            }

            return ret;
        }

        /// <summary>
        /// 风速转风级（蒲福风级）
        /// </summary>
        private WeatherWindSpeedLevel ConvertWeatherWindLevel(string WindSpeedValue)
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
                if (WindSpeed >= 0 && WindSpeed <= 0.2)
                {
                    ret.WindSpeedLevel = ((int)WindSpeedEnum.无风).ToString();
                    ret.WindSpeedLevelStr = WindSpeedEnum.无风.ToString();
                    ret.WindSpeedWarningLevel = "1";
                }
                else if (WindSpeed > 0.2 && WindSpeed <= 1.5)
                {
                    ret.WindSpeedLevel = ((int)WindSpeedEnum.软风).ToString();
                    ret.WindSpeedLevelStr = WindSpeedEnum.软风.ToString();
                    ret.WindSpeedWarningLevel = "1";
                }
                else if (WindSpeed > 1.5 && WindSpeed <= 3.3)
                {
                    ret.WindSpeedLevel = ((int)WindSpeedEnum.轻风).ToString();
                    ret.WindSpeedLevelStr = WindSpeedEnum.轻风.ToString();
                    ret.WindSpeedWarningLevel = "1";
                }
                else if (WindSpeed > 3.3 && WindSpeed <= 5.4)
                {
                    ret.WindSpeedLevel = ((int)WindSpeedEnum.微风).ToString();
                    ret.WindSpeedLevelStr = WindSpeedEnum.微风.ToString();
                    ret.WindSpeedWarningLevel = "1";
                }
                else if (WindSpeed > 5.4 && WindSpeed <= 7.9)
                {
                    ret.WindSpeedLevel = ((int)WindSpeedEnum.和风).ToString();
                    ret.WindSpeedLevelStr = WindSpeedEnum.和风.ToString();
                    ret.WindSpeedWarningLevel = "1";
                }
                else if (WindSpeed > 7.9 && WindSpeed <= 10.7)
                {
                    ret.WindSpeedLevel = ((int)WindSpeedEnum.劲风).ToString();
                    ret.WindSpeedLevelStr = WindSpeedEnum.劲风.ToString();
                    ret.WindSpeedWarningLevel = "1";
                }
                else if (WindSpeed > 10.7 && WindSpeed <= 13.8)
                {
                    ret.WindSpeedLevel = ((int)WindSpeedEnum.强风).ToString();
                    ret.WindSpeedLevelStr = WindSpeedEnum.强风.ToString();
                    ret.WindSpeedWarningLevel = "1";
                }
                else if (WindSpeed > 13.8 && WindSpeed <= 17.1)
                {
                    ret.WindSpeedLevel = ((int)WindSpeedEnum.疾风).ToString();
                    ret.WindSpeedLevelStr = WindSpeedEnum.疾风.ToString();
                    ret.WindSpeedWarningLevel = "2";
                }
                else if (WindSpeed > 17.1 && WindSpeed <= 20.7)
                {
                    ret.WindSpeedLevel = ((int)WindSpeedEnum.大风).ToString();
                    ret.WindSpeedLevelStr = WindSpeedEnum.大风.ToString();
                    ret.WindSpeedWarningLevel = "2";
                }
                else if (WindSpeed > 20.7 && WindSpeed <= 24.4)
                {
                    ret.WindSpeedLevel = ((int)WindSpeedEnum.烈风).ToString();
                    ret.WindSpeedLevelStr = WindSpeedEnum.烈风.ToString();
                    ret.WindSpeedWarningLevel = "2";
                }
                else if (WindSpeed > 24.4 && WindSpeed <= 28.4)
                {
                    ret.WindSpeedLevel = ((int)WindSpeedEnum.狂风).ToString();
                    ret.WindSpeedLevelStr = WindSpeedEnum.狂风.ToString();
                    ret.WindSpeedWarningLevel = "2";
                }
                else if (WindSpeed > 28.4 && WindSpeed <= 32.6)
                {
                    ret.WindSpeedLevel = ((int)WindSpeedEnum.暴风).ToString();
                    ret.WindSpeedLevelStr = WindSpeedEnum.暴风.ToString();
                    ret.WindSpeedWarningLevel = "3";
                }
                else if (WindSpeed > 32.6 && WindSpeed <= 41.5)
                {
                    ret.WindSpeedLevel = ((int)WindSpeedEnum.台风).ToString();
                    ret.WindSpeedLevelStr = WindSpeedEnum.台风.ToString();
                    ret.WindSpeedWarningLevel = "3";
                }
                else if (WindSpeed > 41.5)
                {
                    ret.WindSpeedLevel = ((int)WindSpeedEnum.超强台风).ToString();
                    ret.WindSpeedLevelStr = WindSpeedEnum.超强台风.ToString();
                    ret.WindSpeedWarningLevel = "3";
                }
                else
                {
                    ret.WindSpeedLevel = "";
                    ret.WindSpeedLevelStr = "";
                    ret.WindSpeedWarningLevel = "";
                }
            }
            return ret;
        }

        /// <summary>
        /// 浪高转涌级
        /// </summary>
        private WeatherSwellLevel ConvertWeatherSwellLevel(string SwellValue)
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
                if (Swell == 0)
                {
                    ret.SwellLevel = ((int)SwellLevelEnum.无浪).ToString();
                    ret.SwellLevelStr = SwellLevelEnum.无浪.ToString();
                    ret.SwellWarningLevel = "1";
                }
                else if (Swell > 0 && Swell <= 0.1)
                {
                    ret.SwellLevel = ((int)SwellLevelEnum.微浪).ToString();
                    ret.SwellLevelStr = SwellLevelEnum.微浪.ToString();
                    ret.SwellWarningLevel = "1";
                }
                else if (Swell > 0.1 && Swell <= 0.5)
                {
                    ret.SwellLevel = ((int)SwellLevelEnum.小浪).ToString();
                    ret.SwellLevelStr = SwellLevelEnum.小浪.ToString();
                    ret.SwellWarningLevel = "1";
                }
                else if (Swell > 0.5 && Swell <= 1.25)
                {
                    ret.SwellLevel = ((int)SwellLevelEnum.轻浪).ToString();
                    ret.SwellLevelStr = SwellLevelEnum.轻浪.ToString();
                    ret.SwellWarningLevel = "1";
                }
                else if (Swell > 1.25 && Swell <= 2.5)
                {
                    ret.SwellLevel = ((int)SwellLevelEnum.中浪).ToString();
                    ret.SwellLevelStr = SwellLevelEnum.中浪.ToString();
                    ret.SwellWarningLevel = "1";
                }
                else if (Swell > 2.5 && Swell <= 4.0)
                {
                    ret.SwellLevel = ((int)SwellLevelEnum.大浪).ToString();
                    ret.SwellLevelStr = SwellLevelEnum.大浪.ToString();
                    ret.SwellWarningLevel = "2";
                }
                else if (Swell > 4.0 && Swell <= 6.0)
                {
                    ret.SwellLevel = ((int)SwellLevelEnum.巨浪).ToString();
                    ret.SwellLevelStr = SwellLevelEnum.巨浪.ToString();
                    ret.SwellWarningLevel = "2";
                }
                else if (Swell > 6.0 && Swell <= 9.0)
                {
                    ret.SwellLevel = ((int)SwellLevelEnum.狂浪).ToString();
                    ret.SwellLevelStr = SwellLevelEnum.狂浪.ToString();
                    ret.SwellWarningLevel = "3";
                }
                else if (Swell > 9.0 && Swell <= 14.0)
                {
                    ret.SwellLevel = ((int)SwellLevelEnum.狂涛).ToString();
                    ret.SwellLevelStr = SwellLevelEnum.狂涛.ToString();
                    ret.SwellWarningLevel = "3";
                }
                else if (Swell > 14.0)
                {
                    ret.SwellLevel = ((int)SwellLevelEnum.怒涛).ToString();
                    ret.SwellLevelStr = SwellLevelEnum.怒涛.ToString();
                    ret.SwellWarningLevel = "3";
                }
                else
                {
                    ret.SwellLevel = "";
                    ret.SwellLevelStr = "";
                    ret.SwellWarningLevel = "";
                }
            }
            return ret;
        }

        #endregion

        #region 船顺网

        /// <summary>
        /// 获取船顺网船舶信息
        /// </summary>
        public List<ShipCSWInfo> GetCSWShips()
        {
            List<ShipCSWInfo> list = new List<ShipCSWInfo>();
            try
            {
                string path = @"D:\Json.txt";
                string content = File.ReadAllText(path);
                var model = JsonUtils.Deserialize<ShipCSWResponse>(content);

                byte[] bytes = Convert.FromBase64String(model.data);

                string resultStr = "";
                using (var compressedStream = new MemoryStream(bytes))
                {
                    using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                    {
                        using (var resultStream = new MemoryStream())
                        {
                            zipStream.CopyTo(resultStream);
                            var BData = resultStream.ToArray();
                            resultStr = Encoding.UTF8.GetString(BData);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(resultStr))
                {
                    var arrayList = JsonUtils.Deserialize<List<List<string>>>(resultStr);

                    if (arrayList.Count > 0)
                    {
                        arrayList.ForEach(a =>
                        {
                            if (!string.IsNullOrEmpty(a[0]) && !string.IsNullOrEmpty(a[13]) && !string.IsNullOrEmpty(a[14]) && !string.IsNullOrEmpty(a[3]) && !string.IsNullOrEmpty(a[15]))
                            {
                                list.Add(new ShipCSWInfo()
                                {
                                    MMSI = a[0].ToString(),
                                    ShipName = a[3].ToString(),
                                    Lon = a[13].ToString(),
                                    Lat = a[14].ToString(),
                                    Time = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(a[15])).ToOffset(TimeSpan.FromHours(8)).ToString("yyyy-MM-dd HH:mm:ss")
                                });
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
                list = new List<ShipCSWInfo>();
            }
            return list;
        }

        #endregion

        #region 页面

        /// <summary>
        /// 获取项目信息
        /// </summary>
        public (GisProjectMain data, string msg) GetProjectInfo(GisRequest request)
        {
            GisProjectMain data = new GisProjectMain();
            string msg = "";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var areaRepo = uow.GetRepository<Wind_ProjectArea>();
                    var fanRepo = uow.GetRepository<Wind_ProjectFan>();

                    var Project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    var ProjectAreas = areaRepo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList();

                    if (Project == null || string.IsNullOrEmpty(Project.Lon) || string.IsNullOrEmpty(Project.Lat) || ProjectAreas.Count == 0)
                    {
                        msg = "没有项目信息数据";
                        return (new GisProjectMain(), msg);
                    }

                    var ProjectFans = fanRepo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList();

                    data.ID = Project.ID;
                    data.Lon = Project.Lon ?? "";
                    data.Lat = Project.Lat ?? "";
                    data.ProjectName = Project.ProjectName;
                    data.Status = (int)Project.Status;
                    data.StatusName = Enum.GetName(typeof(CloudWindProjectStatus), Project.Status);

                    ProjectAreas.ForEach(a =>
                    {
                        data.Areas.Add(new GisProjectArea()
                        {
                            ID = a.ID,
                            Lon = a.AreaLon ?? "",
                            Lat = a.AreaLat ?? ""
                        });
                    });
                    //连上第一点
                    data.Areas.Add(new GisProjectArea()
                    {
                        ID = ProjectAreas[0].ID,
                        Lon = ProjectAreas[0].AreaLon ?? "",
                        Lat = ProjectAreas[0].AreaLat ?? ""
                    });

                    ProjectFans.ForEach(a =>
                    {
                        data.Fans.Add(new GisFan()
                        {
                            ID = a.ID,
                            FanName = a.FanName ?? "",
                            Lon = a.Lon ?? "",
                            Lat = a.Lat ?? "",
                            Status = a.Status.Value,
                            StatusName = Enum.GetName(typeof(CloudWindFanStatus), a.Status)
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
                data = new GisProjectMain();
                msg = "发生错误";
            }
            return (data, msg);
        }

        #endregion

        #region GIS业务方法

        /// <summary>
        /// 获取项目基本信息（用于 Index 页面）
        /// </summary>
        public Wind_Project GetProject(int ProjectID)
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                return uow.GetRepository<Wind_Project>().Find(a => !a.IsDelete && a.ID == ProjectID).FirstOrDefault();
            }
        }

        /// <summary>
        /// 获取船舶列表（依赖 WebGIS_Ship 实体）
        /// </summary>
        public (PXYShip data, string msg) GetShips()
        {
            PXYShip data = new PXYShip();
            string msg = "";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    List<PXYShipInfo> list = new List<PXYShipInfo>();
                    uow.GetRepository<WebGIS_Ship>().Find(a => !a.IsDelete.Value).ToList().ForEach(a =>
                    {
                        list.Add(new PXYShipInfo()
                        {
                            ID = a.id,
                            ShipName = a.ShipName ?? "",
                            MMSI = a.mmsi ?? "",
                            ShipType = a.ShipType ?? ""
                        });
                    });

                    list = GetPXYShips(list, out msg);

                    if (string.IsNullOrEmpty(msg))
                    {
                        list.GroupBy(a => a.ShipType).ToList().ForEach(a =>
                        {
                            data.ShipTypes.Add(new PXYShipType()
                            {
                                ShipType = a.FirstOrDefault().ShipType
                            });
                        });
                        data.Ships = list;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
                msg = "发生错误";
            }
            return (data, msg);
        }

        /// <summary>
        /// 查询单船信息（依赖 WebGIS_Ship 实体）
        /// </summary>
        public WindManageShip GetShip(int ShipID)
        {
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var Ship = uow.GetRepository<WebGIS_Ship>().Find(a => a.id == ShipID).FirstOrDefault();
                    if (Ship == null || string.IsNullOrEmpty(Ship.mmsi))
                        return null;

                    var PXYShipInfo = GetSingleShip(Ship.mmsi);

                    WindManageShip se = new WindManageShip();
                    se.ID = Ship.id;
                    se.ShipName = Ship.ShipName;
                    se.MMSI = Ship.mmsi;
                    se.ShipTypeName = Ship.ShipType ?? "";

                    if (PXYShipInfo != null)
                    {
                        se.LonStr = PXYShipInfo.Lon;
                        se.Lon = PXYShipInfo.LonValue;
                        se.LatStr = PXYShipInfo.Lat;
                        se.Lat = PXYShipInfo.LatValue;
                        se.CallSign = PXYShipInfo.CallSign ?? "-";
                        se.IMO = PXYShipInfo.IMO == 0 ? "-" : PXYShipInfo.IMO.ToString();
                        se.Speed = PXYShipInfo.Speed2.ToString();
                        se.Dest = PXYShipInfo.Dest ?? "-";
                        se.YDSJ = PXYShipInfo.Eta ?? "-";
                        se.Navistat = PXYShipInfo.NaviStat ?? "-";
                        se.Heading = PXYShipInfo.ShipDegreeValue == 0 ? "-" : PXYShipInfo.ShipDegreeValue.ToString();
                        se.Left = PXYShipInfo.Left.ToString();
                        se.Trail = PXYShipInfo.Trail.ToString();
                        se.UpdateTime = PXYShipInfo.LastTime;
                        se.Length = PXYShipInfo.Length == 0 ? "-" : PXYShipInfo.Length.ToString();
                        se.Width = PXYShipInfo.Width == 0 ? "-" : PXYShipInfo.Width.ToString();
                        se.Draught = PXYShipInfo.Draught == 0 ? "-" : PXYShipInfo.Draught.ToString();
                        se.StatusStr = PXYShipInfo.NaviStat ?? "";
                    }
                    return se;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
                return null;
            }
        }

        /// <summary>
        /// 获取船舶轨迹（依赖 WebGIS_Ship 实体）
        /// </summary>
        public List<PXYShipTrack> GetShipTrackData(int ShipID, string StartTime, string EndTime)
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var Ship = uow.GetRepository<WebGIS_Ship>().Find(a => !a.IsDelete.Value && a.id == ShipID).FirstOrDefault();
                if (Ship == null || string.IsNullOrEmpty(Ship.mmsi))
                    return new List<PXYShipTrack>();

                return GetShipTrack(Ship.mmsi, StartTime, EndTime);
            }
        }

        /// <summary>
        /// 获取锚点信息（依赖 WebGIS_ShipAnchor 实体）
        /// </summary>
        public List<PXYShipAnchor> GetShipAnchor()
        {
            List<PXYShipAnchor> list = new List<PXYShipAnchor>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var ShipAnchors = uow.GetRepository<WebGIS_ShipAnchor>().Find(a => !a.IsDelete.Value).ToList();

                    ShipAnchors.GroupBy(a => a.AnchorGroup).ToList().ForEach(a =>
                    {
                        PXYShipAnchor se = new PXYShipAnchor();
                        se.AnchorName = a.FirstOrDefault().AnchorName;
                        se.ID = a.FirstOrDefault().AnchorGroup;
                        ShipAnchors.Where(b => b.AnchorGroup.Equals(se.ID)).ToList().ForEach(b =>
                        {
                            ShipAnchorPosition sp = new ShipAnchorPosition();
                            sp.Lon = b.Lon;
                            sp.LonValue = CoorConvertReverse(b.Lon);
                            sp.Lat = b.Lat;
                            sp.LatValue = CoorConvertReverse(b.Lat);
                            se.Positions.Add(sp);
                        });
                        list.Add(se);
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
            }
            return list;
        }

        /// <summary>
        /// 天气预报
        /// </summary>
        public (List<WeatherTime> data, string msg) GetPreWeatherInfo(int ProjectID)
        {
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var Project = uow.GetRepository<Wind_Project>().Find(a => !a.IsDelete && a.ID == ProjectID).FirstOrDefault();
                    if (Project == null || string.IsNullOrEmpty(Project.Lon) || string.IsNullOrEmpty(Project.Lat))
                        return (new List<WeatherTime>(), "数据错误");

                    var WeatherTimes = GetWeatherTime();
                    List<WeatherTime> Dates = new List<WeatherTime>();
                    WeatherTimes.ForEach(a =>
                    {
                        WeatherTime time = new WeatherTime();
                        time.Time = Convert.ToDateTime(a);
                        time.DateStr = Convert.ToDateTime(a).ToShortDateString();
                        Dates.Add(time);
                    });

                    List<WeatherTime> Weathers = new List<WeatherTime>();
                    Dates.OrderBy(a => a.Time).GroupBy(a => a.DateStr).ToList().ForEach(a =>
                    {
                        WeatherTime time = new WeatherTime();
                        var DateStr = a.FirstOrDefault().DateStr;
                        var Date = Convert.ToDateTime(DateStr);
                        time.DateStr = Date.Month + "月" + Date.Day + "日";

                        Dates.Where(b => b.DateStr.Equals(DateStr)).OrderBy(b => b.Time).ToList().ForEach(b =>
                        {
                            var TimeStr = b.Time.ToString("yyyy-MM-dd HH:mm:ss");
                            var WeatherInfo = GetPreWeatherByPoint(Project.Lon, Project.Lat, TimeStr);
                            WeatherInfo.Time = b.Time.ToString("HH:mm");
                            time.Periods.Add(WeatherInfo);
                        });
                        Weathers.Add(time);
                    });

                    return (Weathers, "");
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
                return (new List<WeatherTime>(), "发生错误");
            }
        }

        /// <summary>
        /// 获取地勘数据资源
        /// </summary>
        public (GisDataSource data, string msg) GetDataResource(int ProjectID)
        {
            GisDataSource data = new GisDataSource();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var DKList = uow.GetRepository<View_Library_FanDK>().Find(a => a.ProjectID == ProjectID).OrderBy(a => a.id).ToList();

                    DKList.GroupBy(a => a.dkid).ToList().ForEach(a =>
                    {
                        string DKName = a.FirstOrDefault().DKName;
                        int DKID = a.FirstOrDefault().dkid.Value;

                        var ThisList = DKList.Where(b => b.dkid == DKID).ToList();

                        DKModel dkSource = new DKModel();
                        dkSource.DKName = DKName;

                        ThisList.ForEach(b =>
                        {
                            dkSource.Data.Add(new DKData()
                            {
                                序号 = b.xh,
                                地层编号 = b.dcbh,
                                土层名称 = b.tcbh,
                                层底标高_m = b.cdbg,
                                土层类型 = b.tclx,
                                不排水抗剪强度_cu_kPa = b.bpskjqd,
                                砂土摩擦角_度 = b.stmcj,
                                有效重度 = b.yxzd,
                                标贯击数 = b.bgjs
                            });
                        });
                        data.DKList.Add(dkSource);
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
            }
            return (data, "");
        }

        /// <summary>
        /// 获取项目报表文件
        /// 注意：View_ProjectTaskShipFile、View_ProjectTaskFile、View_ProjectTaskPileFile 实体待创建
        /// </summary>
        public (GisDataReport data, string msg) GetReport(int ProjectID)
        {
            GisDataReport data = new GisDataReport();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    // 1.项目文件
                    ReportModel fileReport = new ReportModel();
                    fileReport.TabName = "相关文件";
                    uow.GetRepository<Wind_ProjectFile>().Find(a => !a.IsDelete && a.ProjectID == ProjectID).ToList()
                        .ForEach(a =>
                        {
                            fileReport.Files.Add(new CloudProjectFile()
                            {
                                ID = a.ID,
                                FileName = a.FileName,
                                FilePath = a.FilePath.Substring(a.FilePath.IndexOf(@"\File\")),
                                FileTime = Convert.ToDateTime(a.CreateTime).ToString("yyyy-MM-dd"),
                                FileExtension = FileUtils.GetFileExtension(a.FileName),
                                FileType = "项目资料文件"
                            });
                        });

                    uow.GetRepository<Library_Geology>().Find(a => !a.IsDelete && a.ProjectID == ProjectID).ToList()
                        .ForEach(a =>
                        {
                            fileReport.Files.Add(new CloudProjectFile()
                            {
                                ID = a.ID,
                                FileName = a.FileName,
                                FilePath = a.FilePath.Substring(a.FilePath.IndexOf(@"\File\")),
                                FileTime = Convert.ToDateTime(a.CreateTime).ToString("yyyy-MM-dd"),
                                FileExtension = FileUtils.GetFileExtension(a.FileName),
                                FileType = a.Type == 1 ? "地勘原始资料文件" : a.Type == 2 ? "地勘数据表" : ""
                            });
                        });
                    data.ProjectFile = fileReport;

                    // 2.插拔桩计算 - 输出文件
                    ReportModel cbzReportExport = new ReportModel();
                    cbzReportExport.TabName = "输出文件";
                    uow.GetRepository<View_ProjectTaskReport>().Find(a => a.ProjectID == ProjectID && a.Type == (int)CloudWindTaskReportType.插拔桩计算修改报告).ToList()
                        .ForEach(a =>
                        {
                            cbzReportExport.Files.Add(new CloudProjectFile()
                            {
                                ID = a.id,
                                FileName = a.ReportName,
                                FilePath = a.ReportPath.Substring(a.ReportPath.IndexOf(@"\File\")),
                                FileTime = a.CreateTime.Value.ToString("yyyy-MM-dd"),
                                FileExtension = FileUtils.GetFileExtension(a.ReportName)
                            });
                        });
                    data.CBZExportFile = cbzReportExport;

                    // 插拔桩计算 - 输入文件
                    ReportModel cbzReportImport = new ReportModel();
                    cbzReportImport.TabName = "输入文件";
                    uow.GetRepository<Library_Geology>().Find(a => !a.IsDelete && a.ProjectID == ProjectID).ToList()
                        .ForEach(a =>
                        {
                            cbzReportImport.Files.Add(new CloudProjectFile()
                            {
                                ID = a.ID,
                                FileName = a.FileName,
                                FilePath = a.FilePath.Substring(a.FilePath.IndexOf(@"\File\")),
                                FileTime = a.CreateTime.Value.ToString("yyyy-MM-dd"),
                                FileExtension = FileUtils.GetFileExtension(a.FileName)
                            });
                        });
                    // TODO: 需要 View_ProjectTaskShipFile 实体
                    data.CBZImportFile = cbzReportImport;

                    // 3.可作业预报 - 输出文件
                    ReportModel kzyReportExport = new ReportModel();
                    kzyReportExport.TabName = "输出文件";
                    // TODO: 需要 View_ProjectTaskFile 实体，当前通过 Wind_Task + Wind_TaskFile 联查
                    var kzyTaskFiles = (from t in uow.GetRepository<Wind_Task>().Find(a => !a.IsDelete && a.ProjectID == ProjectID)
                                        join f in uow.GetRepository<Wind_TaskFile>().Find(a => a.TypeID == (int)CloudWindTaskSendFileType.可作业预报文件 && !(a.IsDelete == true))
                                        on t.ID equals f.TaskID into tf
                                        from f in tf
                                        orderby f.CreateTime descending
                                        select f).Take(50).ToList();
                    kzyTaskFiles.ForEach(a =>
                    {
                        kzyReportExport.Files.Add(new CloudProjectFile()
                        {
                            ID = a.id,
                            FileName = a.FileName,
                            FilePath = a.FilePath.Substring(a.FilePath.IndexOf(@"\File\")),
                            FileTime = a.CreateTime.Value.ToString("yyyy-MM-dd"),
                            FileExtension = FileUtils.GetFileExtension(a.FileName)
                        });
                    });
                    data.KZYExportFile = kzyReportExport;

                    // 4.桩锤 - 输出文件
                    ReportModel pileReportExport = new ReportModel();
                    pileReportExport.TabName = "输出文件";
                    uow.GetRepository<View_ProjectTaskReport>().Find(a => a.ProjectID == ProjectID && a.Type == (int)CloudWindTaskReportType.桩基沉桩可打性分析报告).ToList()
                        .ForEach(a =>
                        {
                            pileReportExport.Files.Add(new CloudProjectFile()
                            {
                                ID = a.id,
                                FileName = a.ReportName,
                                FilePath = a.ReportPath.Substring(a.ReportPath.IndexOf(@"\File\")),
                                FileTime = a.CreateTime.Value.ToString("yyyy-MM-dd"),
                                FileExtension = FileUtils.GetFileExtension(a.ReportName)
                            });
                        });
                    data.PileExportFile = pileReportExport;

                    // 桩锤 - 输入文件
                    ReportModel pileReportImport = new ReportModel();
                    pileReportImport.TabName = "输入文件";
                    // TODO: 需要 View_ProjectTaskPileFile 实体，当前通过 Wind_Task + Library_Pile 联查
                    var pileFiles = (from t in uow.GetRepository<Wind_Task>().Find(a => !a.IsDelete && a.ProjectID == ProjectID)
                                     join f in uow.GetRepository<Library_Pile>().Find(a => !(a.IsDelete == true))
                                     on t.ID equals f.TaskID into pf
                                     from f in pf
                                     select f).ToList();
                    pileFiles.ForEach(a =>
                    {
                        pileReportImport.Files.Add(new CloudProjectFile()
                        {
                            ID = a.id,
                            FileName = a.FileName,
                            FilePath = a.FilePath.Substring(a.FilePath.IndexOf(@"\File\")),
                            FileTime = a.CreateTime.Value.ToString("yyyy-MM-dd"),
                            FileExtension = FileUtils.GetFileExtension(a.FileName)
                        });
                    });
                    data.PileImportFile = pileReportImport;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WebGISService));
            }
            return (data, "");
        }

        #endregion
    }
}
