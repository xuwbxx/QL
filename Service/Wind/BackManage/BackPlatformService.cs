using DataFactory.Factory;
using DataFactory.KingBase;
using DataFactory.KingBase.CloudWind;
using Model.Tech.Cloud;
using Model.Tech.Cloud.BackManage;
using Tool;

namespace Service.Wind.BackManage
{
    public class BackPlatformService
    {
        private readonly TechCenter_KingBase_UnitOfWorkFactory _techCenterUowFactory;
        private readonly CloudWind_KingBase_UnitOfWorkFactory _cloudWindUowFactory;

        public BackPlatformService(
            TechCenter_KingBase_UnitOfWorkFactory techCenterUowFactory,
            CloudWind_KingBase_UnitOfWorkFactory cloudWindUowFactory)
        {
            _techCenterUowFactory = techCenterUowFactory;
            _cloudWindUowFactory = cloudWindUowFactory;
        }

        public (List<CloudWindManageSoftware> list, int totalCount, int pageIndex, string msg) SoftwareListQuery(CloudWindBackManageRequest request)
        {
            List<CloudWindManageSoftware> list = new List<CloudWindManageSoftware>();
            string msg = "";
            int totalCount = 0;
            int pageIndex = request.PageIndex;

            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var repo = uow.GetRepository<TechCenter_Manage_Software>();

                    var predicate = PredicateBuilder.True<TechCenter_Manage_Software>();
                    predicate = PredicateBuilder.And(predicate, a => !a.IsDelete);

                    if (!string.IsNullOrEmpty(request.SoftwareName))
                    {
                        var softwareName = request.SoftwareName;
                        predicate = PredicateBuilder.And(predicate, a => a.Name != null && a.Name.Contains(softwareName));
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
                        list.Add(new CloudWindManageSoftware()
                        {
                            ID = a.ID,
                            SoftwareName = a.Name ?? "",
                            SoftwareUrl = a.Url ?? "",
                            FlowType = a.Type,
                            FlowTypeName = Enum.GetName(typeof(CloudWindFlowType), a.Type) ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackPlatformService));
                msg = "发生错误";
            }

            return (list, totalCount, pageIndex, msg);
        }

        public (CloudWindManageSoftware? model, string msg) SoftwareDataQuery(CloudWindBackManageRequest request)
        {
            CloudWindManageSoftware? model = null;
            string msg = "";

            try
            {
                using (var uow = _techCenterUowFactory.Create())
                {
                    var repo = uow.GetRepository<TechCenter_Manage_Software>();
                    var software = repo.FindFirst(a => a.ID == request.ID);

                    if (software == null)
                    {
                        msg = "数据不存在";
                        return (model, msg);
                    }

                    model = new CloudWindManageSoftware()
                    {
                        ID = software.ID,
                        DepartID = software.DepartID,
                        SoftwareName = software.Name ?? "",
                        FlowType = software.Type,
                        SoftwareUrl = software.Url ?? "",
                        SoftwareComment = software.Info ?? ""
                    };
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackPlatformService));
                msg = "发生错误";
            }

            return (model, msg);
        }

