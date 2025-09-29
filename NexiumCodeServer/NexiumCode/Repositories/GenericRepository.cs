using Microsoft.EntityFrameworkCore;
using NexiumCode.Context;
using System.Linq.Expressions;

namespace NexiumCode.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        public GenericRepository(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<T> GetById(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAll() 
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate) 
        {
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task Add(T entity) 
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public async Task Delete(int id) 
        {
            var entity = await GetById(id);
            if (entity != null) 
            {
                _context.Set<T>().Remove(entity);
            }
        }

        public async Task SaveChanges() 
        {
            await _context.SaveChangesAsync();
        }
    }
}
