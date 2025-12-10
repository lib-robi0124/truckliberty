namespace Vozila.ViewModels.Models
{
    public class AdminDashboardVM
    {
        public List<CompanyVM> Companies { get; set; } = new();
        public List<DestinationVM> Destinations { get; set; } = new();
        public List<TransporterVM> Transporters { get; set; } = new();
    }
}
