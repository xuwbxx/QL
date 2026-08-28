namespace Model.Tech.Cloud
{
    public class CloudWindProjectFlow
    {
        public CloudWindProjectFlow()
        {
            FlowInfo = new FlowInfo();

            Applyer = new ShjUserInfo();
            ProjectManager = new ShjUserInfo();
            ProjectGroupUser = new List<ShjUserInfo>();


            ProjectPosition = new List<CloudProjectPosition>();
            ProjectFan = new List<CloudProjectFanPosition>();
            ProjectFile = new List<CloudProjectFile>();

            File_dksjb = new List<CloudProjectFile>();
            File_dkyszl = new List<CloudProjectFile>();

            ProjectInfo = new CloudWindProjectBaseInfo();
        }

        public string Name { set; get; }

        public int ProjectID { set; get; }

        public int Status { set; get; }

        public int CompanyID { set; get; }



        public string ProjectName { set; get; }

        public string ProjectCode { set; get; }

        public string ApplyTime { set; get; }

        public string ProjectStartTime { set; get; }

        public string ProjectEndTime { set; get; }

        public string ProjectPositionString { set; get; }

        public string ProjectFanPositionString { set; get; }

        public List<CloudProjectPosition> ProjectPosition { set; get; }

        public List<CloudProjectFanPosition> ProjectFan { set; get; }

        public List<CloudProjectFile> ProjectFile { set; get; }

        /// <summary>
        /// 地勘原始资料
        /// </summary>
        public List<CloudProjectFile> File_dkyszl { set; get; }

        /// <summary>
        /// 地勘数据表
        /// </summary>
        public List<CloudProjectFile> File_dksjb { set; get; }

        /// <summary>
        /// 流程相关内容
        /// </summary>
        public FlowInfo FlowInfo { set; get; }

        /// <summary>
        /// 申请人
        /// </summary>
        public ShjUserInfo Applyer { set; get; }


        /// <summary>
        /// 项目负责人
        /// </summary>
        public ShjUserInfo ProjectManager { set; get; }

        /// <summary>
        /// 项目组成员
        /// </summary>
        public List<ShjUserInfo> ProjectGroupUser { set; get; }


        /// <summary>
        /// 项目基本信息
        /// </summary>
        public CloudWindProjectBaseInfo ProjectInfo { set; get; }
    }

    public class CloudWindProjectBaseInfo
    {
        public string WaterDepth { set; get; }

        public string WaterDepthMax { set; get; }

        public string WaterDepthMin { set; get; }

        public string ProjectStatus { set; get; }

        public string Company { set; get; }
    }


    public class ShjUserInfo
    {
        public string UserName { set; get; }

        public string UserCode { set; get; }

        public string UserDepart { set; get; }

        public string UserPhone { set; get; }

        public string UserJobName { set; get; }
    }


    public class FlowInfo
    {
        public FlowInfo()
        {
            LastNode = new List<NodeInfo>();
            Node = new List<NodeInfo>();
            NextNode = new List<NodeInfo>();
        }

        public bool Success { set; get; }
        public string Message { set; get; }

        /// <summary>
        /// 0:项目申请流程 1：委托类流程  2：使用类流程
        /// </summary>
        //public int FlowType { set; get; }


        /// <summary>
        /// 0:退回  1：同意  2：删除
        /// </summary>
        public int ApprovalType { set; get; }

        /// <summary>
        /// 0:审批中 1：退回  2：激活  3：删除
        /// </summary>
        public int FlowStatus { set; get; }

        /// <summary>
        /// 是否退回流程
        /// </summary>
        public bool IsBackFlow { set; get; }

        /// <summary>
        /// 新增new  查看view  审批approval  重填renew  修改update
        /// </summary>
        public string FlowHandle { set; get; }

        /// <summary>
        /// 是否使用软件的节点
        /// </summary>
        public bool IsUsingFlowNode { set; get; }

        public bool MultipleLastNode { set; get; }
        public int LastNodeID { set; get; }
        public string LastNodeName { set; get; }
        public List<NodeInfo> LastNode { set; get; }

        public bool MultipleNode { set; get; }
        public int NodeID { set; get; }
        public string NodeName { set; get; }
        public List<NodeInfo> Node { set; get; }
        public bool DoEdit { set; get; }

        public bool DoEditConfirm { set; get; }

        /// <summary>
        /// 是否单人审批通过
        /// </summary>
        public bool NodeApprovalType { set; get; }

        public bool MultipleNextNode { set; get; }
        public int NextNodeID { set; get; }
        public string NextNodeName { set; get; }
        public List<NodeInfo> NextNode { set; get; }


        public bool IsCreateProjectCode { set; get; }

    }


    public class NodeInfo
    {
        public NodeInfo()
        {
            CommentFiles = new List<CloudProjectFile>();
        }
        //public int NodeID { set; get; }

        //public string NodeName { set; get; }

        public string Comment { set; get; }

        public string UserName { set; get; }

        public string UserCode { set; get; }

        public string UserDepart { set; get; }

        public string UserPhone { set; get; }

        public string UserJobName { set; get; }

        /// <summary>
        /// 0：待审批  1：审批通过  2：退回  3：删除
        /// </summary>
        public int FlowHandle { set; get; }

        public List<CloudProjectFile> CommentFiles { set; get; }

    }



    public class CloudWindTaskFlow
    {
        public CloudWindTaskFlow()
        {
            FlowInfo = new FlowInfo();

            DeliverUser = new ShjUserInfo();

            File_dkyszl = new List<CloudProjectFile>();
            File_dksjb = new List<CloudProjectFile>();
            File_ptcsjb = new List<CloudProjectFile>();
            File_ptcyszl = new List<CloudProjectFile>();

            File_gcxmjbzl = new List<CloudProjectFile>();
            File_jkwdksjb = new List<CloudProjectFile>();
            File_jkwdkzl = new List<CloudProjectFile>();
            File_sggy = new List<CloudProjectFile>();
            File_sjsgqk = new List<CloudProjectFile>();
            File_zyhj = new List<CloudProjectFile>();
            File_qyzbzl = new List<CloudProjectFile>();
            File_ljxmsgzl = new List<CloudProjectFile>();



            Report = new CloudWindTaskReport();

            TaskInfo_KZY = new CloudWindTaskInfo_KZY();

            TaskInfo_ZJCZ = new CloudWindTaskInfo_ZJCZ();

            TaskInfo_THFX = new CloudWindTaskInfo_THFX();

            TaskInfo_XCFX = new CloudWindTaskInfo_XCFX();

            TaskInfo_QXSJ = new CloudWindTaskInfo_QXSJ();

            Node = new CloudWindNextNode();

            LastNodeCommentFiles = new List<CloudProjectFile>();
        }

        public int TaskID { set; get; }

        public int ProjectID { set; get; }

        public string TaskName { set; get; }

        public string TaskCode { set; get; }

        public int TaskStatus { set; get; }

        public string DeliverTime { set; get; }


        public FlowInfo FlowInfo { set; get; }

        public ShjUserInfo DeliverUser { set; get; }

        public CloudWindTaskReport Report { set; get; }

        /// <summary>
        /// 地勘原始资料
        /// </summary>
        public List<CloudProjectFile> File_dkyszl { set; get; }

        /// <summary>
        /// 地勘数据表
        /// </summary>
        public List<CloudProjectFile> File_dksjb { set; get; }

        //插拔桩软件
        /// <summary>
        /// 平台船原始资料
        /// </summary>
        public List<CloudProjectFile> File_ptcyszl { set; get; }

        /// <summary>
        /// 平台船数据表
        /// </summary>
        public List<CloudProjectFile> File_ptcsjb { set; get; }


        //桩基
        /// <summary>
        /// 工程项目基本资料
        /// </summary>
        public List<CloudProjectFile> File_gcxmjbzl { set; get; }
        /// <summary>
        /// 机孔位地勘资料
        /// </summary>
        public List<CloudProjectFile> File_jkwdkzl { set; get; }
        /// <summary>
        /// 机孔位地勘数据表
        /// </summary>
        public List<CloudProjectFile> File_jkwdksjb { set; get; }
        /// <summary>
        /// 嵌岩装备资料
        /// </summary>
        public List<CloudProjectFile> File_qyzbzl { set; get; }
        /// <summary>
        /// 施工工艺
        /// </summary>
        public List<CloudProjectFile> File_sggy { set; get; }
        /// <summary>
        /// 作业环境
        /// </summary>
        public List<CloudProjectFile> File_zyhj { set; get; }
        /// <summary>
        /// 临近项目施工资料
        /// </summary>
        public List<CloudProjectFile> File_ljxmsgzl { set; get; }
        /// <summary>
        /// 实际施工情况
        /// </summary>
        public List<CloudProjectFile> File_sjsgqk { set; get; }

        public CloudWindTaskInfo_KZY TaskInfo_KZY { set; get; }

        public CloudWindNextNode Node { set; get; }

        public List<CloudProjectFile> LastNodeCommentFiles { set; get; }

        public CloudWindTaskInfo_ZJCZ TaskInfo_ZJCZ { set; get; }

        public CloudWindTaskInfo_THFX TaskInfo_THFX { set; get; }

        public CloudWindTaskInfo_XCFX TaskInfo_XCFX { set; get; }

        public CloudWindTaskInfo_QXSJ TaskInfo_QXSJ { set; get; }
    }


    public class CloudWindTaskReport
    {
        public CloudWindTaskReport()
        {
            ReportFile = new List<CloudProjectFile>();
            ReportModifyFile = new List<CloudProjectFile>();
            KZYReportFile = new List<CloudProjectFile>();
        }

        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsDisplay { set; get; }

        /// <summary>
        /// 是否可以上传和删除
        /// </summary>
        public bool IsOperate { set; get; }



        public List<CloudProjectFile> ReportFile { set; get; }

        public List<CloudProjectFile> ReportModifyFile { set; get; }

        public List<CloudProjectFile> KZYReportFile { set; get; }
    }

    public class CloudWindTaskInfo_KZY
    {
        public CloudWindTaskInfo_KZY()
        {
            ShipIDs = new List<int>();
        }
        public List<int> ShipIDs { set; get; }

        public string Lon { set; get; }

        public string Lat { set; get; }

        public string WaterDepth { set; get; }

        public string Standard { set; get; }

        public string ForecastStartTime { set; get; }

        public string ForecastEndTime { set; get; }

        public int TechType { set; get; }

        public string Balance { set; get; }

        public string YFYNo { set; get; }

    }


    public class CloudWindTaskInfo_ZJCZ
    {
        public CloudWindTaskInfo_ZJCZ()
        {
            HammerModels = new List<string>();
            ProjectPositionImgs = new List<CloudProjectFile>();
            File_Pile_YSZL = new List<CloudProjectFile>();
            File_Pile_SJB = new List<CloudProjectFile>();
        }

        public List<string> HammerModels { set; get; }

        public string ProjectPosition { set; get; }

        public List<CloudProjectFile> ProjectPositionImgs { set; get; }

        public List<CloudProjectFile> File_Pile_YSZL { set; get; }

        public List<CloudProjectFile> File_Pile_SJB { set; get; }

        public string PileReplace { set; get; }

        public string ProjectIntroduce { set; get; }


    }


    public class CloudWindTaskInfo_THFX
    {
        public CloudWindTaskInfo_THFX()
        {
            Files = new List<CloudProjectFile>();
        }
        public string Scheme { set; get; }

        public List<CloudProjectFile> Files { set; get; }

    }


    public class CloudWindTaskInfo_XCFX
    {
        public CloudWindTaskInfo_XCFX()
        {
            ShipIDs = new List<int>();
        }
        public string TaskDesc { set; get; }

        public List<int> ShipIDs { set; get; }

    }

    public class CloudWindTaskInfo_QXSJ
    {
        public CloudWindTaskInfo_QXSJ()
        {
            Points = new List<CloudWindCollectPoint_QXSJ>();
        }

        public string CollectStartTime { set; get; }

        public string CollectEndTime { set; get; }

        public List<CloudWindCollectPoint_QXSJ> Points { set; get; }
    }



}
