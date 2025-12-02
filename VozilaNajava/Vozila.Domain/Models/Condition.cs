using System.Diagnostics.Contracts;
using Vozila.Domain.Enums;

namespace Vozila.Domain.Models
{
    public class Condition
    {
        public int Id { get; set; }
        public int DestinationId { get; set; }
        public Destination Destination { get; set; } = default!;
        public decimal ContractOilPrice { get; set; } 
        public int ContractId { get; set; }
        public Contract Contract { get; set; } = default!;
    }
}
