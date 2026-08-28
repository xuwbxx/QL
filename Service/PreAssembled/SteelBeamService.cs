using DataFactory.Factory;
using DataFactory.KingBase;
using Model.tech.QL.DTO.BizProject;

namespace Service.PreAssembled
{
    public class SteelBeamService(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory) : ServiceBase<biz_project_bridge>(qlUowFactory)
    {
        /// <summary>
        /// 分页查询钢梁列表（BeamType=0）
        /// </summary>
        public async Task<SteelBeamPagedResultDTO> List(SteelBeamQueryDTO req)
        {
            var query = from bridge in Db.Query<biz_project_bridge>().Where(a => a.Status != -1 && a.BeamType == 0)
                        join proj in Db.Query<biz_project>().Where(a => a.Status != -1)
                          on bridge.ProjID equals proj.ID
                        select new SteelBeamItemDTO
                        {
                            ID = bridge.ID,
                            ProjID = bridge.ProjID,
                            ProjectName = proj.Name,
                            BridgeName = bridge.Name,
                            BeamType = bridge.BeamType
                        };

            if (req.ProjID.HasValue && req.ProjID.Value > 0)
                query = query.Where(a => a.ProjID == req.ProjID.Value);

            if (req.BridgeID.HasValue && req.BridgeID.Value > 0)
                query = query.Where(a => a.ID == req.BridgeID.Value);

            var total = query.Count();
            var page = req.PageIndex <= 0 ? 1 : req.PageIndex;
            var size = req.PageSize <= 0 ? 10 : req.PageSize;
            var list = query.OrderBy(a => a.ID)
                             .Skip((page - 1) * size)
                             .Take(size)
                             .ToList();

            return new SteelBeamPagedResultDTO { List = list, Total = total };
        }

        /// <summary>
        /// 获取有钢梁桥梁的项目下拉列表
        /// </summary>
        public async Task<List<object>> ProjectOptions()
        {
            var project = (from bridge in Db.Query<biz_project_bridge>().Where(a => a.Status != -1 && a.BeamType == 0)
                           join proj in Db.Query<biz_project>().Where(a => a.Status != -1)
                           on bridge.ProjID equals proj.ID
                           select new { proj.ID, proj.Name })
                         .Distinct()
                         .OrderBy(a => a.Name)
                         .ToList();
            var list = new List<object>();
            foreach (var p in project)
            {
                list.Add(new { id = p.ID, Name = p.Name });
            }
            return list;
        }

        /// <summary>
        /// 按项目获取钢梁桥梁下拉列表
        /// </summary>
        public async Task<List<object>> BridgeOptions(int projID)
        {
            var bridges = Db.Query<biz_project_bridge>()
                .Where(a => a.Status != -1 && a.BeamType == 0 && a.ProjID == projID)
                .OrderBy(a => a.Name)
                .ToList();
            var list = new List<object>();
            foreach (var b in bridges)
            {
                list.Add(new { id = b.ID, Name = b.Name });
            }
            return list;
        }

        /// <summary>
        /// 获取桥梁信息（项目名称-桥梁名称）
        /// </summary>
        public async Task<object> GetBridgeInfo(int bridgeID)
        {
            var info = (from bridge in Db.Query<biz_project_bridge>().Where(a => a.Status != -1 && a.ID == bridgeID)
                        join proj in Db.Query<biz_project>().Where(a => a.Status != -1)
                          on bridge.ProjID equals proj.ID
                        select new { proj.Name, BridgeName = bridge.Name, bridge.ProjID, bridge.BeamType })
                       .FirstOrDefault();

            if (info == null)
                return new { projectName = "", bridgeName = "", projID = 0, beamType = 0 };

            return new { projectName = info.Name, bridgeName = info.BridgeName, projID = info.ProjID, beamType = info.BeamType };
        }



    }
}
