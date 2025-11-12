using Microsoft.EntityFrameworkCore;
using Vozila.DataAccess.DataContext;
using Vozila.DataAccess.Interfaces;

namespace Vozila.DataAccess.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _entities;

        public Repository(AppDbContext context)
        {
            _context = context;
            _entities = context.Set<T>();
        }

        public virtual async Task<T> GetActiveAsync(int id)
            => await _entities.FindAsync(id);

        public virtual async Task<IEnumerable<T>> GetAllAsync()
            => await _entities.ToListAsync();

        public virtual async Task<T> AddAsync(T entity)
        {
            await _entities.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task Update(T entity)
        {
            _entities.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task Remove(T entity)
        {
            _entities.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
