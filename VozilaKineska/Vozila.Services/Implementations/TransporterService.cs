using Microsoft.Extensions.Logging;
using Vozila.DataAccess.Implementations;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Enums;
using Vozila.Domain.Models;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.ModelsTransporter;

namespace Vozila.Services.Implementations
{
    public class TransporterService : ITransporterService
    {
        private readonly ITransporterRepository _transporterRepo;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<TransporterService> _logger;

        public TransporterService(
            ITransporterRepository transporterRepo,
            IOrderRepository orderRepository,
            ILogger<TransporterService> logger)
        {
            _transporterRepo = transporterRepo;
            _orderRepository = orderRepository;
            _logger = logger;
        }
        // -------------------------------------------------------
        // TRANSPORTER LOGIN
        // -------------------------------------------------------
        public async Task<TransporterVM?> LoginTransporterAsync(string email, string password)
        {
            // Add a method to your repository like:
            // Task<Transporter?> GetByEmailAsync(string email)
            var transporter = await _transporterRepo.GetByEmailAsync(email);

            if (transporter == null)
                return null;

            if (!VerifyPassword(password, transporter.Password))
                return null;

            return new TransporterVM
            {
                Id = transporter.Id,
                CompanyName = transporter.CompanyName,
                ContactPerson = transporter.ContactPerson,
                PhoneNumber = transporter.PhoneNumber,
                Email = transporter.Email
            };
        }
        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            // Hash the input password and compare with stored hash
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(inputPassword);
            var hash = sha256.ComputeHash(bytes);
            var hashedInput = Convert.ToBase64String(hash);
            return hashedInput == storedHash;
        }
        // -------------------------------------------------------
        // LIST TRANSPORTERS
        // -------------------------------------------------------
        public async Task<IEnumerable<TransporterListVM>> GetAllTransportersAsync()
        {
            var transporters = await _transporterRepo.GetAllWithDetailsAsync();

            return transporters.Select(t => new TransporterListVM
            {
                Id = t.Id,
                CompanyName = t.CompanyName,
                ContactPerson = t.ContactPerson,
                Email = t.Email,
                DestinationCount = t.Destinations.Count,
                ActiveOrderCount = t.Orders.Count(o =>
                    o.Status == OrderStatus.Pending ||
                    o.Status == OrderStatus.Approved ||
                    o.Status == OrderStatus.Finished)
            });
        }
        // -------------------------------------------------------
        // TRANSPORTER STATS (with Conditions & Destinations)
        // -------------------------------------------------------
        public async Task<TransporterStatsVM> GetTransporterStatsAsync(int transporterId)
        {
            try
            {
                var transporter = await _transporterRepo.GetWithDetailsAsync(transporterId);

                if (transporter == null)
                {
                    throw new ArgumentException($"Transporter with ID {transporterId} not found");
                }

                var stats = new TransporterStatsVM
                {
                    Id = transporter.Id,
                    CompanyName = transporter.CompanyName,
                    TotalDestinations = transporter.Destinations.Count,
                    ActiveContracts = transporter.Contracts.Count(c => c.ValidUntil <= DateTime.Now),
                    PendingOrders = transporter.Orders.Count(o => o.Status == OrderStatus.Pending),
                    ApprovedOrders = transporter.Orders.Count(o => o.Status == OrderStatus.Approved),
                    FinishedOrders = transporter.Orders.Count(o => o.Status == OrderStatus.Finished)
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting stats for transporter {transporterId}");
                throw;
            }
        }
        // -------------------------------------------------------
        // UPDATE SUBMIT TRUCK PLATE NO
        // -------------------------------------------------------
        public async Task UpdateTruckPlateNoAsync(int orderId, string truckPlateNo)
        {
            try
            {
                await _transporterRepo.SubmitTruckPlateAsync(orderId, truckPlateNo);

                // Optionally update order status to InProgress
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order != null && order.Status == OrderStatus.Approved)
                {
                    order.Status = OrderStatus.Pending;
                    await _orderRepository.UpdateAsync(order);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating truck plate for order {orderId}");
                throw;
            }
        }

        public Task<Transporter?> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
