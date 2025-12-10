using Vozila.Domain.Enums;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Interfaces
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<Company?> GetWithOrdersAsync(int id);
        Task<IEnumerable<Company>> GetAllWithOrdersAsync();
        Task<IEnumerable<Company>> GetByCityAsync(City city);
        Task<IEnumerable<Company>> GetByCountryAsync(Country country);
    }
}
