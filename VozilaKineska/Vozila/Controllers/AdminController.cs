using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vozila.DataAccess.DataContext;
using Vozila.Filters;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.Models;
using Vozila.ViewModels.ModelsContract;
using Vozila.ViewModels.ModelsDestination;
using Vozila.ViewModels.ModelsOrder;
using Vozila.ViewModels.ModelsTransporter;

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
        private readonly IPriceOilService _priceOilService;
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IOrderService orderService, 
            IContractService contractService,
            ICompanyService companyService,
            IDestinationService destinationService,
            ITansporterService transporterService,
            IPriceOilService priceOilService,
            AppDbContext context,
            ILogger<AdminController> logger)
        {
            _orderService = orderService;
            _contractService = contractService;
            _companyService = companyService;
            _destinationService = destinationService;
            _transporterService = transporterService;
            _priceOilService = priceOilService;
            _context = context;
            _logger = logger;
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
        // ------------------- ORDER CRUD -------------------
        [HttpGet]
        public IActionResult ManageOrder()
        {
            // Route to OrderController's Index/Manage action
            return RedirectToAction("Index", "Order");
        }

        [HttpGet]
        public IActionResult OrderDetails(int orderId, int userId)
        {
            // Route to OrderController's Details action
            return RedirectToAction("Details", "Order", new { id = orderId, userId = userId });
        }

        [HttpGet]
        public IActionResult OrderCreate()
        {
            // Route to OrderController's Create action
            return RedirectToAction("Create", "Order");
        }

        [HttpGet]
        public IActionResult OrderEdit(int orderId, int userId)
        {
            // Route to OrderController's Edit action
            return RedirectToAction("Edit", "Order", new { id = orderId, userId = userId });
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
