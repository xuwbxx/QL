using Microsoft.EntityFrameworkCore.Storage;

namespace DataFactory.Factory
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        int Save();
        Task<int> SaveAsync();
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        /// <summary>
        /// 只读查询能力
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IQueryable<TEntity> Query<TEntity>() where TEntity : class;
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly BaseDbContext _dbContext;
        private readonly Dictionary<Type, object> _repositories;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        public UnitOfWork(BaseDbContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new Dictionary<Type, object>();
        }

        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);
            if (!_repositories.ContainsKey(type))
            {
                var repository = new GenericRepository<TEntity>(_dbContext);
                _repositories[type] = repository;
            }

            return (IGenericRepository<TEntity>)_repositories[type];
        }

        public int Save()
        {
            return _dbContext.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void BeginTransaction()
        {
            if (_transaction == null)
            {
                _transaction = _dbContext.Database.BeginTransaction();
            }
        }

        public void CommitTransaction()
        {
            try
            {
                _transaction?.Commit();
                _transaction?.Dispose();
                _transaction = null;
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        public void RollbackTransaction()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            _transaction = null;
        }

        public IQueryable<TEntity> Query<TEntity>() where TEntity : class
        {
            return _dbContext.Set<TEntity>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var repository in _repositories.Values.OfType<IDisposable>())
                    {
                        repository.Dispose();
                    }
                    _repositories.Clear();

                    _transaction?.Dispose();
                    _dbContext?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
