using AutoMapper;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Models;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.Models;

namespace Vozila.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRepository<Contract> _contractRepository;
        private readonly IRepository<Destination> _destinationRepository;
        private readonly IRepository<Condition> _conditionRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<PriceOil> _priceOilRepository;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            IRepository<Contract> contractRepository,
            IRepository<Destination> destinationRepository,
            IRepository<Condition> conditionRepository,
            IRepository<Order> orderRepository,
            IRepository<PriceOil> priceOilRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _contractRepository = contractRepository;
            _destinationRepository = destinationRepository;
            _conditionRepository = conditionRepository;
            _orderRepository = orderRepository;
            _priceOilRepository = priceOilRepository;
            _mapper = mapper;
        }

        // ---------------------------
        // AUTHENTICATION
        // ---------------------------
        public async Task<UserVM?> ValidateUser(LoginViewModel loginViewModel)
        {
            var user = await _userRepository.AuthenticateAsync(
                loginViewModel.Email, loginViewModel.Password);
            if (user == null) return null;

            await _userRepository.UpdateLastLoginAsync(user.Id);

            // AutoMapper handles User → UserVM
            return _mapper.Map<UserVM>(user);
        }

        // ---------------------------
        // CREATE USER (Admin only)
        // ---------------------------
        public async Task<UserVM> CreateUserAsync(User user, int performedByUserId)
        {
            await EnsureAdmin(performedByUserId);

            bool exists = await _userRepository.UserExistsAsync(user.FullName);
            if (exists)
                throw new Exception("User already exists.");

            var created = await _userRepository.AddAsync(user);
            return Map(created);
        }

        // ---------------------------
        // UPDATE USER (Admin only)
        // ---------------------------
        public async Task<UserVM> UpdateUserAsync(User user, int performedByUserId)
        {
            await EnsureAdmin(performedByUserId);

            var existing = await _userRepository.GetByIdAsync(user.Id)
                          ?? throw new Exception("User not found.");

            existing.FullName = user.FullName;
            existing.RoleId = user.RoleId;
            existing.TransporterId = user.TransporterId;
            existing.IsActive = user.IsActive;

            await _userRepository.UpdateAsync(existing);
            return Map(existing);
        }

        // ---------------------------
        // DELETE USER (Admin only)
        // ---------------------------
        public async Task<bool> DeleteUserAsync(int userId, int performedByUserId)
        {
            await EnsureAdmin(performedByUserId);

            await _userRepository.DeleteAsync(userId);
            return true;
        }

        // ---------------------------
        // GET ALL USERS (Admin only)
        // ---------------------------
        public async Task<List<UserVM>> GetAllUsersAsync(int performedByUserId)
        {
            await EnsureAdmin(performedByUserId);

            var users = await _userRepository.GetAllUsersAsync();
            return users.Select(Map).ToList();
        }

        // ---------------------------
        // GET SINGLE USER
        // ---------------------------
        public async Task<UserVM?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserWithRoleAsync(id);
            return user == null ? null : Map(user);
        }

        // ---------------------------
        // CHANGING PASSWORD
        // ---------------------------
        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            await _userRepository.ChangePasswordAsync(userId, currentPassword, newPassword);
        }

        public async Task ResetPasswordAsync(int adminUserId, int targetUserId, string newPassword)
        {
            await EnsureAdmin(adminUserId);
            await _userRepository.ResetPasswordAsync(targetUserId, newPassword);
        }

        // ---------------------------
        // TRANSPORTER UPDATE ORDER
        // ---------------------------
        public async Task<bool> TransporterUpdateOrderAsync(int transporterUserId, int orderId, string truckPlateNo)
        {
            var user = await _userRepository.GetByIdAsync(transporterUserId)
                       ?? throw new Exception("User not found.");

            if (!await _userRepository.IsUserTransporterAsync(transporterUserId))
                throw new Exception("Only a transporter can update order.");

            var order = await _orderRepository.GetByIdAsync(orderId)
                        ?? throw new Exception("Order not found.");

            // Ensure he updates ONLY his own transporter's orders
            if (order.TransporterId != user.TransporterId)
                throw new Exception("You cannot update orders of another transporter.");

            order.TruckPlateNo = truckPlateNo;
            await _orderRepository.UpdateAsync(order);

            return true;
        }
        // ADMIN: CRUD Contract, Condition, Destination (assuming Admin role check in controller)
        public async Task<Contract> CreateContractAsync(Contract contract)
        {
            return await _contractRepository.AddAsync(contract);
        }

        public async Task UpdateContractAsync(Contract contract)
        {
            await _contractRepository.UpdateAsync(contract);
        }

        public async Task<Destination> CreateDestinationAsync(Destination destination)
        {
            return await _destinationRepository.AddAsync(destination);
        }

        public async Task UpdateDestinationAsync(Destination destination)
        {
            await _destinationRepository.UpdateAsync(destination);
        }

        public async Task<Condition> CreateConditionAsync(Condition condition)
        {
            return await _conditionRepository.AddAsync(condition);
        }

        public async Task UpdateConditionAsync(Condition condition)
        {
            await _conditionRepository.UpdateAsync(condition);
        }
        // ADMIN: CRUD Order
        public async Task<Order> CreateOrderAsync(Order order)
        {
            return await _orderRepository.AddAsync(order);
        }

        public async Task UpdateOrderAsync(Order order)
        {
            await _orderRepository.UpdateAsync(order);
        }

        public async Task<Order?> GetOrderWithUserAsync(int orderId)
        {
            // Assuming repository method exists or implement in UserRepository
            return await _orderRepository.GetByIdAsync(orderId);
        }

        // ADMIN: Update PriceOil
        public async Task UpdatePriceOilAsync(PriceOil priceOil)
        {
            await _priceOilRepository.UpdateAsync(priceOil);
        }

        // ---------------------------
        // HELPERS
        // ---------------------------
        private async Task EnsureAdmin(int userId)
        {
            if (!await _userRepository.IsUserAdminAsync(userId))
                throw new Exception("Only admin can perform this action.");
        }

        private UserVM Map(User u)
        {
            return new UserVM
            {
                Id = u.Id,
                FullName = u.FullName,
                RoleId = u.RoleId,
                RoleName = u.Role?.Name ?? "",
                CreatedDate = u.CreatedDate,
                IsActive = u.IsActive,
                TransporterId = u.TransporterId,
                TransporterName = u.Transporter?.CompanyName
            };
        }
    }

}
