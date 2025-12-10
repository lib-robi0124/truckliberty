using Vozila.DataAccess.Implementations;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Enums;
using Vozila.Domain.Models;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.ModelsTransporter;

namespace Vozila.Services.Implementations
{
    public class TransporterService : ITansporterService
    {
        private readonly ITransporterRepository _transporterRepo;
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<Contract> _contractRepo;

        public TransporterService(
            ITransporterRepository transporterRepo,
            IRepository<Order> orderRepo,
            IRepository<Contract> contractRepo)
        {
            _transporterRepo = transporterRepo;
            _orderRepo = orderRepo;
            _contractRepo = contractRepo;
        }
        // -------------------------------------------------------
        // TRANSPORTER LOGIN
        // -------------------------------------------------------
        public async Task<TransporterVM?> LoginTransporterAsync(string email, string password)
        {
            var transporters = await _transporterRepo.GetAllAsync();
            var login = transporters.FirstOrDefault(t =>
                t.Email.ToLower() == email.ToLower() &&
                t.Password == password);

            if (login == null)
                return null;

            return new TransporterVM
            {
                Id = login.Id,
                CompanyName = login.CompanyName,
                ContactPerson = login.ContactPerson,
                PhoneNumber = login.PhoneNumber,
                Email = login.Email
            };
        }
        // -------------------------------------------------------
        // LIST TRANSPORTERS
        // -------------------------------------------------------
        public async Task<IEnumerable<TransporterListVM>> GetAllTransportersAsync()
        {
            var list = await _transporterRepo.GetAllWithDetailsAsync();

            return list.Select(t => new TransporterListVM
            {
                Id = t.Id,
                CompanyName = t.CompanyName,
                ContactPerson = t.ContactPerson,
                Email = t.Email,
                ContractCount = t.Contracts.Count,
                ActiveOrderCount = t.Orders.Count(o => o.Status == OrderStatus.Approved)
            });
        }
        // -------------------------------------------------------
        // TRANSPORTER STATS (with Conditions & Destinations)
        // -------------------------------------------------------
        public async Task<TransporterStatsVM> GetTransporterStatsAsync(int transporterId)
        {
            var transporter = await _transporterRepo.GetWithDetailsAsync(transporterId);
            if (transporter == null)
                throw new Exception($"Transporter with Id {transporterId} not found.");

            var totalContracts = transporter.Contracts.Count;
            var totalContractValue = transporter.Contracts.Sum(c => c.ValueEUR);  // assuming Value property
            var pendingOrders = transporter.Orders.Count(o => o.Status == OrderStatus.Pending); // enum assumed
            var approvedOrders = transporter.Orders.Count(o => o.Status == OrderStatus.Approved);
            var finishedOrders = transporter.Orders.Count(o => o.Status == OrderStatus.Finished);

            return new TransporterStatsVM
            {
                Id = transporter.Id,
                CompanyName = transporter.CompanyName,
                TotalContracts = totalContracts,
                TotalContractValue = totalContractValue,
                PendingOrders = pendingOrders,
                ApprovedOrders = approvedOrders,
                FinishedOrders = finishedOrders
            };
        }
        // -------------------------------------------------------
        // UPDATE SUBMIT TRUCK PLATE NO
        // -------------------------------------------------------
        public async Task UpdateTruckPlateNoAsync(int orderId, string truckPlateNo)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
                throw new Exception($"Order with Id {orderId} not found.");

            // Update the TruckPlateNo on the order
            order.TruckPlateNo = truckPlateNo;

            await _orderRepo.UpdateAsync(order);
        }
    }
}
