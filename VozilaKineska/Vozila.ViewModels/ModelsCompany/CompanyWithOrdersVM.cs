using Vozila.ViewModels.Models;

namespace Vozila.ViewModels.ModelsCompany
{
    public class CompanyWithOrdersVM : CompanyVM
    {
        public List<OrderVM> Orders { get; set; } = new List<OrderVM>();
    }
}
