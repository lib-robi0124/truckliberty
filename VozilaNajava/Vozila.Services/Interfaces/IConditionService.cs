using Vozila.ViewModels.Models;

namespace Vozila.Services.Interfaces
{
    public interface IConditionService
    {
        Task<ConditionVM?> CreateConditionAsync(CreateConditionVM createVM);
        Task<ConditionVM> UpdateConditionAsync(UpdateConditionVM updateVM);
        Task<bool> DeleteConditionAsync(int id);
        Task<bool> AddDestinationToConditionAsync(int conditionId, int cityId);
        Task<ConditionVM?> GetConditionSummaryAsync(int contractId);
        Task<ConditionVM?> GetBestConditionForCityAsync(int cityId);
        // Reporting
        Task<IEnumerable<ConditionVM>> GetActiveConditionsAsync();
        Task<IEnumerable<ConditionVM>> GetConditionsExpiringWithinAsync(int days);
        Task<decimal> GetAverageOilPriceForTransporterAsync(int transporterId);
        Task<IEnumerable<ConditionVM>> GetConditionsByTransporterAsync(int transporterId);


    }
}
