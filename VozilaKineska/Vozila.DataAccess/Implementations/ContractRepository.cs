using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using Vozila.DataAccess.DataContext;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Implementations
{
    public class ContractRepository : Repository<Contract>, IContractRepository
    {
        public ContractRepository(AppDbContext context) : base(context)
        {
        }

        // ========== CRUD Overrides ==========

        public override async Task<Contract?> GetByIdAsync(int id)
        {
            return await _entities
                .Include(c => c.Transporter)
                .Include(c => c.Conditions)
                    .ThenInclude(cond => cond.Destinations)
                        .ThenInclude(dest => dest.City)
                .Include(c => c.Conditions)
                    .ThenInclude(cond => cond.Destinations)
                        .ThenInclude(dest => dest.Country)
                .Include(c => c.Destinations)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<Contract?> GetActiveAsync(int id)
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(c => c.Transporter)
                .Include(c => c.Conditions)
                    .ThenInclude(cond => cond.Destinations)
                .Include(c => c.Destinations)
                .Where(c => c.Id == id && c.ValidUntil > currentDate)
                .FirstOrDefaultAsync();
        }

        public override async Task<IEnumerable<Contract>> GetAllAsync()
        {
            return await _entities
                .Include(c => c.Transporter)
                .Include(c => c.Conditions)
                    .ThenInclude(cond => cond.Destinations)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Contract>> GetAllActiveAsync()
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(c => c.Transporter)
                .Include(c => c.Conditions)
                    .ThenInclude(cond => cond.Destinations)
                .Where(c => c.ValidUntil > currentDate)
                .OrderByDescending(c => c.ValidUntil)
                .ToListAsync();
        }

        public override async Task<Contract> AddAsync(Contract entity)
        {
            // Validate that Transporter exists
            var transporterExists = await _context.Set<Transporter>()
                .AnyAsync(t => t.Id == entity.TransporterId);

            if (!transporterExists)
                throw new InvalidOperationException($"Transporter {entity.TransporterId} not found");

            // Ensure ContractNumber is unique
            var contractNumberExists = await _entities
                .AnyAsync(c => c.ContractNumber == entity.ContractNumber);

            if (contractNumberExists)
                throw new InvalidOperationException($"Contract number {entity.ContractNumber} already exists");

            // Set default dates if not provided
            if (entity.CreatedDate == default)
                entity.CreatedDate = DateTime.Now;

            if (entity.ValidUntil == default)
                entity.ValidUntil = DateTime.Now.AddYears(1);

            return await base.AddAsync(entity);
        }

        public override async Task UpdateAsync(Contract entity)
        {
            var existing = await GetByIdAsync(entity.Id);
            if (existing == null)
                throw new NotFoundException($"Contract {entity.Id} not found");

            // Prevent changing critical properties
            entity.CreatedDate = existing.CreatedDate;
            entity.TransporterId = existing.TransporterId;

            // Ensure ContractNumber uniqueness (if changed)
            if (entity.ContractNumber != existing.ContractNumber)
            {
                var contractNumberExists = await _entities
                    .AnyAsync(c => c.ContractNumber == entity.ContractNumber && c.Id != entity.Id);

                if (contractNumberExists)
                    throw new InvalidOperationException($"Contract number {entity.ContractNumber} already exists");
            }

            await base.UpdateAsync(entity);
        }

        public override async Task DeleteAsync(int id)
        {
            var contract = await GetByIdAsync(id);
            if (contract == null)
                throw new NotFoundException($"Contract {id} not found");

            // Check if contract has associated conditions
            if (contract.Conditions != null && contract.Conditions.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete contract {id} because it has {contract.Conditions.Count} conditions. Delete conditions first.");
            }

            // Check if contract has associated destinations
            if (contract.Destinations != null && contract.Destinations.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete contract {id} because it has {contract.Destinations.Count} destinations. Delete destinations first.");
            }

            await base.DeleteAsync(id);
        }

        // ========== CONTRACT-SPECIFIC QUERIES ==========

        public async Task<Contract?> GetContractWithConditionsAsync(int contractId)
        {
            return await _entities
                .Include(c => c.Transporter)
                .Include(c => c.Conditions)
                    .ThenInclude(cond => cond.Destinations)
                        .ThenInclude(dest => dest.City)
                .Include(c => c.Conditions)
                    .ThenInclude(cond => cond.Destinations)
                        .ThenInclude(dest => dest.Country)
                .FirstOrDefaultAsync(c => c.Id == contractId);
        }

        public async Task<Contract?> GetContractWithTransporterAsync(int contractId)
        {
            return await _entities
                .Include(c => c.Transporter)
                .Include(c => c.Conditions)
                .Include(c => c.Destinations)
                .FirstOrDefaultAsync(c => c.Id == contractId);
        }

        public async Task<IEnumerable<Contract>> GetActiveContractsAsync()
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(c => c.Transporter)
                .Include(c => c.Conditions)
                    .ThenInclude(cond => cond.Destinations)
                .Where(c => c.ValidUntil > currentDate)
                .OrderByDescending(c => c.ValidUntil)
                .ThenBy(c => c.ContractNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contract>> GetContractsByTransporterAsync(int transporterId)
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(c => c.Transporter)
                .Include(c => c.Conditions)
                    .ThenInclude(cond => cond.Destinations)
                .Where(c => c.TransporterId == transporterId && c.ValidUntil > currentDate)
                .OrderByDescending(c => c.ValidUntil)
                .ThenBy(c => c.ContractNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contract>> GetExpiringContractsAsync(DateTime thresholdDate)
        {
            return await _entities
                .Include(c => c.Transporter)
                .Include(c => c.Conditions)
                .Where(c => c.ValidUntil <= thresholdDate && c.ValidUntil > DateTime.Now)
                .OrderBy(c => c.ValidUntil)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalContractValueByTransporterAsync(int transporterId)
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Where(c => c.TransporterId == transporterId && c.ValidUntil > currentDate)
                .SumAsync(c => c.ValueEUR);
        }

        // ========== ADDITIONAL BUSINESS METHODS ==========

        public async Task<bool> IsContractNumberUniqueAsync(string contractNumber, int? excludeContractId = null)
        {
            var query = _entities.AsQueryable();

            if (excludeContractId.HasValue)
            {
                query = query.Where(c => c.Id != excludeContractId.Value);
            }

            return !await query.AnyAsync(c => c.ContractNumber == contractNumber);
        }

        public async Task<IEnumerable<Contract>> GetContractsWithExpiringConditionsAsync(int daysThreshold = 30)
        {
            var warningDate = DateTime.Now.AddDays(daysThreshold);

            return await _entities
                .Include(c => c.Transporter)
                .Include(c => c.Conditions)
                    .ThenInclude(cond => cond.Destinations)
                .Where(c => c.ValidUntil <= warningDate && c.ValidUntil > DateTime.Now)
                .OrderBy(c => c.ValidUntil)
                .ToListAsync();
        }

        public async Task<Dictionary<int, decimal>> GetContractValuesByTransporterAsync()
        {
            var currentDate = DateTime.Now;

            return await _entities
                .Include(c => c.Transporter)
                .Where(c => c.ValidUntil > currentDate)
                .GroupBy(c => c.TransporterId)
                .Select(g => new { TransporterId = g.Key, TotalValue = g.Sum(c => c.ValueEUR) })
                .ToDictionaryAsync(x => x.TransporterId, x => x.TotalValue);
        }

        public async Task<int> GetActiveContractCountByTransporterAsync(int transporterId)
        {
            var currentDate = DateTime.Now;

            return await _entities
                .CountAsync(c => c.TransporterId == transporterId && c.ValidUntil > currentDate);
        }

        public async Task<Contract?> GetContractWithAllDetailsAsync(int contractId)
        {
            return await _entities
                .Include(c => c.Transporter)
                .Include(c => c.Conditions)
                    .ThenInclude(cond => cond.Destinations)
                        .ThenInclude(dest => dest.City)
                .Include(c => c.Conditions)
                    .ThenInclude(cond => cond.Destinations)
                        .ThenInclude(dest => dest.Country)
                .Include(c => c.Conditions)
                    .ThenInclude(cond => cond.Destinations)
                        .ThenInclude(dest => dest.Orders)
                .Include(c => c.Destinations)
                    .ThenInclude(dest => dest.City)
                .Include(c => c.Destinations)
                    .ThenInclude(dest => dest.Country)
                .FirstOrDefaultAsync(c => c.Id == contractId);
        }

        public async Task<bool> CanRenewContractAsync(int contractId)
        {
            var contract = await GetByIdAsync(contractId);
            if (contract == null)
                return false;

            // Check if contract is expired or expiring soon (within 30 days)
            var renewalThreshold = DateTime.Now.AddDays(30);
            return contract.ValidUntil <= renewalThreshold;
        }

        public async Task<Contract> RenewContractAsync(int contractId, int extensionYears = 1)
        {
            var contract = await GetByIdAsync(contractId);
            if (contract == null)
                throw new NotFoundException($"Contract {contractId} not found");

            // Create a new contract based on the existing one
            var newContract = new Contract
            {
                ContractNumber = await GenerateRenewalContractNumberAsync(contract.ContractNumber),
                TransporterId = contract.TransporterId,
                ValueEUR = contract.ValueEUR,
                CreatedDate = DateTime.Now,
                ValidUntil = DateTime.Now.AddYears(extensionYears)
            };

            return await AddAsync(newContract);
        }

        private async Task<string> GenerateRenewalContractNumberAsync(string originalContractNumber)
        {
            // Extract base number and find the latest renewal
            var baseNumber = originalContractNumber.Split('-')[0];
            var renewalPattern = $"{baseNumber}-R";

            var latestRenewal = await _entities
                .Where(c => c.ContractNumber.StartsWith(renewalPattern))
                .OrderByDescending(c => c.ContractNumber)
                .FirstOrDefaultAsync();

            if (latestRenewal == null)
                return $"{renewalPattern}01";

            // Extract number and increment
            var lastNumber = int.Parse(latestRenewal.ContractNumber.Substring(renewalPattern.Length));
            return $"{renewalPattern}{(lastNumber + 1):D2}";
        }

        public async Task<IEnumerable<Contract>> SearchContractsAsync(
            string? searchTerm = null,
            int? transporterId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            decimal? minValue = null,
            decimal? maxValue = null,
            bool? activeOnly = true)
        {
            var query = _entities
                .Include(c => c.Transporter)
                .Include(c => c.Conditions)
                .AsQueryable();

            if (activeOnly.HasValue && activeOnly.Value)
            {
                query = query.Where(c => c.ValidUntil > DateTime.Now);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c =>
                    c.ContractNumber.ToLower().Contains(searchTerm) ||
                    c.Transporter.CompanyName.ToLower().Contains(searchTerm) ||
                    c.Transporter.ContactPerson.ToLower().Contains(searchTerm));
            }

            if (transporterId.HasValue)
            {
                query = query.Where(c => c.TransporterId == transporterId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(c => c.CreatedDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(c => c.CreatedDate <= toDate.Value);
            }

            if (minValue.HasValue)
            {
                query = query.Where(c => c.ValueEUR >= minValue.Value);
            }

            if (maxValue.HasValue)
            {
                query = query.Where(c => c.ValueEUR <= maxValue.Value);
            }

            return await query
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }
    }
}

