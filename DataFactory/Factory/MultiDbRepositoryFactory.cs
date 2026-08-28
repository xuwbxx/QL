using DataFactory.KingBase;
using DataFactory.MySql;
using DataFactory.SqlServer;
using Microsoft.Extensions.Configuration;

namespace DataFactory.Factory
{
    public class MultiDbRepositoryFactory
    {
        private readonly IConfiguration _configuration;

        public MultiDbRepositoryFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 获取指定连接的工作单元
        /// </summary>
        public IUnitOfWork GetUnitOfWork(string connectionName)
        {
            var connectionString = _configuration.GetConnectionString(connectionName)
                ?? throw new KeyNotFoundException($"未找到连接名：{connectionName}");

            // 根据连接名前缀判断数据库类型
            var dbType = DetermineDatabaseType(connectionName);

            // 创建对应数据库的上下文
            BaseDbContext dbContext = connectionName switch
            {
                // KingBase数据库
                "KingBase_TestDBConnection" => new TestDbContext(connectionString, dbType),
                "KingBase_CloudWindConnection" => new KingBase.CloudWind.CloudWindDbContext(connectionString, dbType),
                "KingBase_QLJCConnection" => new QLJCDbContext(connectionString, dbType),
                "KingBase_TechCenterDBConnection" => new TechCenterDbContext(connectionString, dbType),
                "KingBase_SJKJCConnection" => new SJKJCDbContext(connectionString, dbType),
                "KingBase_QlPreAssembledDBConnection" => new QlPreAssembledDbContext(connectionString, dbType),

                // SqlServer数据库
                "SqlServer_CloudWindConnection" => new SqlServer.CloudWindDbContext(connectionString, dbType),
                "SqlServer_TowingDbConnection" => new TowingDbContext(connectionString, dbType),
                "SqlServer_TestCloudWindConnection" => new CloudWindTestDbContext(connectionString, dbType),
                "SqlServer_TestTowingDbConnection" => new TowingTestDbContext(connectionString, dbType),

                //MySql
                "MySql_StructHandleDbConnection" => new StructHandleDbContext(connectionString, dbType),

                _ => throw new NotSupportedException($"不支持的连接：{connectionName}")
            };

            return new UnitOfWork(dbContext);
        }

        /// <summary>
        /// 获取指定连接的仓储（兼容旧代码）
        /// </summary>
        public IGenericRepository<TEntity> GetRepository<TEntity>(string connectionName) where TEntity : class
        {
            var uow = GetUnitOfWork(connectionName);
            return uow.GetRepository<TEntity>();
        }

        private BaseDbContext.DatabaseType DetermineDatabaseType(string connectionName)
        {
            return connectionName switch
            {
                string name when name.StartsWith("SqlServer_") => BaseDbContext.DatabaseType.SqlServer,
                string name when name.StartsWith("KingBase_") => BaseDbContext.DatabaseType.KingBase,
                string name when name.StartsWith("Oracle_") => BaseDbContext.DatabaseType.Oracle,
                string name when name.StartsWith("MySql_") => BaseDbContext.DatabaseType.MySql,
                _ => BaseDbContext.DatabaseType.KingBase
            };
        }
    }
}
