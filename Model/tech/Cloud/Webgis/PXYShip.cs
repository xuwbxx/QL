namespace Model.Tech.Cloud.Webgis
{
    public class PXYShip
    {
        public PXYShip()
        {
            ShipTypes = new List<PXYShipType>();
            Ships = new List<PXYShipInfo>();
        }

        public List<PXYShipType> ShipTypes { set; get; }
        public List<PXYShipInfo> Ships { set; get; }
    }

    public class PXYShipType
    {
        public string ShipType { set; get; }
    }

    public class PXYShipInfo
    {
        public int ID { set; get; }
        public string ShipName { set; get; }

        public string ShipShortName { set; get; }
        public string ShipChineseName { set; get; }
        public string MMSI { set; get; }

        public string ShipType { set; get; }

        public int IMO { set; get; }

        public string CallSign { set; get; }

        public double Length { set; get; }

        public double Width { set; get; }

        public double Left { set; get; }

        public double Trail { set; get; }

        public double Draught { set; get; }

        public string Dest { set; get; }

        public string Eta { set; get; }

        public string NaviStat { set; get; }

        public string Lat { set; get; }

        public double LatValue { set; get; }

        public string Lon { set; get; }

        public double LonValue { set; get; }

        public double Speed1 { set; get; }

        public double Speed2 { set; get; }

        public double ShipDegreeValue { set; get; }

        public double Rot { set; get; }

        public string LastTime { set; get; }

        public string UseType { set; get; }

        public string ShipDegree { set; get; }


        public string PropertyUnit { set; get; }

        public string ManageUnit { set; get; }

        public string UseStatus { set; get; }

        public string ShipIcon { set; get; }

        public string ShipTypeIcon { set; get; }

        public string ProjectName { set; get; }

        public string CompanyName { set; get; }



        public string SatellitePhone { set; get; }
        public string CompanyCharger { set; get; }
        public string CompanyPhone { set; get; }
        public string ProjectCharger { set; get; }
        public string ProjectPhone { set; get; }
        public string Captain { set; get; }
        public string CaptainPhone { set; get; }
        public string ChiefMate { set; get; }
        public string ChiefMatePhone { set; get; }

        public int ShipWeight { set; get; }

        public string ShipWeightName { set; get; }

        public int ProjectID { set; get; }

    }


    public class PXYShipTrack
    {
        public string Lat { set; get; }

        public double LatValue { set; get; }

        public string Lon { set; get; }

        public double LonValue { set; get; }

        public double Speed { set; get; }

        public string Time { set; get; }

        public double Cog { set; get; }
    }


    public class PXYShipInfoJson
    {
        /// <summary>
        /// 船ID
        /// </summary>
        public long ShipID { set; get; }

        /// <summary>
        /// 数据来源，0 代表 AIS，1 代表卫星
        /// </summary>
        public int From { set; get; }

        /// <summary>
        /// 船舶 MMSI，9 位数字
        /// </summary>
        public long mmsi { set; get; }

        /// <summary>
        /// 船类型
        /// </summary>
        public int shiptype { set; get; }

        /// <summary>
        /// 0= Not available (default)2147483647=Null
        /// </summary>
        public int imo { set; get; }

        /// <summary>
        /// 船名
        /// </summary>
        public string name { set; get; }

        /// <summary>
        /// 船舶呼号
        /// </summary>
        public string callsign { set; get; }

        /// <summary>
        /// 船长，分米，(0-10220) 
        /// </summary>
        public int length { set; get; }

        /// <summary>
        /// 船宽，分米，(0-1260]
        /// </summary>
        public int width { set; get; }

        /// <summary>
        /// 左舷距，分米,(0-630]
        /// </summary>
        public int left { set; get; }

        /// <summary>
        /// 尾距，分米，(0-5110)
        /// </summary>
        public int trail { set; get; }

        /// <summary>
        /// 吃水，毫米，(0-25500]
        /// </summary>
        public int draught { set; get; }

        /// <summary>
        /// 目的地
        /// </summary>
        public string dest { set; get; }

        /// <summary>
        /// 标准化后的目的地（只有返回json 格式时包含此信息）：根据 dest 内容匹配到船讯网港口库
        /// </summary>
        public string dest_std { set; get; }

        /// <summary>
        /// 标准化后的目的地港口编码（只有返回 json 格式时包含此信息）
        /// </summary>
        public string destcode { set; get; }

        /// <summary>
        /// 预到时间：[MM][DD][HH][MM]
        /// </summary>
        public string eta { set; get; }

        /// <summary>
        /// 标准化后的预到时间[YYYY] [MM][DD][HH][MM]
        /// </summary>
        public string eta_std { set; get; }

        /// <summary>
        /// 船舶航行状态
        /// </summary>
        public int navistat { set; get; }

        /// <summary>
        /// 纬度，1/1000000 度,[-90000000,90000000]
        /// </summary>
        public int lat { set; get; }

        /// <summary>
        /// 经度，1/1000000 度,[-180000000,180000000]
        /// </summary>
        public int lon { set; get; }

        /// <summary>
        /// 速度，毫米/秒, [0,52576]
        /// </summary>
        public int sog { set; get; }

        /// <summary>
        /// 航迹向，1/100 度, [0,35990]
        /// </summary>
        public int cog { set; get; }

        /// <summary>
        /// 船首向，1/100 度, [0,35900]
        /// </summary>
        public int hdg { set; get; }

        /// <summary>
        /// 转向率，1/100 度/秒, [-1200,1200]
        /// </summary>
        public int rot { set; get; }

        /// <summary>
        /// 更新时间，unix 时间戳
        /// </summary>
        public long lasttime { set; get; }
    }



    public class PXYShipTrackJson
    {
        public int datatype { set; get; }

        public long utc { set; get; }

        public int lon { set; get; }

        public int lat { set; get; }

        public int sog { set; get; }

        public int cog { set; get; }
    }


    public class WindManageShip
    {
        public WindManageShip()
        {
            //PXYInfo = new PXYShipInfo();
        }
        public int ID { set; get; }

        public int CompanyID { set; get; }

        public string ShipName { set; get; }

        public string ShipCode { set; get; }

        public string MMSI { set; get; }

        /// <summary>
        /// 船舶类型
        /// </summary>
        public int ShipType { set; get; }
        public string ShipTypeName { set; get; }
        /// <summary>
        /// 船舶状态0闲置 1正常
        /// </summary>
        public int Status { set; get; }
        public string StatusStr { set; get; }
        public int UseType { set; get; }
        public string UseTypeStr { set; get; }
        public double? Lon { set; get; }
        public double? Lat { set; get; }
        public string LonStr { set; get; }
        public string LatStr { set; get; }

        //public PXYShipInfo PXYInfo { set; get; }

        //--------------------AIS-------------------------------
        public string CallSign { set; get; }

        public string IMO { set; get; }

        public string Weight { set; get; }

        /// <summary>
        /// 吊高
        /// </summary>
        public string LiftHeight { set; get; }

        /// <summary>
        /// 吊重
        /// </summary>
        public string LiftWeight { set; get; }

        public string Length { set; get; }

        public string Width { set; get; }

        /// <summary>
        /// 吃水
        /// </summary>
        public string Draught { set; get; }

        /// <summary>
        /// 顶靠能力
        /// </summary>
        public string DKNL { set; get; }

        /// <summary>
        /// 主机功率
        /// </summary>
        public string ZJGL { set; get; }

        public string Speed { set; get; }

        /// <summary>
        /// 目的地
        /// </summary>
        public string Dest { set; get; }

        /// <summary>
        /// 预到时间
        /// </summary>
        public string YDSJ { set; get; }

        /// <summary>
        /// 船舶状态
        /// </summary>
        public string Navistat { set; get; }

        /// <summary>
        /// 船首向
        /// </summary>
        public string Heading { set; get; }

        /// <summary>
        /// 左舷距
        /// </summary>
        public string Left { set; get; }

        /// <summary>
        /// 船尾距
        /// </summary>
        public string Trail { set; get; }

        public string UpdateTime { set; get; }


        public string Captain { set; get; }

        public string CaptainPhone { set; get; }

        public string Name1 { set; get; }

        public string Phone1 { set; get; }

        public string Name2 { set; get; }

        public string Phone2 { set; get; }

    }
}
