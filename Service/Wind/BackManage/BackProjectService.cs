using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Microsoft.AspNetCore.Hosting;
using Model.Tech.Cloud;
using Model.Tech.Cloud.BackManage;
using Tool;

namespace Service.Wind.BackManage
{
    public class BackProjectService
    {
        private readonly CloudWind_KingBase_UnitOfWorkFactory _techCenterUowFactory;
        private readonly IWebHostEnvironment _env;

        public BackProjectService(CloudWind_KingBase_UnitOfWorkFactory techCenterUowFactory, IWebHostEnvironment env)
        {
            _techCenterUowFactory = techCenterUowFactory;
            _env = env;
        }

        public (List<CloudWindManageProjectResponse> list, int totalCount, int pageIndex, string msg) ProjectListQuery(CloudWindBackManageRequest request)
        {
            List<CloudWindManageProjectResponse> list = new List<CloudWindManageProjectResponse>();
            string msg = "";
            int totalCount = 0;
            int pageIndex = request.PageIndex;

            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var repo = uow.GetRepository<Wind_Project>();

                    var predicate = PredicateBuilder.True<Wind_Project>();
                    predicate = PredicateBuilder.And(predicate, a => !a.IsDelete && a.FlowStatus == (int)CloudWindProjectFlowStatus.激活);

                    if (!string.IsNullOrEmpty(request.ProjectName))
                    {
                        var projectName = request.ProjectName;
                        predicate = PredicateBuilder.And(predicate, a => a.ProjectName.Contains(projectName));
                    }

                    var (pageList, count) = repo.FindPage(predicate, a => a.ID, request.PageIndex, request.PageSize);

                    totalCount = count;

                    //如果不是第一页，并且没有数据，则查询第一页
                    if (request.PageIndex != 1 && pageList.Count() == 0)
                    {
                        pageIndex = 1;
                        (pageList, count) = repo.FindPage(predicate, a => a.ID, 1, request.PageSize);
                    }

                    foreach (var a in pageList)
                    {
                        list.Add(new CloudWindManageProjectResponse()
                        {
                            ID = a.ID,
                            ProjectName = a.ProjectName,
                            ProjectCode = a.ProjectCode,
                            StartTime = a.ProjectStartTime != null ? a.ProjectStartTime.Value.ToString("yyyy-MM-dd") : "",
                            EndTime = a.ProjectEndTime != null ? a.ProjectEndTime.Value.ToString("yyyy-MM-dd") : "",
                            Status = a.Status ?? 0,
                            StatusName = Enum.GetName(typeof(CloudWindProjectStatus), a.Status) ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackProjectService));
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
                using (var uow = _techCenterUowFactory.Create())
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
                LoggerUtils.Error(ex.ToString(), typeof(BackProjectService));
                msg = "发生错误";
            }

            return (project, msg);
        }

        public (string filePath, string fileName, string msg) ProjectFileExport(CloudWindBackManageRequest request)
        {
            string filePath = "";
            string fileName = "";
            string msg = "";

            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var repo = uow.GetRepository<Wind_Project>();
                    var project = repo.FindFirst(a => a.ID == request.ProjectID);

                    if (project == null)
                    {
                        msg = "项目数据错误";
                        return (filePath, fileName, msg);
                    }

                    var directoryPath = Path.Combine(_env.WebRootPath, "File", "Project", project.ProjectCode);
                    var tempPath = Path.Combine(_env.WebRootPath, "File", "Project", "TempFile", project.ProjectName + "(" + project.ProjectCode + ").zip");

                    ZipUtils.CompressDirectory(directoryPath, tempPath);

                    filePath = tempPath;
                    fileName = project.ProjectName + "(" + project.ProjectCode + ").zip";
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackProjectService));
                msg = "发生错误，请检查路径是否存在。";
            }

            return (filePath, fileName, msg);
        }

    }
}
