using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vozila.Filters;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.Models;
using Vozila.ViewModels.ModelsContract;

namespace Vozila.Controllers
{
    [SessionAuthorize(RequiredRole = "Admin")]
    public class AdminController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IContractService _contractService;
        private readonly ICompanyService _companyService;
        private readonly IDestinationService _destinationService;
        private readonly ITansporterService _transporterService;

        public AdminController(
            IOrderService orderService, 
            IContractService contractService,
            ICompanyService companyService,
            IDestinationService destinationService,
            ITansporterService transporterService)
        {
            _orderService = orderService;
            _contractService = contractService;
            _companyService = companyService;
            _destinationService = destinationService;
            _transporterService = transporterService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardVM
            {
                Companies = (await _companyService.GetAllCompaniesAsync()).ToList(),
                Destinations = (await _destinationService.GetAllActiveDestinationsAsync())
                    .Select(d => new DestinationVM 
                    { 
                        Id = d.Id, 
                        CityName = d.CityName 
                    }).ToList(),
                Transporters = (await _transporterService.GetAllTransportersAsync())
                    .Select(t => new TransporterVM 
                    { 
                        Id = t.Id, 
                        CompanyName = t.CompanyName 
                    }).ToList()
            };
            return View(model);
        }

        public IActionResult Dashboard()
        {
            return View();
        }
        // ------------------- ORDER CREATE -------------------
        [HttpGet]
        public IActionResult OrderCreate()
        {
            return View(new CreateOrderVM());
        }

        [HttpPost]
        public async Task<IActionResult> OrderCreate(CreateOrderVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int orderId = await _orderService.CreateOrderAsync(model);
            return RedirectToAction("Details", "Order", new { id = orderId });
        }
        // ------------------- CONTRACT CREATE -------------------
        [HttpGet]
        public IActionResult ContractCreate()
        {
            return View(new ContractVM());
        }

        [HttpPost]
        public async Task<IActionResult> ContractCreate(ContractVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _contractService.CreateAsync(model);
            return RedirectToAction("ContractsList");
        }
        public async Task<IActionResult> ContractsList()
        {
            var data = await _contractService.GetAllAsync();
            return View(data);
        }
        // ------------------- REPORTS -------------------
        public IActionResult Reports()
        {
            return RedirectToAction("Index", "Report");
        }
    }
}
