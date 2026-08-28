namespace Model.Tech.Cloud
{
    public class CloudWindDKData
    {
        public CloudWindDKData()
        {
            Fans = new List<CloudWindDKFan>();
            DKs = new List<CloudWindDKModel>();
        }
        public List<CloudWindDKFan> Fans { set; get; }

        public List<CloudWindDKModel> DKs { set; get; }
    }

    public class CloudWindDKFan
    {
        public CloudWindDKFan()
        {
            DKs = new List<CloudWindDKModel>();
        }
        public int ID { set; get; }

        public string FanName { set; get; }

        public List<CloudWindDKModel> DKs { set; get; }
    }

    public class CloudWindDKModel
    {
        public int ID { set; get; }

        public string DKName { set; get; }

        public int FanID { set; get; }

        public bool IsChecked { set; get; }
    }
}
