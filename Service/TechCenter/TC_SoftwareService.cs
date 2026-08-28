using DataFactory.Factory;
using DataFactory.KingBase;
using Tool;

namespace Service.TechCenter
{
    public class TC_SoftwareService
    {
        // 依赖注入：仓储工厂（核心）+ 日志（可选，用于异常追踪）
        private readonly TechCenter_KingBase_UnitOfWorkFactory _techCenterUowFactory;

        public TC_SoftwareService(TechCenter_KingBase_UnitOfWorkFactory techCenterUowFactory)
        {
            _techCenterUowFactory = techCenterUowFactory;
        }

        public TechCenter_Manage_Software? GetSoftwareInfo(int SoftwareID)
        {
            TechCenter_Manage_Software data = new TechCenter_Manage_Software();

            try
            {

                using (var uow = _techCenterUowFactory.Create())
                {
                    var repo = uow.GetRepository<TechCenter_Manage_Software>();

                    data = repo.Find(a => !a.IsDelete && a.ID == SoftwareID).FirstOrDefault();

                    return data;

                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(DataLoginService));
                return null;
            }
        }

    }
}
