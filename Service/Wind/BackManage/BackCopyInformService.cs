using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Model.Tech.Cloud.BackManage;
using Tool;

namespace Service.Wind.BackManage
{
    public class BackCopyInformService
    {
        private readonly CloudWind_KingBase_UnitOfWorkFactory _cloudWindUowFactory;

        public BackCopyInformService(CloudWind_KingBase_UnitOfWorkFactory cloudWindUowFactory)
        {
            _cloudWindUowFactory = cloudWindUowFactory;
        }

        public List<Manage_Software> GetSoftwareList()
        {
            using (var uow = _cloudWindUowFactory.Create())
            {
                var repo = uow.GetRepository<Manage_Software>();
                return repo.Find(a => !a.IsDelete).ToList();
            }
        }

        public (List<CloudWindManageCopyInform> list, int totalCount, int pageIndex, string msg) ListQuery(CloudWindBackManageRequest request)
        {
            List<CloudWindManageCopyInform> list = new List<CloudWindManageCopyInform>();
            string msg = "";
            int totalCount = 0;
            int pageIndex = request.PageIndex;

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var repo = uow.GetRepository<View_Manage_Copyer>();

                    var predicate = PredicateBuilder.True<View_Manage_Copyer>();

                    if (!string.IsNullOrEmpty(request.UserName))
                    {
                        var userName = request.UserName;
                        predicate = PredicateBuilder.And(predicate, a => a.UserName != null && a.UserName.Contains(userName));
                    }

                    if (request.SoftwareID != 0)
                    {
                        var softwareID = request.SoftwareID;
                        predicate = PredicateBuilder.And(predicate, a => a.SoftwareID == softwareID);
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
                        list.Add(new CloudWindManageCopyInform()
                        {
                            ID = a.ID,
                            Software = a.SoftwareName ?? "",
                            UserName = a.UserName ?? "",
                            UserCode = a.UserCode ?? "",
                            UserDepart = a.UserDepart ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackCopyInformService));
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
                    var repo = uow.GetRepository<Manage_Copyer>();

                    var user = request.NodeManagers[0];
                    if (user == null || string.IsNullOrEmpty(user.UserCode))
                    {
                        return "没有任何用户信息";
                    }

                    var existUser = repo.FindFirst(a => !a.IsDelete && a.SoftwareID == request.SoftwareID && a.UserCode == user.UserCode);
                    if (existUser != null)
                    {
                        return "存在相同用户名";
                    }

                    var newUser = new Manage_Copyer()
                    {
                        SoftwareID = request.SoftwareID,
                        UserName = user.UserName,
                        UserCode = user.UserCode,
                        UserDepart = user.UserDepartName ?? "",
                        UserPhone = user.UserPhone ?? "",
                        UserJobName = user.UserJobName ?? "",
                        CreateTime = DateTime.UtcNow,
                        IsDelete = false
                    };

                    repo.Add(newUser);
                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackCopyInformService));
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
                    var repo = uow.GetRepository<Manage_Copyer>();
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
                LoggerUtils.Error(ex.ToString(), typeof(BackCopyInformService));
                msg = "发生错误，请联系管理员";
            }

            return msg;
        }
    }
}
