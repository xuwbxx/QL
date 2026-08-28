using Model.Base;

namespace Model.Tech.Tech
{
    public class TechRequest : BaseRequest
    {
        public int ID { set; get; }

        public string UserID { set; get; }

        public string Depart { set; get; }

        public int SoftwareID { set; get; }

        public string Project { set; get; }

        public string SoftwareStartTime { set; get; }

        public string SoftwareEndTime { set; get; }

        public int FlowStatus { set; get; }

        public string Comment { set; get; }
    }
}
