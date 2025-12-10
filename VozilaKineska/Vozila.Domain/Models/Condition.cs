namespace Vozila.Domain.Models
{
    public class Condition
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public Contract Contract { get; set; } = default!;
        public decimal ContractOilPrice { get; set; }
        public ICollection<Destination> Destinations { get; set; } = new List<Destination>();
    }
}
