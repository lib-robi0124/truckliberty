using Vozila.Domain.Enums;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        // Status-specific queries
        Task<IEnumerable<Order>> GetPendingOrdersForTransporterAsync(int transporterId);
        Task<IEnumerable<Order>> GetApprovedOrdersForTransporterAsync(int transporterId);
        Task<IEnumerable<Order>> GetFinishedOrdersForTransporterAsync(int transporterId);

        // Truck submission workflow
        Task<bool> SubmitTruckForOrderAsync(int orderId, string truckPlateNo, int transporterId);
        Task<bool> CanSubmitTruckForOrderAsync(int orderId, int transporterId);

        // Admin workflow
        Task<bool> MarkOrderAsFinishedAsync(int orderId, int adminUserId);
        Task<bool> CancelOrderAsync(int orderId, string reason, int cancelledByUserId);

        // Auto-expiry
        Task<IEnumerable<Order>> GetExpiredPendingOrdersAsync();
        Task<int> AutoCancelExpiredOrdersAsync(int daysExpired = 3);

        // Transporter-specific queries
        Task<IEnumerable<Order>> GetOrdersAssignedToTransporterAsync(int transporterId);
        Task<bool> IsOrderAssignedToTransporterAsync(int orderId, int transporterId);

        // Business queries
        Task<IEnumerable<Order>> GetOrdersRequiringTruckSubmissionAsync(int transporterId);
        Task<Order?> GetOrderWithTruckDetailsAsync(int orderId);
        Task<IEnumerable<Order>> SearchOrdersForTransporterAsync(int transporterId, OrderSearchCriteria criteria);

        // Statistics
        Task<TransporterOrderStats> GetTransporterOrderStatsAsync(int transporterId);
        Task<int> GetPendingOrdersCountForTransporterAsync(int transporterId);

        // Validation
        Task<bool> ValidateTruckForOrderAsync(string truckPlateNo, DateTime loadingFrom, DateTime loadingTo, int transporterId);
    }
}
