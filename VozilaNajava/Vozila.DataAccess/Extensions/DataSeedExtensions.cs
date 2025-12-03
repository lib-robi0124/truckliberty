using Microsoft.EntityFrameworkCore;
using Vozila.Domain.Enums;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Extensions
{
    public static class DataSeedExtensions
    {
        public static void SeedData(this ModelBuilder modelBuilder)
        {
            // 1. Seed independent tables first (no foreign keys)
            modelBuilder.Entity<Role>().HasData(
               new Role { Id = 1, Name = "Admin" },
               new Role { Id = 2, Name = "Transporter" }
            );

            modelBuilder.Entity<Company>().HasData(
             new Company { Id = 1, CustomerName = "KNAUF GYPSOPIIA S.A.", ShipingAddress = "EYRIPIDOU 10", City = City.Thessaloniki, Country = Country.GR },
             new Company { Id = 2, CustomerName = "Transporti d.o.o.", ShipingAddress = "Ulica 2", City = City.Zagreb, Country = Country.HR },
             new Company { Id = 3, CustomerName = "Logistika d.o.o.", ShipingAddress = "Ulica 3", City = City.Sofia, Country = Country.BG }
            );

            modelBuilder.Entity<Transporter>().HasData(
             new Transporter { Id = 1, CompanyName = "TransLogistika DOOEL", ContactPerson = "Petar Petrovski", PhoneNumber = "+389 70 123 456", Email = "info@translogistika.mk" },
             new Transporter { Id = 2, CompanyName = "Balkan Transport Group", ContactPerson = "Milan Jovanovic", PhoneNumber = "+381 64 987 6543", Email = "office@balkantransport.rs" },
             new Transporter { Id = 3, CompanyName = "EuroCargo Solutions", ContactPerson = "Ivan Petrović", PhoneNumber = "+385 91 332 4422", Email = "contact@eurocargo.hr" }
            );

            modelBuilder.Entity<PriceOil>().HasData(
                new PriceOil { Id = 1, DailyPricePerLiter = 70.0m, Date = new DateTime(2025, 1, 1) },
                new PriceOil { Id = 2, DailyPricePerLiter = 68.5m, Date = new DateTime(2025, 2, 1) }
            );

            // 2. Seed Users (depends on Role and Transporter)
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, FullName = "System Admin", Password = "admin123", RoleId = 1, IsActive = true, CreatedDate = new DateTime(2025, 1, 1, 10, 0, 0), TransporterId = null },
                new User { Id = 2, FullName = "Transporter User", Password = "trans123", RoleId = 2, IsActive = true, CreatedDate = new DateTime(2025, 1, 2, 12, 0, 0), TransporterId = 1 }
            );

            // 3. Seed Contracts (depends on Transporter)
            modelBuilder.Entity<Contract>().HasData(
                new Contract { Id = 1, ContractNumber = "CTR-2025-001", TransporterId = 1, ValueEUR = 15000.00m, CreatedDate = new DateTime(2025, 1, 1, 10, 0, 0), ValidUntil = new DateTime(2026, 1, 1, 0, 0, 0) },
                new Contract { Id = 2, ContractNumber = "CTR-2025-002", TransporterId = 2, ValueEUR = 12000.00m, CreatedDate = new DateTime(2025, 1, 2, 10, 0, 0), ValidUntil = new DateTime(2026, 1, 2, 0, 0, 0) },
                new Contract { Id = 3, ContractNumber = "CTR-2025-003", TransporterId = 3, ValueEUR = 18000.00m, CreatedDate = new DateTime(2025, 1, 3, 10, 0, 0), ValidUntil = new DateTime(2026, 1, 3, 0, 0, 0) }
            );

            // 4. Seed Conditions (depends on Contract)
            modelBuilder.Entity<Condition>().HasData(
                new Condition { Id = 1, ContractId = 1, ContractOilPrice = 70.5m },
                new Condition { Id = 2, ContractId = 2, ContractOilPrice = 69.0m },
                new Condition { Id = 3, ContractId = 3, ContractOilPrice = 71.2m }
            );

            // 5. Seed Destinations (depends on Condition)
            modelBuilder.Entity<Destination>().HasData(
               new Destination { Id = 1, City = City.Ljubljana, Country = Country.Slo, DestinationContractPrice = 150m, ConditionId = 1, DailyPricePerLiter = 70.0m },
               new Destination { Id = 2, City = City.Zagreb, Country = Country.HR, DestinationContractPrice = 120m, ConditionId = 1, DailyPricePerLiter = 70.0m },
               new Destination { Id = 3, City = City.Belgrade, Country = Country.SRB, DestinationContractPrice = 180m, ConditionId = 2, DailyPricePerLiter = 68.5m },
               new Destination { Id = 4, City = City.Bucharest, Country = Country.RO, DestinationContractPrice = 200m, ConditionId = 2, DailyPricePerLiter = 68.5m },
               new Destination { Id = 5, City = City.Sofia, Country = Country.BG, DestinationContractPrice = 175m, ConditionId = 3, DailyPricePerLiter = 71.0m },
               new Destination { Id = 6, City = City.Thessaloniki, Country = Country.GR, DestinationContractPrice = 220m, ConditionId = 3, DailyPricePerLiter = 71.0m },
               new Destination { Id = 7, City = City.Prishtina, Country = Country.RKS, DestinationContractPrice = 160m, ConditionId = 1, DailyPricePerLiter = 70.0m },
               new Destination { Id = 8, City = City.Skopje, Country = Country.MK, DestinationContractPrice = 155m, ConditionId = 2, DailyPricePerLiter = 68.5m }
           );

            // 6. Seed Orders (depends on Company, Transporter, Destination)
            modelBuilder.Entity<Order>().HasData(
                 new Order
                 {
                     Id = 1,
                     CompanyId = 1,
                     TransporterId = 1,
                     DestinationId = 1,
                     TruckPlateNo = null,
                     DateForLoadingFrom = new DateTime(2025, 1, 12, 8, 0, 0),
                     DateForLoadingTo = new DateTime(2025, 1, 12, 16, 0, 0),
                     ContractOilPrice = 70.5m,
                     Status = OrderStatus.Pending,
                     CreatedDate = new DateTime(2025, 1, 10, 9, 30, 0),
                     TruckSubmittedDate = null,
                     FinishedDate = null,
                     CancelledDate = null,
                     CancelledReason = null,
                     CancelledByUserId = null
                 },
                  new Order
                  {
                      Id = 2,
                      CompanyId = 2,
                      TransporterId = 2,
                      DestinationId = 3,
                      TruckPlateNo = null,
                      DateForLoadingFrom = new DateTime(2025, 1, 15, 8, 0, 0),
                      DateForLoadingTo = new DateTime(2025, 1, 15, 16, 0, 0),
                      ContractOilPrice = 69.0m,
                      Status = OrderStatus.Pending,
                      CreatedDate = new DateTime(2025, 1, 11, 10, 15, 0),
                      TruckSubmittedDate = null,
                      FinishedDate = null,
                      CancelledDate = null,
                      CancelledReason = null,
                      CancelledByUserId = null
                  }
             );
        }
    }
}
