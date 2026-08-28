namespace Model.Tech.Cloud.Webgis
{
    /// <summary>
    /// 船顺网模型
    /// </summary>
    public class ShipCSWResponse
    {
        public long time { set; get; }

        public string wait { set; get; }

        public string data { set; get; }

        public string loading { set; get; }

        public List<string> map { set; get; }
    }

    public class ShipCSWModel
    {
        /// <summary>
        /// mmsi
        /// </summary>
        public string id { set; get; }
        /// <summary>
        /// 船舶类型
        /// </summary>
        public string type { set; get; }
        /// <summary>
        /// 中文名
        /// </summary>
        public string cnname { set; get; }
        /// <summary>
        /// 英文名
        /// </summary>
        public string enname { set; get; }
        /// <summary>
        /// 船名
        /// </summary>
        public string name { set; get; }
        /// <summary>
        /// 分组信息（SHIP、VNM、RADIO、NETSONDE）
        /// </summary>
        public string g { set; get; }
        /// <summary>
        /// 船长
        /// </summary>
        public decimal len { set; get; }
        /// <summary>
        /// 船宽
        /// </summary>
        public decimal wid { set; get; }
        /// <summary>
        /// gps 天线位置，距船首
        /// </summary>
        public decimal a { set; get; }
        /// <summary>
        /// 距船尾
        /// </summary>
        public decimal b { set; get; }
        /// <summary>
        /// 距左舷
        /// </summary>
        public decimal c { set; get; }
        /// <summary>
        /// 距右舷
        /// </summary>
        public decimal d { set; get; }
        /// <summary>
        /// 经纬度
        /// </summary>
        public string geom { set; get; }
        /// <summary>
        /// 经度
        /// </summary>
        public decimal lon { set; get; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal lat { set; get; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public long time { set; get; }
        /// <summary>
        /// 速度
        /// </summary>
        public decimal spd { set; get; }
        /// <summary>
        /// 来源
        /// </summary>
        public string from { set; get; }
        /// <summary>
        /// 状态
        /// </summary>
        public string status { set; get; }
        /// <summary>
        /// 船艏向
        /// </summary>
        public string hdg { set; get; }
        /// <summary>
        /// 航迹向
        /// </summary>
        public string cog { set; get; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public string mt { set; get; }
        /// <summary>
        /// 备用字段（暂无作用）
        /// </summary>
        public object isactive { set; get; }
        /// <summary>
        /// 备用字段（暂无作用）
        /// </summary>
        public object num { set; get; }
        /// <summary>
        /// 备用字段（暂无作用）
        /// </summary>
        public object ord { set; get; }
        /// <summary>
        /// 设备定位类型
        /// </summary>
        public string postype { set; get; }
        /// <summary>
        /// 设备定位类型
        /// </summary>
        public string pt { set; get; }
        /// <summary>
        /// 设备厂商
        /// </summary>
        public string firm { set; get; }
        /// <summary>
        /// 总吨
        /// </summary>
        public string gt { set; get; }
    }


    public class ShipCSWInfo
    {
        public string MMSI { set; get; }

        public string ShipName { set; get; }

        public string Lon { set; get; }

        public string Lat { set; get; }

        public string Time { set; get; }
    }
}
