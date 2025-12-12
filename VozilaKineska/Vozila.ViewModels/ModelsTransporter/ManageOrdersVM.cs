using Vozila.Domain.Models;
using Vozila.ViewModels.ModelsOrder;

namespace Vozila.ViewModels.ModelsTransporter
{
    public class ManageOrdersVM
    {
        public List<OrderListVM> Orders { get; set; } = new();
        public TransporterOrderSearchVM SearchCriteria { get; set; } = new();
    }
}
