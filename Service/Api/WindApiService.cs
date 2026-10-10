using BIM.Business.CCSHJWebApi;
using Business.Towing;
using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using DataFactory.KingBase.Towing;
using Microsoft.AspNetCore.Hosting;
using Model.Base;
using Model.Tech.Cloud;
using Model.Tech.WebApi;
using Model.Towing;
using Service.Towing;
using Service.Wind;
using Tool;

namespace Service.Api
{
    public class WindApiService
    {
        private readonly CloudWind_KingBase_UnitOfWorkFactory _cloudWindUowFactory;
        private readonly CloudWindInfoService _cloudWindInfoService;
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;
        private readonly MessageHandleService _messageHandleService;
        private readonly HikInfoService _hikInfoService;
        private readonly CloudWindKZYService _cloudWindKZYService;
        private readonly IWebHostEnvironment _env;

        public WindApiService(
            CloudWind_KingBase_UnitOfWorkFactory cloudWindUowFactory,
            CloudWindInfoService cloudWindInfoService,
            Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory,
            MessageHandleService messageHandleService,
            HikInfoService hikInfoService,
            CloudWindKZYService cloudWindKZYService,
            IWebHostEnvironment env)
        {
            _cloudWindUowFactory = cloudWindUowFactory;
            _cloudWindInfoService = cloudWindInfoService;
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
            _messageHandleService = messageHandleService;
            _hikInfoService = hikInfoService;
            _cloudWindKZYService = cloudWindKZYService;
            _env = env;
        }

        #region 拖航监控系统数据接收

