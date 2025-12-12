namespace Vozila.ViewModels.Models
{
    public class UserVM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public int? TransporterId { get; set; }
        public string? TransporterName { get; set; }
    }
}
