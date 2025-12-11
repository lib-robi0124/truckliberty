using Vozila.Domain.Models;

namespace Vozila.ViewModels.ModelsOrder
{
    public class OrderAuditLog
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Action { get; set; } = string.Empty; // "Create", "Update", "Cancel", "Finish"
        public int ChangedByUserId { get; set; }
        public DateTime ChangedDate { get; set; }
        public string OldValues { get; set; } = string.Empty; // JSON serialized old values
        public string NewValues { get; set; } = string.Empty; // JSON serialized new values
        public string? Notes { get; set; }

        // Navigation properties
        public Order Order { get; set; } = default!;
        public User ChangedByUser { get; set; } = default!;
    }
}
