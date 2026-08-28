using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Microsoft.AspNetCore.Hosting;
using Model.Tech.Cloud;
using Model.Tech.Cloud.BackManage;
using Tool;

namespace Service.Wind.BackManage
{
    public class BackLibraryService
    {
        private readonly CloudWind_KingBase_UnitOfWorkFactory _cloudWindUowFactory;
        private readonly IWebHostEnvironment _env;

        public BackLibraryService(CloudWind_KingBase_UnitOfWorkFactory cloudWindUowFactory, IWebHostEnvironment env)
        {
            _cloudWindUowFactory = cloudWindUowFactory;
            _env = env;
        }

        public (List<CloudWindManageShip> list, int totalCount, int pageIndex, string msg) ShipListQuery(CloudWindBackManageRequest request)
        {
            List<CloudWindManageShip> list = new List<CloudWindManageShip>();
            string msg = "";
            int totalCount = 0;
            int pageIndex = request.PageIndex;

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var repo = uow.GetRepository<Library_Ship>();

                    var predicate = PredicateBuilder.True<Library_Ship>();
                    predicate = PredicateBuilder.And(predicate, a => !(a.IsDelete == true));

                    if (!string.IsNullOrEmpty(request.ShipName))
                    {
                        var shipName = request.ShipName;
                        predicate = PredicateBuilder.And(predicate, a => a.ShipName != null && a.ShipName.Contains(shipName));
                    }

                    if (request.ShipIsConfirm != 0)
                    {
                        var isConfirm = request.ShipIsConfirm == 1;
                        predicate = PredicateBuilder.And(predicate, a => a.IsConfirm == isConfirm);
                    }

                    var (pageList, count) = repo.FindPage(predicate, a => a.ID, request.PageIndex, request.PageSize);

                    totalCount = count;

                    if (request.PageIndex != 1 && pageList.Count() == 0)
                    {
                        pageIndex = 1;
                        (pageList, totalCount) = repo.FindPage(predicate, a => a.ID, 1, request.PageSize);
                    }

                    foreach (var a in pageList)
                    {
                        list.Add(new CloudWindManageShip()
                        {
                            ID = a.ID,
                            ShipName = a.ShipName ?? "",
                            IsConfirm = a.IsConfirm == true,
                            ShipConfirm = a.IsConfirm == true ? "已确认" : "未确认"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackLibraryService));
                msg = "发生错误";
            }

            return (list, totalCount, pageIndex, msg);
        }

