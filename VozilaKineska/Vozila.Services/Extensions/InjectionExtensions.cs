using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vozila.DataAccess.DataContext;
using Vozila.DataAccess.Implementations;
using Vozila.DataAccess.Interfaces;

namespace Vozila.Services.Extensions
{
    public static class InjectionExtensions
    {
        public static void InjectDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(option => option.UseSqlServer(connectionString));
        }
        public static void InjectRepositories(this IServiceCollection services)
        {
            // Register generic base repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            
            // Register specific repositories
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ITransporterRepository, TransporterRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IContractRepository, ContractRepository>();
            services.AddScoped<IDestinationRepository, DestinationRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPriceOilRepository, PriceOilRepository>();
        }
        public static void InjectServices(this IServiceCollection services)
        {
            services.AddScoped<Interfaces.IUserService, Implementations.UserService>();
            services.AddScoped<Interfaces.IOrderService, Implementations.OrderService>();
            services.AddScoped<Interfaces.IContractService, Implementations.ContractService>();
            services.AddScoped<Interfaces.ICompanyService, Implementations.CompanyService>();
            services.AddScoped<Interfaces.IDestinationService, Implementations.DestinationService>();
            services.AddScoped<Interfaces.ITransporterService, Implementations.TransporterService>();
            services.AddScoped<Interfaces.IPriceOilService, Implementations.PriceOilService>();
        }

        public static void InjectAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
