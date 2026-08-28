using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Model.Tech.Cloud;
using Model.Tech.Cloud.BackManage;
using System;
using System.Collections.Generic;
using System.Linq;
using Tool;

namespace Service.Wind.BackManage
{
    public class BackProjectRoleService
    {
        private readonly CloudWind_KingBase_UnitOfWorkFactory _cloudWindUowFactory;

        public BackProjectRoleService(CloudWind_KingBase_UnitOfWorkFactory cloudWindUowFactory)
        {
            _cloudWindUowFactory = cloudWindUowFactory;
        }

        public (List<View_Wind_ProjectContacter> list, int totalCount, int pageIndex, string msg) ListQuery(CloudWindBackManageRequest request, List<string> admins, string userCode)
        {
            var list = new List<View_Wind_ProjectContacter>();
            string msg = "";
            int totalCount = 0;
            int pageIndex = request.PageIndex;

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var repo = uow.GetRepository<View_Wind_ProjectContacter>();

                    var predicate = PredicateBuilder.True<View_Wind_ProjectContacter>();
                    predicate = PredicateBuilder.And(predicate, a => a.FlowStatus == (int)CloudWindProjectFlowStatus.激活);
                    predicate = PredicateBuilder.And(predicate, a => admins.Contains(a.DirectorCode) || a.DirectorCode == userCode);

                    if (!string.IsNullOrEmpty(request.ProjectName))
                    {
                        var projectName = request.ProjectName;
                        predicate = PredicateBuilder.And(predicate, a => a.ProjectName != null && a.ProjectName.Contains(projectName));
                    }

                    var (pageList, count) = repo.FindPage(predicate, a => a.id, request.PageIndex, request.PageSize);

                    totalCount = count;

                    if (request.PageIndex != 1 && pageList.Count() == 0)
                    {
                        pageIndex = 1;
                        (pageList, totalCount) = repo.FindPage(predicate, a => a.id, 1, request.PageSize);
                    }

                    list = pageList.ToList();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackProjectRoleService));
                msg = "发生错误";
            }

            return (list, totalCount, pageIndex, msg);
        }

        public (CloudWindManageProjectRole data, string msg) DataQuery(CloudWindBackManageRequest request)
        {
            var data = new CloudWindManageProjectRole();
            string msg = "";

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    // 查找项目
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var project = projectRepo.FindFirst(a => !a.IsDelete && a.ID == request.ProjectID);
                    if (project == null)
                    {
                        return (null, "项目数据错误");
                    }

                    data.ProjectID = project.ID;
                    data.ProjectName = project.ProjectName;

                    // 角色列表
                    var roleRepo = uow.GetRepository<Manage_Role>();
                    var roles = roleRepo.Find(a => !(a.IsDelete == true)).ToList();
                    foreach (var a in roles)
                    {
                        data.Roles.Add(new CloudWindManageRole()
                        {
                            ID = a.id,
                            RoleName = a.RoleName
                        });
                    }

                    // 项目角色人员
                    var projectRoleRepo = uow.GetRepository<View_Wind_ProjectRole>();
                    var projectRoles = projectRoleRepo.Find(a => a.ProjectID == request.ProjectID).ToList();
                    foreach (var a in projectRoles)
                    {
                        if (!string.IsNullOrEmpty(a.UserName) && !string.IsNullOrEmpty(a.UserCode))
                        {
                            data.List.Add(new CloudWindManageProjectRoleData()
                            {
                                ID = a.id,
                                RoleID = a.RoleID ?? 0,
                                UserName = a.UserName,
                                UserCode = a.UserCode,
                                UserDepartName = a.UserDepartName ?? "",
                                UserPhone = a.UserPhone ?? "",
                                UserJobName = a.UserJobName ?? ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackProjectRoleService));
                msg = "发生错误，请联系管理员";
            }

            return (data, msg);
        }

        public string DataSave(CloudWindBackManageRequest request)
        {
            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    // 查找项目
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var project = projectRepo.FindFirst(a => !a.IsDelete && a.ID == request.ProjectID);
                    if (project == null)
                    {
                        return "项目数据错误";
                    }

                    // 软删除旧的项目角色
                    var projectRoleRepo = uow.GetRepository<Wind_ProjectRole>();
                    var oldRoles = projectRoleRepo.Find(a => !a.IsDelete && a.ProjectID == project.ID).ToList();
                    foreach (var a in oldRoles)
                    {
                        a.IsDelete = true;
                    }

                    // 插入新的项目角色
                    foreach (var a in request.ProjectRoles)
                    {
                        if (!string.IsNullOrEmpty(a.UserName) && !string.IsNullOrEmpty(a.UserCode))
                        {
                            var newRole = new Wind_ProjectRole()
                            {
                                ProjectID = project.ID,
                                RoleID = a.RoleID,
                                UserName = a.UserName,
                                UserCode = a.UserCode,
                                UserDepartName = a.UserDepartName,
                                UserPhone = a.UserPhone,
                                UserJobName = a.UserJobName,
                                IsDelete = false,
                                CreateTime = DateTime.UtcNow
                            };
                            projectRoleRepo.Add(newRole);
                        }
                    }

                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackProjectRoleService));
                return "发生错误，请联系管理员";
            }

            return "";
        }
    }
}
