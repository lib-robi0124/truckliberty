using Vozila.Domain.Models;

namespace Vozila.ViewModels.ModelsTransporter
{
    public class TransporterOrderSearchVM : OrderSearchCriteria
    {
        // Inherits all properties from ViewModel
        // TransporterId will be set automatically in controller
        public int TransporterId { get; set; }
    }
}
