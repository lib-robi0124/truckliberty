using Vozila.Domain.Models;

namespace Vozila.DataAccess.Interfaces
{
    public interface IPriceOilRepository : IRepository<PriceOil>
    {
        Task<PriceOil> GetCurrentOilPriceAsync();
        Task<PriceOil> GetOilPriceByDateAsync(DateTime date);
        Task<PriceOil> UpdateCurrentOilPricesAsync(decimal newPrice);
        Task<PriceOil?> GetLatestPriceAsync();
    }
}
