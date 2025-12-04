using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Enums;
using Vozila.Domain.Models;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.Models;

namespace Vozila.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IDestinationRepository _destinationRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ITransporterRepository _transporterRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IDestinationRepository destinationRepository,
            ICompanyRepository companyRepository,
            ITransporterRepository transporterRepository)
        {
            _orderRepository = orderRepository;
            _destinationRepository = destinationRepository;
            _companyRepository = companyRepository;
            _transporterRepository = transporterRepository;
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
    }
}
