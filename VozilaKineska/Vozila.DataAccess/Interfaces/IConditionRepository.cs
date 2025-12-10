using Vozila.Domain.Models;

namespace Vozila.DataAccess.Interfaces
{
    public interface IConditionRepository : IRepository<Condition>
    {
        // Condition-specific queries
        Task<Condition?> GetConditionWithContractAsync(int conditionId);
        Task<Condition?> GetConditionWithDestinationsAsync(int conditionId);
        Task<Condition?> GetFullConditionDetailsAsync(int conditionId);

        // Business logic queries
        Task<Condition?> GetConditionForContractAsync(int contractId);
        Task<Condition?> GetBestConditionForCityAsync(int cityId);
        Task<decimal> CalculateAverageContractOilPriceAsync(int transporterId);

        // Validation queries
        Task<bool> HasValidConditionForContractAsync(int contractId);
        Task<bool> CanAddDestinationToConditionAsync(int conditionId, int cityId);

        // Bulk operations
        Task<IEnumerable<Condition>> GetConditionsByTransporterAsync(int transporterId);
        Task<IEnumerable<Condition>> GetActiveConditionsAsync();
        Task<IEnumerable<Condition>> GetConditionsExpiringWithinAsync(TimeSpan timeSpan);
    }
}
