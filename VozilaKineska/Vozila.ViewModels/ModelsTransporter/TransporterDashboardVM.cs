using Vozila.ViewModels.ModelsOrder;

namespace Vozila.ViewModels.ModelsTransporter
{
    public class TransporterDashboardVM
    {
        public List<OrderListVM> RecentOrders { get; set; } = new();
        public TransporterStatsVM Stats { get; set; } = new();
    }
}
