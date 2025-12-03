using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using System.Security.Cryptography;
using System.Text;
using Vozila.DataAccess.DataContext;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)   {  }

        // ========== CRUD Overrides ==========

        public override async Task<User?> GetByIdAsync(int id)
        {
            return await _entities
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public override async Task<User?> GetActiveAsync(int id)
        {
               return await GetByIdAsync(id);
        }

        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _entities
                .Include(u => u.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public override async Task<IEnumerable<User>> GetAllActiveAsync()
        {
            return await GetAllAsync();
        }

        public override async Task<User> AddAsync(User entity)
        {
            // Validate FullName uniqueness
            var userExists = await _entities
                .AnyAsync(u => u.FullName == entity.FullName);

            if (userExists)
                throw new InvalidOperationException($"User with full name '{entity.FullName}' already exists");

            // Validate Role exists
            var roleExists = await _context.Set<Role>()
                .AnyAsync(r => r.Id == entity.RoleId);

            if (!roleExists)
                throw new InvalidOperationException($"Role {entity.RoleId} not found");

            // Hash password before saving
            entity.Password = HashPassword(entity.Password);
            entity.CreatedDate = DateTime.UtcNow;

            return await base.AddAsync(entity);
        }

        public override async Task UpdateAsync(User entity)
        {
            var existing = await GetByIdAsync(entity.Id);
            if (existing == null)
                throw new NotFoundException($"User {entity.Id} not found");

            // Prevent changing certain properties
            entity.CreatedDate = existing.CreatedDate;
            entity.Password = existing.Password; // Password should be changed via ChangePasswordAsync

            // Validate FullName uniqueness (if changed)
            if (entity.FullName != existing.FullName)
            {
                var userExists = await _entities
                    .AnyAsync(u => u.FullName == entity.FullName && u.Id != entity.Id);

                if (userExists)
                    throw new InvalidOperationException($"User with full name '{entity.FullName}' already exists");
            }

            // Validate Role exists (if changed)
            if (entity.RoleId != existing.RoleId)
            {
                var roleExists = await _context.Set<Role>()
                    .AnyAsync(r => r.Id == entity.RoleId);

                if (!roleExists)
                    throw new InvalidOperationException($"Role {entity.RoleId} not found");
            }

            await base.UpdateAsync(entity);
        }

        public override async Task DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
                throw new NotFoundException($"User {id} not found");

            // Check if user has associated orders
            var hasOrders = await _context.Set<Order>()
                .AnyAsync(o => EF.Property<int?>(o, "UserId") == id); // Assuming Order has UserId foreign key

            if (hasOrders)
            {
                throw new InvalidOperationException($"Cannot delete user {id} because they have associated orders");
            }

            await base.DeleteAsync(id);
        }

        // ========== AUTHENTICATION METHODS ==========

        public async Task<User?> AuthenticateAsync(string fullName, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _entities
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.FullName == fullName);

            if (user == null)
                return null;

            // Verify password
            if (!VerifyPassword(password, user.Password))
                return null;

            // Update last login time
            await UpdateLastLoginAsync(user.Id);

            return user;
        }

        // ========== USER-SPECIFIC QUERIES ==========

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _entities
                .Include(u => u.Role)
                .Include(u => u.Orders)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<User?> GetUserWithRoleAsync(int userId)
        {
            return await _entities
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserWithOrdersAsync(int userId)
        {
            return await _entities
                .Include(u => u.Role)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.Destination)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.Transporter)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<User>> GetUsersByRoleAsync(int roleId)
        {
            return await _entities
                .Include(u => u.Role)
                .Where(u => u.RoleId == roleId)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<List<User>> GetActiveUsersAsync()
        {
            // If implementing soft delete, add: .Where(u => u.IsActive)
            return await _entities
                .Include(u => u.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<bool> UserExistsAsync(string fullName)
        {
            return await _entities
                .AnyAsync(u => u.FullName == fullName);
        }

        // ========== ROLE-BASED METHODS ==========

        public async Task<bool> IsUserAdminAsync(int userId)
        {
            var user = await GetUserWithRoleAsync(userId);
            return user?.Role?.Name == "Administrator";
        }

        public async Task<bool> IsUserTransporterAsync(int userId)
        {
            var user = await GetUserWithRoleAsync(userId);
            return user?.Role?.Name == "Transporter";
        }

        public async Task<int> GetUserTransporterIdAsync(int userId)
        {
             var user = await _entities
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Role?.Name != "Transporter")
                throw new InvalidOperationException($"User {userId} is not a Transporter");


            return await _context.Set<Transporter>()
                .Where(t => t.ContactPerson == user.FullName)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();

        }

        // ========== PASSWORD MANAGEMENT ==========

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"User {userId} not found");

            // Verify current password
            if (!VerifyPassword(currentPassword, user.Password))
                throw new InvalidOperationException("Current password is incorrect");

            // Update password
            user.Password = HashPassword(newPassword);
            await UpdateAsync(user);
        }

        public async Task ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"User {userId} not found");

            // Only administrators can reset passwords (add authorization check in service layer)
            user.Password = HashPassword(newPassword);
            await UpdateAsync(user);
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return;
            var entry = _context.Entry(user);
        }

        // ========== ADDITIONAL HELPER METHODS ==========

        public async Task<List<User>> SearchUsersAsync(
            string? searchTerm = null,
            int? roleId = null,
            DateTime? createdFrom = null,
            DateTime? createdTo = null)
        {
            var query = _entities
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(searchTerm));
            }

            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            if (createdFrom.HasValue)
            {
                query = query.Where(u => u.CreatedDate >= createdFrom.Value);
            }

            if (createdTo.HasValue)
            {
                query = query.Where(u => u.CreatedDate <= createdTo.Value);
            }

            return await query
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<int> GetUserCountByRoleAsync(int roleId)
        {
            return await _entities
                .CountAsync(u => u.RoleId == roleId);
        }

        public async Task<Dictionary<int, int>> GetUserCountByRoleAsync()
        {
            return await _entities
                .GroupBy(u => u.RoleId)
                .Select(g => new { RoleId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RoleId, x => x.Count);
        }

        // ========== PASSWORD HASHING UTILITIES ==========

        private string HashPassword(string password)
        {
            // Use a proper password hashing algorithm like bcrypt, PBKDF2, or Argon2
            // For production, use: BCrypt.Net.BCrypt.HashPassword(password)

            // Simple implementation for demo (NOT FOR PRODUCTION)
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);

            // For production, install BCrypt.Net-Next and use:
            // return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            // Simple implementation for demo (NOT FOR PRODUCTION)
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            var hashedInput = Convert.ToBase64String(hash);
            return hashedInput == hashedPassword;

            // For production with BCrypt:
            // return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
