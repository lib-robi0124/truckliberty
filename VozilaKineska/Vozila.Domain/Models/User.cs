namespace Vozila.Domain.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public int? TransporterId { get; set; } 
        public Transporter? Transporter { get; set; } 
        public ICollection<Order> Orders { get; set; } 
        public User()
        {
            Orders = new HashSet<Order>();
        }
    }
}
