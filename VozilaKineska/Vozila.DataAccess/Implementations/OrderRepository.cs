using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using Vozila.DataAccess.DataContext;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Enums;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Implementations
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context) {  }

        // ========== CRUD Overrides ==========

        public override async Task<Order?> GetByIdAsync(int id)
        {
            return await _entities
                .Include(o => o.Company)
                .Include(o => o.Transporter)
                .Include(o => o.Destination)
                .Include(o => o.CancelledByUser)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public override async Task<Order?> GetActiveAsync(int id)
        {
            // Active orders are those not cancelled
            return await _entities
                .Include(o => o.Company)
                .Include(o => o.Transporter)
                .Include(o => o.Destination)
                .Where(o => o.Id == id && o.Status != OrderStatus.Cancelled)
                .FirstOrDefaultAsync();
        }

        public override async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _entities
                .Include(o => o.Company)
                .Include(o => o.Transporter)
                .Include(o => o.Destination)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Order>> GetAllActiveAsync()
        {
            return await _entities
                .Include(o => o.Company)
                .Include(o => o.Transporter)
                .Include(o => o.Destination)
                .Where(o => o.Status != OrderStatus.Cancelled)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        public override async Task<Order> AddAsync(Order entity)
        {
            await ValidateOrderRelationshipsAsync(entity);

            // Set default values
            if (entity.CreatedDate == default)
                entity.CreatedDate = DateTime.Now;

            entity.Status = OrderStatus.Pending;

            return await base.AddAsync(entity);
        }

        public override async Task UpdateAsync(Order entity)
        {
            var existing = await GetByIdAsync(entity.Id);
            if (existing == null)
                throw new NotFoundException($"Order {entity.Id} not found");

            // Only allow certain status transitions
            if (entity.Status != existing.Status)
            {
                var isValidTransition = ValidateStatusTransition(existing.Status, entity.Status);
                if (!isValidTransition)
                    throw new InvalidOperationException($"Invalid status transition from {existing.Status} to {entity.Status}");
            }

            // Prevent changing certain properties once set
            if (existing.Status != OrderStatus.Pending)
            {
                entity.TransporterId = existing.TransporterId;
                entity.DestinationId = existing.DestinationId;
                entity.CompanyId = existing.CompanyId;
            }

            // Handle status-specific logic
            if (entity.Status == OrderStatus.Approved && existing.Status != OrderStatus.Approved)
            {
                entity.TruckSubmittedDate = DateTime.Now;
            }
            else if (entity.Status == OrderStatus.Finished && existing.Status != OrderStatus.Finished)
            {
                entity.FinishedDate = DateTime.Now;
            }
            else if (entity.Status == OrderStatus.Cancelled && existing.Status != OrderStatus.Cancelled)
            {
                entity.CancelledDate = DateTime.Now;
            }

            await base.UpdateAsync(entity);
        }

        public override async Task DeleteAsync(int id)
        {
            var order = await GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException($"Order {id} not found");

            // Only allow deletion of pending orders
            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException($"Cannot delete order {id} with status {order.Status}. Only pending orders can be deleted.");

            await base.DeleteAsync(id);
        }

        // ========== TRUCK SUBMISSION WORKFLOW ==========

        public async Task<bool> SubmitTruckForOrderAsync(int orderId, string truckPlateNo, int transporterId)
        {
            var order = await GetByIdAsync(orderId);
            if (order == null || order.TransporterId != transporterId)
                return false;

            // Validate order is in pending status
            if (order.Status != OrderStatus.Pending)
                return false;

            // Validate truck availability
            var isTruckAvailable = await ValidateTruckForOrderAsync(
                truckPlateNo,
                order.DateForLoadingFrom,
                order.DateForLoadingTo,
                transporterId);

            if (!isTruckAvailable)
                return false;

            // Update order
            order.TruckPlateNo = truckPlateNo;
            order.Status = OrderStatus.Approved;
            order.TruckSubmittedDate = DateTime.Now;

            await UpdateAsync(order);
            return true;
        }

        public async Task<bool> CanSubmitTruckForOrderAsync(int orderId, int transporterId)
        {
            var order = await GetByIdAsync(orderId);
            if (order == null || order.TransporterId != transporterId)
                return false;

            // Check if order is in pending status
            if (order.Status != OrderStatus.Pending)
                return false;

            // Check if loading dates are still valid (not in the past)
            if (order.DateForLoadingFrom < DateTime.Now)
                return false;

            return true;
        }

        public async Task<bool> ValidateTruckForOrderAsync(string truckPlateNo, DateTime loadingFrom, DateTime loadingTo, int transporterId)
        {
            // Check if truck is already assigned to another order in the same time period
            var conflictingOrder = await _entities
                .Where(o => o.TransporterId == transporterId &&
                       o.TruckPlateNo == truckPlateNo &&
                       o.Status != OrderStatus.Cancelled &&
                       o.Status != OrderStatus.Finished &&
                       ((o.DateForLoadingFrom <= loadingTo && o.DateForLoadingTo >= loadingFrom)))
                .FirstOrDefaultAsync();

            return conflictingOrder == null;
        }

        // ========== ADMIN WORKFLOW ==========

        public async Task<bool> MarkOrderAsFinishedAsync(int orderId, int adminUserId)
        {
            var order = await GetByIdAsync(orderId);
            if (order == null)
                return false;

            // Only approved orders can be marked as finished
            if (order.Status != OrderStatus.Approved)
                return false;

            order.Status = OrderStatus.Finished;
            order.FinishedDate = DateTime.Now;

            await UpdateAsync(order);
            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId, string reason, int cancelledByUserId)
        {
            var order = await GetByIdAsync(orderId);
            if (order == null)
                return false;

            // Can only cancel pending or approved orders
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Approved)
                return false;

            order.Status = OrderStatus.Cancelled;
            order.CancelledDate = DateTime.Now;
            order.CancelledReason = reason;
            order.CancelledByUserId = cancelledByUserId;

            await UpdateAsync(order);
            return true;
        }

        // ========== AUTO-EXPIRY ==========

        public async Task<IEnumerable<Order>> GetExpiredPendingOrdersAsync()
        {
            var expiryThreshold = DateTime.Now.AddDays(-3); // Orders older than 3 days

            return await _entities
                .Include(o => o.Transporter)
                .Where(o => o.Status == OrderStatus.Pending &&
                       o.CreatedDate < expiryThreshold)
                .ToListAsync();
        }

        public async Task<int> AutoCancelExpiredOrdersAsync(int daysExpired = 3)
        {
            var expiryThreshold = DateTime.Now.AddDays(-daysExpired);

            var expiredOrders = await _entities
                .Where(o => o.Status == OrderStatus.Pending &&
                       o.CreatedDate < expiryThreshold)
                .ToListAsync();

            foreach (var order in expiredOrders)
            {
                order.Status = OrderStatus.Cancelled;
                order.CancelledDate = DateTime.Now;
                order.CancelledReason = "Auto-cancelled: Order expired without truck submission";
            }

            await _context.SaveChangesAsync();
            return expiredOrders.Count;
        }

        // ========== TRANSPORTER-SPECIFIC QUERIES ==========

        public async Task<IEnumerable<Order>> GetPendingOrdersForTransporterAsync(int transporterId)
        {
            return await _entities
                .Include(o => o.Company)
                .Include(o => o.Destination)
                .Where(o => o.TransporterId == transporterId &&
                       o.Status == OrderStatus.Pending)
                .OrderBy(o => o.DateForLoadingFrom)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetApprovedOrdersForTransporterAsync(int transporterId)
        {
            return await _entities
                .Include(o => o.Company)
                .Include(o => o.Destination)
                .Where(o => o.TransporterId == transporterId &&
                       o.Status == OrderStatus.Approved)
                .OrderBy(o => o.TruckSubmittedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetFinishedOrdersForTransporterAsync(int transporterId)
        {
            return await _entities
                .Include(o => o.Company)
                .Include(o => o.Destination)
                .Where(o => o.TransporterId == transporterId &&
                       o.Status == OrderStatus.Finished)
                .OrderByDescending(o => o.FinishedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersAssignedToTransporterAsync(int transporterId)
        {
            return await _entities
                .Include(o => o.Company)
                .Include(o => o.Destination)
                .Where(o => o.TransporterId == transporterId &&
                       o.Status != OrderStatus.Cancelled)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> IsOrderAssignedToTransporterAsync(int orderId, int transporterId)
        {
            return await _entities
                .AnyAsync(o => o.Id == orderId && o.TransporterId == transporterId);
        }

        // ========== BUSINESS QUERIES ==========

        public async Task<IEnumerable<Order>> GetOrdersRequiringTruckSubmissionAsync(int transporterId)
        {
            return await _entities
                .Include(o => o.Company)
                .Include(o => o.Destination)
                .Where(o => o.TransporterId == transporterId &&
                       o.Status == OrderStatus.Pending &&
                       o.DateForLoadingFrom >= DateTime.Now)
                .OrderBy(o => o.DateForLoadingFrom)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderWithTruckDetailsAsync(int orderId)
        {
            return await _entities
                .Include(o => o.Company)
                .Include(o => o.Transporter)
                .Include(o => o.Destination)
                .Include(o => o.CancelledByUser)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        // ========== STATISTICS ==========

        public async Task<TransporterOrderStats> GetTransporterOrderStatsAsync(int transporterId)
        {
            var orders = await _entities
                .Where(o => o.TransporterId == transporterId)
                .ToListAsync();

            var transporter = await _context.Set<Transporter>()
                .FirstOrDefaultAsync(t => t.Id == transporterId);

            var stats = new TransporterOrderStats
            {
                TransporterId = transporterId,
                TransporterName = transporter?.CompanyName ?? string.Empty,
                PendingCount = orders.Count(o => o.Status == OrderStatus.Pending),
                ApprovedCount = orders.Count(o => o.Status == OrderStatus.Approved),
                FinishedCount = orders.Count(o => o.Status == OrderStatus.Finished),
                CancelledCount = orders.Count(o => o.Status == OrderStatus.Cancelled),
                TotalOrders = orders.Count
            };

            if (stats.TotalOrders > 0)
            {
                var completedOrders = stats.FinishedCount + stats.CancelledCount;
                stats.CompletionRate = (decimal)completedOrders / stats.TotalOrders * 100;
            }

            return stats;
        }

        public async Task<int> GetPendingOrdersCountForTransporterAsync(int transporterId)
        {
            return await _entities
                .CountAsync(o => o.TransporterId == transporterId &&
                            o.Status == OrderStatus.Pending);
        }

        // ========== HELPER METHODS ==========

        private async Task ValidateOrderRelationshipsAsync(Order order)
        {
            // Validate Company exists
            var companyExists = await _context.Set<Company>()
                .AnyAsync(c => c.Id == order.CompanyId);
            if (!companyExists)
                throw new InvalidOperationException($"Company {order.CompanyId} not found");

            // Validate Transporter exists
            var transporterExists = await _context.Set<Transporter>()
                .AnyAsync(t => t.Id == order.TransporterId);
            if (!transporterExists)
                throw new InvalidOperationException($"Transporter {order.TransporterId} not found");

            // Validate Destination exists
            var destinationExists = await _context.Set<Destination>()
                .AnyAsync(d => d.Id == order.DestinationId);
            if (!destinationExists)
                throw new InvalidOperationException($"Destination {order.DestinationId} not found");

            // Validate loading dates
            if (order.DateForLoadingFrom >= order.DateForLoadingTo)
                throw new InvalidOperationException("Loading 'From' date must be before 'To' date");

            if (order.DateForLoadingFrom < DateTime.Now.Date)
                throw new InvalidOperationException("Loading date cannot be in the past");
        }

        private bool ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            // Define allowed status transitions
            var allowedTransitions = new Dictionary<OrderStatus, OrderStatus[]>
            {
                [OrderStatus.Pending] = new[] { OrderStatus.Approved, OrderStatus.Cancelled },
                [OrderStatus.Approved] = new[] { OrderStatus.Finished, OrderStatus.Cancelled },
                [OrderStatus.Finished] = Array.Empty<OrderStatus>(), // Can't change from Finished
                [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()  // Can't change from Cancelled
            };

            return allowedTransitions.ContainsKey(currentStatus) &&
                   allowedTransitions[currentStatus].Contains(newStatus);
        }

        // ========== ADDITIONAL QUERIES ==========

        public async Task<IEnumerable<Order>> SearchOrdersForTransporterAsync(int transporterId, OrderSearchCriteria criteria)
        {
            var query = _entities
                .Include(o => o.Company)
                .Include(o => o.Transporter)
                .Include(o => o.Destination)
                .Where(o => o.TransporterId == transporterId);

            if (criteria.Status.HasValue)
                query = query.Where(o => o.Status == criteria.Status.Value);

            if (criteria.FromDate.HasValue)
                query = query.Where(o => o.CreatedDate >= criteria.FromDate.Value);

            if (criteria.ToDate.HasValue)
                query = query.Where(o => o.CreatedDate <= criteria.ToDate.Value);

            if (!string.IsNullOrWhiteSpace(criteria.TruckPlateNo))
                query = query.Where(o => o.TruckPlateNo != null && o.TruckPlateNo.Contains(criteria.TruckPlateNo));

            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                criteria.SearchTerm = criteria.SearchTerm.ToLower();
                query = query.Where(o =>
                    (o.TruckPlateNo != null && o.TruckPlateNo.ToLower().Contains(criteria.SearchTerm)) ||
                    o.Company.CustomerName.ToLower().Contains(criteria.SearchTerm) ||
                    o.Destination.City.ToString().ToLower().Contains(criteria.SearchTerm));
            }

            return await query
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }
    }
}
