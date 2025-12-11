namespace Vozila.ViewModels.ModelsTransporter
{
    public class TransporterListVM
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int DestinationCount { get; set; }
        public int ActiveOrderCount { get; set; }

    }
}
