using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Model.Tech.Cloud;
using Model.Tech.Cloud.BackManage;
using System;
using System.Collections.Generic;
using System.Linq;
using Tool;

namespace Service.Wind.BackManage
{
    public class BackKZYReportService
    {
        private readonly CloudWind_KingBase_UnitOfWorkFactory _cloudWindUowFactory;

        public BackKZYReportService(CloudWind_KingBase_UnitOfWorkFactory cloudWindUowFactory)
        {
            _cloudWindUowFactory = cloudWindUowFactory;
        }

        public (List<CloudWindManageTaskInfo> list, int totalCount, int pageIndex, string msg) ListQuery(CloudWindBackManageRequest request)
        {
            var list = new List<CloudWindManageTaskInfo>();
            string msg = "";
            int totalCount = 0;
            int pageIndex = request.PageIndex;

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<View_Wind_ProjectTask>();

                    var predicate = PredicateBuilder.True<View_Wind_ProjectTask>();
                    predicate = PredicateBuilder.And(predicate, a => a.SoftwareID == (int)CloudWindSoftware.起重船基础施工可作业性预报);
                    predicate = PredicateBuilder.And(predicate, a => a.FlowStatus == (int)CloudWindTaskFlowStatus.完成);

                    if (!string.IsNullOrEmpty(request.ProjectName))
                    {
                        var projectName = request.ProjectName;
                        predicate = PredicateBuilder.And(predicate, a => a.ProjectName != null && a.ProjectName.Contains(projectName));
                    }

                    if (!string.IsNullOrEmpty(request.TaskName))
                    {
                        var taskName = request.TaskName;
                        predicate = PredicateBuilder.And(predicate, a => a.TaskName != null && a.TaskName.Contains(taskName));
                    }

                    var (pageList, count) = taskRepo.FindPage(predicate, a => a.id, request.PageIndex, request.PageSize);

                    totalCount = count;

                    if (request.PageIndex != 1 && pageList.Count() == 0)
                    {
                        pageIndex = 1;
                        (pageList, totalCount) = taskRepo.FindPage(predicate, a => a.id, 1, request.PageSize);
                    }

                    // 查询任务信息
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_KZY>();
                    var taskInfoList = taskInfoRepo.Find(a => !(a.IsDelete == true)).ToList();

                    foreach (var a in pageList)
                    {
                        var data = new CloudWindManageTaskInfo()
                        {
                            ID = a.id,
                            ProjectCode = a.ProjectCode ?? "",
                            ProjectName = a.ProjectName ?? "",
                            TaskName = a.TaskName ?? "",
                            TaskCode = a.TaskCode ?? "",
                            IsTimeOut = true,
                            SendStartTime = "",
                            SendEndTime = ""
                        };

                        var taskInfo = taskInfoList.FirstOrDefault(b => b.TaskID == a.id);
                        if (taskInfo != null)
                        {
                            if (taskInfo.ForecastStartTime != null && taskInfo.ForecastEndTime != null)
                            {
                                var now = DateTime.UtcNow;
                                if (taskInfo.ForecastStartTime <= now && taskInfo.ForecastEndTime >= now)
                                {
                                    data.IsTimeOut = false;
                                }

                                data.SendStartTime = taskInfo.ForecastStartTime.Value.ToString("yyyy-MM-dd");
                                data.SendEndTime = taskInfo.ForecastEndTime.Value.ToString("yyyy-MM-dd");
                            }
                        }

                        list.Add(data);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackKZYReportService));
                msg = "发生错误";
            }

            return (list, totalCount, pageIndex, msg);
        }

        public (CloudWindManageTaskDeliver data, string msg) DataQuery(CloudWindBackManageRequest request)
        {
            var data = new CloudWindManageTaskDeliver();
            string msg = "";

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var task = taskRepo.FindFirst(a => !a.IsDelete && a.ID == request.TaskID);
                    if (task == null)
                    {
                        return (null, "任务数据错误");
                    }

                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var project = projectRepo.FindFirst(a => !a.IsDelete && a.ID == task.ProjectID);
                    if (project == null)
                    {
                        return (null, "任务数据错误");
                    }

                    data.TaskID = task.ID;
                    data.TaskName = task.TaskName ?? "";
                    data.ProjectCode = project.ProjectCode ?? "";
                    data.ProjectName = project.ProjectName ?? "";

                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var delivers = deliverRepo.Find(a => !a.IsDelete && a.TaskID == task.ID).ToList();
                    foreach (var a in delivers)
                    {
                        data.Delivers.Add(new CloudWindManageProjectRoleData()
                        {
                            UserName = a.DeliverName ?? "",
                            UserCode = a.DeliverCode ?? "",
                            UserDepartName = a.DeliverDepart ?? "",
                            UserPhone = a.DeliverPhone ?? "",
                            UserJobName = a.DeliverJobName ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackKZYReportService));
                msg = "发生错误，请联系管理员";
            }

            return (data, msg);
        }

        public string DataSave(CloudWindBackManageRequest request)
        {
            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var task = taskRepo.FindFirst(a => !a.IsDelete && a.ID == request.TaskID);
                    if (task == null)
                    {
                        return "任务数据错误";
                    }

                    // 软删除旧的交付人
                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var oldDelivers = deliverRepo.Find(a => !a.IsDelete && a.TaskID == task.ID).ToList();
                    foreach (var a in oldDelivers)
                    {
                        a.IsDelete = true;
                    }

                    // 插入新的交付人
                    foreach (var a in request.NodeManagers)
                    {
                        if (!string.IsNullOrEmpty(a.UserName) && !string.IsNullOrEmpty(a.UserCode))
                        {
                            var deliver = new Wind_TaskFileDeliver()
                            {
                                TaskID = task.ID,
                                DeliverName = a.UserName,
                                DeliverCode = a.UserCode,
                                DeliverDepart = a.UserDepartName,
                                DeliverPhone = a.UserPhone,
                                DeliverJobName = a.UserJobName,
                                IsDelete = false,
                                CreateTime = DateTime.UtcNow
                            };
                            deliverRepo.Add(deliver);
                        }
                    }

                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackKZYReportService));
                return "发生错误，请联系管理员";
            }

            return "";
        }
    }
}
