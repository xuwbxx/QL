using DataFactory.Factory;
using DataFactory.MySql;
using Tool;

namespace Service.StructHandle
{
    public class SystemService
    {
        // 依赖注入：仓储工厂（核心）+ 日志（可选，用于异常追踪）
        private readonly StructHandle_MySql_Test_UnitOfWorkFactory _structHandleDbFactory;

        public SystemService(StructHandle_MySql_Test_UnitOfWorkFactory structHandleDbFactory)
        {
            _structHandleDbFactory = structHandleDbFactory;
        }

        public bool UserLogin(string UserName, string Password)
        {
            try
            {

                using (var uow = _structHandleDbFactory.Create())
                {

                    var repo = uow.GetRepository<manage_user>();

                    var loginUser = repo.FindFirst(a => a.IsDelete == 0
                                             && string.Equals(a.UserName, UserName)
                                             && string.Equals(a.Password, Password));

                    //var users = repo.FindAll().ToList();

                    if (loginUser != null)
                    {



                        return true;
                    }

                }

                return false;

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(SystemService));
                throw;
            }
        }

    }
}
