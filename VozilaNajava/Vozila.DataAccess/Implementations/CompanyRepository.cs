using Microsoft.EntityFrameworkCore;
using Vozila.DataAccess.DataContext;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Enums;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Implementations
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly AppDbContext _context;

        public CompanyRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Company?> GetWithOrdersAsync(int id)
        {
            return await _context.Companies
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Company>> GetAllWithOrdersAsync()
        {
            return await _context.Companies
                .Include(c => c.Orders)
                .ToListAsync();
        }

        public async Task<IEnumerable<Company>> GetByCityAsync(City city)
        {
            return await _context.Companies
                .Where(c => c.City == city)
                .Include(c => c.Orders)
                .ToListAsync();
        }

        public async Task<IEnumerable<Company>> GetByCountryAsync(Country country)
        {
            return await _context.Companies
                .Where(c => c.Country == country)
                .Include(c => c.Orders)
                .ToListAsync();
        }
    }
}
