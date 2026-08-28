namespace Model.Tech.Cloud
{
    public class CloudBaseInfoResponse<T>
    {

        public int StatusCode { set; get; }

        public T Data { set; get; }

    }

    public class CloudBaseUserInfo
    {
        public int ID { set; get; }

        public string empcode { set; get; }

        public string name { set; get; }

        public string namespell { set; get; }

        public string wholeDeptPath { set; get; }

        public string certno { set; get; }

        public string birthday { set; get; }

        public string phone { set; get; }

        public string email { set; get; }

        public string jobname { set; get; }
    }
}
