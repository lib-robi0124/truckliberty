namespace Vozila.ViewModels.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string CompanyName { get; set; }
        public string Destination { get; set; }
        public string Transporter { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
