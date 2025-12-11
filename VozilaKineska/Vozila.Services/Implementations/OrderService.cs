using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Vozila.DataAccess.DataContext;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Enums;
using Vozila.Domain.Models;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.ModelsOrder;

namespace Vozila.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IDestinationRepository _destinationRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ITransporterRepository _transporterRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public OrderService(
            IOrderRepository orderRepository,
            IDestinationRepository destinationRepository,
            ICompanyRepository companyRepository,
            ITransporterRepository transporterRepository,
            ILogger<OrderService> logger,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext context)
        {
            _orderRepository = orderRepository;
            _destinationRepository = destinationRepository;
            _companyRepository = companyRepository;
            _transporterRepository = transporterRepository;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // ---------------------------------------------------------------------------------------
        // CREATE ORDER (Admin)
        // ---------------------------------------------------------------------------------------
        public async Task<int> CreateOrderAsync(CreateOrderVM model)
        {
            // Validate model
            if (model.DateForLoadingFrom >= model.DateForLoadingTo)
                throw new ArgumentException("Loading 'From' date must be before 'To' date");

            if (model.ContractOilPrice <= 0)
                throw new ArgumentException("Contract oil price must be greater than zero");

            // Validate related entities exist
            var companyExists = await _companyRepository.ExistsAsync(model.CompanyId);
            var transporterExists = await _transporterRepository.ExistsAsync(model.TransporterId);
            var destinationExists = await _destinationRepository.ExistsAsync(model.DestinationId);

            if (!companyExists || !transporterExists || !destinationExists)
                throw new ArgumentException("Invalid company, transporter, or destination");
            // Create order entity
            var entity = new Order
            {
                CompanyId = model.CompanyId,
                TransporterId = model.TransporterId,
                DestinationId = model.DestinationId,
                DateForLoadingFrom = model.DateForLoadingFrom,
                DateForLoadingTo = model.DateForLoadingTo,
                ContractOilPrice = model.ContractOilPrice,
                Status = OrderStatus.Pending,
                CreatedDate = DateTime.Now
            };
            // Save to repository
            var created = await _orderRepository.AddAsync(entity);
            // Send notification to transporter
            //await _notificationService.SendOrderCreatedNotificationAsync(
            //    transporterId: model.TransporterId,
            //    orderId: createdOrder.Id,
            //    companyName: (await _companyRepository.GetByIdAsync(model.CompanyId))?.Name ?? "Unknown Company",
            //    destinationName: (await _destinationRepository.GetByIdAsync(model.DestinationId))?.City ?? "Unknown Destination"
            //);
            return created.Id;
        }
        // ---------------------------------------------------------------------------------------
        // SUBMIT TRUCK (Transporter)
        // ---------------------------------------------------------------------------------------
        public async Task<bool> SubmitTruckAsync(int orderId, string truckPlateNo, int transporterId)
        {
            // Check if order belongs to transporter
            bool assigned = await _orderRepository.IsOrderAssignedToTransporterAsync(orderId, transporterId);
            if (!assigned) throw new InvalidOperationException("Cannot submit truck for this order");

            // Check validation of loading dates vs truck availability
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw new ArgumentException("Order not found");

            bool valid = await _orderRepository.ValidateTruckForOrderAsync(
                truckPlateNo,
                order.DateForLoadingFrom,
                order.DateForLoadingTo,
                transporterId);

            if (!valid) return false;

            // Commit submission
            return await _orderRepository.SubmitTruckForOrderAsync(orderId, truckPlateNo, transporterId);
            //if (valid)
            //   {
            //       // Send notification about truck submission
            //       await _notificationService.SendTruckSubmittedNotificationAsync(
            //           orderId: model.OrderId,
            //           transporterId: model.TransporterId,
            //           truckPlateNo: model.TruckPlateNo
            //       );
            //   }

            //   return result;
        }
        // ---------------------------------------------------------------------------------------
        // CANCEL ORDER (Admin or Transporter if allowed)
        // ---------------------------------------------------------------------------------------
        public async Task<bool> CancelOrderAsync(int orderId, string reason, int userId)
        {
            return await _orderRepository.CancelOrderAsync(orderId, reason, userId);
        }
        // ---------------------------------------------------------------------------------------
        // FINISH ORDER (Admin)
        // ---------------------------------------------------------------------------------------
        public async Task<bool> FinishOrderAsync(int orderId, int adminUserId)
        {
            return await _orderRepository.MarkOrderAsFinishedAsync(orderId, adminUserId);
        }
        // ---------------------------------------------------------------------------------------
        // AUTO EXPIRE
        // ---------------------------------------------------------------------------------------
        public async Task<int> AutoCancelExpiredOrderAsync(int daysExpired = 3)
        {
            return await _orderRepository.AutoCancelExpiredOrdersAsync(daysExpired);
        }
        // ---------------------------------------------------------------------------------------
        // STATS
        // ---------------------------------------------------------------------------------------
        public async Task<TransporterOrderStats> GetTransporterStatsAsync(int transporterId)
        {
            return await _orderRepository.GetTransporterOrderStatsAsync(transporterId);
        }
        // ---------------------------------------------------------------------------------------
        // ORDER DETAILS VIEWMODEL
        // ---------------------------------------------------------------------------------------
        public async Task<OrderDetailsVM> GetOrderDetailsAsync(int orderId, int userId)
        {
            var entity = await _orderRepository.GetOrderWithTruckDetailsAsync(orderId);
            if (entity == null)
                throw new ArgumentException("Order not found");

            return new OrderDetailsVM
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                CompanyName = entity.Company.CustomerName,
                TransporterId = entity.TransporterId,
                TransporterName = entity.Transporter.CompanyName,
                DestinationId = entity.DestinationId,
                DestinationCity = entity.Destination.City.ToString(),
                DestinationCountry = entity.Destination.Country.ToString(),
                TruckPlateNo = entity.TruckPlateNo,
                DateForLoadingFrom = entity.DateForLoadingFrom,
                DateForLoadingTo = entity.DateForLoadingTo,
                ContractOilPrice = entity.ContractOilPrice,
                Status = entity.Status,
                StatusName = entity.Status.ToString(),
                CreatedDate = entity.CreatedDate,
                TruckSubmittedDate = entity.TruckSubmittedDate,
                FinishedDate = entity.FinishedDate,
                CancelledDate = entity.CancelledDate,
                CancelledReason = entity.CancelledReason,
                CancelledByUserName = entity.CancelledByUser?.FullName,
                DestinationPrice = entity.Destination.DestinationContractPrice,
                CanSubmitTruck = entity.TransporterId == entity.TransporterId && entity.Status == OrderStatus.Pending,
                CanCancel = entity.Status == OrderStatus.Pending || entity.Status == OrderStatus.Approved,
                CanFinish = entity.Status == OrderStatus.Approved
            };
        }
        // ---------------------------------------------------------------------------------------
        // LISTS
        // ---------------------------------------------------------------------------------------
        public async Task<IEnumerable<OrderListVM>> GetAllAsync()
        {
            var orders = await _orderRepository.GetAllAsync();

            return orders.Select(o => new OrderListVM
            {
                Id = o.Id,
                CompanyName = o.Company.CustomerName,
                TransporterName = o.Transporter.CompanyName,
                DestinationCity = o.Destination.City.ToString(),
                DateForLoadingFrom = o.DateForLoadingFrom,
                DateForLoadingTo = o.DateForLoadingTo,
                Status = o.Status,
                StatusName = o.Status.ToString(),
                TruckPlateNo = o.TruckPlateNo
            });
        }

        public async Task<IEnumerable<OrderListVM>> GetPendingForTransporterAsync(int transporterId)
        {
            var orders = await _orderRepository.GetPendingOrdersForTransporterAsync(transporterId);

            return orders.Select(o => new OrderListVM
            {
                Id = o.Id,
                CompanyName = o.Company.CustomerName,
                TransporterName = o.Transporter.CompanyName,
                DestinationCity = o.Destination.City.ToString(),
                DateForLoadingFrom = o.DateForLoadingFrom,
                DateForLoadingTo = o.DateForLoadingTo,
                Status = o.Status,
                TruckPlateNo = o.TruckPlateNo,
                StatusName = o.Status.ToString()
            });
        }

        public async Task<IEnumerable<OrderListVM>> GetAllOrdersForAdminAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(o => new OrderListVM
            {
                Id = o.Id,
                CompanyName = o.Company.CustomerName,
                TransporterName = o.Transporter.CompanyName,
                DestinationCity = o.Destination.City.ToString(),
                DateForLoadingFrom = o.DateForLoadingFrom,
                DateForLoadingTo = o.DateForLoadingTo,
                Status = o.Status,
                StatusName = o.Status.ToString(),
                TruckPlateNo = o.TruckPlateNo
            });
        }
        // ---------------------------------------------------------------------------------------
        // SEARCH
        // ---------------------------------------------------------------------------------------
        public async Task<IEnumerable<OrderListVM>> SearchAsync(int transporterId, OrderSearchCriteria criteria)
        {
            var orders = await _orderRepository.SearchOrdersForTransporterAsync(transporterId, criteria);

            return orders.Select(o => new OrderListVM
            {
                Id = o.Id,
                CompanyName = o.Company.CustomerName,
                TransporterName = o.Transporter.CompanyName,
                DestinationCity = o.Destination.City.ToString(),
                DateForLoadingFrom = o.DateForLoadingFrom,
                DateForLoadingTo = o.DateForLoadingTo,
                Status = o.Status,
                StatusName = o.Status.ToString(),
                TruckPlateNo = o.TruckPlateNo
            });
        }
        public async Task<bool> UpdateOrderAsync(EditOrderVM model)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // Find the existing order
                var order = await _context.Orders
                    .Include(o => o.Company)
                    .Include(o => o.Transporter)
                    .Include(o => o.Destination)
                    .FirstOrDefaultAsync(o => o.Id == model.Id);

                if (order == null)
                    return false;

                // Check if order can be modified based on its current status
                if (order.Status == OrderStatus.Finished || order.Status == OrderStatus.Cancelled)
                {
                    // Only allow updates to notes or truck plate for completed/cancelled orders
                    if (!string.IsNullOrEmpty(model.TruckPlateNo))
                        order.TruckPlateNo = model.TruckPlateNo;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }

                // Store original values for logging/audit if needed
                var originalCompanyId = order.CompanyId;
                var originalTransporterId = order.TransporterId;
                var originalDestinationId = order.DestinationId;
                var originalLoadingFrom = order.DateForLoadingFrom;
                var originalLoadingTo = order.DateForLoadingTo;
                var originalPrice = order.ContractOilPrice;

                // Validate the new data
                if (!await ValidateOrderUpdateAsync(model, order.Id))
                    return false;

                // Update order properties
                order.CompanyId = model.CompanyId;
                order.TransporterId = model.TransporterId;
                order.DestinationId = model.DestinationId;
                order.DateForLoadingFrom = model.DateForLoadingFrom;
                order.DateForLoadingTo = model.DateForLoadingTo;
                order.ContractOilPrice = model.ContractOilPrice;
                order.Status = model.Status;

                if (!string.IsNullOrEmpty(model.TruckPlateNo))
                    order.TruckPlateNo = model.TruckPlateNo;

                // If status is changed to "InProgress" and truck is assigned, set truck submission date
                if (model.Status == OrderStatus.Approved &&
                    !string.IsNullOrEmpty(model.TruckPlateNo) &&
                    string.IsNullOrEmpty(order.TruckPlateNo))
                {
                    order.TruckSubmittedDate = DateTime.Now;
                }

                // If status is changed to "Completed", set finished date
                if (model.Status == OrderStatus.Finished && order.FinishedDate == null)
                {
                    order.FinishedDate = DateTime.Now;
                }

                // If status is changed to "Cancelled", handle cancellation
                if (model.Status == OrderStatus.Cancelled && order.CancelledDate == null)
                {
                    order.CancelledDate = DateTime.Now;
                    // Note: Cancellation reason should be handled separately
                }

                // Update modified timestamp
                order.TruckSubmittedDate = DateTime.Now;

                // Save changes
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Log the update for audit trail
                await LogOrderUpdateAsync(order, originalCompanyId, originalTransporterId,
                                         originalDestinationId, originalLoadingFrom,
                                         originalLoadingTo, originalPrice);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", model.Id);
                return false;
            }
        }

        private async Task<bool> ValidateOrderUpdateAsync(EditOrderVM model, int orderId)
        {
            // Validate loading dates
            if (model.DateForLoadingFrom >= model.DateForLoadingTo)
            {
                _logger.LogWarning("Invalid loading dates for order {OrderId}", orderId);
                return false;
            }

            // Validate company exists
            var companyExists = await _context.Companies.AnyAsync(c => c.Id == model.CompanyId);
            if (!companyExists)
            {
                _logger.LogWarning("Company {CompanyId} not found for order {OrderId}", model.CompanyId, orderId);
                return false;
            }

            // Validate transporter exists and has active contract
            var transporterExists = await _context.Transporters
                .AnyAsync(t => t.Id == model.TransporterId &&
                              t.Contracts.Any(c => c.ValidUntil <= DateTime.Now));

            if (!transporterExists)
            {
                _logger.LogWarning("Transporter {TransporterId} not found or has no active contract for order {OrderId}",
                    model.TransporterId, orderId);
                return false;
            }

            // Validate destination exists
            var destinationExists = await _context.Destinations.AnyAsync(d => d.Id == model.DestinationId);
            if (!destinationExists)
            {
                _logger.LogWarning("Destination {DestinationId} not found for order {OrderId}",
                    model.DestinationId, orderId);
                return false;
            }

            // Validate contract oil price is reasonable
            if (model.ContractOilPrice <= 0 || model.ContractOilPrice > 10000)
            {
                _logger.LogWarning("Invalid contract oil price {Price} for order {OrderId}",
                    model.ContractOilPrice, orderId);
                return false;
            }

            // Check for overlapping orders for the same transporter
            var hasOverlap = await _context.Orders
                .Where(o => o.Id != orderId)
                .Where(o => o.TransporterId == model.TransporterId)
                .Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Finished)
                .AnyAsync(o =>
                    (model.DateForLoadingFrom >= o.DateForLoadingFrom && model.DateForLoadingFrom <= o.DateForLoadingTo) ||
                    (model.DateForLoadingTo >= o.DateForLoadingFrom && model.DateForLoadingTo <= o.DateForLoadingTo) ||
                    (model.DateForLoadingFrom <= o.DateForLoadingFrom && model.DateForLoadingTo >= o.DateForLoadingTo));

            if (hasOverlap)
            {
                _logger.LogWarning("Transporter {TransporterId} has overlapping orders for order {OrderId}",
                    model.TransporterId, orderId);
                return false;
            }

            return true;
        }

        private async Task LogOrderUpdateAsync(Order order, int originalCompanyId, int originalTransporterId,
                                             int originalDestinationId, DateTime originalLoadingFrom,
                                             DateTime originalLoadingTo, decimal originalPrice)
        {
            try
            {
                // Get current user ID from HttpContext
                var userIdClaim = _httpContextAccessor.HttpContext?.User
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int changedByUserId))
                {
                    changedByUserId = 0; // System or unknown user
                }
                var auditLog = new OrderAuditLog
                {
                    OrderId = order.Id,
                    Action = "Update",
                    ChangedByUserId = changedByUserId,
                    ChangedDate = DateTime.Now,
                    OldValues = JsonSerializer.Serialize(new
                    {
                        CompanyId = originalCompanyId,
                        TransporterId = originalTransporterId,
                        DestinationId = originalDestinationId,
                        DateForLoadingFrom = originalLoadingFrom,
                        DateForLoadingTo = originalLoadingTo,
                        ContractOilPrice = originalPrice,
                        Status = order.Status.ToString()
                    }),
                    NewValues = JsonSerializer.Serialize(new
                    {
                        order.CompanyId,
                        order.TransporterId,
                        order.DestinationId,
                        order.DateForLoadingFrom,
                        order.DateForLoadingTo,
                        order.ContractOilPrice,
                        Status = order.Status.ToString(),
                        order.TruckPlateNo,
                        order.TruckSubmittedDate,
                        order.FinishedDate,
                        order.CancelledDate
                    })
                };

                await _context.Set<OrderAuditLog>().AddAsync(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging order update for order {OrderId}", order.Id);
                // Don't fail the main operation if audit logging fails
            }
        }

    }
}
