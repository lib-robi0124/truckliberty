namespace Vozila.Domain.Models
{
    public class TransporterOrderStats
    {
        public int TransporterId { get; set; }
        public string TransporterName { get; set; } = string.Empty;
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int FinishedCount { get; set; }
        public int CancelledCount { get; set; }
        public int TotalOrders { get; set; }
        public decimal CompletionRate { get; set; }
    }
}
