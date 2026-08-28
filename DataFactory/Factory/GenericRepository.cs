using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;

namespace DataFactory.Factory
{
    //这个是工具类，不要修改
    public interface IGenericRepository<TEntity> : IDisposable where TEntity : class
    {
        // 1. 查询所有实体
        IEnumerable<TEntity> FindAll(); // 同步
        Task<IEnumerable<TEntity>> FindAllAsync(); // 异步


        // 2. 按条件查询实体
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate); // 同步
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate); // 异步

        IQueryable<TEntity> FindQueryable(Expression<Func<TEntity, bool>> predicate);

        // 7. 分页查询（返回数据列表和总数）
        (IEnumerable<TEntity> list, int totalCount) FindPage(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, object>>? orderBy, int pageIndex, int pageSize);
        Task<(IEnumerable<TEntity> list, int totalCount)> FindPageAsync(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, object>>? orderBy, int pageIndex, int pageSize);

        // 8. 新增：数据操作方法（同步+异步）
        TEntity? FindFirst(Expression<Func<TEntity, bool>> predicate); // 同步
        Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>> predicate); // 异步

        // 4. 按int型主键查询
        TEntity? FindByID(int id); // 同步
        Task<TEntity?> FindByIDAsync(int id); // 异步

        // 5. 按条件统计数量
        int FindCount(Expression<Func<TEntity, bool>>? predicate = null); // 同步
        Task<int> FindCountAsync(Expression<Func<TEntity, bool>>? predicate = null); // 异步

        // 6. 查询最新一条数据（按时间字段排序）
        TEntity? FindNew(Expression<Func<TEntity, Int32>> idSelector); // 同步
        Task<TEntity?> FindNewAsync(Expression<Func<TEntity, Int32>> idSelector); // 异步


        // 新增：数据操作方法（同步+异步）
        #region 新增
        void Add(TEntity entity); // 1. 增加单个实体（同步）
        Task AddAsync(TEntity entity); // 1. 增加单个实体（异步）
        void AddList(IEnumerable<TEntity> entities); // 2. 增加实体列表（同步）
        Task AddListAsync(IEnumerable<TEntity> entities); // 2. 增加实体列表（异步）
        #endregion

        #region 删除
        void Delete(TEntity entity); // 3. 删除单个实体（同步）
        Task DeleteAsync(TEntity entity); // 新增：删除单个实体（异步）
        void DeleteList(IEnumerable<TEntity> entities); // 4. 删除实体列表（同步）
        Task DeleteListAsync(IEnumerable<TEntity> entities); // 新增：删除实体列表（异步）
        #endregion

        #region 更新
        void Update(TEntity entity); // 5. 更新单个实体（同步）

        Task UpdateAsync(TEntity entity);
        #endregion

        #region 事务提交
        int Save(); // 6. 提交事务（同步，返回受影响的行数）
        Task<int> SaveAsync(); // 6. 提交事务（异步，返回受影响的行数）
        #endregion


        #region ADO.NET 原生SQL方法（精简版）
        // 1. 执行查询SQL，返回实体数据集
        IEnumerable<TEntity> QueryBySql(string sql);
        Task<IEnumerable<TEntity>> QueryBySqlAsync(string sql);

        // 2. 执行增删改SQL，返回受影响行数
        int ExecuteSql(string sql);
        Task<int> ExecuteSqlAsync(string sql);

        // 3. 执行查询SQL，返回DataTable
        DataTable QueryDataTable(string sql);
        Task<DataTable> QueryDataTableAsync(string sql);
        #endregion



    }

    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly BaseDbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;
        private bool _disposed = false; // 标记是否已释放

        // 构造函数：注入数据库上下文
        public GenericRepository(BaseDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }

        #region 1. 查询所有实体
        public IEnumerable<TEntity> FindAll()
        {
            return _dbSet.ToList(); // 默认跟踪实体（无AsNoTracking）
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync()
        {
            return await _dbSet.ToListAsync(); // 异步版本
        }
        #endregion

        #region 2. 按条件查询实体
        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.Where(predicate).ToList(); // 默认跟踪实体
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync(); // 异步版本
        }

        public IQueryable<TEntity> FindQueryable(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.Where(predicate); // 默认跟踪实体
        }

        #endregion

        #region 7. 分页查询
        public (IEnumerable<TEntity> list, int totalCount) FindPage(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, object>>? orderBy, int pageIndex, int pageSize)
        {
            IQueryable<TEntity> query = _dbSet;

            if (predicate != null)
                query = query.Where(predicate);

            int totalCount = query.Count();

            if (orderBy != null)
                query = query.OrderBy(orderBy);

            var list = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return (list, totalCount);
        }

        public async Task<(IEnumerable<TEntity> list, int totalCount)> FindPageAsync(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, object>>? orderBy, int pageIndex, int pageSize)
        {
            IQueryable<TEntity> query = _dbSet;

            if (predicate != null)
                query = query.Where(predicate);

            int totalCount = await query.CountAsync();

            if (orderBy != null)
                query = query.OrderBy(orderBy);

            var list = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return (list, totalCount);
        }
        #endregion

        #region 3. 按条件查询第一个实体
        public TEntity? FindFirst(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.FirstOrDefault(predicate); // 默认跟踪实体
        }

        public async Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate); // 异步版本
        }
        #endregion

        #region 4. 按int型主键查询
        public TEntity? FindByID(int id)
        {
            return _dbSet.Find(id); // Find方法默认跟踪实体（优先查本地缓存）
        }

        public async Task<TEntity?> FindByIDAsync(int id)
        {
            return await _dbSet.FindAsync(id); // 异步版本
        }
        #endregion

        #region 5. 按条件统计数量
        public int FindCount(Expression<Func<TEntity, bool>>? predicate = null)
        {
            return predicate == null ? _dbSet.Count() : _dbSet.Count(predicate);
        }

        public async Task<int> FindCountAsync(Expression<Func<TEntity, bool>>? predicate = null)
        {
            return predicate == null
                ? await _dbSet.CountAsync()
                : await _dbSet.CountAsync(predicate);
        }
        #endregion

        #region 6. 查询最新一条数据（按时间字段排序）
        public TEntity? FindNew(Expression<Func<TEntity, Int32>> idSelector)
        {
            return _dbSet.OrderByDescending(idSelector).FirstOrDefault(); // 默认跟踪实体
        }

        public async Task<TEntity?> FindNewAsync(Expression<Func<TEntity, Int32>> idSelector)
        {
            return await _dbSet.OrderByDescending(idSelector).FirstOrDefaultAsync(); // 异步版本
        }
        #endregion




        #region 新增实体
        /// <summary>
        /// 增加单个实体（需调用Save/SaveAsync生效）
        /// </summary>
        public void Add(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "新增的实体不能为null");

            _dbSet.Add(entity);
        }

        /// <summary>
        /// 异步增加单个实体（需调用Save/SaveAsync生效）
        /// </summary>
        public async Task AddAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "新增的实体不能为null");

            await _dbSet.AddAsync(entity);
        }

        /// <summary>
        /// 增加实体列表（需调用Save/SaveAsync生效）
        /// </summary>
        public void AddList(IEnumerable<TEntity> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentException("新增的实体列表不能为null或空", nameof(entities));

            _dbSet.AddRange(entities);
        }

        /// <summary>
        /// 异步增加实体列表（需调用Save/SaveAsync生效）
        /// </summary>
        public async Task AddListAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentException("新增的实体列表不能为null或空", nameof(entities));

            await _dbSet.AddRangeAsync(entities);
        }
        #endregion


        #region 删除实体
        /// <summary>
        /// 删除单个实体（需调用Save/SaveAsync生效）
        /// </summary>
        public void Delete(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "删除的实体不能为null");

            // 如果实体未被跟踪，先附加到上下文
            if (_dbContext.Entry(entity).State == EntityState.Detached)
                _dbSet.Attach(entity);

            _dbSet.Remove(entity);
        }

        /// <summary>
        /// 异步删除单个实体（需调用Save/SaveAsync生效）
        /// </summary>
        /// <remarks>EF Core的Remove操作本身是同步的，此方法为API一致性设计</remarks>
        public async Task DeleteAsync(TEntity entity)
        {
            // 利用Task.Run包装同步操作，避免阻塞调用线程（适用于UI线程等场景）
            await Task.Run(() => Delete(entity));
        }

        /// <summary>
        /// 删除实体列表（需调用Save/SaveAsync生效）
        /// </summary>
        public void DeleteList(IEnumerable<TEntity> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentException("删除的实体列表不能为null或空", nameof(entities));

            // 批量处理：未跟踪的实体先附加
            foreach (var entity in entities)
            {
                if (_dbContext.Entry(entity).State == EntityState.Detached)
                    _dbSet.Attach(entity);
            }

            _dbSet.RemoveRange(entities);
        }

        /// <summary>
        /// 异步删除实体列表（需调用Save/SaveAsync生效）
        /// </summary>
        /// <remarks>EF Core的RemoveRange操作本身是同步的，此方法为API一致性设计</remarks>
        public async Task DeleteListAsync(IEnumerable<TEntity> entities)
        {
            // 利用Task.Run包装同步操作，避免阻塞调用线程
            await Task.Run(() => DeleteList(entities));
        }


        #endregion


        #region 更新实体
        /// <summary>
        /// 更新单个实体（需调用Save/SaveAsync生效）
        /// </summary>
        public void Update(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "更新的实体不能为null");

            // 标记实体为已修改（如果未跟踪则先附加）
            if (_dbContext.Entry(entity).State == EntityState.Detached)
                _dbSet.Attach(entity);

            _dbContext.Entry(entity).State = EntityState.Modified;
        }


        /// <summary>
        /// 异步更新单个实体（需调用Save/SaveAsync生效）
        /// </summary>
        /// <remarks>EF Core的State设置操作本身是同步的，此方法为API一致性设计</remarks>
        public async Task UpdateAsync(TEntity entity)
        {
            // 利用Task.Run包装同步操作，避免阻塞调用线程
            await Task.Run(() => Update(entity));
        }

        #endregion


        #region 事务提交
        /// <summary>
        /// 提交所有更改（同步）
        /// </summary>
        /// <returns>受影响的行数</returns>
        public int Save()
        {
            return _dbContext.SaveChanges();
        }

        /// <summary>
        /// 异步提交所有更改（异步）
        /// </summary>
        /// <returns>受影响的行数</returns>
        public async Task<int> SaveAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
        #endregion





        #region ADO.NET 原生SQL方法（精简版实现）
        /// <summary>
        /// 执行查询SQL，返回实体数据集
        /// </summary>
        /// <param name="sql">查询语句（如SELECT * FROM table）</param>
        public IEnumerable<TEntity> QueryBySql(string sql)
        {
            ValidateSql(sql);
            // 调用金仓驱动执行查询，返回实体列表
            return _dbContext.Set<TEntity>().FromSqlRaw(sql).ToList();
        }

        /// <summary>
        /// 异步执行查询SQL，返回实体数据集
        /// </summary>
        public async Task<IEnumerable<TEntity>> QueryBySqlAsync(string sql)
        {
            ValidateSql(sql);
            return await _dbContext.Set<TEntity>().FromSqlRaw(sql).ToListAsync();
        }

        /// <summary>
        /// 执行增删改SQL，返回受影响的行数
        /// </summary>
        /// <param name="sql">增删改语句（如INSERT/UPDATE/DELETE）</param>
        public int ExecuteSql(string sql)
        {
            ValidateSql(sql);
            // 执行非查询SQL
            return _dbContext.Database.ExecuteSqlRaw(sql);
        }

        /// <summary>
        /// 异步执行增删改SQL，返回受影响的行数
        /// </summary>
        public async Task<int> ExecuteSqlAsync(string sql)
        {
            ValidateSql(sql);
            return await _dbContext.Database.ExecuteSqlRawAsync(sql);
        }

        /// <summary>
        /// 执行查询SQL，返回DataTable
        /// </summary>
        /// <param name="sql">查询语句（如SELECT * FROM table）</param>
        public DataTable QueryDataTable(string sql)
        {
            ValidateSql(sql);
            var connection = _dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            var wasClosed = connection.State == ConnectionState.Closed;
            if (wasClosed) connection.Open();
            try
            {
                using var reader = command.ExecuteReader();
                var dt = new DataTable();
                dt.Load(reader);
                return dt;
            }
            finally
            {
                if (wasClosed) connection.Close();
            }
        }

        /// <summary>
        /// 异步执行查询SQL，返回DataTable
        /// </summary>
        public async Task<DataTable> QueryDataTableAsync(string sql)
        {
            ValidateSql(sql);
            var connection = _dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            var wasClosed = connection.State == ConnectionState.Closed;
            if (wasClosed) await connection.OpenAsync();
            try
            {
                using var reader = await command.ExecuteReaderAsync();
                var dt = new DataTable();
                dt.Load(reader);
                return dt;
            }
            finally
            {
                if (wasClosed) connection.Close();
            }
        }

        //私有辅助方法
        /// <summary>
        /// 校验SQL语句非空
        /// </summary>
        private void ValidateSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL语句不能为空或仅包含空白字符", nameof(sql));
        }
        #endregion












        // 实现Dispose，释放DbContext
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // 告诉GC无需执行Finalize
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            // 释放托管资源（DbContext）
            if (disposing)
            {
                _dbContext?.Dispose();
            }

            _disposed = true;
        }

        // 析构函数，防止未手动调用Dispose时内存泄漏
        ~GenericRepository()
        {
            Dispose(false);
        }

    }
}
