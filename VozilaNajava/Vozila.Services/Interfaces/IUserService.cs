using Vozila.Domain.Models;
using Vozila.ViewModels.Models;

namespace Vozila.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserVM?> AuthenticateAsync(string fullName, string password);
        Task<UserVM> CreateUserAsync(User user, int performedByUserId);
        Task<UserVM> UpdateUserAsync(User user, int performedByUserId);
        Task<bool> DeleteUserAsync(int userId, int performedByUserId);
        Task<List<UserVM>> GetAllUsersAsync(int performedByUserId);
        Task<UserVM?> GetUserByIdAsync(int id);
        Task<bool> TransporterUpdateOrderAsync(int transporterUserId, int orderId, string truckPlateNo);
        Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task ResetPasswordAsync(int adminUserId, int targetUserId, string newPassword);
        // Admin CRUD operations
        Task<Contract> CreateContractAsync(Contract contract);
        Task UpdateContractAsync(Contract contract);
        Task<Destination> CreateDestinationAsync(Destination destination);
        Task UpdateDestinationAsync(Destination destination);
        Task<Condition> CreateConditionAsync(Condition condition);
        Task UpdateConditionAsync(Condition condition);
        Task<Order> CreateOrderAsync(Order order);
        Task UpdateOrderAsync(Order order);
        Task<Order?> GetOrderWithUserAsync(int orderId);
        Task UpdatePriceOilAsync(PriceOil priceOil);
    }

}
