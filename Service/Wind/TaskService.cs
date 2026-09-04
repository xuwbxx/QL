using BIM.Business.CCSHJWebApi;
using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using DataFactory.KingBase.Towing;
using Microsoft.EntityFrameworkCore;
using Model.Tech.Cloud;
using Model.Tech.System;
using Service.Base;
using System.Data;
using Tool;

namespace Service.Wind
{
    public class TaskService : WindBaseService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingUowFactory;
        private readonly CloudCenterService _cloudCenterService;
        private readonly CloudWindInfoService _cloudWindInfoService;

        public TaskService(
            CloudWind_KingBase_UnitOfWorkFactory techCenterUowFactory,
            Towing_KingBase_UnitOfWorkFactory towingUowFactory,
            CookieService cookieService,
            CloudCenterService cloudCenterService,
            CloudWindInfoService cloudWindInfoService)
            : base(techCenterUowFactory, cookieService)
        {
            _towingUowFactory = towingUowFactory;
            _cloudCenterService = cloudCenterService;
            _cloudWindInfoService = cloudWindInfoService;
        }

        /// <summary>
        /// 获取所有未删除的软件列表
        /// </summary>
        public List<Manage_Software> GetSoftwares()
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Manage_Software>();
                return repo.Find(a => !a.IsDelete).ToList();
            }
        }

        /// <summary>
        /// 根据ID获取项目
        /// </summary>
        public Wind_Project? GetProjectById(int projectId)
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Wind_Project>();
                return repo.Find(a => !a.IsDelete && a.ID == projectId).FirstOrDefault();
            }
        }

        /// <summary>
        /// 根据ID获取任务
        /// </summary>
        public Wind_Task? GetTaskById(int taskId)
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Wind_Task>();
                return repo.Find(a => !a.IsDelete && a.ID == taskId).FirstOrDefault();
            }
        }

        /// <summary>
        /// 检查用户是否有任务查看权限
        /// </summary>
        public async Task<bool> HasTaskViewRight(int taskId, string userCode)
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var taskRepo = uow.GetRepository<Wind_Task>();
                var task = taskRepo.Find(a => !a.IsDelete && a.ID == taskId).FirstOrDefault();
                if (task == null) return false;

                // 申请人
                if (task.ApplyerCode == userCode) return true;

                // 项目组成员
                var roleRepo = uow.GetRepository<Wind_ProjectRole>();
                var roles = roleRepo.Find(a => !a.IsDelete && a.ProjectID == task.ProjectID).ToList();
                if (roles.Exists(a => a.UserCode == userCode)) return true;

                // 管理员
                var admins = await GetAdminUserCode();
                if (admins.Contains(userCode)) return true;

                return false;
            }
        }

        /// <summary>
        /// 获取待审批且是自己的任务数量
        /// </summary>
        public async Task<int> GetTaskCount(string userCode)
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var taskRepo = uow.GetRepository<Wind_Task>();

                // 获取管理员列表
                var admins = await GetAdminUserCode();

                // 查询待审批任务
                var pendingTasks = taskRepo.Find(a => !a.IsDelete && a.FlowStatus == (int)CloudWindTaskFlowStatus.审批中).ToList();

                // 筛选出与当前用户相关的任务
                var userTaskIds = pendingTasks
                    .Where(a => a.ApplyerCode == userCode || admins.Contains(userCode))
                    .Select(a => a.ID)
                    .ToHashSet();

                // 通过项目角色关联的任务
                var roleRepo = uow.GetRepository<Wind_ProjectRole>();
                var userRoles = roleRepo.Find(a => !a.IsDelete && a.UserCode == userCode).ToList();
                var roleProjectIds = userRoles.Select(a => a.ProjectID).ToHashSet();

                var roleTaskIds = pendingTasks
                    .Where(a => roleProjectIds.Contains(a.ProjectID))
                    .Select(a => a.ID)
                    .ToHashSet();

                userTaskIds.UnionWith(roleTaskIds);

                return userTaskIds.Count;
            }
        }
        /// <summary>
        /// 查询任务列表
        /// </summary>
        public async Task<List<CloudWindTask>> TaskListQuery(CloudWindRequest request)
        {
            var list = new List<CloudWindTask>();
            try
            {
                var userCode = UserCode;
                var admins = await GetAdminUserCode();
                var cbzCopyers = await GetCopyUserCode((int)CloudWindSoftware.自升式平台桩腿插拔计算);
                var kzyCopyers = await GetCopyUserCode((int)CloudWindSoftware.起重船基础施工可作业性预报);

                // 预计算列表Contains，避免Kdbndp翻译问题
                bool isCurrentUserAdmin = admins.Contains(userCode);
                bool isCBZCopyer = cbzCopyers.Contains(userCode);
                bool isKZYCopyer = kzyCopyers.Contains(userCode);

                using (var uow = _techCenterUowFactory.Create())
                {
                    var viewTaskFlowRepo = uow.GetRepository<View_Wind_TaskFlow>();

                    var taskFlowList = await viewTaskFlowRepo.FindAsync(a =>
                        (request.ProjectID == 0 || a.ProjectID == request.ProjectID) &&
                        (request.SoftwareID == 0 || a.SoftwareID == request.SoftwareID) &&
                        (string.IsNullOrEmpty(request.TaskName) || EF.Functions.Like(a.TaskName, $"%{request.TaskName}%")) &&
                        (string.IsNullOrEmpty(request.ProjectName) || EF.Functions.Like(a.ProjectName, $"%{request.ProjectName}%")) &&
                        (request.FlowStatus == 100 || a.FlowStatus == request.FlowStatus) &&
                        (a.ApplyerCode == userCode || a.NodeUserCode == userCode || a.DeliverCode == userCode ||
                         (!(a.IsRoleExist == true) && a.RoleID == (int)CloudWindProjectRole.项目部领导班子 && a.RoleUserCode == userCode) ||
                         isCurrentUserAdmin ||
                         (a.SoftwareID == (int)CloudWindSoftware.自升式平台桩腿插拔计算 && isCBZCopyer) ||
                         (a.SoftwareID == (int)CloudWindSoftware.起重船基础施工可作业性预报 && isKZYCopyer)));

                    var viewFlowRepo = uow.GetRepository<View_Flow_TaskApply>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();

                    taskFlowList.ToList()
                        .GroupBy(a => a.TaskID)
                        .ToList()
                        .ForEach(a =>
                        {
                            var first = a.First();
                            int taskId = first.TaskID.Value;

                            var model = new CloudWindTask();
                            model.ID = taskId;
                            model.ProjectID = first.ProjectID.Value;
                            model.TaskName = first.TaskName;
                            model.TaskCode = first.TaskCode;
                            model.ProjectName = first.ProjectName;
                            model.ProjectCode = first.ProjectCode;
                            model.IsFinished = first.FlowStatus == (int)CloudWindTaskFlowStatus.完成;

                            model.Applyer.ApplyerName = first.Applyer ?? "";
                            model.Applyer.ApplyerDepart = first.ApplyerDepart ?? "";
                            model.Applyer.ApplyerPhone = first.ApplyerPhone ?? "";
                            model.Applyer.ApplyerJobName = first.ApplyerJobName ?? "";

                            model.ApplyTime = first.CreateTime?.ToString("yyyy-MM-dd HH:mm") ?? "";
                            model.FlowStatus = first.FlowStatus.Value;
                            model.FlowStatusName = Enum.GetName(typeof(CloudWindTaskFlowStatus), first.FlowStatus);
                            model.SoftwareID = first.SoftwareID.Value;
                            model.SoftwareName = first.SoftwareName;

                            var latestFlow = viewFlowRepo.Find(b => b.TaskID == taskId)
                                .OrderByDescending(b => b.CreateTime).FirstOrDefault();
                            model.FlowID = latestFlow.id;
                            model.FlowNode = latestFlow.NodeName;

                            var allFlows = flowRepo.Find(b => !(b.IsDelete == true) && b.TaskID == taskId).ToList();

                            var thisFlows = allFlows
                                .Where(b => b.NodeID == latestFlow.NodeID && b.FlowOrder == latestFlow.FlowOrder)
                                .OrderBy(b => b.FlowOrder).ToList();
                            thisFlows.ForEach(b =>
                            {
                                model.NodeUser.Add(new CloudWindProjectNodeUser()
                                {
                                    NodeUserName = b.NodeUserName ?? "",
                                    NodeUserCode = b.NodeUserCode ?? "",
                                    NodeUserDepart = b.NodeUserDepart ?? "",
                                    NodeUserPhone = b.NodeUserPhone ?? "",
                                    NodeUserJobName = b.NodeUserJobName ?? ""
                                });
                            });

                            model.FlowHandle = "view";
                            if (model.FlowStatus != (int)CloudWindTaskFlowStatus.作废 && !model.IsFinished)
                            {
                                if (thisFlows.Exists(b => b.NodeUserCode == userCode && b.FlowHandle == (int)CloudWindTaskFlowHandleStatus.待审批))
                                {
                                    model.FlowHandle = "approval";

                                    var lastFlows = allFlows.Where(b => b.FlowOrder == (latestFlow.FlowOrder - 1)).ToList();
                                    var firstFlow = allFlows.FirstOrDefault();

                                    if (lastFlows != null && lastFlows.Exists(c => c.FlowHandle == (int)CloudWindTaskFlowHandleStatus.退回 && c.NodeID == firstFlow.NodeID))
                                    {
                                        model.FlowHandle = "renew";
                                    }
                                }
                            }

                            list.Add(model);
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                list = new List<CloudWindTask>();
            }
            return list;
        }

        /// <summary>
        /// 选择项目软件
        /// </summary>
        public async Task<List<CloudWindTaskSoftware>> ChooseProject(CloudWindRequest request)
        {
            var list = new List<CloudWindTaskSoftware>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var softwareRepo = uow.GetRepository<Manage_Software>();
                    var projectRepo = uow.GetRepository<Wind_Project>();

                    var softwares = softwareRepo.Find(a => !a.IsDelete).OrderBy(a => a.ID).ToList();

                    // 1.WebGIS
                    var webgis = softwares.FirstOrDefault(a => a.ID == 1);
                    var wgModel = new CloudWindTaskSoftware();
                    wgModel.ID = webgis.ID;
                    wgModel.IsUsed = false;
                    wgModel.TaskTimeString = "";

                    var webgisProject = projectRepo.Find(a =>
                        !a.IsDelete && a.ID == request.ProjectID &&
                        a.FlowStatus == (int)CloudWindProjectFlowStatus.激活 &&
                        a.ProjectStartTime <= DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified) && a.ProjectEndTime >= DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified))
                        .OrderByDescending(a => a.CreateTime).FirstOrDefault();

                    if (webgisProject != null)
                    {
                        wgModel.IsUsed = true;
                        wgModel.TaskTimeString = "(有效期：" + webgisProject.ProjectStartTime?.ToString("yyyy-MM-dd") + " 至 " + webgisProject.ProjectEndTime?.ToString("yyyy-MM-dd") + ")";
                    }
                    else
                    {
                        wgModel.TaskTimeString = "项目使用期限已过，请联系技术中心海工所。";
                    }
                    list.Add(wgModel);

                    // 2.自升式平台桩腿插拔计算
                    var cbzSoft = softwares.FirstOrDefault(a => a.ID == 2);
                    var cbzModel = new CloudWindTaskSoftware();
                    cbzModel.ID = cbzSoft.ID;
                    cbzModel.IsUsed = false;
                    cbzModel.TaskTimeString = "";
                    list.Add(cbzModel);

                    // 3.起重船基础施工可作业性预报
                    var kzySoft = softwares.FirstOrDefault(a => a.ID == 3);
                    var kzyModel = new CloudWindTaskSoftware();
                    kzyModel.ID = kzySoft.ID;
                    kzyModel.IsUsed = false;
                    kzyModel.TaskTimeString = "";
                    list.Add(kzyModel);

                    // 4.交互式风机整体安装模拟软件
                    var mnazSoft = softwares.FirstOrDefault(a => a.ID == 4);
                    var mnazModel = new CloudWindTaskSoftware();
                    mnazModel.ID = mnazSoft.ID;
                    mnazModel.IsUsed = false;
                    mnazModel.TaskTimeString = "";
                    list.Add(mnazModel);

                    // 6.桩基沉桩可打性分析
                    var zjSoft = softwares.FirstOrDefault(a => a.ID == 6);
                    var zjModel = new CloudWindTaskSoftware();
                    zjModel.ID = zjSoft.ID;
                    zjModel.IsUsed = false;
                    zjModel.TaskTimeString = "";
                    list.Add(zjModel);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                list = new List<CloudWindTaskSoftware>();
            }
            return list;
        }

        /// <summary>
        /// 查询流程历史
        /// </summary>
        public async Task<List<CloudWindFlowHistory>> QueryFlowHistory(CloudWindRequest request)
        {
            var list = new List<CloudWindFlowHistory>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var nodeRepo = uow.GetRepository<Flow_Node>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();
                    var softwareRepo = uow.GetRepository<Manage_Software>();

                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (task == null)
                    {
                        return new List<CloudWindFlowHistory>();
                    }

                    var nodes = nodeRepo.Find(a => !a.IsDelete).ToList();
                    var taskFlow = flowRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList();
                    var software = softwareRepo.Find(a => !a.IsDelete && a.ID == task.SoftwareID).FirstOrDefault();

                    // 1.发起
                    var firstModel = new CloudWindFlowHistory();
                    firstModel.FlowType = 0;
                    firstModel.FlowHandleName = "申请";
                    firstModel.ApprovalType = 1;
                    firstModel.NodeName = Enum.GetName(typeof(CloudWindFlowType), software.FlowType);
                    firstModel.FlowTime = task.CreateTime?.ToString("yyyy-MM-dd HH:mm") ?? "";
                    firstModel.Flow.Add(new CloudWindFlowHistoryInfo()
                    {
                        NodeUserName = task.Applyer ?? "",
                        NodeUserJobName = task.ApplyerJobName ?? "",
                        NodeUserDepart = task.ApplyerDepart ?? "",
                        Comment = ""
                    });
                    list.Add(firstModel);

                    // 2.流程
                    taskFlow.OrderBy(a => a.FlowOrder).GroupBy(a => a.FlowOrder).ToList()
                        .ForEach(a =>
                        {
                            int flowOrder = a.First().FlowOrder.Value;
                            var thisFlows = taskFlow.Where(b => b.FlowOrder == flowOrder).ToList();

                            var model = new CloudWindFlowHistory();
                            model.FlowType = 1;
                            model.NodeName = nodes.FirstOrDefault(b => b.ID == a.First().NodeID)?.NodeName;
                            model.ApprovalType = thisFlows.Count == 1 ? 1 : 2;
                            model.FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批;
                            model.FlowHandleName = CloudWindTaskFlowHandleStatus.待审批.ToString();

                            if (thisFlows.Exists(b => b.FlowHandle == (int)CloudWindTaskFlowHandleStatus.退回))
                            {
                                model.FlowHandle = (int)CloudWindTaskFlowHandleStatus.退回;
                                model.FlowHandleName = CloudWindTaskFlowHandleStatus.退回.ToString();
                            }
                            else if (thisFlows.Count(b => b.FlowHandle == (int)CloudWindTaskFlowHandleStatus.审批通过) == thisFlows.Count)
                            {
                                model.FlowHandle = (int)CloudWindTaskFlowHandleStatus.审批通过;
                                model.FlowHandleName = CloudWindTaskFlowHandleStatus.审批通过.ToString();
                            }

                            thisFlows.ForEach(b =>
                            {
                                model.Flow.Add(new CloudWindFlowHistoryInfo()
                                {
                                    NodeUserName = b.NodeUserName ?? "",
                                    NodeUserDepart = b.NodeUserDepart ?? "",
                                    NodeUserJobName = b.NodeUserJobName ?? "",
                                    Comment = b.Comment ?? "",
                                    FlowHandle = b.FlowHandle ?? 0,
                                    FlowHandleName = Enum.GetName(typeof(CloudWindTaskFlowHandleStatus), b.FlowHandle),
                                    FlowTime = b.ApprovalTime?.ToString("yyyy-MM-dd HH:mm") ?? ""
                                });
                            });

                            list.Add(model);
                        });

                    // 3.结束(加上抄送)
                    if (task.FlowStatus == (int)CloudWindProjectFlowStatus.激活)
                    {
                        var endModel = new CloudWindFlowHistory();
                        endModel.FlowType = 2;
                        endModel.FlowHandleName = "结束";
                        endModel.FlowTime = "";
                        list.Add(endModel);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                list = new List<CloudWindFlowHistory>();
            }
            return list;
        }

        /// <summary>
        /// 加载项目文件
        /// </summary>
        public async Task<CloudProjectFileType> LoadFile(CloudWindRequest request)
        {
            var file = new CloudProjectFileType();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var geologyRepo = uow.GetRepository<Library_Geology>();

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (project == null)
                    {
                        return new CloudProjectFileType();
                    }

                    var geologies = geologyRepo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList();
                    geologies.ForEach(a =>
                    {
                        string filePath = "/File/Project/OW1" + project.ProjectCodeIndex?.ToString().PadLeft(3, '0') + "/Import/" + a.FileName;
                        string fileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? "";

                        if (a.Type == (int)CloudWindTaskFileType.风电项目_机孔位地勘原始资料)
                        {
                            file.Geology_yszl.Add(new CloudProjectFile()
                            {
                                FileName = a.FileName,
                                FilePath = filePath,
                                FileTime = fileTime
                            });
                        }
                        else if (a.Type == (int)CloudWindTaskFileType.风电项目_机孔位地勘数据表)
                        {
                            file.Geology_sjb.Add(new CloudProjectFile()
                            {
                                FileName = a.FileName,
                                FilePath = filePath,
                                FileTime = fileTime
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                file = new CloudProjectFileType();
            }
            return file;
        }

        /// <summary>
        /// 获取可作业性船舶列表
        /// </summary>
        public List<Library_Ship_KZY> GetKZYShipList()
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Library_Ship_KZY>();
                return repo.Find(a => !(a.IsDelete == true)).ToList();
            }
        }

        /// <summary>
        /// 获取航线位置列表
        /// </summary>
        public List<Manage_PlanRoute> GetPlanRouteList()
        {
            using (var uow = _towingUowFactory.Create())
            {
                var repo = uow.GetRepository<Manage_PlanRoute>();
                return repo.Find(a => !a.IsDelete).ToList();
            }
        }

        /// <summary>
        /// 获取任务流程信息
        /// </summary>
        public FlowInfo GetTaskFlow(int taskId, string userCode)
        {
            var flow = new FlowInfo();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var softwareRepo = uow.GetRepository<Manage_Software>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();
                    var nodeRepo = uow.GetRepository<Flow_Node>();
                    var nodeUserRepo = uow.GetRepository<Flow_NodeManageUser>();

                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == taskId).FirstOrDefault();
                    var software = softwareRepo.Find(a => !a.IsDelete && a.ID == task.SoftwareID).FirstOrDefault();
                    var taskFlows = flowRepo.Find(a => !(a.IsDelete == true) && a.TaskID == taskId).OrderBy(a => a.id).ToList();

                    if (task == null || taskFlows == null || taskFlows.Count == 0)
                    {
                        return new FlowInfo();
                    }

                    var allNodes = nodeRepo.Find(a => !a.IsDelete && a.SoftwareID == software.ID).OrderBy(a => a.NodeNo).ToList();
                    var firstNode = allNodes.FirstOrDefault();

                    flow.FlowStatus = task.FlowStatus.Value;

                    // 1.当前流程信息
                    var latestFlow = taskFlows.OrderByDescending(a => a.CreateTime).FirstOrDefault();
                    var node = allNodes.FirstOrDefault(a => a.ID == latestFlow.NodeID);
                    flow.NodeID = node.ID;
                    flow.NodeName = node.NodeName;
                    flow.DoEdit = node.DoEdit == true;
                    flow.NodeApprovalType = node.NodeApprovalType == true;

                    taskFlows.Where(a => a.NodeID == latestFlow.NodeID && a.FlowOrder == latestFlow.FlowOrder).ToList().ForEach(a =>
                    {
                        flow.Node.Add(new NodeInfo()
                        {
                            Comment = a.Comment ?? "",
                            UserName = a.NodeUserName ?? "",
                            UserCode = a.NodeUserCode ?? "",
                            UserDepart = a.NodeUserDepart ?? "",
                            UserPhone = a.NodeUserPhone ?? "",
                            UserJobName = a.NodeUserJobName ?? "",
                            FlowHandle = a.FlowHandle ?? 0
                        });
                    });

                    // 2.上个流程信息
                    Flow_Node lastNode;
                    List<Flow_TaskApply> lastFlow;

                    if (task.FlowStatus == (int)CloudWindTaskFlowStatus.完成)
                    {
                        lastNode = allNodes.FirstOrDefault(a => a.ID == latestFlow.NodeID);
                        lastFlow = taskFlows.Where(a => a.FlowOrder == latestFlow.FlowOrder).ToList();
                    }
                    else
                    {
                        lastNode = allNodes.FirstOrDefault(a => a.NodeNo == (node.NodeNo - 1));
                        lastFlow = taskFlows.Where(a => a.FlowOrder == (latestFlow.FlowOrder - 1)).ToList();
                    }

                    flow.LastNodeID = lastNode == null ? 0 : lastNode.ID;
                    flow.LastNodeName = lastNode == null ? "" : lastNode.NodeName;

                    if (lastFlow == null || lastFlow.Count == 0)
                    {
                        flow.LastNode.Add(new NodeInfo()
                        {
                            Comment = "",
                            UserName = task.Applyer ?? "",
                            UserCode = task.ApplyerCode ?? "",
                            UserDepart = task.ApplyerDepart ?? "",
                            UserPhone = task.ApplyerPhone ?? "",
                            UserJobName = task.ApplyerJobName ?? "",
                            FlowHandle = (int)CloudWindTaskFlowHandleStatus.审批通过
                        });
                    }
                    else
                    {
                        lastFlow.ForEach(a =>
                        {
                            flow.LastNode.Add(new NodeInfo()
                            {
                                Comment = a.Comment ?? "",
                                UserName = a.NodeUserName ?? "",
                                UserCode = a.NodeUserCode ?? "",
                                UserDepart = a.NodeUserDepart ?? "",
                                UserPhone = a.NodeUserPhone ?? "",
                                UserJobName = a.NodeUserJobName ?? "",
                                FlowHandle = a.FlowHandle ?? 0
                            });
                        });
                    }

                    // 3.下一个节点信息
                    var nextNode = allNodes.FirstOrDefault(a => a.NodeNo == (node.NodeNo + 1));
                    if (nextNode == null)
                    {
                        flow.NextNodeID = 0;
                        flow.NextNodeName = "流程结束";
                    }
                    else
                    {
                        flow.NextNodeID = nextNode.ID;
                        flow.NextNodeName = nextNode.NodeName;

                        var nodeManager = nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == nextNode.ID).ToList();
                        if (nodeManager != null && nodeManager.Count > 0)
                        {
                            nodeManager.ForEach(a =>
                            {
                                flow.NextNode.Add(new NodeInfo()
                                {
                                    Comment = "",
                                    UserName = a.ManageName ?? "",
                                    UserCode = a.ManageUserCode ?? "",
                                    UserDepart = a.ManageDepart ?? "",
                                    UserPhone = a.ManagePhone ?? "",
                                    UserJobName = a.ManageJobName ?? ""
                                });
                            });
                        }
                    }

                    // 4.判断是否退回的流程
                    if (lastFlow != null && lastFlow.Count > 0)
                    {
                        if (lastFlow.Exists(a => a.FlowHandle == (int)CloudWindTaskFlowHandleStatus.退回) && flow.NodeID == firstNode.ID)
                        {
                            flow.IsBackFlow = true;
                        }
                    }

                    // 5.判断权限
                    flow.FlowHandle = "view";
                    if (task.FlowStatus == (int)CloudWindTaskFlowStatus.完成 || task.FlowStatus == (int)CloudWindTaskFlowStatus.作废)
                    {
                        flow.FlowHandle = "view";
                    }
                    else
                    {
                        var myFlow = flowRepo.Find(a =>
                            !(a.IsDelete == true) && a.TaskID == taskId &&
                            a.NodeID == node.ID && a.FlowOrder == latestFlow.FlowOrder &&
                            a.NodeUserCode == userCode &&
                            a.FlowHandle == (int)CloudWindTaskFlowHandleStatus.待审批
                        ).FirstOrDefault();

                        if (myFlow != null)
                        {
                            flow.FlowHandle = "approval";
                            if (flow.IsBackFlow && flow.NodeID == firstNode.ID)
                            {
                                flow.FlowHandle = "renew";
                            }
                            if (flow.DoEdit)
                            {
                                flow.DoEditConfirm = true;
                            }
                        }
                    }

                    flow.Success = true;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                flow = new FlowInfo();
                flow.Success = false;
                flow.Message = "获取流程信息出错,请联系技术中心管理员。";
            }
            return flow;
        }




        #region 插拔桩

        /// <summary>
        /// 任务提交（插拔桩）
        /// </summary>
        public async Task<string> TaskSubmit_CBZ(CloudWindRequest request)
        {
            string url = "";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var nodeUserRepo = uow.GetRepository<Flow_NodeManageUser>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var dkRepo = uow.GetRepository<Library_Geology_DK>();
                    var dkDataRepo = uow.GetRepository<Library_Geology_Data>();
                    var dkFileRepo = uow.GetRepository<Flow_Task_DKFile>();
                    var shipViewRepo = uow.GetRepository<View_Library_Ship>();
                    var shipFileRepo = uow.GetRepository<Flow_Task_ShipFile>();
                    var shipRepo = uow.GetRepository<Library_Ship>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();

                    // 检查节点人员配置
                    var nodeManagers = nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == (int)CloudWindSoftwareCBZFlowNode.技术审核).ToList();
                    if (nodeManagers == null || nodeManagers.Count == 0)
                    {
                        if (string.IsNullOrEmpty(request.TaskNodeName) || string.IsNullOrEmpty(request.TaskNodeUserCode))
                        {
                            throw new Exception("没有设置节点人员");
                        }
                    }

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (project == null)
                    {
                        throw new Exception("项目信息错误");
                    }

                    DateTime now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

                    // 1.生成工作任务
                    var newTask = new Wind_Task();
                    newTask.ProjectID = request.ProjectID;
                    newTask.TaskName = request.TaskName;
                    newTask.TaskCode = "TK" + CreateTaskCode().ToString().PadLeft(5, '0');
                    newTask.SoftwareID = request.SoftwareID;
                    newTask.FlowStatus = (int)CloudWindTaskFlowStatus.审批中;
                    newTask.Applyer = CurrentUser.RealName;
                    newTask.ApplyerCode = CurrentUser.UserCode;
                    newTask.ApplyerDepart = CurrentUser.DepartName;
                    newTask.ApplyerPhone = CurrentUser.Mobile;
                    newTask.ApplyerJobName = CurrentUser.JobName;
                    newTask.CreateTime = now;
                    newTask.IsDelete = false;
                    taskRepo.Add(newTask);
                    uow.Save();

                    var newTaskID = newTask.ID;

                    // 3.创建文件夹
                    string fileImportFolder = AppContext.BaseDirectory + @"File\Project\" + project.ProjectCode + @"\Task\" + newTask.TaskCode + @"\Import";
                    FileUtils.CreateFileFolder(fileImportFolder);
                    string fileExportFolder = AppContext.BaseDirectory + @"File\Project\" + project.ProjectCode + @"\Task\" + newTask.TaskCode + @"\Export";
                    FileUtils.CreateFileFolder(fileExportFolder);

                    // 4.生成报告接收人
                    var newDeliver = new Wind_TaskFileDeliver();
                    newDeliver.TaskID = newTaskID;
                    newDeliver.DeliverName = CurrentUser.RealName;
                    newDeliver.DeliverCode = CurrentUser.UserCode;
                    newDeliver.DeliverDepart = CurrentUser.DepartName;
                    newDeliver.DeliverPhone = CurrentUser.Mobile;
                    newDeliver.DeliverJobName = CurrentUser.JobName;
                    newDeliver.CreateTime = now;
                    newDeliver.IsDelete = false;
                    deliverRepo.Add(newDeliver);

                    #region 生成文件
                    // 5.地勘数据表
                    var dkList = dkRepo.Find(a => !a.IsDelete).ToList();
                    var dkIds = request.DKIDstring.Split(',').Select(id => Convert.ToInt32(id)).ToList();
                    var dkDataList = dkDataRepo.Find(a => !a.IsDelete && dkIds.Contains(a.DKID.Value)).ToList();

                    DataSet dsDK = new DataSet();
                    foreach (int i in dkIds)
                    {
                        var dk = dkList.FirstOrDefault(a => a.ID == i);
                        if (dk != null)
                        {
                            DataTable dt = new DataTable();
                            dt.TableName = dk.DKName;
                            dt.Columns.Add("序号");
                            dt.Columns.Add("土层编号");
                            dt.Columns.Add("土层名称");
                            dt.Columns.Add("层底标高(m)");
                            dt.Columns.Add("土层类型");
                            dt.Columns.Add("不排水抗剪强度cu(kPa)");
                            dt.Columns.Add("砂土摩擦角°");
                            dt.Columns.Add("有效重度");
                            dt.Columns.Add("标贯击数");
                            dkDataList.Where(a => a.DKID == dk.ID).ToList().ForEach(a =>
                            {
                                DataRow dr = dt.NewRow();
                                dr[0] = a.xh;
                                dr[1] = a.dcbh;
                                dr[2] = a.tcbh;
                                dr[3] = a.cdbg;
                                dr[4] = a.tclx;
                                dr[5] = a.bpskjqd;
                                dr[6] = a.stmcj;
                                dr[7] = a.yxzd;
                                dr[8] = a.bgjs;
                                dt.Rows.Add(dr);
                            });
                            dsDK.Tables.Add(dt);
                        }
                    }

                    var dkFileTempName = "地勘计算文件(" + DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified).ToString("yyyyMMddHHmmss") + ").xlsx";
                    var dkFilePath = fileImportFolder + @"\" + dkFileTempName;

                    // TODO: ExcelUtils.DataSetToExcel 已替换
                    var dkSuccess = ExcelUtils.DataSetToExcel(dsDK, dkFilePath, true);
                    if (dkSuccess)
                    {
                        dkFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList()
                            .ForEach(a => { a.IsDelete = true; });

                        dkFileRepo.Add(new Flow_Task_DKFile()
                        {
                            FileName = dkFileTempName,
                            FilePath = dkFilePath,
                            CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                            TaskID = newTaskID,
                            IsDelete = false
                        });
                    }
                    else
                    {
                        throw new Exception("生成地勘资料时发生错误。");
                    }

                    // 6.平台船数据表
                    var shipIds = request.ShipIDstring.Split(',').Select(id => Convert.ToInt32(id)).ToList();
                    var ships = shipViewRepo.Find(a => shipIds.Contains(a.ID)).ToList();
                    var shipFiles = new List<Flow_Task_ShipFile>();

                    foreach (var a in ships)
                    {
                        string shipName = a.ShipName;
                        DataTable dt = new DataTable();
                        dt.TableName = "VesselData";
                        dt.Columns.Add("船舶参数");
                        dt.Columns.Add("数值");
                        dt.Columns.Add("单位");

                        dt.Rows.Add("桩腿截面积", a.ztjmj, a.ztjmj_unit);
                        dt.Rows.Add("桩腿周长", a.ztzc, a.ztzc_unit);
                        dt.Rows.Add("桩靴长度 L", string.IsNullOrEmpty(a.zxcdL) ? "*" : a.zxcdL, a.zxcdL_unit);
                        dt.Rows.Add("桩靴宽度 B", a.zxkdB, a.zxkdB_unit);
                        dt.Rows.Add("桩靴高度 H", a.zxgdH, a.zxgdH_unit);
                        dt.Rows.Add("桩靴面积 A", a.zxmjA, a.zxmjA_unit);
                        dt.Rows.Add("桩靴最大截面周长", a.zxzdjmzc, a.zxzdjmzc_unit);
                        dt.Rows.Add("桩靴体积 V", a.zxtjV, a.zxtjV_unit);
                        dt.Rows.Add("桩腿、桩靴自重 W", a.ztzxzzW, a.ztzxzzW_unit);
                        dt.Rows.Add("桩腿预压力", a.ztyyl, a.ztyyl_unit);
                        dt.Rows.Add("计算预压荷载", a.jsyyhz, a.jsyyhz_unit);
                        dt.Rows.Add("拔桩力", a.bzl, a.bzl_unit);
                        dt.Rows.Add("对地比压", a.ddby, a.ddby_unit);
                        dt.Rows.Add("有效桩腿长度（船底到靴底）", a.yxztcd_cddxd, a.yxztcd_cddxd_unit);
                        dt.Rows.Add("气隙（船底到水面）", a.qx_cddsm, a.qx_cddsm_unit);
                        dt.Rows.Add("桩腿有效长度", a.ztyxcd, a.ztyxcd_unit);

                        DataSet dsShip = new DataSet();
                        dsShip.Tables.Add(dt);

                        var shipFileTempName = shipName + "(" + DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified).ToString("yyyyMMddHHmmss") + ").xlsx";
                        var shipFilePath = fileImportFolder + @"\" + shipFileTempName;

                        // TODO: ExcelUtils.DataSetToExcel 已替换
                        var shipSuccess = ExcelUtils.DataSetToExcel(dsShip, shipFilePath, false);
                        if (shipSuccess)
                        {
                            shipFiles.Add(new Flow_Task_ShipFile()
                            {
                                TaskID = newTaskID,
                                FileName = shipName + ".xlsx",
                                FilePath = shipFilePath,
                                CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                                IsDelete = false
                            });
                        }
                    }

                    shipFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList()
                        .ForEach(a => { a.IsDelete = true; });
                    shipFileRepo.AddList(shipFiles);

                    // 如果存在新增船舶，需要修改TaskID
                    shipRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList()
                        .ForEach(a => { a.TaskID = newTaskID; });

                    #endregion

                    uow.Save();

                    // 先进行插拔桩计算
                    bool cbzSuccess = false;
                    {
                        string token = CloudCenterService.CreateToken(UserCode, newTaskID);
                        var cbzUrl = AppSettingUtils.GetSetting("SoftwareApi:CBZUrl");
                        url = cbzUrl + @"?Token=" + token;
                        LoggerUtils.Error(url, typeof(TaskService));

                        // TODO: NetUtils HttpGet 已替换为 HttpUtils.GetSync
                        string retStr = HttpUtils.GetSync(url);
                        LoggerUtils.Error(retStr, typeof(TaskService));

                        string message = GetMessageCBZ(retStr);

                        if (string.IsNullOrEmpty(message))
                        {
                            LoggerUtils.Error("插拔桩计算成功", typeof(TaskService));
                            cbzSuccess = true;
                        }
                        else
                        {
                            LoggerUtils.Error(message, typeof(TaskService));
                            cbzSuccess = false;
                            throw new Exception("插拔装计算发生错误：" + message);
                        }
                    }

                    if (cbzSuccess)
                    {
                        List<string> informers = new List<string>();

                        // 7.生成流程
                        if (nodeManagers == null || nodeManagers.Count == 0)
                        {
                            var newFlow = new Flow_TaskApply();
                            newFlow.TaskID = newTaskID;
                            newFlow.NodeID = (int)CloudWindSoftwareCBZFlowNode.技术审核;
                            newFlow.FlowOrder = 1;
                            newFlow.ApprovalTime = now;
                            newFlow.NodeUserCode = request.TaskNodeUserCode;
                            newFlow.NodeUserName = request.TaskNodeName;
                            newFlow.NodeUserDepart = request.TaskNodeDepart;
                            newFlow.NodeUserPhone = request.TaskNodePhone;
                            newFlow.NodeUserJobName = request.TaskNodeJobName;
                            newFlow.Comment = request.TaskComment ?? "";
                            newFlow.FlowHandle = (int)CloudWindTaskFlowStatus.审批中;
                            newFlow.CreateTime = now;
                            newFlow.IsDelete = false;
                            flowRepo.Add(newFlow);
                            informers.Add(request.TaskNodeUserCode);
                        }
                        else
                        {
                            nodeManagers.ForEach(a =>
                            {
                                var newFlow = new Flow_TaskApply();
                                newFlow.TaskID = newTaskID;
                                newFlow.NodeID = (int)CloudWindSoftwareCBZFlowNode.技术审核;
                                newFlow.FlowOrder = 1;
                                newFlow.ApprovalTime = now;
                                newFlow.NodeUserCode = a.ManageUserCode;
                                newFlow.NodeUserName = a.ManageName;
                                newFlow.NodeUserDepart = a.ManageDepart;
                                newFlow.NodeUserPhone = a.ManagePhone;
                                newFlow.NodeUserJobName = a.ManageJobName;
                                newFlow.Comment = request.TaskComment ?? "";
                                newFlow.FlowHandle = (int)CloudWindTaskFlowStatus.审批中;
                                newFlow.CreateTime = now;
                                newFlow.IsDelete = false;
                                flowRepo.Add(newFlow);
                                informers.Add(a.ManageUserCode);
                            });
                        }

                        uow.Save();

                        // 交建通通知
                        {
                            string cloudPlatUrl = AppSettingUtils.GetSetting("SoftwareApi:CloudPlatUrl");
                            List<SystemJJTInform> informUsers = new List<SystemJJTInform>();
                            foreach (var item in informers)
                            {
                                var accessToken = _cloudCenterService.CreateJJTMessageRedirectToken("Task", item, newTaskID);
                                var redirectUrl = cloudPlatUrl + @"Home/JJTMessageRedirect?CloudToken=" + accessToken;
                                informUsers.Add(new SystemJJTInform()
                                {
                                    UserCode = item,
                                    Url = redirectUrl,
                                    Title = project.ProjectName + "(" + newTask.TaskCode + ")-插拔桩计算流程审批",
                                    Content = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified).ToString("yyyy-MM-dd HH:mm") + "\r\n" + "点击查看详情"
                                });
                            }

                            //Task.Run(async () =>
                            //{
                            //    await _cloudWindInfoService.SendJJTMessage(informUsers);
                            //});

                            _cloudWindInfoService.SendJJTMessage(informUsers);

                        }
                    }
                    else
                    {
                        var deleteTask = taskRepo.Find(a => !a.IsDelete && a.ID == newTaskID).FirstOrDefault();
                        if (deleteTask != null)
                        {
                            deleteTask.IsDelete = true;
                            uow.Save();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                url = "";
            }
            return url;
        }

        /// <summary>
        /// 任务查询（插拔桩）
        /// </summary>
        public CloudWindTaskFlow TaskQuery_CBZ(CloudWindRequest request)
        {
            var data = new CloudWindTaskFlow();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var viewFlowRepo = uow.GetRepository<View_Flow_TaskApply>();
                    var projectRoleRepo = uow.GetRepository<Wind_ProjectRole>();
                    var copyerRepo = uow.GetRepository<Manage_Copyer>();
                    var reportRepo = uow.GetRepository<Wind_TaskReport>();

                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (task == null)
                    {
                        throw new Exception("工作任务数据错误");
                    }

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == task.ProjectID).FirstOrDefault();
                    if (project == null)
                    {
                        throw new Exception("工作任务数据错误");
                    }

                    var deliver = deliverRepo.Find(a => !a.IsDelete && a.TaskID == request.TaskID).FirstOrDefault();
                    if (deliver == null)
                    {
                        throw new Exception("工作任务数据错误");
                    }

                    // 1.基础信息
                    data.TaskID = task.ID;
                    data.ProjectID = task.ProjectID;
                    data.TaskName = task.TaskName;
                    data.TaskStatus = task.FlowStatus ?? 0;
                    data.DeliverTime = task.DeliverTime == null ? "" : task.DeliverTime.Value.ToString("yyyy-MM-dd");

                    // 流程信息
                    var flowInfo = GetTaskFlow(task.ID, UserCode);
                    if (!flowInfo.Success)
                    {
                        throw new Exception(flowInfo.Message);
                    }
                    data.FlowInfo = flowInfo;

                    // 文件报告权限
                    List<string> reportViewer = new List<string>();
                    List<string> reportModifyViewer = new List<string>();

                    viewFlowRepo.Find(a => a.TaskID == request.TaskID && a.NodeID == (int)CloudWindSoftwareCBZFlowNode.技术审核)
                        .GroupBy(a => a.NodeUserCode).ToList()
                        .ForEach(g =>
                        {
                            reportViewer.Add(g.FirstOrDefault().NodeUserCode);
                            reportModifyViewer.Add(g.FirstOrDefault().NodeUserCode);
                        });

                    viewFlowRepo.Find(a => a.TaskID == request.TaskID && (a.NodeID == (int)CloudWindSoftwareCBZFlowNode.业务审核 || a.NodeID == (int)CloudWindSoftwareCBZFlowNode.审核通过))
                        .GroupBy(a => a.NodeUserCode).ToList()
                        .ForEach(g =>
                        {
                            reportModifyViewer.Add(g.FirstOrDefault().NodeUserCode);
                        });

                    if (task.FlowStatus == (int)CloudWindTaskFlowStatus.完成)
                    {
                        reportModifyViewer.Add(deliver.DeliverCode);
                        projectRoleRepo.Find(a => !a.IsDelete && a.ProjectID == project.ID && a.RoleID == (int)CloudWindProjectRole.项目部领导班子).ToList()
                            .ForEach(a => { reportModifyViewer.Add(a.UserCode); });
                        copyerRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.自升式平台桩腿插拔计算).ToList()
                            .ForEach(a => { reportModifyViewer.Add(a.UserCode); });
                    }

                    var reports = reportRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList();

                    if (task.FlowStatus == (int)CloudWindTaskFlowStatus.退回)
                    {
                        if (reports.Exists(a => a.ReportName == "Error.txt"))
                        {
                            reportViewer.Add(task.ApplyerCode);
                        }
                    }

                    if (reportViewer.Contains(CurrentUser.UserCode))
                    {
                        if (reports != null && reports.Count > 0)
                        {
                            reports.Where(a => a.Type == 1).OrderByDescending(a => a.CreateTime).ToList()
                                .ForEach(a =>
                                {
                                    string fileName = a.ReportPath.Substring(a.ReportPath.LastIndexOf('\\') + 1);
                                    data.Report.ReportFile.Add(new CloudProjectFile()
                                    {
                                        ID = a.id,
                                        FileName = a.ReportName,
                                        FilePath = "/File/Project/" + project.ProjectCode + "/Task/" + task.TaskCode + @"/Export/" + fileName,
                                        FileTime = Convert.ToDateTime(a.CreateTime).ToString("yyyy-MM-dd")
                                    });
                                });
                        }
                    }

                    if (reportModifyViewer.Contains(CurrentUser.UserCode))
                    {
                        if (reports != null && reports.Count > 0)
                        {
                            reports.Where(a => a.Type == 2).OrderByDescending(a => a.CreateTime).ToList()
                                .ForEach(a =>
                                {
                                    string fileName = a.ReportPath.Substring(a.ReportPath.LastIndexOf('\\') + 1);
                                    data.Report.ReportModifyFile.Add(new CloudProjectFile()
                                    {
                                        ID = a.id,
                                        FileName = a.ReportName,
                                        FilePath = "/File/Project/" + project.ProjectCode + "/Task/" + task.TaskCode + @"/Export/" + fileName,
                                        FileTime = Convert.ToDateTime(a.CreateTime).ToString("yyyy-MM-dd")
                                    });
                                });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                data = new CloudWindTaskFlow();
            }
            return data;
        }


        /// <summary>
        /// 解析插拔桩计算返回消息
        /// </summary>
        private string GetMessageCBZ(string content)
        {
            string ret = "";
            try
            {
                string keyWord = "MainContent_LabelMessage";
                int startPosition = content.IndexOf(keyWord);
                int endPosition = content.Substring(startPosition + keyWord.Length + 2).IndexOf("</span>");
                ret = content.Substring(startPosition + keyWord.Length + 2, endPosition);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                return "出错";
            }
            return ret;
        }

        /// <summary>
        /// 审批提交（插拔桩）
        /// </summary>
        public async Task<string> TaskApprovalSubmit_CBZ(CloudWindRequest request)
        {
            string url = "";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var nodeRepo = uow.GetRepository<Flow_Node>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();
                    var nodeUserRepo = uow.GetRepository<Flow_NodeManageUser>();
                    var dkRepo = uow.GetRepository<Library_Geology_DK>();
                    var dkDataRepo = uow.GetRepository<Library_Geology_Data>();
                    var dkFileRepo = uow.GetRepository<Flow_Task_DKFile>();
                    var shipViewRepo = uow.GetRepository<View_Library_Ship>();
                    var shipFileRepo = uow.GetRepository<Flow_Task_ShipFile>();
                    var shipRepo = uow.GetRepository<Library_Ship>();
                    var copyerRepo = uow.GetRepository<Manage_Copyer>();

                    DateTime now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

                    var thisTask = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (thisTask == null) throw new Exception("任务数据错误");

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == thisTask.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("任务数据错误");

                    // 获取流程信息
                    var flowInfo = GetTaskFlow(thisTask.ID, UserCode);
                    if (!flowInfo.Success) throw new Exception(flowInfo.Message);

                    var nodes = nodeRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.自升式平台桩腿插拔计算)
                        .OrderBy(a => a.NodeNo).ToList();
                    var node = nodes.FirstOrDefault(a => a.ID == flowInfo.NodeID);
                    var firstNode = nodes.FirstOrDefault();

                    var taskFlows = flowRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList();
                    var latestFlow = taskFlows.OrderByDescending(a => a.CreateTime).FirstOrDefault();
                    if (latestFlow == null) throw new Exception("流程数据错误");

                    var flow = taskFlows.FirstOrDefault(a => a.NodeID == latestFlow.NodeID && a.FlowOrder == latestFlow.FlowOrder && a.NodeUserCode == UserCode);
                    if (flow == null) throw new Exception("流程数据错误");

                    bool doAddInformer = true;
                    List<string> informers = new List<string>();
                    bool doAddCopyer = false;

                    // 流程处理
                    if (request.ApprovalType == (int)CloudWindFlowHandle.审批退回)
                    {
                        flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.退回;
                        flow.ApprovalTime = now;
                        flow.Comment = request.TaskComment;
                        thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.退回;

                        flowRepo.Add(new Flow_TaskApply()
                        {
                            TaskID = request.TaskID,
                            NodeID = (int)CloudWindSoftwareCBZFlowNode.任务申请,
                            FlowOrder = flow.FlowOrder + 1,
                            NodeUserName = thisTask.Applyer ?? "",
                            NodeUserCode = thisTask.ApplyerCode ?? "",
                            NodeUserDepart = thisTask.ApplyerDepart ?? "",
                            NodeUserPhone = thisTask.ApplyerPhone ?? "",
                            NodeUserJobName = thisTask.ApplyerJobName ?? "",
                            FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批,
                            Comment = "",
                            CreateTime = now,
                            IsDelete = false
                        });
                        informers.Add(thisTask.ApplyerCode);
                    }
                    else if (request.ApprovalType == (int)CloudWindFlowHandle.审批通过)
                    {
                        var nextNode = nodeRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.自升式平台桩腿插拔计算 && a.NodeNo == (node.NodeNo + 1)).FirstOrDefault();
                        var nodeManageUsers = new List<Flow_NodeManageUser>();
                        if (nextNode != null)
                        {
                            nodeManageUsers = nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == nextNode.ID).ToList();
                        }

                        var thisFlows = taskFlows.Where(a => a.NodeID == flow.NodeID && a.FlowOrder == flow.FlowOrder && a.id != flow.id && a.FlowHandle == (int)CloudWindTaskFlowHandleStatus.待审批).ToList();
                        var lastFlow = taskFlows.Where(a => a.FlowOrder == (flow.FlowOrder - 1)).ToList();

                        if (lastFlow != null && lastFlow.Exists(a => a.FlowHandle == (int)CloudWindTaskFlowHandleStatus.退回) && latestFlow.NodeID == firstNode.ID)
                        {
                            // 退回后重新提交：更新任务信息，重新生成文件，重新计算
                            thisTask.TaskName = request.TaskName ?? "";
                            thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.审批中;

                            #region 生成文件
                            string fileImportFolder = AppContext.BaseDirectory + @"File\Project\" + project.ProjectCode + @"\Task\" + thisTask.TaskCode + @"\Import";

                            var dkList = dkRepo.Find(a => !a.IsDelete).ToList();
                            var dkIds = request.DKIDstring.Split(',').Select(id => Convert.ToInt32(id)).ToList();
                            var dkDataList = dkDataRepo.Find(a => !a.IsDelete && dkIds.Contains(a.DKID.Value)).ToList();

                            DataSet dsDK = new DataSet();
                            foreach (int i in dkIds)
                            {
                                var dk = dkList.FirstOrDefault(a => a.ID == i);
                                if (dk != null)
                                {
                                    DataTable dt = new DataTable();
                                    dt.TableName = dk.DKName;
                                    dt.Columns.Add("序号"); dt.Columns.Add("土层编号"); dt.Columns.Add("土层名称");
                                    dt.Columns.Add("层底标高(m)"); dt.Columns.Add("土层类型"); dt.Columns.Add("不排水抗剪强度cu(kPa)");
                                    dt.Columns.Add("砂土摩擦角°"); dt.Columns.Add("有效重度"); dt.Columns.Add("标贯击数");
                                    dkDataList.Where(a => a.DKID == dk.ID).ToList().ForEach(a =>
                                    {
                                        DataRow dr = dt.NewRow();
                                        dr[0] = a.xh; dr[1] = a.dcbh; dr[2] = a.tcbh; dr[3] = a.cdbg;
                                        dr[4] = a.tclx; dr[5] = a.bpskjqd; dr[6] = a.stmcj; dr[7] = a.yxzd; dr[8] = a.bgjs;
                                        dt.Rows.Add(dr);
                                    });
                                    dsDK.Tables.Add(dt);
                                }
                            }
                            var dkFileTempName = "地勘计算文件(" + DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified).ToString("yyyyMMddHHmmss") + ").xlsx";
                            var dkFilePath = fileImportFolder + @"\" + dkFileTempName;
                            var dkSuccess = ExcelUtils.DataSetToExcel(dsDK, dkFilePath, true);
                            if (dkSuccess)
                            {
                                dkFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList()
                                    .ForEach(a => { a.IsDelete = true; });
                                dkFileRepo.Add(new Flow_Task_DKFile() { FileName = dkFileTempName, FilePath = dkFilePath, CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified), TaskID = thisTask.ID, IsDelete = false });
                            }
                            else { throw new Exception("生成地勘资料时发生错误。"); }

                            var shipIds = request.ShipIDstring.Split(',').Select(id => Convert.ToInt32(id)).ToList();
                            var ships = shipViewRepo.Find(a => shipIds.Contains(a.ID)).ToList();
                            var shipFiles = new List<Flow_Task_ShipFile>();
                            foreach (var s in ships)
                            {
                                DataTable dt = new DataTable();
                                dt.TableName = "VesselData";
                                dt.Columns.Add("船舶参数"); dt.Columns.Add("数值"); dt.Columns.Add("单位");
                                dt.Rows.Add("桩腿截面积", s.ztjmj, s.ztjmj_unit);
                                dt.Rows.Add("桩腿周长", s.ztzc, s.ztzc_unit);
                                dt.Rows.Add("桩靴长度 L", string.IsNullOrEmpty(s.zxcdL) ? "*" : s.zxcdL, s.zxcdL_unit);
                                dt.Rows.Add("桩靴宽度 B", s.zxkdB, s.zxkdB_unit);
                                dt.Rows.Add("桩靴高度 H", s.zxgdH, s.zxgdH_unit);
                                dt.Rows.Add("桩靴面积 A", s.zxmjA, s.zxmjA_unit);
                                dt.Rows.Add("桩靴最大截面周长", s.zxzdjmzc, s.zxzdjmzc_unit);
                                dt.Rows.Add("桩靴体积 V", s.zxtjV, s.zxtjV_unit);
                                dt.Rows.Add("桩腿、桩靴自重 W", s.ztzxzzW, s.ztzxzzW_unit);
                                dt.Rows.Add("桩腿预压力", s.ztyyl, s.ztyyl_unit);
                                dt.Rows.Add("计算预压荷载", s.jsyyhz, s.jsyyhz_unit);
                                dt.Rows.Add("拔桩力", s.bzl, s.bzl_unit);
                                dt.Rows.Add("对地比压", s.ddby, s.ddby_unit);
                                dt.Rows.Add("有效桩腿长度（船底到靴底）", s.yxztcd_cddxd, s.yxztcd_cddxd_unit);
                                dt.Rows.Add("气隙（船底到水面）", s.qx_cddsm, s.qx_cddsm_unit);
                                dt.Rows.Add("桩腿有效长度", s.ztyxcd, s.ztyxcd_unit);
                                DataSet dsShip = new DataSet(); dsShip.Tables.Add(dt);
                                var shipFileTempName = s.ShipName + "(" + DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified).ToString("yyyyMMddHHmmss") + ").xlsx";
                                var shipFilePath = fileImportFolder + @"\" + shipFileTempName;
                                var shipSuccess = ExcelUtils.DataSetToExcel(dsShip, shipFilePath, false);
                                if (shipSuccess)
                                    shipFiles.Add(new Flow_Task_ShipFile() { TaskID = thisTask.ID, FileName = s.ShipName + ".xlsx", FilePath = shipFilePath, CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified), IsDelete = false });
                            }
                            shipFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList().ForEach(a => { a.IsDelete = true; });
                            shipFileRepo.AddList(shipFiles);
                            shipRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList().ForEach(a => { a.TaskID = thisTask.ID; });
                            #endregion

                            uow.Save();

                            // 插拔桩计算
                            bool cbzSuccess = false;
                            {
                                string token = CloudCenterService.CreateToken(UserCode, thisTask.ID);
                                var cbzUrl = AppSettingUtils.GetSetting("SoftwareApi:CBZUrl");
                                url = cbzUrl + @"?Token=" + token;
                                LoggerUtils.Error(url, typeof(TaskService));
                                string retStr = HttpUtils.GetSync(url);
                                LoggerUtils.Error(retStr, typeof(TaskService));
                                string message = GetMessageCBZ(retStr);
                                if (string.IsNullOrEmpty(message))
                                {
                                    LoggerUtils.Error("插拔桩计算成功", typeof(TaskService));
                                    cbzSuccess = true;
                                }
                                else
                                {
                                    LoggerUtils.Error(message, typeof(TaskService));
                                    throw new Exception("插拔装计算发生错误：" + message);
                                }
                            }

                            if (cbzSuccess)
                            {
                                flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.审批通过;
                                flow.ApprovalTime = now;
                                flow.Comment = request.TaskComment ?? "";

                                if (nodeManageUsers == null || nodeManageUsers.Count == 0)
                                {
                                    if (string.IsNullOrEmpty(request.TaskNodeName) || string.IsNullOrEmpty(request.TaskNodeUserCode))
                                        throw new Exception("没有选择下一节点人员");
                                    flowRepo.Add(new Flow_TaskApply()
                                    {
                                        TaskID = request.TaskID, NodeID = nextNode.ID, FlowOrder = flow.FlowOrder + 1,
                                        NodeUserName = request.TaskNodeName ?? "", NodeUserCode = request.TaskNodeUserCode ?? "",
                                        NodeUserDepart = request.TaskNodeDepart ?? "", NodeUserPhone = request.TaskNodePhone ?? "",
                                        NodeUserJobName = request.TaskNodeJobName ?? "", FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                        Comment = "", CreateTime = now, IsDelete = false
                                    });
                                    informers.Add(request.TaskNodeUserCode);
                                }
                                else
                                {
                                    nodeManageUsers.ForEach(a =>
                                    {
                                        flowRepo.Add(new Flow_TaskApply()
                                        {
                                            TaskID = request.TaskID, NodeID = nextNode.ID, FlowOrder = flow.FlowOrder + 1,
                                            NodeUserName = a.ManageName, NodeUserCode = a.ManageUserCode,
                                            NodeUserDepart = a.ManageDepart, NodeUserPhone = a.ManagePhone,
                                            NodeUserJobName = a.ManageJobName, FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                            Comment = "", CreateTime = now, IsDelete = false
                                        });
                                        informers.Add(a.ManageUserCode);
                                    });
                                }
                            }
                        }
                        else
                        {
                            // 非退回流程：正常审批通过
                            flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.审批通过;
                            flow.ApprovalTime = now;
                            flow.Comment = request.TaskComment ?? "";

                            if (flowInfo.NodeApprovalType)
                            {
                                // 单人审批通过类型
                                if (nextNode == null)
                                {
                                    thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.完成;
                                    doAddCopyer = true;
                                }
                                else
                                {
                                    thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.审批中;
                                    if (thisFlows.Count > 0 && (nodeManageUsers == null || nodeManageUsers.Count == 0))
                                        throw new Exception("多人审批环节的下一节点，必须配置默认审批人员,请联系管理员配置");
                                    if ((nodeManageUsers == null || nodeManageUsers.Count == 0) && string.IsNullOrEmpty(request.TaskNodeUserCode))
                                        throw new Exception("没有选择下一节点人员");

                                    if (nodeManageUsers != null && nodeManageUsers.Count > 0)
                                    {
                                        nodeManageUsers.ForEach(a =>
                                        {
                                            flowRepo.Add(new Flow_TaskApply()
                                            {
                                                TaskID = request.TaskID, NodeID = nextNode.ID, FlowOrder = flow.FlowOrder + 1,
                                                NodeUserName = a.ManageName ?? "", NodeUserCode = a.ManageUserCode ?? "",
                                                NodeUserDepart = a.ManageDepart ?? "", NodeUserPhone = a.ManagePhone ?? "",
                                                NodeUserJobName = a.ManageJobName ?? "", FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                                Comment = "", CreateTime = now, IsDelete = false
                                            });
                                            informers.Add(a.ManageUserCode);
                                        });
                                    }
                                    else
                                    {
                                        flowRepo.Add(new Flow_TaskApply()
                                        {
                                            TaskID = request.TaskID, NodeID = nextNode.ID, FlowOrder = flow.FlowOrder + 1,
                                            NodeUserName = request.TaskNodeName ?? "", NodeUserCode = request.TaskNodeUserCode ?? "",
                                            NodeUserDepart = request.TaskNodeDepart ?? "", NodeUserPhone = request.TaskNodePhone ?? "",
                                            NodeUserJobName = request.TaskNodeJobName ?? "", FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                            Comment = "", CreateTime = now, IsDelete = false
                                        });
                                        informers.Add(request.TaskNodeUserCode);
                                    }
                                }
                                // 同节点其他流程删除
                                if (thisFlows.Count > 0)
                                    thisFlows.ForEach(a => { a.IsDelete = true; });
                            }
                            else
                            {
                                // 必须所有人审批通过
                                if (thisFlows.Count == 0)
                                {
                                    if (nextNode == null)
                                    {
                                        thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.完成;
                                        doAddCopyer = true;
                                    }
                                    else
                                    {
                                        if ((nodeManageUsers == null || nodeManageUsers.Count == 0) && string.IsNullOrEmpty(request.TaskNodeUserCode))
                                            throw new Exception("没有选择下一节点人员");
                                        thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.审批中;

                                        if (nodeManageUsers != null && nodeManageUsers.Count > 0)
                                        {
                                            nodeManageUsers.ForEach(a =>
                                            {
                                                flowRepo.Add(new Flow_TaskApply()
                                                {
                                                    TaskID = request.TaskID, NodeID = nextNode.ID, FlowOrder = flow.FlowOrder + 1,
                                                    NodeUserName = a.ManageName ?? "", NodeUserCode = a.ManageUserCode ?? "",
                                                    NodeUserDepart = a.ManageDepart ?? "", NodeUserPhone = a.ManagePhone ?? "",
                                                    NodeUserJobName = a.ManageJobName ?? "", FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                                    Comment = "", CreateTime = now, IsDelete = false
                                                });
                                                informers.Add(a.ManageUserCode);
                                            });
                                        }
                                        else
                                        {
                                            flowRepo.Add(new Flow_TaskApply()
                                            {
                                                TaskID = request.TaskID, NodeID = nextNode.ID, FlowOrder = flow.FlowOrder + 1,
                                                NodeUserName = request.TaskNodeName ?? "", NodeUserCode = request.TaskNodeUserCode ?? "",
                                                NodeUserDepart = request.TaskNodeDepart ?? "", NodeUserPhone = request.TaskNodePhone ?? "",
                                                NodeUserJobName = request.TaskNodeJobName ?? "", FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                                Comment = "", CreateTime = now, IsDelete = false
                                            });
                                            informers.Add(request.TaskNodeUserCode);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    uow.Save();

                    // 交建通通知
                    if (doAddInformer)
                    {
                        var informTitle = project.ProjectName + "(" + thisTask.TaskCode + ")-插拔桩计算流程审批";
                        if (doAddCopyer)
                        {
                            informTitle = project.ProjectName + "(" + thisTask.TaskCode + ")-插拔桩计算完成";
                            informers.Add(thisTask.ApplyerCode);
                        }

                        string cloudPlatUrl = AppSettingUtils.GetSetting("SoftwareApi:CloudPlatUrl");
                        List<SystemJJTInform> informRequests = new List<SystemJJTInform>();
                        foreach (string userCode in informers)
                        {
                            var token = _cloudCenterService.CreateJJTMessageRedirectToken("Task", userCode, thisTask.ID);
                            var redirectUrl = cloudPlatUrl + @"Home/JJTMessageRedirect?CloudToken=" + token;
                            informRequests.Add(new SystemJJTInform()
                            {
                                UserCode = userCode,
                                Url = redirectUrl,
                                Title = informTitle,
                                Content = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified).ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                            });
                        }
                        await _cloudWindInfoService.SendJJTMessage(informRequests);
                    }

                    // 抄送
                    if (doAddCopyer)
                    {
                        string cloudPlatUrl = AppSettingUtils.GetSetting("SoftwareApi:CloudPlatUrl");
                        List<SystemJJTInform> informRequests = new List<SystemJJTInform>();
                        copyerRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.自升式平台桩腿插拔计算).ToList()
                            .ForEach(a =>
                            {
                                if (!string.IsNullOrEmpty(a.UserCode))
                                {
                                    var token = _cloudCenterService.CreateJJTMessageRedirectToken("Task", a.UserCode, thisTask.ID);
                                    var redirectUrl = cloudPlatUrl + @"Home/JJTMessageRedirect?CloudToken=" + token;
                                    informRequests.Add(new SystemJJTInform()
                                    {
                                        UserCode = a.UserCode,
                                        Url = redirectUrl,
                                        Title = project.ProjectName + "(" + thisTask.TaskCode + ")-插拔桩计算完成",
                                        Content = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified).ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                                    });
                                }
                            });
                        await _cloudWindInfoService.SendJJTMessage(informRequests);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                url = "";
            }
            return url;
        }

        /// <summary>
        /// 退回至上一节点（插拔桩）
        /// </summary>
        public async Task TaskBackLastSubmit_CBZ(CloudWindRequest request)
        {
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var nodeRepo = uow.GetRepository<Flow_Node>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();

                    DateTime now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

                    var thisTask = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (thisTask == null) throw new Exception("任务数据错误");

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == thisTask.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("任务数据错误");

                    // 获取流程信息
                    var flowInfo = GetTaskFlow(thisTask.ID, UserCode);
                    if (!flowInfo.Success) throw new Exception(flowInfo.Message);

                    var node = nodeRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.自升式平台桩腿插拔计算 && a.ID == flowInfo.NodeID).FirstOrDefault();

                    var taskFlows = flowRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList();
                    var latestFlow = taskFlows.OrderByDescending(a => a.CreateTime).FirstOrDefault();
                    if (latestFlow == null) throw new Exception("流程数据错误");

                    var flow = taskFlows.FirstOrDefault(a => a.NodeID == latestFlow.NodeID && a.FlowOrder == latestFlow.FlowOrder && a.NodeUserCode == UserCode);
                    if (flow == null) throw new Exception("流程数据错误");

                    bool doAddInformer = true;
                    List<string> informers = new List<string>();
                    bool doAddCopyer = false;

                    flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.退回;
                    flow.ApprovalTime = now;
                    flow.Comment = request.TaskComment;
                    thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.退回;

                    var nodes = nodeRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.自升式平台桩腿插拔计算)
                        .OrderBy(a => a.NodeNo).ToList();
                    var nodeNo = nodes.FirstOrDefault(a => a.ID == latestFlow.NodeID).NodeNo;
                    var lastNode = nodes.FirstOrDefault(a => a.NodeNo == (nodeNo - 1));

                    if (latestFlow.FlowOrder == 1)
                    {
                        // 起始点，退回至申请人
                        flowRepo.Add(new Flow_TaskApply()
                        {
                            TaskID = request.TaskID,
                            NodeID = (int)CloudWindSoftwareCBZFlowNode.任务申请,
                            FlowOrder = flow.FlowOrder + 1,
                            NodeUserName = thisTask.Applyer ?? "",
                            NodeUserCode = thisTask.ApplyerCode ?? "",
                            NodeUserDepart = thisTask.ApplyerDepart ?? "",
                            NodeUserPhone = thisTask.ApplyerPhone ?? "",
                            NodeUserJobName = thisTask.ApplyerJobName ?? "",
                            FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批,
                            Comment = "",
                            CreateTime = now,
                            IsDelete = false
                        });
                        informers.Add(thisTask.ApplyerCode);
                    }
                    else
                    {
                        // 退回至上一节点
                        taskFlows.Where(a => a.FlowOrder == (latestFlow.FlowOrder - 1)).ToList()
                            .ForEach(a =>
                            {
                                flowRepo.Add(new Flow_TaskApply()
                                {
                                    TaskID = request.TaskID,
                                    NodeID = lastNode.ID,
                                    FlowOrder = flow.FlowOrder + 1,
                                    NodeUserName = a.NodeUserName,
                                    NodeUserCode = a.NodeUserCode,
                                    NodeUserDepart = a.NodeUserDepart,
                                    NodeUserPhone = a.NodeUserPhone,
                                    NodeUserJobName = a.NodeUserJobName,
                                    FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批,
                                    Comment = "",
                                    CreateTime = now,
                                    IsDelete = false
                                });
                                informers.Add(a.NodeUserCode);
                            });
                    }

                    uow.Save();

                    // 交建通通知
                    if (doAddInformer)
                    {
                        var informTitle = project.ProjectName + "(" + thisTask.TaskCode + ")-插拔桩计算流程审批";
                        if (doAddCopyer)
                        {
                            informTitle = project.ProjectName + "(" + thisTask.TaskCode + ")-插拔桩计算完成";
                            informers.Add(thisTask.ApplyerCode);
                        }

                        string cloudPlatUrl = AppSettingUtils.GetSetting("SoftwareApi:CloudPlatUrl");
                        List<SystemJJTInform> informRequests = new List<SystemJJTInform>();
                        foreach (string userCode in informers)
                        {
                            var token = _cloudCenterService.CreateJJTMessageRedirectToken("Task", userCode, thisTask.ID);
                            var redirectUrl = cloudPlatUrl + @"Home/JJTMessageRedirect?CloudToken=" + token;
                            informRequests.Add(new SystemJJTInform()
                            {
                                UserCode = userCode,
                                Url = redirectUrl,
                                Title = informTitle,
                                Content = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified).ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                            });
                        }
                        await _cloudWindInfoService.SendJJTMessage(informRequests);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
            }
        }

        /// <summary>
        /// 作废任务
        /// </summary>
        public void CancelTask(int taskId)
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var taskRepo = uow.GetRepository<Wind_Task>();
                var task = taskRepo.Find(a => !a.IsDelete && a.ID == taskId).FirstOrDefault();
                if (task == null)
                {
                    throw new Exception("任务信息出错");
                }
                task.FlowStatus = (int)CloudWindTaskFlowStatus.作废;
                task.CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                uow.Save();
            }
        }

        #endregion

    }
}
