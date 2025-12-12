using Vozila.Domain.Models;

namespace Vozila.DataAccess.Interfaces
{
    public interface ITransporterRepository : IRepository<Transporter>
    {
        Task<Transporter?> GetWithDetailsAsync(int id);
        Task<IEnumerable<Transporter>> GetAllWithDetailsAsync();
        Task SubmitTruckPlateAsync(int orderId, string truckPlateNo);
        Task<IEnumerable<Contract>> GetActiveContractsAsync(int transporterId);
        Task<Transporter?> GetByEmailAsync(string email);
    }
}
