using DataFactory.Factory;
using DataFactory.KingBase;
using Microsoft.AspNetCore.Http;
using Model.tech.QL;
using Model.tech.QL.DTO.SysMenu;
using Tool;

namespace Service.PreAssembled
{
    public class SysMenuService(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory, IHttpContextAccessor httpContextAccessor)
            : ServiceBase<sys_menu>(qlUowFactory, httpContextAccessor)
    {
        public async Task<List<SysMenuItemDTO>> List(SysMenuQueryDTO req)
        {
            var menu = Db.Query<sys_menu>().Where(a => a.Status != -1).DefaultIfEmpty().ToList();
            string userAccount = CurrUser.Account;
            string userName = CurrUser.Name;
            string deptName = CurrUser.DeptName;
            string deptOID = CurrUser.DeptOID;
            var list = new List<SysMenuItemDTO>();
            if (menu == null) return null;
            foreach (var data in menu)
            {
                var item = new SysMenuItemDTO();
                ObjectUtils.CopyObjectValue(data, item);
                list.Add(item);
            }
            return list;
        }

        public async Task<SysMenuItemDTO> Save(SysMenuItemDTO item)
        {
            try
            {
                item.ID = base.Save(item, (entity) =>
                {
                    if (entity.ParentID != null)
                    {
                        var parent = dbSet.FindByID(entity.ParentID ?? 0);
                        if (parent != null)
                        {
                            entity.FullName = parent.FullName + " / " + entity.Name;
                            entity.EnFullName = (parent.EnFullName ?? "-") + " / " + (entity.EnName ?? "-");
                        }
                    }
                    var data = new sys_menu();
                    ObjectUtils.CopyObjectValue(entity, data);
                    return data;
                });
            }
            catch (Exception ex)
            {
                // "uidx_sys_menu_permissionflag";
                if (ex.InnerException != null && ex.InnerException.Message != null 
                    && ex.InnerException.Message.Contains("uidx_sys_menu_permissionflag"))
                {
                    throw new BizException("权限标识不可重复。");
                }
            }
            return item;
        }

        public async Task<SysMenuItemDTO> Delete(SysMenuItemDTO item)
        {
            item.ID = base.Delete(item.ID);
            return item;
        }
    }
}