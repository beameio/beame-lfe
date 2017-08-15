using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LFE.Domain.Core
{

    public interface IGetRepository<T> : IDisposable where T : class
    {

        T Get(Expression<Func<T, bool>> where);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(string includeProperties);
        IEnumerable<T> GetMany(Expression<Func<T, bool>> where);
        IEnumerable<T> Take(Expression<Func<T, bool>> where, int count);
        IEnumerable<T> GetMany(Expression<Func<T, bool>> where, string includeProperties);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetByIdAsync(Guid id);
        Task<T> GetByIdAsync(int id);
        Task<T> GetAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
        Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
        bool IsAny(Expression<Func<T, bool>> where);
        Task<bool> IsAnyAsync(Expression<Func<T, bool>> predicate);
        int Count(Expression<Func<T, bool>> where);
    }

    public interface IRepository<T> : IGetRepository<T> where T : class
    {
        /// <summary>
        /// Get the unit of work in this repository
        /// </summary>
        IUnitOfWork UnitOfWork { get; }
        T GetById(long Id);
        T GetById(Guid Id);
        T GetById(int Id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        void Delete(Expression<Func<T, bool>> where);
        void SaveChanges();
        T Create();

        void ReloadContext();

    }
   
}
