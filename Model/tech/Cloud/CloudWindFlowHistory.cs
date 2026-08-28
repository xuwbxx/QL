namespace Model.Tech.Cloud
{
    public class CloudWindFlowHistory
    {
        public CloudWindFlowHistory()
        {
            Flow = new List<CloudWindFlowHistoryInfo>();
        }

        public int ID { set; get; }

        public string NodeName { set; get; }
        public string FlowTime { set; get; }

        /// <summary>
        /// 1：单人审批 2：多人审批
        /// </summary>
        public int ApprovalType { set; get; }

        /// <summary>
        /// 0:开始  1：正常  2：结束
        /// </summary>
        public int FlowType { set; get; }

        /// <summary>
        /// CloudWindProjectFlowHandleStatus 流程操作  0：待审批  1：审批通过  2：退回  3：删除
        /// </summary>
        public int FlowHandle { set; get; }

        public string FlowHandleName { set; get; }

        public List<CloudWindFlowHistoryInfo> Flow { set; get; }
    }

    public class CloudWindFlowHistoryInfo
    {
        public string NodeUserName { set; get; }

        public string NodeUserJobName { set; get; }

        public string NodeUserDepart { set; get; }

        public string Comment { set; get; }

        /// <summary>
        /// 0：待审批  1：审批通过  2：退回  3：删除
        /// </summary>
        public int FlowHandle { set; get; }

        public string FlowHandleName { set; get; }

        public string FlowTime { set; get; }
    }
}
