namespace Vozila.ViewModels.Models
{
    public class UserListVM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
