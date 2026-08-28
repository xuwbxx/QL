using DataFactory.Factory;
using DataFactory.KingBase;
using Model.tech.QL.DTO.SysMenu;
using Tool;

namespace Service.PreAssembled
{
    public class SysMenuService(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory) : ServiceBase<sys_menu>(qlUowFactory)
    {
        public async Task<List<SysMenuItemDTO>> List(SysMenuQueryDTO req)
        {
            var menu = Db.Query<sys_menu>().Where(a => a.Status != -1).DefaultIfEmpty().ToList();
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

            item.ID = base.SaveInt(item, (entity) =>
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
            return item;
        }

        public async Task<SysMenuItemDTO> Delete(SysMenuItemDTO item)
        {
            item.ID = base.Delete(item.ID);
            return item;
        }
    }
}