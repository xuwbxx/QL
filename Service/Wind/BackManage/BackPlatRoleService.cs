using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Model.Tech.Cloud.BackManage;
using Tool;

namespace Service.Wind.BackManage
{
    public class BackPlatRoleService
    {
        private readonly CloudWind_KingBase_UnitOfWorkFactory _cloudWindUowFactory;

        public BackPlatRoleService(CloudWind_KingBase_UnitOfWorkFactory cloudWindUowFactory)
        {
            _cloudWindUowFactory = cloudWindUowFactory;
        }

        public (List<CloudWindManageProjectRoleData> list, int totalCount, int pageIndex, string msg) ListQuery(CloudWindBackManageRequest request)
        {
            List<CloudWindManageProjectRoleData> list = new List<CloudWindManageProjectRoleData>();
            string msg = "";
            int totalCount = 0;
            int pageIndex = request.PageIndex;

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var repo = uow.GetRepository<Manage_Viewer>();

                    var predicate = PredicateBuilder.True<Manage_Viewer>();
                    predicate = PredicateBuilder.And(predicate, a => !a.IsDelete);

                    if (!string.IsNullOrEmpty(request.UserName))
                    {
                        var userName = request.UserName;
                        predicate = PredicateBuilder.And(predicate, a => a.UserName != null && a.UserName.Contains(userName));
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
                        list.Add(new CloudWindManageProjectRoleData()
                        {
                            ID = a.ID,
                            UserName = a.UserName ?? "",
                            UserDepartName = a.UserDepartName ?? "",
                            UserJobName = a.UserJobName ?? "",
                            RoleName = "数据浏览"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackPlatRoleService));
                msg = "发生错误";
            }

            return (list, totalCount, pageIndex, msg);
        }

        public string DataSave(CloudWindBackManageRequest request)
        {
            string msg = "";

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var repo = uow.GetRepository<Manage_Viewer>();

                    var user = request.ProjectRoles[0];
                    if (string.IsNullOrEmpty(user.UserCode))
                    {
                        return "没有任何用户信息";
                    }

                    var existList = repo.Find(a => !a.IsDelete && a.UserCode == user.UserCode).ToList();
                    if (existList.Count != 0)
                    {
                        return "存在相同用户名";
                    }

                    var newViewer = new Manage_Viewer()
                    {
                        UserName = user.UserName,
                        UserCode = user.UserCode,
                        UserDepartName = user.UserDepartName,
                        UserJobName = user.UserJobName,
                        UserPhone = user.UserPhone,
                        CreateTime = DateTime.UtcNow,
                        IsDelete = false
                    };

                    repo.Add(newViewer);
                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackPlatRoleService));
                msg = "发生错误，请联系管理员";
            }

            return msg;
        }

        public string DataDelete(CloudWindBackManageRequest request)
        {
            string msg = "";

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var repo = uow.GetRepository<Manage_Viewer>();
                    var data = repo.FindFirst(a => !a.IsDelete && a.ID == request.ID);

                    if (data == null)
                    {
                        return "数据错误";
                    }

                    data.IsDelete = true;
                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackPlatRoleService));
                msg = "发生错误，请联系管理员";
            }

            return msg;
        }
    }
}
