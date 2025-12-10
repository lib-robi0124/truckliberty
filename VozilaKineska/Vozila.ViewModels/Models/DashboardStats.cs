namespace Vozila.ViewModels.Models
{
    public class DashboardStats
    {
        public int TotalCompanies { get; set; }
        public int TotalDestinations { get; set; }
        public int TotalTransporters { get; set; }
        public int TotalOrdersThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueGrowth { get; set; }
    }
}
