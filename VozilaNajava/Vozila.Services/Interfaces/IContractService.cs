using Vozila.Domain.Models;
using Vozila.ViewModels.Models;

namespace Vozila.Services.Interfaces
{
    public interface IContractService
    {
        Task<ContractVM?> GetByIdAsync(int id);
        Task<IEnumerable<ContractListVM>> GetAllAsync();
        Task<ContractDetailsVM?> GetDetailsAsync(int id);
        Task<ContractVM> CreateAsync(ContractVM model);
        Task UpdateAsync(ContractVM model);
        Task DeleteAsync(int id);
        Task<IEnumerable<ContractListVM>> GetContractsByTransporterAsync(int transporterId);
        Task<IEnumerable<ContractListVM>> GetExpiringContractsAsync(int daysThreshold);
        Task<bool> TransporterHasContractAsync(int transporterId);
        // Condition logic:
        Task<Condition?> GetConditionForContractAsync(int contractId);
        Task AssignConditionToContractAsync(int contractId, Condition condition);
        Task<decimal?> GetPriceForDestinationAsync(int contractId, int cityId, string conditionType);
    }
}


