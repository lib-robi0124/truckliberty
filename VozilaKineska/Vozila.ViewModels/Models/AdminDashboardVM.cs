using Vozila.ViewModels.ModelsCompany;
using Vozila.ViewModels.ModelsContract;
using Vozila.ViewModels.ModelsDestination;
using Vozila.ViewModels.ModelsTransporter;

namespace Vozila.ViewModels.Models
{
    public class AdminDashboardVM
    {
        // Tabs/Buttons for header
        public string ActiveTab { get; set; } = "PriceOil";
        public List<string> Tabs { get; set; } = new List<string> { "PriceOil", "Contract", "Order" };

        // Left dropdown menu items
        public List<CompanyItem> Companies { get; set; } = new List<CompanyItem>();
        public List<DestinationItem> Destinations { get; set; } = new List<DestinationItem>();
        public List<TransporterItem> Transporters { get; set; } = new List<TransporterItem>();

        // Selected values from dropdowns
        public int? SelectedCompanyId { get; set; }
        public int? SelectedDestinationId { get; set; }
        public int? SelectedTransporterId { get; set; }

        // Data for each tab
        public PriceOilVM PriceOilData { get; set; } = new PriceOilVM();
        public ContractVM ContractData { get; set; } = new ContractVM();
        public OrderVM OrderData { get; set; } = new OrderVM();

        // For search/filter functionality
        public string SearchTerm { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // Dashboard statistics
        public DashboardStats Stats { get; set; } = new DashboardStats();
    }
}
