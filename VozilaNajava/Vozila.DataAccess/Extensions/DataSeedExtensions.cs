using Microsoft.EntityFrameworkCore;
using Vozila.Domain.Enums;
using Vozila.Domain.Models;

namespace Vozila.DataAccess.Extensions
{
    public static class DataSeedExtensions
    {
        public static void SeedData(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
               new Role { Id = 1, Name = "Administrator" },
               new Role { Id = 2, Name = "Transporter" }
           );
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, FullName = "DraganRisteski", Password = "dragan123", RoleId = 1 },
                new User { Id = 2, FullName = "ZoranMultisped", Password = "multisped123", RoleId = 2 },
                new User { Id = 3, FullName = "PeroTimsped", Password = "timsped123", RoleId = 2 }
            );
            modelBuilder.Entity<City>().HasData(
                new City { ID = 1, Name = "Belisce", PostalCode = 10000, Country = Country.HR },
                new City { ID = 2, Name = "Bestovje", PostalCode = 21000, Country = Country.HR },
                new City { ID = 3, Name = "Donji Stupnik", PostalCode = 51000, Country = Country.HR },
                new City { ID = 4, Name = "DJAKOVO", PostalCode = 31000, Country = Country.HR },
                new City { ID = 5, Name = "LIPOVLJANI", PostalCode = 20000, Country = Country.HR }
            );
            modelBuilder.Entity<Destination>().HasData(
               new Destination { Id = 1, CityId = 1, Country = Country.HR, Km = 500.1m, ContractOilPrice = 700.0m },
               new Destination { Id = 2, CityId = 2, Country = Country.HR, Km = 600.4m, ContractOilPrice = 750.0m }
           );
            modelBuilder.Entity<Company>().HasData(
                new Company { Id = 1, CustomerName = "KNAUF GYPSOPIIA S.A.", ShipingAddress = "EYRIPIDOU 10", Country = Country.GR, CityId = 1 },
                new Company { Id = 2, CustomerName = "Transporti d.o.o.", ShipingAddress = "Ulica 2", Country = Country.HR, CityId = 2 },
                new Company { Id = 3, CustomerName = "Logistika d.o.o.", ShipingAddress = "Ulica 3", Country = Country.HR, CityId = 3 }
            );
            modelBuilder.Entity<Transporter>().HasData(
                new Transporter { Id = 1, CompanyName = "MULTISPED", ContactPerson = "Dragan", Address = "ERNEST TELMAN Skopje", PhoneNumber = "123456789" },
                new Transporter { Id = 1, CompanyName = "Фершпед АД Скопје", ContactPerson = "Dragan", Address = "MARSAL TITO 11A Skopje", PhoneNumber = "123456789" },
                new Transporter { Id = 1, CompanyName = "ТИМ ШПЕД ЈВТ ДООЕЛ", ContactPerson = "Dragan", Address = "Bul.A.Makedonski br.9 lok.68 i 70 Skopje", PhoneNumber = "123456789" },
                new Transporter { Id = 1, CompanyName = "Тимс Импекс ДОО", ContactPerson = "Dragan", Address = "IBE PALIKUKA 43 1000 Skopje", PhoneNumber = "123456789" }
            );
            modelBuilder.Entity<PriceOil>().HasData(
                new PriceOil { Id = 1, PricePerLiter = 70.0m, Date = new DateTime(2025, 1, 1) },
                new PriceOil { Id = 2, PricePerLiter = 68.5m, Date = new DateTime(2025, 2, 1) }
            );
            modelBuilder.Entity<Order>().HasData(
                new Order { Id = 1, CompanyId = 1, TransporterId = 1, DestinationId = 1, TruckPlateNo = null, // Transporter fills later
                DateForLoadingFrom = new DateTime(2025, 10, 13, 8, 0, 0), DateForLoadingTo = new DateTime(2025, 10, 13, 10, 0, 0),
                PriceOilFromContractConditions = 275.00m,  Status = OrderStatus.Pending, CreatedDate = new DateTime(2025, 10, 10, 9, 0, 0) },
                new Order { Id = 2, CompanyId = 2, TransporterId = 2, DestinationId = 2, TruckPlateNo = null,
                DateForLoadingFrom = new DateTime(2025, 11, 1, 9, 0, 0), DateForLoadingTo = new DateTime(2025, 11, 1, 12, 0, 0),
                PriceOilFromContractConditions = 340.00m, Status = OrderStatus.Pending, CreatedDate = new DateTime(2025, 10, 28, 14, 30, 0) }
             );
            modelBuilder.Entity<Condition>().HasData(
                new Condition { Id = 1, CityId = 1, PriceOilBase = 1.45m, Km = 250m, PricePerKm = 1.10m, ContractId = 1 },
                new Condition { Id = 2, CityId = 2, PriceOilBase = 1.45m, Km = 400m, PricePerKm = 1.20m, ContractId = 1 },
                new Condition { Id = 3, CityId = 3, PriceOilBase = 1.50m, Km = 300m, PricePerKm = 1.15m, ContractId = 2 }
            );
            modelBuilder.Entity<Contract>().HasData(
                new Contract  { Id = 1, ContractNumber = "CT-2025-001", NameOfTransporter = "TransMax Logistics", CreatedDate = new DateTime(2025, 1, 1),
                    ValidUntil = new DateTime(2026, 1, 1),  ValueEUR = 150000m,  TransporterId = 1 },
                new Contract  { Id = 2, ContractNumber = "CT-2025-002", NameOfTransporter = "EuroCargo Transport", CreatedDate = new DateTime(2025, 3, 15),
                    ValidUntil = new DateTime(2026, 3, 15), ValueEUR = 98000m,   TransporterId = 2 }
            );
        }
    }
}
