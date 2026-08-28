using DataFactory.Factory;
using DataFactory.KingBase;
using Model.tech.QL.DTO.BizProject;
using Tool;

namespace Service.PreAssembled
{
    public class BizProjectService(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory) : ServiceBase<biz_project>(qlUowFactory)
    {
        public async Task<List<BizProjectItemDTO>> List(BizProjectQueryDTO req)
        {
            var query = Db.Query<biz_project>().Where(a => a.Status != -1);

            if (!string.IsNullOrEmpty(req?.Name))
                query = query.Where(a => a.Name.Contains(req.Name));

            if (req?.ProgressStatus != null)
                query = query.Where(a => a.ProgressStatus == req.ProgressStatus);

            var menu = query.OrderByDescending(a => a.ID).ToList();
            var list = new List<BizProjectItemDTO>();
            foreach (var data in menu)
            {
                var item = new BizProjectItemDTO();
                ObjectUtils.CopyObjectValue(data, item);
                list.Add(item);
            }
            return list;

        }

        /// <summary>
        /// 获取启用的用户列表（用于项目负责人下拉选择）
        /// </summary>
        public async Task<List<object>> GetUserList()
        {
            var users = Db.Query<sys_userinfo>()
                .Where(a => a.Status == 1)
                .OrderBy(a => a.Name)
                .ToList();

            var list = new List<object>();
            foreach (var user in users)
            {
                list.Add(new { id = user.ID, name = user.Name });
            }
            return list;
        }

        public async Task<BizProjectItemDTO> Save(BizProjectItemDTO item)
        {

            item.ID = base.SaveInt(item, (entity) =>
            {

                var data = new biz_project();
                ObjectUtils.CopyObjectValue(entity, data);
                return data;
            });
            return item;
        }

        public async Task<BizProjectItemDTO> Delete(BizProjectItemDTO item)
        {
            // 检查是否存在桥梁子项
            var hasChildren = Db.Query<biz_project_bridge>()
                .Any(a => a.ProjID == item.ID && a.Status != -1);
            if (hasChildren)
            {
                throw new Exception("该项目下存在桥梁子项，无法删除");
            }

            item.ID = base.Delete(item.ID);
            return item;
        }
    }
}
