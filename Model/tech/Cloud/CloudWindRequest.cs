using Model.Base;

namespace Model.Tech.Cloud
{
    public class CloudWindRequest : BaseRequest
    {
        public CloudWindRequest()
        {
            ProjectPosition = new List<CloudProjectPosition>();
            ProjectFanPosition = new List<CloudProjectFanPosition>();
            Fans = new List<CloudWindDKFan>();


        }

        public int ID { set; get; }

        public string InitType { set; get; }

        public string PostToken { set; get; }
        public int ProjectID { set; get; }

        public string ProjectName { set; get; }

        public string ProjectCode { set; get; }

        public string TaskName { set; get; }

        public int TaskID { set; get; }

        public string Name { set; get; }


        /// <summary>
        /// 项目组成员
        /// </summary>
        public string ProjectGroupUsers { set; get; }

        public string ProjectManagerName { set; get; }
        public string ProjectManagerUserCode { set; get; }
        public string ProjectManagerDepart { set; get; }
        public string ProjectManagerPhone { set; get; }
        public string ProjectManagerJobName { set; get; }
        public string ProjectStartTime { set; get; }
        public string ProjectEndTime { set; get; }

        public string ProjectComment { set; get; }

        public string TaskComment { set; get; }

        public string ProjectPositionString { set; get; }

        public string ProjectFanPositionString { set; get; }

        public List<CloudProjectPosition> ProjectPosition { set; get; }

        public List<CloudProjectFanPosition> ProjectFanPosition { set; get; }


        /// <summary>
        /// 文件接收人
        /// </summary>
        public string TaskDeliverName { set; get; }

        public string TaskDeliverUserCode { set; get; }

        public string TaskDeliverDepart { set; get; }

        public string TaskDeliverPhone { set; get; }

        public string TaskDeliverJobName { set; get; }

        public string TaskDeliverTime { set; get; }

        public int SoftwareID { set; get; }

        public int SoftwareUseType { set; get; }
        public string ProjectNodeName { set; get; }
        public string ProjectNodeUserCode { set; get; }
        public string ProjectNodeDepart { set; get; }
        public string ProjectNodePhone { set; get; }
        public string ProjectNodeJobName { set; get; }




        public string TaskNodeName { set; get; }
        public string TaskNodeUserCode { set; get; }
        public string TaskNodeDepart { set; get; }
        public string TaskNodePhone { set; get; }
        public string TaskNodeJobName { set; get; }

        /// <summary>
        /// 0：审批退回 2：审批通过
        /// </summary>
        public int ApprovalType { set; get; }

        public int NodeID { set; get; }

        public bool IsBackFlow { set; get; }

        /// <summary>
        /// 0: 审批中 1：退回  2：完成  3：删除
        /// </summary>
        public int FlowStatus { set; get; }

        public bool DoEdit { set; get; }


        public List<CloudWindDKFan> Fans { set; get; }

        /// <summary>
        /// 1:地勘原始文件 2.地勘数据表
        /// </summary>
        public int FileType { set; get; }


        /// <summary>
        /// 流程操作类型 new approval renew view
        /// </summary>
        public string FlowHandle { set; get; }

        /// <summary>
        /// 地勘数据ID
        /// </summary>
        public List<int> DKIDs { set; get; }

        public string DKIDstring { set; get; }


        public int ShipID { set; get; }

        public List<int> ShipIDs { set; get; }

        public string ShipIDstring { set; get; }

        /// <summary>
        /// 桩腿截面积
        /// </summary>
        public string ShipZTJMJ { set; get; }

        /// <summary>
        /// 桩腿预压力
        /// </summary>
        public string ShipZTYYL { set; get; }

        /// <summary>
        /// 计算预压荷载
        /// </summary>
        public string ShipJSYYHZ { set; get; }

        /// <summary>
        /// 对地比压
        /// </summary>
        public string ShipDDBY { set; get; }

        public string ShipName { set; get; }



        //基本信息
        public int CompanyID { set; get; }

        public string WaterDepth { set; get; }
        public string WaterDepthMin { set; get; }
        public string WaterDepthMax { set; get; }

        public int Status { set; get; }


        #region 可作业
        public string KZYForecastStartTime { set; get; }
        public string KZYForecastEndTime { set; get; }
        public string KZYLon { set; get; }
        public string KZYLat { set; get; }
        public string KZYWaterDepth { set; get; }
        public string KZYStandard { set; get; }

        public string KZYTechType { set; get; }

        /// <summary>
        /// 亿方云编号
        /// </summary>
        public string KZYYFYNo { set; get; }
        /// <summary>
        /// 衡准
        /// </summary>
        public string KZYBalance { set; get; }


        public int ReportSize { set; get; }

        #endregion


        #region 桩基沉桩

        public string ZJCZFileType { set; get; }

        public string ZJCZProjectIntroduce { set; get; }

        /// <summary>
        /// 项目位置
        /// </summary>
        public string ZJCZProjectPosition { set; get; }

        /// <summary>
        /// 桩基沉桩
        /// </summary>
        public string PileReplace { set; get; }

        public string HammerModel { set; get; }



        #endregion


        #region 拖航分析

        /// <summary>
        /// 被拖物
        /// </summary>
        public string THFX_Scheme { set; get; }



        #endregion


        #region 选船分析

        /// <summary>
        /// 工作描述
        /// </summary>
        public string XCFX_TaskDesc { set; get; }



        #endregion


        #region 气象收集

        public string QXSJ_StartTime { set; get; }

        public string QXSJ_EndTime { set; get; }

        public string QXSJ_Points { set; get; }

        #endregion

    }

    public class CloudWindCollectPoint_QXSJ
    {
        public double? PointLon { set; get; }

        public double? PointLat { set; get; }

        public string PointName { set; get; }

    }


}
