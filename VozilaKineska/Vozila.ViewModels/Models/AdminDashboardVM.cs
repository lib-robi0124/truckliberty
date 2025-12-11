using Vozila.ViewModels.ModelsCompany;
using Vozila.ViewModels.ModelsDestination;
using Vozila.ViewModels.ModelsTransporter;

namespace Vozila.ViewModels.Models
{
    public class AdminDashboardVM
    {
        public List<CompanyVM> Companies { get; set; } = new();
        public List<DestinationVM> Destinations { get; set; } = new();
        public List<TransporterVM> Transporters { get; set; } = new();
    }
}
