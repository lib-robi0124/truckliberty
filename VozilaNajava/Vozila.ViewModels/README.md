# ViewModels and AutoMapper Configuration

## Overview

This project uses **AutoMapper** with **LINQ Projections** to efficiently map domain entities to ViewModels, reducing database queries and improving performance.

## Structure

- **ViewModels.cs** - All ViewModel definitions
- **MappingProfile.cs** - AutoMapper configuration with projection support
- **RepositoryExtensions.cs** - Extension methods for efficient querying

## Key Benefits

### 1. **ProjectTo<T>()** - Efficient Database Queries
Instead of loading entire entities with `.Include()` and then mapping, `ProjectTo` generates optimized SQL that only selects needed columns.

#### ❌ Old Way (Inefficient)
```csharp
// Loads ALL fields from Orders, Company, Transporter, Destination
var orders = await _context.Orders
    .Include(o => o.Company)
    .Include(o => o.Transporter)
    .Include(o => o.Destination)
    .ToListAsync();

// Then map in memory
var viewModels = _mapper.Map<List<OrderListViewModel>>(orders);
```

#### ✅ New Way (Efficient with ProjectTo)
```csharp
// Only selects fields needed for OrderListViewModel
var viewModels = await _context.Orders
    .ProjectTo<OrderListViewModel>(_mapper.ConfigurationProvider)
    .ToListAsync();
```

### 2. **Computed Properties in ViewModels**
All calculations (like `CalculatedPrice`, `IsActive`, `CanSubmitTruck`) are done in the mapping profile and translated to SQL.

## Usage Examples

### In Controllers

```csharp
using AutoMapper;
using Vozila.DataAccess.DataContext;
using Vozila.DataAccess.Extensions;
using Vozila.ViewModels;

public class OrdersController : Controller
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public OrdersController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // List all orders (efficient)
    public async Task<IActionResult> Index()
    {
        var orders = await _context.GetOrderListAsync(_mapper);
        return View(orders);
    }

    // Get order details
    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.GetOrderDetailsAsync(_mapper, id);
        if (order == null) return NotFound();
        return View(order);
    }

    // Filter by transporter
    public async Task<IActionResult> ByTransporter(int transporterId)
    {
        var orders = await _context.GetOrdersByTransporterAsync(_mapper, transporterId);
        return View("Index", orders);
    }
}
```

### In Services

```csharp
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

public class OrderService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public OrderService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Custom query with projection
    public async Task<List<OrderListViewModel>> GetPendingOrdersAsync()
    {
        return await _context.Orders
            .Where(o => o.Status == OrderStatus.Pending)
            .ProjectTo<OrderListViewModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    // Paginated results
    public async Task<List<OrderListViewModel>> GetPagedOrdersAsync(int page, int pageSize)
    {
        return await _context.Orders
            .OrderByDescending(o => o.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<OrderListViewModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    // Search with filters
    public async Task<List<OrderListViewModel>> SearchOrdersAsync(
        string? searchTerm, 
        OrderStatus? status,
        DateTime? fromDate)
    {
        var query = _context.Orders.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(o => o.Company.CustomerName.Contains(searchTerm) ||
                                     o.Transporter.CompanyName.Contains(searchTerm));

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(o => o.CreatedDate >= fromDate.Value);

        return await query
            .ProjectTo<OrderListViewModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}
```

### Direct Mapping (when you already have entities)

```csharp
// Map single entity
var order = await _context.Orders.FindAsync(id);
var viewModel = _mapper.Map<OrderViewModel>(order);

// Map collection
var orders = await _context.Orders.ToListAsync();
var viewModels = _mapper.Map<List<OrderViewModel>>(orders);
```

## Available ViewModels

### Orders
- `OrderViewModel` - Basic order info
- `OrderListViewModel` - Lightweight for lists
- `OrderDetailsViewModel` - Full details with computed fields

### Contracts
- `ContractViewModel` - Basic contract info
- `ContractListViewModel` - For lists with counts
- `ContractDetailsViewModel` - Full details with conditions

### Destinations
- `DestinationViewModel` - Basic destination info
- `DestinationListViewModel` - For lists with calculated prices
- `DestinationDetailsViewModel` - Full details with contract info

### Transporters
- `TransporterViewModel` - Basic transporter info
- `TransporterListViewModel` - With counts
- `TransporterStatsViewModel` - Full statistics

### Others
- `CompanyViewModel`
- `UserViewModel`, `UserListViewModel`
- `RoleViewModel`
- `PriceOilViewModel`, `PriceOilHistoryViewModel`
- `ConditionViewModel`

## Performance Tips

1. **Use ProjectTo for lists** - It generates optimal SQL
2. **Use specific ViewModels** - Don't load more data than needed
3. **Filter before ProjectTo** - Apply `.Where()` before `.ProjectTo()`
4. **Avoid N+1 queries** - ProjectTo handles joins automatically

## Example Generated SQL

### With ProjectTo (Optimized)
```sql
SELECT 
    o.Id, 
    c.CustomerName, 
    t.CompanyName, 
    d.City,
    o.Status,
    o.DateForLoadingFrom
FROM Orders o
INNER JOIN Companies c ON o.CompanyId = c.Id
INNER JOIN Transporters t ON o.TransporterId = t.Id
INNER JOIN Destinations d ON o.DestinationId = d.Id
```

### Without ProjectTo (Loads everything)
```sql
SELECT * FROM Orders
SELECT * FROM Companies WHERE Id IN (...)
SELECT * FROM Transporters WHERE Id IN (...)
SELECT * FROM Destinations WHERE Id IN (...)
```

## Configuration

AutoMapper is configured in `Program.cs`:

```csharp
builder.Services.AddAutoMapper(typeof(Vozila.ViewModels.MappingProfile));
```

All mappings are defined in `MappingProfile.cs` with support for:
- Computed properties
- Enum to string conversions
- Nested relationships
- Conditional mappings
