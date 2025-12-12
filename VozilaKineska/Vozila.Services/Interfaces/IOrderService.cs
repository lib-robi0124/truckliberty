using Vozila.Domain.Models;
using Vozila.ViewModels.ModelsOrder;

namespace Vozila.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDetailsVM> GetOrderDetailsAsync(int orderId, int userId);
        Task<IEnumerable<OrderListVM>> GetAllAsync();
        Task<IEnumerable<OrderListVM>> GetPendingForTransporterAsync(int transporterId);

        Task<int> CreateOrderAsync(CreateOrderVM model);
        Task<bool> UpdateOrderAsync(EditOrderVM model);
        Task<bool> SubmitTruckAsync(int orderId, string truckPlateNo, int transporterId);
        Task<bool> CancelOrderAsync(int orderId, string reason, int userId);
        Task<bool> FinishOrderAsync(int orderId, int adminUserId);

        Task<IEnumerable<OrderListVM>> SearchAsync(int transporterId, Domain.Models.OrderSearchCriteria criteria);
        Task<int> AutoCancelExpiredOrderAsync(int daysExpired = 3);

        Task<TransporterOrderStats> GetTransporterStatsAsync(int transporterId);

        // Admin list
        Task<IEnumerable<OrderListVM>> GetAllOrdersForAdminAsync();
    }
}
