using DataFactory.Factory;
using DataFactory.KingBase;
using Microsoft.AspNetCore.Http;
using Model.tech.QL.DTO.BizProject;
using Tool;

namespace Service.PreAssembled
{
    public class BizProjectService(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory, IHttpContextAccessor httpContextAccessor) 
        : ServiceBase<biz_project>(qlUowFactory, httpContextAccessor)
    {
        public async Task<List<BizProjectItemDTO>> List(BizProjectQueryDTO req)
        {
            var projIDList = GetValidProjectIDs();
            var query = Db.Query<biz_project>().Where(a => a.Status != -1)
                .Where(a => projIDList.Contains(a.ID));

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

        // ==================== 项目授权 ====================

        /// <summary>
        /// 获取项目的已授权用户ID列表
        /// </summary>
        public List<int> GetAuthorizedUserIds(int projectId)
        {
            return Db.Query<biz_project_user>()
                .Where(a => a.ProjID == projectId && a.Status == 1)
                .Select(a => a.UserID)
                .ToList();
        }

        /// <summary>
        /// 保存项目授权（全量覆盖）
        /// </summary>
        public void SaveAuthorization(int projectId, List<int> userIds)
        {
            var rep = Db.GetRepository<biz_project_user>();

            // 物理删除该项目下的所有旧关联
            var oldList = Db.Query<biz_project_user>()
                .Where(a => a.ProjID == projectId).ToList();
            foreach (var old in oldList)
            {
                rep.Delete(old);
            }

            // 新增授权关联
            if (userIds != null)
            {
                foreach (var userId in userIds.Distinct())
                {
                    rep.Add(new biz_project_user
                    {
                        ProjID = projectId,
                        UserID = userId,
                        Status = 1,
                        CreatedTime = DateTime.Now
                    });
                }
            }

            Db.Save();
        }
    }
}
