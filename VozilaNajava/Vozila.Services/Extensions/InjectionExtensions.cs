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
            services.AddScoped<ITransporterRepository, TransporterRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IContractRepository, ContractRepository>();
            services.AddScoped<IConditionRepository, ConditionRepository>();
            services.AddScoped<IDestinationRepository, DestinationRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPriceOilRepository, PriceOilRepository>();
        }
        public static void InjectServices(this IServiceCollection services)
        {      
        }

        public static void InjectAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
