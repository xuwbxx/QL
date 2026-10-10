using BIM.Business.CCSHJWebApi;
using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Microsoft.AspNetCore.Hosting;
using Model.Tech.Cloud;
using Model.Tech.System;
using System.Data;
using Tool;

namespace Service.Wind
{
    public class CloudWindKZYService
    {
        private readonly CloudWind_KingBase_UnitOfWorkFactory _techCenterUowFactory;
        private readonly CloudCenterService _cloudCenterService;
        private readonly CloudWindInfoService _cloudWindInfoService;
        private readonly IWebHostEnvironment _env;

        public CloudWindKZYService(
            CloudWind_KingBase_UnitOfWorkFactory techCenterUowFactory,
            CloudCenterService cloudCenterService,
            CloudWindInfoService cloudWindInfoService,
            IWebHostEnvironment env)
        {
            _techCenterUowFactory = techCenterUowFactory;
            _cloudCenterService = cloudCenterService;
            _cloudWindInfoService = cloudWindInfoService;
            _env = env;
        }

        /// <summary>
        /// 推送可作业预报到交建通
        /// </summary>
        public void SendKZYReportToJJT(string taskCode)
        {
            if (string.IsNullOrEmpty(taskCode))
            {
                return;
            }

            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_KZY>();
                    var jjtResultRepo = uow.GetRepository<Wind_Task_JJTResult>();

                    var thisTask = taskRepo.Find(a => !a.IsDelete && a.TaskCode == taskCode).FirstOrDefault();
                    if (thisTask == null) return;

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == thisTask.ProjectID).FirstOrDefault();
                    if (project == null) return;

                    var receivers = deliverRepo.Find(a => !a.IsDelete && a.TaskID == thisTask.ID).ToList();

                    string cloudPlatUrl = AppSettingUtils.GetSetting("SoftwareApi:CloudPlatUrl");

                    // Token可以多次使用
                    var accessToken = _cloudWindInfoService.GetShjToken();

                    List<Wind_Task_JJTResult> list = new List<Wind_Task_JJTResult>();
                    List<SystemJJTInform> informRequests = new List<SystemJJTInform>();

                    var thisTaskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == thisTask.ID).FirstOrDefault();
                    if (thisTaskInfo == null || string.IsNullOrEmpty(thisTaskInfo.ShipIDs))
                    {
                        return;
                    }

                    var now = DateTime.Now;
                    string dateStr = now.Year + "年" + now.Month + "月" + now.Day + "日";
                    string informTitle = "(" + project.ProjectCode + ")" + project.ProjectName + " " + dateStr + " 起重船基础施工可作业性预报";

                    receivers.ForEach(a =>
                    {
                        var token = _cloudCenterService.CreateJJTMessageRedirectToken("Task", a.DeliverCode, thisTask.ID);
                        var redirectUrl = cloudPlatUrl + @"Home/JJTMessageRedirect?CloudToken=" + token;

                        informRequests.Add(new SystemJJTInform()
                        {
                            UserCode = a.DeliverCode,
                            Url = redirectUrl,
                            Title = informTitle,
                            Content = now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                        });

                        list.Add(new Wind_Task_JJTResult()
                        {
                            Type = "KZY",
                            UserCode = a.DeliverCode,
                            DirectUrl = redirectUrl,
                            Result = true,
                            CreateTime = now,
                            IsDelete = false
                        });
                    });

                    jjtResultRepo.AddList(list);
                    uow.Save();

