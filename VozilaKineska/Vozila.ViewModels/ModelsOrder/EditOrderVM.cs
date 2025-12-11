using System.ComponentModel.DataAnnotations;
using Vozila.Domain.Enums;

namespace Vozila.ViewModels.ModelsOrder
{
    public class EditOrderVM : CreateOrderVM
    {
        public int Id { get; set; }

        [Required]
        public OrderStatus Status { get; set; }
    }
}
