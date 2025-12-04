namespace Vozila.ViewModels.Models
{
    public class CreateOrderVM
    {
        public int CompanyId { get; set; }
        public int TransporterId { get; set; }
        public int DestinationId { get; set; }
        public DateTime DateForLoadingFrom { get; set; }
        public DateTime DateForLoadingTo { get; set; }
        public decimal ContractOilPrice { get; set; }
    }
}
