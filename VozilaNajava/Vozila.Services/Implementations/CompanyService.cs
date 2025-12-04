using Microsoft.Extensions.Logging;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Enums;
using Vozila.Domain.Models;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.Models;

namespace Vozila.Services.Implementations
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(
            ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }
        // ========== CRUD ==========
        public async Task<CompanyVM> GetCompanyByIdAsync(int id)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
                throw new KeyNotFoundException($"Company with ID {id} not found");

            return MapToCompanyVM(company);
        }
        public async Task<IEnumerable<CompanyVM>> GetAllCompaniesAsync()
        {
            var companies = await _companyRepository.GetAllAsync();
            return companies.Select(MapToCompanyVM).ToList();
        }
        public async Task<CompanyWithOrdersVM> GetCompanyWithOrdersAsync(int id)
        {
            var company = await _companyRepository.GetWithOrdersAsync(id);
            if (company == null)
            {
                return null;
            }
            else
            {
                return (CompanyWithOrdersVM)MapToCompanyVM(company);
            }
        }
        public async Task<CompanyVM> CreateCompanyAsync(CreateCompanyVM companyVM)
        {
            // Validate input
            if (companyVM == null)
                throw new ArgumentNullException(nameof(companyVM));

            // Map to entity
            var company = new Company
            {
                CustomerName = companyVM.CustomerName,
                ShipingAddress = companyVM.ShippingAddress,
                Country = companyVM.Country,
                City = companyVM.City
            };

            // Save to repository
            var createdCompany = await _companyRepository.AddAsync(company);

            return MapToCompanyVM(createdCompany);
        }
        public async Task<CompanyVM> UpdateCompanyAsync(UpdateCompanyVM companyVM)
        {
            // Validate input
            if (companyVM == null)
                throw new ArgumentNullException(nameof(companyVM));

            // Check if company exists
            var existingCompany = await _companyRepository.GetByIdAsync(companyVM.Id);
            if (existingCompany == null)
                throw new KeyNotFoundException($"Company with ID {companyVM.Id} not found");

            // Update entity
            existingCompany.CustomerName = companyVM.CustomerName;
            existingCompany.ShipingAddress = companyVM.ShippingAddress;
            existingCompany.Country = companyVM.Country;
            existingCompany.City = companyVM.City;

            // Save changes
            await _companyRepository.UpdateAsync(existingCompany);

            return MapToCompanyVM(existingCompany);
        }
        public async Task<bool> DeleteCompanyAsync(int id)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
                throw new KeyNotFoundException($"Company with ID {id} not found");

            await _companyRepository.DeleteAsync(id);

            return true;
        }
        // ========== SPECIAL QUERIES ==========
        public async Task<IEnumerable<CompanyVM>> GetCompanyByCityAsync(City city)
        {
            var companies = await _companyRepository.GetByCityAsync(city);
            return companies.Select(MapToCompanyVM).ToList();
        }
        public async Task<IEnumerable<CompanyVM>> GetCompanyByCountryAsync(Country country)
        {
            var companies = await _companyRepository.GetByCountryAsync(country);
            return companies.Select(MapToCompanyVM).ToList();
        }
        private CompanyVM MapToCompanyVM(Company company)
        {
            return new CompanyVM
            {
                Id = company.Id,
                CustomerName = company.CustomerName,
                ShipingAddress = company.ShipingAddress,
                Country = company.Country,
                City = company.City,
                CountryName = company.Country.ToString(),
                CityName = company.City.ToString(),
                OrderCount = company.Orders?.Count ?? 0
            };
        }
    }
}
