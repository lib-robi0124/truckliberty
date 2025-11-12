namespace Vozila.DataAccess.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetActiveAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task Update(T entity);
        Task Remove(T entity);
    }
}
