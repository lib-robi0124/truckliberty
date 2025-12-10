using Vozila.Domain.Models;

namespace Vozila.DataAccess.Interfaces
{
    public interface IDestinationRepository : IRepository<Destination>
    {
        // Price Calculation
        Task<decimal> GetCurrentDestinationPriceAsync(int destinationId);
        Task<Dictionary<int, decimal>> CalculateAllDestinationPricesAsync(int contractId);
        Task<Dictionary<int, decimal>> CalculateAllPricesForContractAsync(int contractId); // Alias for backward compatibility

        // Price Updates
        Task UpdateAllDestinationOilPricesAsync(decimal newDailyPrice);
        Task UpdateContractDestinationsOilPricesAsync(int contractId, decimal newDailyPrice);
        Task UpdateDailyPricesForContractAsync(int contractId, decimal newDailyPrice); // Alias for backward compatibility

        // Query Methods
        Task<Destination?> GetDestinationWithFullDetailsAsync(int destinationId);
        Task<Destination?> GetWithFullDetailsAsync(int destinationId); // Alias for backward compatibility
        Task<List<Destination>> GetDestinationsByContractIdAsync(int contractId);
        Task<IEnumerable<Destination>> GetByContractIdAsync(int contractId); // Alias for backward compatibility
        Task<IEnumerable<Destination>> GetByTransporterIdAsync(int transporterId);
        Task<IEnumerable<Destination>> GetByCityIdAsync(int cityId);

        // Transporter & Validation
        Task<Transporter?> FindTransporterForDestinationAsync(int cityId);
        Task<bool> ValidateDestinationForOrderAsync(int destinationId);

        // Additional useful methods
        Task<IEnumerable<Destination>> GetActiveDestinationsByContractAsync(int contractId);
        Task<decimal> GetLatestOilPriceAsync();
        Task<IEnumerable<Destination>> GetDestinationsNeedingPriceUpdateAsync(DateTime sinceDate);
    }
}
