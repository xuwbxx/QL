namespace Model.Tech.Cloud.Webgis
{
    public class PXYTyphoon
    {
        /// <summary>
        /// 国际编号
        /// </summary>
        public string interCode { set; get; }

        /// <summary>
        /// 国内编号
        /// </summary>
        public string chnCode { set; get; }

        /// <summary>
        /// 台风名称（中文）
        /// </summary>
        public string chnName { set; get; }

        /// <summary>
        /// 台风名称（英文）
        /// </summary>
        public string enName { set; get; }

        /// <summary>
        /// 发生年份
        /// </summary>
        public string currentyear { set; get; }

        /// <summary>
        /// 是否正在发生（ ing：进行中，否则空）
        /// </summary>
        public string dataMark { set; get; }


        /// <summary>
        /// 序号
        /// </summary>
        public string xuHao { set; get; }


    }


    public class PXYTyphoonData
    {
        /// <summary>
        /// 台风序号
        /// </summary>
        public string id { set; get; }

        /// <summary>
        /// 台风产生时间
        /// </summary>
        public string time { set; get; }

        /// <summary>
        /// 预测信息（为空表示实际点，不为空说明是预测点）
        /// </summary>
        public string forecast { set; get; }

        /// <summary>
        /// 预测时间范围
        /// </summary>
        public string fhour { set; get; }

        /// <summary>
        /// 纬度
        /// </summary>
        public string lat { set; get; }

        /// <summary>
        /// 经度
        /// </summary>
        public string lon { set; get; }

        /// <summary>
        /// 风级（5-18）
        /// </summary>
        public string grade { set; get; }

        /// <summary>
        /// 风速（m/s）
        /// </summary>
        public string mspeed { set; get; }

        /// <summary>
        /// 中心气压（百帕）
        /// </summary>
        public string pressure { set; get; }

        /// <summary>
        /// 移动速度（Km/h）
        /// </summary>
        public string kspeed { set; get; }

        /// <summary>
        /// 移向
        /// </summary>
        public string direction { set; get; }

        /// <summary>
        /// 7 级风圈半径（Km）
        /// </summary>
        public string radius7 { set; get; }

        /// <summary>
        /// 10 级风圈半径（Km）
        /// </summary>
        public string radius10 { set; get; }

        public DateTime? dateTime
        {

            get
            {
                return TimeConvert(time);
            }

        }



        private DateTime? TimeConvert(string TimeStr)
        {
            if (string.IsNullOrEmpty(TimeStr) || TimeStr.Length != 12)
                return null;

            int year = Convert.ToInt32(TimeStr.Substring(0, 4));
            int month = Convert.ToInt32(TimeStr.Substring(4, 2));
            int day = Convert.ToInt32(TimeStr.Substring(6, 2));
            int hour = Convert.ToInt32(TimeStr.Substring(8, 2));
            int minute = Convert.ToInt32(TimeStr.Substring(10, 2));

            DateTime dt = new DateTime(year, month, day, hour, minute, 0);

            return dt;

        }

    }


    public class PXYTyphoonAnalyzeModel
    {
        public PXYTyphoonAnalyzeModel()
        {
            Typhoons = new List<PXYTyphoonInfo>();
        }

        public string chnCode { set; get; }

        public string chnName { set; get; }

        public string xuHao { set; get; }

        public List<PXYTyphoonInfo> Typhoons { set; get; }
    }

    public class PXYTyphoonInfo
    {
        public int ID { set; get; }
        public DateTime forecastTime { set; get; }

        public string forecastTimeStr
        {
            get
            {

                return forecastTime == null ? "" : forecastTime.ToString("yyyy-MM-dd HH:mm:ss");

            }
        }

        public decimal lon { set; get; }

        public decimal lat { set; get; }

        public decimal grade { set; get; }

        public decimal speed { set; get; }
    }

    public class PXYArea
    {
        public decimal Lon { set; get; }

        public decimal Lat { set; get; }
    }

}
