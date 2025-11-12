using Vozila.Domain.Enums;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetByIdAsync(int id);
        Task AddAsync(Order order);
        Task SaveChangesAsync();
        IAsyncEnumerable<Order> GetPendingOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<IEnumerable<Order>> GetOrdersByTransporterAsync(int transporterId);
        Task<IEnumerable<Order>> GetOrdersByCompanyAsync(int companyId);
        Task<Order> GetOrderWithDetailsAsync(int id);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    }
}
