using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Vozila.DataAccess.DataContext;
using Vozila.Domain.Enums;
using Vozila.ViewModels.Models;

namespace Vozila.DataAccess.Extensions
{
    /// <summary>
    /// Extension methods for repositories to use AutoMapper projections
    /// These methods use ProjectTo for efficient database queries
    /// </summary>
    public static class RepositoryExtensions
    {
        // ===== Order Extensions =====
        
        public static async Task<List<OrderListVM>> GetOrderListAsync(
            this AppDbContext context, 
            IMapper mapper)
        {
            return await context.Orders
                .ProjectTo<OrderListVM>(mapper.ConfigurationProvider)
                .OrderByDescending(o => o.DateForLoadingFrom)
                .ToListAsync();
        }

        public static async Task<List<OrderListVM>> GetOrdersByTransporterAsync(
            this AppDbContext context, 
            IMapper mapper, 
            int transporterId)
        {
            return await context.Orders
                .Where(o => o.TransporterId == transporterId)
                .ProjectTo<OrderListVM>(mapper.ConfigurationProvider)
                .OrderByDescending(o => o.DateForLoadingFrom)
                .ToListAsync();
        }

        public static async Task<List<OrderListVM>> GetOrdersByStatusAsync(
            this AppDbContext context, 
            IMapper mapper, 
            OrderStatus status)
        {
            return await context.Orders
                .Where(o => o.Status == status)
                .ProjectTo<OrderListVM>(mapper.ConfigurationProvider)
                .OrderByDescending(o => o.DateForLoadingFrom)
                .ToListAsync();
        }

        public static async Task<OrderDetailsVM?> GetOrderDetailsAsync(
            this AppDbContext context, 
            IMapper mapper, 
            int orderId)
        {
            return await context.Orders
                .Where(o => o.Id == orderId)
                .ProjectTo<OrderDetailsVM>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        // ===== Contract Extensions =====

        public static async Task<List<ContractListVM>> GetContractListAsync(
            this AppDbContext context, 
            IMapper mapper)
        {
            return await context.Contracts
                .ProjectTo<ContractListVM>(mapper.ConfigurationProvider)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public static async Task<List<ContractListVM>> GetActiveContractsListAsync(
            this AppDbContext context, 
            IMapper mapper)
        {
            var currentDate = DateTime.Now;
            return await context.Contracts
                .Where(c => c.ValidUntil > currentDate)
                .ProjectTo<ContractListVM>(mapper.ConfigurationProvider)
                .OrderByDescending(c => c.ValidUntil)
                .ToListAsync();
        }

        public static async Task<ContractDetailsVM?> GetContractDetailsAsync(
            this AppDbContext context, 
            IMapper mapper, 
            int contractId)
        {
            return await context.Contracts
                .Where(c => c.Id == contractId)
                .ProjectTo<ContractDetailsVM>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        // ===== Destination Extensions =====

        public static async Task<List<DestinationListVM>> GetDestinationListAsync(
            this AppDbContext context, 
            IMapper mapper)
        {
            return await context.Destinations
                .ProjectTo<DestinationListVM>(mapper.ConfigurationProvider)
                .OrderBy(d => d.CityName)
                .ToListAsync();
        }

        public static async Task<List<DestinationListVM>> GetDestinationsByContractAsync(
            this AppDbContext context, 
            IMapper mapper, 
            int contractId)
        {
            return await context.Destinations
                .Where(d => d.Condition.ContractId == contractId)
                .ProjectTo<DestinationListVM>(mapper.ConfigurationProvider)
                .OrderBy(d => d.CityName)
                .ToListAsync();
        }

        public static async Task<DestinationDetailsVM?> GetDestinationDetailsAsync(
            this AppDbContext context, 
            IMapper mapper, 
            int destinationId)
        {
            return await context.Destinations
                .Where(d => d.Id == destinationId)
                .ProjectTo<DestinationDetailsVM>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        // ===== Company Extensions =====

        public static async Task<List<CompanyVM>> GetCompanyListAsync(
            this AppDbContext context, 
            IMapper mapper)
        {
            return await context.Companies
                .ProjectTo<CompanyVM>(mapper.ConfigurationProvider)
                .OrderBy(c => c.CustomerName)
                .ToListAsync();
        }

        // ===== Transporter Extensions =====

        public static async Task<List<TransporterListVM>> GetTransporterListAsync(
            this AppDbContext context, 
            IMapper mapper)
        {
            return await context.Transporters
                .ProjectTo<TransporterListVM>(mapper.ConfigurationProvider)
                .OrderBy(t => t.CompanyName)
                .ToListAsync();
        }

        public static async Task<TransporterStatsVM?> GetTransporterStatsAsync(
            this AppDbContext context, 
            IMapper mapper, 
            int transporterId)
        {
            return await context.Transporters
                .Where(t => t.Id == transporterId)
                .ProjectTo<TransporterStatsVM>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        // ===== User Extensions =====

        public static async Task<List<UserListVM>> GetUserListAsync(
            this AppDbContext context, 
            IMapper mapper)
        {
            return await context.Users
                .ProjectTo<UserListVM>(mapper.ConfigurationProvider)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public static async Task<UserVM?> GetUserDetailsAsync(
            this AppDbContext context, 
            IMapper mapper, 
            int userId)
        {
            return await context.Users
                .Where(u => u.Id == userId)
                .ProjectTo<UserVM>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }
    }
}
