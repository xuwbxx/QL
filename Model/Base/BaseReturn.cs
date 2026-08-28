namespace Model.Base
{
    public class BaseReturn
    {

        public BaseReturn()
        {
            Success = false;
            Message = "失败";
        }
        public bool Success { set; get; }
        public string Message { set; get; }

        public int TotalPage { set; get; }
        public int TotalCount { set; get; }

        public int PageIndex { set; get; }

        public object Data { set; get; }
        public object Foot { set; get; }

        public object Echart { set; get; }
        public int value { set; get; }
        public string Url { set; get; }

        public string Order { set; get; }

        /// <summary>
        /// 用于请求接口的token
        /// </summary>
        public string PostToken { set; get; }

    }
}
