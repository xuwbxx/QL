using DataFactory.Factory;
using DataFactory.KingBase;
using Tool;

namespace Service.PreAssembled
{
    public class testTableService
    {

        private readonly QlPreAssembled_KingBase_UnitOfWorkFactory _qlUowFactory;

        public testTableService(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory)
        {
            _qlUowFactory = qlUowFactory;
        }

        public async Task<List<testTable>?> GetListAsync()
        {
            try
            {
                using (var uow = _qlUowFactory.Create())
                {
                    // 获取软件仓储
                    var areaRepo = uow.GetRepository<testTable>();

                    // 获取部门仓储
                    var list = await areaRepo.FindAllAsync();

                    var query = from usr in uow.Query<sys_userinfo>().Where(a => a.Status != -1)
                                from dep in uow.Query<sys_dept>().Where(a => a.Status != -1)
                                   .Where(a => usr.DeptOID == a.OID)
                                select new
                                {
                                    usr,
                                    dep
                                };

                    return list.ToList();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(testTableService));
                return null;
            }
        }

        public async Task Add(testTable data)
        {
            try
            {
                using (var uow = _qlUowFactory.Create())
                {
                    // 获取软件仓储
                    var areaRepo = uow.GetRepository<testTable>();
                    // 获取部门仓储
                    await areaRepo.AddAsync(data);

                    await areaRepo.SaveAsync();
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(testTableService));
            }
        }

        public async Task<List<testTable>>? queryBySql(String sql)
        {
            try
            {
                using (var uow = _qlUowFactory.Create())
                {
                    // 获取软件仓储
                    var areaRepo = uow.GetRepository<testTable>();
                    // 获取部门仓储
                    var list = await areaRepo.QueryBySqlAsync(sql);

                    return list.ToList();

                    //await areaRepo.SaveAsync();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }


    }
}
