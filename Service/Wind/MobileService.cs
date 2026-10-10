using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Model.Base;
using Model.Tech.Cloud;
using Model.TechCenter.JJT;
using Service.Base;
using Tool;

namespace Service.Wind
{
    public class MobileService : WindBaseService
    {
        private readonly ProjectService _projectService;
        private readonly TaskService _taskService;
        private readonly LoginService _loginService;
        private readonly CloudWindInfoService _cloudWindInfoService;

        public MobileService(
            CloudWind_KingBase_UnitOfWorkFactory techCenterUowFactory,
            CookieService cookieService,
            ProjectService projectService,
            TaskService taskService,
            LoginService loginService,
            CloudWindInfoService cloudWindInfoService)
            : base(techCenterUowFactory, cookieService)
        {
            _projectService = projectService;
            _taskService = taskService;
            _loginService = loginService;
            _cloudWindInfoService = cloudWindInfoService;
        }

        /// <summary>
        /// SSO单点登录：解密token并获取用户信息
        /// </summary>
        public (SHJUserInfo? userInfo, string errorMsg) ProcessSSOLogin(string paramString)
        {
            try
            {
                SingleSinOnModel ssoModel = new SingleSinOnModel();
                string ssoResult = _cloudWindInfoService.AESSingleSignOn(paramString, out ssoModel);
                if (!string.IsNullOrEmpty(ssoResult))
                {
                    return (null, "身份信息错误");
                }

                var shjUser = _cloudWindInfoService.GetShjUserInfo(ssoModel.usrCode);
                if (shjUser.StatusCode != 200 || shjUser.Data == null || shjUser.Data.Count != 1)
                {
                    return (null, "身份信息错误");
                }

                SHJUserInfo userinfo = new SHJUserInfo();
                userinfo.UserID = shjUser.Data[0].ID;
                userinfo.UserName = shjUser.Data[0].namespell;
                userinfo.UserCode = shjUser.Data[0].empcode;
                userinfo.RealName = shjUser.Data[0].name;
                userinfo.DepartName = shjUser.Data[0].wholeDeptPath;
                userinfo.Email = shjUser.Data[0].email;
                userinfo.Mobile = shjUser.Data[0].phone;
                userinfo.Birthday = shjUser.Data[0].birthday;
                userinfo.JobName = shjUser.Data[0].jobname;

                return (userinfo, "");
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MobileService));
                return (null, "身份错误，请联系管理员。");
            }
        }

        /// <summary>
        /// 保存登录记录
        /// </summary>
        public void SaveLoginRecord(SHJUserInfo user)
        {
            _loginService.JJTLoginRecord(user);
        }

        /// <summary>
        /// 根据ID获取项目
        /// </summary>
        public Wind_Project? GetProjectByID(int projectID)
        {
            return _projectService.getProjectByID(projectID);
        }

        /// <summary>
        /// 检查项目浏览权限
        /// </summary>
        public bool HasProjectViewRight(int projectID, string userCode)
        {
            return _projectService.hasProjectViewRight(projectID, userCode);
        }

        /// <summary>
        /// 获取项目列表（手机端）
        /// </summary>
        public async Task<(List<CloudWindProject> list, string msg)> GetMobileProjectList(CloudWindRequest request)
        {
            var (list, msg) = await _projectService.GetProjectList(request);
            return (list, msg);
        }

        /// <summary>
        /// 获取项目详情（手机端）
        /// </summary>
        public async Task<(CloudWindProjectFlow data, string msg)> GetMobileProjectInfo(CloudWindRequest request)
        {
            var (data, msg) = await _projectService.GetProject(request);
            return (data, msg);
        }

        /// <summary>
        /// 根据ID获取任务
        /// </summary>
        public Wind_Task? GetTaskByID(int taskID)
        {
            return _taskService.GetTaskById(taskID);
        }

        /// <summary>
        /// 检查任务浏览权限
        /// </summary>
        public async Task<bool> HasTaskViewRight(int taskID, string userCode)
        {
            return await _taskService.HasTaskViewRight(taskID, userCode);
        }

        /// <summary>
        /// 获取任务列表（手机端）
        /// </summary>
        public async Task<List<CloudWindTask>> GetMobileTaskList(CloudWindRequest request)
        {
            return await _taskService.TaskListQuery(request);
        }

        /// <summary>
        /// 获取CBZ任务详情（手机端）
        /// </summary>
        public CloudWindTaskFlow GetCBZTaskInfo(CloudWindRequest request)
        {
            return _taskService.TaskQuery_CBZ(request);
        }

        /// <summary>
        /// 获取KZY任务详情及报告文件（手机端）
        /// </summary>
        public CloudWindTaskFlow GetKZYTASKInfoWithFiles(CloudWindRequest request)
        {
            var data = _taskService.TaskQuery_KZY(request);

            // 查询KZY可作业预报文件
            using (var uow = _techCenterUowFactory.Create())
            {
                var fileRepo = uow.GetRepository<Wind_TaskFile>();
                var files = fileRepo.Find(a => !(a.IsDelete == true)
                    && a.TypeID == (int)CloudWindTaskSendFileType.可作业预报文件
                    && a.TaskID == request.TaskID)
                    .OrderByDescending(a => a.CreateTime)
                    .Take(10)
                    .ToList();

                if (data.Report != null)
                {
                    foreach (var a in files)
                    {
                        data.Report.KZYReportFile.Add(new CloudProjectFile()
                        {
                            FileName = a.FileName,
                            FilePath = a.FilePath,
                            FileTime = a.CreateTime.HasValue
                                ? a.CreateTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
                                : ""
                        });
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// 获取KZY船舶列表
        /// </summary>
        public List<Library_Ship_KZY> GetKZYShipList()
        {
            return _taskService.GetKZYShipList();
        }
    }
}
