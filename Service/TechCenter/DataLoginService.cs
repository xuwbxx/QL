using DataFactory.Factory;
using DataFactory.KingBase;
using Tool;

namespace Service.TechCenter
{
    public class DataLoginService
    {

        // 依赖注入：仓储工厂（核心）+ 日志（可选，用于异常追踪）
        private readonly TechCenter_KingBase_UnitOfWorkFactory _techCenterDbFactory;

        public DataLoginService(TechCenter_KingBase_UnitOfWorkFactory techCenterDbFactory)
        {
            _techCenterDbFactory = techCenterDbFactory;
        }

        //表格操作
        #region TechCenter_DataLogin
        public async Task DataLoginAdd(TechCenter_DataLogin data)
        {
            try
            {
                if (data == null)
                {
                    return;
                }

                using (var uow = _techCenterDbFactory.Create())
                {

                    var repo = uow.GetRepository<TechCenter_DataLogin>();

                    //await repo.AddAsync(data);
                    //int count = await repo.SaveAsync();

                    string ThisSql = SqlUtils.ToInsertSql(data, "Data_Login");
                    int count = await repo.ExecuteSqlAsync(ThisSql);

                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(DataLoginService));
                throw;
            }
        }

        public async Task DataLoginUpdate(TechCenter_DataLogin data)
        {
            try
            {
                if (data == null)
                {
                    return;
                }

                using (var uow = _techCenterDbFactory.Create())
                {

                    var repo = uow.GetRepository<TechCenter_DataLogin>();

                    await repo.UpdateAsync(data);
                    int count = await repo.SaveAsync();

                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(DataLoginService));
                throw;
            }
        }


        #endregion

        #region TechCenter_DataLoginResult
        public async Task DataLoginResultAdd(TechCenter_DataLoginResult data)
        {
            try
            {
                if (data == null)
                {
                    return;
                }

                using (var uow = _techCenterDbFactory.Create())
                {

                    var repo = uow.GetRepository<TechCenter_DataLoginResult>();

                    await repo.AddAsync(data);
                    int count = await repo.SaveAsync();

                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(DataLoginService));
                throw;
            }
        }

        public async Task DataLoginResultUpdate(TechCenter_DataLoginResult data)
        {
            try
            {
                if (data == null)
                {
                    return;
                }

                using (var uow = _techCenterDbFactory.Create())
                {

                    var repo = uow.GetRepository<TechCenter_DataLoginResult>();

                    await repo.UpdateAsync(data);
                    int count = await repo.SaveAsync();

                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(DataLoginService));
                throw;
            }
        }

        public async Task DataLoginResultQuery(TechCenter_DataLoginResult data)
        {
            try
            {
                if (data == null)
                {
                    return;
                }

                using (var uow = _techCenterDbFactory.Create())
                {

                    var repo = uow.GetRepository<TechCenter_DataLoginResult>();

                    await repo.UpdateAsync(data);
                    int count = await repo.SaveAsync();

                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(DataLoginService));
                throw;
            }
        }


        #endregion

        //#region TechCenter_Manage_UserSoftware
        //public async Task Manage_UserSoftware_Add(TechCenter_Manage_UserSoftware data)
        //{
        //    try
        //    {
        //        if (data == null)
        //        {
        //            return;
        //        }

        //        using (var repo = _techCenterDbFactory.GetRepository<TechCenter_DataLogin>())
        //        {
        //            await repo.AddAsync(data);
        //            int count = await repo.SaveAsync();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LoggerUtils.Error(ex.ToString(), typeof(DataLoginService));
        //        throw;
        //    }
        //}

        //public async Task DataLoginUpdate(TechCenter_DataLogin data)
        //{
        //    try
        //    {
        //        if (data == null)
        //        {
        //            return;
        //        }

        //        using (var repo = _techCenterDbFactory.GetRepository<TechCenter_DataLogin>())
        //        {
        //            await repo.UpdateAsync(data);
        //            int count = await repo.SaveAsync();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LoggerUtils.Error(ex.ToString(), typeof(DataLoginService));
        //        throw;
        //    }
        //}


        //#endregion
    }
}
