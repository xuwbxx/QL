namespace Model.Tech.Cloud.Webgis
{
    public class PXYWeather
    {
        public string bm500 { set; get; }

        public string humidity { set; get; }

        public string pressure { set; get; }

        public string swelldir { set; get; }

        public string swellheight { set; get; }

        public string swellperiod { set; get; }

        public string temperature { set; get; }

        public string visibility { set; get; }

        public string waveheight { set; get; }

        public string winddir { set; get; }

        public string winddirStr { set; get; }

        public string windspeed { set; get; }

        public string windlevel { set; get; }

        public string windlevelStr { set; get; }

        public string windwarninglevel { set; get; }

        public bool windalarm { set; get; }

        public string time { set; get; }

        public string timeStr { set; get; }

        /// <summary>
        /// 流向(度)
        /// </summary>
        public string oceandir { set; get; }

        /// <summary>
        /// 流向
        /// </summary>
        public string oceandirStr { set; get; }
        /// <summary>
        /// 流速
        /// </summary>
        public string oceanspeed { set; get; }
    }

    public class PXYWeatherJson
    {
        /// <summary>
        /// 500mb 高程气压，单位（gpm}
        /// </summary>
        public double bm500 { set; get; }

        /// <summary>
        /// 湿度，单位（%）
        /// </summary>
        public double humidity { set; get; }

        /// <summary>
        /// 压强，单位：hPa（百帕）
        /// </summary>
        public double pressure { set; get; }

        /// <summary>
        /// 涌向，单位（度）
        /// </summary>
        public double swelldir { set; get; }

        /// <summary>
        /// 涌高，单位（m）
        /// </summary>
        public double swellheight { set; get; }

        /// <summary>
        /// 涌周期，单位（s）
        /// </summary>
        public double swellperiod { set; get; }

        /// <summary>
        /// 温度，单位：（℃）
        /// </summary>
        public double temperature { set; get; }

        /// <summary>
        /// 能见度
        /// </summary>
        public double visibility { set; get; }

        /// <summary>
        /// 浪高，单位（m）
        /// </summary>
        public double waveheight { set; get; }

        /// <summary>
        /// 风向，单位（度）
        /// </summary>
        public double winddir { set; get; }

        /// <summary>
        /// 风速，单位（m/s）
        /// </summary>
        public double windspeed { set; get; }


        public double oceandir { set; get; }

        public double oceanspeed { set; get; }

    }


    public class HFWeatherInfo
    {
        public string Time { set; get; }

        public string ShortTime { set; get; }

        public string WindDir { set; get; }

        public string WindDirStr { set; get; }

        public string WindSpeed { set; get; }

        public bool IsWindSpeedWarning { set; get; }

        /// <summary>
        /// 1:绿色 2：黄色  3：红色  
        /// </summary>
        public string WindSpeedWarningLevel { set; get; }

        public string WindSpeedLevel { set; get; }

        public string WindSpeedLevelStr { set; get; }

        public string WaveHeight { set; get; }

        public string SwellLevel { set; get; }

        public string SwellLevelStr { set; get; }

        public string SwellWarningLevel { set; get; }

        public string SwellDir { set; get; }

        public string SwellHeight { set; get; }

        public string SwellPeriod { set; get; }

        public string Temperature { set; get; }

        public string Visibility { set; get; }

        public string Pressure { set; get; }

        public string Bm500 { set; get; }

        public string Humidity { set; get; }

        public string OceanDir { set; get; }

        public string OceanSpeed { set; get; }

        public string OceanDirStr { set; get; }
    }

    public class WeatherTime
    {
        public WeatherTime()
        {
            Periods = new List<HFWeatherInfo>();
        }

        public DateTime Time { set; get; }

        public string DateStr { set; get; }

        public List<HFWeatherInfo> Periods { set; get; }
    }

    public class WeatherWindSpeedLevel
    {
        public string WindSpeedLevel { set; get; }

        public string WindSpeedLevelStr { set; get; }

        public string WindSpeedWarningLevel { set; get; }
    }

    public class WeatherSwellLevel
    {
        public string SwellLevel { set; get; }

        public string SwellLevelStr { set; get; }

        public string SwellWarningLevel { set; get; }
    }


    public enum WindSpeedEnum
    {
        无风 = 0,
        软风 = 1,
        轻风 = 2,
        微风 = 3,
        和风 = 4,
        劲风 = 5,
        强风 = 6,
        疾风 = 7,
        大风 = 8,
        烈风 = 9,
        狂风 = 10,
        暴风 = 11,
        台风 = 12,
        强台风 = 13,
        超强台风 = 14
    }

    public enum SwellLevelEnum
    {
        无浪 = 0,
        微浪 = 1,
        小浪 = 2,
        轻浪 = 3,
        中浪 = 4,
        大浪 = 5,
        巨浪 = 6,
        狂浪 = 7,
        狂涛 = 8,
        怒涛 = 9
    }
}
