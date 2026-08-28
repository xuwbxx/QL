using Model.Base;

namespace Model.Tech.TechCenter
{
    public class TechCenterRequest : BaseRequest
    {
        public int ID { set; get; }

        public int PlatID { set; get; }

        public string PostToken { set; get; }
    }
}
