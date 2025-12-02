# truckliberty
this is vehicle loading scheduling app with this proccess: user - administrator is creating contract for transporter with conditions user - administrator logged on app and making order for company, list for transport is drobdown with transporter with lowest price in contract / conditions for that destination app is sending mail Transporter user - transporter logged to app and submitt regplateno to order - status from pending is approved after truck is loaded - dispatchnote will be imported and order will get status finished if transporter do not have truck on 2 days time, order is getting status pending and administrator is changing transporter, next with lowest price in contract
Project/folder layout and architecture notes

Domain entities (C#) — canonical versions (based on your provided models)
VehicleLoadingSchedulingApp/
├─ src/
│ ├─ VehicleApp.Api/ # ASP.NET Core Web API
│ ├─ VehicleApp.Application/ # Services, DTOs, Interfaces
│ ├─ VehicleApp.Domain/ # Entities, enums, domain logic
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
