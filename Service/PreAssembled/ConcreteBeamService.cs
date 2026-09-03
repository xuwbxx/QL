using DataFactory.Factory;
using DataFactory.KingBase;
using Model.tech.QL.DTO.BizProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.PreAssembled
{
    public class ConcreteBeamService(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory) : ServiceBase<biz_project_bridge_castingGroup>(qlUowFactory)
    {
        /// <summary>
        /// 分页查询混凝土梁列表（BeamType=1）
        /// </summary>
        public async Task<ConcreteBeamPagedResultDTO> List(ConcreteBeamQueryDTO req)
        {
            var query = from cg in Db.Query<biz_project_bridge_castingGroup>().Where(a => a.Status != -1)
                        join bridge in Db.Query<biz_project_bridge>().Where(a => a.Status != -1 && a.BeamType == 1)
                          on cg.BridgeID equals bridge.ID
                        join proj in Db.Query<biz_project>().Where(a => a.Status != -1)
                          on bridge.ProjID equals proj.ID
                        select new ConcreteBeamItemDTO
                        {
                            ID = cg.ID,
                            BridgeID = cg.BridgeID,
                            ProjID = bridge.ProjID,
                            ProjectName = proj.Name,
                            BridgeName = bridge.Name,
                            CastingGroupName = cg.Name
                        };

            if (req.ProjID.HasValue && req.ProjID.Value > 0)
                query = query.Where(a => a.ProjID == req.ProjID.Value);

            if (!string.IsNullOrWhiteSpace(req.CastingGroupName))
                query = query.Where(a => a.CastingGroupName.Contains(req.CastingGroupName));

            var total = query.Count();
            var page = req.PageIndex <= 0 ? 1 : req.PageIndex;
            var size = req.PageSize <= 0 ? 10 : req.PageSize;
            var list = query.OrderBy(a => a.ID)
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();

            return new ConcreteBeamPagedResultDTO { List = list, Total = total };
        }

        /// <summary>
        /// 获取有混凝土梁桥梁的项目下拉列表
        /// </summary>
        public async Task<List<object>> ProjectOptions()
        {
            var project = (from cg in Db.Query<biz_project_bridge_castingGroup>().Where(a => a.Status != -1)
                           join bridge in Db.Query<biz_project_bridge>().Where(a => a.Status != -1 && a.BeamType == 1)
                             on cg.BridgeID equals bridge.ID
                           join proj in Db.Query<biz_project>().Where(a => a.Status != -1)
                             on bridge.ProjID equals proj.ID
                           select new { proj.ID, proj.Name })
                           .Distinct()
                           .OrderBy(a => a.Name)
                           .ToList();
            var list = new List<object>();
            foreach (var p in project)
            {
                list.Add(new { id = p.ID, name = p.Name });
            }
            return list;
        }

        /// <summary>
        /// 获取浇筑组信息（项目名称-桥梁名称-浇筑组名称）
        /// </summary>
        public async Task<object> GetCastingGroupInfo(int groupID)
        {
            var info = (from cg in Db.Query<biz_project_bridge_castingGroup>().Where(a => a.Status != -1 && a.ID == groupID)
                        join bridge in Db.Query<biz_project_bridge>().Where(a => a.Status != -1)
                          on cg.BridgeID equals bridge.ID
                        join proj in Db.Query<biz_project>().Where(a => a.Status != -1)
                          on bridge.ProjID equals proj.ID
                        select new { proj.Name, BridgeName = bridge.Name, CastingGroupName = cg.Name, ManagerName=proj.ManagerName, bridge.ProjID, cg.BridgeID })
                        .FirstOrDefault();

            if (info == null)
                return new { projectName = "", bridgeName = "", castingGroupName = "", ManagerName="",projID = 0, bridgeID = 0 };

            return new { projectName = info.Name, bridgeName = info.BridgeName, castingGroupName = info.CastingGroupName, ManagerName=info.ManagerName, projID = info.ProjID, bridgeID = info.BridgeID };
        }
    }
}
