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
        public DbSet<Destination> Destinations { get; set; } = default!;
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<Transporter> Transporters { get; set; } = default!;
        public DbSet<Company> Companies { get; set; } = default!;
        public DbSet<PriceOil> PriceOils { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.SeedData();

            // ===== Role Configuration =====
            modelBuilder.Entity<Role>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);

            // ===== User Configuration =====
            modelBuilder.Entity<User>()
                .Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(200);
            modelBuilder.Entity<User>()
                .Property(x => x.Password)
                .IsRequired()
                .HasMaxLength(500);
            modelBuilder.Entity<User>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
            modelBuilder.Entity<User>()
                .Property(x => x.IsActive)
                .HasDefaultValue(true);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.FullName);
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasOne(u => u.Transporter)
                .WithMany()
                .HasForeignKey(u => u.TransporterId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // ===== Company Configuration =====
            modelBuilder.Entity<Company>()
                .Property(x => x.CustomerName)
                .IsRequired()
                .HasMaxLength(200);
            modelBuilder.Entity<Company>()
                .Property(x => x.ShipingAddress)
                .HasMaxLength(300);
            modelBuilder.Entity<Company>()
                .Property(x => x.Country)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);
            modelBuilder.Entity<Company>()
                .Property(x => x.City)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(100);
            modelBuilder.Entity<Company>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Company)
                .HasForeignKey(o => o.CompanyId);

            // ===== Transporter Configuration =====
            modelBuilder.Entity<Transporter>()
                .Property(x => x.CompanyName)
                .IsRequired()
                .HasMaxLength(200);
            modelBuilder.Entity<Transporter>()
                .Property(x => x.ContactPerson)
                .HasMaxLength(150);
            modelBuilder.Entity<Transporter>()
                .Property(x => x.PhoneNumber)
                .HasMaxLength(50);
            modelBuilder.Entity<Transporter>()
                .Property(x => x.Email)
                .HasMaxLength(150);
            modelBuilder.Entity<Transporter>()
                .HasMany(t => t.Orders)
                .WithOne(o => o.Transporter)
                .HasForeignKey(o => o.TransporterId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== Contract Configuration =====
            modelBuilder.Entity<Contract>()
                .Property(x => x.ContractNumber)
                .IsRequired()
                .HasMaxLength(100);
            modelBuilder.Entity<Contract>()
                .Property(x => x.ContractOilPrice)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Contract>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
            modelBuilder.Entity<Contract>()
                .Property(x => x.ValidUntil)
                .HasColumnType("datetime2");
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Transporter)
                .WithMany(t => t.Contracts)   // you need to add ICollection<Contract> to Transporter
                .HasForeignKey(c => c.TransporterId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Contract>()
                .HasMany(c => c.Destinations)
                .WithOne(d => d.Contract)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            // ===== Destination Configuration =====
            modelBuilder.Entity<Destination>()
                .Property(x => x.City)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(100);
            modelBuilder.Entity<Destination>()
                .Property(x => x.Country)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);
            modelBuilder.Entity<Destination>()
                .Property(x => x.DailyPricePerLiter)
                .HasPrecision(18, 4);
            modelBuilder.Entity<Destination>()
                .Property(x => x.DestinationContractPrice)
                .HasPrecision(18, 4);
            modelBuilder.Entity<Destination>()
                .Ignore(x => x.ContractOilPrice);
            modelBuilder.Entity<Destination>()
                .Ignore(x => x.DestinationPriceFromFormula);
            
            // ===== Order Configuration =====
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
                .Property(x => x.ContractOilPrice)
                .HasPrecision(18, 4);
            modelBuilder.Entity<Order>()
                .Property(x => x.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);
            modelBuilder.Entity<Order>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
            modelBuilder.Entity<Order>()
                .Property(x => x.TruckSubmittedDate)
                .HasColumnType("datetime2");
            modelBuilder.Entity<Order>()
                .Property(x => x.FinishedDate)
                .HasColumnType("datetime2");
            modelBuilder.Entity<Order>()
                .Property(x => x.CancelledDate)
                .HasColumnType("datetime2");
            modelBuilder.Entity<Order>()
                .Property(x => x.CancelledReason)
                .HasMaxLength(500);
            modelBuilder.Entity<Order>()
                .HasOne(x => x.Company)
                .WithMany(c => c.Orders)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .HasOne(x => x.Transporter)
                .WithMany(t => t.Orders)
                .HasForeignKey(x => x.TransporterId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .HasOne(x => x.Destination)
                .WithMany(d => d.Orders)
                .HasForeignKey(x => x.DestinationId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .HasOne(x => x.CancelledByUser)
                .WithMany(u => u.Orders)
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // ===== PriceOil Configuration =====
            modelBuilder.Entity<PriceOil>()
                .Property(x => x.Date)
                .HasColumnType("date");
            modelBuilder.Entity<PriceOil>()
                .Property(x => x.DailyPricePerLiter)
                .HasPrecision(18, 4);
        }
    }
}
