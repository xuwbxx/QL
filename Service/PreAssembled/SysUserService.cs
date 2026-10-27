using DataFactory.Factory;
using DataFactory.KingBase;
using Microsoft.AspNetCore.Http;
using Model.tech.QL.DTO.SysUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tool;

namespace Service.PreAssembled
{
    public class SysUserService(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory, IHttpContextAccessor httpContextAccessor)
            : ServiceBase(qlUowFactory, httpContextAccessor)
    {
        public List<sys_userinfo> GetList()
        {
            using (var db = base.DbFactory.Create())
            {
                var query = from usr in db.Query<sys_userinfo>().Where(a => a.Status != -1)
                            select usr;
                return base.GetList(query);
            }
        }

        public List<SysUserItemDTO> GetUserList(SysUserQueryDTO req)
        {
            var query = Db.Query<sys_userinfo>().Where(a => a.Status != -1);

            if (!string.IsNullOrEmpty(req?.Name))
                query = query.Where(a => a.Name.Contains(req.Name));

            if (!string.IsNullOrEmpty(req?.Mobile))
                query = query.Where(a => a.Mobile.Contains(req.Mobile));

            var users = query.OrderByDescending(a => a.ID).ToList();

            var roles = Db.Query<sys_role>().Where(a => a.Status != -1).ToList();
            var userRoles = Db.Query<sys_user_role>().Where(a => a.Status != -1).ToList();

            var list = new List<SysUserItemDTO>();
            foreach (var user in users)
            {
                var item = new SysUserItemDTO();
                ObjectUtils.CopyObjectValue(user, item);

                var roleIds = userRoles.Where(ur => ur.UserID == user.ID).Select(ur => ur.RoleID).ToList();
                var roleNames = roles.Where(r => roleIds.Contains(r.ID)).Select(r => r.Name).ToList();
                item.RoleNames = string.Join(",", roleNames);

                list.Add(item);
            }
            return list;
        }

        public SysUserSaveDTO GetUserById(int id)
        {
            var user = Db.Query<sys_userinfo>().FirstOrDefault(a => a.ID == id && a.Status != -1);
            if (user == null) return null;

            var dto = new SysUserSaveDTO();
            dto.ID = user.ID;
            dto.EmpNo = user.EmpNo;
            dto.Account = user.Account;
            dto.Name = user.Name;
            dto.Mobile = user.Mobile;
            dto.Email = user.Email;
            dto.DeptOID = user.DeptOID;
            dto.DeptName = user.DeptName;

            var roleIds = Db.Query<sys_user_role>()
                .Where(a => a.Status != -1 && a.UserID == id)
                .Select(a => a.RoleID).ToList();
            dto.RoleIds = roleIds;

            return dto;
        }

        public int SaveUser(SysUserSaveDTO dto)
        {
            // 校验：登录账号不能重复
            if (!string.IsNullOrEmpty(dto.Account))
            {
                var accountExists = Db.Query<sys_userinfo>()
                    .Any(a => a.Status != -1 && a.Account == dto.Account && a.ID != dto.ID);
                if (accountExists)
                {
                    throw new Exception("登录账号已存在，请更换");
                }
            }

            var entityId = SaveInt<SysUserSaveDTO, sys_userinfo>(dto, (item) =>
            {
                var data = new sys_userinfo();
                ObjectUtils.CopyObjectValue(item, data);
                data.Status = 1;
                if (!string.IsNullOrEmpty(item.Password))
                {
                    data.PasswordHash = item.Password;
                }
                else if (item.ID != 0)
                {
                    var existing = Db.Query<sys_userinfo>().FirstOrDefault(a => a.ID == item.ID);
                    if (existing != null)
                    {
                        data.PasswordHash = existing.PasswordHash;
                    }
                }
                return data;
            });

            if (entityId == 0)
            {
                throw new Exception("保存用户失败");
            }

            // 保存角色关联 - 物理删除所有旧关联（含软删除记录），避免唯一约束冲突
            var roleRep = Db.GetRepository<sys_user_role>();
            var oldRoles = Db.Query<sys_user_role>()
                .Where(a => a.UserID == entityId).ToList();
            foreach (var old in oldRoles)
            {
                roleRep.Delete(old);
            }
            try
            {
                Db.Save();
            }
            catch (Exception ex)
            {
                var fullMsg = ex.Message;
                var inner = ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                    fullMsg += " | " + inner.Message;
                }
                LoggerUtils.Error($"SaveUser-删除旧角色关联失败: {fullMsg}", typeof(SysUserService));
                throw new Exception(fullMsg);
            }

            if (dto.RoleIds != null && dto.RoleIds.Count > 0)
            {
                var account = CurrUser?.Account ?? "system";
                var now = DateTime.UtcNow;
                foreach (var roleId in dto.RoleIds)
                {
                    roleRep.Add(new sys_user_role
                    {
                        UserID = entityId,
                        RoleID = roleId,
                        Status = 1,
                        CreatedBy = account,
                        CreatedTime = now
                    });
                }
                Db.Save();
            }

            return entityId;
        }