                    _cloudWindInfoService.SendJJTMessage(informRequests);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWindKZYService));
            }
        }

        /// <summary>
        /// 生成可作业预报文件
        /// </summary>
        public void CreateKZYFile(int taskId)
        {
            if (taskId == 0) return;

            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_KZY>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var kzyShipRepo = uow.GetRepository<Library_Ship_KZY>();
                    var viewProjectTaskRepo = uow.GetRepository<View_Wind_ProjectTask>();

                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == taskId).FirstOrDefault();
                    if (task == null) return;

                    var taskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == taskId).FirstOrDefault();
                    if (taskInfo == null) return;

                    var project = projectRepo.Find(a => a.ID == task.ProjectID).FirstOrDefault();
                    if (project == null) return;

                    if (string.IsNullOrEmpty(project.ProjectCode) || string.IsNullOrEmpty(taskInfo.YFYNo)
                        || string.IsNullOrEmpty(task.TaskCode) || string.IsNullOrEmpty(taskInfo.Lat)
                        || string.IsNullOrEmpty(taskInfo.Lon) || string.IsNullOrEmpty(taskInfo.ShipIDs)
                        || string.IsNullOrEmpty(project.ProjectName) || string.IsNullOrEmpty(taskInfo.Balance))
                    {
                        LoggerUtils.Error("生成可作业预报文件出错：" + taskId + " - 数据不完整", typeof(CloudWindKZYService));
                        return;
                    }

                    string windFarmNumber = project.ProjectCode;
                    string tn = taskInfo.YFYNo;
                    string tk = task.TaskCode;
                    string latOW = taskInfo.Lat;
                    string lonOW = taskInfo.Lon;
                    string cvNofOW = taskInfo.ShipIDs.Split(',').Length.ToString();
                    string windFarmName = project.ProjectName;
                    string criteriaOW = taskInfo.Balance;

                    DataSet ds = new DataSet();

                    DataTable dt1 = new DataTable();
                    dt1.TableName = "BasicInfo";
                    dt1.Columns.Add("变量", typeof(string));
                    dt1.Columns.Add("赋值", typeof(string));
                    dt1.Columns.Add("说明", typeof(string));
                    dt1.Columns.Add("填写备注", typeof(string));
                    dt1.Rows.Add("WindFarmNumber", windFarmNumber, "风场号", "云平台自动关联");
                    dt1.Rows.Add("TN", tn, "亿方云上的TaskNo.", "技术中心输入");
                    dt1.Rows.Add("TK", tk, "云平台上的TaskNo.", "云平台自动生成");
                    dt1.Rows.Add("LatOW", latOW, "纬度（风场代表性点）", "技术中心输入");
                    dt1.Rows.Add("LonOW", lonOW, "经度（风场代表性点）", "技术中心输入");
                    dt1.Rows.Add("CVNofOW", cvNofOW, "该风场计算起重船数量", "项目部输入");
                    dt1.Rows.Add("WindFarmName", windFarmName, "该风电场名字", "云平台自动生成");
                    ds.Tables.Add(dt1);

                    DataTable dt2 = new DataTable();
                    dt2.TableName = "VesselInfo";
                    dt2.Columns.Add("起重船序号", typeof(string));
                    dt2.Columns.Add("起重船名", typeof(string));
                    var libraryKZYShips = kzyShipRepo.Find(a => !(a.IsDelete == true)).ToList();
                    int shipCount = 1;
                    foreach (string shipId in taskInfo.ShipIDs.Split(','))
                    {
                        int sid = Convert.ToInt32(shipId);
                        var ship = libraryKZYShips.FirstOrDefault(a => a.ID == sid);
                        if (ship != null)
                        {
                            dt2.Rows.Add("CV" + shipCount + "NameofOW", ship.ShipName);
                            shipCount++;
                        }
                    }
                    ds.Tables.Add(dt2);

                    DataTable dt3 = new DataTable();
                    dt3.TableName = "Criteria";
                    dt3.Columns.Add("衡准", typeof(string));
                    dt3.Columns.Add("数值", typeof(string));
                    dt3.Rows.Add("CriteriaOW", criteriaOW);
                    ds.Tables.Add(dt3);

                    string fileFolder = Path.Combine(_env.WebRootPath, "File", "ShipWorkReport", "CloudFile");
                    string fileName = "CVReliabilityAnalysisInput" + project.ProjectCode + "_V1.0.xlsx";

                    FileUtils.DeleteFile(Path.Combine(fileFolder, fileName));
                    ExcelUtils.DataSetToExcel(ds, Path.Combine(fileFolder, fileName), false);

                    // 生成触发文件
                    string triggerFileName = "CVReliabilityAnalysisInputTriggerFile.xlsx";
                    FileUtils.DeleteFile(Path.Combine(fileFolder, triggerFileName));

                    DataSet ds2 = new DataSet();
                    DataTable dt4 = new DataTable();
                    dt4.TableName = "CVReliabilityAnalysisInputTrigg";
                    dt4.Columns.Add("ProjectCode", typeof(string));

                    var kzyTaskInfos = taskInfoRepo.Find(a => !(a.IsDelete == true)).ToList();
                    var now = DateTime.Now;
                    viewProjectTaskRepo.Find(a => a.SoftwareID == (int)CloudWindSoftware.起重船基础施工可作业性预报 && a.FlowStatus == (int)CloudWindTaskFlowStatus.完成)
                        .OrderBy(a => a.ProjectID).ToList()
                        .ForEach(a =>
                        {
                            var info = kzyTaskInfos.FirstOrDefault(b => b.TaskID == a.id);
                            if (info != null && info.ForecastStartTime <= now && info.ForecastEndTime >= now)
                            {
                                dt4.Rows.Add(a.ProjectCode);
                            }
                        });
                    ds2.Tables.Add(dt4);
                    ExcelUtils.DataSetToExcel(ds2, Path.Combine(fileFolder, triggerFileName), false);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWindKZYService));
            }
        }

        /// <summary>
        /// 推送拖航报告到交建通
        /// </summary>
        public void SendTowingReportToJJT(string taskCode)
        {
            if (string.IsNullOrEmpty(taskCode)) return;

            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var jjtResultRepo = uow.GetRepository<Wind_Task_JJTResult>();

                    var thisTask = taskRepo.Find(a => !a.IsDelete && a.TaskCode == taskCode).FirstOrDefault();
                    if (thisTask == null) return;

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == thisTask.ProjectID).FirstOrDefault();
                    if (project == null) return;

                    var receivers = deliverRepo.Find(a => !a.IsDelete && a.TaskID == thisTask.ID).ToList();

                    string cloudPlatUrl = AppSettingUtils.GetSetting("SoftwareApi:CloudPlatUrl");

                    var accessToken = _cloudWindInfoService.GetShjToken();

                    List<Wind_Task_JJTResult> list = new List<Wind_Task_JJTResult>();
                    List<SystemJJTInform> informRequests = new List<SystemJJTInform>();

                    string informTitle = "(" + project.ProjectCode + ")" + project.ProjectName + "-拖航可行性分析报告";

                    var now = DateTime.Now;
                    receivers.ForEach(a =>
                    {
                        var token = _cloudCenterService.CreateJJTMessageRedirectToken("Task", a.DeliverCode, thisTask.ID);
                        var redirectUrl = cloudPlatUrl + @"Home/JJTMessageRedirect?CloudToken=" + token;

                        informRequests.Add(new SystemJJTInform()
                        {
                            UserCode = a.DeliverCode,
                            Url = redirectUrl,
                            Title = informTitle,
                            Content = now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                        });

                        list.Add(new Wind_Task_JJTResult()
                        {
                            Type = "Towing",
                            UserCode = a.DeliverCode,
                            DirectUrl = redirectUrl,
                            Result = true,
                            CreateTime = now,
                            IsDelete = false
                        });
                    });

                    jjtResultRepo.AddList(list);
                    uow.Save();

                    _cloudWindInfoService.SendJJTMessage(informRequests);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWindKZYService));
            }
        }
    }
}
