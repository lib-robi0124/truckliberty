using Vozila.Domain.Enums;
using Vozila.ViewModels.Models;

namespace Vozila.Services.Interfaces
{
    public interface ICompanyService
    {
        // Basic CRUD operations
        Task<CompanyVM> GetCompanyByIdAsync(int id);
        Task<IEnumerable<CompanyVM>> GetAllCompaniesAsync();
        Task<CompanyWithOrdersVM> GetCompanyWithOrdersAsync(int id);
        Task<CompanyVM> CreateCompanyAsync(CreateCompanyVM companyVM);
        Task<CompanyVM> UpdateCompanyAsync(UpdateCompanyVM companyVM);
        Task<bool> DeleteCompanyAsync(int id);

        Task<IEnumerable<CompanyVM>> GetCompanyByCityAsync(City city);
        Task<IEnumerable<CompanyVM>> GetCompanyByCountryAsync(Country country);
    }
}