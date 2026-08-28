namespace Model.Tech.WebApi
{
    public class CloudWindMonitorRequest
    {
        public CloudWindMonitorRequest()
        {
            data = new List<CloudWindMonitorData>();
        }

        public string text { set; get; }

        public string msg { set; get; }

        public int code { set; get; }

        public int SourceID { set; get; }

        public List<CloudWindMonitorData> data { set; get; }


        public string file64 { set; get; }

        public string fileName { set; get; }

        public DateTime? snapTime { set; get; }

    }

    public class CloudWindMonitorData
    {
        public decimal angrate_y { set; get; }

        public string unique_index { set; get; }

        public decimal roll { set; get; }

        public decimal posmru_f { set; get; }

        public decimal posmru_s { set; get; }

        public decimal angrate_r { set; get; }

        public decimal pitch { set; get; }

        public decimal posmru_d { set; get; }

        public decimal angrate_p { set; get; }

        public decimal yaw { set; get; }

        public DateTime ts { set; get; }

    }
}
