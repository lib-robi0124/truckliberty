using Vozila.ViewModels.Models;

namespace Vozila.Services.Interfaces
{
    public interface IDestinationService
    {
        Task<IEnumerable<DestinationListVM>> GetAllActiveDestinationsAsync();
        Task<IEnumerable<DestinationListVM>> GetDestinationsByConditionAsync(int conditionId);
        Task<IEnumerable<DestinationListVM>> GetByContractAsync(int contractId);
        Task<IEnumerable<DestinationListVM>> GetByTransporterAsync(int transporterId);
        Task<DestinationDetailsVM?> GetDestinationDetailsAsync(int id);
        Task<DestinationVM?> GetDestinationByIdAsync(int id);
        Task<DestinationVM> CreateDestinationAsync(DestinationVM model);
        Task UpdateDestinationAsync(DestinationVM model);
        Task DeleteDestinationAsync(int id);
        // Price operations
        Task<decimal> GetCurrentPriceAsync(int destinationId);
        Task<Dictionary<int, decimal>> GetAllPricesForContractAsync(int contractId);
        Task UpdateOilPriceForAllAsync(decimal newDailyPrice);
        Task UpdateOilPriceForContractAsync(int contractId, decimal newDailyPrice);
    }

}
