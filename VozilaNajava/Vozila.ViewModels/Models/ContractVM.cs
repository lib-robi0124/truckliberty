namespace Vozila.ViewModels.Models
{
    public class ContractVM
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public int TransporterId { get; set; }
        public string TransporterName { get; set; } = string.Empty;
        public decimal ValueEUR { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public bool IsActive { get; set; }
        public int DaysUntilExpiry { get; set; }
    }
}
