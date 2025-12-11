using Microsoft.AspNetCore.Mvc;
using Vozila.Filters;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.ModelsOrder;

namespace Vozila.Controllers
{
   
    [SessionAuthorize(RequiredRole = "Transporter")]
    public class TransporterController : Controller
    {
        private readonly IOrderService _orderService;

        public TransporterController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
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
