namespace Service.Shj
{
    public class SingleSinOnModel
    {
        public string usrName { set; get; }
        public string usrCode { get; set; }
        public string businessUrl { get; set; }
    }

    public class ShjTokenModel
    {
        public int errcode { set; get; }
        public string errmsg { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }

    public class ShjJJTMessageRequest
    {
        public ShjJJTMessageRequest()
        {
            textcard = new ShjJJTMessageTextcard();
        }
        public string touser { set; get; }

        public string toparty { set; get; }

        public string totag { set; get; }

        public string msgtype { set; get; }

        public int agentid { set; get; }

        public ShjJJTMessageTextcard textcard { set; get; }
    }

    public class ShjJJTMessageTextcard
    {
        public string title { set; get; }

        public string description { set; get; }

        public string url { set; get; }


    }
}
