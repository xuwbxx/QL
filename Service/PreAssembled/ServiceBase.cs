using DataFactory.Factory;
using DataFactory.KingBase;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Model.tech.QL.DTO.SysUser;
using System.Data;
using System.Security.Principal;
using Tool;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Service.PreAssembled
{
    public class ServiceBase : IDisposable
    {
        private readonly QlPreAssembled_KingBase_UnitOfWorkFactory _qlUowFactory;
        protected QlPreAssembled_KingBase_UnitOfWorkFactory DbFactory => _qlUowFactory;
        private LoginUserInfoDTO _currUser;
        protected LoginUserInfoDTO CurrUser
        {
            get
            {
                if (_currUser == null && _httpContextAccessor.HttpContext != null)
                    SetUser(_httpContextAccessor.HttpContext.User?.Identity);
                return _currUser;
            }
        }
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        public static readonly int StatusDelete = -1;
        public static readonly int StatusNormal = 0;
        public static readonly int StatusSubmit = 1;
        public ServiceBase(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory)
        {
            _qlUowFactory = qlUowFactory;
        }
        public ServiceBase(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _qlUowFactory = qlUowFactory;
        }
        public ServiceBase(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory, LoginUserInfoDTO user)
        {
            _currUser = user;
            _qlUowFactory = qlUowFactory;
        }

        public ServiceBase(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory, IIdentity user)
        {
            _qlUowFactory = qlUowFactory;
            SetUser(user);
        }

        public ServiceBase SetUser(IIdentity user)
        {
            if (user == null) return this;
            _currUser = new LoginUserInfoDTO()
            {
                Account = user.GetAccount(),
                Name = user.Name,
                DeptName = user.GetDeptName(),
                DeptOID = user.GetDeptOID()
            };

            return this;
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
                if (CurrUser != null)
                {
                    data.CreatedBy = CurrUser?.Account;
                    data.UpdatedBy = CurrUser?.Account;
                }
                else
                {
                    data.CreatedBy = "system";
                    data.UpdatedBy = "system";
                }
                data.CreatedTime = DateTime.UtcNow;
                data.UpdatedTime = DateTime.UtcNow;
                if (data.Status == null) data.Status = StatusNormal;
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
                    if (CurrUser != null)
                    {
                        if (updator.UpdatedBy != CurrUser?.Account)
                        {
                            updator.UpdatedBy = CurrUser?.Account;
                        }
                    }
                    else
                    {
                        updator.UpdatedBy = "system";
                    }
                    if (updator.Status == null) updator.Status = StatusNormal;
                    updator.UpdatedTime = DateTime.UtcNow;
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
        /// <typeparam name="TItem">数据类</typeparam>
        /// <typeparam name="TEntity">表类</typeparam>
        /// <param name="entity"></param>
        /// <param name="formatter"></param>
        /// <returns></returns>
        public long SaveLong<TItem, TEntity>(TItem entity, Func<TItem, TEntity> formatter)
                    where TEntity : class, ILongIDTable
        {
            var data = formatter(entity);
            var rep = Db.GetRepository<TEntity>();
            if (data.ID == 0L)
            {
                if (CurrUser != null)
                {
                    data.CreatedBy = CurrUser?.Account;
                    data.UpdatedBy = CurrUser?.Account;
                }
                else
                {
                    data.CreatedBy = "system";
                    data.UpdatedBy = "system";
                }
                data.CreatedTime = DateTime.UtcNow;
                data.UpdatedTime = DateTime.UtcNow;
                if (data.Status == null) data.Status = StatusNormal;
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
                    if (CurrUser != null)
                    {
                        if (updator.UpdatedBy != CurrUser?.Account)
                        {
                            updator.UpdatedBy = CurrUser?.Account;
                        }
                    }
                    else
                    {
                        updator.UpdatedBy = "system";
                    }
                    updator.UpdatedTime = DateTime.UtcNow;
                    if (updator.Status == null) updator.Status = StatusNormal;
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

    public class ServiceBase<TEntity>(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory, IHttpContextAccessor httpContextAccessor) 
            : ServiceBase(qlUowFactory, httpContextAccessor)
        where TEntity : class, IIDTable
    {
        /// <summary>
        /// 如果使用的是<object>则将dbSet封闭
        /// </summary>
        protected IGenericRepository<TEntity> dbSet => Db.GetRepository<TEntity>();
        public TEntity Get(int ID)
        {
            return dbSet.FindByID(ID);
        }
        public int Delete(int ID)
        {
            var updator = dbSet.FindByID(ID);
            if (updator == null)
            {
            }
            else
            {
                updator.Status = StatusDelete;
                if (CurrUser?.Account != null)
                {
                    updator.UpdatedBy = CurrUser?.Account;
                }
                updator.UpdatedTime = DateTime.UtcNow;
            }
            var count = this.Db.Save();
            if (count > 0)
            {
                return ID;
            }
            return 0;
        }

        /// <summary>
        /// 主键是INT的表公用保存方法（兼容新增和更新）
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="item">实体</param>
        /// <param name="formatter">实体和表数据转换方法</param>
        /// <returns></returns>
        public int Save<TItem>(TItem item, Func<TItem, TEntity> formatter)
        {
            return base.SaveInt(item, formatter);
        }

        /// <summary>
        /// 主键是INT的表公用保存方法（兼容新增和更新），将自动完成实体和表数据的转换
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="item">实体</param>
        /// <returns></returns>
        public int Save<TItem>(TItem item)
        {
            return base.SaveInt(item, (oitem) =>
            {
                var data = Activator.CreateInstance<TEntity>();
                ObjectUtils.CopyObjectValue(oitem, data, ObjectUtils.DefaultSkipList);
                return data;
            });
        }
    }

    public class ServiceBaseLongID<TEntity>(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory, IHttpContextAccessor httpContextAccessor)
            : ServiceBase(qlUowFactory, httpContextAccessor)
        where TEntity : class, ILongIDTable
    {
        /// <summary>
        /// 如果使用的是<object>则将dbSet封闭
        /// </summary>
        protected IGenericRepository<TEntity> dbSet => Db.GetRepository<TEntity>();
        public TEntity Get(long ID)
        {
            return dbSet.FindFirst(a => a.ID == ID);
        }
        public long Delete(long ID)
        {
            var updator = dbSet.FindFirst(a => a.ID == ID);
            if (updator == null)
            {
            }
            else
            {
                updator.Status = StatusDelete;
                if (CurrUser != null)
                {
                    if (updator.UpdatedBy != CurrUser?.Account)
                    {
                        updator.UpdatedBy = CurrUser?.Account;
                    }
                }
                else
                {
                    updator.UpdatedBy = "system";
                }
                updator.UpdatedTime = DateTime.UtcNow;
            }
            var count = this.Db.Save();
            if (count > 0)
            {
                return ID;
            }
            return 0;
        }

        /// <summary>
        /// 主键是LONG的表公用保存方法（兼容新增和更新）
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="entity">实体</param>
        /// <param name="formatter">实体和表数据转换方法</param>
        /// <returns></returns>
        public long Save<TItem>(TItem entity, Func<TItem, TEntity> formatter)
        {
            return base.SaveLong(entity, formatter);
        }

        /// <summary>
        /// 主键是LONG的表公用保存方法（兼容新增和更新），将自动完成实体和表数据的转换
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="item">实体</param>
        /// <returns></returns>
        public long Save<TItem>(TItem item)
        {
            return base.SaveLong(item, (oitem) =>
            {
                var data = Activator.CreateInstance<TEntity>();
                ObjectUtils.CopyObjectValue(oitem, data);
                return data;
            });
        }
    }
}
