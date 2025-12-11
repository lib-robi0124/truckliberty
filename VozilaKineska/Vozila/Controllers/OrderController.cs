using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vozila.DataAccess.DataContext;
using Vozila.Domain.Enums;
using Vozila.Domain.Models;
using Vozila.Filters;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.ModelsOrder;

namespace Vozila.Controllers
{
    [SessionAuthorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly AppDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            AppDbContext context,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _context = context;
            _logger = logger;
        }

        // ------------------- ORDER CRUD -------------------

        // GET: Order/Index or Order/Manage - Admin only
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("Role");
            var userType = HttpContext.Session.GetString("UserType");
            var transporterId = HttpContext.Session.GetInt32("TransporterId");

            // Admin sees dashboard with stats and quick actions
            if (userRole == "Admin" || userType == "Admin")
            {
                var dashboardModel = await GetAdminOrderDashboard();
                return View("AdminOrderDashboard", dashboardModel);
            }

            // Transporter sees their orders list
            if (userRole == "Transporter" || userType == "Transporter")
            {
                if (transporterId.HasValue)
                {
                    var transporterOrders = await _orderService.GetPendingForTransporterAsync(transporterId.Value);
                    return View("TransporterOrders", transporterOrders);
                }
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "Access denied.";
            return RedirectToAction("Index", "Home");
        }
        private async Task<AdminOrderDashboardVM> GetAdminOrderDashboard()
        {
            var allOrders = await _orderService.GetAllOrdersForAdminAsync();

            // Convert to list for multiple operations
            var ordersList = allOrders.ToList();

            var pendingOrders = ordersList.Where(o => o.Status == OrderStatus.Pending).ToList();
            var approvedOrders = ordersList.Where(o => o.Status == OrderStatus.Approved).ToList();
            var finishedOrders = ordersList.Where(o => o.Status == OrderStatus.Finished).ToList();
            var cancelledOrders = ordersList.Where(o => o.Status == OrderStatus.Cancelled).ToList();

            return new AdminOrderDashboardVM
            {
                TotalOrders = ordersList.Count(),
                PendingOrders = pendingOrders.Count,
                ApprovedOrders = approvedOrders.Count,
                FinishedOrders = finishedOrders.Count,
                CancelledOrders = cancelledOrders.Count,
                RecentOrders = ordersList.OrderByDescending(o => o.DateForLoadingFrom).Take(10).ToList(),
                // You might want to add more stats like orders by company, transporter, etc.
            };
        }
        // GET: Order/Details - Accessible by Admin and Transporter (for their own orders)
        [HttpGet]
        public async Task<IActionResult> Details(int id, int userId)
        {
            var userRole = HttpContext.Session.GetString("Role");
            var userType = HttpContext.Session.GetString("UserType");
            var transporterId = HttpContext.Session.GetInt32("TransporterId");

            // For transporter, ensure they can only access their own orders
            if (userRole == "Transporter" || userType == "Transporter")
            {
                // Get order and verify transporter owns it
                var order = await _orderService.GetOrderDetailsAsync(id, userId);
                if (order == null || order.TransporterId != transporterId)
                {
                    TempData["ErrorMessage"] = "Access denied to this order.";
                    return RedirectToAction("Index", "Home");
                }
            }

            // For admin, we need to get user ID from logged-in user
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var orderDetails = await _orderService.GetOrderDetailsAsync(id, userId);

            if (orderDetails == null)
                return NotFound();

            return View("OrderDetails", orderDetails);
        }

        // GET: Order/Create - Admin only
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userRole = HttpContext.Session.GetString("Role");
            var userType = HttpContext.Session.GetString("UserType");

            if (userRole != "Admin" && userType != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin role required.";
                return RedirectToAction("Index", "Home");
            }

            var model = new CreateOrderVM
            {
                DateForLoadingFrom = DateTime.Now.AddDays(1),
                DateForLoadingTo = DateTime.Now.AddDays(2)
            };

