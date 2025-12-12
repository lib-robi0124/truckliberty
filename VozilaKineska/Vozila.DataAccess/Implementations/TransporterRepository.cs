using Microsoft.EntityFrameworkCore;
using Vozila.DataAccess.DataContext;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Implementations
{
    public class TransporterRepository : Repository<Transporter>, ITransporterRepository
    {
        private readonly AppDbContext _context;

        public TransporterRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Transporter?> GetWithDetailsAsync(int id)
        {
            return await _context.Transporters
            .Include(t => t.Destinations)
            .Include(t => t.Contracts)
            .Include(t => t.Orders)
            .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Transporter>> GetAllWithDetailsAsync()
        {
            return await _context.Transporters
            .Include(t => t.Destinations)
            .Include(t => t.Contracts)
            .Include(t => t.Orders)
            .ToListAsync();
        }

        public async Task<IEnumerable<Contract>> GetActiveContractsAsync(int transporterId)
        {
            return await _context.Contracts
                .Where(c => c.TransporterId == transporterId)
                .ToListAsync();
        }
        public async Task<Transporter?> GetByEmailAsync(string email)
        {
            return await _context.Transporters
                .FirstOrDefaultAsync(t => t.Email.ToLower() == email.ToLower());
        }

        public async Task SubmitTruckPlateAsync(int orderId, string truckPlateNo)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
                throw new KeyNotFoundException($"Order {orderId} not found");

            order.TruckPlateNo = truckPlateNo;
            order.TruckSubmittedDate = DateTime.UtcNow;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }

}
