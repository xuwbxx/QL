using BIM.Business.CCSHJWebApi;
using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using DataFactory.KingBase.Towing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        private readonly CloudWindKZYService _cloudWindKZYService;
        private readonly IWebHostEnvironment _env;

        public TaskService(
            CloudWind_KingBase_UnitOfWorkFactory techCenterUowFactory,
            Towing_KingBase_UnitOfWorkFactory towingUowFactory,
            CookieService cookieService,
            CloudCenterService cloudCenterService,
            CloudWindInfoService cloudWindInfoService,
            CloudWindKZYService cloudWindKZYService,
            IWebHostEnvironment env)
            : base(techCenterUowFactory, cookieService)
        {
            _towingUowFactory = towingUowFactory;
            _cloudCenterService = cloudCenterService;
            _cloudWindInfoService = cloudWindInfoService;
            _cloudWindKZYService = cloudWindKZYService;
            _env = env;
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
        /// 获取文件保存路径（物理路径）
        /// </summary>
        public string GetFileSavePath(int taskId, string flowHandle)
        {
            if (flowHandle.Equals("new"))
            {
                return Path.Combine(_env.WebRootPath, "File", "Task", "TempFile");
            }

            using (var uow = _techCenterUowFactory.Create())
            {
                var viewRepo = uow.GetRepository<View_Wind_ProjectTask>();
                var projectTask = viewRepo.Find(a => a.id == taskId).FirstOrDefault();
                if (projectTask == null) throw new Exception("任务数据错误");

                return Path.Combine(_env.WebRootPath, "File", "Project", projectTask.ProjectCode, "Task", projectTask.TaskCode, "Import");
            }
        }

        /// <summary>
        /// 获取文件下载路径（URL路径）
        /// </summary>
        public string GetFileDownloadPath(int taskId, string flowHandle)
        {
            if (flowHandle.Equals("new"))
            {
                return "/File/Task/TempFile/";
            }

            using (var uow = _techCenterUowFactory.Create())
            {
                var viewRepo = uow.GetRepository<View_Wind_ProjectTask>();
                var projectTask = viewRepo.Find(a => a.id == taskId).FirstOrDefault();
                if (projectTask == null) throw new Exception("任务数据错误");

                return "/File/Project/" + projectTask.ProjectCode + "/Task/" + projectTask.TaskCode + "/Import/";
            }
        }

        /// <summary>
        /// 获取项目的地勘资料
        /// </summary>
        public CloudWindTaskDK LoadDKZL(CloudWindRequest request)
        {
            CloudWindTaskDK data = new CloudWindTaskDK();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var dkFileRepo = uow.GetRepository<Flow_Task_DKFile>();
                    var dkRepo = uow.GetRepository<Library_Geology_DK>();

                    var dkFile = dkFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).FirstOrDefault();
                    if (dkFile != null)
                    {
                        data.File.FileName = dkFile.FileName;
                        data.File.FilePath = FileUtils.GetRelativeFilePath(dkFile.FilePath);
                    }

                    dkRepo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID).ToList()
                        .ForEach(a =>
                        {
                            data.DKs.Add(new CloudWindTaskDKData()
                            {
                                ID = a.ID,
                                DKName = a.DKName
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return data;
        }

        /// <summary>
        /// 加载船舶库
        /// </summary>
        public CloudWindTaskShip LoadShip(CloudWindRequest request)
        {
            CloudWindTaskShip data = new CloudWindTaskShip();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var shipRepo = uow.GetRepository<Library_Ship>();
                    var shipFileRepo = uow.GetRepository<Flow_Task_ShipFile>();

                    string fileDownloadPath = GetFileDownloadPath(request.TaskID, request.FlowHandle);

                    shipRepo.Find(a => !(a.IsDelete == true) && (a.IsConfirm == true || a.TaskID == request.TaskID)).ToList()
                        .ForEach(a =>
                        {
                            data.Ships.Add(new CloudWindTaskShipData()
                            {
                                ID = a.ID,
                                ShipName = a.ShipName
                            });
                        });

                    shipFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList()
                        .ForEach(a =>
                        {
                            string fileName = a.FilePath.Substring(a.FilePath.LastIndexOf('/') + 1);
                            data.Files.Add(new CloudWindTaskShipFile()
                            {
                                ID = a.id,
                                FileName = a.FileName,
                                FilePath = fileDownloadPath + fileName
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return data;
        }

        /// <summary>
        /// 查询船舶信息
        /// </summary>
        public CloudWindLibraryShip OpenShip(CloudWindRequest request)
        {
            CloudWindLibraryShip se = null;
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var shipRepo = uow.GetRepository<Library_Ship>();
                    var shipDataRepo = uow.GetRepository<Library_Ship_Data>();

                    var ship = shipRepo.Find(a => !(a.IsDelete == true) && a.ID == request.ShipID).FirstOrDefault();
                    if (ship == null) throw new Exception("船舶数据错误");

                    var d = shipDataRepo.Find(a => !(a.IsDelete == true) && a.ShipID == request.ShipID).FirstOrDefault();
                    if (d == null) throw new Exception("船舶数据错误");

                    se = new CloudWindLibraryShip();
                    se.ID = d.ShipID ?? 0;
                    se.ShipName = ship.ShipName;
                    se.BZL = d.bzl ?? "";
                    se.DDBY = d.ddby ?? "";
                    se.JSYYHZ = d.jsyyhz ?? "";
                    se.QX = d.qx_cddsm ?? "";
                    se.YXZTCD = d.yxztcd_cddxd ?? "";
                    se.ZTYXCD = d.ztyxcd ?? "";
                    se.ZTYYL = d.ztyyl ?? "";
                    se.ZTZC = d.ztzc ?? "";
                    se.ZTJMJ = d.ztjmj ?? "";
                    se.ZTZXZZ = d.ztzxzzW ?? "";
                    se.ZXCD = d.zxcdL ?? "";
                    se.ZXGD = d.zxgdH ?? "";
                    se.ZXKD = d.zxkdB ?? "";
                    se.ZXMJ = d.zxmjA ?? "";
                    se.ZXTJ = d.zxtjV ?? "";
                    se.ZXZDJMZC = d.zxzdjmzc ?? "";
                    se.ZTZJ = d.ztzj ?? "";
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return se;
        }

        /// <summary>
        /// 更新平台船库
        /// </summary>
        public void SaveLibraryShip(CloudWindRequest request)
        {
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var shipDataRepo = uow.GetRepository<Library_Ship_Data>();

                    var shipData = shipDataRepo.Find(a => !(a.IsDelete == true) && a.ShipID == request.ShipID).FirstOrDefault();
                    if (shipData == null) throw new Exception("船舶数据错误");

                    shipData.ztyyl = request.ShipZTYYL;
                    shipData.jsyyhz = request.ShipJSYYHZ;
                    shipData.ddby = request.ShipDDBY;
                    shipData.ztjmj = request.ShipZTJMJ;
                    shipData.CreateTime = DateTime.Now;
                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
        }

        /// <summary>
        /// 上传船舶库
        /// </summary>
        public List<Library_Ship> CreateLibraryShip(CloudWindRequest request, IFormFile file)
        {
            List<Library_Ship> ships = new List<Library_Ship>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var shipRepo = uow.GetRepository<Library_Ship>();
                    var shipDataRepo = uow.GetRepository<Library_Ship_Data>();

                    var existShip = shipRepo.Find(a => !(a.IsDelete == true) && a.ShipName.Equals(request.ShipName)).FirstOrDefault();
                    if (existShip != null) throw new Exception("船库中已存在此船名");

                    string fileSavePath = GetFileSavePath(request.TaskID, request.FlowHandle);
                    string filePath = fileSavePath + Guid.NewGuid() + Path.GetExtension(file.FileName);

                    // 先从内存流读取Excel数据（在保存文件之前）
                    DataSet ds = ExcelUtils.ConvertExcelFileToDataSet(file);
                    DataTable dt = ds.Tables[0];

                    // 保存文件到磁盘
                    FileUtils.SaveFile(file, filePath);

                    // 插入船舶记录
                    Library_Ship ship = new Library_Ship();
                    ship.TaskID = request.TaskID;
                    ship.ShipName = request.ShipName;
                    ship.FileName = file.FileName;
                    ship.FilePath = filePath;
                    ship.IsConfirm = false;
                    ship.CreateTime = DateTime.Now;
                    ship.IsDelete = false;
                    shipRepo.Add(ship);
                    uow.Save();

                    int newShipID = ship.ID;

                    // 创建船舶数据记录
                    Library_Ship_Data data = new Library_Ship_Data();
                    data.ShipID = newShipID;
                    data.ztzj = SafeRound(GetCell(dt, 0, 1)); data.ztzj_unit = GetCell(dt, 0, 2);
                    data.ztzc = SafeRound(GetCell(dt, 1, 1)); data.ztzc_unit = GetCell(dt, 1, 2);
                    data.zxcdL = SafeRound(GetCell(dt, 2, 1)); data.zxcdL_unit = GetCell(dt, 2, 2);
                    data.zxkdB = SafeRound(GetCell(dt, 3, 1)); data.zxkdB_unit = GetCell(dt, 3, 2);
                    data.zxgdH = SafeRound(GetCell(dt, 4, 1)); data.zxgdH_unit = GetCell(dt, 4, 2);
                    data.zxmjA = SafeRound(GetCell(dt, 5, 1)); data.zxmjA_unit = GetCell(dt, 5, 2);
                    data.zxzdjmzc = SafeRound(GetCell(dt, 6, 1)); data.zxzdjmzc_unit = GetCell(dt, 6, 2);
                    data.zxtjV = SafeRound(GetCell(dt, 7, 1)); data.zxtjV_unit = GetCell(dt, 7, 2);
                    data.ztzxzzW = SafeRound(GetCell(dt, 8, 1)); data.ztzxzzW_unit = GetCell(dt, 8, 2);
                    data.ztyyl = SafeRound(GetCell(dt, 9, 1)); data.ztyyl_unit = GetCell(dt, 9, 2);
                    data.jsyyhz = SafeRound(GetCell(dt, 10, 1)); data.jsyyhz_unit = GetCell(dt, 10, 2);
                    data.bzl = SafeRound(GetCell(dt, 11, 1)); data.bzl_unit = GetCell(dt, 11, 2);
                    data.ddby = SafeRound(GetCell(dt, 12, 1)); data.ddby_unit = GetCell(dt, 12, 2);
                    data.yxztcd_cddxd = SafeRound(GetCell(dt, 13, 1)); data.yxztcd_cddxd_unit = GetCell(dt, 13, 2);
                    data.qx_cddsm = SafeRound(GetCell(dt, 14, 1)); data.qx_cddsm_unit = GetCell(dt, 14, 2);
                    data.ztyxcd = SafeRound(GetCell(dt, 15, 1)); data.ztyxcd_unit = GetCell(dt, 15, 2);
                    data.ztjmj = SafeRound(GetCell(dt, 16, 1)); data.ztjmj_unit = GetCell(dt, 16, 2);
                    data.CreateTime = DateTime.Now;
                    data.IsDelete = false;
                    shipDataRepo.Add(data);
                    uow.Save();

                    ships = shipRepo.Find(a => !(a.IsDelete == true) && a.IsConfirm == true).ToList();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return ships;
        }

        /// <summary>
        /// 上传可作业船舶
        /// </summary>
        public List<Library_Ship_KZY> CreateLibraryKZYShip(CloudWindRequest request, IFormFile file)
        {
            List<Library_Ship_KZY> ships = new List<Library_Ship_KZY>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var shipRepo = uow.GetRepository<Library_Ship_KZY>();

                    var existShip = shipRepo.Find(a => !(a.IsDelete == true) && a.ShipName.Equals(request.ShipName)).FirstOrDefault();
                    if (existShip != null) throw new Exception("船库中已存在此船名");

                    string fileSavePath = Path.Combine(_env.WebRootPath, "File", "Project", "KZYShipFile");
                    string filePath = Path.Combine(fileSavePath, Guid.NewGuid() + Path.GetExtension(file.FileName));
                    FileUtils.SaveFile(file, filePath);

                    Library_Ship_KZY ship = new Library_Ship_KZY();
                    ship.ShipName = request.ShipName;
                    ship.CreateTime = DateTime.Now;
                    ship.IsDelete = false;
                    shipRepo.Add(ship);
                    uow.Save();

                    ships = shipRepo.Find(a => !(a.IsDelete == true)).ToList();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return ships;
        }

        /// <summary>
        /// 生成平台船文件excel
        /// </summary>
        public List<CloudProjectFile> CreateShipFile(CloudWindRequest request)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var viewShipRepo = uow.GetRepository<View_Library_Ship>();
                    var shipFileRepo = uow.GetRepository<Flow_Task_ShipFile>();

                    string fileSavePath = GetFileSavePath(request.TaskID, request.FlowHandle);
                    string fileDownloadPath = GetFileDownloadPath(request.TaskID, request.FlowHandle);

                    var viewShips = viewShipRepo.Find(a => request.ShipIDs.Contains(a.ID)).ToList();
                    List<Flow_Task_ShipFile> files = new List<Flow_Task_ShipFile>();

                    foreach (var a in viewShips)
                    {
                        DataTable dt = new DataTable();
                        dt.TableName = "VesselData";
                        dt.Columns.Add("船舶参数");
                        dt.Columns.Add("数值");
                        dt.Columns.Add("单位");

                        dt.Rows.Add("桩腿截面积", a.ztjmj, a.ztjmj_unit);
                        dt.Rows.Add("桩腿周长", a.ztzc, a.ztzc_unit);
                        dt.Rows.Add("桩靴长度 L", a.zxcdL, a.zxcdL_unit);
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

                        DataSet ds = new DataSet();
                        ds.Tables.Add(dt);

                        string fileTempName = a.ShipName + "(" + DateTime.Now.ToString("yyyyMMddHHmmss") + ").xlsx";
                        string filePath = fileSavePath + fileTempName;

                        bool success = ExcelUtils.DataSetToExcel(ds, filePath, false);
                        if (success)
                        {
                            list.Add(new CloudProjectFile()
                            {
                                FileName = fileTempName,
                                FilePath = fileDownloadPath + fileTempName
                            });

                            files.Add(new Flow_Task_ShipFile()
                            {
                                TaskID = request.TaskID,
                                FileName = fileTempName,
                                FilePath = filePath,
                                CreateTime = DateTime.Now,
                                IsDelete = false
                            });
                        }
                    }

                    // 软删除旧的船舶文件记录
                    shipFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList()
                        .ForEach(a => { a.IsDelete = true; });

                    // 插入新的船舶文件记录
                    shipFileRepo.AddList(files);
                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return list;
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
                        a.ProjectStartTime <= DateTime.Now && a.ProjectEndTime >= DateTime.Now)
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
        public async Task<(string url, string msg)> TaskSubmit_CBZ(CloudWindRequest request)
        {
            string url = "";
            string msg = "";
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
                            return ("", "没有设置节点人员");
                        }
                    }

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (project == null)
                    {
                        return ("", "项目信息错误");
                    }

                    DateTime now = DateTime.Now;

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
                    string fileImportFolder = Path.Combine(_env.WebRootPath, "File", "Project", project.ProjectCode, "Task", newTask.TaskCode, "Import");
                    FileUtils.CreateFileFolder(fileImportFolder);
                    string fileExportFolder = Path.Combine(_env.WebRootPath, "File", "Project", project.ProjectCode, "Task", newTask.TaskCode, "Export");
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

                    var dkFileTempName = "地勘计算文件(" + DateTime.Now.ToString("yyyyMMddHHmmss") + ").xlsx";
                    var dkFilePath = fileImportFolder + @"/" + dkFileTempName;

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
                            CreateTime = DateTime.Now,
                            TaskID = newTaskID,
                            IsDelete = false
                        });
                    }
                    else
                    {
                        return ("", "生成地勘资料时发生错误。");
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

                        var shipFileTempName = shipName + "(" + DateTime.Now.ToString("yyyyMMddHHmmss") + ").xlsx";
                        var shipFilePath = fileImportFolder + @"/" + shipFileTempName;

                        // TODO: ExcelUtils.DataSetToExcel 已替换
                        var shipSuccess = ExcelUtils.DataSetToExcel(dsShip, shipFilePath, false);
                        if (shipSuccess)
                        {
                            shipFiles.Add(new Flow_Task_ShipFile()
                            {
                                TaskID = newTaskID,
                                FileName = shipName + ".xlsx",
                                FilePath = shipFilePath,
                                CreateTime = DateTime.Now,
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
                        LoggerUtils.Info(url, typeof(TaskService));

                        // TODO: NetUtils HttpGet 已替换为 HttpUtils.GetSync
                        string retStr = HttpUtils.GetSync(url);
                        LoggerUtils.Info(retStr, typeof(TaskService));

                        string message = GetMessageCBZ(retStr);

                        if (string.IsNullOrEmpty(message))
                        {
                            LoggerUtils.Error("插拔桩计算成功", typeof(TaskService));
                            cbzSuccess = true;
                        }
                        else
                        {
                            LoggerUtils.Error(message, typeof(TaskService));
                            return ("", "插拔装计算发生错误：" + message);
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
                                    Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n" + "点击查看详情"
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
                return ("", "发生错误");
            }
            return (url, msg);
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
                                    string fileName = a.ReportPath.Substring(a.ReportPath.LastIndexOf('/') + 1);
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
                                    string fileName = a.ReportPath.Substring(a.ReportPath.LastIndexOf('/') + 1);
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

                    DateTime now = DateTime.Now;

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
                            string fileImportFolder = Path.Combine(_env.WebRootPath, "File", "Project", project.ProjectCode, "Task", thisTask.TaskCode, "Import");

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
                            var dkFileTempName = "地勘计算文件(" + DateTime.Now.ToString("yyyyMMddHHmmss") + ").xlsx";
                            var dkFilePath = fileImportFolder + @"/" + dkFileTempName;
                            var dkSuccess = ExcelUtils.DataSetToExcel(dsDK, dkFilePath, true);
                            if (dkSuccess)
                            {
                                dkFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList()
                                    .ForEach(a => { a.IsDelete = true; });
                                dkFileRepo.Add(new Flow_Task_DKFile() { FileName = dkFileTempName, FilePath = dkFilePath, CreateTime = DateTime.Now, TaskID = thisTask.ID, IsDelete = false });
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
                                var shipFileTempName = s.ShipName + "(" + DateTime.Now.ToString("yyyyMMddHHmmss") + ").xlsx";
                                var shipFilePath = fileImportFolder + @"/" + shipFileTempName;
                                var shipSuccess = ExcelUtils.DataSetToExcel(dsShip, shipFilePath, false);
                                if (shipSuccess)
                                    shipFiles.Add(new Flow_Task_ShipFile() { TaskID = thisTask.ID, FileName = s.ShipName + ".xlsx", FilePath = shipFilePath, CreateTime = DateTime.Now, IsDelete = false });
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
                                        TaskID = request.TaskID,
                                        NodeID = nextNode.ID,
                                        FlowOrder = flow.FlowOrder + 1,
                                        NodeUserName = request.TaskNodeName ?? "",
                                        NodeUserCode = request.TaskNodeUserCode ?? "",
                                        NodeUserDepart = request.TaskNodeDepart ?? "",
                                        NodeUserPhone = request.TaskNodePhone ?? "",
                                        NodeUserJobName = request.TaskNodeJobName ?? "",
                                        FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                        Comment = "",
                                        CreateTime = now,
                                        IsDelete = false
                                    });
                                    informers.Add(request.TaskNodeUserCode);
                                }
                                else
                                {
                                    nodeManageUsers.ForEach(a =>
                                    {
                                        flowRepo.Add(new Flow_TaskApply()
                                        {
                                            TaskID = request.TaskID,
                                            NodeID = nextNode.ID,
                                            FlowOrder = flow.FlowOrder + 1,
                                            NodeUserName = a.ManageName,
                                            NodeUserCode = a.ManageUserCode,
                                            NodeUserDepart = a.ManageDepart,
                                            NodeUserPhone = a.ManagePhone,
                                            NodeUserJobName = a.ManageJobName,
                                            FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                            Comment = "",
                                            CreateTime = now,
                                            IsDelete = false
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
                                                TaskID = request.TaskID,
                                                NodeID = nextNode.ID,
                                                FlowOrder = flow.FlowOrder + 1,
                                                NodeUserName = a.ManageName ?? "",
                                                NodeUserCode = a.ManageUserCode ?? "",
                                                NodeUserDepart = a.ManageDepart ?? "",
                                                NodeUserPhone = a.ManagePhone ?? "",
                                                NodeUserJobName = a.ManageJobName ?? "",
                                                FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                                Comment = "",
                                                CreateTime = now,
                                                IsDelete = false
                                            });
                                            informers.Add(a.ManageUserCode);
                                        });
                                    }
                                    else
                                    {
                                        flowRepo.Add(new Flow_TaskApply()
                                        {
                                            TaskID = request.TaskID,
                                            NodeID = nextNode.ID,
                                            FlowOrder = flow.FlowOrder + 1,
                                            NodeUserName = request.TaskNodeName ?? "",
                                            NodeUserCode = request.TaskNodeUserCode ?? "",
                                            NodeUserDepart = request.TaskNodeDepart ?? "",
                                            NodeUserPhone = request.TaskNodePhone ?? "",
                                            NodeUserJobName = request.TaskNodeJobName ?? "",
                                            FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                            Comment = "",
                                            CreateTime = now,
                                            IsDelete = false
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
                                                    TaskID = request.TaskID,
                                                    NodeID = nextNode.ID,
                                                    FlowOrder = flow.FlowOrder + 1,
                                                    NodeUserName = a.ManageName ?? "",
                                                    NodeUserCode = a.ManageUserCode ?? "",
                                                    NodeUserDepart = a.ManageDepart ?? "",
                                                    NodeUserPhone = a.ManagePhone ?? "",
                                                    NodeUserJobName = a.ManageJobName ?? "",
                                                    FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                                    Comment = "",
                                                    CreateTime = now,
                                                    IsDelete = false
                                                });
                                                informers.Add(a.ManageUserCode);
                                            });
                                        }
                                        else
                                        {
                                            flowRepo.Add(new Flow_TaskApply()
                                            {
                                                TaskID = request.TaskID,
                                                NodeID = nextNode.ID,
                                                FlowOrder = flow.FlowOrder + 1,
                                                NodeUserName = request.TaskNodeName ?? "",
                                                NodeUserCode = request.TaskNodeUserCode ?? "",
                                                NodeUserDepart = request.TaskNodeDepart ?? "",
                                                NodeUserPhone = request.TaskNodePhone ?? "",
                                                NodeUserJobName = request.TaskNodeJobName ?? "",
                                                FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                                Comment = "",
                                                CreateTime = now,
                                                IsDelete = false
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
                                Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
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
                                        Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
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
                return url;
            }
            return (url);
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

                    DateTime now = DateTime.Now;

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
                                Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
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
                task.CreateTime = DateTime.Now;
                uow.Save();
            }
        }

        #endregion


        #region 可作业预报

        /// <summary>
        /// 任务提交（可作业性预报）
        /// </summary>
        public async Task TaskSubmit_KZY(CloudWindRequest request)
        {
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var nodeUserRepo = uow.GetRepository<Flow_NodeManageUser>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_KZY>();
                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();

                    var nodeManagers = nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == (int)CloudWindSoftwareKZYFlowNode.技术人员审核).ToList();
                    if (nodeManagers == null || nodeManagers.Count == 0)
                    {
                        if (string.IsNullOrEmpty(request.TaskNodeName) || string.IsNullOrEmpty(request.TaskNodeUserCode))
                        {
                            throw new Exception("没有设置节点人员");
                        }
                    }

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("项目信息错误");

                    DateTime now = DateTime.Now;

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
                    newTask.DeliverTime = now;
                    newTask.CreateTime = now;
                    newTask.IsDelete = false;
                    taskRepo.Add(newTask);
                    uow.Save();

                    var newTaskID = newTask.ID;

                    // 3.表单数据
                    var newTaskInfo = new Wind_TaskInfo_KZY();
                    newTaskInfo.TaskID = newTaskID;
                    newTaskInfo.ShipIDs = request.ShipIDstring;
                    newTaskInfo.TechType = null;
                    newTaskInfo.ForecastStartTime = Convert.ToDateTime(request.KZYForecastStartTime);
                    newTaskInfo.ForecastEndTime = Convert.ToDateTime(request.KZYForecastEndTime);
                    newTaskInfo.Lon = "";
                    newTaskInfo.Lat = "";
                    newTaskInfo.YFYNo = "";
                    newTaskInfo.Balance = "";
                    newTaskInfo.CreateTime = now;
                    newTaskInfo.IsDelete = false;
                    taskInfoRepo.Add(newTaskInfo);

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

                    List<string> informers = new List<string>();

                    // 7.生成流程
                    if (nodeManagers == null || nodeManagers.Count == 0)
                    {
                        flowRepo.Add(new Flow_TaskApply()
                        {
                            TaskID = newTaskID,
                            NodeID = (int)CloudWindSoftwareKZYFlowNode.技术人员审核,
                            FlowOrder = 1,
                            NodeUserCode = request.TaskNodeUserCode,
                            NodeUserName = request.TaskNodeName,
                            NodeUserDepart = request.TaskNodeDepart,
                            NodeUserPhone = request.TaskNodePhone,
                            NodeUserJobName = request.TaskNodeJobName,
                            Comment = request.TaskComment ?? "",
                            FlowHandle = (int)CloudWindTaskFlowStatus.审批中,
                            CreateTime = now,
                            IsDelete = false
                        });
                        informers.Add(request.TaskNodeUserCode);
                    }
                    else
                    {
                        nodeManagers.ForEach(a =>
                        {
                            flowRepo.Add(new Flow_TaskApply()
                            {
                                TaskID = newTaskID,
                                NodeID = (int)CloudWindSoftwareKZYFlowNode.技术人员审核,
                                FlowOrder = 1,
                                NodeUserCode = a.ManageUserCode,
                                NodeUserName = a.ManageName,
                                NodeUserDepart = a.ManageDepart,
                                NodeUserPhone = a.ManagePhone,
                                NodeUserJobName = a.ManageJobName,
                                Comment = request.TaskComment ?? "",
                                FlowHandle = (int)CloudWindTaskFlowStatus.审批中,
                                CreateTime = now,
                                IsDelete = false
                            });
                            informers.Add(a.ManageUserCode);
                        });
                    }

                    uow.Save();

                    // 交建通通知
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
                            Title = project.ProjectName + "-起重船基础施工可作业性预报流程审批",
                            Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                        });
                    }
                    _cloudWindInfoService.SendJJTMessage(informUsers);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
            }
        }

        /// <summary>
        /// 任务查询（可作业性预报）
        /// </summary>
        public CloudWindTaskFlow TaskQuery_KZY(CloudWindRequest request)
        {
            var data = new CloudWindTaskFlow();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_KZY>();
                    var commentFileRepo = uow.GetRepository<Flow_Task_CommentFile>();

                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (task == null) throw new Exception("工作任务数据错误");

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == task.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("工作任务数据错误");

                    var deliver = deliverRepo.Find(a => !a.IsDelete && a.TaskID == request.TaskID).FirstOrDefault();
                    if (deliver == null) throw new Exception("工作任务数据错误");

                    var taskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).FirstOrDefault();
                    if (taskInfo == null) throw new Exception("工作任务数据错误");

                    // 1.基础信息
                    data.TaskID = task.ID;
                    data.ProjectID = task.ProjectID;
                    data.TaskName = task.TaskName;

                    // 2.可作业表单数据
                    data.TaskInfo_KZY.ForecastStartTime = taskInfo.ForecastStartTime == null ? "" : taskInfo.ForecastStartTime.Value.ToString("yyyy-MM-dd");
                    data.TaskInfo_KZY.ForecastEndTime = taskInfo.ForecastEndTime == null ? "" : taskInfo.ForecastEndTime.Value.ToString("yyyy-MM-dd");
                    data.TaskInfo_KZY.Lon = taskInfo.Lon ?? "";
                    data.TaskInfo_KZY.Lat = taskInfo.Lat ?? "";
                    data.TaskInfo_KZY.YFYNo = taskInfo.YFYNo ?? "";
                    data.TaskInfo_KZY.Balance = taskInfo.Balance ?? "";
                    if (!string.IsNullOrEmpty(taskInfo.ShipIDs))
                    {
                        foreach (string shipId in taskInfo.ShipIDs.Split(','))
                        {
                            data.TaskInfo_KZY.ShipIDs.Add(Convert.ToInt32(shipId));
                        }
                    }

                    // 流程信息
                    var flowInfo = GetTaskFlow(task.ID, UserCode);
                    if (!flowInfo.Success)
                    {
                        throw new Exception(flowInfo.Message);
                    }
                    data.FlowInfo = flowInfo;

                    // 上一节点评论文件
                    commentFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == task.ID).ToList()
                        .ForEach(a =>
                        {
                            data.LastNodeCommentFiles.Add(new CloudProjectFile()
                            {
                                FileName = a.FileName,
                                FilePath = "/File/Project/" + project.ProjectCode + "/Task/" + task.TaskCode + @"/Import/" + a.FileName,
                            });
                        });
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
        /// 审批提交（可作业性预报）
        /// </summary>
        public async Task TaskApprovalSubmit_KZY(CloudWindRequest request)
        {
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_KZY>();
                    var nodeRepo = uow.GetRepository<Flow_Node>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();
                    var nodeUserRepo = uow.GetRepository<Flow_NodeManageUser>();
                    var copyerRepo = uow.GetRepository<Manage_Copyer>();

                    DateTime now = DateTime.Now;

                    var thisTask = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (thisTask == null) throw new Exception("任务数据错误");

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == thisTask.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("任务数据错误");

                    var taskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).FirstOrDefault();
                    if (taskInfo == null) throw new Exception("任务数据错误");

                    // 获取流程信息
                    var flowInfo = GetTaskFlow(thisTask.ID, UserCode);
                    if (!flowInfo.Success) throw new Exception(flowInfo.Message);

                    var taskFlows = flowRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList();
                    var latestFlow = taskFlows.OrderByDescending(a => a.CreateTime).FirstOrDefault();
                    if (latestFlow == null) throw new Exception("流程数据错误");

                    var flow = taskFlows.FirstOrDefault(a => a.NodeID == latestFlow.NodeID && a.FlowOrder == latestFlow.FlowOrder && a.NodeUserCode == UserCode);
                    if (flow == null) throw new Exception("流程数据错误");

                    List<string> informers = new List<string>();
                    bool isFlowFinished = false;

                    // 流程处理
                    if (request.ApprovalType == (int)CloudWindFlowHandle.审批退回)
                    {
                        flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.退回;
                        flow.ApprovalTime = now;
                        flow.Comment = request.TaskComment;
                        thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.退回;

                        if (flow.FlowOrder == 1)
                        {
                            flowRepo.Add(new Flow_TaskApply()
                            {
                                TaskID = request.TaskID,
                                NodeID = flow.NodeID - 1,
                                FlowOrder = flow.FlowOrder + 1,
                                NodeUserName = thisTask.Applyer,
                                NodeUserCode = thisTask.ApplyerCode,
                                NodeUserDepart = thisTask.ApplyerDepart,
                                NodeUserPhone = thisTask.ApplyerPhone,
                                NodeUserJobName = thisTask.ApplyerJobName,
                                FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批,
                                Comment = "",
                                CreateTime = now,
                                IsDelete = false
                            });
                            informers.Add(thisTask.ApplyerCode);
                        }
                        else
                        {
                            taskFlows.Where(a => a.FlowOrder == (flow.FlowOrder - 1) && a.NodeID == (flow.NodeID - 1)).ToList()
                                .ForEach(a =>
                                {
                                    flowRepo.Add(new Flow_TaskApply()
                                    {
                                        TaskID = request.TaskID,
                                        NodeID = flow.NodeID - 1,
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
                                    informers.Add(thisTask.ApplyerCode);
                                });
                        }
                    }
                    else if (request.ApprovalType == (int)CloudWindFlowHandle.审批通过)
                    {
                        flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.审批通过;
                        flow.ApprovalTime = now;
                        flow.Comment = request.TaskComment ?? "";

                        var nextNode = nodeRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.起重船基础施工可作业性预报 && a.ID == (flow.NodeID + 1)).FirstOrDefault();
                        var nodeManageUsers = new List<Flow_NodeManageUser>();
                        if (nextNode != null)
                        {
                            nodeManageUsers = nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == nextNode.ID).ToList();
                        }

                        var thisFlows = taskFlows.Where(a => a.NodeID == flow.NodeID && a.FlowOrder == flow.FlowOrder && a.id != flow.id && a.FlowHandle == (int)CloudWindTaskFlowHandleStatus.待审批).ToList();

                        // 表单内容
                        if (string.IsNullOrEmpty(request.KZYForecastStartTime) || string.IsNullOrEmpty(request.KZYForecastEndTime))
                            throw new Exception("预报时间不能为空");

                        if (nextNode != null)
                        {
                            if (nextNode.ID == (int)CloudWindSoftwareKZYFlowNode.技术人员审核)
                            {
                                if (string.IsNullOrEmpty(request.ShipIDstring))
                                    throw new Exception("没有选择浮式船舶");
                                taskInfo.ShipIDs = request.ShipIDstring ?? "";
                                taskInfo.ForecastStartTime = Convert.ToDateTime(request.KZYForecastStartTime);
                                taskInfo.ForecastEndTime = Convert.ToDateTime(request.KZYForecastEndTime);
                            }
                            else if (nextNode.ID == (int)CloudWindSoftwareKZYFlowNode.审核通过)
                            {
                                if (string.IsNullOrEmpty(request.KZYLon) || string.IsNullOrEmpty(request.KZYLat))
                                    throw new Exception("没有输入经纬度");
                                if (string.IsNullOrEmpty(request.KZYBalance))
                                    throw new Exception("没有输入衡准");
                                if (string.IsNullOrEmpty(request.KZYYFYNo))
                                    throw new Exception("没有输入亿方云编号");
                                taskInfo.ForecastStartTime = Convert.ToDateTime(request.KZYForecastStartTime);
                                taskInfo.ForecastEndTime = Convert.ToDateTime(request.KZYForecastEndTime);
                                taskInfo.Lat = request.KZYLat ?? "";
                                taskInfo.Lon = request.KZYLon ?? "";
                                taskInfo.YFYNo = request.KZYYFYNo ?? "";
                                taskInfo.Balance = request.KZYBalance ?? "";
                            }
                        }

                        // 流程相关
                        if (flowInfo.NodeApprovalType)
                        {
                            if (nextNode == null)
                            {
                                thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.完成;
                                isFlowFinished = true;
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
                                            TaskID = request.TaskID,
                                            NodeID = nextNode.ID,
                                            FlowOrder = flow.FlowOrder + 1,
                                            NodeUserName = a.ManageName ?? "",
                                            NodeUserCode = a.ManageUserCode ?? "",
                                            NodeUserDepart = a.ManageDepart ?? "",
                                            NodeUserPhone = a.ManagePhone ?? "",
                                            NodeUserJobName = a.ManageJobName ?? "",
                                            FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                            Comment = "",
                                            CreateTime = now,
                                            IsDelete = false
                                        });
                                        informers.Add(a.ManageUserCode);
                                    });
                                }
                                else
                                {
                                    flowRepo.Add(new Flow_TaskApply()
                                    {
                                        TaskID = request.TaskID,
                                        NodeID = nextNode.ID,
                                        FlowOrder = flow.FlowOrder + 1,
                                        NodeUserName = request.TaskNodeName ?? "",
                                        NodeUserCode = request.TaskNodeUserCode ?? "",
                                        NodeUserDepart = request.TaskNodeDepart ?? "",
                                        NodeUserPhone = request.TaskNodePhone ?? "",
                                        NodeUserJobName = request.TaskNodeJobName ?? "",
                                        FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                        Comment = "",
                                        CreateTime = now,
                                        IsDelete = false
                                    });
                                    informers.Add(request.TaskNodeUserCode);
                                }
                            }
                            if (thisFlows.Count > 0)
                                thisFlows.ForEach(a => { a.IsDelete = true; });
                        }
                        else
                        {
                            if (thisFlows.Count == 0)
                            {
                                if (nextNode == null)
                                {
                                    thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.完成;
                                    isFlowFinished = true;
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
                                                TaskID = request.TaskID,
                                                NodeID = nextNode.ID,
                                                FlowOrder = flow.FlowOrder + 1,
                                                NodeUserName = a.ManageName ?? "",
                                                NodeUserCode = a.ManageUserCode ?? "",
                                                NodeUserDepart = a.ManageDepart ?? "",
                                                NodeUserPhone = a.ManagePhone ?? "",
                                                NodeUserJobName = a.ManageJobName ?? "",
                                                FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                                Comment = "",
                                                CreateTime = now,
                                                IsDelete = false
                                            });
                                            informers.Add(a.ManageUserCode);
                                        });
                                    }
                                    else
                                    {
                                        flowRepo.Add(new Flow_TaskApply()
                                        {
                                            TaskID = request.TaskID,
                                            NodeID = nextNode.ID,
                                            FlowOrder = flow.FlowOrder + 1,
                                            NodeUserName = request.TaskNodeName ?? "",
                                            NodeUserCode = request.TaskNodeUserCode ?? "",
                                            NodeUserDepart = request.TaskNodeDepart ?? "",
                                            NodeUserPhone = request.TaskNodePhone ?? "",
                                            NodeUserJobName = request.TaskNodeJobName ?? "",
                                            FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                            Comment = "",
                                            CreateTime = now,
                                            IsDelete = false
                                        });
                                        informers.Add(request.TaskNodeUserCode);
                                    }
                                }
                            }
                        }
                    }

                    uow.Save();

                    // 交建通通知
                    var informTitle = project.ProjectName + "-起重船基础施工可作业性预报流程审批";
                    if (isFlowFinished)
                    {
                        informTitle = project.ProjectName + "(" + thisTask.TaskCode + ")-起重船基础施工可作业性预报完成";
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
                            Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                        });
                    }
                    await _cloudWindInfoService.SendJJTMessage(informRequests);

                    // 抄送
                    if (isFlowFinished)
                    {
                        List<SystemJJTInform> copyerInformRequests = new List<SystemJJTInform>();
                        copyerRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.起重船基础施工可作业性预报).ToList()
                            .ForEach(a =>
                            {
                                if (!string.IsNullOrEmpty(a.UserCode))
                                {
                                    var token = _cloudCenterService.CreateJJTMessageRedirectToken("Task", a.UserCode, thisTask.ID);
                                    var redirectUrl = cloudPlatUrl + @"Home/JJTMessageRedirect?CloudToken=" + token;
                                    copyerInformRequests.Add(new SystemJJTInform()
                                    {
                                        UserCode = a.UserCode,
                                        Url = redirectUrl,
                                        Title = project.ProjectName + "(" + thisTask.TaskCode + ")-起重船基础施工可作业性预报流程结束",
                                        Content = "点击查看详情"
                                    });
                                }
                            });
                        await _cloudWindInfoService.SendJJTMessage(copyerInformRequests);
                    }

                    // 流程结束，生成推送文件
                    if (isFlowFinished)
                    {
                        _cloudWindKZYService.CreateKZYFile(thisTask.ID);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
            }
        }

        /// <summary>
        /// 生成可作业预报浮式船舶文件
        /// </summary>
        public bool CreateTaskFile_KZY(int taskId)
        {
            bool success = false;
            if (taskId == 0) return success;
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_KZY>();
                    var floatShipRepo = uow.GetRepository<Library_FloatShip>();

                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == taskId).FirstOrDefault();
                    if (task == null) return success;

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == task.ProjectID).FirstOrDefault();
                    if (project == null) return success;

                    var taskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == taskId).FirstOrDefault();
                    if (taskInfo == null) return success;
                    if (string.IsNullOrEmpty(taskInfo.ShipIDs)) return success;

                    var floatShips = floatShipRepo.Find(a => !(a.IsDelete == true)).ToList();

                    string kzyFilePath = Path.Combine(_env.WebRootPath, "File", "Project", project.ProjectCode, "Task", task.TaskCode, "KZY");
                    FileUtils.DeleteFolder(kzyFilePath);
                    FileUtils.CreateFileFolder(kzyFilePath);

                    string[] shipArr = taskInfo.ShipIDs.Split(',');
                    foreach (string id in shipArr)
                    {
                        int shipID = Convert.ToInt32(id);
                        var ship = floatShips.FirstOrDefault(a => a.id == shipID);
                        if (ship == null) continue;

                        string shipFolderPath = kzyFilePath + @"/" + ship.ShipName;
                        FileUtils.CreateFileFolder(shipFolderPath);

                        DataTable dt = new DataTable();
                        dt.TableName = "浮式船舶参数";
                        dt.Columns.Add("栏目");
                        dt.Columns.Add("数值");
                        dt.Columns.Add("备注");

                        dt.Rows.Add("船名", ship.ShipName, "");
                        dt.Rows.Add("项目号", project.ProjectCode, "");
                        dt.Rows.Add("项目名", project.ProjectName, "");
                        dt.Rows.Add("任务号", task.TaskCode, "");
                        dt.Rows.Add("船名", ship.ShipName, "");
                        dt.Rows.Add("水深（m）", null, "");
                        dt.Rows.Add("经度", taskInfo.Lon, "");
                        dt.Rows.Add("纬度", taskInfo.Lat, "");
                        dt.Rows.Add("衡准", null, "");
                        dt.Rows.Add("存储路径", shipFolderPath, "");
                        dt.Rows.Add("起始时间", taskInfo.ForecastStartTime?.ToString("yyyyMMdd"), "");
                        dt.Rows.Add("结束时间", taskInfo.ForecastEndTime?.ToString("yyyyMMdd"), "");

                        string fullFilePath = shipFolderPath + @"/" + ship.ShipName + ".csv";
                        ExcelUtils.SaveCSV(dt, fullFilePath);
                    }

                    success = true;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                success = false;
            }
            return success;
        }

        #endregion



        #region 桩基沉桩

        /// <summary>
        /// 桩基沉桩文件上传
        /// </summary>
        public List<CloudProjectFile> TaskPileFileUpload(CloudWindRequest request, List<IFormFile> files)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var pileRepo = uow.GetRepository<Library_Pile>();
                    var pileDataRepo = uow.GetRepository<Library_Pile_Data>();

                    string fileFolderPath = "";
                    if (request.FlowHandle == "new")
                    {
                        fileFolderPath = Path.Combine(_env.WebRootPath, "File", "Project", "ZJCZTempFile");
                    }
                    else
                    {
                        if (request.TaskID == 0) throw new Exception("任务数据错误");

                        var task = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                        if (task == null) throw new Exception("任务数据错误");

                        var project = projectRepo.Find(a => !a.IsDelete && a.ID == task.ProjectID).FirstOrDefault();
                        if (project == null) throw new Exception("任务数据错误");

                        fileFolderPath = Path.Combine(_env.WebRootPath, "File", "Project", project.ProjectCode.ToUpper(), "Task", task.TaskCode, "Import");
                    }

                    // 如果是桩数据表，删除原来的数据
                    if (request.FileType == (int)CloudWindTaskFileType.桩基沉桩_桩数据表)
                    {
                        pileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID && a.Type == (int)CloudWindTaskFileType.桩基沉桩_桩数据表).ToList()
                            .ForEach(a =>
                            {
                                a.IsDelete = true;
                                a.CreateTime = DateTime.Now;
                            });
                    }

                    int newDataId = 0;
                    List<Library_Pile> pileList = new List<Library_Pile>();
                    foreach (var file in files)
                    {
                        if (file != null)
                        {
                            Library_Pile model = new Library_Pile();
                            model.TaskID = request.TaskID;
                            model.Type = request.FileType;
                            model.FileName = file.FileName;
                            model.FilePath = fileFolderPath + @"/" + Guid.NewGuid() + Path.GetExtension(file.FileName);
                            model.CreateTime = DateTime.Now;
                            model.IsDelete = false;
                            pileList.Add(model);

                            FileUtils.SaveFile(file, model.FilePath);
                        }
                    }
                    pileRepo.AddList(pileList);
                    uow.Save();

                    var newData = pileRepo.Find(a => true).OrderByDescending(a => a.id).FirstOrDefault();
                    if (newData != null) newDataId = newData.id;

                    // 导入数据表
                    if (request.FileType == (int)CloudWindTaskFileType.桩基沉桩_桩数据表 && newDataId != 0)
                    {
                        var pileEntity = pileRepo.FindByID(newDataId);
                        if (pileEntity != null && !string.IsNullOrEmpty(pileEntity.FilePath))
                        {
                            DataSet ds = ExcelUtils.ExcelToTableForXLSX(pileEntity.FilePath);
                            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                            {
                                foreach (DataTable dt in ds.Tables)
                                {
                                    string pileName = dt.TableName.Trim();
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        Library_Pile_Data data = new Library_Pile_Data();
                                        data.PileID = newDataId;
                                        data.PileName = pileName;
                                        data.mdcd = dr[0].ToString().Trim();
                                        data.zc = dr[1].ToString().Trim();
                                        data.fzzwj = dr[2].ToString().Trim();
                                        data.bh = dr[3].ToString().Trim();
                                        data.zzl = dr[5].ToString().Trim();
                                        data.zdgcT = dr[6].ToString().Trim();
                                        data.zcm = dr[7].ToString().Trim();
                                        data.zdgcB = dr[8].ToString().Trim();
                                        data.CreateTime = DateTime.Now;
                                        data.IsDelete = false;
                                        pileDataRepo.Add(data);
                                    }
                                }
                                uow.Save();
                            }
                        }
                    }

                    pileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID && a.Type == request.FileType).ToList()
                        .ForEach(a =>
                        {
                            list.Add(new CloudProjectFile()
                            {
                                ID = a.id,
                                FileName = a.FileName,
                                FileTime = DateTime.Now.ToString("yyyy-MM-dd"),
                                FilePath = FileUtils.GetRelativeFilePath(a.FilePath)
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                list = new List<CloudProjectFile>();
                throw new Exception("发生错误,请检查桩数据表格式。");
            }
            return list;
        }

        /// <summary>
        /// 桩基沉桩位置图片上传
        /// </summary>
        public List<CloudProjectFile> TaskPilePositionFileUpload(CloudWindRequest request, List<IFormFile> files)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var imgRepo = uow.GetRepository<Wind_TaskInfoImg_ZJCZ>();

                    string fileFolderPath = "";
                    if (request.FlowHandle == "new")
                    {
                        fileFolderPath = Path.Combine(_env.WebRootPath, "File", "Project", "ZJCZTempFile");
                    }
                    else
                    {
                        if (request.TaskID == 0) throw new Exception("任务数据错误");

                        var task = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                        if (task == null) throw new Exception("任务数据错误");

                        var project = projectRepo.Find(a => !a.IsDelete && a.ID == task.ProjectID).FirstOrDefault();
                        if (project == null) throw new Exception("任务数据错误");

                        fileFolderPath = Path.Combine(_env.WebRootPath, "File", "Project", project.ProjectCode.ToUpper(), "Task", task.TaskCode, "Import");
                    }

                    List<Wind_TaskInfoImg_ZJCZ> fileList = new List<Wind_TaskInfoImg_ZJCZ>();
                    foreach (var file in files)
                    {
                        if (file != null)
                        {
                            Wind_TaskInfoImg_ZJCZ model = new Wind_TaskInfoImg_ZJCZ();
                            model.TaskID = request.TaskID;
                            model.FileName = file.FileName;
                            model.FilePath = fileFolderPath + @"/" + Guid.NewGuid() + Path.GetExtension(file.FileName);
                            model.CreateTime = DateTime.Now;
                            model.IsDelete = false;
                            fileList.Add(model);

                            FileUtils.SaveFile(file, model.FilePath);
                        }
                    }
                    imgRepo.AddList(fileList);
                    uow.Save();

                    imgRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList()
                        .ForEach(a =>
                        {
                            list.Add(new CloudProjectFile()
                            {
                                ID = a.id,
                                FileName = a.FileName,
                                FileTime = DateTime.Now.ToString("yyyy-MM-dd"),
                                FilePath = FileUtils.GetRelativeFilePath(a.FilePath)
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                list = new List<CloudProjectFile>();
                throw new Exception("发生错误,请检查桩数据表格式。");
            }
            return list;
        }

        /// <summary>
        /// 删除项目位置图片（桩基沉桩）
        /// </summary>
        public List<CloudProjectFile> DeleteProjectPositionFile(CloudWindRequest request)
        {
            List<CloudProjectFile> files = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var imgRepo = uow.GetRepository<Wind_TaskInfoImg_ZJCZ>();

                    var data = imgRepo.Find(a => !(a.IsDelete == true) && a.id == request.ID).FirstOrDefault();
                    if (data == null) throw new Exception("数据错误");

                    data.CreateTime = DateTime.Now;
                    data.IsDelete = true;
                    uow.Save();

                    imgRepo.Find(a => !(a.IsDelete == true) && a.TaskID == data.TaskID).ToList().ForEach(a =>
                    {
                        files.Add(new CloudProjectFile()
                        {
                            ID = a.id,
                            FileName = a.FileName,
                            FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                            FileTime = a.CreateTime?.ToString("yyyy-MM-dd")
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return files;
        }

        /// <summary>
        /// 删除桩文件（桩基沉桩）
        /// </summary>
        public List<CloudProjectFile> DeletePileFile(CloudWindRequest request)
        {
            List<CloudProjectFile> files = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var pileRepo = uow.GetRepository<Library_Pile>();

                    var data = pileRepo.Find(a => !(a.IsDelete == true) && a.id == request.ID).FirstOrDefault();
                    if (data == null) throw new Exception("数据错误");

                    data.CreateTime = DateTime.Now;
                    data.IsDelete = true;
                    uow.Save();

                    pileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == data.TaskID && a.Type == data.Type).ToList().ForEach(a =>
                    {
                        files.Add(new CloudProjectFile()
                        {
                            ID = a.id,
                            FileName = a.FileName,
                            FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                            FileTime = a.CreateTime?.ToString("yyyy-MM-dd")
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return files;
        }

        /// <summary>
        /// 任务提交（桩基沉桩）
        /// </summary>
        public async Task TaskSubmit_ZJCZ(CloudWindRequest request)
        {
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var nodeUserRepo = uow.GetRepository<Flow_NodeManageUser>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_ZJCZ>();
                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();
                    var imgRepo = uow.GetRepository<Wind_TaskInfoImg_ZJCZ>();
                    var pileRepo = uow.GetRepository<Library_Pile>();
                    var geologyRepo = uow.GetRepository<Library_Geology>();

                    var nodeManagers = nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == (int)CloudWindSoftwareZJCZFlowNode.技术审核).ToList();
                    if (nodeManagers == null || nodeManagers.Count == 0)
                    {
                        if (string.IsNullOrEmpty(request.TaskNodeName) || string.IsNullOrEmpty(request.TaskNodeUserCode))
                            throw new Exception("没有设置节点人员");
                    }

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("项目信息错误");

                    // 验证桩数据表
                    var pileCount = pileRepo.Find(a => !a.IsDelete.Value && a.TaskID == request.TaskID && a.Type == (int)CloudWindTaskFileType.桩基沉桩_桩数据表).Count();
                    if (pileCount == 0) throw new Exception("没有添加桩数据表");

                    // 验证地勘数据表
                    var dkCount = geologyRepo.Find(a => !a.IsDelete && a.ProjectID == request.ProjectID && a.Type == (int)CloudWindTaskFileType.风电项目_机孔位地勘数据表).Count();
                    if (dkCount == 0) throw new Exception("没有添加地勘数据表");

                    DateTime now = DateTime.Now;

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

                    // 附加表单
                    Wind_TaskInfo_ZJCZ taskInfo = new Wind_TaskInfo_ZJCZ();
                    taskInfo.TaskID = newTaskID;
                    taskInfo.ProjectPosition = request.ZJCZProjectPosition ?? "";
                    taskInfo.ProjectIntroduce = request.ZJCZProjectIntroduce ?? "";
                    taskInfo.zmtd = request.PileReplace ?? "";
                    taskInfo.HammerModel = request.HammerModel ?? "";
                    taskInfo.CreateTime = now;
                    taskInfo.IsDelete = false;
                    taskInfoRepo.Add(taskInfo);

                    // 3.创建文件夹
                    string fileImportFolder = Path.Combine(_env.WebRootPath, "File", "Project", project.ProjectCode, "Task", newTask.TaskCode, "Import");
                    FileUtils.CreateFileFolder(fileImportFolder);
                    string fileExportFolder = Path.Combine(_env.WebRootPath, "File", "Project", project.ProjectCode, "Task", newTask.TaskCode, "Export");
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

                    List<string> informers = new List<string>();

                    // 7.生成流程
                    if (nodeManagers == null || nodeManagers.Count == 0)
                    {
                        var newFlow = new Flow_TaskApply();
                        newFlow.TaskID = newTaskID;
                        newFlow.NodeID = (int)CloudWindSoftwareZJCZFlowNode.技术审核;
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
                            newFlow.NodeID = (int)CloudWindSoftwareZJCZFlowNode.技术审核;
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

                    // 8.将文件移动到所属文件夹
                    //（1）项目位置文件
                    imgRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList()
                        .ForEach(a =>
                        {
                            string fileName = a.FilePath.Substring(a.FilePath.LastIndexOf('/') + 1);
                            FileUtils.MoveFile(a.FilePath, fileImportFolder, fileName);
                            a.TaskID = newTaskID;
                            a.FilePath = fileImportFolder + @"/" + fileName;
                        });
                    //（2）桩数据
                    pileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList()
                        .ForEach(a =>
                        {
                            string fileName = a.FilePath.Substring(a.FilePath.LastIndexOf('/') + 1);
                            FileUtils.MoveFile(a.FilePath, fileImportFolder, fileName);
                            a.TaskID = newTaskID;
                            a.FilePath = fileImportFolder + @"/" + fileName;
                        });

                    uow.Save();

                    // 交建通通知
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
                            Title = project.ProjectName + "(" + newTask.TaskCode + ")-桩基沉桩可打性分析",
                            Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n" + "点击查看详情"
                        });
                    }
                    _cloudWindInfoService.SendJJTMessage(informUsers);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
        }

        /// <summary>
        /// 任务查询（桩基沉桩）
        /// </summary>
        public CloudWindTaskFlow TaskQuery_ZJCZ(CloudWindRequest request)
        {
            CloudWindTaskFlow data = new CloudWindTaskFlow();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_ZJCZ>();
                    var imgRepo = uow.GetRepository<Wind_TaskInfoImg_ZJCZ>();
                    var pileRepo = uow.GetRepository<Library_Pile>();
                    var viewFlowRepo = uow.GetRepository<View_Flow_TaskApply>();
                    var projectRoleRepo = uow.GetRepository<Wind_ProjectRole>();
                    var copyerRepo = uow.GetRepository<Manage_Copyer>();
                    var reportRepo = uow.GetRepository<Wind_TaskReport>();

                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (task == null) throw new Exception("工作任务数据错误");

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == task.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("工作任务数据错误");

                    var deliver = deliverRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).FirstOrDefault();
                    if (deliver == null) throw new Exception("工作任务数据错误");

                    var taskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).FirstOrDefault();
                    if (taskInfo == null) throw new Exception("工作任务数据错误");

                    // 1.基础信息
                    data.TaskID = task.ID;
                    data.ProjectID = task.ProjectID;
                    data.TaskName = task.TaskName;
                    data.TaskStatus = task.FlowStatus ?? 0;
                    data.DeliverTime = task.DeliverTime == null ? "" : task.DeliverTime.Value.ToString("yyyy-MM-dd");

                    data.TaskInfo_ZJCZ.ProjectIntroduce = taskInfo.ProjectIntroduce ?? "";
                    data.TaskInfo_ZJCZ.PileReplace = taskInfo.zmtd ?? "";
                    // 锤型号
                    if (!string.IsNullOrEmpty(taskInfo.HammerModel))
                    {
                        foreach (string hammer in taskInfo.HammerModel.Split(','))
                        {
                            data.TaskInfo_ZJCZ.HammerModels.Add(hammer);
                        }
                    }
                    // 项目位置图片
                    imgRepo.Find(a => !(a.IsDelete == true) && a.TaskID == task.ID).ToList()
                        .ForEach(a =>
                        {
                            data.TaskInfo_ZJCZ.ProjectPositionImgs.Add(new CloudProjectFile()
                            {
                                ID = a.id,
                                FileName = a.FileName,
                                FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                            });
                        });
                    // 桩数据
                    pileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == task.ID).ToList()
                        .ForEach(a =>
                        {
                            if (a.Type == (int)CloudWindTaskFileType.桩基沉桩_桩原始资料)
                            {
                                data.TaskInfo_ZJCZ.File_Pile_YSZL.Add(new CloudProjectFile()
                                {
                                    ID = a.id,
                                    FileName = a.FileName,
                                    FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                    FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                                });
                            }
                            if (a.Type == (int)CloudWindTaskFileType.桩基沉桩_桩数据表)
                            {
                                data.TaskInfo_ZJCZ.File_Pile_SJB.Add(new CloudProjectFile()
                                {
                                    ID = a.id,
                                    FileName = a.FileName,
                                    FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                    FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                                });
                            }
                        });

                    // 流程信息
                    var flowInfo = GetTaskFlow(task.ID, UserCode);
                    if (!flowInfo.Success) throw new Exception(flowInfo.Message);
                    data.FlowInfo = flowInfo;

                    // 文件报告（特定节点人员才能查看）
                    List<string> reportViewer = new List<string>();
                    viewFlowRepo.Find(a => a.TaskID == request.TaskID && a.NodeID == (int)CloudWindSoftwareZJCZFlowNode.技术审核)
                        .GroupBy(a => a.NodeUserCode).ToList()
                        .ForEach(a =>
                        {
                            reportViewer.Add(a.FirstOrDefault().NodeUserCode);
                        });
                    viewFlowRepo.Find(a => a.TaskID == request.TaskID && (a.NodeID == (int)CloudWindSoftwareZJCZFlowNode.业务审核 || a.NodeID == (int)CloudWindSoftwareZJCZFlowNode.审核通过))
                        .GroupBy(a => a.NodeUserCode).ToList()
                        .ForEach(a =>
                        {
                            reportViewer.Add(a.FirstOrDefault().NodeUserCode);
                        });

                    if (task.FlowStatus == (int)CloudWindTaskFlowStatus.完成)
                    {
                        reportViewer.Add(deliver.DeliverCode);
                        // 项目组领导班子
                        projectRoleRepo.Find(a => !a.IsDelete && a.ProjectID == project.ID && a.RoleID == (int)CloudWindProjectRole.项目部领导班子).ToList()
                            .ForEach(a =>
                            {
                                reportViewer.Add(a.UserCode);
                            });
                        // 抄送的领导
                        copyerRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.桩基沉桩可打性分析).ToList()
                            .ForEach(a =>
                            {
                                reportViewer.Add(a.UserCode);
                            });
                    }

                    var reports = reportRepo.Find(a => !(a.IsDelete == true) && a.TaskID == task.ID).ToList();
                    if (reportViewer.Contains(CurrentUser.UserCode))
                    {
                        if (reports != null && reports.Count > 0)
                        {
                            reports.OrderByDescending(a => a.CreateTime).ToList()
                                .ForEach(a =>
                                {
                                    data.Report.ReportFile.Add(new CloudProjectFile()
                                    {
                                        ID = a.id,
                                        FileName = a.ReportName,
                                        FilePath = FileUtils.GetRelativeFilePath(a.ReportPath),
                                        FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
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
        /// 审批提交（桩基沉桩）
        /// </summary>
        public async Task TaskApprovalSubmit_ZJCZ(CloudWindRequest request)
        {
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_ZJCZ>();
                    var nodeRepo = uow.GetRepository<Flow_Node>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();
                    var nodeUserRepo = uow.GetRepository<Flow_NodeManageUser>();
                    var copyerRepo = uow.GetRepository<Manage_Copyer>();

                    DateTime now = DateTime.Now;

                    var thisTask = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (thisTask == null) throw new Exception("任务数据错误");

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == thisTask.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("任务数据错误");

                    var taskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == thisTask.ID).FirstOrDefault();
                    if (taskInfo == null) throw new Exception("任务数据错误");

                    // 获取流程信息
                    var flowInfo = GetTaskFlow(thisTask.ID, UserCode);
                    if (!flowInfo.Success) throw new Exception(flowInfo.Message);

                    var node = nodeRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.桩基沉桩可打性分析 && a.ID == flowInfo.NodeID).FirstOrDefault();

                    var taskFlows = flowRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList();
                    var latestFlow = taskFlows.OrderByDescending(a => a.CreateTime).FirstOrDefault();
                    if (latestFlow == null) throw new Exception("流程数据错误");

                    var flow = taskFlows.FirstOrDefault(a => a.NodeID == latestFlow.NodeID && a.FlowOrder == latestFlow.FlowOrder && a.NodeUserCode == UserCode);
                    if (flow == null) throw new Exception("流程数据错误");

                    List<string> informers = new List<string>();
                    bool isFlowFinished = false;

                    // 流程处理
                    if (request.ApprovalType == (int)CloudWindFlowHandle.审批退回)
                    {
                        flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.退回;
                        flow.ApprovalTime = now;
                        flow.Comment = request.TaskComment;
                        thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.退回;

                        // 退回至申请人
                        flowRepo.Add(new Flow_TaskApply()
                        {
                            TaskID = request.TaskID,
                            NodeID = (int)CloudWindSoftwareZJCZFlowNode.任务申请,
                            FlowOrder = flow.FlowOrder + 1,
                            NodeUserName = thisTask.Applyer,
                            NodeUserCode = thisTask.ApplyerCode,
                            NodeUserDepart = thisTask.ApplyerDepart,
                            NodeUserPhone = thisTask.ApplyerPhone,
                            NodeUserJobName = thisTask.ApplyerJobName,
                            FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批,
                            Comment = "",
                            CreateTime = now,
                            IsDelete = false
                        });
                        informers.Add(thisTask.ApplyerCode);
                    }
                    else if (request.ApprovalType == (int)CloudWindFlowHandle.审批通过)
                    {
                        var nextNode = nodeRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.桩基沉桩可打性分析 && a.NodeNo == (node.NodeNo + 1)).FirstOrDefault();
                        var nodeManageUsers = new List<Flow_NodeManageUser>();
                        if (nextNode != null)
                        {
                            nodeManageUsers = nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == nextNode.ID).ToList();
                        }

                        var thisFlows = taskFlows.Where(a => a.NodeID == flow.NodeID && a.FlowOrder == flow.FlowOrder && a.id != flow.id && a.FlowHandle == (int)CloudWindTaskFlowHandleStatus.待审批).ToList();
                        var lastFlow = taskFlows.Where(a => a.FlowOrder == (flow.FlowOrder - 1)).ToList();

                        if (lastFlow != null && lastFlow.Exists(a => a.FlowHandle == (int)CloudWindTaskFlowHandleStatus.退回))
                        {
                            // 退回后重新提交
                            if (request.TaskID == 0) throw new Exception("任务数据错误");
                            if (string.IsNullOrEmpty(request.TaskName)) throw new Exception("任务名不能为空");
                            if (string.IsNullOrEmpty(request.ZJCZProjectIntroduce)) throw new Exception("项目介绍不能为空");
                            if (string.IsNullOrEmpty(request.HammerModel)) throw new Exception("锤型号不能为空");

                            thisTask.TaskName = request.TaskName ?? "";
                            thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.审批中;
                            taskInfo.ProjectIntroduce = request.ZJCZProjectIntroduce ?? "";
                            taskInfo.ProjectPosition = request.ZJCZProjectPosition ?? "";
                            taskInfo.zmtd = request.PileReplace ?? "";
                            taskInfo.HammerModel = request.HammerModel;

                            flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.审批通过;
                            flow.ApprovalTime = now;
                            flow.Comment = request.TaskComment ?? "";

                            // 新增流程
                            if (nodeManageUsers == null || nodeManageUsers.Count == 0)
                            {
                                if (string.IsNullOrEmpty(request.TaskNodeUserCode))
                                    throw new Exception("没有选择下一节点人员");
                                flowRepo.Add(new Flow_TaskApply()
                                {
                                    TaskID = request.TaskID,
                                    NodeID = nextNode.ID,
                                    FlowOrder = flow.FlowOrder + 1,
                                    NodeUserName = request.TaskNodeName ?? "",
                                    NodeUserCode = request.TaskNodeUserCode ?? "",
                                    NodeUserDepart = request.TaskNodeDepart ?? "",
                                    NodeUserPhone = request.TaskNodePhone ?? "",
                                    NodeUserJobName = request.TaskNodeJobName ?? "",
                                    FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                    Comment = "",
                                    CreateTime = now,
                                    IsDelete = false
                                });
                                informers.Add(request.TaskNodeUserCode);
                            }
                            else
                            {
                                nodeManageUsers.ForEach(a =>
                                {
                                    flowRepo.Add(new Flow_TaskApply()
                                    {
                                        TaskID = request.TaskID,
                                        NodeID = nextNode.ID,
                                        FlowOrder = flow.FlowOrder + 1,
                                        NodeUserName = a.ManageName ?? "",
                                        NodeUserCode = a.ManageUserCode ?? "",
                                        NodeUserDepart = a.ManageDepart ?? "",
                                        NodeUserPhone = a.ManagePhone ?? "",
                                        NodeUserJobName = a.ManageJobName ?? "",
                                        FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                        Comment = "",
                                        CreateTime = now,
                                        IsDelete = false
                                    });
                                    informers.Add(a.ManageUserCode);
                                });
                            }
                        }
                        else
                        {
                            flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.审批通过;
                            flow.ApprovalTime = now;
                            flow.Comment = request.TaskComment ?? "";

                            // 单人审批类型
                            if (flowInfo.NodeApprovalType)
                            {
                                if (nextNode == null)
                                {
                                    thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.完成;
                                    isFlowFinished = true;
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
                                                TaskID = request.TaskID,
                                                NodeID = nextNode.ID,
                                                FlowOrder = flow.FlowOrder + 1,
                                                NodeUserName = a.ManageName ?? "",
                                                NodeUserCode = a.ManageUserCode ?? "",
                                                NodeUserDepart = a.ManageDepart ?? "",
                                                NodeUserPhone = a.ManagePhone ?? "",
                                                NodeUserJobName = a.ManageJobName ?? "",
                                                FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                                Comment = "",
                                                CreateTime = now,
                                                IsDelete = false
                                            });
                                            informers.Add(a.ManageUserCode);
                                        });
                                    }
                                    else
                                    {
                                        flowRepo.Add(new Flow_TaskApply()
                                        {
                                            TaskID = request.TaskID,
                                            NodeID = nextNode.ID,
                                            FlowOrder = flow.FlowOrder + 1,
                                            NodeUserName = request.TaskNodeName ?? "",
                                            NodeUserCode = request.TaskNodeUserCode ?? "",
                                            NodeUserDepart = request.TaskNodeDepart ?? "",
                                            NodeUserPhone = request.TaskNodePhone ?? "",
                                            NodeUserJobName = request.TaskNodeJobName ?? "",
                                            FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                            Comment = "",
                                            CreateTime = now,
                                            IsDelete = false
                                        });
                                        informers.Add(request.TaskNodeUserCode);
                                    }
                                }
                                if (thisFlows.Count > 0)
                                    thisFlows.ForEach(a => { a.IsDelete = true; });
                            }
                            else
                            {
                                // 多人审批
                                if (thisFlows.Count == 0)
                                {
                                    if (nextNode == null)
                                    {
                                        thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.完成;
                                        isFlowFinished = true;
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
                                                    TaskID = request.TaskID,
                                                    NodeID = nextNode.ID,
                                                    FlowOrder = flow.FlowOrder + 1,
                                                    NodeUserName = a.ManageName ?? "",
                                                    NodeUserCode = a.ManageUserCode ?? "",
                                                    NodeUserDepart = a.ManageDepart ?? "",
                                                    NodeUserPhone = a.ManagePhone ?? "",
                                                    NodeUserJobName = a.ManageJobName ?? "",
                                                    FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                                    Comment = "",
                                                    CreateTime = now,
                                                    IsDelete = false
                                                });
                                                informers.Add(a.ManageUserCode);
                                            });
                                        }
                                        else
                                        {
                                            flowRepo.Add(new Flow_TaskApply()
                                            {
                                                TaskID = request.TaskID,
                                                NodeID = nextNode.ID,
                                                FlowOrder = flow.FlowOrder + 1,
                                                NodeUserName = request.TaskNodeName ?? "",
                                                NodeUserCode = request.TaskNodeUserCode ?? "",
                                                NodeUserDepart = request.TaskNodeDepart ?? "",
                                                NodeUserPhone = request.TaskNodePhone ?? "",
                                                NodeUserJobName = request.TaskNodeJobName ?? "",
                                                FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                                Comment = "",
                                                CreateTime = now,
                                                IsDelete = false
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
                    var informTitle = project.ProjectName + "(" + thisTask.TaskCode + ")-桩基沉桩可打性分析";
                    if (isFlowFinished)
                    {
                        informTitle = project.ProjectName + "(" + thisTask.TaskCode + ")-桩基沉桩可打性分析完成";
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
                            Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                        });
                    }
                    _cloudWindInfoService.SendJJTMessage(informRequests);

                    // 抄送
                    if (isFlowFinished)
                    {
                        List<SystemJJTInform> copyerInformRequests = new List<SystemJJTInform>();
                        copyerRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.桩基沉桩可打性分析).ToList()
                            .ForEach(a =>
                            {
                                if (!string.IsNullOrEmpty(a.UserCode))
                                {
                                    var token = _cloudCenterService.CreateJJTMessageRedirectToken("Task", a.UserCode, thisTask.ID);
                                    var redirectUrl = cloudPlatUrl + @"Home/JJTMessageRedirect?CloudToken=" + token;
                                    copyerInformRequests.Add(new SystemJJTInform()
                                    {
                                        UserCode = a.UserCode,
                                        Url = redirectUrl,
                                        Title = project.ProjectName + "(" + thisTask.TaskCode + ")-桩基沉桩可打性分析完成",
                                        Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                                    });
                                }
                            });
                        _cloudWindInfoService.SendJJTMessage(copyerInformRequests);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
            }
        }

        #endregion


        #region 拖航预报和分析

        /// <summary>
        /// 任务提交（拖航可行性分析）
        /// </summary>
        public async Task TaskSubmit_THFX(CloudWindRequest request)
        {
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var nodeUserRepo = uow.GetRepository<Flow_NodeManageUser>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_THFX>();
                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();

                    var nodeManagers = nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == (int)CloudWindSoftwareTHFXFlowNode.审核通过).ToList();
                    if (nodeManagers == null || nodeManagers.Count == 0)
                    {
                        if (string.IsNullOrEmpty(request.TaskNodeName) || string.IsNullOrEmpty(request.TaskNodeUserCode))
                            throw new Exception("没有设置节点人员");
                    }

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("项目信息错误");

                    DateTime now = DateTime.Now;

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
                    newTask.DeliverTime = now;
                    newTask.CreateTime = now;
                    newTask.IsDelete = false;
                    taskRepo.Add(newTask);
                    uow.Save();

                    var newTaskID = newTask.ID;

                    // 3.表单数据
                    var newTaskInfo = new Wind_TaskInfo_THFX();
                    newTaskInfo.TaskID = newTaskID;
                    newTaskInfo.Scheme = request.THFX_Scheme ?? "";
                    newTaskInfo.CreateTime = now;
                    newTaskInfo.IsDelete = false;
                    taskInfoRepo.Add(newTaskInfo);

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

                    List<string> informers = new List<string>();

                    // 7.生成流程
                    if (nodeManagers == null || nodeManagers.Count == 0)
                    {
                        var newFlow = new Flow_TaskApply();
                        newFlow.TaskID = newTaskID;
                        newFlow.NodeID = (int)CloudWindSoftwareTHFXFlowNode.审核通过;
                        newFlow.FlowOrder = 1;
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
                            newFlow.NodeID = (int)CloudWindSoftwareTHFXFlowNode.审核通过;
                            newFlow.FlowOrder = 1;
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
                            Title = project.ProjectName + "-拖航可行性分析",
                            Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                        });
                    }
                    _cloudWindInfoService.SendJJTMessage(informUsers);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
        }

        /// <summary>
        /// 任务查询（拖航可行性分析）
        /// </summary>
        public CloudWindTaskFlow TaskQuery_THFX(CloudWindRequest request)
        {
            CloudWindTaskFlow data = new CloudWindTaskFlow();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_THFX>();

                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (task == null) throw new Exception("工作任务数据错误");

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == task.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("工作任务数据错误");

                    var taskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).FirstOrDefault();
                    if (taskInfo == null) throw new Exception("工作任务数据错误");

                    // 1.基础信息
                    data.TaskID = task.ID;
                    data.ProjectID = task.ProjectID;
                    data.TaskName = task.TaskName;

                    // 2.表单数据
                    data.TaskInfo_THFX.Scheme = taskInfo.Scheme ?? "";

                    // 流程信息
                    var flowInfo = GetTaskFlow(task.ID, UserCode);
                    if (!flowInfo.Success) throw new Exception(flowInfo.Message);
                    data.FlowInfo = flowInfo;
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
        /// 审批提交（拖航可行性分析）
        /// </summary>
        public async Task TaskApprovalSubmit_THFX(CloudWindRequest request)
        {
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_THFX>();
                    var nodeRepo = uow.GetRepository<Flow_Node>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();
                    var nodeUserRepo = uow.GetRepository<Flow_NodeManageUser>();
                    var copyerRepo = uow.GetRepository<Manage_Copyer>();

                    DateTime now = DateTime.Now;

                    var thisTask = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (thisTask == null) throw new Exception("任务数据错误");

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == thisTask.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("任务数据错误");

                    var taskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).FirstOrDefault();
                    if (taskInfo == null) throw new Exception("任务数据错误");

                    // 获取流程信息
                    var flowInfo = GetTaskFlow(thisTask.ID, UserCode);
                    if (!flowInfo.Success) throw new Exception(flowInfo.Message);

                    var taskFlows = flowRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList();
                    var latestFlow = taskFlows.OrderByDescending(a => a.CreateTime).FirstOrDefault();
                    if (latestFlow == null) throw new Exception("流程数据错误");

                    var flow = taskFlows.FirstOrDefault(a => a.NodeID == latestFlow.NodeID && a.FlowOrder == latestFlow.FlowOrder && a.NodeUserCode == UserCode);
                    if (flow == null) throw new Exception("流程数据错误");

                    List<string> informers = new List<string>();
                    bool isFlowFinished = false;

                    // 流程处理
                    if (request.ApprovalType == (int)CloudWindFlowHandle.审批退回)
                    {
                        flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.退回;
                        flow.ApprovalTime = now;
                        flow.Comment = request.TaskComment;
                        thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.退回;

                        if (flow.FlowOrder == 1)
                        {
                            flowRepo.Add(new Flow_TaskApply()
                            {
                                TaskID = request.TaskID,
                                NodeID = flow.NodeID - 1,
                                FlowOrder = flow.FlowOrder + 1,
                                NodeUserName = thisTask.Applyer,
                                NodeUserCode = thisTask.ApplyerCode,
                                NodeUserDepart = thisTask.ApplyerDepart,
                                NodeUserPhone = thisTask.ApplyerPhone,
                                NodeUserJobName = thisTask.ApplyerJobName,
                                FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批,
                                Comment = "",
                                CreateTime = now,
                                IsDelete = false
                            });
                            informers.Add(thisTask.ApplyerCode);
                        }
                        else
                        {
                            taskFlows.Where(a => a.FlowOrder == (flow.FlowOrder - 1) && a.NodeID == (flow.NodeID - 1)).ToList()
                                .ForEach(a =>
                                {
                                    flowRepo.Add(new Flow_TaskApply()
                                    {
                                        TaskID = request.TaskID,
                                        NodeID = flow.NodeID - 1,
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
                                    informers.Add(thisTask.ApplyerCode);
                                });
                        }
                    }
                    else if (request.ApprovalType == (int)CloudWindFlowHandle.审批通过)
                    {
                        flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.审批通过;
                        flow.ApprovalTime = now;
                        flow.Comment = request.TaskComment ?? "";

                        var nextNode = nodeRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.拖航可行性分析与预报 && a.ID == (flow.NodeID + 1)).FirstOrDefault();
                        var nodeManageUsers = new List<Flow_NodeManageUser>();
                        if (nextNode != null)
                        {
                            nodeManageUsers = nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == nextNode.ID).ToList();
                        }

                        var thisFlows = taskFlows.Where(a => a.NodeID == flow.NodeID && a.FlowOrder == flow.FlowOrder && a.id != flow.id && a.FlowHandle == (int)CloudWindTaskFlowHandleStatus.待审批).ToList();
                        var lastFlow = taskFlows.Where(a => a.FlowOrder == (flow.FlowOrder - 1)).ToList();

                        // 重填的流程
                        if (lastFlow != null && lastFlow.Exists(a => a.FlowHandle == (int)CloudWindTaskFlowHandleStatus.退回))
                        {
                            taskInfo.Scheme = request.THFX_Scheme;
                        }

                        // 单人审批类型
                        if (flowInfo.NodeApprovalType)
                        {
                            if (nextNode == null)
                            {
                                thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.完成;
                                isFlowFinished = true;
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
                                            TaskID = request.TaskID,
                                            NodeID = nextNode.ID,
                                            FlowOrder = flow.FlowOrder + 1,
                                            NodeUserName = a.ManageName ?? "",
                                            NodeUserCode = a.ManageUserCode ?? "",
                                            NodeUserDepart = a.ManageDepart ?? "",
                                            NodeUserPhone = a.ManagePhone ?? "",
                                            NodeUserJobName = a.ManageJobName ?? "",
                                            FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                            Comment = "",
                                            CreateTime = now,
                                            IsDelete = false
                                        });
                                        informers.Add(a.ManageUserCode);
                                    });
                                }
                                else
                                {
                                    flowRepo.Add(new Flow_TaskApply()
                                    {
                                        TaskID = request.TaskID,
                                        NodeID = nextNode.ID,
                                        FlowOrder = flow.FlowOrder + 1,
                                        NodeUserName = request.TaskNodeName ?? "",
                                        NodeUserCode = request.TaskNodeUserCode ?? "",
                                        NodeUserDepart = request.TaskNodeDepart ?? "",
                                        NodeUserPhone = request.TaskNodePhone ?? "",
                                        NodeUserJobName = request.TaskNodeJobName ?? "",
                                        FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                        Comment = "",
                                        CreateTime = now,
                                        IsDelete = false
                                    });
                                    informers.Add(request.TaskNodeUserCode);
                                }
                            }
                            if (thisFlows.Count > 0)
                                thisFlows.ForEach(a => { a.IsDelete = true; });
                        }
                        else
                        {
                            // 多人审批
                            if (thisFlows.Count == 0)
                            {
                                if (nextNode == null)
                                {
                                    thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.完成;
                                    isFlowFinished = true;
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
                                                TaskID = request.TaskID,
                                                NodeID = nextNode.ID,
                                                FlowOrder = flow.FlowOrder + 1,
                                                NodeUserName = a.ManageName ?? "",
                                                NodeUserCode = a.ManageUserCode ?? "",
                                                NodeUserDepart = a.ManageDepart ?? "",
                                                NodeUserPhone = a.ManagePhone ?? "",
                                                NodeUserJobName = a.ManageJobName ?? "",
                                                FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                                Comment = "",
                                                CreateTime = now,
                                                IsDelete = false
                                            });
                                            informers.Add(a.ManageUserCode);
                                        });
                                    }
                                    else
                                    {
                                        flowRepo.Add(new Flow_TaskApply()
                                        {
                                            TaskID = request.TaskID,
                                            NodeID = nextNode.ID,
                                            FlowOrder = flow.FlowOrder + 1,
                                            NodeUserName = request.TaskNodeName ?? "",
                                            NodeUserCode = request.TaskNodeUserCode ?? "",
                                            NodeUserDepart = request.TaskNodeDepart ?? "",
                                            NodeUserPhone = request.TaskNodePhone ?? "",
                                            NodeUserJobName = request.TaskNodeJobName ?? "",
                                            FlowHandle = (int)CloudWindTaskFlowHandleStatus.待审批,
                                            Comment = "",
                                            CreateTime = now,
                                            IsDelete = false
                                        });
                                        informers.Add(request.TaskNodeUserCode);
                                    }
                                }
                            }
                        }
                    }

                    uow.Save();

                    // 交建通通知
                    var informTitle = project.ProjectName + "-拖航可作业预报与分析";
                    if (isFlowFinished)
                    {
                        informTitle = project.ProjectName + "(" + thisTask.TaskCode + ")-拖航可作业预报与分析流程审批";
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
                            Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                        });
                    }
                    _cloudWindInfoService.SendJJTMessage(informRequests);

                    // 抄送
                    if (isFlowFinished)
                    {
                        List<SystemJJTInform> copyerInformRequests = new List<SystemJJTInform>();
                        copyerRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.拖航可行性分析与预报).ToList()
                            .ForEach(a =>
                            {
                                if (!string.IsNullOrEmpty(a.UserCode))
                                {
                                    var token = _cloudCenterService.CreateJJTMessageRedirectToken("Task", a.UserCode, thisTask.ID);
                                    var redirectUrl = cloudPlatUrl + @"Home/JJTMessageRedirect?CloudToken=" + token;
                                    copyerInformRequests.Add(new SystemJJTInform()
                                    {
                                        UserCode = a.UserCode,
                                        Url = redirectUrl,
                                        Title = project.ProjectName + "(" + thisTask.TaskCode + ")-拖航可作业预报与分析流程结束",
                                        Content = "点击查看详情"
                                    });
                                }
                            });
                        _cloudWindInfoService.SendJJTMessage(copyerInformRequests);
                    }

                    // 流程结束，生成推送文件夹
                    if (isFlowFinished)
                    {
                        _cloudWindKZYService.CreateKZYFile(thisTask.ID);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
            }
        }

        /// <summary>
        /// 拖航计划航线上传
        /// </summary>
        public List<TowingPlanRoute> TowingPlanRouteUpload(CloudWindRequest request, IFormFile file)
        {
            List<TowingPlanRoute> list = new List<TowingPlanRoute>();
            try
            {
                using (var techUow = _techCenterUowFactory.Create())
                using (var towingUow = _towingUowFactory.Create())
                {
                    var projectRepo = techUow.GetRepository<Wind_Project>();
                    var towingProjectRepo = towingUow.GetRepository<Manage_Project>();
                    var planRouteRepo = towingUow.GetRepository<Manage_PlanRoute>();

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("数据错误");

                    var towingProject = towingProjectRepo.Find(a => !a.IsDelete && a.CloudProjectID == project.ID).FirstOrDefault();
                    if (towingProject == null) throw new Exception("数据错误");

                    string filePath = Path.Combine(_env.WebRootPath, "File", "Project", project.ProjectCode.ToUpper(), "Import", Guid.NewGuid() + Path.GetExtension(file.FileName));
                    FileUtils.SaveFile(file, filePath);

                    DataSet ds = ExcelUtils.ExcelToTableForXLSX(filePath);
                    if (ds.Tables.Count == 0 || ds.Tables[0] == null) throw new Exception("数据错误");

                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count == 0) throw new Exception("数据错误");

                    // 先删除旧数据
                    planRouteRepo.Find(a => !a.IsDelete && a.ProjectID == towingProject.id).ToList()
                        .ForEach(a =>
                        {
                            a.IsDelete = true;
                            a.CreateTime = DateTime.Now;
                        });

                    List<Manage_PlanRoute> routes = new List<Manage_PlanRoute>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        string lonStr = dr[1].ToString();
                        string latStr = dr[0].ToString();

                        if (!string.IsNullOrEmpty(lonStr) && !string.IsNullOrEmpty(latStr))
                        {
                            Manage_PlanRoute route = new Manage_PlanRoute();
                            route.ProjectID = towingProject.id;
                            route.Lon = Convert.ToDecimal(lonStr);
                            route.Lat = Convert.ToDecimal(latStr);
                            route.CreateTime = DateTime.Now;
                            route.IsDelete = false;
                            routes.Add(route);

                            list.Add(new TowingPlanRoute()
                            {
                                Lon = lonStr,
                                Lat = latStr
                            });
                        }
                    }

                    if (routes.Count > 0)
                    {
                        planRouteRepo.AddList(routes);
                        towingUow.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                list = new List<TowingPlanRoute>();
                throw new Exception("发生错误,请检查地勘数据表格式。");
            }
            return list;
        }

        /// <summary>
        /// 拖航文件上传
        /// </summary>
        public List<CloudProjectFile> TowingFileUpload(CloudWindRequest request, List<IFormFile> files)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var fileRepo = uow.GetRepository<Wind_Task_ProjectFile>();

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("项目数据错误");

                    string fileFolderPath = Path.Combine(_env.WebRootPath, "File", "Project", "CommonFile");

                    DateTime now = DateTime.Now;
                    List<Wind_Task_ProjectFile> fileList = new List<Wind_Task_ProjectFile>();

                    foreach (var file in files)
                    {
                        if (file != null)
                        {
                            Wind_Task_ProjectFile model = new Wind_Task_ProjectFile();
                            model.ProjectID = request.ProjectID;
                            model.TaskID = null;
                            model.FileName = file.FileName;
                            model.FilePath = fileFolderPath + @"/" + Guid.NewGuid() + Path.GetExtension(file.FileName);
                            model.CreateTime = now;
                            model.IsDelete = false;
                            fileList.Add(model);

                            FileUtils.SaveFile(file, model.FilePath);
                        }
                    }

                    fileRepo.AddList(fileList);
                    uow.Save();

                    fileRepo.Find(a => !(a.IsDelete == true) && a.ProjectID == request.ProjectID).ToList()
                        .ForEach(a =>
                        {
                            list.Add(new CloudProjectFile()
                            {
                                ID = a.id,
                                FileName = a.FileName,
                                FileTime = now.ToString("yyyy-MM-dd"),
                                FilePath = FileUtils.GetRelativeFilePath(a.FilePath)
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                list = new List<CloudProjectFile>();
                throw new Exception("发生错误,请检查文件格式。");
            }
            return list;
        }

        /// <summary>
        /// 删除拖航文件
        /// </summary>
        public List<CloudProjectFile> TowingFileDelete(CloudWindRequest request)
        {
            List<CloudProjectFile> files = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var fileRepo = uow.GetRepository<Wind_Task_ProjectFile>();

                    var data = fileRepo.Find(a => !(a.IsDelete == true) && a.id == request.ID).FirstOrDefault();
                    if (data == null) throw new Exception("数据错误");

                    data.IsDelete = true;
                    uow.Save();

                    fileRepo.Find(a => !(a.IsDelete == true) && a.ProjectID == data.ProjectID).ToList()
                        .ForEach(a =>
                        {
                            files.Add(new CloudProjectFile()
                            {
                                ID = a.id,
                                FileName = a.FileName,
                                FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                FileTime = a.CreateTime?.ToString("yyyy-MM-dd")
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return files;
        }

        /// <summary>
        /// 加载拖航航线（双库）
        /// </summary>
        public List<TowingPlanRoute> LoadPlanRoute(CloudWindRequest request)
        {
            List<TowingPlanRoute> list = new List<TowingPlanRoute>();
            try
            {
                using (var techUow = _techCenterUowFactory.Create())
                using (var towingUow = _towingUowFactory.Create())
                {
                    var projectRepo = techUow.GetRepository<Wind_Project>();
                    var towingProjectRepo = towingUow.GetRepository<Manage_Project>();
                    var planRouteRepo = towingUow.GetRepository<Manage_PlanRoute>();

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("数据错误");

                    var towingProject = towingProjectRepo.Find(a => !a.IsDelete && a.CloudProjectID == project.ID).FirstOrDefault();
                    if (towingProject == null) throw new Exception("数据错误");

                    planRouteRepo.Find(a => !a.IsDelete && a.ProjectID == towingProject.id).ToList()
                        .ForEach(a =>
                        {
                            list.Add(new TowingPlanRoute()
                            {
                                ID = a.id,
                                Lon = a.Lon?.ToString() ?? "",
                                Lat = a.Lat?.ToString() ?? ""
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return list;
        }

        /// <summary>
        /// 加载拖航文件
        /// </summary>
        public List<CloudProjectFile> LoadTowingFile(CloudWindRequest request)
        {
            List<CloudProjectFile> files = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var fileRepo = uow.GetRepository<Wind_Task_ProjectFile>();

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("数据错误");

                    fileRepo.Find(a => !(a.IsDelete == true) && a.ProjectID == project.ID).ToList()
                        .ForEach(a =>
                        {
                            files.Add(new CloudProjectFile()
                            {
                                ID = a.id,
                                FileName = a.FileName,
                                FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                FileTime = a.CreateTime?.ToString("yyyy-MM-dd")
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return files;
        }

        #endregion


        #region 选船分析

        /// <summary>
        /// 任务提交（选船分析）
        /// </summary>
        public async Task TaskSubmit_XCFX(CloudWindRequest request)
        {
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var nodeUserRepo = uow.GetRepository<Flow_NodeManageUser>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_XCFX>();
                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();

                    var nodeManagers = nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == (int)CloudWindSoftwareXCFXFlowNode.技术审核).ToList();
                    if (nodeManagers == null || nodeManagers.Count == 0)
                    {
                        if (string.IsNullOrEmpty(request.TaskNodeName) || string.IsNullOrEmpty(request.TaskNodeUserCode))
                            throw new Exception("没有设置节点人员");
                    }

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("项目信息错误");

                    DateTime now = DateTime.Now;

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
                    newTask.DeliverTime = now;
                    newTask.CreateTime = now;
                    newTask.IsDelete = false;
                    taskRepo.Add(newTask);
                    uow.Save();

                    var newTaskID = newTask.ID;

                    // 3.表单数据
                    var newTaskInfo = new Wind_TaskInfo_XCFX();
                    newTaskInfo.TaskID = newTaskID;
                    newTaskInfo.ShipIDs = request.ShipIDstring;
                    newTaskInfo.TaskDesc = request.XCFX_TaskDesc ?? "";
                    newTaskInfo.CreateTime = now;
                    newTaskInfo.IsDelete = false;
                    taskInfoRepo.Add(newTaskInfo);

                    // 3.创建文件夹
                    string fileImportFolder = Path.Combine(_env.WebRootPath, "File", "Project", project.ProjectCode, "Task", newTask.TaskCode, "XCFX");
                    FileUtils.CreateFileFolder(fileImportFolder);

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

                    List<string> informers = new List<string>();

                    // 7.生成流程
                    if (nodeManagers == null || nodeManagers.Count == 0)
                    {
                        var newFlow = new Flow_TaskApply();
                        newFlow.TaskID = newTaskID;
                        newFlow.NodeID = (int)CloudWindSoftwareXCFXFlowNode.技术审核;
                        newFlow.FlowOrder = 1;
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
                            newFlow.NodeID = (int)CloudWindSoftwareXCFXFlowNode.技术审核;
                            newFlow.FlowOrder = 1;
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
                            Title = project.ProjectName + "-基于历史环境数据的选船分析流程审批",
                            Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                        });
                    }
                    _cloudWindInfoService.SendJJTMessage(informUsers);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
        }

        /// <summary>
        /// 任务查询（选船分析）
        /// </summary>
        public CloudWindTaskFlow TaskQuery_XCFX(CloudWindRequest request)
        {
            CloudWindTaskFlow data = new CloudWindTaskFlow();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_XCFX>();
                    var viewFlowRepo = uow.GetRepository<View_Flow_TaskApply>();
                    var projectRoleRepo = uow.GetRepository<Wind_ProjectRole>();
                    var reportRepo = uow.GetRepository<Wind_TaskReport>();

                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (task == null) throw new Exception("工作任务数据错误");

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == task.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("工作任务数据错误");

                    var deliver = deliverRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).FirstOrDefault();
                    if (deliver == null) throw new Exception("工作任务数据错误");

                    var taskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).FirstOrDefault();
                    if (taskInfo == null) throw new Exception("工作任务数据错误");

                    // 1.基础信息
                    data.TaskID = task.ID;
                    data.ProjectID = task.ProjectID;
                    data.TaskName = task.TaskName;

                    // 2.表单数据
                    data.TaskInfo_XCFX.TaskDesc = taskInfo.TaskDesc ?? "";
                    if (!string.IsNullOrEmpty(taskInfo.ShipIDs))
                    {
                        string[] shipArr = taskInfo.ShipIDs.Split(',');
                        foreach (string shipID in shipArr)
                        {
                            data.TaskInfo_XCFX.ShipIDs.Add(Convert.ToInt32(shipID));
                        }
                    }

                    // 流程信息
                    var flowInfo = GetTaskFlow(task.ID, UserCode);
                    if (!flowInfo.Success) throw new Exception(flowInfo.Message);
                    data.FlowInfo = flowInfo;

                    // 文件报告（特定节点人员才能查看）
                    List<string> reportViewer = new List<string>();
                    viewFlowRepo.Find(a => a.TaskID == request.TaskID && (a.NodeID == (int)CloudWindSoftwareXCFXFlowNode.技术审核 || a.NodeID == (int)CloudWindSoftwareXCFXFlowNode.审核通过))
                        .GroupBy(a => a.NodeUserCode).ToList()
                        .ForEach(a =>
                        {
                            reportViewer.Add(a.FirstOrDefault().NodeUserCode);
                        });

                    if (task.FlowStatus == (int)CloudWindTaskFlowStatus.完成)
                    {
                        reportViewer.Add(deliver.DeliverCode);
                        // 项目组领导班子
                        projectRoleRepo.Find(a => !a.IsDelete && a.ProjectID == project.ID && a.RoleID == (int)CloudWindProjectRole.项目部领导班子).ToList()
                            .ForEach(a =>
                            {
                                reportViewer.Add(a.UserCode);
                            });
                    }

                    var reports = reportRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList();
                    if (reportViewer.Contains(CurrentUser.UserCode))
                    {
                        if (reports != null && reports.Count > 0)
                        {
                            reports.Where(a => a.Type == (int)CloudWindTaskReportType.选船分析报告)
                                .OrderByDescending(a => a.CreateTime).ToList()
                                .ForEach(a =>
                                {
                                    data.Report.ReportFile.Add(new CloudProjectFile()
                                    {
                                        ID = a.id,
                                        FileName = a.ReportName,
                                        FilePath = "/File/Project/" + project.ProjectCode + "/Task/" + task.TaskCode + @"/Export/" + a.ReportPath.Substring(a.ReportPath.LastIndexOf('/') + 1),
                                        FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
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
        /// 审批提交（选船分析）
        /// </summary>
        public async Task TaskApprovalSubmit_XCFX(CloudWindRequest request)
        {
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_XCFX>();
                    var nodeRepo = uow.GetRepository<Flow_Node>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();
                    var nodeUserRepo = uow.GetRepository<Flow_NodeManageUser>();
                    var reportRepo = uow.GetRepository<Wind_TaskReport>();

                    DateTime now = DateTime.Now;

                    var thisTask = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (thisTask == null) throw new Exception("任务数据错误");

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == thisTask.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("任务数据错误");

                    var taskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).FirstOrDefault();
                    if (taskInfo == null) throw new Exception("任务数据错误");

                    // 获取流程信息
                    var flowInfo = GetTaskFlow(thisTask.ID, UserCode);
                    if (!flowInfo.Success) throw new Exception(flowInfo.Message);

                    var taskFlows = flowRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList();
                    var latestFlow = taskFlows.OrderByDescending(a => a.CreateTime).FirstOrDefault();
                    if (latestFlow == null) throw new Exception("流程数据错误");

                    var flow = taskFlows.FirstOrDefault(a => a.NodeID == latestFlow.NodeID && a.FlowOrder == latestFlow.FlowOrder && a.NodeUserCode == UserCode);
                    if (flow == null) throw new Exception("流程数据错误");

                    List<string> informers = new List<string>();
                    bool isFlowFinished = false;

                    // 流程处理
                    if (request.ApprovalType == (int)CloudWindFlowHandle.审批退回)
                    {
                        flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.退回;
                        flow.ApprovalTime = now;
                        flow.Comment = request.TaskComment;
                        thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.退回;

                        if (flow.FlowOrder == 1)
                        {
                            flowRepo.Add(new Flow_TaskApply()
                            {
                                TaskID = request.TaskID, NodeID = flow.NodeID - 1, FlowOrder = flow.FlowOrder + 1,
                                NodeUserName = thisTask.Applyer, NodeUserCode = thisTask.ApplyerCode,
                                NodeUserDepart = thisTask.ApplyerDepart, NodeUserPhone = thisTask.ApplyerPhone,
                                NodeUserJobName = thisTask.ApplyerJobName, FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批,
                                Comment = "", CreateTime = now, IsDelete = false
                            });
                            informers.Add(thisTask.ApplyerCode);
                        }
                        else
                        {
                            taskFlows.Where(a => a.FlowOrder == (flow.FlowOrder - 1) && a.NodeID == (flow.NodeID - 1)).ToList()
                                .ForEach(a =>
                                {
                                    flowRepo.Add(new Flow_TaskApply()
                                    {
                                        TaskID = request.TaskID, NodeID = flow.NodeID - 1, FlowOrder = flow.FlowOrder + 1,
                                        NodeUserName = a.NodeUserName, NodeUserCode = a.NodeUserCode,
                                        NodeUserDepart = a.NodeUserDepart, NodeUserPhone = a.NodeUserPhone,
                                        NodeUserJobName = a.NodeUserJobName, FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批,
                                        Comment = "", CreateTime = now, IsDelete = false
                                    });
                                    informers.Add(thisTask.ApplyerCode);
                                });
                        }
                    }
                    else if (request.ApprovalType == (int)CloudWindFlowHandle.审批通过)
                    {
                        flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.审批通过;
                        flow.ApprovalTime = now;
                        flow.Comment = request.TaskComment ?? "";

                        var nextNode = nodeRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.基于历史环境数据的选船分析 && a.ID == (flow.NodeID + 1)).FirstOrDefault();
                        var nodeManageUsers = new List<Flow_NodeManageUser>();
                        if (nextNode != null)
                        {
                            nodeManageUsers = nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == nextNode.ID).ToList();
                        }

                        var thisFlows = taskFlows.Where(a => a.NodeID == flow.NodeID && a.FlowOrder == flow.FlowOrder && a.id != flow.id && a.FlowHandle == (int)CloudWindTaskFlowHandleStatus.待审批).ToList();
                        var lastFlow = taskFlows.Where(a => a.FlowOrder == (flow.FlowOrder - 1)).ToList();

                        // 非退回重填时，检查报告是否已上传
                        if (lastFlow == null || !lastFlow.Exists(a => a.FlowHandle == (int)CloudWindTaskFlowHandleStatus.退回))
                        {
                            var reports = reportRepo.Find(a => !(a.IsDelete == true) && a.Type == (int)CloudWindTaskReportType.选船分析报告 && a.TaskID == request.TaskID).ToList();
                            if (reports == null || reports.Count == 0)
                                throw new Exception("没有上传任何报告");
                        }

                        if (nextNode != null)
                        {
                            taskInfo.ShipIDs = request.ShipIDstring ?? "";
                            taskInfo.TaskDesc = request.XCFX_TaskDesc ?? "";
                        }

                        // 单人审批类型
                        if (flowInfo.NodeApprovalType)
                        {
                            if (nextNode == null)
                            {
                                thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.完成;
                                isFlowFinished = true;
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
                            if (thisFlows.Count > 0)
                                thisFlows.ForEach(a => { a.IsDelete = true; });
                        }
                        else
                        {
                            // 多人审批
                            if (thisFlows.Count == 0)
                            {
                                if (nextNode == null)
                                {
                                    thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.完成;
                                    isFlowFinished = true;
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

                    uow.Save();

                    // 交建通通知
                    var informTitle = project.ProjectName + "-基于历史环境数据的选船分析流程审批";
                    if (isFlowFinished)
                    {
                        informTitle = project.ProjectName + "(" + thisTask.TaskCode + ")-基于历史环境数据的选船分析完成";
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
                            UserCode = userCode, Url = redirectUrl, Title = informTitle,
                            Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                        });
                    }
                    _cloudWindInfoService.SendJJTMessage(informRequests);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
            }
        }

        /// <summary>
        /// 上传选船分析报告
        /// </summary>
        public List<CloudProjectFile> UploadReport_XCFX(CloudWindRequest request, List<IFormFile> files)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var reportRepo = uow.GetRepository<Wind_TaskReport>();

                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (task == null) throw new Exception("数据错误");

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == task.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("数据错误");

                    List<Wind_TaskReport> fileList = new List<Wind_TaskReport>();
                    foreach (var file in files)
                    {
                        if (file != null)
                        {
                            Wind_TaskReport model = new Wind_TaskReport();
                            model.TaskID = request.TaskID;
                            model.Type = (int)CloudWindTaskReportType.选船分析报告;
                            model.ReportName = file.FileName;
                            string relPath = "/File/Project/" + project.ProjectCode + "/Task/" + task.TaskCode + "/XCFX/" + Guid.NewGuid() + Path.GetExtension(file.FileName);
                            model.ReportPath = Path.Combine(_env.WebRootPath, relPath.TrimStart('/'));
                            model.CreateTime = DateTime.Now;
                            model.IsDelete = false;
                            fileList.Add(model);

                            FileUtils.SaveFile(file, model.ReportPath);
                        }
                    }
                    if (fileList.Count > 0)
                    {
                        reportRepo.AddList(fileList);
                        uow.Save();
                    }

                    reportRepo.Find(a => !(a.IsDelete == true) && a.Type == (int)CloudWindTaskReportType.选船分析报告 && a.TaskID == request.TaskID).ToList()
                        .ForEach(a =>
                        {
                            list.Add(new CloudProjectFile()
                            {
                                ID = a.id,
                                FileName = a.ReportName,
                                FilePath = FileUtils.GetRelativeFilePath(a.ReportPath),
                                FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                list = new List<CloudProjectFile>();
                throw new Exception("发生错误");
            }
            return list;
        }

        #endregion


        #region 气象收集

        /// <summary>
        /// 任务提交（气象收集）
        /// </summary>
        public async Task TaskSubmit_QXSJ(CloudWindRequest request)
        {
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var nodeUserRepo = uow.GetRepository<Flow_NodeManageUser>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_QXSJ>();
                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();

                    var points = JsonUtils.Deserialize<List<CloudWindCollectPoint_QXSJ>>(request.QXSJ_Points);
                    if (points == null || points.Count == 0)
                        throw new Exception("没有配置采集点，数据输入要完整");

                    var nodeManagers = nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == (int)CloudWindSoftwareQXSJFlowNode.审核通过).ToList();
                    if (nodeManagers == null || nodeManagers.Count == 0)
                    {
                        if (string.IsNullOrEmpty(request.TaskNodeName) || string.IsNullOrEmpty(request.TaskNodeUserCode))
                            throw new Exception("没有设置节点人员");
                    }

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("项目信息错误");

                    DateTime now = DateTime.Now;

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
                    newTask.DeliverTime = now;
                    newTask.CreateTime = now;
                    newTask.IsDelete = false;
                    taskRepo.Add(newTask);
                    uow.Save();

                    var newTaskID = newTask.ID;

                    // 3.表单数据
                    string pointStr = "";
                    points.ForEach(a =>
                    {
                        if (!string.IsNullOrEmpty(a.PointName) && a.PointLon != null && a.PointLat != null)
                            pointStr += a.PointName + "_" + a.PointLon.ToString() + "_" + a.PointLat + ",";
                    });

                    var newTaskInfo = new Wind_TaskInfo_QXSJ();
                    newTaskInfo.TaskID = newTaskID;
                    newTaskInfo.Points = pointStr.Trim(',');
                    newTaskInfo.CollectStartTime = Convert.ToDateTime(request.QXSJ_StartTime);
                    newTaskInfo.CollectEndTime = Convert.ToDateTime(request.QXSJ_EndTime);
                    newTaskInfo.CreateTime = now;
                    newTaskInfo.IsDelete = false;
                    taskInfoRepo.Add(newTaskInfo);

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

                    List<string> informers = new List<string>();

                    // 7.生成流程
                    if (nodeManagers == null || nodeManagers.Count == 0)
                    {
                        var newFlow = new Flow_TaskApply();
                        newFlow.TaskID = newTaskID;
                        newFlow.NodeID = (int)CloudWindSoftwareQXSJFlowNode.审核通过;
                        newFlow.FlowOrder = 1;
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
                            newFlow.NodeID = (int)CloudWindSoftwareQXSJFlowNode.审核通过;
                            newFlow.FlowOrder = 1;
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
                            Title = project.ProjectName + "-海况与气象数据收集流程审批",
                            Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                        });
                    }
                    _cloudWindInfoService.SendJJTMessage(informUsers);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
        }

        /// <summary>
        /// 任务查询（气象收集）
        /// </summary>
        public CloudWindTaskFlow TaskQuery_QXSJ(CloudWindRequest request)
        {
            CloudWindTaskFlow data = new CloudWindTaskFlow();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var deliverRepo = uow.GetRepository<Wind_TaskFileDeliver>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_QXSJ>();

                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (task == null) throw new Exception("工作任务数据错误");

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == task.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("工作任务数据错误");

                    var deliver = deliverRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).FirstOrDefault();
                    if (deliver == null) throw new Exception("工作任务数据错误");

                    var taskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).FirstOrDefault();
                    if (taskInfo == null) throw new Exception("工作任务数据错误");

                    // 1.基础信息
                    data.TaskID = task.ID;
                    data.ProjectID = task.ProjectID;
                    data.TaskName = task.TaskName;

                    // 2.表单数据
                    data.TaskInfo_QXSJ.CollectStartTime = taskInfo.CollectStartTime == null ? "" : taskInfo.CollectStartTime.Value.ToString("yyyy-MM-dd");
                    data.TaskInfo_QXSJ.CollectEndTime = taskInfo.CollectEndTime == null ? "" : taskInfo.CollectEndTime.Value.ToString("yyyy-MM-dd");
                    if (!string.IsNullOrEmpty(taskInfo.Points))
                    {
                        string[] pointArr = taskInfo.Points.Split(',');
                        foreach (string str in pointArr)
                        {
                            string[] parts = str.Split('_');
                            if (parts.Length >= 3)
                            {
                                data.TaskInfo_QXSJ.Points.Add(new CloudWindCollectPoint_QXSJ()
                                {
                                    PointName = parts[0],
                                    PointLon = parts[1].ToString(),
                                    PointLat = parts[2].ToString()
                                });
                            }
                        }
                    }

                    // 流程信息
                    var flowInfo = GetTaskFlow(task.ID, UserCode);
                    if (!flowInfo.Success) throw new Exception(flowInfo.Message);
                    data.FlowInfo = flowInfo;
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
        /// 审批提交（气象收集）
        /// </summary>
        public async Task TaskApprovalSubmit_QXSJ(CloudWindRequest request)
        {
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_QXSJ>();
                    var nodeRepo = uow.GetRepository<Flow_Node>();
                    var flowRepo = uow.GetRepository<Flow_TaskApply>();
                    var nodeUserRepo = uow.GetRepository<Flow_NodeManageUser>();

                    DateTime now = DateTime.Now;

                    var thisTask = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (thisTask == null) throw new Exception("任务数据错误");

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == thisTask.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("任务数据错误");

                    var taskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).FirstOrDefault();
                    if (taskInfo == null) throw new Exception("任务数据错误");

                    // 获取流程信息
                    var flowInfo = GetTaskFlow(thisTask.ID, UserCode);
                    if (!flowInfo.Success) throw new Exception(flowInfo.Message);

                    var taskFlows = flowRepo.Find(a => !(a.IsDelete == true) && a.TaskID == request.TaskID).ToList();
                    var latestFlow = taskFlows.OrderByDescending(a => a.CreateTime).FirstOrDefault();
                    if (latestFlow == null) throw new Exception("流程数据错误");

                    var flow = taskFlows.FirstOrDefault(a => a.NodeID == latestFlow.NodeID && a.FlowOrder == latestFlow.FlowOrder && a.NodeUserCode == UserCode);
                    if (flow == null) throw new Exception("流程数据错误");

                    List<string> informers = new List<string>();
                    bool isFlowFinished = false;

                    // 流程处理
                    if (request.ApprovalType == (int)CloudWindFlowHandle.审批退回)
                    {
                        flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.退回;
                        flow.ApprovalTime = now;
                        flow.Comment = request.TaskComment;
                        thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.退回;

                        if (flow.FlowOrder == 1)
                        {
                            flowRepo.Add(new Flow_TaskApply()
                            {
                                TaskID = request.TaskID, NodeID = flow.NodeID - 1, FlowOrder = flow.FlowOrder + 1,
                                NodeUserName = thisTask.Applyer, NodeUserCode = thisTask.ApplyerCode,
                                NodeUserDepart = thisTask.ApplyerDepart, NodeUserPhone = thisTask.ApplyerPhone,
                                NodeUserJobName = thisTask.ApplyerJobName, FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批,
                                Comment = "", CreateTime = now, IsDelete = false
                            });
                            informers.Add(thisTask.ApplyerCode);
                        }
                        else
                        {
                            taskFlows.Where(a => a.FlowOrder == (flow.FlowOrder - 1) && a.NodeID == (flow.NodeID - 1)).ToList()
                                .ForEach(a =>
                                {
                                    flowRepo.Add(new Flow_TaskApply()
                                    {
                                        TaskID = request.TaskID, NodeID = flow.NodeID - 1, FlowOrder = flow.FlowOrder + 1,
                                        NodeUserName = a.NodeUserName, NodeUserCode = a.NodeUserCode,
                                        NodeUserDepart = a.NodeUserDepart, NodeUserPhone = a.NodeUserPhone,
                                        NodeUserJobName = a.NodeUserJobName, FlowHandle = (int)CloudWindProjectFlowHandleStatus.待审批,
                                        Comment = "", CreateTime = now, IsDelete = false
                                    });
                                    informers.Add(thisTask.ApplyerCode);
                                });
                        }
                    }
                    else if (request.ApprovalType == (int)CloudWindFlowHandle.审批通过)
                    {
                        flow.FlowHandle = (int)CloudWindTaskFlowHandleStatus.审批通过;
                        flow.ApprovalTime = now;
                        flow.Comment = request.TaskComment ?? "";

                        var nextNode = nodeRepo.Find(a => !a.IsDelete && a.SoftwareID == (int)CloudWindSoftware.海况与气象数据收集 && a.ID == (flow.NodeID + 1)).FirstOrDefault();
                        var nodeManageUsers = new List<Flow_NodeManageUser>();
                        if (nextNode != null)
                        {
                            nodeManageUsers = nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == nextNode.ID).ToList();
                        }

                        var thisFlows = taskFlows.Where(a => a.NodeID == flow.NodeID && a.FlowOrder == flow.FlowOrder && a.id != flow.id && a.FlowHandle == (int)CloudWindTaskFlowHandleStatus.待审批).ToList();
                        var lastFlow = taskFlows.Where(a => a.FlowOrder == (flow.FlowOrder - 1)).ToList();

                        // 重填的流程
                        if (lastFlow != null && lastFlow.Exists(a => a.FlowHandle == (int)CloudWindTaskFlowHandleStatus.退回))
                        {
                            var points = JsonUtils.Deserialize<List<CloudWindCollectPoint_QXSJ>>(request.QXSJ_Points);
                            if (points == null || points.Count == 0)
                                throw new Exception("没有配置采集点，数据输入要完整");
                            string pointStr = "";
                            points.ForEach(a =>
                            {
                                if (!string.IsNullOrEmpty(a.PointName) && a.PointLon != null && a.PointLat != null)
                                    pointStr += a.PointName + "_" + a.PointLon.ToString() + "_" + a.PointLat + ",";
                            });
                            taskInfo.Points = pointStr.Trim(',');
                            taskInfo.CollectStartTime = Convert.ToDateTime(request.QXSJ_StartTime);
                            taskInfo.CollectEndTime = Convert.ToDateTime(request.QXSJ_EndTime);
                        }

                        // 单人审批类型
                        if (flowInfo.NodeApprovalType)
                        {
                            if (nextNode == null)
                            {
                                thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.完成;
                                isFlowFinished = true;
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
                            if (thisFlows.Count > 0)
                                thisFlows.ForEach(a => { a.IsDelete = true; });
                        }
                        else
                        {
                            if (thisFlows.Count == 0)
                            {
                                if (nextNode == null)
                                {
                                    thisTask.FlowStatus = (int)CloudWindTaskFlowStatus.完成;
                                    isFlowFinished = true;
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

                    uow.Save();

                    // 交建通通知
                    var informTitle = project.ProjectName + "-海况与气象数据收集流程审批";
                    if (isFlowFinished)
                    {
                        informTitle = project.ProjectName + "(" + thisTask.TaskCode + ")-海况与气象数据收集完成";
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
                            UserCode = userCode, Url = redirectUrl, Title = informTitle,
                            Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n点击查看详情"
                        });
                    }
                    _cloudWindInfoService.SendJJTMessage(informRequests);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
            }
        }

        #endregion

        /// <summary>
        /// 查询下一流程节点
        /// </summary>
        public CloudWindNextNode NextNodeQuery(CloudWindRequest request)
        {
            CloudWindNextNode node = new CloudWindNextNode();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var nodeUserRepo = uow.GetRepository<Flow_NodeManageUser>();

                    if (request.SoftwareID == (int)CloudWindSoftware.自升式平台桩腿插拔计算)
                    {
                        nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == (int)CloudWindSoftwareCBZFlowNode.技术审核).ToList()
                            .ForEach(a => { node.NodeUsers.Add(new CloudWindNextNodeManage { ID = a.ID, ManageName = a.ManageName, ManageDepart = a.ManageDepart }); });
                        node.ID = (int)CloudWindSoftwareCBZFlowNode.技术审核;
                        node.NodeName = CloudWindSoftwareCBZFlowNode.技术审核.ToString();
                    }
                    else if (request.SoftwareID == (int)CloudWindSoftware.起重船基础施工可作业性预报)
                    {
                        nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == (int)CloudWindSoftwareKZYFlowNode.技术人员审核).ToList()
                            .ForEach(a => { node.NodeUsers.Add(new CloudWindNextNodeManage { ID = a.ID, ManageName = a.ManageName, ManageDepart = a.ManageDepart }); });
                        node.ID = (int)CloudWindSoftwareKZYFlowNode.技术人员审核;
                        node.NodeName = CloudWindSoftwareKZYFlowNode.技术人员审核.ToString();
                    }
                    else if (request.SoftwareID == (int)CloudWindSoftware.桩基沉桩可打性分析)
                    {
                        nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == (int)CloudWindSoftwareZJCZFlowNode.技术审核).ToList()
                            .ForEach(a => { node.NodeUsers.Add(new CloudWindNextNodeManage { ID = a.ID, ManageName = a.ManageName, ManageDepart = a.ManageDepart }); });
                        node.ID = (int)CloudWindSoftwareZJCZFlowNode.技术审核;
                        node.NodeName = CloudWindSoftwareZJCZFlowNode.技术审核.ToString();
                    }
                    else if (request.SoftwareID == (int)CloudWindSoftware.拖航可行性分析与预报)
                    {
                        nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == (int)CloudWindSoftwareTHFXFlowNode.审核通过).ToList()
                            .ForEach(a => { node.NodeUsers.Add(new CloudWindNextNodeManage { ID = a.ID, ManageName = a.ManageName, ManageDepart = a.ManageDepart }); });
                        node.ID = (int)CloudWindSoftwareTHFXFlowNode.审核通过;
                        node.NodeName = CloudWindSoftwareTHFXFlowNode.审核通过.ToString();
                    }
                    else if (request.SoftwareID == (int)CloudWindSoftware.基于历史环境数据的选船分析)
                    {
                        nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == (int)CloudWindSoftwareXCFXFlowNode.技术审核).ToList()
                            .ForEach(a => { node.NodeUsers.Add(new CloudWindNextNodeManage { ID = a.ID, ManageName = a.ManageName, ManageDepart = a.ManageDepart }); });
                        node.ID = (int)CloudWindSoftwareXCFXFlowNode.技术审核;
                        node.NodeName = CloudWindSoftwareXCFXFlowNode.技术审核.ToString();
                    }
                    else if (request.SoftwareID == (int)CloudWindSoftware.海况与气象数据收集)
                    {
                        nodeUserRepo.Find(a => !a.IsDelete && a.NodeID == (int)CloudWindSoftwareQXSJFlowNode.审核通过).ToList()
                            .ForEach(a => { node.NodeUsers.Add(new CloudWindNextNodeManage { ID = a.ID, ManageName = a.ManageName, ManageDepart = a.ManageDepart }); });
                        node.ID = (int)CloudWindSoftwareQXSJFlowNode.审核通过;
                        node.NodeName = CloudWindSoftwareQXSJFlowNode.审核通过.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return node;
        }

        /// <summary>
        /// 上传任务评论文件
        /// </summary>
        public List<CloudProjectFile> UploadCommentFile(CloudWindRequest request, List<IFormFile> files)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var commentFileRepo = uow.GetRepository<Flow_Task_CommentFile>();
                    string fileFolderPath = Path.Combine(_env.WebRootPath, "File", "Task", "CommentFile");

                    foreach (var file in files)
                    {
                        if (file != null)
                        {
                            string filePath = fileFolderPath + Guid.NewGuid() + Path.GetExtension(file.FileName);
                            FileUtils.SaveFile(file, filePath);

                            commentFileRepo.Add(new Flow_Task_CommentFile()
                            {
                                TaskID = request.TaskID,
                                FileName = file.FileName,
                                FilePath = filePath,
                                CreateTime = DateTime.Now,
                                IsDelete = false
                            });

                            list.Add(new CloudProjectFile()
                            {
                                FileName = file.FileName,
                                FilePath = FileUtils.GetRelativeFilePath(filePath)
                            });
                        }
                    }
                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return list;
        }

        /// <summary>
        /// 上传任务修改报告
        /// </summary>
        public List<CloudProjectFile> UploadModifyReport(CloudWindRequest request, List<IFormFile> files)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var reportRepo = uow.GetRepository<Wind_TaskReport>();
                    string fileFolderPath = Path.Combine(_env.WebRootPath, "File", "Task", "ModifyReport");

                    foreach (var file in files)
                    {
                        if (file != null)
                        {
                            string filePath = fileFolderPath + Guid.NewGuid() + Path.GetExtension(file.FileName);
                            FileUtils.SaveFile(file, filePath);

                            reportRepo.Add(new Wind_TaskReport()
                            {
                                TaskID = request.TaskID,
                                Type = 2,
                                ReportName = file.FileName,
                                ReportPath = filePath,
                                CreateTime = DateTime.Now,
                                IsDelete = false
                            });
                        }
                    }
                    uow.Save();

                    reportRepo.Find(a => a.TaskID == request.TaskID && a.Type == 2 && !a.IsDelete.Value).ToList()
                        .ForEach(a =>
                        {
                            list.Add(new CloudProjectFile()
                            {
                                ID = a.id,
                                FileName = a.ReportName,
                                FilePath = FileUtils.GetRelativeFilePath(a.ReportPath),
                                FileTime = a.CreateTime.Value.ToString("yyyy-MM-dd")
                            });
                        });

                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return list;
        }

        /// <summary>
        /// 加载任务文件
        /// </summary>
        public List<CloudProjectFile> LoadTaskFile(CloudWindRequest request)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskFileRepo = uow.GetRepository<Wind_TaskFile>();

                    taskFileRepo.Find(a => !(a.IsDelete == true) && a.TypeID == (int)CloudWindTaskSendFileType.可作业预报文件 && a.TaskID == request.TaskID)
                        .OrderByDescending(a => a.CreateTime)
                        .Take(10)
                        .ToList()
                        .ForEach(a =>
                        {
                            list.Add(new CloudProjectFile()
                            {
                                FileName = a.FileName ?? "",
                                FilePath = a.FilePath ?? "",
                                FileTime = a.CreateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return list;
        }

        /// <summary>
        /// 删除修改报告文件
        /// </summary>
        public List<CloudProjectFile> DeleteModifyFile(CloudWindRequest request)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var reportRepo = uow.GetRepository<Wind_TaskReport>();
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();

                    var modifyReport = reportRepo.Find(a => !(a.IsDelete == true) && a.id == request.ID).FirstOrDefault();
                    if (modifyReport == null) throw new Exception("数据错误");

                    modifyReport.IsDelete = true;
                    uow.Save();

                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == modifyReport.TaskID).FirstOrDefault();
                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == task.ProjectID).FirstOrDefault();

                    reportRepo.Find(a => !(a.IsDelete == true) && a.TaskID == modifyReport.TaskID && a.Type == 2 && a.id != request.ID)
                        .ToList()
                        .ForEach(a =>
                        {
                            string fileName = a.ReportPath.Substring(a.ReportPath.LastIndexOf('/') + 1);
                            list.Add(new CloudProjectFile()
                            {
                                ID = a.id,
                                FileName = a.ReportName,
                                FilePath = "/File/Project/" + project.ProjectCode + "/Task/" + task.TaskCode + @"/Export/" + fileName,
                                FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return list;
        }

        /// <summary>
        /// 加载地勘文件
        /// </summary>
        public CloudWindDKFile LoadDKFile(CloudWindRequest request)
        {
            CloudWindDKFile file = new CloudWindDKFile();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    int projectId = request.ProjectID;
                    if (projectId == 0)
                    {
                        var taskRepo = uow.GetRepository<Wind_Task>();
                        var task = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                        if (task == null) throw new Exception("数据错误");
                        projectId = task.ProjectID;
                    }

                    var geologyRepo = uow.GetRepository<Library_Geology>();
                    var geologyFiles = geologyRepo.Find(a => !a.IsDelete && a.ProjectID == projectId).ToList();

                    // 地勘原始资料
                    geologyFiles.Where(a => a.Type == (int)CloudWindTaskFileType.风电项目_机孔位地勘原始资料)
                        .ToList()
                        .ForEach(a =>
                        {
                            file.File_dkyszl.Add(new CloudProjectFile()
                            {
                                ID = a.ID,
                                FileName = a.FileName,
                                FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                            });
                        });

                    // 地勘数据表
                    geologyFiles.Where(a => a.Type == (int)CloudWindTaskFileType.风电项目_机孔位地勘数据表)
                        .ToList()
                        .ForEach(a =>
                        {
                            file.File_dksjb.Add(new CloudProjectFile()
                            {
                                FileName = a.FileName,
                                FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return file;
        }

        /// <summary>
        /// 删除地勘文件
        /// </summary>
        public List<CloudProjectFile> DeleteDKFile(CloudWindRequest request)
        {
            List<CloudProjectFile> files = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var geologyRepo = uow.GetRepository<Library_Geology>();

                    var data = geologyRepo.Find(a => !a.IsDelete && a.ID == request.ID).FirstOrDefault();
                    if (data == null) throw new Exception("数据错误");

                    data.IsDelete = true;
                    uow.Save();

                    geologyRepo.Find(a => !a.IsDelete && a.ProjectID == data.ProjectID && a.Type == data.Type)
                        .ToList()
                        .ForEach(a =>
                        {
                            files.Add(new CloudProjectFile()
                            {
                                ID = a.ID,
                                FileName = a.FileName,
                                FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return files;
        }

        /// <summary>
        /// 获取任务报告文件
        /// </summary>
        public CloudWindTaskFile GetTaskReport(CloudWindRequest request)
        {
            CloudWindTaskFile file = null;
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();

                    var thisTask = taskRepo.Find(a => !a.IsDelete && a.ID == request.TaskID).FirstOrDefault();
                    if (thisTask == null) throw new Exception("数据错误");

                    var thisProject = projectRepo.Find(a => !a.IsDelete && a.ID == thisTask.ProjectID).FirstOrDefault();
                    if (thisProject == null) throw new Exception("数据错误");

                    file = new CloudWindTaskFile();
                    file.TaskCode = thisTask.TaskCode;
                    file.TypeID = thisTask.SoftwareID ?? 0;
                    file.Type = Enum.GetName(typeof(CloudWindSoftware), thisTask.SoftwareID) ?? "";
                    file.TaskName1 = thisProject.ProjectName;
                    file.TaskTime = thisTask.CreateTime?.ToString("yyyy-MM-dd") ?? "";

                    if (thisTask.SoftwareID == (int)CloudWindSoftware.自升式平台桩腿插拔计算)
                    {
                        file.TaskName2 = thisTask.TaskName;

                        // 输出文件
                        var reportRepo = uow.GetRepository<Wind_TaskReport>();
                        reportRepo.Find(a => !(a.IsDelete == true) && a.TaskID == thisTask.ID && a.Type == 2).ToList()
                            .ForEach(a =>
                            {
                                file.ExportFiles.Add(new CloudProjectFile()
                                {
                                    ID = a.id,
                                    FileName = a.ReportName,
                                    FilePath = FileUtils.GetRelativeFilePath(a.ReportPath),
                                    FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                                });
                            });

                        // 地勘数据
                        var geologyRepo = uow.GetRepository<Library_Geology>();
                        geologyRepo.Find(a => !a.IsDelete && a.ProjectID == thisProject.ID).ToList()
                            .ForEach(a =>
                            {
                                file.ImportFiles.Add(new CloudProjectFile()
                                {
                                    ID = a.ID,
                                    FileName = a.FileName,
                                    FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                    FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                                });
                            });

                        // 平台船
                        var shipFileRepo = uow.GetRepository<Flow_Task_ShipFile>();
                        shipFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == thisTask.ID).ToList()
                            .ForEach(a =>
                            {
                                file.ImportFiles.Add(new CloudProjectFile()
                                {
                                    ID = a.id,
                                    FileName = a.FileName,
                                    FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                    FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                                });
                            });
                    }
                    else if (thisTask.SoftwareID == (int)CloudWindSoftware.起重船基础施工可作业性预报)
                    {
                        var taskInfoRepo = uow.GetRepository<Wind_TaskInfo_KZY>();
                        var thisTaskInfo = taskInfoRepo.Find(a => !(a.IsDelete == true) && a.TaskID == thisTask.ID).FirstOrDefault();
                        if (thisTaskInfo == null || string.IsNullOrEmpty(thisTaskInfo.ShipIDs))
                            throw new Exception("数据错误");

                        string dateString = thisTask.CreateTime?.Year + "年" + thisTask.CreateTime?.Month + "月" + thisTask.CreateTime?.Day + "日";
                        file.TaskName2 = "起重船基础施工可作业性预报(" + dateString + ")";

                        // 输出文件
                        var taskFileRepo = uow.GetRepository<Wind_TaskFile>();
                        taskFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == thisTask.ID
                            && a.TypeID == (int)CloudWindTaskSendFileType.可作业预报文件
                            && EF.Functions.Like(a.FileName, thisProject.ProjectCode + "%"))
                            .OrderByDescending(a => a.CreateTime)
                            .Take(request.ReportSize)
                            .ToList()
                            .ForEach(a =>
                            {
                                file.ExportFiles.Add(new CloudProjectFile()
                                {
                                    ID = a.id,
                                    FileName = a.FileName,
                                    FilePath = a.FilePath,
                                    FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                                });
                            });
                    }
                    else if (thisTask.SoftwareID == (int)CloudWindSoftware.桩基沉桩可打性分析)
                    {
                        // 项目位置图片
                        var imgRepo = uow.GetRepository<Wind_TaskInfoImg_ZJCZ>();
                        imgRepo.Find(a => !(a.IsDelete == true) && a.TaskID == thisTask.ID).ToList()
                            .ForEach(a =>
                            {
                                file.ImportFiles.Add(new CloudProjectFile()
                                {
                                    ID = a.id,
                                    FileName = a.FileName,
                                    FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                    FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                                });
                            });

                        // 桩文件
                        var pileRepo = uow.GetRepository<Library_Pile>();
                        pileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == thisTask.ID).ToList()
                            .ForEach(a =>
                            {
                                file.ImportFiles.Add(new CloudProjectFile()
                                {
                                    ID = a.id,
                                    FileName = a.FileName,
                                    FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                    FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                                });
                            });

                        // 地勘数据
                        var geologyRepo = uow.GetRepository<Library_Geology>();
                        geologyRepo.Find(a => !a.IsDelete && a.ProjectID == thisProject.ID).ToList()
                            .ForEach(a =>
                            {
                                file.ImportFiles.Add(new CloudProjectFile()
                                {
                                    ID = a.ID,
                                    FileName = a.FileName,
                                    FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                    FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                                });
                            });

                        // 报告
                        var reportRepo = uow.GetRepository<Wind_TaskReport>();
                        reportRepo.Find(a => !(a.IsDelete == true) && a.TaskID == thisTask.ID).ToList()
                            .ForEach(a =>
                            {
                                file.ExportFiles.Add(new CloudProjectFile()
                                {
                                    ID = a.id,
                                    FileName = a.ReportName,
                                    FilePath = FileUtils.GetRelativeFilePath(a.ReportPath),
                                    FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                                });
                            });
                    }
                    else if (thisTask.SoftwareID == (int)CloudWindSoftware.拖航可行性分析与预报)
                    {
                        // 输入文件 - 相关文件
                        var projectFileRepo = uow.GetRepository<Wind_Task_ProjectFile>();
                        projectFileRepo.Find(a => !(a.IsDelete == true) && a.ProjectID == thisProject.ID).ToList()
                            .ForEach(a =>
                            {
                                file.ImportFiles.Add(new CloudProjectFile()
                                {
                                    ID = a.id,
                                    FileName = a.FileName,
                                    FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                    FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                                });
                            });

                        // 输出文件
                        var taskFileRepo = uow.GetRepository<Wind_TaskFile>();
                        taskFileRepo.Find(a => !(a.IsDelete == true) && a.TaskID == thisTask.ID).ToList()
                            .ForEach(a =>
                            {
                                file.ExportFiles.Add(new CloudProjectFile()
                                {
                                    ID = a.id,
                                    FileName = a.FileName,
                                    FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                    FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                                });
                            });
                    }
                    else if (thisTask.SoftwareID == (int)CloudWindSoftware.基于历史环境数据的选船分析)
                    {
                        // 输入文件 - 相关文件
                        var projectFileRepo = uow.GetRepository<Wind_Task_ProjectFile>();
                        projectFileRepo.Find(a => !(a.IsDelete == true) && a.ProjectID == thisProject.ID).ToList()
                            .ForEach(a =>
                            {
                                file.ImportFiles.Add(new CloudProjectFile()
                                {
                                    ID = a.id,
                                    FileName = a.FileName,
                                    FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                                    FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                                });
                            });

                        // 输出文件
                        var reportRepo = uow.GetRepository<Wind_TaskReport>();
                        reportRepo.Find(a => !(a.IsDelete == true) && a.TaskID == thisTask.ID).ToList()
                            .ForEach(a =>
                            {
                                file.ExportFiles.Add(new CloudProjectFile()
                                {
                                    ID = a.id,
                                    FileName = a.ReportName,
                                    FilePath = FileUtils.GetRelativeFilePath(a.ReportPath),
                                    FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                                });
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return file;
        }

        /// <summary>
        /// 上传报告文件
        /// </summary>
        public List<CloudProjectFile> UploadFileReport(CloudWindRequest request, List<IFormFile> files)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var reportRepo = uow.GetRepository<Wind_TaskReport>();
                    string fileFolderPath = Path.Combine(_env.WebRootPath, "File", "Task", "ReportFile");

                    foreach (var file in files)
                    {
                        if (file != null)
                        {
                            string filePath = fileFolderPath + Guid.NewGuid() + Path.GetExtension(file.FileName);
                            FileUtils.SaveFile(file, filePath);

                            reportRepo.Add(new Wind_TaskReport()
                            {
                                TaskID = request.TaskID,
                                Type = request.FileType,
                                ReportName = file.FileName,
                                ReportPath = filePath,
                                CreateTime = DateTime.Now,
                                IsDelete = false
                            });

                            list.Add(new CloudProjectFile()
                            {
                                FileName = file.FileName,
                                FilePath = FileUtils.GetRelativeFilePath(filePath)
                            });
                        }
                    }
                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return list;
        }

        /// <summary>
        /// 删除报告文件
        /// </summary>
        public List<CloudProjectFile> DeleteReportFile(CloudWindRequest request)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var reportRepo = uow.GetRepository<Wind_TaskReport>();

                    var deleteReport = reportRepo.Find(a => !(a.IsDelete == true) && a.id == request.ID).FirstOrDefault();
                    if (deleteReport == null) throw new Exception("数据错误");

                    deleteReport.IsDelete = true;
                    uow.Save();

                    reportRepo.Find(a => !(a.IsDelete == true) && a.TaskID == deleteReport.TaskID)
                        .ToList()
                        .ForEach(a =>
                        {
                            list.Add(new CloudProjectFile()
                            {
                                ID = a.id,
                                FileName = a.ReportName,
                                FilePath = FileUtils.GetRelativeFilePath(a.ReportPath),
                                FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return list;
        }

        /// <summary>
        /// 加载项目位置
        /// </summary>
        public string LoadProjectPosition(CloudWindRequest request)
        {
            string position = "未知";
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("数据错误");

                    if (!string.IsNullOrEmpty(project.Lon) && !string.IsNullOrEmpty(project.Lat))
                    {
                        position = "东经：" + project.Lon + "  北纬：" + project.Lat;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return position;
        }

        /// <summary>
        /// 拖航路线坐标下载
        /// </summary>
        public (byte[] bytes, string fileName) TowingPlanRouteDownload(CloudWindRequest request)
        {
            try
            {
                using (var techUow = _techCenterUowFactory.Create())
                using (var towingUow = _towingUowFactory.Create())
                {
                    var projectRepo = techUow.GetRepository<Wind_Project>();
                    var towingProjectRepo = towingUow.GetRepository<Manage_Project>();
                    var planRouteRepo = towingUow.GetRepository<Manage_PlanRoute>();

                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == request.ProjectID).FirstOrDefault();
                    if (project == null) throw new Exception("项目数据错误");

                    var towingProject = towingProjectRepo.Find(a => !a.IsDelete && a.CloudProjectID == project.ID).FirstOrDefault();
                    if (towingProject == null) throw new Exception("拖航项目数据错误");

                    DataTable dt = new DataTable();
                    dt.Columns.Add("纬度（°）：", typeof(string));
                    dt.Columns.Add("经度（°）：", typeof(string));

                    var positions = planRouteRepo.Find(a => !a.IsDelete && a.ProjectID == towingProject.id).ToList();
                    foreach (var item in positions)
                    {
                        DataRow dr = dt.NewRow();
                        dr[0] = item.Lat == null ? "" : item.Lat.ToString();
                        dr[1] = item.Lon == null ? "" : item.Lon.ToString();
                        dt.Rows.Add(dr);
                    }

                    byte[] bytes = ExcelUtils.TableToBytes(dt, "拖航路线坐标");
                    string fileName = project.ProjectName + "_拖航路线坐标_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                    return (bytes, fileName);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
        }

        /// <summary>
        /// 删除选船分析报告文件
        /// </summary>
        public List<CloudProjectFile> DeleteXCFXFile(CloudWindRequest request)
        {
            List<CloudProjectFile> list = new List<CloudProjectFile>();
            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var reportRepo = uow.GetRepository<Wind_TaskReport>();
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var projectRepo = uow.GetRepository<Wind_Project>();

                    var modifyReport = reportRepo.Find(a => !(a.IsDelete == true) && a.id == request.ID).FirstOrDefault();
                    if (modifyReport == null) throw new Exception("数据错误");

                    modifyReport.IsDelete = true;
                    uow.Save();

                    var task = taskRepo.Find(a => !a.IsDelete && a.ID == modifyReport.TaskID).FirstOrDefault();
                    var project = projectRepo.Find(a => !a.IsDelete && a.ID == task.ProjectID).FirstOrDefault();

                    reportRepo.Find(a => !(a.IsDelete == true) && a.TaskID == modifyReport.TaskID
                        && a.Type == (int)CloudWindTaskReportType.选船分析报告 && a.id != request.ID)
                        .ToList()
                        .ForEach(a =>
                        {
                            string fileName = a.ReportPath.Substring(a.ReportPath.LastIndexOf('/') + 1);
                            list.Add(new CloudProjectFile()
                            {
                                ID = a.id,
                                FileName = a.ReportName,
                                FilePath = "/File/Project/" + project.ProjectCode + "/Task/" + task.TaskCode + @"/XCFX/" + fileName,
                                FileTime = a.CreateTime?.ToString("yyyy-MM-dd") ?? ""
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TaskService));
                throw;
            }
            return list;
        }

        #region 私有辅助方法

        /// <summary>
        /// 安全读取 DataTable 单元格值
        /// </summary>
        private string GetCell(DataTable dt, int row, int col)
        {
            if (dt.Rows.Count <= row || dt.Columns.Count <= col)
                return "";
            return dt.Rows[row][col]?.ToString() ?? "";
        }

        /// <summary>
        /// 安全转换字符串为数值并四舍五入保留2位小数
        /// </summary>
        private string SafeRound(string value)
        {
            if (string.IsNullOrEmpty(value)) return "0";
            if (double.TryParse(value, out double num))
                return Math.Round(num, 2).ToString();
            return value;
        }

        #endregion

    }
}