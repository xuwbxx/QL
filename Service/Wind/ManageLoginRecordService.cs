using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;

namespace Service.Wind
{
    public class ManageLoginRecordService
    {

        // 依赖注入：仓储工厂（核心）+ 日志（可选，用于异常追踪）
        private readonly CloudWind_KingBase_UnitOfWorkFactory _techCenterUowFactory;

        public ManageLoginRecordService(CloudWind_KingBase_UnitOfWorkFactory techCenterUowFactory)
        {
            _techCenterUowFactory = techCenterUowFactory;
        }

        public void add(Manage_LoginRecord data)
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var repo = uow.GetRepository<Manage_LoginRecord>();

                repo.Add(data);

                repo.Save();

            }
        }

    }
}
