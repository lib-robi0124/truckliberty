using Vozila.Domain.Models;

namespace Vozila.DataAccess.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserWithRoleAsync(int userId);
        Task<User?> GetUserWithOrdersAsync(int userId);
        Task<List<User>> GetUsersByRoleAsync(int roleId);
        Task<List<User>> GetActiveUsersAsync();
        Task<bool> UserExistsAsync(string fullName);
        Task<bool> IsUserAdminAsync(int userId);
        Task<bool> IsUserTransporterAsync(int userId);
        Task<int> GetUserTransporterIdAsync(int userId);
        Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task ResetPasswordAsync(int userId, string newPassword);
        Task UpdateLastLoginAsync(int userId);
    }

}