        public int DeleteUser(int userId)
        {
            var user = Db.Query<sys_userinfo>().FirstOrDefault(a => a.ID == userId && a.Status != -1);
            if (user != null)
            {
                // 检查是否被项目管理引用为负责人
                var projectCount = Db.Query<biz_project>()
                    .Count(a => a.ManagerId == userId && a.Status != -1);
                if (projectCount > 0)
                {
                    throw new Exception($"该用户已被{projectCount}个项目引用为负责人，无法删除");
                }

                user.Status = -1;
                user.UpdatedBy = CurrUser?.Account ?? "system";
                user.UpdatedTime = DateTime.UtcNow;
                Db.Save();
                return userId;
            }
            return 0;
        }

        public bool ResetPassword(int userId, string defaultPassword)
        {
            var user = Db.Query<sys_userinfo>().FirstOrDefault(a => a.ID == userId && a.Status != -1);
            if (user != null)
            {
                user.PasswordHash = defaultPassword;
                user.UpdatedBy = CurrUser?.Account ?? "system";
                user.UpdatedTime = DateTime.UtcNow;
                Db.Save();
                return true;
            }
            return false;
        }

        public bool ChangePassword(string account, string oldPasswordHash, string newPasswordHash)
        {
            var user = Db.Query<sys_userinfo>().FirstOrDefault(a => a.Account == account && a.Status != -1);
            if (user == null) throw new Exception("用户不存在");
            if (user.PasswordHash != oldPasswordHash) throw new Exception("旧密码错误");
            user.PasswordHash = newPasswordHash;
            user.UpdatedBy = CurrUser?.Account ?? "system";
            user.UpdatedTime = DateTime.UtcNow;
            Db.Save();
            return true;
        }

        private static string ComputeMD5Hash(string input)
        {
            using var md5 = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = md5.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (var b in hash)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public List<object> GetRoleList()
        {
            var roles = Db.Query<sys_role>()
                .Where(a => a.Status != -1)
                .OrderBy(a => a.Name)
                .ToList();

            var list = new List<object>();
            foreach (var role in roles)
            {
                list.Add(new { id = role.ID, name = role.Name });
            }
            return list;
        }

        /// <summary>
        /// 调用外部接口查询交建通用户
        /// </summary>
        public string QueryMDUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return JsonSerializer.Serialize(new { code = 400, msg = "查询用户名称不可为空" });
            }

            var reqObj = new
            {
                SysType = "ShjWebsitePlatform",
                VerifyKey = "f380f33d66dca14b9c383261949b9e79",
                UserName = userName
            };
            var responseJson = Tool.HttpUtils.PostSync("http://contractsys.ccshj.cn:9098/In_Emp/QueryEmpByName", reqObj);

            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("StatusCode", out var statusEl) && statusEl.GetInt32() != 200)
            {
                var info = root.TryGetProperty("Info", out var infoEl) ? infoEl.GetString() : "查询失败";
                return JsonSerializer.Serialize(new { code = 500, msg = info, data = new List<object>() });
            }

            var dataList = new List<object>();
            if (root.TryGetProperty("Data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataEl.EnumerateArray())
                {
                    var deptPath = item.TryGetProperty("wholeDeptPath", out var dp) ? dp.GetString() : "";
                    if (deptPath != null)
                    {
                        dataList.Add(new
                        {
                            dept = deptPath,
                            name = item.TryGetProperty("name", out var n) ? n.GetString() : "",
                            userLogin = item.TryGetProperty("userLogin", out var ul) ? ul.GetString() : "",
                            empcode = item.TryGetProperty("empcode", out var ec) ? ec.GetString() : "",
                            roid = item.TryGetProperty("roid", out var ro) ? ro.GetString() : ""
                        });
                    }
                }
            }

            var result = new { code = 200, msg = "ok", data = dataList };
            return JsonSerializer.Serialize(result);
        }

        public List<SysUserMenuDTO> GetMenu(string usrAccount)
        {
            using (var db = this.DbFactory.Create())
            {
                var query = from menu in db.Query<sys_menu>().Where(a => a.Status != -1)
                            from rm in db.Query<sys_role_menu>().Where(a => a.Status != -1 && a.MenuID == menu.ID)
                            from um in db.Query<sys_user_role>().Where(a => a.Status != -1 && a.RoleID == rm.RoleID)
                            from usr in db.Query<sys_userinfo>().Where(a => a.Status != -1 && a.ID == um.UserID)
                            where usr.Account == usrAccount
                            select menu;
                List<SysUserMenuDTO> list = GetList(query.Distinct(), (data) =>
                {
                    var item = new SysUserMenuDTO();
                    ObjectUtils.CopyObjectValue(data, item);
                    item.ParentID = data.ParentID;
                    return item;
                });
                return list;
            }
        }

        public async Task<LoginUserInfoDTO> ValidUser(string account, string pwdhashed)
        {
            using (var db = DbFactory.Create())
            {
                var usr = db.Query<sys_userinfo>().FirstOrDefault(a => a.Account == account && a.PasswordHash == pwdhashed);
                if (usr != null)
                {
                    var loginUser = new LoginUserInfoDTO();
                    loginUser.Account = usr.Account;
                    loginUser.Name = usr.Name;
                    loginUser.DeptName = usr.DeptName;
                    loginUser.DeptOID = usr.DeptOID;
                    return loginUser;
                }
                return null;
            }
        }
    }
}