        /// <summary>
        /// 拖航监控数据接收（风速、电磁罗经、GPS、发电机、电池等）
        /// </summary>
        public async Task<int> TowingMonitorApi(CloudWindMonitorRequest request)
        {
            try
            {
                if (request == null)
                {
                    LoggerUtils.Error("request为null", typeof(WindApiService));
                    return 0;
                }

                if (string.IsNullOrEmpty(request.text))
                {
                    LoggerUtils.Error("text没有数据", typeof(WindApiService));
                    return 0;
                }

                await _messageHandleService.DataHandle_SanXia(request.text);

                return 1;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WindApiService));
                return 0;
            }
        }

        /// <summary>
        /// 拖航监控MRU数据接收
        /// </summary>
        public async Task<int> TowingMonitorMRUApi(CloudWindMonitorRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.text))
                {
                    LoggerUtils.Error("没有数据", typeof(WindApiService));
                    return 0;
                }

                await _messageHandleService.DataHandleMRU_SanXia(request.text);

                return 1;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WindApiService));
                return 0;
            }
        }

        /// <summary>
        /// 接受图片
        /// </summary>
        public string HikPicUpload(CloudWindMonitorRequest request)
        {
            var res = new ResponseBase<MessageInfo>();
            try
            {
                string file64 = request.file64;
                string fileName = request.fileName;
                DateTime? snapTime = request.snapTime;

                // 移除可能存在的URI前缀（如"data:image/png;base64,"）
                file64 = file64 ?? "";
                string cleanBase64 = file64.IndexOf(',') > 0 ? (file64 ?? "").Split(',')[1] : file64;
                if (string.IsNullOrEmpty(cleanBase64))
                {
                    res.SetError("文件为空");
                    return JsonUtils.Serialize(res);
                }
                // 转换为字节数组
                byte[] imageBytes = Convert.FromBase64String(cleanBase64);
                // 指定文件保存路径
                string stamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string stampedFileName = Path.GetFileNameWithoutExtension(fileName) + "_" + stamp + Path.GetExtension(fileName);
                string refUrl = "/Content/Towing/Uploads/HikLocalPics/" + stampedFileName;
                string dir = Path.Combine(_env.WebRootPath, "Content", "Towing", "Uploads", "HikLocalPics");
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, stampedFileName);
                // 写入文件
                File.WriteAllBytes(path, imageBytes);
                var r = _hikInfoService.SaveLocalPic(DateTime.Now, refUrl);
                if (r.IsValid)
                {
                    res.SetOK();
                }
                else
                {
                    res.SetError(r.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                res.SetError(ex);
            }
            return JsonUtils.Serialize(res);
        }

        /// <summary>
        /// 获取拖航监控最新数据
        /// </summary>
        public BaseReturn TowingMonitorGetData()
        {
            var ret = new BaseReturn();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var backupRepo = uow.GetRepository<Data_Backup>();
                    var latestData = backupRepo.Find(a => true).OrderByDescending(a => a.id).FirstOrDefault();

                    var messageRepo = uow.GetRepository<Data_Message>();
                    var message = messageRepo.Find(a => !a.IsDelete).FirstOrDefault();

                    if (latestData != null && !string.IsNullOrEmpty(latestData.DataString)
                        && message != null && !string.IsNullOrEmpty(message.Message))
                    {
                        var data = new TowingMonitorData();
                        data.Code = message.Message;
                        data.Message = latestData.DataString;
                        data.Time = latestData.CreateTime?.ToString("yyyy-MM-dd HH:mm:ss");

                        ret.Data = data;
                        ret.Success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(WindApiService));
            }
            return ret;
        }

        /// <summary>
        /// 设置拖航监控数据
        /// </summary>
        public string TowingMonitorSetData(CloudWindMonitorRequest request)
        {
            if (string.IsNullOrEmpty(request.text.Trim()))
            {
                return "数据是空的";
            }
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var messageRepo = uow.GetRepository<Data_Message>();
                    var message = messageRepo.Find(a => !a.IsDelete).FirstOrDefault();

                    if (message != null)
                    {
                        message.Message = request.text.Trim();
                        message.CreateTime = DateTime.Now;
                        uow.Save();
                        return "成功设置为 " + request.text.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WindApiService));
                return "发生了错误";
            }
            return "";
        }

        #endregion

        /// <summary>
        /// SSO 登录验证
        /// </summary>
        public BaseReturn SSOLoginChecking(TechCloudApiRequest request)
        {
            var ret = new BaseReturn();

            if (string.IsNullOrEmpty(request.Token))
            {
                ret.Message = "token是空";
                ret.Success = false;
                return ret;
            }

            try
            {
                var code = CloudCenterService.DecryptToken(request.Token);
                if (string.IsNullOrEmpty(code))
                {
                    ret.Message = "token错误";
                    ret.Success = false;
                    LoggerUtils.Error("解析token发生错误", typeof(CloudCenterApiService));
                    return ret;
                }

                //9:21-2018001515-10-2023/04/18 09:21:54
                var codeArray = code.Split('-');
                var userID = codeArray[1];
                var taskID = Convert.ToInt32(codeArray[2]);
                var time = codeArray[3];
                var postToken = codeArray[4];

                ret.PostToken = postToken;

                using (var uow = _cloudWindUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == taskID).FirstOrDefault();
                    if (task == null)
                    {
                        ret.Message = "token错误";
                        ret.Success = false;
                        LoggerUtils.Error("解析token发生错误", typeof(CloudCenterApiService));
                        return ret;
                    }

                    var softwareRepo = uow.GetRepository<Manage_Software>();
                    var software = softwareRepo.Find(a => !a.IsDelete && a.ID == task.SoftwareID).FirstOrDefault();
                    if (software == null)
                    {
                        ret.Message = JsonUtils.Serialize(request);
                        ret.Success = false;
                        LoggerUtils.Error("解析Software错误", typeof(CloudCenterApiService));
                        return ret;
                    }

                    //测试token不参与时间验证
                    var testToken = "eExyMFpyVHd4Mlc2dXlQcUtYUjI5OVVZQWtoYUZaU3VXdW1IeEJDOXRZWHVGRXRaaXJzbWNnPT0=";

                    //1.验证时间（10分钟内）
                    var loginTime = Convert.ToDateTime(time);

                    if (!request.Token.Equals(testToken))
                    {
                        if (loginTime.AddMinutes(10) < DateTime.Now)
                        {
                            ret.Message = "token失效";
                            ret.Success = false;
                            LoggerUtils.Error("token时间超时:" + request.Token, typeof(CloudCenterApiService));
                            return ret;
                        }
                    }

                    //获取用户信息
                    var shjUser = _cloudWindInfoService.GetShjUserInfo(userID);
                    if (shjUser.StatusCode != 200 || shjUser.Data == null || shjUser.Data.Count != 1)
                    {
                        ret.Message = "用户信息错误";
                        ret.Success = false;
                        LoggerUtils.Error("用户ID:" + userID + "用户信息错误", typeof(CloudCenterApiService));
                        return ret;
                    }

                    var loginRecord = new Base_LoginRecord();
                    loginRecord.UserName = shjUser.Data[0].namespell;
                    loginRecord.UserCode = userID;
                    loginRecord.Name = shjUser.Data[0].name;
                    loginRecord.SoftwareID = software.ID;
                    loginRecord.LoginResult = true;
                    loginRecord.LoginTime = DateTime.Now;
                    loginRecord.CreateTime = DateTime.Now;
                    loginRecord.IsDelete = false;

                    var loginRecordRepo = uow.GetRepository<Base_LoginRecord>();
                    loginRecordRepo.Add(loginRecord);
                    uow.Save();

                    var userinfo = new CloudCenterSSOUserInfo();
                    userinfo.UserID = userID;
                    userinfo.UserName = shjUser.Data[0].name;
                    userinfo.TaskID = taskID;

                    ret.Data = userinfo;
                    ret.Success = true;
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(CloudCenterApiService));
            }

            return ret;
        }

        /// <summary>
        /// 发送任务文件
        /// </summary>
        public WindCloudBaseReturn SendTaskFile(TechCloudWindApiRequest request)
        {
            var ret = new WindCloudBaseReturn();

            try
            {
                var list = new List<CloudWindTaskFileResponse>();

                using (var uow = _cloudWindUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (task == null)
                    {
                        ret.Message = "TaskID出错";
                        ret.Success = false;
                        return ret;
                    }

                    var projectInfoRepo = uow.GetRepository<View_Wind_ProjectInfo>();
                    var projectInfo = projectInfoRepo.Find(a => a.id == task.ProjectID).FirstOrDefault();
                    if (projectInfo == null)
                    {
                        ret.Message = "没有所属风电项目";
                        ret.Success = false;
                        return ret;
                    }

                    ret.CBZinfo.ProjectName = projectInfo.ProjectName;
                    ret.CBZinfo.WaterDepth = "0";
                    if (!string.IsNullOrEmpty(projectInfo.WaterDepthMax) && !string.IsNullOrEmpty(projectInfo.WaterDepthMin))
                    {
                        ret.CBZinfo.WaterDepth = Math.Round((Convert.ToDouble(projectInfo.WaterDepthMax) + Convert.ToDouble(projectInfo.WaterDepthMin)) / 2, 2).ToString();
                    }
                    ret.CBZinfo.TaskName = task.TaskName;

                    // 1.插拔桩软件
                    if (request.TaskTypeID == (int)CloudWindSoftware.自升式平台桩腿插拔计算)
                    {
                        // (1)地勘数据表
                        var dkFileRepo = uow.GetRepository<Flow_Task_DKFile>();
                        dkFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList()
                            .ForEach(a =>
                            {
                                list.Add(new CloudWindTaskFileResponse
                                {
                                    ID = a.id,
                                    TypeID = (int)CloudWindTaskFileType.风电项目_机孔位地勘数据表,
                                    TypeName = CloudWindTaskFileType.风电项目_机孔位地勘数据表.ToString(),
                                    FileName = "SoilData.xlsx",
                                    FileContent = FileUtils.CreateFileStream(a.FilePath)
                                });
                            });

                        // (2)平台船数据表和计算参数
                        var shipFileRepo = uow.GetRepository<Flow_Task_ShipFile>();
                        shipFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList()
                            .ForEach(a =>
                            {
                                list.Add(new CloudWindTaskFileResponse
                                {
                                    ID = a.id,
                                    TypeID = (int)CloudWindTaskFileType.插拔桩_自升式平台船数据表,
                                    TypeName = CloudWindTaskFileType.插拔桩_自升式平台船数据表.ToString(),
                                    FileName = a.FileName,
                                    FileContent = FileUtils.CreateFileStream(a.FilePath)
                                });

                                // 计算参数
                                string paramPath = Path.Combine(_env.WebRootPath, "File", "Task", "计算参数.xlsx");
                                list.Add(new CloudWindTaskFileResponse
                                {
                                    ID = a.id,
                                    TypeID = (int)CloudWindTaskFileType.插拔桩_自升式平台船计算参数,
                                    TypeName = CloudWindTaskFileType.插拔桩_自升式平台船计算参数.ToString(),
                                    FileName = "计算参数-" + a.FileName,
                                    FileContent = FileUtils.CreateFileStream(paramPath)
                                });
                            });
                    }
                }

                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(WindApiService));
            }

            return ret;
        }

        /// <summary>
        /// 接收插拔桩文件
        /// </summary>
        public WindCloudBaseReturn ReceiveTaskFile(TechCloudWindApiRequest request)
        {
            var ret = new WindCloudBaseReturn();

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var projectTaskRepo = uow.GetRepository<View_Wind_ProjectTask>();
                    var projectTask = projectTaskRepo.Find(a => a.id == request.TaskID).FirstOrDefault();
                    if (projectTask == null)
                    {
                        ret.Message = "TaskID出错";
                        ret.Success = false;
                        return ret;
                    }

                    projectTask.FlowStatus = (int)CloudWindTaskFlowStatus.审批中;

                    string exportDir = Path.Combine(_env.WebRootPath, "File", "Project", projectTask.ProjectCode, "Task", projectTask.TaskCode, "Export");
                    Directory.CreateDirectory(exportDir);

                    var reportList = new List<Wind_TaskReport>();
                    foreach (var item in request.TaskFiles)
                    {
                        string fileSavePath;
                        if (item.FileName.Equals("Error.txt"))
                        {
                            fileSavePath = Path.Combine(exportDir, item.FileName);
                        }
                        else
                        {
                            string ext = item.FileName.Substring(item.FileName.LastIndexOf('.'));
                            string newFileName = $"插拔桩计算报告({Guid.NewGuid()}){ext}";
                            fileSavePath = Path.Combine(exportDir, newFileName);
                        }

                        FileUtils.CreateFile(item.FileContent, fileSavePath);

                        reportList.Add(new Wind_TaskReport
                        {
                            Type = 1,
                            ReportName = item.FileName,
                            ReportPath = fileSavePath,
                            TaskID = projectTask.id,
                            CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                            IsDelete = false
                        });
                    }

                    // 先标记旧报告为删除
                    var reportRepo = uow.GetRepository<Wind_TaskReport>();
                    reportRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID && a.Type == 1).ToList()
                        .ForEach(a => { a.IsDelete = true; });

                    // 添加新报告
                    reportRepo.AddList(reportList);
                    uow.Save();
                }

                ret.Data = null;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(WindApiService));
            }

            return ret;
        }

        /// <summary>
        /// 接收可作业预报文件
        /// </summary>
        public WindCloudBaseReturn ReceiveShipPreReport(TechCloudWindApiRequest request)
        {
            var ret = new WindCloudBaseReturn();

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var thisTask = taskRepo.Find(a => !a.IsDelete && a.TaskCode == request.TaskCode).FirstOrDefault();
                    if (thisTask == null)
                    {
                        ret.Message = "数据错误";
                        ret.Success = false;
                        return ret;
                    }

                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var thisProject = projectRepo.Find(a => !a.IsDelete && a.ID == thisTask.ProjectID).FirstOrDefault();
                    if (thisProject == null)
                    {
                        ret.Message = "没有所属风电项目";
                        ret.Success = false;
                        return ret;
                    }

                    var taskFileRepo = uow.GetRepository<Wind_TaskFile>();
                    var existFiles = taskFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == thisTask.ID && a.TypeID == (int)CloudWindTaskSendFileType.可作业预报文件).ToList();

                    string saveFileFolder = Path.Combine(_env.WebRootPath, "File", "ShipWorkReport");
                    var newList = new List<Wind_TaskFile>();

                    foreach (var item in request.TaskFiles)
                    {
                        if (!item.FileName.StartsWith(thisProject.ProjectCode))
                            continue;

                        string fileSavePath = Path.Combine(saveFileFolder + item.FilePath, item.FileName);
                        string dirPath = Path.Combine(saveFileFolder, item.FilePath.TrimStart('\\', '/'));
                        Directory.CreateDirectory(dirPath);
                        FileUtils.CreateFile(item.FileContent, fileSavePath);

                        var sameModel = existFiles.FirstOrDefault(a => a.FileName == item.FileName);
                        if (sameModel == null)
                        {
                            newList.Add(new Wind_TaskFile
                            {
                                TypeID = (int)CloudWindTaskSendFileType.可作业预报文件,
                                TaskID = thisTask.ID,
                                FileName = item.FileName,
                                FilePath = "/File/ShipWorkReport" + item.FilePath + "/" + item.FileName,
                                CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                                IsDelete = false
                            });
                        }
                        else
                        {
                            sameModel.CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                        }
                    }

                    if (newList.Count > 0)
                    {
                        taskFileRepo.AddList(newList);
                    }
                    uow.Save();

                    ret.Success = true;

                    // 发送报告的日期限制
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_KZY>();
                    var taskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == thisTask.ID).FirstOrDefault();
                    if (taskInfo != null && taskInfo.ForecastStartTime != null && taskInfo.ForecastEndTime != null)
                    {
                        if (taskInfo.ForecastStartTime <= DateTime.Now && taskInfo.ForecastEndTime >= DateTime.Now)
                        {
                            _cloudWindKZYService.SendKZYReportToJJT(request.TaskCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(WindApiService));
            }

            return ret;
        }

        /// <summary>
        /// 发送可作业预报计算文件
        /// </summary>
        public WindCloudBaseReturn SendShipPreFile(TechCloudWindApiRequest request)
        {
            var ret = new WindCloudBaseReturn();

            try
            {
                string fileFolder = Path.Combine(_env.WebRootPath, "File", "ShipWorkReport", "CloudFile");
                string triggerFile = "CVReliabilityAnalysisInputTriggerFile.xlsx";

                if (!File.Exists(Path.Combine(fileFolder, triggerFile)))
                {
                    ret.Success = false;
                    ret.Message = "缺少 " + triggerFile;
                    return ret;
                }

                var list = new List<CloudWindTaskFileResponse>();

                list.Add(new CloudWindTaskFileResponse
                {
                    FileName = triggerFile,
                    FileContent = FileUtils.CreateFileStream(Path.Combine(fileFolder, triggerFile))
                });

                using (var uow = _cloudWindUowFactory.Create())
                {
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_KZY>();
                    var kzyTaskInfos = taskInfoRepo.Find(a => !(a.IsDelete == true)).ToList();

                    var projectTaskRepo = uow.GetRepository<View_Wind_ProjectTask>();
                    projectTaskRepo.Find(a => a.SoftwareID == (int)CloudWindSoftware.起重船基础施工可作业性预报 && a.FlowStatus == (int)CloudWindTaskFlowStatus.完成).ToList()
                        .ForEach(a =>
                        {
                            var thisTaskInfo = kzyTaskInfos.FirstOrDefault(b => b.TaskID == a.id);
                            if (thisTaskInfo != null && thisTaskInfo.ForecastStartTime <= DateTime.Now && thisTaskInfo.ForecastEndTime >= DateTime.Now)
                            {
                                string fileName = $@"CVReliabilityAnalysisInput{a.ProjectCode}_V1.0.xlsx";
                                string fullPath = Path.Combine(fileFolder, fileName);
                                if (!File.Exists(fullPath))
                                {
                                    LoggerUtils.Error($"{thisTaskInfo.TaskID} 缺少 {fileName}", typeof(WindApiService));
                                }
                                else
                                {
                                    list.Add(new CloudWindTaskFileResponse
                                    {
                                        FileName = fileName,
                                        FileContent = FileUtils.CreateFileStream(fullPath)
                                    });
                                }
                            }
                        });
                }

                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(WindApiService));
            }

            return ret;
        }

        /// <summary>
        /// 接收拖航报告文件
        /// </summary>
        public WindCloudBaseReturn ReceiveTowingReport(TechCloudWindApiRequest request)
        {
            var ret = new WindCloudBaseReturn();

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var thisTask = taskRepo.Find(a => !a.IsDelete && a.TaskCode == request.TaskCode).FirstOrDefault();
                    if (thisTask == null)
                    {
                        ret.Message = "数据错误";
                        ret.Success = false;
                        return ret;
                    }

                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var thisProject = projectRepo.Find(a => !a.IsDelete && a.ID == thisTask.ProjectID).FirstOrDefault();
                    if (thisProject == null)
                    {
                        ret.Message = "没有所属风电项目";
                        ret.Success = false;
                        return ret;
                    }

                    var taskFileRepo = uow.GetRepository<Wind_TaskFile>();
                    var existFiles = taskFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == thisTask.ID && a.TypeID == (int)CloudWindTaskSendFileType.拖航可行性分析文件).ToList();

                    string saveFileFolder = Path.Combine(_env.WebRootPath, "File", "TowingReport");
                    Directory.CreateDirectory(saveFileFolder);

                    var newList = new List<Wind_TaskFile>();

                    foreach (var item in request.TaskFiles)
                    {
                        string fileNameNew = Guid.NewGuid().ToString() + FileUtils.GetFileExtension(item.FileName);
                        string fileSavePath = Path.Combine(saveFileFolder, fileNameNew);
                        FileUtils.CreateFolderFile(item.FileContent, saveFileFolder, fileSavePath);

                        var sameModel = existFiles.FirstOrDefault(a => a.FileName == item.FileName);
                        if (sameModel == null)
                        {
                            newList.Add(new Wind_TaskFile
                            {
                                TypeID = (int)CloudWindTaskSendFileType.拖航可行性分析文件,
                                TaskID = thisTask.ID,
                                FileName = item.FileName,
                                FilePath = "/File/TowingReport/" + fileNameNew,
                                CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                                IsDelete = false
                            });
                        }
                        else
                        {
                            sameModel.CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                        }
                    }

                    if (newList.Count > 0)
                    {
                        taskFileRepo.AddList(newList);
                    }
                    uow.Save();

                    ret.Success = true;

                    _cloudWindKZYService.SendTowingReportToJJT(request.TaskCode);
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(WindApiService));
            }

            return ret;
        }

        /// <summary>
        /// 接收云服务文件
        /// </summary>
        public WindCloudBaseReturn ReceiveCloudFile(TechCloudWindApiRequest request)
        {
            var ret = new WindCloudBaseReturn();

            try
            {
                using (var cloudWindUow = _cloudWindUowFactory.Create())
                {
                    var taskRepo = cloudWindUow.GetRepository<Wind_Task>();
                    var thisTask = taskRepo.Find(a => !a.IsDelete && a.TaskCode == request.TaskCode).FirstOrDefault();
                    if (thisTask == null)
                    {
                        ret.Message = "数据错误";
                        ret.Success = false;
                        return ret;
                    }

                    var projectRepo = cloudWindUow.GetRepository<Wind_Project>();
                    var thisProject = projectRepo.Find(a => !a.IsDelete && a.ID == thisTask.ProjectID).FirstOrDefault();
                    if (thisProject == null)
                    {
                        ret.Message = "没有所属风电项目";
                        ret.Success = false;
                        return ret;
                    }

                    var taskFileRepo = cloudWindUow.GetRepository<Wind_TaskFile>();
                    string saveFileFolder = Path.Combine(_env.WebRootPath, "File", "CloudFile");
                    Directory.CreateDirectory(saveFileFolder);

                    var fileList = new List<Wind_TaskFile>();
                    List<string[]> predictionSummaryData = null;
                    List<string[]> warningSummaryData = null;

                    foreach (var item in request.TaskFiles)
                    {
                        string fileNameNew = Guid.NewGuid().ToString() + FileUtils.GetFileExtension(item.FileName);
                        string fileSavePath = Path.Combine(saveFileFolder, fileNameNew);
                        FileUtils.CreateFolderFile(item.FileContent, saveFileFolder, fileSavePath);

                        fileList.Add(new Wind_TaskFile
                        {
                            TypeID = (int)CloudWindTaskSendFileType.云推送文件,
                            TaskID = thisTask.ID,
                            FileName = item.FileName,
                            FilePath = "/File/CloudFile/" + fileNameNew,
                            CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                            IsDelete = false
                        });

                        if (item.FileName.Equals("PredictionSummary.csv"))
                        {
                            predictionSummaryData = CSVhelper.ReadCsvFile(fileSavePath);
                        }
                        if (item.FileName.Equals("WarningSummary.csv"))
                        {
                            warningSummaryData = CSVhelper.ReadCsvFile(fileSavePath);
                        }
                    }

                    // 处理CSV数据，写入Towing数据库
                    if (predictionSummaryData != null && warningSummaryData != null
                        && predictionSummaryData.Count > 0 && predictionSummaryData.Count == warningSummaryData.Count)
                    {
                        using (var towingUow = _towingKingBaseUowFactory.Create())
                        {
                            var forecastRepo = towingUow.GetRepository<Data_Forecast>();
                            var latestData = forecastRepo.Find(a => !a.IsDelete).OrderByDescending(a => a.No).FirstOrDefault();
                            int no = latestData == null ? 1 : (latestData.No.Value + 1);

                            var forList = new List<Data_Forecast>();

                            for (int i = 0; i < predictionSummaryData.Count; i++)
                            {
                                var prow = predictionSummaryData[i];
                                var wrow = warningSummaryData[i];

                                string yearStr = prow[0], monthStr = prow[1], dayStr = prow[2];
                                string hourStr = prow[3], minuteStr = prow[4], secondStr = prow[5];
                                string latStr = prow[6], lonStr = prow[7];
                                string maxYaoXStr = prow[8], maxYaoYStr = prow[9], maxDangZStr = prow[10];
                                string totalAlarmStr = wrow[8], windAlarmStr = wrow[9], waveAlarmStr = wrow[10], shakeAlarmStr = wrow[11];

                                if (!string.IsNullOrEmpty(yearStr) && !string.IsNullOrEmpty(monthStr) && !string.IsNullOrEmpty(dayStr)
                                    && !string.IsNullOrEmpty(hourStr) && !string.IsNullOrEmpty(minuteStr) && !string.IsNullOrEmpty(secondStr)
                                    && !string.IsNullOrEmpty(latStr) && !string.IsNullOrEmpty(lonStr)
                                    && !string.IsNullOrEmpty(maxYaoXStr) && !string.IsNullOrEmpty(maxYaoYStr) && !string.IsNullOrEmpty(maxDangZStr)
                                    && !string.IsNullOrEmpty(totalAlarmStr) && !string.IsNullOrEmpty(windAlarmStr) && !string.IsNullOrEmpty(waveAlarmStr) && !string.IsNullOrEmpty(shakeAlarmStr))
                                {
                                    forList.Add(new Data_Forecast
                                    {
                                        No = no,
                                        ForecastTime = new DateTime(Convert.ToInt32(yearStr), Convert.ToInt32(monthStr), Convert.ToInt32(dayStr), Convert.ToInt32(hourStr), Convert.ToInt32(minuteStr), Convert.ToInt32(secondStr)),
                                        Lat = Convert.ToDecimal(latStr),
                                        Lon = Convert.ToDecimal(lonStr),
                                        MaxYaoX = Convert.ToDecimal(maxYaoXStr),
                                        MaxYaoY = Convert.ToDecimal(maxYaoYStr),
                                        MaxDangZ = Convert.ToDecimal(maxDangZStr),
                                        TotalAlarm = Convert.ToInt32(totalAlarmStr),
                                        WindAlarm = Convert.ToInt32(windAlarmStr),
                                        WaveAlarm = Convert.ToInt32(waveAlarmStr),
                                        ShakeAlarm = Convert.ToInt32(shakeAlarmStr),
                                        CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                                        IsDelete = false
                                    });
                                }
                            }

                            if (forList.Count > 0)
                            {
                                forecastRepo.AddList(forList);
                                towingUow.Save();
                            }
                        }
                    }

                    if (fileList.Count > 0)
                    {
                        taskFileRepo.AddList(fileList);
                    }
                    cloudWindUow.Save();

                    ret.Success = true;

                    _cloudWindKZYService.SendTowingReportToJJT(request.TaskCode);
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(WindApiService));
            }

            return ret;
        }

        /// <summary>
        /// 获取拖航数据
        /// </summary>
        public WindCloudBaseReturn GetTowingData(TechCloudWindApiRequest request)
        {
            var ret = new WindCloudBaseReturn();

            try
            {
                using (var cloudWindUow = _cloudWindUowFactory.Create())
                {
                    var taskRepo = cloudWindUow.GetRepository<Wind_Task>();
                    var thisTask = taskRepo.Find(a => !a.IsDelete && a.TaskCode == request.TaskCode).FirstOrDefault();
                    if (thisTask == null)
                    {
                        ret.Message = "数据错误";
                        ret.Success = false;
                        return ret;
                    }

                    var projectRepo = cloudWindUow.GetRepository<Wind_Project>();
                    var thisProject = projectRepo.Find(a => !a.IsDelete && a.ID == thisTask.ProjectID).FirstOrDefault();
                    if (thisProject == null)
                    {
                        ret.Message = "数据错误";
                        ret.Success = false;
                        return ret;
                    }

                    using (var towingUow = _towingKingBaseUowFactory.Create())
                    {
                        var manageProjectRepo = towingUow.GetRepository<Manage_Project>();
                        var towingProject = manageProjectRepo.Find(a => !a.IsDelete && a.CloudProjectID == thisProject.ID).FirstOrDefault();
                        if (towingProject == null)
                        {
                            ret.Message = "数据错误";
                            ret.Success = false;
                            return ret;
                        }

                        var satelliteRepo = towingUow.GetRepository<Data_SATELLITE>();
                        var position = satelliteRepo.Find(a => !a.IsDelete).OrderByDescending(a => a.id).FirstOrDefault();
                        if (position == null)
                        {
                            ret.Message = "没有实时定位数据";
                            ret.Success = false;
                            return ret;
                        }

                        var data = new TowingData();
                        data.PositionTime.Lon = position.Lon == null ? "" : position.Lon.ToString();
                        data.PositionTime.Lat = position.Lat == null ? "" : position.Lat.ToString();
                        data.PositionTime.Time = Convert.ToDateTime(position.CreateTime).ToString("yyyy-MM-dd HH:mm:ss");

                        ret.Data = data;
                        ret.Success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(WindApiService));
            }

            return ret;
        }




    }
}
