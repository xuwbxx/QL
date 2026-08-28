using DataFactory.Factory;
using DataFactory.KingBase;
using Model.tech.QL.DTO.BizProject;
using Tool;

namespace Service.PreAssembled
{
    public class BizProjectBridgeCastingGroupService(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory) : ServiceBase<biz_project_bridge_castingGroup>(qlUowFactory)
    {
        public async Task<List<BizProjectBridgeCastingGroupItemDTO>> CastingGroupList(BizProjectBridgeCastingGroupQueryDTO req)
        {
            var query = Db.Query<biz_project_bridge_castingGroup>().Where(a => a.Status != -1);

            if (req?.BridgeID != null && req.BridgeID > 0)
                query = query.Where(a => a.BridgeID == req.BridgeID);

            var groups = query.OrderByDescending(a => a.ID).ToList();
            var list = new List<BizProjectBridgeCastingGroupItemDTO>();
            foreach (var data in groups)
            {
                var item = new BizProjectBridgeCastingGroupItemDTO();
                ObjectUtils.CopyObjectValue(data, item);
                list.Add(item);
            }
            return list;
        }

        public async Task<BizProjectBridgeCastingGroupItemDTO> CastingGroupSave(BizProjectBridgeCastingGroupItemDTO item)
        {
            item.ID = base.SaveInt(item, (entity) =>
            {
                var data = new biz_project_bridge_castingGroup();
                ObjectUtils.CopyObjectValue(entity, data);
                return data;
            });
            return item;
        }

        public async Task<BizProjectBridgeCastingGroupItemDTO> CastingGroupDelete(BizProjectBridgeCastingGroupItemDTO item)
        {
            item.ID = base.Delete(item.ID);
            return item;
        }
    }
}