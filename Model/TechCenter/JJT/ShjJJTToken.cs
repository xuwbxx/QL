namespace Model.TechCenter.JJT
{
    public class ShjJJTToken
    {
        public int errcode { set; get; }
        public string? errmsg { get; set; }
        public string? access_token { get; set; }
        public int expires_in { get; set; }
    }

    public class ShjJJTInform
    {
        public string? UserCode { set; get; }

        public string? Url { set; get; }

        public string? Title { set; get; }

        public string? Content { set; get; }
    }

    public class ShjJJTMessageRequest
    {
        public ShjJJTMessageRequest()
        {
            textcard = new ShjJJTMessageTextcard();
        }
        public string? touser { set; get; }

        public string? toparty { set; get; }

        public string? totag { set; get; }

        public string? msgtype { set; get; }

        public int agentid { set; get; }

        public ShjJJTMessageTextcard textcard { set; get; }
    }

    public class ShjJJTMessageTextcard
    {
        public string? title { set; get; }

        public string? description { set; get; }

        public string? url { set; get; }

    }

    public class ShjJJTMessageTCRequest
    {
        public ShjJJTMessageTCRequest()
        {
            userlist = new List<string>();
        }

        public string? title { set; get; }

        public List<string> userlist { set; get; }

        public string? content { set; get; }

        public string? url { set; get; }
    }

    public class ShjJJTMessageTC
    {
        /// <summary>
        /// 系统标识
        /// </summary>
        public string? sysType { set; get; }

        /// <summary>
        /// 消息类型  text 普通消息  textcard 卡片消息
        /// </summary>
        public string? msgtype { set; get; }

        /// <summary>
        /// 接收消息人员列表 数组，多人的话用逗号隔开 
        /// </summary>
        public List<string>? userlist { set; get; }

        /// <summary>
        /// 卡片消息标题  当msgtype=textcard 时，此字段显示在卡片标题侧
        /// </summary>
        public string? title { set; get; }

        /// <summary>
        /// 消息内容  
        /// </summary>
        public string? content { set; get; }

        /// <summary>
        /// 接收消息单点的地址
        /// </summary>
        public string? url { set; get; }

        /// <summary>
        /// 二次跳转地址  若需要单点后在三方系统内二次跳转的，需传二次跳转路径
        /// </summary>
        public string? detailUrl { set; get; }

        /// <summary>
        /// 发送卡片消息人姓名  当msgtype=textcard 时，此字段为发送卡片来源的人名
        /// </summary>
        public string? senderName { set; get; }

        /// <summary>
        /// 消息框跳转详情页打开方式	消息打开方式： 默认1：外部浏览器打开，0:交建通内置浏览器打开
        /// </summary>
        public string? openTarget { set; get; }
    }


    public class ShjJJTMessageTCResponse
    {
        public int StatusCode { set; get; }

        public object? Data { set; get; }

        public string? JsonStr { set; get; }

        public string? Info { set; get; }

        public string? Debug { set; get; }

        public string? ManageInfo { set; get; }

        public string? FBFileInfo { set; get; }

        public int Count { set; get; }

        public int TotalCount { set; get; }

        public int PageIndex { set; get; }

        public int PageSize { set; get; }
    }
}
