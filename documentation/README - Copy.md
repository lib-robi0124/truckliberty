1. ICompanyService.cs (Service Interface)
csharp
using Vozila.Domain.Models;

namespace Vozila.Business.Services
{
    public interface ICompanyService
    {
              
        
        
        // Filter Operations
        Task<IEnumerable<CompanyVM>> GetCompaniesByCityAsync(City city);
        Task<IEnumerable<CompanyVM>> GetCompaniesByCountryAsync(Country country);
        
        // Validation
        Task<bool> ValidateCompanyForOrderAsync(int companyId, City destinationCity);
    }
}
2. ViewModels/CompanyViewModels.cs
csharp
using Vozila.Domain.Models;

namespace Vozila.Business.ViewModels
{
    // Company List View
    public class CompanyVM
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ShipingAddress { get; set; } = string.Empty;
        public Country Country { get; set; }
        public City City { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
    }

    // Create Company
    public class CreateCompanyVM
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        public Country Country { get; set; }

        [Required]
        public City City { get; set; }
    }

    // Update Company
    public class UpdateCompanyVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        public Country Country { get; set; }

        [Required]
        public City City { get; set; }
    }

    // Company with Orders
    public class CompanyWithOrdersVM : CompanyVM
    {
        public List<OrderVM> Orders { get; set; } = new List<OrderVM>();
    }

    // Order View
    public class OrderVM
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal DestinationPrice { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public City DestinationCity { get; set; }
    }

    // Destination Price Offer
    public class DestinationPriceOfferVM
    {
        public int DestinationId { get; set; }
        public City DestinationCity { get; set; }
        public decimal BestPrice { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public bool UseFormulaPrice { get; set; }
        public OrderPriceBreakdownVM? PriceBreakdown { get; set; }
    }

    // Detailed Price Breakdown
    public class DestinationPriceDetailVM
    {
        public int DestinationId { get; set; }
        public City City { get; set; }
        public Country Country { get; set; }
        public decimal BaseContractPrice { get; set; }
        public decimal CalculatedPrice { get; set; }
        public decimal DailyPricePerLiter { get; set; }
        public decimal ContractOilPrice { get; set; }
        public bool UseFormula { get; set; }
        public decimal PriceDifference { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string FormulaExplanation { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
    }

    // Available Destinations List
    public class AvailableDestinationVM
    {
        public int DestinationId { get; set; }
        public City City { get; set; }
        public Country Country { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string PriceType { get; set; } = "Fixed";
        public decimal BasePrice { get; set; }
        public decimal DiscountApplied { get; set; }
    }

    // Price Breakdown Details
    public class OrderPriceBreakdownVM
    {
        public decimal BaseDestinationPrice { get; set; }
        public decimal DailyOilPrice { get; set; }
        public decimal ContractOilPrice { get; set; }
        public decimal FormulaAdjustment { get; set; }
        public decimal CompanyDiscount { get; set; }
        public decimal LoyaltyDiscount { get; set; }
        public decimal VolumeDiscount { get; set; }
        public decimal FinalPrice { get; set; }
        public string PriceCalculationMethod { get; set; } = "Fixed";
    }

    // Create Order (for OrderService)
    public class CreateOrderVM
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public City DestinationCity { get; set; }
        public int DestinationId { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public decimal DestinationPrice { get; set; }
        public decimal BaseContractPrice { get; set; }
        public bool UsedFormulaPrice { get; set; }
        public decimal DailyPricePerLiter { get; set; }
        public decimal ContractOilPrice { get; set; }
        public int ConditionId { get; set; }
        public OrderPriceBreakdownVM? PriceBreakdown { get; set; }
    }
}
3. CompanyService.cs (Service Implementation)
csharp
using Microsoft.Extensions.Logging;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Models;
using Vozila.Business.ViewModels;

namespace Vozila.Business.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IDestinationRepository _destinationRepository;
        private readonly ILogger<CompanyService> _logger;
        private readonly IOrderService _orderService;

        public CompanyService(
            ICompanyRepository companyRepository,
            IDestinationRepository destinationRepository,
            ILogger<CompanyService> logger,
            IOrderService orderService)
        {
            _companyRepository = companyRepository;
            _destinationRepository = destinationRepository;
            _logger = logger;
            _orderService = orderService;
        }

        // Basic CRUD Operations
        public async Task<CompanyVM> GetCompanyByIdAsync(int id)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            return company == null ? throw new KeyNotFoundException($"Company {id} not found") : MapToCompanyVM(company);
        }

        public async Task<IEnumerable<CompanyVM>> GetAllCompaniesAsync()
            => (await _companyRepository.GetAllAsync()).Select(MapToCompanyVM);

        public async Task<CompanyWithOrdersVM> GetCompanyWithOrdersAsync(int id)
        {
            var company = await _companyRepository.GetWithOrdersAsync(id);
            return company == null ? throw new KeyNotFoundException($"Company {id} not found") : MapToCompanyWithOrdersVM(company);
        }

        public async Task<CompanyVM> CreateCompanyAsync(CreateCompanyVM vm)
        {
            var company = new Company
            {
                CustomerName = vm.CustomerName,
                ShipingAddress = vm.ShippingAddress,
                Country = vm.Country,
                City = vm.City
            };

            var created = await _companyRepository.AddAsync(company);
            _logger.LogInformation($"Company {created.CustomerName} created (ID: {created.Id})");
            return MapToCompanyVM(created);
        }

        public async Task<CompanyVM> UpdateCompanyAsync(UpdateCompanyVM vm)
        {
            var company = await _companyRepository.GetByIdAsync(vm.Id) 
                ?? throw new KeyNotFoundException($"Company {vm.Id} not found");

            company.CustomerName = vm.CustomerName;
            company.ShipingAddress = vm.ShippingAddress;
            company.Country = vm.Country;
            company.City = vm.City;

            await _companyRepository.UpdateAsync(company);
            _logger.LogInformation($"Company {vm.Id} updated");
            return MapToCompanyVM(company);
        }

        public async Task<bool> DeleteCompanyAsync(int id)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null) return false;

            if (company.Orders?.Any(o => o.Status is "Active" or "Processing") == true)
                throw new InvalidOperationException($"Cannot delete company {id} with active orders");

            await _companyRepository.DeleteAsync(id);
            _logger.LogInformation($"Company {id} deleted");
            return true;
        }

        // Business Logic Core: Company is mandatory for Destination Price
        public async Task<DestinationPriceOfferVM> GetBestDestinationPriceOfferAsync(int companyId, City destinationCity)
        {
            var company = await GetAndValidateCompanyAsync(companyId);
            var destination = await GetDestinationByCityAsync(destinationCity);
            
            var priceBreakdown = await CalculateBestPriceAsync(company, destination);
            
            return new DestinationPriceOfferVM
            {
                DestinationId = destination.Id,
                DestinationCity = destinationCity,
                BestPrice = priceBreakdown.FinalPrice,
                ShippingAddress = company.ShipingAddress,
                IsAvailable = priceBreakdown.FinalPrice > 0,
                CompanyName = company.CustomerName,
                UseFormulaPrice = destination.ContractOilPrice > 0,
                PriceBreakdown = priceBreakdown
            };
        }

        public async Task<DestinationPriceDetailVM> GetDestinationPriceDetailAsync(int companyId, City destinationCity)
        {
            var company = await GetAndValidateCompanyAsync(companyId);
            var destination = await GetDestinationByCityAsync(destinationCity);
            var breakdown = await CalculateBestPriceAsync(company, destination);

            return new DestinationPriceDetailVM
            {
                DestinationId = destination.Id,
                City = destinationCity,
                Country = destination.Country,
                BaseContractPrice = destination.DestinationContractPrice,
                CalculatedPrice = breakdown.FinalPrice,
                DailyPricePerLiter = destination.DailyPricePerLiter,
                ContractOilPrice = destination.ContractOilPrice,
                UseFormula = destination.ContractOilPrice > 0,
                PriceDifference = breakdown.FinalPrice - destination.DestinationContractPrice,
                DiscountPercentage = CalculateDiscountPercentage(destination.DestinationContractPrice, breakdown.FinalPrice),
                FormulaExplanation = GetFormulaExplanation(destination),
                IsAvailable = breakdown.FinalPrice > 0,
                ShippingAddress = company.ShipingAddress,
                CompanyName = company.CustomerName
            };
        }

        // Order Operations
        public async Task<bool> CanPlaceOrderAsync(int companyId, City destinationCity)
        {
            try
            {
                var offer = await GetBestDestinationPriceOfferAsync(companyId, destinationCity);
                return offer.IsAvailable && offer.BestPrice > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<OrderVM?> PlaceOrderAsync(int companyId, City destinationCity, string shippingAddress)
        {
            var company = await GetAndValidateCompanyAsync(companyId);
            var offer = await GetBestDestinationPriceOfferAsync(companyId, destinationCity);
            
            if (!offer.IsAvailable)
                throw new InvalidOperationException($"No price offer for {destinationCity}");

            var destination = await GetDestinationByCityAsync(destinationCity);
            
            // Validate destination for order
            if (!await _destinationRepository.ValidateDestinationForOrderAsync(destination.Id))
                throw new InvalidOperationException($"Destination {destinationCity} not valid for orders");

            // Create order via OrderService
            return await _orderService.CreateOrderAsync(new CreateOrderVM
            {
                CompanyId = companyId,
                CompanyName = company.CustomerName,
                DestinationCity = destinationCity,
                DestinationId = destination.Id,
                ShippingAddress = shippingAddress ?? company.ShipingAddress,
                DestinationPrice = offer.BestPrice,
                BaseContractPrice = destination.DestinationContractPrice,
                UsedFormulaPrice = offer.UseFormulaPrice,
                DailyPricePerLiter = destination.DailyPricePerLiter,
                ContractOilPrice = destination.ContractOilPrice,
                ConditionId = destination.ConditionId,
                PriceBreakdown = offer.PriceBreakdown
            });
        }

        // Destination Operations
        public async Task<IEnumerable<AvailableDestinationVM>> GetAvailableDestinationsAsync(int companyId)
        {
            var company = await GetAndValidateCompanyAsync(companyId);
            var destinations = await GetAllDestinationsAsync();
            
            var available = new List<AvailableDestinationVM>();
            
            foreach (var destination in destinations.Where(d => d.City != company.City))
            {
                var breakdown = await CalculateBestPriceAsync(company, destination);
                if (breakdown.FinalPrice > 0)
                {
                    available.Add(new AvailableDestinationVM
                    {
                        DestinationId = destination.Id,
                        City = destination.City,
                        Country = destination.Country,
                        Price = breakdown.FinalPrice,
                        IsAvailable = true,
                        PriceType = destination.ContractOilPrice > 0 ? "Formula" : "Fixed",
                        BasePrice = destination.DestinationContractPrice,
                        DiscountApplied = destination.DestinationContractPrice - breakdown.FinalPrice
                    });
                }
            }
            
            return available.OrderBy(d => d.Price);
        }

        public async Task<IEnumerable<AvailableDestinationVM>> GetDestinationsByContractAsync(int companyId, int contractId)
        {
            var company = await GetAndValidateCompanyAsync(companyId);
            var destinations = await _destinationRepository.GetDestinationsByContractIdAsync(contractId);
            
            return await ProcessDestinationsForCompany(company, destinations);
        }

        // Filter Operations
        public async Task<IEnumerable<CompanyVM>> GetCompaniesByCityAsync(City city)
            => (await _companyRepository.GetByCityAsync(city)).Select(MapToCompanyVM);

        public async Task<IEnumerable<CompanyVM>> GetCompaniesByCountryAsync(Country country)
            => (await _companyRepository.GetByCountryAsync(country)).Select(MapToCompanyVM);

        // Validation
        public async Task<bool> ValidateCompanyForOrderAsync(int companyId, City destinationCity)
        {
            try
            {
                var company = await _companyRepository.GetByIdAsync(companyId);
                if (company == null || string.IsNullOrWhiteSpace(company.CustomerName))
                    return false;

                var destinations = await GetAllDestinationsAsync();
                var destination = destinations.FirstOrDefault(d => d.City == destinationCity);
                if (destination == null) return false;

                var breakdown = await CalculateBestPriceAsync(company, destination);
                return breakdown.FinalPrice > 0;
            }
            catch
            {
                return false;
            }
        }

        // Private Helper Methods
        private async Task<Company> GetAndValidateCompanyAsync(int companyId)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
                throw new KeyNotFoundException($"Company {companyId} not found");
            if (string.IsNullOrWhiteSpace(company.CustomerName))
                throw new InvalidOperationException("Company name is required for destination pricing");
            return company;
        }

        private async Task<Destination> GetDestinationByCityAsync(City city)
        {
            var destinations = await GetAllDestinationsAsync();
            var destination = destinations.FirstOrDefault(d => d.City == city);
            return destination ?? throw new KeyNotFoundException($"Destination for city {city} not found");
        }

        private async Task<IEnumerable<Destination>> GetAllDestinationsAsync()
            => await _destinationRepository.GetAllAsync();

        private async Task<OrderPriceBreakdownVM> CalculateBestPriceAsync(Company company, Destination destination)
        {
            var breakdown = new OrderPriceBreakdownVM
            {
                BaseDestinationPrice = destination.DestinationContractPrice,
                DailyOilPrice = destination.DailyPricePerLiter,
                ContractOilPrice = destination.ContractOilPrice
            };

            decimal calculatedPrice = destination.ContractOilPrice > 0
                ? destination.DestinationPriceFromFormula
                : destination.DestinationContractPrice;

            breakdown.FormulaAdjustment = calculatedPrice - destination.DestinationContractPrice;
            breakdown.PriceCalculationMethod = destination.ContractOilPrice > 0 ? "Formula" : "Fixed";

            var (companyDiscount, loyaltyDiscount, volumeDiscount, finalPrice) = 
                await CalculateCompanyDiscountsAsync(company, destination, calculatedPrice);

            breakdown.CompanyDiscount = companyDiscount;
            breakdown.LoyaltyDiscount = loyaltyDiscount;
            breakdown.VolumeDiscount = volumeDiscount;
            breakdown.FinalPrice = Math.Round(finalPrice, 2);

            return breakdown;
        }

        private async Task<(decimal, decimal, decimal, decimal)> CalculateCompanyDiscountsAsync(
            Company company, Destination destination, decimal basePrice)
        {
            decimal finalPrice = basePrice;
            decimal companyDiscount = 0, loyaltyDiscount = 0, volumeDiscount = 0;

            var companyWithOrders = await _companyRepository.GetWithOrdersAsync(company.Id);

            // Loyalty discount
            if (companyWithOrders.Orders?.Count > 10)
            {
                loyaltyDiscount = finalPrice * 0.05m;
                finalPrice -= loyaltyDiscount;
            }
            else if (companyWithOrders.Orders?.Count > 5)
            {
                loyaltyDiscount = finalPrice * 0.02m;
                finalPrice -= loyaltyDiscount;
            }

            // Volume discount
            if (companyWithOrders.Orders?.Average(o => o.TotalAmount) > 10000)
            {
                volumeDiscount = finalPrice * 0.03m;
                finalPrice -= volumeDiscount;
            }

            // Domestic discount
            if (company.Country == destination.Country)
            {
                companyDiscount = finalPrice * 0.02m;
                finalPrice -= companyDiscount;
            }

            // Premium customer discount
            if (company.CustomerName.Contains("Preferred", StringComparison.OrdinalIgnoreCase) ||
                company.CustomerName.Contains("Premium", StringComparison.OrdinalIgnoreCase))
            {
                var premiumDiscount = finalPrice * 0.04m;
                companyDiscount += premiumDiscount;
                finalPrice -= premiumDiscount;
            }

            return (companyDiscount, loyaltyDiscount, volumeDiscount, finalPrice);
        }

        private async Task<IEnumerable<AvailableDestinationVM>> ProcessDestinationsForCompany(
            Company company, IEnumerable<Destination> destinations)
        {
            var available = new List<AvailableDestinationVM>();
            
            foreach (var destination in destinations)
            {
                var breakdown = await CalculateBestPriceAsync(company, destination);
                if (breakdown.FinalPrice > 0)
                {
                    available.Add(new AvailableDestinationVM
                    {
                        DestinationId = destination.Id,
                        City = destination.City,
                        Country = destination.Country,
                        Price = breakdown.FinalPrice,
                        IsAvailable = true,
                        PriceType = destination.ContractOilPrice > 0 ? "Formula" : "Fixed",
                        BasePrice = destination.DestinationContractPrice,
                        DiscountApplied = destination.DestinationContractPrice - breakdown.FinalPrice
                    });
                }
            }
            
            return available.OrderBy(d => d.Price);
        }

        // Mapping Methods
        private CompanyVM MapToCompanyVM(Company company) => new()
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

        private CompanyWithOrdersVM MapToCompanyWithOrdersVM(Company company) => new()
        {
            Id = company.Id,
            CustomerName = company.CustomerName,
            ShipingAddress = company.ShipingAddress,
            Country = company.Country,
            City = company.City,
            CountryName = company.Country.ToString(),
            CityName = company.City.ToString(),
            OrderCount = company.Orders?.Count ?? 0,
            Orders = company.Orders?.Select(o => new OrderVM
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                DestinationPrice = o.DestinationPrice,
                ShippingAddress = o.ShippingAddress,
                DestinationCity = o.DestinationCity
            }).ToList() ?? new()
        };

        private decimal CalculateDiscountPercentage(decimal basePrice, decimal finalPrice)
            => basePrice > 0 ? Math.Round((basePrice - finalPrice) / basePrice * 100, 2) : 0;

        private string GetFormulaExplanation(Destination destination)
            => destination.ContractOilPrice > 0
                ? $"Formula: {destination.DestinationContractPrice} × (1 + ({destination.DailyPricePerLiter} - {destination.ContractOilPrice}) / {destination.ContractOilPrice} × 0.3)"
                : "Fixed contract price";
    }
}
4. Updated IDestinationRepository.cs
csharp
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Interfaces
{
    public interface IDestinationRepository : IRepository<Destination>
    {
        // Price Calculation
        Task<decimal> GetCurrentDestinationPriceAsync(int destinationId);
        Task<Dictionary<int, decimal>> CalculateAllDestinationPricesAsync(int contractId);
        Task<Dictionary<int, decimal>> CalculateAllPricesForContractAsync(int contractId);

        // Price Updates
        Task UpdateAllDestinationOilPricesAsync(decimal newDailyPrice);
        Task UpdateContractDestinationsOilPricesAsync(int contractId, decimal newDailyPrice);
        Task UpdateDailyPricesForContractAsync(int contractId, decimal newDailyPrice);

        // Query Methods
        Task<Destination?> GetDestinationWithFullDetailsAsync(int destinationId);
        Task<Destination?> GetWithFullDetailsAsync(int destinationId);
        Task<List<Destination>> GetDestinationsByContractIdAsync(int contractId);
        Task<IEnumerable<Destination>> GetByContractIdAsync(int contractId);
        Task<IEnumerable<Destination>> GetByTransporterIdAsync(int transporterId);
        Task<IEnumerable<Destination>> GetByCityIdAsync(int cityId);
        
        // NEW: Get destinations by City enum (for CompanyService)
        Task<IEnumerable<Destination>> GetByCityAsync(City city);

        // Transporter & Validation
        Task<Transporter?> FindTransporterForDestinationAsync(int cityId);
        Task<bool> ValidateDestinationForOrderAsync(int destinationId);

        // Additional methods
        Task<IEnumerable<Destination>> GetActiveDestinationsByContractAsync(int contractId);
        Task<decimal> GetLatestOilPriceAsync();
        Task<IEnumerable<Destination>> GetDestinationsNeedingPriceUpdateAsync(DateTime sinceDate);
    }
}
5. Updated DestinationRepository.cs (Partial Implementation)
csharp
using Microsoft.EntityFrameworkCore;
using Vozila.DataAccess.Context;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Repositories
{
    public class DestinationRepository : Repository<Destination>, IDestinationRepository
    {
        private readonly ApplicationDbContext _context;

        public DestinationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // Price Calculation
        public async Task<decimal> GetCurrentDestinationPriceAsync(int destinationId)
        {
            var destination = await _context.Destinations
                .Include(d => d.Condition)
                .FirstOrDefaultAsync(d => d.Id == destinationId);
            
            return destination?.DestinationPriceFromFormula ?? 0;
        }

        public async Task<Dictionary<int, decimal>> CalculateAllDestinationPricesAsync(int contractId)
        {
            var destinations = await _context.Destinations
                .Include(d => d.Condition)
                .Where(d => d.ContractId == contractId)
                .ToListAsync();

            return destinations.ToDictionary(
                d => d.Id,
                d => d.DestinationPriceFromFormula
            );
        }

        public async Task<Dictionary<int, decimal>> CalculateAllPricesForContractAsync(int contractId)
            => await CalculateAllDestinationPricesAsync(contractId);

        // NEW: Get by City enum
        public async Task<IEnumerable<Destination>> GetByCityAsync(City city)
        {
            return await _context.Destinations
                .Include(d => d.Condition)
                .Where(d => d.City == city)
                .ToListAsync();
        }

        // Query Methods
        public async Task<Destination?> GetDestinationWithFullDetailsAsync(int destinationId)
        {
            return await _context.Destinations
                .Include(d => d.Condition)
                .Include(d => d.Orders)
                .FirstOrDefaultAsync(d => d.Id == destinationId);
        }

        public async Task<Destination?> GetWithFullDetailsAsync(int destinationId)
            => await GetDestinationWithFullDetailsAsync(destinationId);

        public async Task<List<Destination>> GetDestinationsByContractIdAsync(int contractId)
        {
            return await _context.Destinations
                .Include(d => d.Condition)
                .Where(d => d.ContractId == contractId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Destination>> GetByContractIdAsync(int contractId)
            => await GetDestinationsByContractIdAsync(contractId);

        public async Task<IEnumerable<Destination>> GetByCityIdAsync(int cityId)
        {
            return await _context.Destinations
                .Include(d => d.Condition)
                .Where(d => d.CityId == cityId)
                .ToListAsync();
        }

        // Validation
        public async Task<bool> ValidateDestinationForOrderAsync(int destinationId)
        {
            var destination = await _context.Destinations
                .Include(d => d.Condition)
                .FirstOrDefaultAsync(d => d.Id == destinationId);

            if (destination == null) return false;
            
            // Validate destination is active and has valid pricing
            return destination.DestinationContractPrice > 0 && 
                   destination.Condition != null &&
                   destination.DailyPricePerLiter > 0;
        }

        // Additional methods
        public async Task<IEnumerable<Destination>> GetActiveDestinationsByContractAsync(int contractId)
        {
            return await _context.Destinations
                .Include(d => d.Condition)
                .Where(d => d.ContractId == contractId && d.IsActive)
                .ToListAsync();
        }

        public async Task<decimal> GetLatestOilPriceAsync()
        {
            var latestCondition = await _context.Conditions
                .OrderByDescending(c => c.UpdatedDate)
                .FirstOrDefaultAsync();
            
            return latestCondition?.CurrentOilPrice ?? 0;
        }

        // Price Updates (simplified implementation)
        public async Task UpdateAllDestinationOilPricesAsync(decimal newDailyPrice)
        {
            var destinations = await _context.Destinations.ToListAsync();
            foreach (var destination in destinations)
            {
                destination.DailyPricePerLiter = newDailyPrice;
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateContractDestinationsOilPricesAsync(int contractId, decimal newDailyPrice)
        {
            var destinations = await _context.Destinations
                .Where(d => d.ContractId == contractId)
                .ToListAsync();
            
            foreach (var destination in destinations)
            {
                destination.DailyPricePerLiter = newDailyPrice;
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDailyPricesForContractAsync(int contractId, decimal newDailyPrice)
            => await UpdateContractDestinationsOilPricesAsync(contractId, newDailyPrice);

        // Other methods from interface...
        public async Task<IEnumerable<Destination>> GetByTransporterIdAsync(int transporterId)
        {
            return await _context.Destinations
                .Where(d => d.TransporterId == transporterId)
                .ToListAsync();
        }

        public async Task<Transporter?> FindTransporterForDestinationAsync(int cityId)
        {
            var destination = await _context.Destinations
                .Include(d => d.Transporter)
                .FirstOrDefaultAsync(d => d.CityId == cityId);
            
            return destination?.Transporter;
        }

        public async Task<IEnumerable<Destination>> GetDestinationsNeedingPriceUpdateAsync(DateTime sinceDate)
        {
            return await _context.Destinations
                .Include(d => d.Condition)
                .Where(d => d.UpdatedDate < sinceDate || 
                           (d.Condition != null && d.Condition.UpdatedDate < sinceDate))
                .ToListAsync();
        }
    }
}
Summary of Clean Code Structure:
text
Vozila.Business/
├── Services/
│   ├── ICompanyService.cs
│   └── CompanyService.cs
├── ViewModels/
│   └── CompanyViewModels.cs
└── Interfaces/
    └── IOrderService.cs (assumed)

Vozila.DataAccess/
├── Interfaces/
│   └── IDestinationRepository.cs (updated)
└── Repositories/
    └── DestinationRepository.cs (updated)
