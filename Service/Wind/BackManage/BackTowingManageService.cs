using DataFactory.Factory;
using DataFactory.KingBase.Towing;
using Model.Tech.Cloud.BackManage;
using Tool;

namespace Service.Wind.BackManage
{
    public class BackTowingManageService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingUowFactory;

        public BackTowingManageService(Towing_KingBase_UnitOfWorkFactory towingUowFactory)
        {
            _towingUowFactory = towingUowFactory;
        }

        /// <summary>
        /// 用户列表查询（分页+筛选）
        /// </summary>
        public (List<Manage_User> list, int totalCount, int pageIndex, string msg) ListQuery(CloudWindBackManageRequest request)
        {
            var list = new List<Manage_User>();
            string msg = "";
            int totalCount = 0;
            int pageIndex = request.PageIndex;

            try
            {
                using (var uow = _towingUowFactory.Create())
                {
                    var repo = uow.GetRepository<Manage_User>();

                    var predicate = PredicateBuilder.True<Manage_User>();
                    predicate = PredicateBuilder.And(predicate, a => !a.IsDelete);

                    if (!string.IsNullOrEmpty(request.RealName))
                    {
                        var realName = request.RealName;
                        predicate = PredicateBuilder.And(predicate, a => a.RealName != null && a.RealName.Contains(realName));
                    }

                    totalCount = repo.FindCount(predicate);

                    var (pageList, _) = repo.FindPage(
                        predicate,
                        a => a.CreateTime!,
                        pageIndex,
                        request.PageSize
                    );

                    list = pageList.OrderByDescending(a => a.CreateTime).ToList();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackTowingManageService));
                msg = "发生错误";
            }

            return (list, totalCount, pageIndex, msg);
        }

        /// <summary>
        /// 根据ID查询单条用户数据
        /// </summary>
        public (Manage_User? data, string msg) DataQuery(CloudWindBackManageRequest request)
        {
            Manage_User? data = null;
            string msg = "";

            try
            {
                using (var uow = _towingUowFactory.Create())
                {
                    var repo = uow.GetRepository<Manage_User>();
                    data = repo.FindFirst(a => !a.IsDelete && a.id == request.ID);

                    if (data == null)
                    {
                        msg = "数据错误";
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackTowingManageService));
                msg = "发生错误";
            }

            return (data, msg);
        }

        /// <summary>
        /// 新增或编辑用户
        /// </summary>
        public (bool success, string msg, string userCode) DataSave(CloudWindBackManageRequest request)
        {
            string msg = "";
            string userCode = "";

            try
            {
                using (var uow = _towingUowFactory.Create())
                {
                    var repo = uow.GetRepository<Manage_User>();

                    if (request.Type.Equals("add"))
                    {
                        if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.RealName))
                        {
                            return (false, "数据不完整", "");
                        }

                        var existUser = repo.FindFirst(a => !a.IsDelete && a.UserName == request.UserName);
                        if (existUser != null)
                        {
                            return (false, "已经存在此用户", "");
                        }

                        Manage_User user = new Manage_User();
                        user.UserName = request.UserName;
                        user.RealName = request.RealName;
                        user.Depart = request.Depart;
                        user.IsConfirm = request.IsConfirm;
                        user.Role = request.Role;
                        user.CreateTime = DateTime.Now;
                        user.IsDelete = false;

                        repo.Add(user);
                        uow.Save();

                        userCode = request.UserName;
                    }
                    else
                    {
                        if (request.ID == 0)
                        {
                            return (false, "发生错误", "");
                        }

                        var user = repo.FindFirst(a => !a.IsDelete && a.id == request.ID);
                        if (user == null)
                        {
                            return (false, "发生错误", "");
                        }

                        user.IsConfirm = request.IsConfirm;
                        user.Role = request.Role;
                        user.CreateTime = DateTime.Now;

                        userCode = user.UserName ?? "";
                        uow.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackTowingManageService));
                msg = "发生错误，请联系管理员";
                return (false, msg, "");
            }

            return (true, msg, userCode);
        }

        /// <summary>
        /// 软删除用户
        /// </summary>
        public (bool success, string msg) DataDelete(CloudWindBackManageRequest request)
        {
            string msg = "";

            try
            {
                using (var uow = _towingUowFactory.Create())
                {
                    var repo = uow.GetRepository<Manage_User>();
                    var data = repo.FindFirst(a => !a.IsDelete && a.id == request.ID);
                    if (data == null)
                    {
                        return (false, "数据错误");
                    }

                    data.IsDelete = true;
                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackTowingManageService));
                msg = "发生错误，请联系管理员";
                return (false, msg);
            }

            return (true, msg);
        }
    }
}
