using DataFactory.Factory;
using DataFactory.KingBase;
using Model.tech.QL.DTO.BizProject;
using Tool;

namespace Service.PreAssembled
{
    public class BizProjectBridgeService(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory) : ServiceBase<biz_project_bridge>(qlUowFactory)
    {
        public async Task<List<BizProjectBridgeItemDTO>> BridgeList(BizProjectBridgeQueryDTO req)
        {
            var query = Db.Query<biz_project_bridge>().Where(a => a.Status != -1);

            if (req?.ProjID != null && req.ProjID > 0)
                query = query.Where(a => a.ProjID == req.ProjID);

            var bridges = query.OrderByDescending(a => a.ID).ToList();
            var list = new List<BizProjectBridgeItemDTO>();
            foreach (var data in bridges)
            {
                var item = new BizProjectBridgeItemDTO();
                ObjectUtils.CopyObjectValue(data, item);
                list.Add(item);
            }
            return list;
        }

        public async Task<BizProjectBridgeItemDTO> BridgeSave(BizProjectBridgeItemDTO item)
        {
            item.ID = base.SaveInt(item, (entity) =>
            {
                var data = new biz_project_bridge();
                ObjectUtils.CopyObjectValue(entity, data);
                return data;
            });
            return item;
        }

        public async Task<BizProjectBridgeItemDTO> BridgeDelete(BizProjectBridgeItemDTO item)
        {
            // 检查是否存在浇筑分组子项
            var hasChildren = Db.Query<biz_project_bridge_castingGroup>()
                .Any(a => a.BridgeID == item.ID && a.Status != -1);
            if (hasChildren)
            {
                throw new Exception("该桥梁下存在浇筑分组子项，无法删除");
            }

            item.ID = base.Delete(item.ID);
            return item;
        }
    }
}