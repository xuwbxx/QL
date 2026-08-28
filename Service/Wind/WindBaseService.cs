using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Model.Base;
using Service.Base;

namespace Service.Wind
{
    public class WindBaseService
    {
        // 依赖注入：仓储工厂（核心）+ 日志（可选，用于异常追踪）
        public readonly CloudWind_KingBase_UnitOfWorkFactory _techCenterUowFactory;
        public CookieService _cookieService { get; }
        public WindBaseService(CloudWind_KingBase_UnitOfWorkFactory techCenterUowFactory, CookieService cookieService)
        {
            _techCenterUowFactory = techCenterUowFactory;
            _cookieService = cookieService;
        }

        public string UserCode
        {
            get
            {
                try
                {
                    SHJUserInfo user = _cookieService.GetUserCookie();

                    return user.UserCode;
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public SHJUserInfo CurrentUser
        {
            get
            {
                try
                {
                    SHJUserInfo user = _cookieService.GetUserCookie();

                    return user;
                }
                catch (Exception)
                {
                    return new SHJUserInfo();
                }
            }
        }

        public async Task<List<string>> GetAdminUserCode()
        {
            List<string> userCodes = new List<string>();

            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Manage_Admin>();

                var listAsy = await repo.FindAsync(a => !a.IsDelete);
                listAsy.ToList().ForEach(a =>
                {
                    userCodes.Add(a.UserCode);
                });
            }
            return userCodes;
        }

        public async Task<List<string>> GetViewUserCode()
        {
            List<string> userCodes = new List<string>();

            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Manage_Viewer>();

                var listAsy = await repo.FindAsync(a => !a.IsDelete);
                listAsy.ToList().ForEach(a =>
                {
                    userCodes.Add(a.UserCode);
                });
            }
            return userCodes;
        }

        public async Task<List<string>> GetCopyUserCode(int SoftwareID)
        {
            List<string> userCodes = new List<string>();
            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Manage_Copyer>();

                var listAsy = await repo.FindAsync(a => !a.IsDelete && a.SoftwareID == SoftwareID);
                listAsy.ToList().ForEach(a =>
                {
                    userCodes.Add(a.UserCode);
                });

            }
            return userCodes;
        }

        public async Task<List<string>> GetDeliverUserCode()
        {
            List<string> userCodes = new List<string>();
            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Wind_TaskFileDeliver>();

                var listAsy = await repo.FindAsync(a => !a.IsDelete);
                listAsy.ToList().ForEach(a =>
                {
                    userCodes.Add(a.DeliverCode);
                });
            }
            return userCodes;
        }

        public async Task<List<Wind_ProjectRole>> GetProjectRoles()
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Wind_ProjectRole>();
                var list = await repo.FindAsync(a => !a.IsDelete);
                return list.ToList();
            }
        }

        public async Task<List<Wind_Project>> GetProjects()
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Wind_Project>();
                var list = await repo.FindAsync(a => !a.IsDelete);
                return list.ToList();
            }
        }

        protected int CreateProjectCode()
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Wind_Project>();
                var LastestProject = repo.Find(a => !a.IsDelete).OrderByDescending(a => a.ProjectCodeIndex).FirstOrDefault();
                if (LastestProject == null)
                {
                    return 1;
                }
                else
                {
                    return LastestProject.ProjectCodeIndex.Value + 1;
                }
            }
        }

        protected int CreateTaskCode()
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Wind_Task>();
                var LastestTask = repo.Find(a => !a.IsDelete).OrderByDescending(a => a.ID).FirstOrDefault();
                if (LastestTask == null)
                {
                    return 1;
                }
                else
                {
                    string TaskCode = LastestTask.TaskCode;
                    int TaskNo = Convert.ToInt32(TaskCode.Substring(2));
                    return TaskNo + 1;
                }
            }
        }

    }
}