        public (List<CloudWindManageCompany> list, int totalCount, int pageIndex, string msg) CompanyListQuery(CloudWindBackManageRequest request)
        {
            List<CloudWindManageCompany> list = new List<CloudWindManageCompany>();
            string msg = "";
            int totalCount = 0;
            int pageIndex = request.PageIndex;

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var companyRepo = uow.GetRepository<Manage_Company>();
                    var roleRepo = uow.GetRepository<Manage_CompanyRole>();

                    var predicate = PredicateBuilder.True<Manage_Company>();
                    predicate = PredicateBuilder.And(predicate, a => !a.IsDelete);

                    if (!string.IsNullOrEmpty(request.CompanyName))
                    {
                        var companyName = request.CompanyName;
                        predicate = PredicateBuilder.And(predicate, a => a.Company != null && a.Company.Contains(companyName));
                    }

                    var (pageList, count) = companyRepo.FindPage(predicate, a => a.ID, request.PageIndex, request.PageSize);

                    totalCount = count;

                    if (request.PageIndex != 1 && pageList.Count() == 0)
                    {
                        pageIndex = 1;
                        (pageList, totalCount) = companyRepo.FindPage(predicate, a => a.ID, 1, request.PageSize);
                    }

                    var companyRoles = roleRepo.Find(a => !a.IsDelete).ToList();

                    foreach (var a in pageList)
                    {
                        var se = new CloudWindManageCompany()
                        {
                            ID = a.ID,
                            Company = a.Company ?? ""
                        };

                        var majorRole = companyRoles.FirstOrDefault(b => b.CompanyID == a.ID);
                        if (majorRole != null)
                        {
                            se.MajorEng = new CloudWindBackManageUserInfo()
                            {
                                UserName = majorRole.UserName,
                                UserCode = majorRole.UserCode,
                                UserDepartName = majorRole.UserDepartName,
                                UserPhone = majorRole.UserPhone,
                                UserJobName = majorRole.UserJobName
                            };
                        }

                        list.Add(se);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackPlatformService));
                msg = "发生错误";
            }

            return (list, totalCount, pageIndex, msg);
        }

        public (CloudWindManageCompany? data, string msg) CompanyDataQuery(CloudWindBackManageRequest request)
        {
            CloudWindManageCompany? data = null;
            string msg = "";

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var companyRepo = uow.GetRepository<Manage_Company>();
                    var roleRepo = uow.GetRepository<Manage_CompanyRole>();

                    var company = companyRepo.FindFirst(a => a.ID == request.ID);
                    if (company == null)
                    {
                        msg = "数据不存在";
                        return (data, msg);
                    }

                    data = new CloudWindManageCompany()
                    {
                        ID = company.ID,
                        Company = company.Company ?? ""
                    };

                    var companyRole = roleRepo.FindFirst(a => !a.IsDelete && a.CompanyID == company.ID);
                    if (companyRole != null)
                    {
                        data.MajorEng = new CloudWindBackManageUserInfo()
                        {
                            UserName = companyRole.UserName ?? "",
                            UserCode = companyRole.UserCode ?? "",
                            UserDepartName = companyRole.UserDepartName ?? "",
                            UserPhone = companyRole.UserPhone ?? "",
                            UserJobName = companyRole.UserJobName ?? ""
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackPlatformService));
                msg = "发生错误";
            }

            return (data, msg);
        }

        public string CompanyDataSave(CloudWindBackManageRequest request)
        {
            string msg = "";

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var companyRepo = uow.GetRepository<Manage_Company>();
                    var roleRepo = uow.GetRepository<Manage_CompanyRole>();

                    int companyID = 0;

                    if (request.ID == 0)
                    {
                        // 新增
                        var newCompany = new Manage_Company()
                        {
                            Company = request.CompanyName,
                            IsDelete = false,
                            CreateTime = DateTime.UtcNow
                        };
                        companyRepo.Add(newCompany);
                        uow.Save();

                        companyID = companyRepo.Find(a => true).OrderByDescending(a => a.ID).First().ID;
                    }
                    else
                    {
                        // 修改
                        var company = companyRepo.FindFirst(a => !a.IsDelete && a.ID == request.ID);
                        if (company == null)
                        {
                            return "账号出错";
                        }
                        company.Company = request.CompanyName;
                        company.CreateTime = DateTime.UtcNow;
                        companyID = company.ID;
                    }

                    // 总工
                    var majorUser = request.NodeManagers[0];
                    var roleUser = roleRepo.FindFirst(a => !a.IsDelete && a.CompanyID == companyID);

                    if (string.IsNullOrEmpty(majorUser.UserCode))
                    {
                        if (roleUser != null)
                        {
                            roleUser.IsDelete = true;
                        }
                    }
                    else
                    {
                        if (roleUser != null)
                        {
                            roleUser.CompanyID = companyID;
                            roleUser.RoleID = (int)CloudWindManageCompanyRoleEnum.项目总工;
                            roleUser.UserName = majorUser.UserName;
                            roleUser.UserCode = majorUser.UserCode;
                            roleUser.UserDepartName = majorUser.UserDepartName;
                            roleUser.UserPhone = majorUser.UserPhone;
                            roleUser.UserJobName = majorUser.UserJobName;
                            roleUser.CreateTime = DateTime.UtcNow;
                        }
                        else
                        {
                            var newRole = new Manage_CompanyRole()
                            {
                                CompanyID = companyID,
                                RoleID = (int)CloudWindManageCompanyRoleEnum.项目总工,
                                UserName = majorUser.UserName,
                                UserCode = majorUser.UserCode,
                                UserDepartName = majorUser.UserDepartName,
                                UserPhone = majorUser.UserPhone,
                                UserJobName = majorUser.UserJobName,
                                CreateTime = DateTime.UtcNow,
                                IsDelete = false
                            };
                            roleRepo.Add(newRole);
                        }
                    }

                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackPlatformService));
                msg = "发生错误，请联系管理员";
            }

            return msg;
        }
    }
}
