using Vozila.ViewModels.ModelsTransporter;

namespace Vozila.Services.Interfaces
{
    public interface ITansporterService
    {
        Task<TransporterVM?> LoginTransporterAsync(string email, string password);
        Task<IEnumerable<TransporterListVM>> GetAllTransportersAsync();
        Task<TransporterStatsVM> GetTransporterStatsAsync(int transporterId);
        Task UpdateTruckPlateNoAsync(int orderId, string truckPlateNo);

    }
}
