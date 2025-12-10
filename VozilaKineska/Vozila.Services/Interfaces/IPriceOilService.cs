using Vozila.ViewModels.Models;

namespace Vozila.Services.Interfaces
{
    public interface IPriceOilService
    {
        Task<PriceOilVM> GetCurrentOilPriceAsync();
        Task<PriceOilVM?> GetOilPriceByDateAsync(DateTime date);
        Task<PriceOilVM> UpdateDailyOilPriceAsync(decimal newPrice);
        Task<IEnumerable<PriceOilHistoryVM>> GetOilPriceHistoryAsync();
    }
}
