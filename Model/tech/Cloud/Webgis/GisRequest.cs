using Model.Base;

namespace Model.Tech.Cloud.Webgis
{
    public class GisRequest : BaseRequest
    {
        public int ID { set; get; }

        public int ProjectID { set; get; }

        public int ProjectName { set; get; }

        public string PostToken { set; get; }

        public int FanID { set; get; }

        public int ShipID { set; get; }

        public string StartTime { set; get; }

        public string EndTime { set; get; }
    }
}
