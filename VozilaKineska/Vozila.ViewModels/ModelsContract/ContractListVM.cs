namespace Vozila.ViewModels.ModelsContract
{
    public class ContractListVM
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string TransporterName { get; set; } = string.Empty;
        public decimal ValueEUR { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public bool IsActive { get; set; }
        public int ConditionCount { get; set; }
    }
}
