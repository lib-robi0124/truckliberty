using Vozila.DataAccess.DataContext;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Enums;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Implementations
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context) { }

        public Task<Order?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Order>> GetOrdersByCompanyAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Order>> GetOrdersByTransporterAsync(int transporterId)
        {
            throw new NotImplementedException();
        }

        public Task<Order> GetOrderWithDetailsAsync(int id)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<Order> GetPendingOrdersAsync()
        {
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            throw new NotImplementedException();
        }

        Task IOrderRepository.AddAsync(Order order)
        {
            return AddAsync(order);
        }
    }
}
