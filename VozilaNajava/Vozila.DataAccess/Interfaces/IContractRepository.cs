using Vozila.Domain.Models;

namespace Vozila.DataAccess.Interfaces
{
    public interface IContractRepository : IRepository<Contract>
    {
        Task<Contract> GetContractWithConditionsAsync(int id);
        Task<IEnumerable<Contract>> GetActiveContractsAsync();
        Task<Condition> GetBestConditionForDestinationAsync(int cityId);
        Task<IEnumerable<Contract>> GetContractsByTransporterAsync(int transporterId);
    }
}
