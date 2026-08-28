namespace Model.Tech.Cloud
{
    public enum CloudWindEnum
    {

    }

    public enum CloudWindProjectStatus
    {
        未知 = 0,
        可研 = 1,
        投标 = 2,
        在建 = 3,
        运维 = 4,
        拆除 = 5,
        中标 = 6
    }

    public enum CloudWindFanStatus
    {
        未安装 = 0,
        在安装 = 1,
        已安装 = 2
    }

    public enum CloudWindProjectFlowStatus
    {
        审批中 = 0,
        退回 = 1,
        激活 = 2,
        删除 = 3
    }

    public enum CloudWindProjectFlowHandleStatus
    {
        待审批 = 0,
        审批通过 = 1,
        退回 = 2,
        删除 = 3
    }

    public enum CloudWindTaskFlowStatus
    {
        审批中 = 0,
        退回 = 1,
        完成 = 2,
        作废 = 3
    }

    public enum CloudWindTaskFlowHandleStatus
    {
        待审批 = 0,
        审批通过 = 1,
        退回 = 2,
        删除 = 3
    }

    //public enum CloudWindProjectFlowNode
    //{
    //    项目部业务员申请 = 1,
    //    项目部经理审批 = 2,
    //    分子公司总工审批 = 3,
    //    公司科技部审批 = 4,
    //    技术中心云平台负责人审批 = 5
    //}

    public enum CloudWindProjectFlowNode
    {
        //项目部业务员申请 = 1,
        //项目部经理审批 = 2,
        //分子公司总工审批 = 3,
        业务人员立项申请 = 4,
        技术中心云平台负责人审批 = 5
    }

    /// <summary>
    /// 插拔桩流程
    /// </summary>
    public enum CloudWindSoftwareCBZFlowNode
    {
        任务申请 = 6,
        技术审核 = 7,
        业务审核 = 11,
        审核通过 = 12
    }

    /// <summary>
    /// 可作业流程
    /// </summary>
    public enum CloudWindSoftwareKZYFlowNode
    {
        任务申请 = 8,
        技术人员审核 = 9,
        审核通过 = 10
    }

    /// <summary>
    /// 桩基沉桩可打性分析
    /// </summary>
    public enum CloudWindSoftwareZJCZFlowNode
    {
        任务申请 = 13,
        技术审核 = 14,
        业务审核 = 15,
        审核通过 = 16
    }

    public enum CloudWindSoftwareTHFXFlowNode
    {
        任务申请 = 17,
        审核通过 = 18
    }

    /// <summary>
    /// 选船分析
    /// </summary>
    public enum CloudWindSoftwareXCFXFlowNode
    {
        任务申请 = 19,
        技术审核 = 20,
        审核通过 = 21
    }

    /// <summary>
    /// 气象收集
    /// </summary>
    public enum CloudWindSoftwareQXSJFlowNode
    {
        任务申请 = 22,
        审核通过 = 23
    }

    //public enum CloudWindSoftwareFlowNode
    //{
    //    项目部总工申请 = 6,
    //    技术中心核对资料 = 7,
    //    技术中心实施技术服务 = 8,
    //    技术中心上传分析结果 = 9
    //}

    //public enum CloudWindSoftwareUseFlowNode
    //{
    //    项目部总工申请 = 10,
    //    使用软件 = 11,
    //    技术中心审核分析结果 = 12,
    //    审核分析通过 = 13
    //}

    public enum CloudWindTaskFileType
    {
        风电项目_机孔位地勘原始资料 = 1,
        风电项目_机孔位地勘数据表 = 2,
        插拔桩_自升式平台船原始资料 = 3,
        插拔桩_自升式平台船数据表 = 4,
        桩基沉桩_桩原始资料 = 5,
        桩基沉桩_桩数据表 = 6,
        桩基沉桩_地勘原始资料 = 7,
        桩基沉桩_地勘数据表 = 8,
        插拔桩_自升式平台船计算参数 = 13,
    }

    public enum CloudWindSoftware
    {
        海上风电WebGIS平台 = 1,
        自升式平台桩腿插拔计算 = 2,
        起重船基础施工可作业性预报 = 3,
        拖航可行性分析与预报 = 4,
        海上风电单桩嵌岩施工孔壁失稳风险评估 = 5,
        桩基沉桩可打性分析 = 6,
        基于历史环境数据的选船分析 = 7,
        海况与气象数据收集 = 8
    }


    public enum CloudWindFlowHandle
    {
        审批退回 = 0,
        新增项目 = 1,
        审批通过 = 2,

    }

    public enum CloudWindFlowType
    {
        项目申请类审批流程 = 0,
        软件委托型审批流程 = 1,
        软件使用类审批流程 = 2,

    }
    public enum CloudWindProjectRole
    {
        项目协调员 = 1,
        项目部领导班子 = 2,
        项目组成员 = 3,
        //项目总工 = 4
    }

    public enum CloudWindTaskKZYTechnology
    {
        大直径单桩基础施工 = 1,
        导管架基础施工 = 2,
        高桩承台基础施工 = 3
    }

    public enum CloudWindTaskSendFileType
    {
        可作业预报文件 = 1,
        拖航可行性分析文件 = 2,
        云推送文件 = 3
    }

    public enum CloudWindTaskReportType
    {
        插拔桩计算原始报告 = 1,
        插拔桩计算修改报告 = 2,
        桩基沉桩可打性分析报告 = 3,
        选船分析报告 = 4
    }
}
