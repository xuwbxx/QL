using DataFactory.Factory;
using DataFactory.SqlServer;

namespace Service.Towing
{
    public class ProjectAreaService
    {
        private readonly Towing_Sql_UnitOfWorkFactory _towingUowFactory;

        public ProjectAreaService(Towing_Sql_UnitOfWorkFactory towingUowFactory)
        {
            _towingUowFactory = towingUowFactory;
        }

        public async Task<List<Manage_Area>?> GetListAsync()
        {
            try
            {
                using (var uow = _towingUowFactory.Create())
                {
                    // 获取软件仓储
                    var areaRepo = uow.GetRepository<Manage_Area>();
                    // 获取部门仓储
                    var list = await areaRepo.FindAllAsync();

                    return list.ToList();
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
