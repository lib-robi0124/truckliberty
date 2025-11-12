namespace Vozila.Domain.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public ICollection<Order> Orders { get; set; } 
        public User()
        {
            Orders = new HashSet<Order>();
        }
    }
}
