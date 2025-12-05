using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.Models;

namespace Vozila.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IContractService _contractService;

        public AdminController(IOrderService orderService, IContractService contractService)
        {
            _orderService = orderService;
            _contractService = contractService;
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
