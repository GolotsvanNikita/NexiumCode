using Microsoft.Identity.Client;
using System.Linq.Expressions;

namespace NexiumCode.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetById(int id);
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate);
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(int id);
        Task SaveChanges();
    }
}
