using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vozila.Domain.Models;
using Vozila.Filters;
using Vozila.Services.Implementations;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.ModelsOrder;
using Vozila.ViewModels.ModelsTransporter;

namespace Vozila.Controllers
{

    [SessionAuthorize(RequiredRole = "Transporter")]
    public class TransporterController : Controller
    {
        private readonly ITransporterService _transporterService;
        private readonly IOrderService _orderService;
        private readonly ILogger<TransporterController> _logger;

        public TransporterController(
            ITransporterService transporterService,
            IOrderService orderService,
            ILogger<TransporterController> logger)
        {
            _transporterService = transporterService;
            _orderService = orderService;
            _logger = logger;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            // Redirect to Home/Login with transporter parameter
            return RedirectToAction("Login", "Home", new { userType = "transporter", returnUrl });
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var transporterId = GetCurrentTransporterId();

                // Get recent orders for this transporter - ADD THIS BACK
                var orders = await _orderService.GetPendingForTransporterAsync(transporterId);

                // Get transporter stats
                var stats = await _transporterService.GetTransporterStatsAsync(transporterId);

                var model = new TransporterDashboardVM
                {
                    RecentOrders = orders.Take(10).ToList(),
                    Stats = stats
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transporter dashboard");
                TempData["ErrorMessage"] = "Error loading dashboard. Please try again.";
                return RedirectToAction("Login", "Home", new { userType = "transporter" });
            }
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Redirect to Dashboard
            return RedirectToAction("Dashboard");
        }

        private int GetCurrentTransporterId()
        {
            var transporterId = HttpContext.Session.GetInt32("TransporterId");
            return transporterId ?? 0;
        }

        // GET: Transporter/ManageOrder
        public async Task<IActionResult> ManageOrder(Domain.Models.OrderSearchCriteria? criteria = null)
        {
            var transporterId = GetCurrentTransporterId();

            if (criteria == null)
            {
                criteria = new Domain.Models.OrderSearchCriteria();
            }

            criteria.TransporterId = transporterId; // Ensure transporter ID is set

            var orders = await _orderService.SearchAsync(transporterId, criteria);

            return View(orders.ToList());
        }
        // GET: Transporter/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var transporterId = GetCurrentTransporterId();

            try
            {
                var orderDetails = await _orderService.GetOrderDetailsAsync(id, transporterId);

                if (orderDetails == null)
                {
                    TempData["ErrorMessage"] = "Order not found or you don't have permission to edit it.";
                    return RedirectToAction(nameof(ManageOrder));
                }

                var model = new TransporterEditOrderVM
                {
                    Id = orderDetails.Id,
                    CompanyName = orderDetails.CompanyName,
                    TransporterName = orderDetails.TransporterName,
                    DestinationCity = orderDetails.DestinationCity,
                    DateForLoadingFrom = orderDetails.DateForLoadingFrom,
                    DateForLoadingTo = orderDetails.DateForLoadingTo,
                    TruckPlateNo = orderDetails.TruckPlateNo,
                    CurrentStatus = orderDetails.Status
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading order {id} for edit");
                TempData["ErrorMessage"] = "Error loading order details.";
                return RedirectToAction(nameof(ManageOrder));
            }
        }
        // POST: Transporter/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransporterEditOrderVM model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Invalid order ID.";
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var transporterId = GetCurrentTransporterId();

            try
            {
                // Submit truck plate number
                await _transporterService.UpdateTruckPlateNoAsync(id, model.TruckPlateNo);
                TempData["SuccessMessage"] = "Truck plate number submitted successfully!";
                return RedirectToAction(nameof(ManageOrder));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to submit truck plate number.";
                return View(model);
            }
        }
   
        // GET: Transporter/Profile
        public async Task<IActionResult> Profile()
        {
            var transporterId = GetCurrentTransporterId();

            try
            {
                var stats = await _transporterService.GetTransporterStatsAsync(transporterId);
                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transporter profile");
                TempData["ErrorMessage"] = "Error loading profile.";
                return RedirectToAction(nameof(Dashboard));
            }
        }
           // ------------------- PENDING ORDERS -------------------
        public async Task<IActionResult> PendingOrders()
        {
            int? transporterId = HttpContext.Session.GetInt32("TransporterId");
            if (transporterId == null)
                return RedirectToAction("Login", "Home");
                
            var list = await _orderService.GetPendingForTransporterAsync(transporterId.Value);
            return View(list);
        }

        // ------------------- SUBMIT TRUCK -------------------
        [HttpGet]
        public IActionResult SubmitTruck(int orderId)
        {
            return View(new SubmitTruckVM { OrderId = orderId });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitTruck(SubmitTruckVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int? transporterId = HttpContext.Session.GetInt32("TransporterId");
            if (transporterId == null)
                return RedirectToAction("Login", "Home");

            bool ok = await _orderService.SubmitTruckAsync(model.OrderId, model.TruckPlateNo, transporterId.Value);

            if (!ok)
            {
                ModelState.AddModelError("", "Could not submit truck.");
                return View(model);
            }

            return RedirectToAction("PendingOrders");
        }
    }
}
