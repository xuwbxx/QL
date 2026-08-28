using Model.Base;

namespace Model.TechCenter.Monitor
{
    public class MonitorRequest : BaseRequest
    {
        public MonitorRequest() { }

        public int ID { set; get; }

        public string Name { set; get; }

        public string Description { set; get; }
    }
}
