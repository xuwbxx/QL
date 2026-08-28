using Model.Base;

namespace Model.Tech.Cloud.BackManage
{
    public class CloudWindBackManageRequest : BaseRequest
    {
        public CloudWindBackManageRequest()
        {
            ProjectRoles = new List<CloudWindBackManageProjectRole>();
            NodeManagers = new List<CloudWindBackManageUserInfo>();
        }
        public string PostToken { set; get; }
        public int ID { set; get; }

        public int Role { set; get; }

        public string Type { set; get; }

        public bool IsConfirm { set; get; }

        public string UserName { set; get; }

        public string RealName { set; get; }

        public int DepartID { set; get; }

        public string Depart { set; get; }

        public int SoftwareID { set; get; }

        public string SoftwareName { set; get; }

        public string SoftwareComment { set; get; }

        public string SoftwareUrl { set; get; }

        public int NodeID { set; get; }
        public string NodeName { set; get; }

        /// <summary>
        /// 节点是否可编辑
        /// </summary>
        public bool NodeDoEdit { set; get; }

        /// <summary>
        /// 节点是否可以单人审批通过
        /// </summary>
        public bool NodeApprovalType { set; get; }

        public int FlowType { set; get; }

        public string ProjectName { set; get; }

        public int TaskID { set; get; }
        public string TaskName { set; get; }

        public int ProjectID { set; get; }

        public List<CloudWindBackManageProjectRole> ProjectRoles { set; get; }

        public List<CloudWindBackManageUserInfo> NodeManagers { set; get; }

        public int CompanyID { set; get; }

        public string CompanyName { set; get; }

        public string ProjectDirectoryPath { set; get; }

        public string ShipName { set; get; }

        public int ShipIsConfirm { set; get; }


        public int ShipID { set; get; }


        /// <summary>
        /// 船舶信息
        /// </summary>
        /// <summary>
        /// 桩腿截面积
        /// </summary>
        public string Ship_ZTJMJ { set; get; }

        /// <summary>
        /// 桩腿直径
        /// </summary>
        public string Ship_ZTZJ { set; get; }

        /// <summary>
        /// 桩腿周长
        /// </summary>
        public string Ship_ZTZC { set; get; }

        /// <summary>
        /// 桩靴长度
        /// </summary>
        public string Ship_ZXCD { set; get; }

        /// <summary>
        /// 桩靴宽度
        /// </summary>
        public string Ship_ZXKD { set; get; }

        /// <summary>
        /// 桩靴高度
        /// </summary>
        public string Ship_ZXGD { set; get; }

        /// <summary>
        /// 桩靴面积
        /// </summary>
        public string Ship_ZXMJ { set; get; }

        /// <summary>
        /// 桩靴最大截面周长
        /// </summary>
        public string Ship_ZXZDJMZC { set; get; }

        /// <summary>
        /// 桩靴体积
        /// </summary>
        public string Ship_ZXTJ { set; get; }

        /// <summary>
        /// 桩腿、桩靴自重
        /// </summary>
        public string Ship_ZTZXZZ { set; get; }

        /// <summary>
        /// 桩腿预压力
        /// </summary>
        public string Ship_ZTYYL { set; get; }

        /// <summary>
        /// 计算预压荷载
        /// </summary>
        public string Ship_JSYYHZ { set; get; }

        /// <summary>
        /// 拔桩力
        /// </summary>
        public string Ship_BZL { set; get; }

        /// <summary>
        /// 对地比压
        /// </summary>
        public string Ship_DDBY { set; get; }

        /// <summary>
        /// 有效桩腿长度(船底到靴底)
        /// </summary>
        public string Ship_YXZTCD { set; get; }

        /// <summary>
        /// 气隙(船底到水面)
        /// </summary>
        public string Ship_QX { set; get; }

        /// <summary>
        /// 桩腿有效长度
        /// </summary>
        public string Ship_ZTYXCD { set; get; }


        public string OldProjectCode { set; get; }
        public string NewProjectCode { set; get; }
    }

    public class CloudWindBackManageProjectRole
    {
        public int RoleID { set; get; }

        public string UserName { set; get; }

        public string UserCode { set; get; }

        public string UserDepartName { set; get; }

        public string UserPhone { set; get; }

        public string UserJobName { set; get; }
    }

    public class CloudWindBackManageUserInfo
    {
        public string UserName { set; get; }

        public string UserCode { set; get; }

        public string UserDepartName { set; get; }

        public string UserPhone { set; get; }

        public string UserJobName { set; get; }
    }
}
