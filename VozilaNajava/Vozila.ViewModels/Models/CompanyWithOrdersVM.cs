namespace Vozila.ViewModels.Models
{
    public class CompanyWithOrdersVM : CompanyVM
    {
        public List<OrderVM> Orders { get; set; } = new List<OrderVM>();
    }
}
