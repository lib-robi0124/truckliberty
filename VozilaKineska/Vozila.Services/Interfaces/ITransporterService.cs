using Vozila.Domain.Models;
using Vozila.ViewModels.ModelsTransporter;

namespace Vozila.Services.Interfaces
{
    public interface ITransporterService
    {
        Task<TransporterVM?> LoginTransporterAsync(string email, string password);
        Task<IEnumerable<TransporterListVM>> GetAllTransportersAsync();
        Task<TransporterStatsVM> GetTransporterStatsAsync(int transporterId);
        Task UpdateTruckPlateNoAsync(int orderId, string truckPlateNo);
        Task<Transporter?> GetByEmailAsync(string email);
    }
}
