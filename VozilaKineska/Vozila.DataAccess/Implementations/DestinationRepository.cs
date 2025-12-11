using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using Vozila.DataAccess.DataContext;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Implementations
{
    public class DestinationRepository : Repository<Destination>, IDestinationRepository
    {
        public DestinationRepository(AppDbContext context) : base(context)
        {
        }

        // ========== CRUD Overrides ==========
        public override async Task<Destination?> GetByIdAsync(int id)
        {
            return await _entities
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Transporter)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
        public override async Task<Destination?> GetActiveAsync(int id)
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Transporter)
                .Where(d => d.Id == id &&
                       d.Contract.ValidUntil > currentDate)
                .FirstOrDefaultAsync();
        }
        public override async Task<IEnumerable<Destination>> GetAllAsync()
        {
            return await _entities
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Transporter)
                .OrderBy(d => d.City)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Destination>> GetAllActiveAsync()
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Transporter)
                .Where(d => d.Contract.ValidUntil > currentDate)
                .OrderBy(d => d.City)
                .ToListAsync();
        }

        public override async Task<Destination> AddAsync(Destination entity)
        {
            // Validate that Contract exists
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == entity.ContractId);

            if (contract == null)
                throw new InvalidOperationException("Contract not found");

            // Check if destination already exists for this city in the same contract
            var existingDestination = await _entities
                .FirstOrDefaultAsync(d =>
                    d.City == entity.City &&
                    d.ContractId == entity.ContractId);

            if (existingDestination != null)
            {
                throw new InvalidOperationException(
                    $"Destination for city {entity.City} already exists in contract {entity.ContractId}");
            }

            // Set initial daily price from latest oil price
            var latestOilPrice = await GetLatestOilPriceAsync();
            entity.DailyPricePerLiter = latestOilPrice;

            return await base.AddAsync(entity);
        }

        public override async Task UpdateAsync(Destination entity)
        {
            var existing = await GetByIdAsync(entity.Id);
            if (existing == null)
                throw new NotFoundException($"Destination {entity.Id} not found");

            // Preserve immutable properties
            entity.DailyPricePerLiter = existing.DailyPricePerLiter;
            entity.ContractId = existing.ContractId;

            await base.UpdateAsync(entity);
        }

        // ========== PRICE CALCULATION METHODS ==========

        public async Task<decimal> GetCurrentDestinationPriceAsync(int destinationId)
        {
            var destination = await GetDestinationWithFullDetailsAsync(destinationId);

            if (destination == null)
                throw new NotFoundException($"Destination {destinationId} not found");

            if (destination.Contract == null)
                throw new InvalidOperationException($"Destination {destinationId} has no associated contract");

            var latestOilPrice = await GetLatestOilPriceAsync();
            destination.DailyPricePerLiter = latestOilPrice;

            return destination.DestinationPriceFromFormula;
        }

        public async Task<Dictionary<int, decimal>> CalculateAllDestinationPricesAsync(int contractId)
        {
            return await CalculateAllPricesForContractAsync(contractId);
        }

        public async Task<Dictionary<int, decimal>> CalculateAllPricesForContractAsync(int contractId)
        {
            var latestOilPrice = await GetLatestOilPriceAsync();

            var destinations = await _entities
                .Where(d => d.ContractId == contractId)
                .ToListAsync();

            var result = new Dictionary<int, decimal>();

            foreach (var destination in destinations)
            {
                destination.DailyPricePerLiter = latestOilPrice;
                result.Add(destination.Id, destination.DestinationPriceFromFormula);
            }

            return result;
        }

        public async Task<decimal> GetLatestOilPriceAsync()
        {
            return await _context.PriceOils
                .OrderByDescending(p => p.Date)
                .Select(p => p.DailyPricePerLiter)
                .FirstOrDefaultAsync();
        }

        // ========== PRICE UPDATE METHODS ==========

        public async Task UpdateAllDestinationOilPricesAsync(decimal newDailyPrice)
        {
            await CreatePriceOilRecordAsync(newDailyPrice);

            var destinations = await _entities.ToListAsync();
            foreach (var destination in destinations)
            {
                destination.DailyPricePerLiter = newDailyPrice;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateContractDestinationsOilPricesAsync(int contractId, decimal newDailyPrice)
        {
            await UpdateDailyPricesForContractAsync(contractId, newDailyPrice);
        }

        public async Task UpdateDailyPricesForContractAsync(int contractId, decimal newDailyPrice)
        {
            await CreatePriceOilRecordAsync(newDailyPrice);

            var destinations = await _entities
                .Where(d => d.ContractId == contractId)
                .ToListAsync();

            foreach (var destination in destinations)
            {
                destination.DailyPricePerLiter = newDailyPrice;
            }

            await _context.SaveChangesAsync();
        }

        private async Task CreatePriceOilRecordAsync(decimal price)
        {
            var priceOil = new PriceOil
            {
                Date = DateTime.Now,
                DailyPricePerLiter = price
            };
            _context.PriceOils.Add(priceOil);
        }

        // ========== QUERY METHODS ==========

        public async Task<Destination?> GetDestinationWithFullDetailsAsync(int destinationId)
        {
            return await GetWithFullDetailsAsync(destinationId);
        }

        public async Task<Destination?> GetWithFullDetailsAsync(int destinationId)
        {
            return await _entities
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Transporter)
                .Include(d => d.Orders)
                .FirstOrDefaultAsync(d => d.Id == destinationId);
        }

        public async Task<List<Destination>> GetDestinationsByContractIdAsync(int contractId)
        {
            return await GetByContractIdAsync(contractId).ContinueWith(t => t.Result.ToList());
        }

        public async Task<IEnumerable<Destination>> GetByContractIdAsync(int contractId)
        {
            return await _entities
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Transporter)
                .Where(d => d.ContractId == contractId)
                .OrderBy(d => d.City)
                .ToListAsync();
        }

        public async Task<IEnumerable<Destination>> GetByTransporterIdAsync(int transporterId)
        {
            return await _entities
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Transporter)
                .Where(d => d.Contract.TransporterId == transporterId)
                .OrderBy(d => d.City)
                .ToListAsync();
        }

        public async Task<IEnumerable<Destination>> GetByCityIdAsync(int cityId)
        {
            return await _entities
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Transporter)
                .Where(d => (int)d.City == cityId)
                .OrderByDescending(d => d.Contract.ValidUntil)
                .ToListAsync();
        }

        // ========== TRANSPORTER & VALIDATION METHODS ==========

        public async Task<Transporter?> FindTransporterForDestinationAsync(int cityId)
        {
            var currentDate = DateTime.Now;

            var destination = await _entities
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Transporter)
                .Where(d => (int)d.City == cityId &&
                       d.Contract.ValidUntil > currentDate)
                .OrderByDescending(d => d.Contract.ValidUntil)
                .FirstOrDefaultAsync();

            return destination?.Contract?.Transporter;
        }

        public async Task<bool> ValidateDestinationForOrderAsync(int destinationId)
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(d => d.Contract)
                .AnyAsync(d => d.Id == destinationId &&
                          d.Contract.ValidUntil > currentDate);
        }

        // ========== ADDITIONAL METHODS ==========

        public async Task<IEnumerable<Destination>> GetActiveDestinationsByContractAsync(int contractId)
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Transporter)
                .Where(d => d.ContractId == contractId &&
                       d.Contract.ValidUntil > currentDate)
                .OrderBy(d => d.City)
                .ToListAsync();
        }

        public async Task<IEnumerable<Destination>> GetDestinationsNeedingPriceUpdateAsync(DateTime sinceDate)
        {
            var latestOilPriceDate = await _context.PriceOils
                .OrderByDescending(p => p.Date)
                .Select(p => p.Date)
                .FirstOrDefaultAsync();

            // Get destinations where the daily price was last updated before the latest oil price update
            return await _entities
                .Include(d => d.Contract)
                .Where(d => d.DailyPricePerLiter == 0 ||
                       (latestOilPriceDate > sinceDate &&
                        EF.Property<DateTime>(d, "ModifiedDate") < latestOilPriceDate))
                .ToListAsync();
        }

        // ========== HELPER METHODS ==========

        public async Task<IEnumerable<Destination>> SearchDestinationsAsync(
            string searchTerm,
            int? contractId = null,
            int? transporterId = null,
            int? countryId = null)
        {
            var query = _entities
                .Include(d => d.Contract)
                    .ThenInclude(c => c.Transporter)
                .AsQueryable();

            if (contractId.HasValue)
            {
                query = query.Where(d => d.ContractId == contractId.Value);
            }

            if (transporterId.HasValue)
            {
                query = query.Where(d => d.Contract.TransporterId == transporterId.Value);
            }

            if (countryId.HasValue)
            {
                query = query.Where(d => (int)d.Country == countryId.Value);
            }

            var results = await query.OrderBy(d => d.City).ToListAsync();

            // Apply search term filter in memory if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                results = results.Where(d =>
                    d.City.ToString().ToLower().Contains(searchTerm) ||
                    d.Country.ToString().ToLower().Contains(searchTerm))
                    .ToList();
            }

            return results;
        }

        public async Task<decimal> GetAverageDestinationPriceByContractAsync(int contractId)
        {
            var latestOilPrice = await GetLatestOilPriceAsync();

            var destinations = await _entities
                .Where(d => d.ContractId == contractId)
                .ToListAsync();

            if (!destinations.Any())
                return 0;

            var totalPrice = 0m;
            foreach (var destination in destinations)
            {
                destination.DailyPricePerLiter = latestOilPrice;
                totalPrice += destination.DestinationPriceFromFormula;
            }

            return totalPrice / destinations.Count;
        }
    }
}
