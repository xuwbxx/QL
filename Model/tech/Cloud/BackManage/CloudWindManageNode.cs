namespace Model.Tech.Cloud.BackManage
{
    public class CloudWindManageNode
    {
        public CloudWindManageNode()
        {
            NodeManagers = new List<CloudWindManageNodeManager>();
        }
        public int ID { set; get; }

        public int SoftwareID { set; get; }

        public string SoftwareName { set; get; }

        /// <summary>
        /// 节点是否可以担任审批通过
        /// </summary>
        public bool NodeApprovalType { set; get; }

        /// <summary>
        /// 节点审批时，是否可以编辑
        /// </summary>
        public bool DoEdit { set; get; }

        public string FlowTypeName { set; get; }

        public bool ManagerIsSetting { set; get; }

        public string NodeName { set; get; }

        public List<CloudWindManageNodeManager> NodeManagers { set; get; }
    }

    public class CloudWindManageNodeManager
    {
        public int ID { set; get; }

        public string ManageName { set; get; }

        public string ManageUserCode { set; get; }

        public string ManagePhone { set; get; }

        public string ManageDepart { set; get; }

        public string ManageJobName { set; get; }


    }
}
