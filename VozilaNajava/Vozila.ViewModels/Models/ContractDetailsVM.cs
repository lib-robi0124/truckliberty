namespace Vozila.ViewModels.Models
{
    public class ContractDetailsVM
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string TransporterName { get; set; } = string.Empty;
        public string TransporterEmail { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public bool IsActive { get; set; }
        public List<DestinationVM> Destinations { get; set; } = new();
    }
}
