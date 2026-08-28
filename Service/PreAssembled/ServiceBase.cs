using DataFactory.Factory;
using Tool;

namespace Service.PreAssembled
{
    public class ServiceBase : IDisposable
    {
        private readonly QlPreAssembled_KingBase_UnitOfWorkFactory _qlUowFactory;
        protected QlPreAssembled_KingBase_UnitOfWorkFactory DbFactory => _qlUowFactory;

        private IUnitOfWork _unitOfWork;
        protected IUnitOfWork Db
        {
            get
            {
                if (_unitOfWork == null) _unitOfWork = _qlUowFactory.Create();
                return _unitOfWork;
            }
            set
            {
                _unitOfWork = value;
            }
        }
        protected int TotalCount { private set; get; }
        protected int PageIndex { set; get; } = 0;
        protected int PageSize { set; get; } = 0;
        public ServiceBase(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory)
        {
            _qlUowFactory = qlUowFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<T> GetList<T>(IQueryable<T> query, int? pageIndex = null, int? pageSize = null)
        {
            this.TotalCount = query.Count();
            PageIndex = pageIndex ?? PageIndex;
            PageSize = pageSize ?? PageSize;
            if (PageIndex > 0 && PageSize > 0)
            {
                query = query.Skip((PageIndex - 1) * PageSize).Take(PageSize);
            }
            List<T> list = query.ToList();

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="rowFormatter"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<T> GetList<T>(IQueryable<T> query, Action<T> rowFormatter, int? pageIndex = null, int? pageSize = null)
        {
            this.TotalCount = query.Count();
            PageIndex = pageIndex ?? PageIndex;
            PageSize = pageSize ?? PageSize;
            if (PageIndex > 0 && PageSize > 0)
            {
                query = query.Skip((PageIndex - 1) * PageSize).Take(PageSize);
            }
            List<T> list = query.ToList();
            if (list != null)
            {
                foreach (var data in list)
                {
                    rowFormatter.Invoke(data);
                }
            }

            return list;
        }

        /// <summary>
        /// Set Rows
        /// </summary>
        /// <typeparam name="TIn">Row type</typeparam>
        /// <param name="query">Linq Query</param>
        /// <param name="rowFormatter">Row Formatter</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>this PagedList</returns>
        public List<T> GetList<TIn, T>(IQueryable<TIn> query, Func<TIn, T> rowFormatter, int? pageIndex = null, int? pageSize = null)
        {
            this.TotalCount = query.Count();
            this.PageIndex = pageIndex ?? this.PageIndex;
            this.PageSize = pageSize ?? this.PageSize;
            if (this.PageIndex > 0 && this.PageSize > 0)
            {
                query = query.Skip((this.PageIndex - 1) * this.PageSize).Take(this.PageSize);
            }
            List<T> list = new List<T>();
            var l = query.ToList();
            if (l != null && rowFormatter != null)
            {
                foreach (var data in l)
                {
                    list.Add(rowFormatter.Invoke(data));
                }
            }

            return list;
        }


        /// <summary>
        /// 主键是INT的表公用保存方法（兼容新增和更新）
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity">实体</param>
        /// <param name="formatter">实体和表数据转换方法</param>
        /// <returns></returns>
        public int SaveInt<TItem, TEntity>(TItem entity, Func<TItem, TEntity> formatter)
            where TEntity : class, IIDTable
        {
            var data = formatter(entity);
            var rep = Db.GetRepository<TEntity>();
            if (data.ID == 0)
            {
                rep.Add(data);
                var count = rep.Save();
                if (count > 0)
                {
                    return data.ID;
                }
            }
            else
            {
                var updator = rep.FindByID(data.ID);
                if (updator == null)
                {
                }
                else
                {
                    ObjectUtils.CopyObjectValue(data, updator, ObjectUtils.DefaultSkipList);
                    rep.Update(updator);
                }
                var count = rep.Save();
                if (count > 0)
                {
                    return data.ID;
                }
            }
            return 0;
        }

        /// <summary>
        /// 主键是Long的表公用保存方法（兼容新增和更新）
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="formatter"></param>
        /// <returns></returns>
        public long SaveLong<TItem, TEntity>(TItem entity, Func<TItem, TEntity> formatter)
                    where TEntity : class, ILongIDTable
        {
            var data = formatter(entity);
            var rep = Db.GetRepository<TEntity>();
            if (data.ID == 0)
            {
                rep.Add(data);
                var count = rep.Save();
                if (count > 0)
                {
                    return data.ID;
                }
            }
            else
            {
                var updator = rep.FindFirst(a => a.ID == data.ID);
                if (updator == null)
                {
                }
                else
                {
                    ObjectUtils.CopyObjectValue(data, updator, ObjectUtils.DefaultSkipList);
                    rep.Update(updator);
                }
                var count = rep.Save();
                if (count > 0)
                {
                    return data.ID;
                }
            }
            return 0L;
        }


        public void Dispose()
        {
            if (_qlUowFactory != null)
            {

            }
            if (_unitOfWork != null)
            {
                _unitOfWork.Dispose();
            }
        }
    }

    public class ServiceBase<TEntity>(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory) : ServiceBase(qlUowFactory)
        where TEntity : class, IIDTable
    {
        /// <summary>
        /// 如果使用的是<object>则将dbSet封闭
        /// </summary>
        protected IGenericRepository<TEntity> dbSet => Db.GetRepository<TEntity>();
        public int Delete(int ID)
        {
            var updator = dbSet.FindByID(ID);
            if (updator == null)
            {
            }
            else

            {
                updator.Status = -1;
            }
            var count = this.Db.Save();
            if (count > 0)
            {
                return ID;
            }
            return 0;
        }

    }

    public class ServiceBaseLongID<TEntity>(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory) : ServiceBase(qlUowFactory)
        where TEntity : class, ILongIDTable
    {
        /// <summary>
        /// 如果使用的是<object>则将dbSet封闭
        /// </summary>
        protected IGenericRepository<TEntity> dbSet => Db.GetRepository<TEntity>();
        public long Delete(long ID)
        {
            var updator = dbSet.FindFirst(a => a.ID == ID);
            if (updator == null)
            {
            }
            else
            {
                updator.Status = -1;
            }
            var count = this.Db.Save();
            if (count > 0)
            {
                return ID;
            }
            return 0;
        }

    }
}
