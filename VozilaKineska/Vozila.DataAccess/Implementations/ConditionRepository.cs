using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using Vozila.DataAccess.DataContext;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Implementations
{
    public class ConditionRepository : Repository<Condition>, IConditionRepository
    {
        public ConditionRepository(AppDbContext context) : base(context)
        {
        }

        // ========== CRUD Overrides ==========

        public override async Task<Condition?> GetByIdAsync(int id)
        {
            return await _entities
                .Include(c => c.Contract)
                    .ThenInclude(contract => contract.Transporter)
                .Include(c => c.Destinations)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<Condition?> GetActiveAsync(int id)
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(c => c.Contract)
                    .ThenInclude(contract => contract.Transporter)
                .Include(c => c.Destinations)
                .Where(c => c.Id == id &&
                       c.Contract.ValidUntil > currentDate)
                .FirstOrDefaultAsync();
        }

        public override async Task<IEnumerable<Condition>> GetAllAsync()
        {
            return await _entities
                .Include(c => c.Contract)
                    .ThenInclude(contract => contract.Transporter)
                .Include(c => c.Destinations)
                .OrderBy(c => c.Contract.ContractNumber)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Condition>> GetAllActiveAsync()
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(c => c.Contract)
                    .ThenInclude(contract => contract.Transporter)
                .Include(c => c.Destinations)
                .Where(c => c.Contract.ValidUntil > currentDate)
                .OrderBy(c => c.Contract.ContractNumber)
                .ToListAsync();
        }

        public override async Task<Condition> AddAsync(Condition entity)
        {
            // Validate that Contract exists
            var contract = await _context.Contracts
                .Include(c => c.Transporter)
                .FirstOrDefaultAsync(c => c.Id == entity.ContractId);

            if (contract == null)
                throw new InvalidOperationException($"Contract {entity.ContractId} not found");

            // Check if Condition already exists for this Contract
            // (One Contract has One Condition)
            var existingCondition = await _entities
                .FirstOrDefaultAsync(c => c.ContractId == entity.ContractId);

            if (existingCondition != null)
            {
                throw new InvalidOperationException(
                    $"Contract {entity.ContractId} already has a condition (ID: {existingCondition.Id})");
            }

            return await base.AddAsync(entity);
        }

        public override async Task UpdateAsync(Condition entity)
        {
            var existing = await GetByIdAsync(entity.Id);
            if (existing == null)
                throw new NotFoundException($"Condition {entity.Id} not found");

            // Prevent changing the Contract association
            entity.ContractId = existing.ContractId;

            await base.UpdateAsync(entity);
        }

        public override async Task DeleteAsync(int id)
        {
            var condition = await GetByIdAsync(id);
            if (condition == null)
                throw new NotFoundException($"Condition {id} not found");

            // Check if condition has destinations
            if (condition.Destinations != null && condition.Destinations.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete condition {id} because it has {condition.Destinations.Count} destinations. Delete destinations first.");
            }

            await base.DeleteAsync(id);
        }

        // ========== CONDITION-SPECIFIC QUERIES ==========

        public async Task<Condition?> GetConditionWithContractAsync(int conditionId)
        {
            return await _entities
                .Include(c => c.Contract)
                    .ThenInclude(contract => contract.Transporter)
                .FirstOrDefaultAsync(c => c.Id == conditionId);
        }

        public async Task<Condition?> GetConditionWithDestinationsAsync(int conditionId)
        {
            return await _entities
                .Include(c => c.Destinations)
                    .ThenInclude(d => d.City)
                .Include(c => c.Destinations)
                    .ThenInclude(d => d.Country)
                .FirstOrDefaultAsync(c => c.Id == conditionId);
        }

        public async Task<Condition?> GetFullConditionDetailsAsync(int conditionId)
        {
            return await _entities
                .Include(c => c.Contract)
                    .ThenInclude(contract => contract.Transporter)
                .Include(c => c.Destinations)
                    .ThenInclude(d => d.City)
                .Include(c => c.Destinations)
                    .ThenInclude(d => d.Country)
                .Include(c => c.Destinations)
                    .ThenInclude(d => d.Orders)
                .FirstOrDefaultAsync(c => c.Id == conditionId);
        }

        // ========== BUSINESS LOGIC QUERIES ==========

        public async Task<Condition?> GetConditionForContractAsync(int contractId)
        {
            return await _entities
                .Include(c => c.Contract)
                .Include(c => c.Destinations)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);
        }

        public async Task<Condition?> GetBestConditionForCityAsync(int cityId)
        {
            var currentDate = DateTime.Now;

            // Get conditions that have this city as a destination,
            // ordered by contract expiration date (most recent first),
            // then by oil price (lowest first for better deal)
            return await _entities
                .Include(c => c.Contract)
                    .ThenInclude(contract => contract.Transporter)
                .Include(c => c.Destinations)
                    .ThenInclude(d => d.City)
                .Where(c => c.Contract.ValidUntil > currentDate &&
                       c.Destinations.Any(d => (int)d.City == cityId))
                .OrderByDescending(c => c.Contract.ValidUntil) // Most recent expiry first
                .ThenBy(c => c.ContractOilPrice) // Lowest oil price first
                .ThenByDescending(c => c.Contract.ValueEUR) // Highest contract value first
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> CalculateAverageContractOilPriceAsync(int transporterId)
        {
            var currentDate = DateTime.Now;

            var averagePrice = await _entities
                .Include(c => c.Contract)
                .Where(c => c.Contract.TransporterId == transporterId &&
                       c.Contract.ValidUntil > currentDate)
                .Select(c => c.ContractOilPrice)
                .AverageAsync();

            return averagePrice;
        }

        // ========== VALIDATION QUERIES ==========

        public async Task<bool> HasValidConditionForContractAsync(int contractId)
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(c => c.Contract)
                .AnyAsync(c => c.ContractId == contractId &&
                          c.Contract.ValidUntil > currentDate);
        }

        public async Task<bool> CanAddDestinationToConditionAsync(int conditionId, int cityId)
        {
            var condition = await GetConditionWithDestinationsAsync(conditionId);

            if (condition == null)
                return false;

            // Check if city already exists in this condition's destinations
            return !condition.Destinations.Any(d => (int)d.City == cityId);
        }

        // ========== BULK OPERATIONS ==========

        public async Task<IEnumerable<Condition>> GetConditionsByTransporterAsync(int transporterId)
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(c => c.Contract)
                    .ThenInclude(contract => contract.Transporter)
                .Include(c => c.Destinations)
                .Where(c => c.Contract.TransporterId == transporterId &&
                       c.Contract.ValidUntil > currentDate)
                .OrderByDescending(c => c.Contract.ValidUntil)
                .ToListAsync();
        }

        public async Task<IEnumerable<Condition>> GetActiveConditionsAsync()
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(c => c.Contract)
                    .ThenInclude(contract => contract.Transporter)
                .Include(c => c.Destinations)
                .Where(c => c.Contract.ValidUntil > currentDate)
                .OrderBy(c => c.Contract.ContractNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Condition>> GetConditionsExpiringWithinAsync(TimeSpan timeSpan)
        {
            var expirationDate = DateTime.Now.Add(timeSpan);

            return await _entities
                .Include(c => c.Contract)
                    .ThenInclude(contract => contract.Transporter)
                .Include(c => c.Destinations)
                .Where(c => c.Contract.ValidUntil <= expirationDate &&
                       c.Contract.ValidUntil > DateTime.Now)
                .OrderBy(c => c.Contract.ValidUntil)
                .ToListAsync();
        }

        // ========== HELPER METHODS ==========

        public async Task<decimal> GetTotalContractValueForTransporterAsync(int transporterId)
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(c => c.Contract)
                .Where(c => c.Contract.TransporterId == transporterId &&
                       c.Contract.ValidUntil > currentDate)
                .SumAsync(c => c.Contract.ValueEUR);
        }

        public async Task<int> GetDestinationCountForConditionAsync(int conditionId)
        {
            return await _entities
                .Where(c => c.Id == conditionId)
                .SelectMany(c => c.Destinations)
                .CountAsync();
        }

        public async Task<IEnumerable<Condition>> SearchConditionsAsync(
            string? searchTerm = null,
            int? transporterId = null,
            DateTime? validFrom = null,
            DateTime? validTo = null,
            decimal? minOilPrice = null,
            decimal? maxOilPrice = null)
        {
            var query = _entities   
                .Include(c => c.Contract)
                    .ThenInclude(contract => contract.Transporter)
                .Include(c => c.Destinations)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c =>
                    c.Contract.ContractNumber.ToLower().Contains(searchTerm) ||
                    c.Contract.Transporter.CompanyName.ToLower().Contains(searchTerm));
            }

            if (transporterId.HasValue)
            {
                query = query.Where(c => c.Contract.TransporterId == transporterId.Value);
            }

            if (validFrom.HasValue)
            {
                query = query.Where(c => c.Contract.CreatedDate >= validFrom.Value);
            }

            if (validTo.HasValue)
            {
                query = query.Where(c => c.Contract.ValidUntil <= validTo.Value);
            }

            if (minOilPrice.HasValue)
            {
                query = query.Where(c => c.ContractOilPrice >= minOilPrice.Value);
            }

            if (maxOilPrice.HasValue)
            {
                query = query.Where(c => c.ContractOilPrice <= maxOilPrice.Value);
            }

            return await query
                .OrderByDescending(c => c.Contract.ValidUntil)
                .ToListAsync();
        }
    }
}
