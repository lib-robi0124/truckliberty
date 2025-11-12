using Vozila.Domain.Models;

namespace Vozila.DataAccess.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> AuthenticateAsync(string fullname, string password);
        Task<List<User>> GetAllUsersAsync();
    }
}
