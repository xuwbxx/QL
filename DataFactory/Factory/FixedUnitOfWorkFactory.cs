namespace DataFactory.Factory
{
    public interface IFixedUnitOfWorkFactory
    {
        IUnitOfWork Create();
    }

    public class FixedUnitOfWorkFactory : IFixedUnitOfWorkFactory
    {
        private readonly string _connectionName;
        private readonly MultiDbRepositoryFactory _multiDbFactory;

        public FixedUnitOfWorkFactory(string connectionName, MultiDbRepositoryFactory multiDbFactory)
        {
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _multiDbFactory = multiDbFactory ?? throw new ArgumentNullException(nameof(multiDbFactory));
        }

        public IUnitOfWork Create()
        {
            return _multiDbFactory.GetUnitOfWork(_connectionName);
        }
    }

    // 为每个数据库创建专用的工作单元工厂


    #region 正式数据库

    /// <summary>
    /// 技术中心集成平台10.6.48.25
    /// </summary>
    public class TechCenter_KingBase_UnitOfWorkFactory : FixedUnitOfWorkFactory
    {
        public TechCenter_KingBase_UnitOfWorkFactory(string connectionName, MultiDbRepositoryFactory multiDbFactory)
            : base(connectionName, multiDbFactory) { }
    }

    /// <summary>
    /// 风电云平台10.6.48.21
    /// </summary>
    public class CloudWind_KingBase_UnitOfWorkFactory : FixedUnitOfWorkFactory
    {
        public CloudWind_KingBase_UnitOfWorkFactory(string connectionName, MultiDbRepositoryFactory multiDbFactory)
            : base(connectionName, multiDbFactory) { }
    }

    /// <summary>
    /// 拖航
    /// </summary>
    public class Towing_KingBase_UnitOfWorkFactory : FixedUnitOfWorkFactory
    {
        public Towing_KingBase_UnitOfWorkFactory(string connectionName, MultiDbRepositoryFactory multiDbFactory)
            : base(connectionName, multiDbFactory) { }
    }

    /// <summary>
    /// 桥梁监测10.6.48.25
    /// </summary>
    public class QLJC_KingBase_UnitOfWorkFactory : FixedUnitOfWorkFactory
    {
        public QLJC_KingBase_UnitOfWorkFactory(string connectionName, MultiDbRepositoryFactory multiDbFactory)
            : base(connectionName, multiDbFactory) { }
    }

    /// <summary>
    /// 深基坑监测10.6.48.26
    /// </summary>
    public class SJKJC_KingBase_UnitOfWorkFactory : FixedUnitOfWorkFactory
    {
        public SJKJC_KingBase_UnitOfWorkFactory(string connectionName, MultiDbRepositoryFactory multiDbFactory)
            : base(connectionName, multiDbFactory) { }
    }

    /// <summary>
    /// 云平台10.6.55.74
    /// </summary>
    public class CloudWind_Sql_UnitOfWorkFactory : FixedUnitOfWorkFactory
    {
        public CloudWind_Sql_UnitOfWorkFactory(string connectionName, MultiDbRepositoryFactory multiDbFactory)
            : base(connectionName, multiDbFactory) { }
    }

    /// <summary>
    /// 拖航监控10.6.55.74
    /// </summary>
    public class Towing_Sql_UnitOfWorkFactory : FixedUnitOfWorkFactory
    {
        public Towing_Sql_UnitOfWorkFactory(string connectionName, MultiDbRepositoryFactory multiDbFactory)
            : base(connectionName, multiDbFactory) { }
    }

    //桥梁预拼装
    public class QlPreAssembled_KingBase_UnitOfWorkFactory : FixedUnitOfWorkFactory
    {
        public QlPreAssembled_KingBase_UnitOfWorkFactory(string connectionName, MultiDbRepositoryFactory multiDbFactory)
            : base(connectionName, multiDbFactory) { }
    }

    #endregion


    #region 测试数据库

    /// <summary>
    /// kingbase测试库
    /// </summary>
    public class TestDB_KingBase_Test_UnitOfWorkFactory : FixedUnitOfWorkFactory
    {
        public TestDB_KingBase_Test_UnitOfWorkFactory(string connectionName, MultiDbRepositoryFactory multiDbFactory)
            : base(connectionName, multiDbFactory) { }
    }


    public class CloudWind_Sql_Test_UnitOfWorkFactory : FixedUnitOfWorkFactory
    {
        public CloudWind_Sql_Test_UnitOfWorkFactory(string connectionName, MultiDbRepositoryFactory multiDbFactory)
            : base(connectionName, multiDbFactory) { }
    }

    public class Towing_Sql_Test_UnitOfWorkFactory : FixedUnitOfWorkFactory
    {
        public Towing_Sql_Test_UnitOfWorkFactory(string connectionName, MultiDbRepositoryFactory multiDbFactory)
            : base(connectionName, multiDbFactory) { }
    }

    public class StructHandle_MySql_Test_UnitOfWorkFactory : FixedUnitOfWorkFactory
    {
        public StructHandle_MySql_Test_UnitOfWorkFactory(string connectionName, MultiDbRepositoryFactory multiDbFactory)
            : base(connectionName, multiDbFactory) { }
    }

    #endregion


}
