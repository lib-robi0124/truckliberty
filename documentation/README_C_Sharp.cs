# truckliberty
this is vehicle loading scheduling app with this proccess: user - administrator is creating contract for transporter with conditions user - administrator logged on app and making order for company, list for transport is drobdown with transporter with lowest price in contract / conditions for that destination app is sending mail Transporter user - transporter logged to app and submitt regplateno to order - status from pending is approved after truck is loaded - dispatchnote will be imported and order will get status finished if transporter do not have truck on 2 days time, order is getting status pending and administrator is changing transporter, next with lowest price in contract
Project/folder layout and architecture notes

Domain entities (C#) — canonical versions (based on your provided models)
VehicleLoadingSchedulingApp/
├─ src/
│ ├─ VehicleApp.Api/ # ASP.NET Core Web API
│ ├─ VehicleApp.Application/ # Services, DTOs, Interfaces
│ ├─ VehicleApp.Domain/ # Entities, enums, domain logic;
 public class User
 {
     public int Id { get; set; }
     public string FullName { get; set; }
     public string Password { get; set; }
     public int RoleId { get; set; }
     public Role Role { get; set; }
     public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
     public ICollection<Order> Orders { get; set; } 
     public User()
     {
         Orders = new HashSet<Order>();
     }
 }
  public class City
 {
     public int ID { get; set; }
     public string Name { get; set; }
     public int PostalCode { get; set; }
     public Country Country { get; set; } // Enum
     public ICollection<Condition> Conditions { get; set; } = new HashSet<Condition>();
     public ICollection<Destination> Destinations { get; set; } = new HashSet<Destination>();
     public ICollection<Company> Companies { get; set; } = new HashSet<Company>();
 }
  public class Destination
 {
     public int Id { get; set; }
     public int CityId { get; set; }
     public City City { get; set; } = default!;
     public Country Country { get; set; }
     public decimal Km { get; set; }
     public decimal CurrentOilPrice { get; set; }   // You will update this daily from PriceOil table
     public decimal ContractOilPrice { get; set; }
     public decimal TransportFullPrice => Km * ContractOilPrice;
     public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
 }
  public class Contract
 {
     public int Id { get; set; }
     public string ContractNumber { get; set; } = string.Empty;
     public string NameOfTransporter { get; set; } = string.Empty;
     public DateTime CreatedDate { get; set; } = DateTime.Now;
     public DateTime ValidUntil { get; set; } = DateTime.Now.AddYears(1);
     public decimal ValueEUR { get; set; }
     public int TransporterId { get; set; }
     public Transporter Transporter { get; set; } = default!;
     public ICollection<Condition> Conditions { get; set; } = new List<Condition>();
 }
   public class Company
  {
      public int Id { get; set; }
      public string CustomerName { get; set; } = string.Empty;
      public string ShipingAddress { get; set; } = string.Empty;
      public Country Country { get; set; } // Enum
      public int CityId { get; set; }
      public City City { get; set; }
      public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
  }
     public class Order
   {
       public int Id { get; set; }
       public int CompanyId { get; set; }
       public Company Company { get; set; } = default!; 
       public int TransporterId { get; set; }
       public Transporter Transporter { get; set; } = default!;
       public int DestinationId { get; set; }
       public Destination Destination { get; set; } = default!;
       public string? TruckPlateNo { get; set; }
       public DateTime DateForLoadingFrom { get; set; }
       public DateTime DateForLoadingTo { get; set; }
       public decimal PriceOilFromContractConditions { get; set; }
       public OrderStatus Status { get; set; } = OrderStatus.Pending; // Enum
       public DateTime CreatedDate { get; set; } = DateTime.Now;
       public DateTime? ApprovedDate { get; set; }
       public DateTime? FinishedDate { get; set; }
   }
    public class Transporter
 {
     public int Id { get; set; }
     public string CompanyName { get; set; } = string.Empty;
     public string ContactPerson { get; set; } = string.Empty;
     public string Address { get; set; } = string.Empty;
     public string PhoneNumber { get; set; } = string.Empty;
     public string Email { get; set; } = string.Empty;
     public ICollection<Contract> Contracts { get; set; } = new HashSet<Contract>();
     public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
 }
│ ├─ VehicleApp.Infrastructure/ # EF Core DbContext, Migrations, Repos
│ └─ VehicleApp.Workers/ # Background services (IHostedService)
├─ tests/
└─ README.md
DbContext and EF Core configuration

Repository & Service patterns + key service logic (choosing transporter with lowest price)
Business Logic — key service: OrderService

Service responsibilities:

Create Order: choose transporter with lowest price for destination (from valid contracts/conditions)

Send notification email to transporter

Transporter submits TruckPlateNo -> mark Approved

On dispatch import -> mark Finished

Background check to auto-reset or reassign if transporter has no truck within 2 days
Background worker descriptions (auto-expire, price submission validity)

Email notification approach

Example API endpoints
