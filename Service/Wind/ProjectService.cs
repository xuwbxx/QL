using BIM.Business.CCSHJWebApi;
using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Model.Tech.Cloud;
using Model.Tech.Cloud.BackManage;
using Model.Tech.System;
using Service.Base;
using System.Data;
using Tool;

namespace Service.Wind
{
    public class ProjectService : WindBaseService
    {
        public ProjectService(CloudWind_KingBase_UnitOfWorkFactory techCenterUowFactory, CookieService cookieService, CloudWindInfoService cloudWindInfoService, CloudCenterService cloudCenterService, IWebHostEnvironment env)
            : base(techCenterUowFactory, cookieService)
        {
            _cloudWindInfoService = cloudWindInfoService;
            _cloudCenterService = cloudCenterService;
            _env = env;
        }

        private readonly CloudWindInfoService _cloudWindInfoService;
        private readonly CloudCenterService _cloudCenterService;
        private readonly IWebHostEnvironment _env;

        public Wind_Project getProjectByID(int ProjectID)
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Wind_Project>();

                Wind_Project p = repo.FindByID(ProjectID);

                return p;

            }
        }

        public bool hasProjectViewRight(int ProjectID, string UserCode)
        {
            if (ProjectID == 0)
            {
                return false;
            }
            var IsHasRight = false;

            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var Manage_Admin_Repo = uow.GetRepository<Manage_Admin>();
                    var Manage_Copyer_Repo = uow.GetRepository<Manage_Copyer>();
                    var Manage_Viewer_Repo = uow.GetRepository<Manage_Viewer>();
                    var Wind_ProjectRole_Repo = uow.GetRepository<Wind_ProjectRole>();
                    var Flow_ProjectApply_Repo = uow.GetRepository<Flow_ProjectApply>();
                    var Wind_ProjectContacter_Repo = uow.GetRepository<Wind_ProjectContacter>();

                    //管理员、抄送人、局领导、协调员、项目组成员、项目经理、申请人、审核人
                    List<string> users = new List<string>();
                    Manage_Admin_Repo.Find(a => !a.IsDelete).ToList().ForEach(a =>
                    {
                        users.Add(a.UserCode);
                    });
                    Manage_Copyer_Repo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.海上风电WebGIS平台).ToList().ForEach(a =>
                    {
                        users.Add(a.UserCode);
                    });
                    Manage_Viewer_Repo.Find(a => !a.IsDelete).ToList().ForEach(a =>
                    {
                        users.Add(a.UserCode);
                    });
                    Wind_ProjectRole_Repo.Find(a => !a.IsDelete && a.ProjectID == ProjectID).ToList().ForEach(a =>
                    {
                        users.Add(a.UserCode);
                    });
                    Flow_ProjectApply_Repo.Find(a => !a.IsDelete && a.ProjectID == ProjectID).ToList().ForEach(a =>
                    {
                        users.Add(a.NodeUserCode);
                    });
                    var ProjectContacter = Wind_ProjectContacter_Repo.Find(a => !a.IsDelete && a.ProjectID == ProjectID).FirstOrDefault();
                    if (ProjectContacter != null)
                    {
                        users.Add(ProjectContacter.ApplyerCode);
                        users.Add(ProjectContacter.DirectorCode);
                    }


                    IsHasRight = users.Contains(UserCode);

                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                return false;
            }

            return IsHasRight;

        }

        public List<Manage_Company> getCompanys()
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Manage_Company>();

                var list = repo.Find(a => !a.IsDelete).ToList();

                return list;

            }
        }

        public async Task<(List<CloudWindProject> list, string msg)> GetProjectList(CloudWindRequest request)
        {
            List<CloudWindProject> list = new List<CloudWindProject>();
            string msg = "";
            try
            {
                //管理员
                var Admins = await GetAdminUserCode();
                //局领导
                var Viewers = await GetViewUserCode();
                //抄送人
                var Copyers = await GetCopyUserCode((int)CloudWindSoftware.海上风电WebGIS平台);

                var ProjectRoles = await GetProjectRoles();

                var ProjectList = await GetProjects();

                //View_Wind_ProjectFlow 后面2个表isdelete = 0接删除了，因为新增立项没有Wind_ProjectRole Wind_Project_Copyer 数据
                //1.获取项目相关人员

                using (var uow = _techCenterUowFactory.Create())
                {
                    var Flow_ProjectApply_repo = uow.GetRepository<Flow_ProjectApply>();
                    var View_Wind_ProjectFlow_repo = uow.GetRepository<View_Wind_ProjectFlow>();
                    var ProjectGroupList = await View_Wind_ProjectFlow_repo.FindAsync(a => (string.IsNullOrEmpty(request.ProjectName) || a.ProjectName.Contains(request.ProjectName)) && (a.ApplyerCode.Equals(UserCode) || a.DirectorCode.Equals(UserCode) || a.NodeUserCode.Equals(UserCode) || (!a.isroleexist.Value && a.roleusercode.Equals(UserCode)) || Admins.Contains(UserCode) || Viewers.Contains(UserCode) || Copyers.Contains(UserCode)));
                    ProjectGroupList.ToList().GroupBy(a => a.ProjectID).ToList().ForEach(a =>
                    {
                        int ProjectID = a.FirstOrDefault().ProjectID.Value;

                        CloudWindProject model = new CloudWindProject();
                        model.ID = ProjectID;
                        model.ProjectName = a.FirstOrDefault().ProjectName;
                        model.ProjectCode = a.FirstOrDefault().ProjectCode;
                        model.IsFinished = a.FirstOrDefault().FlowStatus == (int)CloudWindProjectFlowStatus.激活 ? true : false;
                        model.ProjectStartTime = a.FirstOrDefault().ProjectStartTime == null ? "" : a.FirstOrDefault().ProjectStartTime.Value.ToString("yyyy-MM-dd");
                        model.ProjectEndTime = a.FirstOrDefault().ProjectEndTime == null ? "" : a.FirstOrDefault().ProjectEndTime.Value.ToString("yyyy-MM-dd");
                        model.ProjectStatus = (Enum.GetName(typeof(CloudWindProjectStatus), a.FirstOrDefault().projectstatus));

                        model.Applyer.ApplyerName = a.FirstOrDefault().Applyer ?? "";
                        model.Applyer.ApplyerDepart = a.FirstOrDefault().ApplyerDepart ?? "";
                        model.Applyer.ApplyerPhone = a.FirstOrDefault().ApplyerPhone ?? "";
                        model.Applyer.ApplyerJobName = a.FirstOrDefault().ApplyerJobName ?? "";

                        model.Director.DirectorName = a.FirstOrDefault().Director ?? "";
                        model.Director.DirectorDepart = a.FirstOrDefault().DirectorDepart ?? "";
                        model.Director.DirectorPhone = a.FirstOrDefault().DirectorPhone ?? "";
                        model.Director.DirectorJobName = a.FirstOrDefault().DirectorJobName ?? "";

                        var ThisProject = ProjectList.FirstOrDefault(b => b.ID == ProjectID);
                        model.IsWebGIS = (string.IsNullOrEmpty(ThisProject.Lon) || string.IsNullOrEmpty(ThisProject.Lat)) ? false : true;

                        //协调员
                        var ProjectAssisters = ProjectRoles.Where(b => b.ProjectID == ProjectID).Take(5).ToList();
                        if (ProjectAssisters == null || ProjectAssisters.Count == 0)
                        {
                            model.Assister = "";
                        }
                        else
                        {
                            ProjectAssisters.ForEach(b =>
                            {
                                model.Assister += b.UserName + "、";
                            });
                        }
                        model.Assister = model.Assister.Trim('、');


                        model.ApplyTime = a.FirstOrDefault().CreateTime.ToString("yyyy-MM-dd HH:mm");
                        model.FlowStatus = a.FirstOrDefault().FlowStatus;
                        model.FlowStatusName = (Enum.GetName(typeof(CloudWindProjectFlowStatus), a.FirstOrDefault().FlowStatus));

                        var LatestFlow = View_Wind_ProjectFlow_repo.Find(b => b.ProjectID == ProjectID).OrderByDescending(b => b.CreateTime).FirstOrDefault();
                        //var LatestFlow = service.Repository_CloudWind_View_Wind_ProjectFlow.Query(b => b.ProjectID == ProjectID).OrderByDescending(b => b.CreateTime).FirstOrDefault();
                        model.FlowID = LatestFlow.ID;
                        model.FlowNode = LatestFlow.NodeName;

                        //var ThisFlows = service.Repository_CloudWind_Flow_ProjectApply.Query(b => !b.IsDelete.Value && b.ProjectID == ProjectID && b.NodeID == LatestFlow.NodeID && b.FlowOrder == LatestFlow.FlowOrder).ToList();
                        var ThisFlows = Flow_ProjectApply_repo.Find(b => !b.IsDelete && b.ProjectID == ProjectID && b.NodeID == LatestFlow.NodeID && b.FlowOrder == LatestFlow.FlowOrder).ToList();


                        ThisFlows.ForEach(b =>
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
                        if (!model.IsFinished && ThisFlows.Exists(b => b.NodeUserCode.Equals(UserCode) && b.FlowHandle == (int)CloudWindProjectFlowHandleStatus.待审批))
                        {
                            model.FlowHandle = "approval";
                            //var LastFlow = service.Repository_CloudWind_Flow_ProjectApply.Query(b => !b.IsDelete.Value && b.ProjectID == ProjectID && b.FlowHandle != (int)CloudWindProjectFlowHandleStatus.待审批).OrderByDescending(b => b.CreateTime).FirstOrDefault();
                            var LastFlow = Flow_ProjectApply_repo.Find(b => b.IsDelete && b.ProjectID == ProjectID && b.FlowHandle != (int)CloudWindProjectFlowHandleStatus.待审批).OrderByDescending(b => b.CreateTime).FirstOrDefault();
                            if (LastFlow != null && LastFlow.FlowHandle == (int)CloudWindProjectFlowHandleStatus.退回)
                            {
                                model.FlowHandle = "renew";
                            }
                        }

                        model.DoRightEdit = false;
                        model.DoRightDelete = false;
                        //超级管理员和协调员可以修改,加上项目经理
                        if (Admins.Contains(UserCode) || a.FirstOrDefault().DirectorCode.Equals(UserCode) || ProjectRoles.Exists(b => b.ProjectID == ProjectID && b.RoleID == (int)CloudWindProjectRole.项目协调员 && b.UserCode.Equals(UserCode)))
                        {
                            model.DoRightEdit = true;

                            if (a.FirstOrDefault().FlowStatus != (int)CloudWindProjectFlowStatus.激活)
                            {
                                model.DoRightDelete = true;
                            }

                        }
                        //model.IsCurrentNodeUser = LastestFlow.NodeUserCode.Equals(UserCode) ? true : false;
                        list.Add(model);
                    });



                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                list = new List<CloudWindProject>();
                msg = "发生错误";
            }

            return (list, msg);
        }

        public async Task<List<CloudWindProject>> GetWindProject()
        {
            try
            {
                //管理员
                var Admins = await GetAdminUserCode();
                //局领导
                var Viewers = await GetViewUserCode();
                //抄送人
                var Copyers = await GetCopyUserCode((int)CloudWindSoftware.海上风电WebGIS平台);

                List<CloudWindProject> list = new List<CloudWindProject>();

                using (var uow = _techCenterUowFactory.Create())
                {
                    var repo = uow.GetRepository<View_Wind_ProjectFlow>();
                    repo.Find(a => (a.ApplyerCode.Equals(UserCode) || a.DirectorCode.Equals(UserCode) || (!a.isroleexist.Value && a.roleusercode.Equals(UserCode)) || Admins.Contains(UserCode) || Viewers.Contains(UserCode) || Copyers.Contains(UserCode)))
                        .GroupBy(a => a.ProjectID).ToList().ForEach(a =>
                        {
                            int ProjectID = a.FirstOrDefault().ProjectID.Value;
                            list.Add(new CloudWindProject()
                            {
                                ID = a.FirstOrDefault().ProjectID.Value,
                                ProjectName = a.FirstOrDefault().ProjectName
                            });
                        });
                }

                return list;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                return new List<CloudWindProject>();
            }


        }

        public async Task<(CloudWindProjectFlow data, string msg)> GetProject(CloudWindRequest request)
        {
            CloudWindProjectFlow data = new CloudWindProjectFlow();
            string msg = "";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var Wind_Project_repo = uow.GetRepository<Wind_Project>();
                    var Wind_ProjectContacter_repo = uow.GetRepository<Wind_ProjectContacter>();
                    var Manage_Company_repo = uow.GetRepository<Manage_Company>();

                    #region 项目基本信息
                    //项目信息
                    var Project = Wind_Project_repo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (Project == null)
                    {
                        msg = "项目数据错误";
                        return (data, msg);
                    }

                    //项目申请人和管理员
                    var Contactor = Wind_ProjectContacter_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).FirstOrDefault();
                    if (Contactor == null)
                    {
                        msg = "项目数据错误";
                        return (data, msg);
                    }

                    //项目基础信息（TODO: Wind_ProjectInfo 实体尚未在 DbContext 中注册，需先添加）
                    var ProjectInfo = uow.GetRepository<Wind_ProjectInfo>().Find(a => !a.IsDelete.Value && a.ProjectID == request.ProjectID).FirstOrDefault();
                    if (ProjectInfo == null)
                    {
                        msg = "项目数据错误";
                        return (data, msg);
                    }

                    var Companys = Manage_Company_repo.Find(a => !a.IsDelete).ToList();

                    //1.基础信息
                    data.ProjectID = Project.ID;
                    data.ProjectName = Project.ProjectName;
                    data.ProjectCode = Project.ProjectCode;
                    data.CompanyID = Project.CompanyID ?? 0;

                    data.ProjectStartTime = Project.ProjectStartTime.Value.ToString("yyyy-MM-dd");
                    data.ProjectEndTime = Project.ProjectEndTime.Value.ToString("yyyy-MM-dd");
                    data.Status = Project.Status.Value;
                    data.ProjectInfo.Company = Companys.FirstOrDefault(a => a.ID == Project.CompanyID) == null ? "" : Companys.FirstOrDefault(a => a.ID == Project.CompanyID).Company;
                    data.ProjectInfo.ProjectStatus = Enum.GetName(typeof(CloudWindProjectStatus), Project.Status.Value);
                    data.ProjectInfo.WaterDepth = ProjectInfo.WaterDepth ?? "";
                    data.ProjectInfo.WaterDepthMax = ProjectInfo.WaterDepthMax ?? "";
                    data.ProjectInfo.WaterDepthMin = ProjectInfo.WaterDepthMin ?? "";

                    //申请人信息
                    data.Applyer.UserName = Contactor.Applyer ?? "";
                    data.Applyer.UserCode = Contactor.ApplyerCode ?? "";
                    data.Applyer.UserDepart = Contactor.ApplyerDepart ?? "";
                    data.Applyer.UserPhone = Contactor.ApplyerPhone ?? "";
                    data.Applyer.UserJobName = Contactor.ApplyerJobName ?? "";
                    //项目经理
                    data.ProjectManager.UserName = Contactor.Director ?? "";
                    data.ProjectManager.UserCode = Contactor.DirectorCode ?? "";
                    data.ProjectManager.UserDepart = Contactor.DirectorDepart ?? "";
                    data.ProjectManager.UserPhone = Contactor.DirectorPhone ?? "";
                    data.ProjectManager.UserJobName = Contactor.DirectorJobName ?? "";
                    //项目组成员
                    var Wind_ProjectRole_repo = uow.GetRepository<Wind_ProjectRole>();
                    Wind_ProjectRole_repo.Find(a => !a.IsDelete && a.ProjectID == Project.ID && (a.RoleID == (int)CloudWindProjectRole.项目组成员 || a.RoleID == (int)CloudWindProjectRole.项目部领导班子))
                        .ToList().ForEach(a =>
                        {
                            data.ProjectGroupUser.Add(new ShjUserInfo()
                            {
                                UserCode = a.UserCode,
                                UserName = a.UserName,
                                UserDepart = a.UserDepartName,
                                UserPhone = a.UserPhone,
                                UserJobName = a.UserJobName
                            });
                        });

                    #endregion

                    #region 流程信息

                    var FlowInfo = await GetProjectFlow(Project.ID, UserCode);
                    if (!FlowInfo.Success)
                    {
                        msg = FlowInfo.Message;
                        return (data, msg);
                    }
                    data.FlowInfo = FlowInfo;

                    #endregion

                    #region 资源文件信息

                    var Wind_ProjectArea_repo = uow.GetRepository<Wind_ProjectArea>();
                    var Wind_ProjectFan_repo = uow.GetRepository<Wind_ProjectFan>();
                    var Wind_ProjectFile_repo = uow.GetRepository<Wind_ProjectFile>();
                    var Library_Geology_repo = uow.GetRepository<Library_Geology>();

                    //3.文件信息
                    //风场坐标
                    var ProjectPositions = Wind_ProjectArea_repo.Find(a => !a.IsDelete && a.ProjectID == Project.ID).ToList();
                    if (ProjectPositions != null && ProjectPositions.Count > 0)
                    {
                        ProjectPositions.ForEach(a =>
                        {
                            data.ProjectPosition.Add(new CloudProjectPosition()
                            {
                                Lon = a.AreaLon,
                                Lat = a.AreaLat
                            });
                        });
                    }

                    //风机坐标
                    var FanPositions = Wind_ProjectFan_repo.Find(a => !a.IsDelete && a.ProjectID == Project.ID).ToList();
                    if (FanPositions != null && FanPositions.Count > 0)
                    {
                        FanPositions.ForEach(a =>
                        {
                            data.ProjectFan.Add(new CloudProjectFanPosition()
                            {
                                FanName = a.FanName,
                                Lon = a.Lon,
                                Lat = a.Lat
                            });
                        });
                    }

                    //项目文件
                    var Files = Wind_ProjectFile_repo.Find(a => !a.IsDelete && a.ProjectID == Project.ID).ToList();
                    if (Files != null && Files.Count > 0)
                    {
                        Files.ForEach(a =>
                        {
                            data.ProjectFile.Add(new CloudProjectFile()
                            {
                                ID = a.ID,
                                FileName = a.FileName,
                                FilePath = a.FilePath.Substring(a.FilePath.IndexOf(@"\File\")),
                                FileTime = Convert.ToDateTime(a.CreateTime).ToString("yyyy-MM-dd")
                            });
                        });
                    }

                    //地勘信息
                    var GeologyFiles = Library_Geology_repo.Find(a => !a.IsDelete && a.ProjectID == Project.ID).ToList();
                    if (GeologyFiles != null && GeologyFiles.Count > 0)
                    {
                        //地勘原始资料
                        GeologyFiles.Where(a => a.Type == (int)CloudWindTaskFileType.风电项目_机孔位地勘原始资料).ToList().ForEach(a =>
                        {
                            data.File_dkyszl.Add(new CloudProjectFile()
                            {
                                FileName = a.FileName,
                                FilePath = a.FilePath.Substring(a.FilePath.IndexOf(@"\File\")),
                                FileTime = Convert.ToDateTime(a.CreateTime).ToString("yyyy-MM-dd")
                            });
                        });

                        //地勘数据表
                        GeologyFiles.Where(a => a.Type == (int)CloudWindTaskFileType.风电项目_机孔位地勘数据表).ToList().ForEach(a =>
                        {
                            data.File_dksjb.Add(new CloudProjectFile()
                            {
                                FileName = a.FileName,
                                FilePath = a.FilePath.Substring(a.FilePath.IndexOf(@"\File\")),
                                FileTime = Convert.ToDateTime(a.CreateTime).ToString("yyyy-MM-dd")
                            });
                        });
                    }

                    #endregion

                }



            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                data = new CloudWindProjectFlow();
                msg = "发生错误";
            }

            return (data, msg);
        }

        public async Task<FlowInfo> GetProjectFlow(int ProjectID, string UserCode)
        {
            FlowInfo flowInfo = new FlowInfo();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var Wind_Project_repo = uow.GetRepository<Wind_Project>();
                    var Flow_ProjectApply_repo = uow.GetRepository<Flow_ProjectApply>();
                    var Flow_Node_repo = uow.GetRepository<Flow_Node>();
                    var Wind_ProjectContacter_repo = uow.GetRepository<Wind_ProjectContacter>();
                    var Flow_NodeManageUser_repo = uow.GetRepository<Flow_NodeManageUser>();

                    var Project = Wind_Project_repo.Find(a => !a.IsDelete && a.ID == ProjectID).FirstOrDefault();
                    var ProjectFlows = Flow_ProjectApply_repo.Find(a => !a.IsDelete && a.ProjectID == ProjectID).ToList();
                    if (Project == null || ProjectFlows == null || ProjectFlows.Count == 0)
                    {
                        return new FlowInfo();
                    }
                    var AllNodes = Flow_Node_repo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.海上风电WebGIS平台).ToList();

                    //flow.FlowType = (int)CloudWindFlowType.项目申请类审批流程;
                    //审批中 = 0,
                    //退回 = 1,
                    //激活 = 2,
                    //删除 = 3
                    flowInfo.FlowStatus = Project.FlowStatus.Value;

                    //1.当前流程信息
                    var LatestFlow = ProjectFlows.OrderByDescending(a => a.CreateTime).FirstOrDefault();
                    var Node = AllNodes.FirstOrDefault(a => a.ID == LatestFlow.NodeID);
                    flowInfo.NodeID = Node.ID;
                    flowInfo.NodeName = Node.NodeName;
                    flowInfo.DoEdit = Node.DoEdit == null ? false : Node.DoEdit.Value;
                    flowInfo.NodeApprovalType = Node.NodeApprovalType == null ? false : Node.NodeApprovalType.Value;
                    var LatestFlows = ProjectFlows.Where(a => a.NodeID == LatestFlow.NodeID && a.FlowOrder == LatestFlow.FlowOrder).ToList();
                    LatestFlows.ForEach(a =>
                    {
                        flowInfo.Node.Add(new NodeInfo()
                        {
                            Comment = a.Comment ?? "",
                            UserName = a.NodeUserName ?? "",
                            UserCode = a.NodeUserCode ?? "",
                            UserDepart = a.NodeUserDepart ?? "",
                            UserPhone = a.NodeUserPhone ?? "",
                            UserJobName = a.NodeUserJobName ?? "",
                            FlowHandle = a.FlowHandle.Value
                        });
                    });
                    if (LatestFlow.NodeID == (int)CloudWindProjectFlowNode.技术中心云平台负责人审批 && LatestFlows.Exists(a => a.NodeUserCode.Equals(UserCode)) && Project.FlowStatus != (int)CloudWindProjectFlowStatus.激活)
                    {
                        flowInfo.IsCreateProjectCode = true;
                    }
                    else
                    {
                        flowInfo.IsCreateProjectCode = false;
                    }

                    //2.上个流程信息
                    var LastNode = AllNodes.FirstOrDefault(a => a.NodeNo == (Node.NodeNo - 1));
                    flowInfo.LastNodeID = LastNode == null ? 0 : LastNode.ID;
                    flowInfo.LastNodeName = LastNode == null ? "" : LastNode.NodeName;
                    var LastFlow = ProjectFlows.Where(a => a.FlowOrder == (LatestFlow.FlowOrder - 1)).ToList();
                    if (LastFlow == null || LastFlow.Count == 0)
                    {
                        //刚创建的流程
                        var ProjectManager = Wind_ProjectContacter_repo.Find(a => !a.IsDelete && a.ProjectID == ProjectID).FirstOrDefault();
                        flowInfo.LastNode.Add(new NodeInfo()
                        {
                            Comment = "",
                            UserName = ProjectManager == null ? "" : ProjectManager.Director,
                            UserCode = ProjectManager == null ? "" : ProjectManager.DirectorCode,
                            UserDepart = ProjectManager == null ? "" : ProjectManager.DirectorDepart,
                            UserPhone = ProjectManager == null ? "" : ProjectManager.DirectorPhone,
                            UserJobName = ProjectManager == null ? "" : ProjectManager.DirectorJobName,
                            FlowHandle = (int)CloudWindProjectFlowHandleStatus.审批通过
                        });
                    }
                    else
                    {
                        LastFlow.ForEach(a =>
                        {
                            flowInfo.LastNode.Add(new NodeInfo()
                            {
                                Comment = a.Comment ?? "",
                                UserName = a.NodeUserName ?? "",
                                UserCode = a.NodeUserCode ?? "",
                                UserDepart = a.NodeUserDepart ?? "",
                                UserPhone = a.NodeUserPhone ?? "",
                                UserJobName = a.NodeUserJobName ?? "",
                                FlowHandle = a.FlowHandle.Value
                            });
                        });
                    }

                    //3.下一个节点信息
                    var NextNode = AllNodes.FirstOrDefault(a => a.NodeNo == (Node.NodeNo + 1));
                    if (NextNode == null)
                    {
                        //流程结束
                        flowInfo.NextNodeID = 0;
                        flowInfo.NextNodeName = "流程结束";
                    }
                    else
                    {
                        flowInfo.NextNodeID = NextNode.ID;
                        flowInfo.NextNodeName = NextNode.NodeName;

                        //如果配置了节点人员，就强制；否则就让用户自己选择
                        var NodeManager = Flow_NodeManageUser_repo.Find(a => !a.IsDelete && a.NodeID == NextNode.ID).ToList();
                        if (NodeManager != null && NodeManager.Count > 0)
                        {
                            NodeManager.ForEach(a =>
                            {
                                flowInfo.NextNode.Add(new NodeInfo()
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

                    //4.判断是否退回的流程
                    if (LastFlow != null && LastFlow.Count > 0)
                    {
                        //只要有一个退回，就是退回状态
                        if (LastFlow.Exists(a => a.FlowHandle == (int)CloudWindProjectFlowHandleStatus.退回))
                        {
                            flowInfo.IsBackFlow = true;
                        }
                    }

                    //5.判断权限
                    flowInfo.FlowHandle = "view";
                    if (Project.FlowStatus == (int)CloudWindProjectFlowStatus.激活)
                    {
                        flowInfo.FlowHandle = "view";
                    }
                    else
                    {
                        var MyFlow = Flow_ProjectApply_repo.Find(a => !a.IsDelete && a.ProjectID == ProjectID && a.NodeID == Node.ID && a.FlowOrder == LatestFlow.FlowOrder && a.NodeUserCode.Equals(UserCode) && a.FlowHandle == (int)CloudWindProjectFlowHandleStatus.待审批).FirstOrDefault();
                        if (MyFlow != null)
                        {
                            flowInfo.FlowHandle = "approval";
                            if (flowInfo.IsBackFlow)
                            {
                                flowInfo.FlowHandle = "renew";
                            }
                            if (flowInfo.DoEdit)
                            {
                                //必须是审批情况下，才能编辑
                                flowInfo.DoEditConfirm = true;
                            }
                        }
                    }

                    flowInfo.Success = true;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                flowInfo = new FlowInfo();
                flowInfo.Success = false;
                flowInfo.Message = "获取流程信息出错,请联系技术中心管理员。";
            }

            return flowInfo;
        }

        public async Task<(CloudWindProjectFlow data, string msg)> GetProjectEdit(CloudWindRequest request)
        {
            CloudWindProjectFlow data = new CloudWindProjectFlow();
            string msg = "";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var Wind_Project_repo = uow.GetRepository<Wind_Project>();
                    var Wind_ProjectContacter_repo = uow.GetRepository<Wind_ProjectContacter>();
                    var Wind_ProjectRole_repo = uow.GetRepository<Wind_ProjectRole>();
                    var Wind_ProjectArea_repo = uow.GetRepository<Wind_ProjectArea>();
                    var Wind_ProjectFan_repo = uow.GetRepository<Wind_ProjectFan>();
                    var Wind_ProjectFile_repo = uow.GetRepository<Wind_ProjectFile>();
                    var Library_Geology_repo = uow.GetRepository<Library_Geology>();

                    #region 项目基本信息
                    //项目信息
                    var Project = Wind_Project_repo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (Project == null)
                    {
                        msg = "项目数据错误";
                        return (data, msg);
                    }

                    var Admins = await GetAdminUserCode();

                    var ProjectManage = Wind_ProjectContacter_repo.Find(a => !a.IsDelete && a.ProjectID == Project.ID).FirstOrDefault();
                    if (ProjectManage == null)
                    {
                        msg = "项目数据错误";
                        return (data, msg);
                    }

                    var ProjectRoles = Wind_ProjectRole_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList();

                    if (!Admins.Contains(UserCode) && !ProjectManage.DirectorCode.Equals(UserCode) && !ProjectRoles.Exists(a => a.UserCode.Equals(UserCode)))
                    {
                        msg = "没有权限修改项目资料";
                        return (data, msg);
                    }

                    //项目申请人和管理员
                    var Contactor = Wind_ProjectContacter_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).FirstOrDefault();
                    if (Contactor == null)
                    {
                        msg = "项目数据错误";
                        return (data, msg);
                    }
                    //项目基础信息
                    var ProjectInfo = uow.GetRepository<Wind_ProjectInfo>().Find(a => !a.IsDelete.Value && a.ProjectID == request.ProjectID).FirstOrDefault();
                    if (ProjectInfo == null)
                    {
                        msg = "项目数据错误";
                        return (data, msg);
                    }
                    //1.基础信息
                    data.ProjectID = Project.ID;
                    data.ProjectName = Project.ProjectName;
                    data.ProjectCode = Project.ProjectCode;
                    data.CompanyID = Project.CompanyID ?? 0;
                    data.ProjectStartTime = Project.ProjectStartTime.Value.ToString("yyyy-MM-dd");
                    data.ProjectEndTime = Project.ProjectEndTime.Value.ToString("yyyy-MM-dd");
                    data.Status = Project.Status.Value;
                    data.ProjectInfo.WaterDepth = ProjectInfo.WaterDepth ?? "";
                    data.ProjectInfo.WaterDepthMax = ProjectInfo.WaterDepthMax ?? "";
                    data.ProjectInfo.WaterDepthMin = ProjectInfo.WaterDepthMin ?? "";

                    //申请人信息
                    data.Applyer.UserName = Contactor.Applyer ?? "";
                    data.Applyer.UserCode = Contactor.ApplyerCode ?? "";
                    data.Applyer.UserDepart = Contactor.ApplyerDepart ?? "";
                    data.Applyer.UserPhone = Contactor.ApplyerPhone ?? "";
                    data.Applyer.UserJobName = Contactor.ApplyerJobName ?? "";
                    //项目负责人
                    data.ProjectManager.UserName = Contactor.Director ?? "";
                    data.ProjectManager.UserCode = Contactor.DirectorCode ?? "";
                    data.ProjectManager.UserDepart = Contactor.DirectorDepart ?? "";
                    data.ProjectManager.UserPhone = Contactor.DirectorPhone ?? "";
                    data.ProjectManager.UserJobName = Contactor.DirectorJobName ?? "";
                    //项目组成员
                    Wind_ProjectRole_repo.Find(a => !a.IsDelete && a.ProjectID == Project.ID && (a.RoleID == (int)CloudWindProjectRole.项目组成员 || a.RoleID == (int)CloudWindProjectRole.项目部领导班子))
                        .ToList().ForEach(a =>
                        {
                            data.ProjectGroupUser.Add(new ShjUserInfo()
                            {
                                UserCode = a.UserCode,
                                UserName = a.UserName,
                                UserDepart = a.UserDepartName,
                                UserPhone = a.UserPhone,
                                UserJobName = a.UserJobName
                            });
                        });

                    #endregion

                    #region 流程信息

                    var ThisFlowInfo = await GetProjectFlow(Project.ID, UserCode);
                    if (!ThisFlowInfo.Success)
                    {
                        msg = ThisFlowInfo.Message;
                        return (data, msg);
                    }
                    ThisFlowInfo.FlowHandle = "update";
                    data.FlowInfo = ThisFlowInfo;

                    #endregion

                    #region 资源文件信息
                    //3.文件信息
                    //风场坐标
                    var ProjectPositions = Wind_ProjectArea_repo.Find(a => !a.IsDelete && a.ProjectID == Project.ID).ToList();
                    if (ProjectPositions != null && ProjectPositions.Count > 0)
                    {
                        ProjectPositions.ForEach(a =>
                        {
                            data.ProjectPosition.Add(new CloudProjectPosition()
                            {
                                Lon = a.AreaLon,
                                Lat = a.AreaLat
                            });
                        });
                    }

                    //风机坐标
                    var FanPositions = Wind_ProjectFan_repo.Find(a => !a.IsDelete && a.ProjectID == Project.ID).ToList();
                    if (FanPositions != null && FanPositions.Count > 0)
                    {
                        FanPositions.ForEach(a =>
                        {
                            data.ProjectFan.Add(new CloudProjectFanPosition()
                            {
                                FanName = a.FanName,
                                Lon = a.Lon,
                                Lat = a.Lat
                            });
                        });
                    }

                    //项目文件
                    var Files = Wind_ProjectFile_repo.Find(a => !a.IsDelete && a.ProjectID == Project.ID).ToList();
                    if (Files != null && Files.Count > 0)
                    {
                        Files.ForEach(a =>
                        {
                            data.ProjectFile.Add(new CloudProjectFile()
                            {
                                ID = a.ID,
                                FileName = a.FileName,
                                FilePath = a.FilePath.Substring(a.FilePath.IndexOf(@"\File\")),
                                FileTime = Convert.ToDateTime(a.CreateTime).ToString("yyyy-MM-dd")
                            });
                        });
                    }

                    //地勘信息
                    var GeologyFiles = Library_Geology_repo.Find(a => !a.IsDelete && a.ProjectID == Project.ID).ToList();
                    if (GeologyFiles != null && GeologyFiles.Count > 0)
                    {
                        //地勘原始资料
                        GeologyFiles.Where(a => a.Type == (int)CloudWindTaskFileType.风电项目_机孔位地勘原始资料).ToList().ForEach(a =>
                        {
                            data.File_dkyszl.Add(new CloudProjectFile()
                            {
                                FileName = a.FileName,
                                FilePath = a.FilePath.Substring(a.FilePath.IndexOf(@"\File\")),
                                FileTime = Convert.ToDateTime(a.CreateTime).ToString("yyyy-MM-dd")
                            });
                        });

                        //地勘数据表
                        GeologyFiles.Where(a => a.Type == (int)CloudWindTaskFileType.风电项目_机孔位地勘数据表).ToList().ForEach(a =>
                        {
                            data.File_dksjb.Add(new CloudProjectFile()
                            {
                                FileName = a.FileName,
                                FilePath = a.FilePath.Substring(a.FilePath.IndexOf(@"\File\")),
                                FileTime = Convert.ToDateTime(a.CreateTime).ToString("yyyy-MM-dd")
                            });
                        });
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                data = new CloudWindProjectFlow();
                msg = "发生错误";
            }
            return (data, msg);
        }

        public async Task<(List<Wind_ProjectArea> data, string msg)> ProjectPositionUpload(CloudWindRequest request, IFormFile file)
        {
            List<Wind_ProjectArea> list = new List<Wind_ProjectArea>();
            string msg = "";
            try
            {
                if (file != null)
                {
                    using (var uow = _techCenterUowFactory.Create())
                    {
                        var Wind_ProjectArea_repo = uow.GetRepository<Wind_ProjectArea>();
                        var Wind_Project_repo = uow.GetRepository<Wind_Project>();

                        //保存文件到服务器
                        string saveDir = Path.Combine(_env.WebRootPath, "File", "Project", "ProjectPosition");
                        string FileName = Path.Combine(saveDir, Guid.NewGuid() + Path.GetExtension(file.FileName));
                        FileUtils.SaveFile(file, FileName);

                        //excel文件
                        DataSet dt = ExcelUtils.ConvertExcelFileToDataSet(file);

                        if (dt == null || dt.Tables == null || dt.Tables[0] == null)
                        {
                            msg = "表格数据为空";
                            return (list, msg);
                        }

                        //软删除旧数据
                        Wind_ProjectArea_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList()
                            .ForEach(a =>
                            {
                                a.IsDelete = true;
                                a.CreateTime = DateTime.UtcNow;
                            });

                        //更新项目中心
                        decimal Lons = 0;
                        decimal Lats = 0;
                        int Count = 0;
                        foreach (DataRow dr in dt.Tables[0].Rows)
                        {
                            Wind_ProjectArea area = new Wind_ProjectArea();
                            area.ProjectID = request.ProjectID;
                            area.AreaLon = dr[0].ToString().Trim();
                            area.AreaLat = dr[1].ToString().Trim();
                            area.CreateTime = DateTime.UtcNow;
                            area.IsDelete = false;
                            list.Add(area);

                            Lons += Convert.ToDecimal(dr[0].ToString().Trim());
                            Lats += Convert.ToDecimal(dr[1].ToString().Trim());
                            Count++;
                        }

                        var Project = Wind_Project_repo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                        if (Project != null)
                        {
                            Project.Lon = Math.Round(Lons / Count, 6).ToString();
                            Project.Lat = Math.Round(Lats / Count, 6).ToString();
                        }

                        Wind_ProjectArea_repo.AddList(list);
                        await uow.SaveAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                list = new List<Wind_ProjectArea>();
                msg = "发生错误";
            }
            return (list, msg);
        }

        public async Task<(List<Wind_ProjectFan> data, string msg)> ProjectFanPositionUpload(CloudWindRequest request, IFormFile file)
        {
            List<Wind_ProjectFan> list = new List<Wind_ProjectFan>();
            string msg = "";
            try
            {
                if (file != null)
                {
                    using (var uow = _techCenterUowFactory.Create())
                    {
                        var Wind_ProjectFan_repo = uow.GetRepository<Wind_ProjectFan>();

                        //保存文件到服务器
                        string saveDir = Path.Combine(_env.WebRootPath, "File", "Project", "FanPosition");
                        string FileName = Path.Combine(saveDir, Guid.NewGuid() + Path.GetExtension(file.FileName));
                        FileUtils.SaveFile(file, FileName);

                        //excel文件
                        DataSet dt = ExcelUtils.ConvertExcelFileToDataSet(file);

                        if (dt == null || dt.Tables == null || dt.Tables[0] == null)
                        {
                            msg = "表格数据为空";
                            return (list, msg);
                        }

                        //软删除旧数据
                        Wind_ProjectFan_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList()
                            .ForEach(a =>
                            {
                                a.IsDelete = true;
                                a.CreateTime = DateTime.UtcNow;
                            });

                        foreach (DataRow dr in dt.Tables[0].Rows)
                        {
                            Wind_ProjectFan fan = new Wind_ProjectFan();
                            fan.ProjectID = request.ProjectID;
                            fan.FanName = dr[0].ToString().Trim();
                            fan.Lon = dr[1].ToString().Trim();
                            fan.Lat = dr[2].ToString().Trim();
                            fan.CreateTime = DateTime.UtcNow;
                            fan.IsDelete = false;
                            fan.Status = (int)CloudWindFanStatus.在安装;
                            list.Add(fan);
                        }

                        Wind_ProjectFan_repo.AddList(list);
                        await uow.SaveAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                list = new List<Wind_ProjectFan>();
                msg = "发生错误";
            }
            return (list, msg);
        }

        public async Task<(List<CloudProjectFile> data, string msg)> ProjectFileInfoUpload(CloudWindRequest request, List<IFormFile> Files)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            string msg = "";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var Wind_Project_repo = uow.GetRepository<Wind_Project>();
                    var Wind_ProjectFile_repo = uow.GetRepository<Wind_ProjectFile>();

                    string FileFolderPath = "";
                    if (request.FlowHandle.Equals("new"))
                    {
                        FileFolderPath = Path.Combine(_env.WebRootPath, "File", "Project", "TempFile");
                    }
                    else
                    {
                        var Project = Wind_Project_repo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                        if (Project == null)
                        {
                            msg = "项目数据错误";
                            return (list, msg);
                        }
                        if (Project.ProjectCode.Equals("未生成"))
                        {
                            FileFolderPath = Path.Combine(_env.WebRootPath, "File", "Project", $"TempProject\\Project{Project.ID}", "Import");
                        }
                        else
                        {
                            FileFolderPath = Path.Combine(_env.WebRootPath, "File", "Project", Project.ProjectCode.ToUpper(), "Import");
                        }
                    }

                    //确保目录存在
                    List<Wind_ProjectFile> FileList = new List<Wind_ProjectFile>();
                    foreach (var File in Files)
                    {
                        if (File != null)
                        {
                            Wind_ProjectFile model = new Wind_ProjectFile();
                            model.ProjectID = request.ProjectID;
                            model.FileName = File.FileName;
                            model.FilePath = Path.Combine(FileFolderPath, Guid.NewGuid() + Path.GetExtension(File.FileName));
                            model.CreateTime = DateTime.UtcNow;
                            model.IsDelete = false;
                            FileList.Add(model);

                            //保存文件到服务器
                            FileUtils.SaveFile(File, model.FilePath);
                        }
                    }

                    Wind_ProjectFile_repo.AddList(FileList);
                    await uow.SaveAsync();

                    //返回文件列表
                    Wind_ProjectFile_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList()
                        .ForEach(a =>
                        {
                            CloudProjectFile model = new CloudProjectFile();
                            model.ID = a.ID;
                            model.FileName = a.FileName;
                            model.FilePath = a.FilePath.Substring(a.FilePath.IndexOf(@"\File\"));
                            model.FileTime = DateTime.UtcNow.ToString("yyyy-MM-dd");
                            list.Add(model);
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                list = new List<CloudProjectFile>();
                msg = "发生错误";
            }
            return (list, msg);
        }

        public async Task<(List<CloudProjectFile> data, string msg)> ProjectDKFileInfoUpload(CloudWindRequest request, List<IFormFile> Files)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            string msg = "";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var Wind_Project_repo = uow.GetRepository<Wind_Project>();
                    var Library_Geology_repo = uow.GetRepository<Library_Geology>();
                    var Library_Geology_DK_repo = uow.GetRepository<Library_Geology_DK>();
                    var Library_Geology_Data_repo = uow.GetRepository<Library_Geology_Data>();

                    string FileFolderPath = "";
                    if (request.FlowHandle.Equals("new"))
                    {
                        FileFolderPath = Path.Combine(_env.WebRootPath, "File", "Project", "TempFile");
                    }
                    else
                    {
                        var Project = Wind_Project_repo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                        if (Project == null)
                        {
                            msg = "项目数据错误";
                            return (list, msg);
                        }
                        if (Project.ProjectCode.Equals("未生成"))
                        {
                            FileFolderPath = Path.Combine(_env.WebRootPath, "File", "Project", $"TempProject\\Project{Project.ID}", "Import");
                        }
                        else
                        {
                            FileFolderPath = Path.Combine(_env.WebRootPath, "File", "Project", Project.ProjectCode.ToUpper(), "Import");
                        }
                    }

                    //如果是地勘数据表，要删除原来的数据
                    if (request.FileType == 2)
                    {
                        Library_Geology_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID && a.Type == request.FileType).ToList()
                            .ForEach(a =>
                            {
                                a.IsDelete = true;
                                a.CreateTime = DateTime.UtcNow;
                            });

                        Library_Geology_DK_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList()
                            .ForEach(a =>
                            {
                                a.IsDelete = true;
                                a.CreateTime = DateTime.UtcNow;
                            });
                    }

                    List<Library_Geology> GeologyList = new List<Library_Geology>();

                    foreach (var File in Files)
                    {
                        if (File != null)
                        {
                            Library_Geology model = new Library_Geology();
                            model.ProjectID = request.ProjectID;
                            model.Type = request.FileType;
                            model.FileName = File.FileName;
                            model.FilePath = Path.Combine(FileFolderPath, Guid.NewGuid() + Path.GetExtension(File.FileName));
                            model.CreateTime = DateTime.UtcNow;
                            model.IsDelete = false;
                            GeologyList.Add(model);

                            //保存文件到服务器
                            FileUtils.SaveFile(File, model.FilePath);
                        }
                    }

                    Library_Geology_repo.AddList(GeologyList);
                    await uow.SaveAsync();

                    //保存地勘数据表的数据
                    if (request.FileType == (int)CloudWindTaskFileType.风电项目_机孔位地勘数据表)
                    {
                        Library_Geology_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID && a.Type == (int)CloudWindTaskFileType.风电项目_机孔位地勘数据表).ToList()
                        .ForEach(a =>
                        {
                            if (!string.IsNullOrEmpty(a.FilePath))
                            {
                                DataSet ds = ExcelUtils.ReadExcel(a.FilePath);

                                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                                {
                                    foreach (DataTable dt in ds.Tables)
                                    {
                                        Library_Geology_DK dk = new Library_Geology_DK();
                                        dk.DKName = dt.TableName;
                                        dk.FanID = null;
                                        dk.ProjectID = request.ProjectID;
                                        dk.CreateTime = DateTime.UtcNow;
                                        dk.IsDelete = false;
                                        Library_Geology_DK_repo.Add(dk);
                                        uow.SaveAsync().Wait();

                                        //获取主键
                                        var NewID = Library_Geology_DK_repo.Find(b => !b.IsDelete && b.ProjectID == request.ProjectID).OrderByDescending(b => b.ID).FirstOrDefault().ID;

                                        List<Library_Geology_Data> DataList = new List<Library_Geology_Data>();
                                        foreach (DataRow dr in dt.Rows)
                                        {
                                            Library_Geology_Data data = new Library_Geology_Data();
                                            data.DKID = NewID;
                                            data.xh = dr[0].ToString();
                                            data.dcbh = dr[1].ToString();
                                            data.tcbh = dr[2].ToString();
                                            data.cdbg = dr[3].ToString();
                                            data.tclx = dr[4].ToString();
                                            data.bpskjqd = dr[5].ToString();
                                            data.stmcj = dr[6].ToString();
                                            data.yxzd = dr[7].ToString();
                                            data.bgjs = dr[8].ToString();
                                            data.CreateTime = DateTime.UtcNow;
                                            data.IsDelete = false;
                                            DataList.Add(data);
                                        }

                                        Library_Geology_Data_repo.AddList(DataList);
                                        uow.SaveAsync().Wait();
                                    }
                                }
                            }
                        });
                    }

                    //返回文件列表
                    Library_Geology_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID && a.Type == request.FileType).ToList()
                        .ForEach(a =>
                        {
                            CloudProjectFile model = new CloudProjectFile();
                            model.ID = a.ID;
                            model.FileName = a.FileName;
                            model.FileTime = DateTime.UtcNow.ToString("yyyy-MM-dd");
                            model.FilePath = a.FilePath.Substring(a.FilePath.IndexOf(@"\File\"));
                            list.Add(model);
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                list = new List<CloudProjectFile>();
                msg = "发生错误,请检查地勘数据表格式。";
            }
            return (list, msg);
        }

        public async Task<string> ProjectSubmit(CloudWindRequest request)
        {
            string msg = "";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var Flow_NodeManageUser_repo = uow.GetRepository<Flow_NodeManageUser>();
                    var Wind_Project_repo = uow.GetRepository<Wind_Project>();
                    var Wind_ProjectInfo_repo = uow.GetRepository<Wind_ProjectInfo>();
                    var Wind_ProjectContacter_repo = uow.GetRepository<Wind_ProjectContacter>();
                    var Wind_ProjectRole_repo = uow.GetRepository<Wind_ProjectRole>();
                    var Wind_ProjectFile_repo = uow.GetRepository<Wind_ProjectFile>();
                    var Library_Geology_repo = uow.GetRepository<Library_Geology>();
                    var Library_Geology_DK_repo = uow.GetRepository<Library_Geology_DK>();
                    var Wind_ProjectArea_repo = uow.GetRepository<Wind_ProjectArea>();
                    var Wind_ProjectFan_repo = uow.GetRepository<Wind_ProjectFan>();
                    var Flow_ProjectApply_repo = uow.GetRepository<Flow_ProjectApply>();

                    var NodeUsers = Flow_NodeManageUser_repo.Find(a => !a.IsDelete && a.NodeID == (int)CloudWindProjectFlowNode.技术中心云平台负责人审批).ToList();
                    if (NodeUsers == null || NodeUsers.Count == 0)
                    {
                        msg = "没有配置 项目立项负责人审批人员，请联系管理员";
                        return msg;
                    }

                    DateTime now = DateTime.UtcNow;

                    //1.生成风场项目
                    var NewWindProject = new Wind_Project();
                    NewWindProject.ProjectCode = "未生成";
                    NewWindProject.ProjectCodeIndex = CreateProjectCode();
                    NewWindProject.ProjectName = request.ProjectName;
                    NewWindProject.Lon = "";
                    NewWindProject.Lat = "";
                    NewWindProject.FlowStatus = (int)CloudWindProjectFlowStatus.审批中;
                    NewWindProject.ProjectStartTime = Convert.ToDateTime(request.ProjectStartTime);
                    NewWindProject.ProjectEndTime = Convert.ToDateTime(request.ProjectEndTime);
                    NewWindProject.CompanyID = request.CompanyID;
                    NewWindProject.Status = request.Status;
                    NewWindProject.CreateTime = now;
                    NewWindProject.IsDelete = false;
                    Wind_Project_repo.Add(NewWindProject);
                    await uow.SaveAsync();

                    //2.获取风场主键
                    var NewProjectID = Wind_Project_repo.Find(a => !a.IsDelete).OrderByDescending(a => a.ID).FirstOrDefault().ID;

                    //项目基本信息
                    var NewProjectInfo = new Wind_ProjectInfo();
                    NewProjectInfo.ProjectID = NewProjectID;
                    NewProjectInfo.WaterDepth = request.WaterDepth ?? "";
                    NewProjectInfo.WaterDepthMax = request.WaterDepthMax ?? "";
                    NewProjectInfo.WaterDepthMin = request.WaterDepthMin ?? "";
                    NewProjectInfo.CreateTime = now;
                    NewProjectInfo.IsDelete = false;
                    Wind_ProjectInfo_repo.Add(NewProjectInfo);

                    //创建文件夹(暂存临时文件夹)
                    string FileImportFolder = Path.Combine(_env.WebRootPath, "File", "Project", $"TempProject\\Project{NewProjectID}", "Import");
                    string FileExportFolder = Path.Combine(_env.WebRootPath, "File", "Project", $"TempProject\\Project{NewProjectID}", "Export");
                    Directory.CreateDirectory(FileImportFolder);
                    Directory.CreateDirectory(FileExportFolder);

                    //3.生成相关人员
                    var NewProjectContacter = new Wind_ProjectContacter();
                    NewProjectContacter.ProjectID = NewProjectID;
                    NewProjectContacter.Applyer = CurrentUser.RealName;
                    NewProjectContacter.ApplyerCode = CurrentUser.UserCode;
                    NewProjectContacter.ApplyerDepart = CurrentUser.DepartName;
                    NewProjectContacter.ApplyerPhone = CurrentUser.Mobile;
                    NewProjectContacter.ApplyerJobName = CurrentUser.JobName;
                    NewProjectContacter.Director = request.ProjectManagerName;
                    NewProjectContacter.DirectorCode = request.ProjectManagerUserCode;
                    NewProjectContacter.DirectorDepart = request.ProjectManagerDepart;
                    NewProjectContacter.DirectorPhone = request.ProjectManagerPhone;
                    NewProjectContacter.DirectorJobName = request.ProjectManagerJobName;
                    NewProjectContacter.CreateTime = now;
                    NewProjectContacter.IsDelete = false;
                    Wind_ProjectContacter_repo.Add(NewProjectContacter);

                    //4.项目组成员
                    if (!string.IsNullOrEmpty(request.ProjectGroupUsers))
                    {
                        List<CloudWindBackManageUserInfo> users = JsonUtils.Deserialize<List<CloudWindBackManageUserInfo>>(request.ProjectGroupUsers);
                        foreach (var user in users)
                        {
                            if (!string.IsNullOrEmpty(user.UserCode))
                            {
                                Wind_ProjectRole newRole = new Wind_ProjectRole();
                                newRole.ProjectID = NewProjectID;
                                newRole.RoleID = (int)CloudWindProjectRole.项目组成员;
                                newRole.UserName = user.UserName;
                                newRole.UserCode = user.UserCode;
                                newRole.UserDepartName = user.UserDepartName;
                                newRole.UserPhone = user.UserPhone;
                                newRole.UserJobName = user.UserJobName;
                                newRole.IsDelete = false;
                                newRole.CreateTime = now;
                                Wind_ProjectRole_repo.Add(newRole);
                            }
                        }
                    }

                    //7.流程相关(创建时，流程只发给项目总工，并且不能是多人审批环节)
                    //直接发给 "技术中心云平台负责人审批" 人员
                    NodeUsers.ForEach(a =>
                    {
                        var NewFlow = new Flow_ProjectApply();
                        NewFlow.ProjectID = NewProjectID;
                        NewFlow.NodeID = (int)CloudWindProjectFlowNode.技术中心云平台负责人审批;
                        NewFlow.NodeUserCode = a.ManageUserCode;
                        NewFlow.NodeUserName = a.ManageName;
                        NewFlow.NodeUserDepart = a.ManageDepart;
                        NewFlow.NodeUserPhone = a.ManagePhone;
                        NewFlow.NodeUserJobName = a.ManageJobName;
                        NewFlow.Comment = request.ProjectComment;
                        NewFlow.FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批;
                        NewFlow.FlowOrder = 1;
                        NewFlow.CreateTime = now;
                        NewFlow.IsDelete = false;
                        Flow_ProjectApply_repo.Add(NewFlow);
                    });

                    //8.将文件移动到所属文件夹
                    //（1）项目资料
                    Wind_ProjectFile_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList()
                        .ForEach(a =>
                        {
                            string FileName = Path.GetFileName(a.FilePath);
                            FileUtils.MoveFile(a.FilePath, Path.Combine(FileImportFolder, FileName));
                            a.ProjectID = NewProjectID;
                            a.FilePath = Path.Combine(FileImportFolder, FileName);
                        });

                    //（2）地勘资料
                    Library_Geology_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList()
                        .ForEach(a =>
                        {
                            string FileName = Path.GetFileName(a.FilePath);
                            FileUtils.MoveFile(a.FilePath, Path.Combine(FileImportFolder, FileName));
                            a.ProjectID = NewProjectID;
                            a.FilePath = Path.Combine(FileImportFolder, FileName);
                        });

                    Library_Geology_DK_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList()
                        .ForEach(a =>
                        {
                            a.ProjectID = NewProjectID;
                        });

                    //(3)风场区域坐标
                    decimal Lons = 0;
                    decimal Lats = 0;
                    int Count = 0;
                    Wind_ProjectArea_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList()
                        .ForEach(a =>
                        {
                            a.ProjectID = NewProjectID;
                            Lons += Convert.ToDecimal(a.AreaLon);
                            Lats += Convert.ToDecimal(a.AreaLat);
                            Count++;
                        });
                    if (Lons != 0 && Lats != 0)
                    {
                        NewWindProject.Lon = Math.Round(Lons / Count, 6).ToString();
                        NewWindProject.Lat = Math.Round(Lats / Count, 6).ToString();
                    }

                    //(4)风机坐标
                    Wind_ProjectFan_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList()
                        .ForEach(a =>
                        {
                            a.ProjectID = NewProjectID;
                        });

                    //8 交建通发消息通知
                    List<SystemJJTInform> informUsers = new List<SystemJJTInform>();
                    string CloudPlatUrl = AppSettingUtils.GetSetting("CloudPlatUrl");

                    // TODO: CloudCenterService 尚未迁移，需实现 CreateJJTMessageRedirectToken
                    NodeUsers.ForEach(a =>
                    {
                        var Token = _cloudCenterService.CreateJJTMessageRedirectToken("Project", a.ManageUserCode, NewProjectID);
                        var RedirectUrl = CloudPlatUrl + @"Home/JJTMessageRedirect?CloudToken=" + Token;
                        informUsers.Add(new SystemJJTInform()
                        {
                            UserCode = a.ManageUserCode,
                            Title = NewWindProject.ProjectName + "-立项流程审批",
                            Content = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情",
                            Url = RedirectUrl
                        });
                    });

                    //await Task.Run(() =>
                    //{
                    //    _cloudWindInfoService.SendJJTMessage(informUsers);
                    //});
                    _cloudWindInfoService.SendJJTMessage(informUsers);

                    await uow.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                msg = "发生错误";
            }
            return msg;
        }


        public async Task<string> ProjectApprovalSubmit(CloudWindRequest request)
        {
            string msg = "";
            try
            {
                DateTime now = DateTime.UtcNow;

                using (var uow = _techCenterUowFactory.Create())
                {
                    var Wind_Project_repo = uow.GetRepository<Wind_Project>();
                    var Flow_Node_repo = uow.GetRepository<Flow_Node>();
                    var Flow_ProjectApply_repo = uow.GetRepository<Flow_ProjectApply>();
                    var Wind_ProjectContacter_repo = uow.GetRepository<Wind_ProjectContacter>();
                    var Wind_ProjectRole_repo = uow.GetRepository<Wind_ProjectRole>();
                    var Wind_ProjectInfo_repo = uow.GetRepository<Wind_ProjectInfo>();
                    var Flow_NodeManageUser_repo = uow.GetRepository<Flow_NodeManageUser>();
                    var Manage_Copyer_repo = uow.GetRepository<Manage_Copyer>();
                    var Wind_Project_Copyer_repo = uow.GetRepository<Wind_Project_Copyer>();
                    var Wind_ProjectFile_repo = uow.GetRepository<Wind_ProjectFile>();

                    var Project = Wind_Project_repo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (Project == null)
                    {
                        msg = "项目数据错误";
                        return msg;
                    }

                    //获取流程信息
                    var FlowInfo = await GetProjectFlow(Project.ID, UserCode);
                    if (!FlowInfo.Success)
                    {
                        msg = FlowInfo.Message;
                        return msg;
                    }

                    var Node = Flow_Node_repo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.海上风电WebGIS平台 && a.ID == FlowInfo.NodeID).FirstOrDefault();

                    var ProjectFlow = Flow_ProjectApply_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList();

                    var LatestFlow = ProjectFlow.OrderByDescending(a => a.CreateTime).FirstOrDefault();
                    if (LatestFlow == null)
                    {
                        msg = "流程数据错误";
                        return msg;
                    }
                    var Flow = ProjectFlow.Where(a => a.NodeID == LatestFlow.NodeID && a.FlowOrder == LatestFlow.FlowOrder && a.NodeUserCode.Equals(UserCode)).FirstOrDefault();
                    if (Flow == null)
                    {
                        msg = "流程数据错误";
                        return msg;
                    }

                    //流程处理
                    var DoAddCopyer = false;
                    var DoAddInformer = true;
                    List<string> Informers = new List<string>();

                    // NextNode 需要在多个分支中使用，提前声明
                    Flow_Node NextNode = null;

                    if (request.ApprovalType == (int)CloudWindFlowHandle.审批退回)
                    {
                        Flow.FlowHandle = (int)CloudWindProjectFlowHandleStatus.退回;
                        Flow.ApprovalTime = now;
                        Flow.Comment = request.ProjectComment;

                        //多人审批时，只要一人退回，其他人不用审核(直接生成一条新流程)，退回至申请人
                        Project.FlowStatus = (int)CloudWindProjectFlowStatus.退回;
                        var Contacter = Wind_ProjectContacter_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).FirstOrDefault();
                        if (Contacter == null)
                        {
                            msg = "申请人数据错误";
                            return msg;
                        }
                        var NewFlow = new Flow_ProjectApply();
                        NewFlow.ProjectID = request.ProjectID;
                        NewFlow.NodeID = (int)CloudWindProjectFlowNode.业务人员立项申请;
                        NewFlow.FlowOrder = Flow.FlowOrder + 1;
                        NewFlow.NodeUserName = Contacter.Applyer ?? "";
                        NewFlow.NodeUserCode = Contacter.ApplyerCode ?? "";
                        NewFlow.NodeUserDepart = Contacter.ApplyerDepart ?? "";
                        NewFlow.NodeUserPhone = Contacter.ApplyerPhone ?? "";
                        NewFlow.NodeUserJobName = Contacter.ApplyerJobName ?? "";
                        NewFlow.FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批;
                        NewFlow.Comment = "";
                        NewFlow.CreateTime = now;
                        NewFlow.IsDelete = false;
                        Flow_ProjectApply_repo.Add(NewFlow);

                        Informers.Add(Contacter.ApplyerCode);
                    }
                    else if (request.ApprovalType == (int)CloudWindFlowHandle.审批通过)
                    {
                        //多人审批时，所有人通过后，才产生下一个流程节点
                        Flow.FlowHandle = (int)CloudWindProjectFlowHandleStatus.审批通过;
                        Flow.ApprovalTime = now;
                        Flow.Comment = request.ProjectComment ?? "";

                        Project.FlowStatus = (int)CloudWindProjectFlowStatus.审批中;

                        //下一流程节点
                        NextNode = Flow_Node_repo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.海上风电WebGIS平台 && a.NodeNo == (Node.NodeNo + 1)).FirstOrDefault();

                        if (NextNode == null)
                        {
                            if (string.IsNullOrEmpty(request.ProjectCode) || request.ProjectCode.Length != 6)
                            {
                                msg = "项目编号必须是以2个数字开头，再加上4位数字";
                                return msg;
                            }

                            var ExistProject = Wind_Project_repo.Find(a => !a.IsDelete && a.ProjectCode.Equals(request.ProjectCode.Trim())).FirstOrDefault();
                            if (ExistProject != null)
                            {
                                msg = "已经存在 " + request.ProjectCode.Trim() + " 项目编号，请检查。";
                                return msg;
                            }

                            string ProjectFolderPath = Path.Combine(_env.WebRootPath, "File", "Project", request.ProjectCode.Trim());
                            if (Directory.Exists(ProjectFolderPath))
                            {
                                msg = "云服务器上已经存在 " + request.ProjectCode.Trim() + " 文件夹，请联系管理员检查。";
                                return msg;
                            }
                        }

                        //下一节点人员
                        var NodeManageUsers = Flow_NodeManageUser_repo.Find(a => !a.IsDelete && a.NodeID == NextNode.ID).ToList();

                        //同节点其他Flow
                        var ThisFlows = ProjectFlow.Where(a => a.NodeID == Flow.NodeID && a.FlowOrder == Flow.FlowOrder && a.ID != Flow.ID && a.FlowHandle == (int)CloudWindProjectFlowHandleStatus.待审批).ToList();

                        //上一节点
                        var LastFlow = ProjectFlow.Where(a => a.FlowOrder == (Flow.FlowOrder - 1)).ToList();

                        if (LastFlow != null && LastFlow.Exists(a => a.FlowHandle == (int)CloudWindProjectFlowHandleStatus.退回))
                        {
                            //退回重新填写的流程
                            if (string.IsNullOrEmpty(request.ProjectManagerUserCode))
                            {
                                msg = "项目负责人信息不能空";
                                return msg;
                            }
                            //项目负责人
                            var Contacter = Wind_ProjectContacter_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).FirstOrDefault();
                            if (Contacter != null)
                            {
                                Contacter.Director = request.ProjectManagerName ?? "";
                                Contacter.DirectorCode = request.ProjectManagerUserCode ?? "";
                                Contacter.DirectorDepart = request.ProjectManagerDepart ?? "";
                                Contacter.DirectorPhone = request.ProjectManagerPhone ?? "";
                                Contacter.DirectorJobName = request.ProjectManagerJobName ?? "";
                            }

                            //项目组成员
                            Wind_ProjectRole_repo.Find(a => !a.IsDelete && a.RoleID == (int)CloudWindProjectRole.项目组成员).ToList()
                                .ForEach(a =>
                                {
                                    a.IsDelete = true;
                                });

                            if (!string.IsNullOrEmpty(request.ProjectGroupUsers))
                            {
                                List<CloudWindBackManageUserInfo> users = JsonUtils.Deserialize<List<CloudWindBackManageUserInfo>>(request.ProjectGroupUsers);
                                foreach (var user in users)
                                {
                                    if (!string.IsNullOrEmpty(user.UserCode))
                                    {
                                        Wind_ProjectRole newRole = new Wind_ProjectRole();
                                        newRole.ProjectID = Project.ID;
                                        newRole.RoleID = (int)CloudWindProjectRole.项目组成员;
                                        newRole.UserName = user.UserName;
                                        newRole.UserCode = user.UserCode;
                                        newRole.UserDepartName = user.UserDepartName;
                                        newRole.UserPhone = user.UserPhone;
                                        newRole.UserJobName = user.UserJobName;
                                        newRole.IsDelete = false;
                                        newRole.CreateTime = now;
                                        Wind_ProjectRole_repo.Add(newRole);
                                    }
                                }
                            }

                            //项目期限
                            if (string.IsNullOrEmpty(request.ProjectStartTime) || string.IsNullOrEmpty(request.ProjectEndTime))
                            {
                                msg = "项目期限不能为空";
                                return msg;
                            }
                            Project.ProjectName = request.ProjectName;
                            Project.ProjectStartTime = Convert.ToDateTime(request.ProjectStartTime);
                            Project.ProjectEndTime = Convert.ToDateTime(request.ProjectEndTime);
                            Project.CompanyID = request.CompanyID;
                            Project.Status = request.Status;
                            var ProjectInfo = Wind_ProjectInfo_repo.Find(a => !a.IsDelete.Value && a.ProjectID == request.ProjectID).FirstOrDefault();
                            if (ProjectInfo == null)
                            {
                                msg = "项目基本信息错误";
                                return msg;
                            }
                            ProjectInfo.WaterDepthMin = request.WaterDepthMin ?? "";
                            ProjectInfo.WaterDepthMax = request.WaterDepthMax ?? "";

                            var ApprovalManageUsers = Flow_NodeManageUser_repo.Find(a => !a.IsDelete && a.NodeID == (int)CloudWindProjectFlowNode.技术中心云平台负责人审批).ToList();

                            //新增流程(初始只能发给项目经理)(2024-12-2 修改  只能发给技术中心管理人员)
                            ApprovalManageUsers.ForEach(a =>
                            {
                                var NewFlow = new Flow_ProjectApply();
                                NewFlow.ProjectID = request.ProjectID;
                                NewFlow.NodeID = NextNode.ID;
                                NewFlow.FlowOrder = Flow.FlowOrder + 1;
                                NewFlow.NodeUserName = a.ManageName;
                                NewFlow.NodeUserCode = a.ManageUserCode;
                                NewFlow.NodeUserDepart = a.ManageDepart;
                                NewFlow.NodeUserPhone = a.ManagePhone;
                                NewFlow.NodeUserJobName = a.ManageJobName;
                                NewFlow.FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批;
                                NewFlow.Comment = "";
                                NewFlow.CreateTime = now;
                                NewFlow.IsDelete = false;
                                Flow_ProjectApply_repo.Add(NewFlow);

                                Informers.Add(a.ManageUserCode);
                            });
                        }
                        else
                        {
                            //判断节点是否单人审批通过类型
                            if (FlowInfo.NodeApprovalType)
                            {
                                if (NextNode == null)
                                {
                                    //流程结束
                                    Project.FlowStatus = (int)CloudWindProjectFlowStatus.激活;
                                    Project.ProjectCode = request.ProjectCode.Trim();
                                    DoAddCopyer = true;
                                }
                                else
                                {
                                    Project.FlowStatus = (int)CloudWindProjectFlowStatus.审批中;

                                    if ((NodeManageUsers == null || NodeManageUsers.Count == 0) && string.IsNullOrEmpty(request.ProjectNodeUserCode))
                                    {
                                        msg = "没有设置节点人员、或者下一节点(" + NextNode.NodeName + ")配置错误，请联系技术中心管理员";
                                        return msg;
                                    }

                                    //如果本节点是多人审批，下节点必须配置人员，否则报错
                                    if (ThisFlows.Count > 0)
                                    {
                                        if (NodeManageUsers == null || NodeManageUsers.Count == 0)
                                        {
                                            msg = "多人审批环节的下一节点，必须配置默认审批人员,请联系管理员配置";
                                            return msg;
                                        }
                                    }

                                    if (NodeManageUsers != null && NodeManageUsers.Count > 0)
                                    {
                                        NodeManageUsers.ForEach(a =>
                                        {
                                            var NewFlow = new Flow_ProjectApply();
                                            NewFlow.ProjectID = request.ProjectID;
                                            NewFlow.NodeID = NextNode.ID;
                                            NewFlow.FlowOrder = Flow.FlowOrder + 1;
                                            NewFlow.NodeUserName = a.ManageName ?? "";
                                            NewFlow.NodeUserCode = a.ManageUserCode ?? "";
                                            NewFlow.NodeUserDepart = a.ManageDepart ?? "";
                                            NewFlow.NodeUserPhone = a.ManagePhone ?? "";
                                            NewFlow.NodeUserJobName = a.ManageJobName ?? "";
                                            NewFlow.FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批;
                                            NewFlow.Comment = "";
                                            NewFlow.CreateTime = now;
                                            NewFlow.IsDelete = false;
                                            Flow_ProjectApply_repo.Add(NewFlow);

                                            Informers.Add(a.ManageUserCode);
                                        });
                                    }
                                    else
                                    {
                                        //新增流程
                                        var NewFlow = new Flow_ProjectApply();
                                        NewFlow.ProjectID = request.ProjectID;
                                        NewFlow.NodeID = NextNode.ID;
                                        NewFlow.FlowOrder = Flow.FlowOrder + 1;
                                        NewFlow.NodeUserName = request.ProjectNodeName ?? "";
                                        NewFlow.NodeUserCode = request.ProjectNodeUserCode ?? "";
                                        NewFlow.NodeUserDepart = request.ProjectNodeDepart ?? "";
                                        NewFlow.NodeUserPhone = request.ProjectNodePhone ?? "";
                                        NewFlow.NodeUserJobName = request.ProjectNodeJobName ?? "";
                                        NewFlow.FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批;
                                        NewFlow.Comment = "";
                                        NewFlow.CreateTime = now;
                                        NewFlow.IsDelete = false;
                                        Flow_ProjectApply_repo.Add(NewFlow);

                                        Informers.Add(request.ProjectNodeUserCode);
                                    }
                                }

                                //同节点其他流程删除
                                if (ThisFlows.Count > 0)
                                {
                                    ThisFlows.ForEach(a =>
                                    {
                                        a.IsDelete = true;
                                    });
                                }
                            }
                            else
                            {
                                if (ThisFlows.Count == 0) //如果没有其他同节点流程
                                {
                                    if (NextNode == null)
                                    {
                                        //流程结束
                                        Project.FlowStatus = (int)CloudWindProjectFlowStatus.激活;
                                        Project.ProjectCode = request.ProjectCode.Trim();
                                        DoAddCopyer = true;
                                    }
                                    else
                                    {
                                        if ((NodeManageUsers == null || NodeManageUsers.Count == 0) && string.IsNullOrEmpty(request.ProjectNodeUserCode))
                                        {
                                            msg = "没有设置节点人员、或者下一节点(" + NextNode.NodeName + ")配置错误，请联系技术中心管理员";
                                            return msg;
                                        }

                                        Project.FlowStatus = (int)CloudWindProjectFlowStatus.审批中;

                                        //如果下一节点有配置，则必须发给此用户（可能是多节点）
                                        if (NodeManageUsers != null && NodeManageUsers.Count > 0)
                                        {
                                            NodeManageUsers.ForEach(a =>
                                            {
                                                var NewFlow = new Flow_ProjectApply();
                                                NewFlow.ProjectID = request.ProjectID;
                                                NewFlow.NodeID = NextNode.ID;
                                                NewFlow.FlowOrder = Flow.FlowOrder + 1;
                                                NewFlow.NodeUserName = a.ManageName ?? "";
                                                NewFlow.NodeUserCode = a.ManageUserCode ?? "";
                                                NewFlow.NodeUserDepart = a.ManageDepart ?? "";
                                                NewFlow.NodeUserPhone = a.ManagePhone ?? "";
                                                NewFlow.NodeUserJobName = a.ManageJobName ?? "";
                                                NewFlow.FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批;
                                                NewFlow.Comment = "";
                                                NewFlow.CreateTime = now;
                                                NewFlow.IsDelete = false;
                                                Flow_ProjectApply_repo.Add(NewFlow);

                                                Informers.Add(a.ManageUserCode);
                                            });
                                        }
                                        else
                                        {
                                            //新增流程
                                            var NewFlow = new Flow_ProjectApply();
                                            NewFlow.ProjectID = request.ProjectID;
                                            NewFlow.NodeID = NextNode.ID;
                                            NewFlow.FlowOrder = Flow.FlowOrder + 1;
                                            NewFlow.NodeUserName = request.ProjectNodeName ?? "";
                                            NewFlow.NodeUserCode = request.ProjectNodeUserCode ?? "";
                                            NewFlow.NodeUserDepart = request.ProjectNodeDepart ?? "";
                                            NewFlow.NodeUserPhone = request.ProjectNodePhone ?? "";
                                            NewFlow.NodeUserJobName = request.ProjectNodeJobName ?? "";
                                            NewFlow.FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批;
                                            NewFlow.Comment = "";
                                            NewFlow.CreateTime = now;
                                            NewFlow.IsDelete = false;
                                            Flow_ProjectApply_repo.Add(NewFlow);

                                            Informers.Add(request.ProjectNodeUserCode);
                                        }
                                    }
                                }
                                else
                                {
                                    //还有其他同节点流程，不处理
                                }
                            }
                        }
                    }

                    //交建通发送通知(审批任务)
                    if (DoAddInformer)
                    {
                        string CloudPlatUrl = AppSettingUtils.GetSetting("SoftwareApi:CloudPlatUrl");

                        var InformTitle = Project.ProjectName + "-立项流程审批";
                        if (DoAddCopyer)
                        {
                            //流程结束,要通知申请人和项目经理
                            InformTitle = Project.ProjectName + "(" + Project.ProjectCode + ")-立项成功";
                            var Contacter = Wind_ProjectContacter_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).FirstOrDefault();
                            if (Contacter == null)
                            {
                                msg = "申请人数据错误";
                                return msg;
                            }
                            Informers.Add(Contacter.ApplyerCode);
                            Informers.Add(Contacter.DirectorCode);
                        }

                        List<SystemJJTInform> InformRequests = new List<SystemJJTInform>();
                        foreach (string userCode in Informers)
                        {
                            var Token = _cloudCenterService.CreateJJTMessageRedirectToken("Project", userCode, Project.ID);
                            var RedirectUrl = CloudPlatUrl + @"Home/JJTMessageRedirect?CloudToken=" + Token;

                            SystemJJTInform InformRequest = new SystemJJTInform();
                            InformRequest.UserCode = userCode;
                            InformRequest.Url = RedirectUrl;
                            InformRequest.Title = InformTitle;
                            InformRequest.Content = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情";
                            InformRequests.Add(InformRequest);
                        }

                        await _cloudWindInfoService.SendJJTMessage(InformRequests);
                    }

                    //抄送
                    if (DoAddCopyer)
                    {
                        string CloudPlatUrl = AppSettingUtils.GetSetting("SoftwareApi:CloudPlatUrl");
                        List<SystemJJTInform> InformRequests = new List<SystemJJTInform>();

                        Manage_Copyer_repo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.海上风电WebGIS平台).ToList()
                            .ForEach(a =>
                            {
                                if (!string.IsNullOrEmpty(a.UserCode))
                                {
                                    var NewCopyer = new Wind_Project_Copyer();
                                    NewCopyer.ProjectID = Project.ID;
                                    NewCopyer.UserCode = a.UserCode;
                                    NewCopyer.UserName = a.UserName;
                                    NewCopyer.UserDepart = a.UserDepart;
                                    NewCopyer.UserPhone = a.UserPhone;
                                    NewCopyer.UserJobName = a.UserJobName;
                                    NewCopyer.CreateTime = now;
                                    NewCopyer.IsDelete = false;
                                    Wind_Project_Copyer_repo.Add(NewCopyer);

                                    var Token = _cloudCenterService.CreateJJTMessageRedirectToken("Project", a.UserCode, Project.ID);
                                    var RedirectUrl = CloudPlatUrl + @"Home/JJTMessageRedirect?CloudToken=" + Token;

                                    SystemJJTInform InformRequest = new SystemJJTInform();
                                    InformRequest.UserCode = a.UserCode;
                                    InformRequest.Url = RedirectUrl;
                                    InformRequest.Title = Project.ProjectName + "(" + Project.ProjectCode + ")-立项成功";
                                    InformRequest.Content = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情";
                                    InformRequests.Add(InformRequest);
                                }
                            });

                        await _cloudWindInfoService.SendJJTMessage(InformRequests);

                        //立项成功后生成新的文件夹
                        var OldProjectFolder = Path.Combine(_env.WebRootPath, "File", "Project", "TempProject", "Project" + Project.ID);
                        var NewProjectFolder = Path.Combine(_env.WebRootPath, "File", "Project", Project.ProjectCode);
                        Directory.CreateDirectory(NewProjectFolder);
                        FileUtils.MoveDirectory(OldProjectFolder, NewProjectFolder);
                        Wind_ProjectFile_repo.Find(a => !a.IsDelete && a.ProjectID == Project.ID).ToList()
                            .ForEach(a =>
                            {
                                a.FilePath = a.FilePath.Replace(OldProjectFolder, NewProjectFolder);
                            });
                    }

                    await uow.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                msg = "发生错误";
            }
            return msg;
        }

        public async Task<(List<CloudWindFlowHistory> list, string msg)> QueryFlowHistory(CloudWindRequest request)
        {
            List<CloudWindFlowHistory> list = new List<CloudWindFlowHistory>();
            string msg = "";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var Wind_Project_repo = uow.GetRepository<Wind_Project>();
                    var Wind_ProjectContacter_repo = uow.GetRepository<Wind_ProjectContacter>();
                    var Flow_Node_repo = uow.GetRepository<Flow_Node>();
                    var Flow_ProjectApply_repo = uow.GetRepository<Flow_ProjectApply>();
                    var Wind_Project_Copyer_repo = uow.GetRepository<Wind_Project_Copyer>();

                    var Project = Wind_Project_repo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (Project == null)
                    {
                        msg = "项目数据错误";
                        return (new List<CloudWindFlowHistory>(), msg);
                    }

                    var Contactor = Wind_ProjectContacter_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).FirstOrDefault();
                    var Nodes = Flow_Node_repo.Find(a => true).ToList();
                    var ProjectFlow = Flow_ProjectApply_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList();

                    //1.发起
                    CloudWindFlowHistory FirstModel = new CloudWindFlowHistory();
                    FirstModel.FlowType = 0;
                    FirstModel.FlowHandleName = "申请";
                    FirstModel.ApprovalType = 1;
                    FirstModel.NodeName = Nodes.FirstOrDefault(a => a.ID == (int)CloudWindProjectFlowNode.业务人员立项申请).NodeName;
                    FirstModel.FlowTime = Project.CreateTime.Value.ToString("yyyy-MM-dd HH:mm");
                    FirstModel.Flow.Add(new CloudWindFlowHistoryInfo()
                    {
                        NodeUserName = Contactor.Applyer ?? "",
                        NodeUserJobName = Contactor.ApplyerJobName ?? "",
                        NodeUserDepart = Contactor.ApplyerDepart ?? "",
                        Comment = ""
                    });
                    list.Add(FirstModel);

                    //2.流程
                    ProjectFlow.OrderBy(a => a.FlowOrder).GroupBy(a => a.FlowOrder).ToList()
                        .ForEach(a =>
                        {
                            int FlowOrder = a.FirstOrDefault().FlowOrder.Value;
                            var ThisFlows = ProjectFlow.Where(b => b.FlowOrder == FlowOrder).ToList();

                            CloudWindFlowHistory model = new CloudWindFlowHistory();
                            model.FlowType = 1;
                            model.NodeName = Nodes.FirstOrDefault(b => b.ID == a.FirstOrDefault().NodeID).NodeName;
                            model.ApprovalType = ThisFlows.Count == 1 ? 1 : 2;
                            model.FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批;
                            model.FlowHandleName = CloudWindProjectFlowHandleStatus.待审批.ToString();
                            if (ThisFlows.Exists(b => b.FlowHandle == (int)CloudWindProjectFlowHandleStatus.退回))
                            {
                                model.FlowHandle = (int)CloudWindProjectFlowHandleStatus.退回;
                                model.FlowHandleName = CloudWindProjectFlowHandleStatus.退回.ToString();
                            }
                            else if (ThisFlows.Count(b => b.FlowHandle == (int)CloudWindProjectFlowHandleStatus.审批通过) == ThisFlows.Count)
                            {
                                model.FlowHandle = (int)CloudWindProjectFlowHandleStatus.审批通过;
                                model.FlowHandleName = CloudWindProjectFlowHandleStatus.审批通过.ToString();
                            }
                            ThisFlows.ForEach(b =>
                            {
                                model.Flow.Add(new CloudWindFlowHistoryInfo()
                                {
                                    NodeUserName = b.NodeUserName ?? "",
                                    NodeUserDepart = b.NodeUserDepart ?? "",
                                    NodeUserJobName = b.NodeUserJobName ?? "",
                                    Comment = b.Comment ?? "",
                                    FlowHandle = b.FlowHandle.Value,
                                    FlowHandleName = Enum.GetName(typeof(CloudWindProjectFlowHandleStatus), b.FlowHandle),
                                    FlowTime = b.ApprovalTime == null ? "" : b.ApprovalTime.Value.ToString("yyyy-MM-dd HH:mm")
                                });
                            });

                            list.Add(model);
                        });

                    //3.结束(加上抄送)
                    if (Project.FlowStatus == (int)CloudWindProjectFlowStatus.激活)
                    {
                        CloudWindFlowHistory EndModel = new CloudWindFlowHistory();
                        EndModel.FlowType = 2;
                        EndModel.FlowHandleName = "结束";
                        EndModel.FlowTime = "";
                        var ProjectCopyer = Wind_Project_Copyer_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList();
                        if (ProjectCopyer != null && ProjectCopyer.Count > 0)
                        {
                            ProjectCopyer.ForEach(a =>
                            {
                                if (!string.IsNullOrEmpty(a.UserName))
                                {
                                    EndModel.Flow.Add(new CloudWindFlowHistoryInfo()
                                    {
                                        NodeUserName = a.UserName,
                                        NodeUserDepart = a.UserDepart ?? "",
                                        NodeUserJobName = a.UserJobName ?? "",
                                    });
                                }
                            });

                            EndModel.FlowTime = Convert.ToDateTime(ProjectCopyer[0].CreateTime).ToString("yyyy-MM-dd HH:mm");
                        }

                        list.Add(EndModel);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                msg = "发生错误";
                list = new List<CloudWindFlowHistory>();
            }
            return (list, msg);
        }

        public async Task<(CloudWindDKData data, string msg)> QueryDKData(CloudWindRequest request)
        {
            CloudWindDKData data = new CloudWindDKData();
            string msg = "";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var Wind_ProjectFan_repo = uow.GetRepository<Wind_ProjectFan>();
                    var Library_Geology_DK_repo = uow.GetRepository<Library_Geology_DK>();

                    var Fans = Wind_ProjectFan_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList();
                    var Dks = Library_Geology_DK_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList();

                    Fans.ForEach(a =>
                    {
                        CloudWindDKFan fan = new CloudWindDKFan();
                        fan.ID = a.ID;
                        fan.FanName = a.FanName;
                        Dks.Where(b => b.FanID != null && b.FanID == a.ID).ToList().ForEach(b =>
                        {
                            fan.DKs.Add(new CloudWindDKModel()
                            {
                                ID = b.ID,
                                DKName = b.DKName,
                                FanID = b.FanID.Value
                            });
                        });
                        data.Fans.Add(fan);
                    });

                    Dks.ForEach(a =>
                    {
                        CloudWindDKModel model = new CloudWindDKModel();
                        model.ID = a.ID;
                        model.DKName = a.DKName;
                        model.FanID = a.FanID ?? 0;
                        model.IsChecked = a.FanID == null ? false : true;

                        data.DKs.Add(model);
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                msg = "发生错误";
                data = new CloudWindDKData();
            }
            return (data, msg);
        }

        public async Task<string> SaveDKMatch(CloudWindRequest request)
        {
            string msg = "";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var Wind_ProjectFan_repo = uow.GetRepository<Wind_ProjectFan>();
                    var Library_Geology_DK_repo = uow.GetRepository<Library_Geology_DK>();

                    var Fans = Wind_ProjectFan_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList();
                    var Dks = Library_Geology_DK_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList();

                    Dks.ForEach(a =>
                    {
                        a.CreateTime = DateTime.UtcNow;
                        a.FanID = null;
                    });

                    if (request.Fans != null && request.Fans.Count > 0)
                    {
                        foreach (var item in request.Fans)
                        {
                            var fan = Fans.FirstOrDefault(a => a.ID == item.ID);
                            if (fan != null)
                            {
                                foreach (var item2 in item.DKs)
                                {
                                    var dk = Dks.FirstOrDefault(b => b.ID == item2.ID);
                                    if (dk != null)
                                    {
                                        dk.FanID = item.ID;
                                    }
                                }
                            }
                        }
                    }

                    await uow.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                msg = "发生错误";
            }
            return msg;
        }

        public async Task<string> ProjectEditSubmit(CloudWindRequest request)
        {
            string msg = "";
            try
            {
                var Admins = await GetAdminUserCode();

                using (var uow = _techCenterUowFactory.Create())
                {
                    var Wind_ProjectRole_repo = uow.GetRepository<Wind_ProjectRole>();
                    var Wind_ProjectContacter_repo = uow.GetRepository<Wind_ProjectContacter>();
                    var Wind_Project_repo = uow.GetRepository<Wind_Project>();
                    var Wind_ProjectInfo_repo = uow.GetRepository<Wind_ProjectInfo>();

                    var ProjectAss = Wind_ProjectRole_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID && a.RoleID == (int)CloudWindProjectRole.项目协调员).ToList();
                    var ProjectManager = Wind_ProjectContacter_repo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).FirstOrDefault();

                    //管理员、协调员和项目经理才可以改
                    if (!Admins.Contains(UserCode) && !ProjectAss.Exists(a => a.UserCode.Equals(UserCode)) && !ProjectManager.DirectorCode.Equals(UserCode))
                    {
                        msg = "没有权限修改项目资料";
                        return msg;
                    }

                    var Project = Wind_Project_repo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (Project == null)
                    {
                        msg = "项目信息错误";
                        return msg;
                    }
                    var ProjectContactor = Wind_ProjectContacter_repo.Find(a => !a.IsDelete && a.ProjectID == Project.ID).FirstOrDefault();
                    if (ProjectContactor == null)
                    {
                        msg = "项目信息错误";
                        return msg;
                    }
                    var ProjectInfo = Wind_ProjectInfo_repo.Find(a => !a.IsDelete.Value && a.ProjectID == Project.ID).FirstOrDefault();
                    if (ProjectInfo == null)
                    {
                        msg = "项目信息错误";
                        return msg;
                    }

                    Project.ProjectName = request.ProjectName;
                    Project.ProjectStartTime = Convert.ToDateTime(request.ProjectStartTime);
                    Project.ProjectEndTime = Convert.ToDateTime(request.ProjectEndTime);
                    Project.CompanyID = request.CompanyID;
                    Project.Status = request.Status;

                    ProjectContactor.Director = request.ProjectManagerName;
                    ProjectContactor.DirectorCode = request.ProjectManagerUserCode;
                    ProjectContactor.DirectorDepart = request.ProjectManagerDepart;
                    ProjectContactor.DirectorPhone = request.ProjectManagerPhone;
                    ProjectContactor.DirectorJobName = request.ProjectManagerJobName;

                    ProjectInfo.WaterDepthMax = request.WaterDepthMax ?? "";
                    ProjectInfo.WaterDepthMin = request.WaterDepthMin ?? "";

                    //4.项目组成员
                    Wind_ProjectRole_repo.Find(a => !a.IsDelete && a.ProjectID == Project.ID && a.RoleID != (int)CloudWindProjectRole.项目协调员).ToList()
                        .ForEach(a =>
                        {
                            a.IsDelete = true;
                        });

                    if (!string.IsNullOrEmpty(request.ProjectGroupUsers))
                    {
                        List<CloudWindBackManageUserInfo> users = JsonUtils.Deserialize<List<CloudWindBackManageUserInfo>>(request.ProjectGroupUsers);
                        foreach (var user in users)
                        {
                            if (!string.IsNullOrEmpty(user.UserCode))
                            {
                                Wind_ProjectRole newRole = new Wind_ProjectRole();
                                newRole.ProjectID = Project.ID;
                                newRole.RoleID = (int)CloudWindProjectRole.项目组成员;
                                newRole.UserName = user.UserName;
                                newRole.UserCode = user.UserCode;
                                newRole.UserDepartName = user.UserDepartName;
                                newRole.UserPhone = user.UserPhone;
                                newRole.UserJobName = user.UserJobName;
                                newRole.IsDelete = false;
                                newRole.CreateTime = DateTime.UtcNow;
                                Wind_ProjectRole_repo.Add(newRole);
                            }
                        }
                    }

                    await uow.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                msg = "发生错误";
            }
            return msg;
        }

        public async Task<string> DeleteProjectFlow(CloudWindRequest request)
        {
            string msg = "";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var Wind_Project_repo = uow.GetRepository<Wind_Project>();

                    var Project = Wind_Project_repo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (Project == null)
                    {
                        msg = "项目数据错误";
                        return msg;
                    }

                    Project.IsDelete = true;

                    await uow.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                msg = "发生错误";
            }
            return msg;
        }

        public async Task<(List<CloudProjectFile> list, string msg)> DeleteProjectFile(int ID)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            string msg = "";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var Wind_ProjectFile_repo = uow.GetRepository<Wind_ProjectFile>();

                    var ProjectFile = Wind_ProjectFile_repo.Find(a => !a.IsDelete && a.ID == ID).FirstOrDefault();
                    if (ProjectFile == null)
                    {
                        msg = "数据错误";
                        return (list, msg);
                    }
                    ProjectFile.IsDelete = true;

                    await uow.SaveAsync();

                    var ProjectFiles = Wind_ProjectFile_repo.Find(a => !a.IsDelete && a.ProjectID == ProjectFile.ProjectID).ToList();
                    ProjectFiles.ForEach(a =>
                    {
                        list.Add(new CloudProjectFile()
                        {
                            ID = a.ID,
                            FileName = a.FileName,
                            FilePath = a.FilePath.Substring(a.FilePath.IndexOf(@"\File\")),
                            FileTime = Convert.ToDateTime(a.CreateTime).ToString("yyyy-MM-dd")
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                msg = "发生错误";
                list = new List<CloudProjectFile>();
            }
            return (list, msg);
        }


        public List<CloudBaseUserInfo> FindShjUser(string Name, out string msg)
        {
            List<CloudBaseUserInfo> list = new List<CloudBaseUserInfo>();
            msg = "";
            try
            {
                var ShjPostData = _cloudWindInfoService.GetShjUserInfoByName(Name);
                if (ShjPostData.StatusCode != 200)
                {
                    msg = "人员信息获取失败";
                    return new List<CloudBaseUserInfo>();
                }
                list = ShjPostData.Data;

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectService));
                list = new List<CloudBaseUserInfo>();
                msg = "发生错误";
            }
            return list;
        }


    }
}
