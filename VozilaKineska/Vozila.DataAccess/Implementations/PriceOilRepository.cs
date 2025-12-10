using Microsoft.EntityFrameworkCore;
using Vozila.DataAccess.DataContext;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Implementations
{
    public class PriceOilRepository : Repository<PriceOil>, IPriceOilRepository
    {
        private readonly AppDbContext _context;

        public PriceOilRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PriceOil> GetCurrentOilPriceAsync()
        {
            var today = DateTime.UtcNow.Date;

            // Try to get today's price
            var todayPrice = await _context.PriceOils
                .FirstOrDefaultAsync(p => p.Date == today);

            if (todayPrice != null)
                return todayPrice;

            // If today's record does not exist → return the most recent price
            var latest = await _context.PriceOils
                .OrderByDescending(p => p.Date)
                .FirstOrDefaultAsync();

            if (latest == null)
                throw new InvalidOperationException("No oil price records found.");

            return latest;
        }

        public async Task<PriceOil?> GetOilPriceByDateAsync(DateTime date)
        {
            date = date.Date;
            return await _context.PriceOils
                .FirstOrDefaultAsync(p => p.Date == date);
        }

        public async Task<PriceOil?> GetLatestPriceAsync()
        {
            return await _context.PriceOils
                .OrderByDescending(p => p.Date)
                .FirstOrDefaultAsync();
        }

        public async Task<PriceOil> UpdateCurrentOilPricesAsync(decimal newPrice)
        {
            var today = DateTime.UtcNow.Date;

            // Check if today's price already exists
            var existing = await _context.PriceOils
                .FirstOrDefaultAsync(p => p.Date == today);

            if (existing != null)
            {
                // Update today's existing record
                existing.DailyPricePerLiter = newPrice;

                _context.PriceOils.Update(existing);
                await _context.SaveChangesAsync();
                return existing;
            }

            // If not exists → create new daily record
            var newRecord = new PriceOil
            {
                Date = today,
                DailyPricePerLiter = newPrice
            };

            await _context.PriceOils.AddAsync(newRecord);
            await _context.SaveChangesAsync();

            return newRecord;
        }
    }
}
