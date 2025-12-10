namespace Vozila.ViewModels.ModelsContract
{
    public class ContractItem
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; }
        public string CompanyName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public decimal TotalValue { get; set; }
    }
}
