namespace Model.Base
{
    public class BaseRequest
    {
        public BaseRequest()
        {
            PageIndex = 1;
            PageSize = 10;
        }

        /// <summary>
        /// 请求方
        /// </summary>
        public string Requester { set; get; }

        /// <summary>
        /// 页数
        /// </summary>
        public int PageIndex { set; get; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageSize { set; get; }

        public int GetSkipCount()
        {
            return (PageIndex - 1) * PageSize;
        }

        /// <summary>
        /// asc  desc
        /// </summary>
        public string Order { set; get; }

        public int OrderID { set; get; }

        public string OrderName { set; get; }

    }
}
