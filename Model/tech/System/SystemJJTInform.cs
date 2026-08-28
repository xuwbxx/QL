namespace Model.Tech.System
{
    public class SystemJJTInform
    {
        public string UserCode { set; get; }

        public string Url { set; get; }

        public string Title { set; get; }

        public string Content { set; get; }
    }

    public class JJTInformRequest
    {
        public string UserCode { set; get; }

        public string Url { set; get; }

        public string Title { set; get; }

        public string Description { set; get; }
    }
}
