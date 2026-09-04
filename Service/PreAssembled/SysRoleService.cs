using DataFactory.Factory;
using DataFactory.KingBase;
using Microsoft.AspNetCore.Http;
using Model.tech.QL.DTO.SysRole;

namespace Service.PreAssembled;

public class SysRoleService(QlPreAssembled_KingBase_UnitOfWorkFactory factory, IHttpContextAccessor httpContextAccessor)
            : ServiceBase(factory, httpContextAccessor)
{
    private const string AdminCode = "role_admin";

    public Task<bool> CanManage(string account)
    {
        account = Normalize(account);
        if (account.Length == 0) return Task.FromResult(false);
        var roles = from user in Db.Query<sys_userinfo>().Where(x => x.Status == 1 && x.Account.ToLower() == account)
                    join ur in Db.Query<sys_user_role>().Where(x => x.Status == 1) on user.ID equals ur.UserID
                    join role in Db.Query<sys_role>().Where(x => !x.IsDeleted && x.Status == 1) on ur.RoleID equals role.ID
                    select role;
        if (roles.Any(x => x.Code != null && x.Code.ToLower() == AdminCode)) return Task.FromResult(true);
        var allowed = from role in roles
                      join rm in Db.Query<sys_role_menu>().Where(x => x.Status == 1) on role.ID equals rm.RoleID
                      join menu in Db.Query<sys_menu>().Where(x => x.Status == 1) on rm.MenuID equals menu.ID
                      where menu.Action == "/Sys/Role/Index" || menu.Action == "/Sys/Role" || menu.Name == "角色管理"
                      select menu.ID;
        return Task.FromResult(allowed.Any());
    }

    public Task<SysRolePagedResultDTO> List(SysRoleQueryDTO req)
    {
        var page = Math.Max(1, req.PageIndex); var size = Math.Clamp(req.PageSize, 1, 100);
        var query = Db.Query<sys_role>().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(req.Name)) { var v = req.Name.Trim().ToLower(); query = query.Where(x => x.Name.ToLower().Contains(v)); }
        if (!string.IsNullOrWhiteSpace(req.Code)) { var v = req.Code.Trim().ToLower(); query = query.Where(x => x.Code != null && x.Code.ToLower().Contains(v)); }
        if (req.Status.HasValue) query = query.Where(x => x.Status == req.Status.Value);
        var total = query.Count();
        var rows = query.OrderBy(x => x.Sort).ThenBy(x => x.ID).Skip((page - 1) * size).Take(size).ToList();
        var ids = rows.Select(x => x.ID).ToList();
        var counts = Db.Query<sys_user_role>().Where(x => x.Status == 1 && ids.Contains(x.RoleID)).GroupBy(x => x.RoleID)
            .Select(x => new { ID = x.Key, Count = x.Count() }).ToDictionary(x => x.ID, x => x.Count);
        return Task.FromResult(new SysRolePagedResultDTO { PageIndex = page, PageSize = size, Total = total,
            Items = rows.Select(x => new SysRoleItemDTO { ID = x.ID, Name = x.Name, Code = x.Code ?? "", RoleType = x.RoleType, Sort = x.Sort,
                Remark = x.Remark, Status = x.Status, UserCount = counts.GetValueOrDefault(x.ID), CreatedTime = x.CreatedTime }).ToList() });
    }

    public async Task<SysRoleItemDTO> Save(SysRoleItemDTO item, string actor)
    {
        var name = item.Name?.Trim() ?? ""; var code = Normalize(item.Code);
        if (name.Length == 0 || code.Length == 0) throw new InvalidOperationException("角色名称和角色标识不能为空");
        if (Db.Query<sys_role>().Any(x => !x.IsDeleted && x.ID != item.ID && x.Name.ToLower() == name.ToLower())) throw new InvalidOperationException("角色名称已存在");
        if (Db.Query<sys_role>().Any(x => !x.IsDeleted && x.ID != item.ID && x.Code != null && x.Code.ToLower() == code)) throw new InvalidOperationException("角色标识已存在");
        var rep = Db.GetRepository<sys_role>(); sys_role role;
        if (item.ID == 0)
        {
            if (code == AdminCode) throw new InvalidOperationException("role_admin 为系统保留标识");
            role = new sys_role { Name = name, Code = code, RoleType = "custom", Sort = item.Sort, Remark = item.Remark?.Trim(), Status = item.Status == 0 ? 0 : 1,
                IsDeleted = false, CreatedBy = actor, CreatedTime = DateTime.Now }; rep.Add(role);
        }
        else
        {
            role = RequireRole(item.ID);
            if (!string.Equals(role.Code, code, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("角色标识创建后不可修改");
            role.Name = name; role.Sort = item.Sort; role.Remark = item.Remark?.Trim(); role.UpdatedBy = actor; role.UpdatedTime = DateTime.Now;
        }
        await Db.SaveAsync(); item.ID = role.ID; item.Code = role.Code ?? ""; item.RoleType = role.RoleType; item.Status = role.Status; return item;
    }

    public async Task SetStatus(SysRoleStatusDTO item, string actor)
    {
        if (item.Status is not (0 or 1)) throw new InvalidOperationException("无效的角色状态");
        var role = RequireMutable(item.ID); role.Status = item.Status; role.UpdatedBy = actor; role.UpdatedTime = DateTime.Now; await Db.SaveAsync();
    }

    public async Task<int> Delete(SysRoleIdDTO item, string actor)
    {
        var role = RequireMutable(item.ID); var affected = Db.Query<sys_user_role>().Count(x => x.RoleID == role.ID && x.Status == 1);
        role.IsDeleted = true; role.UpdatedBy = actor; role.UpdatedTime = DateTime.Now; await Db.SaveAsync(); return affected;
    }

    public Task<List<SysRoleMenuItemDTO>> Menus(int roleId)
    {
        RequireRole(roleId); var selected = Db.Query<sys_role_menu>().Where(x => x.RoleID == roleId && x.Status == 1).Select(x => x.MenuID).ToHashSet();
        return Task.FromResult(Db.Query<sys_menu>().Where(x => x.Status == 1).OrderBy(x => x.Sort).ThenBy(x => x.ID)
            .Select(x => new SysRoleMenuItemDTO { ID = x.ID, ParentID = x.ParentID, Name = x.Name, MenuType = x.MenuType, Sort = x.Sort, Selected = selected.Contains(x.ID) }).ToList());
    }

    public async Task SaveMenus(SysRoleMenuDTO item, string actor)
    {
        RequireRole(item.ID); var valid = Db.Query<sys_menu>().Where(x => x.Status == 1 && item.MenuIDs.Contains(x.ID)).Select(x => x.ID).ToHashSet();
        var parents = Db.Query<sys_menu>().Where(x => x.Status == 1).Select(x => new { x.ID, x.ParentID }).ToDictionary(x => x.ID, x => x.ParentID);
        foreach (var id in valid.ToList()) { var parent = parents.GetValueOrDefault(id); while (parent.HasValue) { valid.Add(parent.Value); parent = parents.GetValueOrDefault(parent.Value); } }
        var rep = Db.GetRepository<sys_role_menu>(); var rows = rep.Find(x => x.RoleID == item.ID).ToList();
        foreach (var row in rows) row.Status = valid.Contains(row.MenuID) ? 1 : -1;
        foreach (var id in valid.Except(rows.Select(x => x.MenuID))) rep.Add(new sys_role_menu { RoleID = item.ID, MenuID = id, Status = 1, CreatedBy = actor, CreatedTime = DateTime.Now });
        await Db.SaveAsync();
    }

    public Task<List<SysRoleUserItemDTO>> Users(int roleId)
    {
        RequireRole(roleId); var selected = Db.Query<sys_user_role>().Where(x => x.RoleID == roleId && x.Status == 1).Select(x => x.UserID).ToHashSet();
        return Task.FromResult(Db.Query<sys_userinfo>().Where(x => x.Status == 1 || selected.Contains(x.ID)).OrderBy(x => x.Name)
            .Select(x => new SysRoleUserItemDTO { ID = x.ID, Name = x.Name, Account = x.Account, EmpNo = x.EmpNo, Status = x.Status, Selected = selected.Contains(x.ID) }).ToList());
    }

    public async Task SaveUsers(SysRoleUserDTO item, string actor)
    {
        RequireRole(item.ID); var rep = Db.GetRepository<sys_user_role>(); var rows = rep.Find(x => x.RoleID == item.ID).ToList();
        var current = rows.Where(x => x.Status == 1).Select(x => x.UserID).ToHashSet();
        var desired = Db.Query<sys_userinfo>().Where(x => x.Status == 1 && item.UserIDs.Contains(x.ID)).Select(x => x.ID).ToHashSet();
        desired.UnionWith(current.Where(item.UserIDs.Contains));
        foreach (var row in rows) row.Status = desired.Contains(row.UserID) ? 1 : -1;
        foreach (var id in desired.Except(rows.Select(x => x.UserID))) rep.Add(new sys_user_role { RoleID = item.ID, UserID = id, Status = 1, CreatedBy = actor, CreatedTime = DateTime.Now });
        await Db.SaveAsync();
    }

    private sys_role RequireRole(int id) => Db.GetRepository<sys_role>().FindFirst(x => x.ID == id && !x.IsDeleted) ?? throw new InvalidOperationException("角色不存在");
    private sys_role RequireMutable(int id) { var role = RequireRole(id); if (string.Equals(role.Code, AdminCode, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("内置超级管理员不可执行此操作"); return role; }
    private static string Normalize(string? value) => value?.Trim().ToLowerInvariant() ?? "";
}
