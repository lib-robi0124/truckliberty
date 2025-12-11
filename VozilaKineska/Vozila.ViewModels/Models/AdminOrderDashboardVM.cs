// AdminOrderDashboardVM.cs
namespace Vozila.ViewModels.ModelsOrder
{
    public class AdminOrderDashboardVM
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ApprovedOrders { get; set; }  // Added for OrderStatus.Approved
        public int FinishedOrders { get; set; }   // Changed from CompletedOrders
        public int CancelledOrders { get; set; }
        public List<OrderListVM> RecentOrders { get; set; } = new List<OrderListVM>();

        // Optional: Add statistics for chart
        public Dictionary<string, int> OrdersByMonth { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> OrdersByCompany { get; set; } = new Dictionary<string, int>();
    }
}