            await PopulateOrderDropdowns();
            return View("OrderCreate", model);
        }

        // POST: Order/Create - Admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderVM model)
        {
            var userRole = HttpContext.Session.GetString("Role");
            var userType = HttpContext.Session.GetString("UserType");

            if (userRole != "Admin" && userType != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin role required.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                await PopulateOrderDropdowns();
                return View("OrderCreate", model);
            }

            try
            {
                if (model.DateForLoadingFrom >= model.DateForLoadingTo)
                {
                    ModelState.AddModelError("", "Loading 'From' date must be before 'To' date.");
                    await PopulateOrderDropdowns();
                    return View("OrderCreate", model);
                }

                int orderId = await _orderService.CreateOrderAsync(model);
                TempData["SuccessMessage"] = "Order created successfully!";
                return RedirectToAction("Details", new { id = orderId, userId = 0 });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating order: {ex.Message}");
                _logger.LogError(ex, "Error creating order");
                await PopulateOrderDropdowns();
                return View("OrderCreate", model);
            }
        }

        // GET: Order/Edit - Accessible by Admin and Transporter (limited access)
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int userId)
        {
            var userRole = HttpContext.Session.GetString("Role");
            var userType = HttpContext.Session.GetString("UserType");
            var transporterId = HttpContext.Session.GetInt32("TransporterId");

            // For transporter, ensure they can only access their own orders
            if (userRole == "Transporter" || userType == "Transporter")
            {
                var order = await _orderService.GetOrderDetailsAsync(id, userId);
                if (order == null || order.TransporterId != transporterId)
                {
                    TempData["ErrorMessage"] = "Access denied to this order.";
                    return RedirectToAction("Index", "Home");
                }

                // Transporter can only edit TruckPlateNo
                var limitedModel = new EditOrderVM
                {
                    Id = order.Id,
                    TruckPlateNo = order.TruckPlateNo,
                    // Set other fields as read-only or hidden
                    CompanyId = order.CompanyId,
                    TransporterId = order.TransporterId,
                    DestinationId = order.DestinationId,
                    DateForLoadingFrom = order.DateForLoadingFrom,
                    DateForLoadingTo = order.DateForLoadingTo,
                    ContractOilPrice = order.ContractOilPrice,
                    Status = order.Status
                };

                ViewBag.IsTransporter = true; // Flag for view to disable fields
                return View("OrderEditTransporter", limitedModel);
            }

            // For admin, full access
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var adminOrder = await _orderService.GetOrderDetailsAsync(id, userId);

            if (adminOrder == null)
                return NotFound();

            var model = new EditOrderVM
            {
                Id = adminOrder.Id,
                CompanyId = adminOrder.CompanyId,
                TransporterId = adminOrder.TransporterId,
                DestinationId = adminOrder.DestinationId,
                DateForLoadingFrom = adminOrder.DateForLoadingFrom,
                DateForLoadingTo = adminOrder.DateForLoadingTo,
                ContractOilPrice = adminOrder.ContractOilPrice,
                Status = adminOrder.Status,
                TruckPlateNo = adminOrder.TruckPlateNo
            };

            await PopulateOrderDropdowns(model.CompanyId, model.TransporterId, model.DestinationId);
            ViewBag.IsTransporter = false;
            return View("OrderEdit", model);
        }

        // POST: Order/Edit - Different handling for Admin vs Transporter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditOrderVM model)
        {
            var userRole = HttpContext.Session.GetString("Role");
            var userType = HttpContext.Session.GetString("UserType");
            var transporterId = HttpContext.Session.GetInt32("TransporterId");

            // Transporter can only update TruckPlateNo
            if (userRole == "Transporter" || userType == "Transporter")
            {
                // Verify transporter owns this order
                var existingOrder = await _orderService.GetOrderDetailsAsync(model.Id, 0);
                if (existingOrder == null || existingOrder.TransporterId != transporterId)
                {
                    TempData["ErrorMessage"] = "Access denied to this order.";
                    return RedirectToAction("Index", "Home");
                }

                // Update only TruckPlateNo
                var updateResult = await _orderService.SubmitTruckAsync(model.Id, model.TruckPlateNo, model.TransporterId);

                if (updateResult)
                {
                    TempData["SuccessMessage"] = "Truck plate number updated successfully!";
                    return RedirectToAction("Details", new { id = model.Id, userId = 0 });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update truck plate number.");
                    ViewBag.IsTransporter = true;
                    return View("OrderEditTransporter", model);
                }
            }

            // Admin - full update
            if (!ModelState.IsValid)
            {
                await PopulateOrderDropdowns(model.CompanyId, model.TransporterId, model.DestinationId);
                ViewBag.IsTransporter = false;
                return View("OrderEdit", model);
            }

            if (model.DateForLoadingFrom >= model.DateForLoadingTo)
            {
                ModelState.AddModelError("", "Loading 'From' date must be before 'To' date.");
                await PopulateOrderDropdowns(model.CompanyId, model.TransporterId, model.DestinationId);
                ViewBag.IsTransporter = false;
                return View("OrderEdit", model);
            }

            try
            {
                var result = await _orderService.UpdateOrderAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "Order updated successfully!";
                    return RedirectToAction("Details", new { id = model.Id, userId = 0 });
                }

                ModelState.AddModelError("", "Failed to update order.");
                await PopulateOrderDropdowns(model.CompanyId, model.TransporterId, model.DestinationId);
                ViewBag.IsTransporter = false;
                return View("OrderEdit", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating order: {ex.Message}");
                _logger.LogError(ex, "Error updating order");
                await PopulateOrderDropdowns(model.CompanyId, model.TransporterId, model.DestinationId);
                ViewBag.IsTransporter = false;
                return View("OrderEdit", model);
            }
        }

        // POST: Order/Cancel - Admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string reason)
        {
            var userRole = HttpContext.Session.GetString("Role");
            var userType = HttpContext.Session.GetString("UserType");

            if (userRole != "Admin" && userType != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin role required.";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var result = await _orderService.CancelOrderAsync(id, reason, currentUserId);

                if (result)
                {
                    TempData["SuccessMessage"] = "Order cancelled successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to cancel order.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error cancelling order: {ex.Message}";
                _logger.LogError(ex, "Error cancelling order");
                return RedirectToAction("Details", new { id });
            }
        }

        // POST: Order/Finish - Admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finish(int id)
        {
            var userRole = HttpContext.Session.GetString("Role");
            var userType = HttpContext.Session.GetString("UserType");

            if (userRole != "Admin" && userType != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin role required.";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var result = await _orderService.FinishOrderAsync(id, currentUserId);

                if (result)
                {
                    TempData["SuccessMessage"] = "Order marked as finished!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to finish order.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error finishing order: {ex.Message}";
                _logger.LogError(ex, "Error finishing order");
                return RedirectToAction("Details", new { id });
            }
        }

        // Helper method to populate dropdowns
        private async Task PopulateOrderDropdowns(int? selectedCompanyId = null, int? selectedTransporterId = null, int? selectedDestinationId = null)
        {
            // Companies: All companies that exist in database are considered active
            var companies = await _context.Companies
                .Select(c => new { c.Id, c.CustomerName })
                .OrderBy(c => c.CustomerName)
                .ToListAsync();

            // Transporters: Only those with active contracts
            var transporters = await _context.Transporters
                .Where(t => t.Contracts.Any(c => c.ValidUntil >= DateTime.Now))
                .Select(t => new { t.Id, t.CompanyName })
                .OrderBy(t => t.CompanyName)
                .ToListAsync();

            // Destinations: All destinations are active, with current price
            var destinations = await _context.Destinations
                .Select(d => new
                {
                    d.Id,
                    Name = $"{d.City}, {d.Country} "
                })
                .OrderBy(d => d.Name)
                .ToListAsync();

            ViewBag.Companies = new SelectList(companies, "Id", "Name", selectedCompanyId);
            ViewBag.Transporters = new SelectList(transporters, "Id", "Name", selectedTransporterId);
            ViewBag.Destinations = new SelectList(destinations, "Id", "Name", selectedDestinationId);
        }
    }
}