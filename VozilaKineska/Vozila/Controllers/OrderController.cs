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
        // In OrderController.cs
        [HttpGet]
        public async Task<IActionResult> ManageOrder(string status = null, string company = null,
            DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var userRole = HttpContext.Session.GetString("Role");
            var userType = HttpContext.Session.GetString("UserType");

            if (userRole != "Admin" && userType != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin role required.";
                return RedirectToAction("Index", "Home");
            }

            var orders = await _orderService.GetAllOrdersForAdminAsync();

            // Apply filters if provided
            // Try to parse the string to OrderStatus enum
            if (Enum.TryParse<OrderStatus>(status, out var statusEnum))
            {
                orders = orders.Where(o => o.Status == statusEnum);
            }
            else
            {
                // If parsing fails, you might want to handle this case
                // For now, we'll just skip the status filter
                ModelState.AddModelError("status", "Invalid status value");
            }

            if (!string.IsNullOrEmpty(company))
            {
                orders = orders.Where(o => o.CompanyName.Contains(company, StringComparison.OrdinalIgnoreCase));
            }

            if (dateFrom.HasValue)
            {
                orders = orders.Where(o => o.DateForLoadingFrom >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                orders = orders.Where(o => o.DateForLoadingFrom <= dateTo.Value);
            }

            ViewBag.StatusFilter = status;
            ViewBag.CompanyFilter = company;
            ViewBag.DateFromFilter = dateFrom;
            ViewBag.DateToFilter = dateTo;

            return View(orders);
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
            // Get data safely
            var companies = await _context.Companies.ToListAsync();
            var transporters = await _context.Transporters.ToListAsync();
            var destinations = await _context.Destinations.ToListAsync();

            // Create SelectListItems manually
            var companyItems = companies
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CustomerName,
                    Selected = c.Id == selectedCompanyId
                })
                .OrderBy(c => c.Text)
                .ToList();

            var transporterItems = transporters
                .Where(t => t.Contracts?.Any(c => c.ValidUntil >= DateTime.Now) ?? false)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.CompanyName,
                    Selected = t.Id == selectedTransporterId
                })
                .OrderBy(t => t.Text)
                .ToList();

            var destinationItems = destinations
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"{d.City}, {d.Country}",
                    Selected = d.Id == selectedDestinationId
                })
                .OrderBy(d => d.Text)
                .ToList();

            ViewBag.Companies = companyItems;
            ViewBag.Transporters = transporterItems;
            ViewBag.Destinations = destinationItems;
        }
    }
}