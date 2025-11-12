using Microsoft.EntityFrameworkCore;
using Vozila.DataAccess.Extensions;
using Vozila.Domain.Enums;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.DataContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<Contract> Contracts { get; set; } = default!;
        public DbSet<Condition> Conditions { get; set; } = default!;
        public DbSet<Destination> Destinations { get; set; } = default!;
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<Transporter> Transporters { get; set; } = default!;
        public DbSet<Company> Companies { get; set; } = default!;
        public DbSet<City> Cities { get; set; } = default!;
        public DbSet<PriceOil> PriceOils { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<City>()
                    .HasKey(x => x.ID);
            modelBuilder.Entity<City>()
                    .Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);
            modelBuilder.Entity<City>()
                    .Property(x => x.PostalCode)
                    .IsRequired();
            modelBuilder.Entity<City>()
                    .Property(x => x.Destination)
                    .HasConversion<string>()
                    .HasMaxLength(10);

            modelBuilder.Entity<Company>()
                    .Property(x => x.CustomerName)
                    .IsRequired()
                    .HasMaxLength(200);
            modelBuilder.Entity<Company>()
                    .Property(x => x.ShipingAddress)
                    .HasMaxLength(300);
            modelBuilder.Entity<Company>()
                    .Property<int>("CityId");
            modelBuilder.Entity<Company>()
                    .HasOne(x => x.City)
                    .WithMany()
                    .HasForeignKey("CityId")
                    .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Company>()
                   .Property(x => x.Country)
                   .HasConversion<string>()
                   .HasMaxLength(10);
            
            modelBuilder.Entity<Transporter>()
                    .Property(x => x.CompanyName)
                    .IsRequired()
                    .HasMaxLength(200);
            modelBuilder.Entity<Transporter>()
                    .Property(x => x.ContactPerson)
                    .HasMaxLength(150);
            modelBuilder.Entity<Transporter>()
                    .Property(x => x.Address)
                    .HasMaxLength(300);
            modelBuilder.Entity<Transporter>()
                    .Property(x => x.PhoneNumber)
                    .HasMaxLength(50);
            modelBuilder.Entity<Transporter>()
                    .Property(x => x.Email)
                    .HasMaxLength(150);
            
            modelBuilder.Entity<Contract>()
                    .Property(x => x.ContractNumber)
                    .HasMaxLength(100);
            modelBuilder.Entity<Contract>()
                    .Property(x => x.NameOfTransporter)
                    .HasMaxLength(100);
            modelBuilder.Entity<Contract>()
                    .Property(x => x.ValueEUR)
                    .HasPrecision(18, 2);
            modelBuilder.Entity<Contract>()
                    .Property(x => x.CreatedDate)
                    .HasDefaultValueSql("GETUTCDATE()");
            modelBuilder.Entity<Contract>()
                    .Property(x => x.ValidUntil)
                    .HasColumnType("datetime2");
            modelBuilder.Entity<Contract>()
                    .HasOne(x => x.Transporter)
                    .WithMany(t => t.Contracts)
                    .HasForeignKey(x => x.TransporterId)
                    .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Contract>()
                    .HasMany(c => c.Conditions)
                    .WithOne(c => c.Contract)
                    .HasForeignKey(c => c.ContractId)
                    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Condition>()
                   .HasOne(c => c.Contract)
                   .WithMany(c => c.Conditions)
                   .HasForeignKey(c => c.ContractId);
            modelBuilder.Entity<Condition>()
                    .HasOne(x => x.City)
                    .WithMany(c => c.Conditions)
                    .HasForeignKey(x => x.CityId)
                    .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Condition>()
                    .Property(x => x.Km)
                    .HasPrecision(9, 2);
            modelBuilder.Entity<Condition>()
                   .Property(x => x.PricePerKm)
                   .HasPrecision(18, 4);
            modelBuilder.Entity<Condition>()
                   .Property(x => x.PriceOilBase)
                   .HasPrecision(18, 4);
            modelBuilder.Entity<Condition>()
                  .Ignore(x => x.FullPrice);
           

            modelBuilder.Entity<Destination>()
                    .HasOne(x => x.City)
                    .WithMany()
                    .HasForeignKey(x => x.CityId)
                    .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Destination>()
                   .Property(x => x.Km)
                   .HasPrecision(9, 2);
            modelBuilder.Entity<Destination>()
                   .Property(x => x.CurrentOilPrice)
                   .HasPrecision(18, 4);
            modelBuilder.Entity<Destination>()
                   .Property(x => x.ContractOilPrice)
                   .HasPrecision(18, 4);
            modelBuilder.Entity<Destination>()
                   .Property(x => x.Country)
                   .HasConversion<string>()
                   .HasMaxLength(10);
            modelBuilder.Entity<Destination>()
                   .Ignore(x => x.TransportFullPrice);
            

            modelBuilder.Entity<Order>()
                    .HasOne(x => x.Company)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Order>()
                    .HasOne(x => x.Transporter)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(x => x.TransporterId)
                    .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Order>()
                    .HasOne(x => x.Destination)
                    .WithMany(d => d.Orders)
                    .HasForeignKey(x => x.DestinationId)
                    .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Order>()
                    .Property(x => x.TruckPlateNo)
                    .HasMaxLength(50);
            modelBuilder.Entity<Order>()
                    .Property(x => x.DateForLoadingFrom)
                    .HasColumnType("datetime2");
            modelBuilder.Entity<Order>()
                    .Property(x => x.DateForLoadingTo)
                    .HasColumnType("datetime2");
            modelBuilder.Entity<Order>()
                    .Property(x => x.PriceOilFromContractConditions)
                    .HasPrecision(18, 4);
            modelBuilder.Entity<Order>()
                    .Property(x => x.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            modelBuilder.Entity<Order>()
                   .Property(o => o.Status)
                   .HasDefaultValue(OrderStatus.Pending);
            modelBuilder.Entity<Order>()
                   .Property(x => x.CreatedDate)
                   .HasDefaultValueSql("GETUTCDATE()");
            
            // configure relationships, indexes, default values
            modelBuilder.Entity<User>()
                .HasIndex(u => u.FullName);
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<PriceOil>()
                .Property(x => x.Date)
                .HasColumnType("date");
            modelBuilder.Entity<PriceOil>()
                .Property(x => x.PricePerLiter)
                .HasPrecision(18, 4);

            // Seed initial data
            modelBuilder.SeedData();
        }
    }
}