        public (CloudWindLibraryShip? ship, string msg) ShipDataQuery(CloudWindBackManageRequest request)
        {
            CloudWindLibraryShip? ship = null;
            string msg = "";

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var repo = uow.GetRepository<View_Library_Ship>();
                    var shipInfo = repo.FindFirst(a => a.ID == request.ShipID);

                    if (shipInfo == null)
                    {
                        msg = "船舶数据错误";
                        return (ship, msg);
                    }

                    ship = new CloudWindLibraryShip()
                    {
                        ID = shipInfo.ID,
                        ShipName = shipInfo.ShipName ?? "",
                        BZL = shipInfo.bzl ?? "",
                        DDBY = shipInfo.ddby ?? "",
                        JSYYHZ = shipInfo.jsyyhz ?? "",
                        QX = shipInfo.qx_cddsm ?? "",
                        YXZTCD = shipInfo.yxztcd_cddxd ?? "",
                        ZTYXCD = shipInfo.ztyxcd ?? "",
                        ZTYYL = shipInfo.ztyyl ?? "",
                        ZTZC = shipInfo.ztzc ?? "",
                        ZTJMJ = shipInfo.ztjmj ?? "",
                        ZTZXZZ = shipInfo.ztzxzzW ?? "",
                        ZXCD = shipInfo.zxcdL ?? "",
                        ZXGD = shipInfo.zxgdH ?? "",
                        ZXKD = shipInfo.zxkdB ?? "",
                        ZXMJ = shipInfo.zxmjA ?? "",
                        ZXTJ = shipInfo.zxtjV ?? "",
                        ZXZDJMZC = shipInfo.zxzdjmzc ?? "",
                        ZTZJ = shipInfo.ztzj ?? "",
                        IsConfirm = shipInfo.IsConfirm == true
                    };
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackLibraryService));
                msg = "发生错误";
            }

            return (ship, msg);
        }

        public string ShipDataSave(CloudWindBackManageRequest request)
        {
            string msg = "";

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var shipRepo = uow.GetRepository<Library_Ship>();
                    var dataRepo = uow.GetRepository<Library_Ship_Data>();

                    var ship = shipRepo.FindFirst(a => a.ID == request.ShipID && !(a.IsDelete == true));
                    if (ship == null)
                    {
                        return "船舶数据错误";
                    }

                    var shipData = dataRepo.FindFirst(a => !(a.IsDelete == true) && a.ShipID == ship.ID);
                    if (shipData == null)
                    {
                        return "船舶数据错误";
                    }

                    ship.IsConfirm = request.IsConfirm;

                    shipData.ztzj = request.Ship_ZTZJ;
                    shipData.ztjmj = request.Ship_ZTJMJ;
                    shipData.ztzc = request.Ship_ZTZC;
                    shipData.zxcdL = request.Ship_ZXCD;
                    shipData.zxkdB = request.Ship_ZXKD;
                    shipData.zxgdH = request.Ship_ZXGD;
                    shipData.zxmjA = request.Ship_ZXMJ;
                    shipData.zxzdjmzc = request.Ship_ZXZDJMZC;
                    shipData.zxtjV = request.Ship_ZXTJ;
                    shipData.ztzxzzW = request.Ship_ZTZXZZ;
                    shipData.ztyyl = request.Ship_ZTYYL;
                    shipData.jsyyhz = request.Ship_JSYYHZ;
                    shipData.bzl = request.Ship_BZL;
                    shipData.ddby = request.Ship_DDBY;
                    shipData.yxztcd_cddxd = request.Ship_YXZTCD;
                    shipData.qx_cddsm = request.Ship_QX;
                    shipData.ztyxcd = request.Ship_ZTYXCD;

                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackLibraryService));
                msg = "发生错误，请联系管理员";
            }

            return msg;
        }

        public (List<CloudWindManageProjectResponse> list, int totalCount, int pageIndex, string msg) ProjectListQuery(CloudWindBackManageRequest request)
        {
            List<CloudWindManageProjectResponse> list = new List<CloudWindManageProjectResponse>();
            string msg = "";
            int totalCount = 0;
            int pageIndex = request.PageIndex;

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var repo = uow.GetRepository<Wind_Project>();

                    var predicate = PredicateBuilder.True<Wind_Project>();
                    predicate = PredicateBuilder.And(predicate, a => !a.IsDelete && a.FlowStatus == (int)CloudWindProjectFlowStatus.激活);

                    if (!string.IsNullOrEmpty(request.ProjectName))
                    {
                        var projectName = request.ProjectName;
                        predicate = PredicateBuilder.And(predicate, a => a.ProjectName != null && a.ProjectName.Contains(projectName));
                    }

                    var (pageList, count) = repo.FindPage(predicate, a => a.ID, request.PageIndex, request.PageSize);

                    totalCount = count;

                    if (request.PageIndex != 1 && pageList.Count() == 0)
                    {
                        pageIndex = 1;
                        (pageList, totalCount) = repo.FindPage(predicate, a => a.ID, 1, request.PageSize);
                    }

                    foreach (var a in pageList)
                    {
                        list.Add(new CloudWindManageProjectResponse()
                        {
                            ID = a.ID,
                            ProjectName = a.ProjectName ?? "",
                            ProjectCode = a.ProjectCode ?? "",
                            StartTime = a.ProjectStartTime.HasValue ? a.ProjectStartTime.Value.ToString("yyyy-MM-dd") : "",
                            EndTime = a.ProjectEndTime.HasValue ? a.ProjectEndTime.Value.ToString("yyyy-MM-dd") : "",
                            Status = a.Status ?? 0,
                            StatusName = Enum.GetName(typeof(CloudWindProjectStatus), a.Status ?? 0) ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackLibraryService));
                msg = "发生错误";
            }

            return (list, totalCount, pageIndex, msg);
        }

        public (Wind_Project? project, string msg) ProjectDataQuery(CloudWindBackManageRequest request)
        {
            Wind_Project? project = null;
            string msg = "";

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var repo = uow.GetRepository<Wind_Project>();
                    project = repo.FindFirst(a => !a.IsDelete && a.ID == request.ProjectID);

                    if (project == null)
                    {
                        msg = "项目数据错误";
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackLibraryService));
                msg = "发生错误";
            }

            return (project, msg);
        }

        public (string filePath, string fileName, string msg) DKFileExport(CloudWindBackManageRequest request)
        {
            string filePath = "";
            string fileName = "";
            string msg = "";

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var project = projectRepo.FindFirst(a => a.ID == request.ProjectID);

                    if (project == null)
                    {
                        msg = "项目数据错误";
                        return (filePath, fileName, msg);
                    }

                    var geologyRepo = uow.GetRepository<Library_Geology>();
                    var geologyFiles = geologyRepo.Find(a => !a.IsDelete && a.ProjectID == project.ID).ToList();

                    List<FileResponse> filesToZip = new List<FileResponse>();
                    foreach (var a in geologyFiles)
                    {
                        filesToZip.Add(new FileResponse()
                        {
                            FileName = a.FileName,
                            FilePath = a.FilePath
                        });
                    }

                    var exportName = project.ProjectName + "-地勘资料(" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ").zip";
                    var exportDir = Path.Combine(_env.WebRootPath, "File", "Project", "DKTempFile");
                    if (!Directory.Exists(exportDir))
                    {
                        Directory.CreateDirectory(exportDir);
                    }
                    filePath = Path.Combine(exportDir, exportName);
                    fileName = exportName;

                    ZipUtils.CreateZip(filePath, filesToZip);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackLibraryService));
                msg = "发生错误，请检查路径是否存在。";
            }

            return (filePath, fileName, msg);
        }

        public string DKFileDelete(CloudWindBackManageRequest request)
        {
            string msg = "";

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var repo = uow.GetRepository<Library_Geology>();
                    var geology = repo.FindFirst(a => a.ID == request.ID && !a.IsDelete);

                    if (geology == null)
                    {
                        return "地勘数据错误";
                    }

                    geology.IsDelete = true;
                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackLibraryService));
                msg = "发生错误，请联系管理员";
            }

            return msg;
        }
    }
}
