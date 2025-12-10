using Vozila.Domain.Models;

namespace Vozila.DataAccess.Interfaces
{
    public interface IContractRepository : IRepository<Contract>
    {
        Task<Contract?> GetContractWithConditionsAsync(int contractId);
        Task<Contract?> GetContractWithTransporterAsync(int contractId);
        Task<IEnumerable<Contract>> GetActiveContractsAsync();
        Task<IEnumerable<Contract>> GetContractsByTransporterAsync(int transporterId);
        Task<IEnumerable<Contract>> GetExpiringContractsAsync(DateTime thresholdDate);
        Task<decimal> GetTotalContractValueByTransporterAsync(int transporterId);
    }
}
