using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LFE.Model;

namespace LFE.Domain.Core.Data
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly IUnitOfWork _unitOfWork;
        private lfeAuthorEntities _dataContext;

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _unitOfWork;
            }
        }

        #region .ctor
        public Repository(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
                throw new ArgumentNullException(nameof(unitOfWork));

            _unitOfWork = unitOfWork;

            _dataContext = new lfeAuthorEntities();
            _dataContext.Configuration.LazyLoadingEnabled = false;

            Dbset = _dataContext.Set<TEntity>();
        }
        #endregion

        public DbSet<TEntity> Dbset { get; private set; }

        protected lfeAuthorEntities DataContext => _dataContext ?? new lfeAuthorEntities();

        public void ReloadContext()
        {
            _dataContext = new lfeAuthorEntities();
        }
        private IDbSet<TEntity> GetSet()
        {
            ValidateDataContext();
            return _unitOfWork.CreateSet<TEntity>();
        }

        private void ValidateDataContext()
        {
            if (_dataContext == null) _dataContext = new lfeAuthorEntities();
        }
        //public Repository(lfeAuthorEntities context)
        //{
        //    DataContext = context;
        //    Dbset = context.Set<TEntity>();
        //}

        public virtual TEntity GetById(long id)
        {
            return GetSet().Find(id);
        }

        public virtual TEntity GetById(int id)
        {
            var set = GetSet();

            return set.Find(id);
        }

        public TEntity GetById(Guid Id)
        {
            return GetSet().Find(Id);
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            return GetSet().ToList();
        }

        public virtual IEnumerable<TEntity> GetAll(string includeProperties)
        {
            var query = GetSet().AsQueryable();

            //query = includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

            foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return query.ToList();
        }

        public virtual IEnumerable<TEntity> GetMany(Expression<Func<TEntity, bool>> where)
        {
            return GetSet().Where(where).ToList();
        }

        public virtual IEnumerable<TEntity> Take(Expression<Func<TEntity, bool>> where, int count)
        {
            return GetSet().Where(where).Take(count).ToList();
        }

        public bool IsAny(Expression<Func<TEntity, bool>> where)
        {
            return GetSet().Any(where);
        }

        public int Count(Expression<Func<TEntity, bool>> where)
        {
            return GetSet().Count(where);
        }

        public TEntity Get(Expression<Func<TEntity, bool>> where)
        {
            return GetSet().Where(where).SingleOrDefault();
        }

        #region async
        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dataContext.Set<TEntity>().CountAsync(predicate);
        }

        public async Task<TEntity> GetByIdAsync(Guid id)
        {
            return await _dataContext.Set<TEntity>().FindAsync(id);
        }
        public async Task<TEntity> GetByIdAsync(int id)
        {
            return await _dataContext.Set<TEntity>().FindAsync(id);
        }
        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var query = PrepareIncludes(includeProperties);
            return await query.SingleOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var query = PrepareIncludes(includeProperties);
            return await query.Where(predicate).ToListAsync();
        }

        public async Task<bool> IsAnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dataContext.Set<TEntity>().CountAsync(predicate) > 0;
        }

        #endregion

        public IEnumerable<TEntity> GetMany(Expression<Func<TEntity, bool>> where, string includeProperties)
        {
            var query = GetSet().Where(where);

            query = includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

            //foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            //{
            //    query = query.Include(includeProperty);
            //}

            return query.ToList();
        }
     
        private IQueryable<TEntity> PrepareIncludes(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = _dataContext.Set<TEntity>();
            return includeProperties == null ? query :
                includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        public virtual void Add(TEntity entity)
        {
            GetSet().Add(entity);
        }

        public virtual void Update(TEntity entity)
        {
            if (entity != null)
                _unitOfWork.SetModified(entity);
            //DataContext.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete(TEntity entity)
        {
            GetSet().Remove(entity);
        }

        public virtual void SaveChanges()
        {
            DataContext.SaveChanges();
        }

        public virtual void Delete(Expression<Func<TEntity, bool>> where)
        {
            var objects = GetSet().Where(where).AsEnumerable();
            foreach (var obj in objects)
                GetSet().Remove(obj);
        }

        public TEntity Create()
        {
            return GetSet().Create();
        }



        public void Dispose()
        {
            _unitOfWork?.Dispose();
            if (_dataContext != null)
            {
                _dataContext.Dispose();
                _dataContext = null;
            }
            // GC.SuppressFinalize(this);
        }


    }
}
