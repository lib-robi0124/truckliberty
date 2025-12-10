using Microsoft.AspNetCore.Mvc;

namespace Vozila.Controllers
{
    public class ReportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
