using DataFactory.Factory;
using DataFactory.KingBase;

using Model.tech.QL.DTO.SysUser;

using Tool;

namespace Service.PreAssembled
{
    public class SysUserService(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory) : ServiceBase(qlUowFactory)
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
                List<SysUserMenuDTO> list = GetList(query, (data) =>
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
                    loginUser.DeptName = usr.DeptOID?.ToString();
                    loginUser.DeptOID = usr.DeptOID?.ToString();
                    return loginUser;
                }
                return null;
            }
        }
    }
}
