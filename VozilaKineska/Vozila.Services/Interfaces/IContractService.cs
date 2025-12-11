using Vozila.Domain.Models;
using Vozila.ViewModels.ModelsContract;

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
        Task<Destination?> GetDestinationForContractAsync(int destinationId);
        Task AssignDestinationToContractAsync(int contractId, Destination destination);
        Task<decimal?> GetPriceForDestinationAsync(int contractId, int cityId, string conditionType);
    }
}


