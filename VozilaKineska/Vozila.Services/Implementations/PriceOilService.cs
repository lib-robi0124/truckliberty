using Vozila.DataAccess.Implementations;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Models;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.Models;

namespace Vozila.Services.Implementations
{
    public class PriceOilService : IPriceOilService
    {
        private readonly IPriceOilRepository _priceOilService;
        public PriceOilService(IPriceOilRepository priceOilService)
        {
            _priceOilService = priceOilService;
        }
        // ---------------------------------------------------------
        // 1. GET CURRENT OIL PRICE
        // ---------------------------------------------------------
        public async Task<PriceOilVM> GetCurrentOilPriceAsync()
        {
            var entity = await _priceOilService.GetLatestPriceAsync()
                     ?? throw new Exception("No oil price found in database.");
            return MapToVM(entity);
        }
        // ---------------------------------------------------------
        // 2. GET OIL PRICE BY DATE
        // ---------------------------------------------------------
        public async Task<PriceOilVM?> GetOilPriceByDateAsync(DateTime date)
        {
            var entity = await _priceOilService.GetOilPriceByDateAsync(date);
            return entity == null ? null : MapToVM(entity);
        }
        // ---------------------------------------------------------
        // 4. FULL PRICE HISTORY WITH PRICE CHANGE & % CHANGE
        // ---------------------------------------------------------
        public async Task<IEnumerable<PriceOilHistoryVM>> GetOilPriceHistoryAsync()
        {
            var allPrices = (await _priceOilService.GetAllAsync())
                .OrderBy(x => x.Date)
                .ToList();

            var history = new List<PriceOilHistoryVM>();

            PriceOil? previous = null;

            foreach (var item in allPrices)
            {
                decimal? change = null;
                decimal? percent = null;

                if (previous != null)
                {
                    change = item.DailyPricePerLiter - previous.DailyPricePerLiter;
                    percent = previous.DailyPricePerLiter == 0
                        ? null
                        : (change / previous.DailyPricePerLiter) * 100;
                }

                history.Add(new PriceOilHistoryVM
                {
                    Date = item.Date,
                    DailyPricePerLiter = item.DailyPricePerLiter,
                    PriceChange = change,
                    PercentageChange = percent
                });

                previous = item;
            }

            return history;
        }
        // ---------------------------------------------------------
        // 3. UPDATE DAILY PRICE (market price update)
        // ---------------------------------------------------------
        public async Task<PriceOilVM> UpdateDailyOilPriceAsync(decimal newPrice)
        {
            if (newPrice <= 0)
                throw new ArgumentException("Daily oil price must be greater than 0.");

            // Create daily record or update existing one
            var updated = await _priceOilService.UpdateCurrentOilPricesAsync(newPrice);

            return MapToVM(updated);
        }
        // ---------------------------------------------------------
        // MAPPING HELPERS
        // ---------------------------------------------------------
        private static PriceOilVM MapToVM(PriceOil entity)
            => new PriceOilVM
            {
                Id = entity.Id,
                Date = entity.Date,
                DailyPricePerLiter = entity.DailyPricePerLiter
            };
    }
}
