using Microsoft.EntityFrameworkCore;

namespace DataFactory.Factory
{
    // 数据库上下文（单数据库）
    public class BaseDbContext : DbContext
    {
        private readonly string _connectionString;
        private readonly DatabaseType _databaseType;

        // 数据库类型枚举
        public enum DatabaseType
        {
            KingBase,
            SqlServer,
            MySql,
            Oracle
        }

        // 构造函数：接受连接字符串和数据库类型
        public BaseDbContext(string connectionString, DatabaseType databaseType)
            : base(GetOptions(connectionString, databaseType))
        {
            _connectionString = connectionString;
            _databaseType = databaseType;
        }

        // 动态配置数据库驱动
        private static DbContextOptions GetOptions(string connectionString, DatabaseType databaseType)
        {
            var builder = new DbContextOptionsBuilder();
            switch (databaseType)
            {
                case DatabaseType.KingBase:
                    builder.UseKdbndp(connectionString); // Kingbase驱动
                    break;
                case DatabaseType.SqlServer:
                    builder.UseSqlServer(connectionString); // SQL Server驱动
                    break;
                case DatabaseType.MySql:
                    // 使用 Pomelo.EntityFrameworkCore.MySql（推荐的 MySQL EF Core 驱动）
                    builder.UseMySql(
                        connectionString,
                        // 指定 MySQL 服务器版本（根据实际环境调整，如 8.0、5.7 等）
                        new MySqlServerVersion(new Version(8, 0, 45)),
                        // 可选：添加 MySQL 特有配置
                        options => options.EnableRetryOnFailure() // 启用失败重试
                    );
                    break;
                //MySql和Oracle还没有集成
                //case DatabaseType.MySql:
                //    builder.use(connectionString); // SQL Server驱动
                //    break;
                default:
                    throw new NotSupportedException($"不支持的数据库类型");
            }
            return builder.Options;
        }

    }
}
