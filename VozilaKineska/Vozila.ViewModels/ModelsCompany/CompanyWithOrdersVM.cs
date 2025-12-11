using Vozila.ViewModels.ModelsOrder;

namespace Vozila.ViewModels.ModelsCompany
{
    public class CompanyWithOrdersVM : CompanyVM
    {
        public List<OrderVM> Orders { get; set; } = new List<OrderVM>();
    }
}
