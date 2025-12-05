using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.Models;

namespace Vozila.Controllers
{
   
    [Authorize(Roles = "Transporter")]
    public class TransporterController : Controller
    {
        private readonly IOrderService _orderService;

        public TransporterController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        // ------------------- PENDING ORDERS -------------------
        public async Task<IActionResult> PendingOrders()
        {
            int transporterId = int.Parse(User.FindFirst("UserId")!.Value);
            var list = await _orderService.GetPendingForTransporterAsync(transporterId);
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

            int transporterId = int.Parse(User.FindFirst("UserId")!.Value);

            bool ok = await _orderService.SubmitTruckAsync(model.OrderId, model.TruckPlateNo, transporterId);

            if (!ok)
            {
                ModelState.AddModelError("", "Could not submit truck.");
                return View(model);
            }

            return RedirectToAction("PendingOrders");
        }
    }

}
