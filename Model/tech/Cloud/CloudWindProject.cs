namespace Model.Tech.Cloud
{
    public class CloudWindProject
    {
        public CloudWindProject()
        {
            Applyer = new CloudWindProjectApplyer();
            Director = new CloudWindProjectDirector();
            NodeUser = new List<CloudWindProjectNodeUser>();
        }

        public int ID { set; get; }

        public string ProjectCode { set; get; }

        public string ProjectName { set; get; }

        public string ProjectStatus { set; get; }

        public bool IsFinished { set; get; }

        /// <summary>
        /// 新增new  查看view  审批approval  重填renew
        /// </summary>
        public string FlowHandle { set; get; }

        public string ApplyTime { set; get; }

        public int FlowID { set; get; }

        public string FlowNode { set; get; }

        public int FlowStatus { set; get; }

        public string FlowStatusName { set; get; }

        //public bool IsCurrentNodeUser { set; get; }

        public string ProjectStartTime { set; get; }

        public string ProjectEndTime { set; get; }

        public bool DoRightEdit { set; get; }

        public bool DoRightDelete { set; get; }

        public string Assister { set; get; }

        public bool IsWebGIS { set; get; }

        public CloudWindProjectApplyer Applyer { set; get; }

        public CloudWindProjectDirector Director { set; get; }

        public List<CloudWindProjectNodeUser> NodeUser { set; get; }
    }

    public class CloudWindProjectApplyer
    {
        public string ApplyerName { set; get; }

        public string ApplyerDepart { set; get; }

        public string ApplyerPhone { set; get; }

        public string ApplyerJobName { set; get; }
    }

    public class CloudWindProjectDirector
    {
        public string DirectorName { set; get; }

        public string DirectorDepart { set; get; }

        public string DirectorPhone { set; get; }

        public string DirectorJobName { set; get; }
    }

    public class CloudWindProjectNodeUser
    {
        public string NodeUserCode { set; get; }
        public string NodeUserName { set; get; }

        public string NodeUserDepart { set; get; }

        public string NodeUserPhone { set; get; }

        public string NodeUserJobName { set; get; }
    }


    public class CloudWindTask
    {
        public CloudWindTask()
        {
            Applyer = new CloudWindProjectApplyer();
            NodeUser = new List<CloudWindProjectNodeUser>();
        }

        //public int Compare(int x, int y)
        //{
        //    // 实现自定义比较逻辑
        //    // 例如，这里将字符串按照它们的长度进行排序
        //    return x.CompareTo(y);
        //}


        public int ID { set; get; }

        public int ProjectID { set; get; }
        public string TaskCode { set; get; }

        public string TaskName { set; get; }

        public string ProjectCode { set; get; }

        public string ProjectName { set; get; }

        public bool IsFinished { set; get; }

        public bool IsUseStatus { set; get; }

        public string ApplyTime { set; get; }

        public int FlowID { set; get; }

        public string FlowNode { set; get; }

        public int FlowStatus { set; get; }

        public string FlowStatusName { set; get; }

        public bool IsCurrentNodeUser { set; get; }
        public int SoftwareID { set; get; }
        public string SoftwareName { set; get; }


        public string SoftUrl { set; get; }

        /// <summary>
        /// 新增new  查看view  审批approval  重填renew
        /// </summary>
        public string FlowHandle { set; get; }

        public bool IsUsingFlow { set; get; }


        public CloudWindProjectApplyer Applyer { set; get; }

        public List<CloudWindProjectNodeUser> NodeUser { set; get; }
    }




}
