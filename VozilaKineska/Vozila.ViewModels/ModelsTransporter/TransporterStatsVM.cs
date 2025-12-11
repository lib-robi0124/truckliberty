namespace Vozila.ViewModels.ModelsTransporter
{
    public class TransporterStatsVM
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int TotalDestinations { get; set; }
        public int ActiveContracts { get; set; }
        public int PendingOrders { get; set; }
        public int ApprovedOrders { get; set; }
        public int FinishedOrders { get; set; }
    }
}
