namespace Vozila.Domain.Models
{
    public class Transporter
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public ICollection<Contract> Contracts { get; set; } = new HashSet<Contract>();
        public ICollection<Destination> Destinations { get; set; } = new HashSet<Destination>();
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
