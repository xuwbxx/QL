namespace Model.AI
{
    // API请求模型
    public class DifyChatRequest
    {
        public DifyChatRequest()
        {
            WindApiRequest = new Wind_DifyApiRequest();
        }

        public string PostUtl { set; get; }

        public string ApiKey { set; get; }

        public string Query { set; get; }

        public string user { set; get; }

        public Wind_DifyApiRequest WindApiRequest { set; get; }

    }

    public class Wind_DifyApiRequest
    {
        public string DataBase { set; get; }

        public string Password { set; get; }
    }
}
