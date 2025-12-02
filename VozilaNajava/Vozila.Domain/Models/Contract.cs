namespace Vozila.Domain.Models
{
    public class Contract
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public int TransporterId { get; set; }
        public Transporter Transporter { get; set; } = default!;
        public decimal ValueEUR { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ValidUntil { get; set; } = DateTime.Now.AddYears(1);
        public ICollection<Condition> Conditions { get; set; } = new List<Condition>();
        public ICollection<Destination> Destinations { get; set; } = new List<Destination>(); 

    }
}
