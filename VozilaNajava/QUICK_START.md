# VozilaNajava Quick Start Guide

## Application is Ready! ✅

The truck logistics application is fully configured and ready to run.

## Running the Application

### Option 1: Visual Studio
1. Open `VozilaNajava.sln` in Visual Studio
2. Set `Vozila` project as startup project (if not already)
3. Press F5 or click "Start Debugging"
4. Browser will open at `https://localhost:7129`

### Option 2: Command Line
```powershell
cd D:\proekt-office\vozila\truckliberty\VozilaNajava\Vozila
dotnet run
```
Then navigate to: `https://localhost:7129`

## Login Credentials (Seed Data)

### Admin Account (User table)
- **FullName**: System Admin
- **Password**: admin123
- **Role**: Admin
- **Redirects to**: /Admin/Index
- **Note**: Login with FullName, not email

### Transporter Account #1 (Transporter table)
- **Email**: info@translogistika.mk
- **Password**: trans123
- **Company**: TransLogistika DOOEL
- **Redirects to**: /Transporter/Index

### Other Transporter Accounts
- **Email**: office@balkantransport.rs / **Password**: trans123
- **Email**: contact@eurocargo.hr / **Password**: trans123

## Application Structure

### Routes
- `/` or `/Home/Login` - Login page (default)
- `/Home/Index` - Home page (after login for regular users)
- `/Admin/Index` - Admin dashboard
- `/Transporter/Index` - Transporter dashboard

### Database
- **Server**: Local SQL Server (.)
- **Database**: TruckTestDb
- **Connection**: Windows Authentication (Trusted_Connection)
- **Migration**: Already applied with seed data

## What's Working

✅ Database migrations applied
✅ Seed data loaded (roles, users, companies, transporters, contracts, conditions, destinations, orders)
✅ Identity authentication configured
✅ Session management enabled
✅ All repositories registered (8 specific + generic)
✅ All services registered (8 services)
✅ AutoMapper configured (ViewModels + Services profiles)
✅ Login view created
✅ Role-based routing configured
✅ Application builds without errors

## Next Steps for Development

1. **Complete Admin Views**: Build out Admin/Index.cshtml with admin functionality
2. **Complete Transporter Views**: Build out Transporter/Index.cshtml with order management
3. **Add Logout**: Add logout action and link in navigation (_Layout.cshtml)
4. **Order Management**: Create views for OrderController (if exists) or create controller for CRUD operations
5. **Authorization**: Add [Authorize] attributes to controllers/actions as needed

## Architecture Overview

```
Domain Models → Repositories → Services → ViewModels → Controllers → Views
     ↓              ↓             ↓           ↓            ↓          ↓
  Entities    IRepository<T>  IService    Display/    MVC        Razor
              + Specific      Logic      Input VMs   Actions     Pages
              interfaces
```

## Troubleshooting

### "No web page found" Error
- **Fixed**: Login.cshtml created and default route updated

### Database Connection Issues
- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure Windows Authentication is enabled

### Build Warnings
- NU1701: Old Identity package - non-critical, works with .NET 8
- CS8603/CS8625/CS8604: Nullability warnings - non-critical, app functions correctly

## Seed Data Summary

- **Roles**: Admin, Transporter
- **Companies**: 3 companies (CMP-001 through CMP-003)
- **Transporters**: 3 transporters
- **Contracts**: 3 contracts with conditions
- **Destinations**: 8 destinations across contracts
- **Orders**: 2 sample orders (1 pending, 1 in transit)
- **Oil Prices**: 2 historical oil price records

## Documentation

- **CONTROLLER_READINESS_REPORT.md**: Comprehensive architecture analysis
- **STARTUP_FIX.md**: DI and session configuration details
- **This file (QUICK_START.md)**: Quick reference guide
