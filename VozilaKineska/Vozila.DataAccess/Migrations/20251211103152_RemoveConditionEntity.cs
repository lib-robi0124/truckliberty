using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Vozila.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConditionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShipingAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceOils",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    DailyPricePerLiter = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceOils", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transporters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transporters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransporterId = table.Column<int>(type: "int", nullable: false),
                    ContractOilPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Transporters_TransporterId",
                        column: x => x.TransporterId,
                        principalTable: "Transporters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TransporterId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Transporters_TransporterId",
                        column: x => x.TransporterId,
                        principalTable: "Transporters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Destinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DestinationContractPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DailyPricePerLiter = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    TransporterId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Destinations_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Destinations_Transporters_TransporterId",
                        column: x => x.TransporterId,
                        principalTable: "Transporters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TransporterId = table.Column<int>(type: "int", nullable: false),
                    DestinationId = table.Column<int>(type: "int", nullable: false),
                    TruckPlateNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateForLoadingFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateForLoadingTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractOilPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    TruckSubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Destinations_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Destinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Transporters_TransporterId",
                        column: x => x.TransporterId,
                        principalTable: "Transporters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Users_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "City", "Country", "CustomerName", "ShipingAddress" },
                values: new object[,]
                {
                    { 1, "Thessaloniki", "GR", "KNAUF GYPSOPIIA S.A.", "EYRIPIDOU 10" },
                    { 2, "Zagreb", "HR", "Transporti d.o.o.", "Ulica 2" },
                    { 3, "Sofia", "BG", "Logistika d.o.o.", "Ulica 3" }
                });

            migrationBuilder.InsertData(
                table: "PriceOils",
                columns: new[] { "Id", "DailyPricePerLiter", "Date" },
                values: new object[,]
                {
                    { 1, 70.0m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 68.5m, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Transporter" }
                });

            migrationBuilder.InsertData(
                table: "Transporters",
                columns: new[] { "Id", "CompanyName", "ContactPerson", "Email", "Password", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, "TransLogistika DOOEL", "Petar Petrovski", "info@translogistika.mk", "trans123", "+389 70 123 456" },
                    { 2, "Balkan Transport Group", "Milan Jovanovic", "office@balkantransport.rs", "trans123", "+381 64 987 6543" },
                    { 3, "EuroCargo Solutions", "Ivan Petrović", "contact@eurocargo.hr", "trans123", "+385 91 332 4422" }
                });

            migrationBuilder.InsertData(
                table: "Contracts",
                columns: new[] { "Id", "ContractNumber", "ContractOilPrice", "CreatedDate", "TransporterId", "ValidUntil" },
                values: new object[,]
                {
                    { 1, "CTR-2025-001", 1.50m, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "CTR-2025-002", 1.45m, new DateTime(2025, 1, 2, 10, 0, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, "CTR-2025-003", 1.55m, new DateTime(2025, 1, 3, 10, 0, 0, 0, DateTimeKind.Unspecified), 3, new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedDate", "Email", "FullName", "IsActive", "Password", "RoleId", "TransporterId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "admin@liberty.com", "System Admin", true, "admin123", 1, null },
                    { 2, new DateTime(2025, 1, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), "transporter@liberty.com", "Transporter User", true, "trans123", 2, 1 }
                });

            migrationBuilder.InsertData(
                table: "Destinations",
                columns: new[] { "Id", "City", "ContractId", "Country", "DailyPricePerLiter", "DestinationContractPrice", "TransporterId" },
                values: new object[,]
                {
                    { 1, "Ljubljana", 1, "Slo", 70.0m, 150m, null },
                    { 2, "Zagreb", 1, "HR", 70.0m, 120m, null },
                    { 3, "Belgrade", 2, "SRB", 68.5m, 180m, null },
                    { 4, "Bucharest", 2, "RO", 68.5m, 200m, null },
                    { 5, "Sofia", 3, "BG", 71.0m, 175m, null },
                    { 6, "Thessaloniki", 3, "GR", 71.0m, 220m, null },
                    { 7, "Prishtina", 1, "RKS", 70.0m, 160m, null },
                    { 8, "Skopje", 2, "MK", 68.5m, 155m, null }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CancelledByUserId", "CancelledDate", "CancelledReason", "CompanyId", "ContractOilPrice", "CreatedDate", "DateForLoadingFrom", "DateForLoadingTo", "DestinationId", "FinishedDate", "Status", "TransporterId", "TruckPlateNo", "TruckSubmittedDate" },
                values: new object[,]
                {
                    { 1, null, null, null, 1, 70.5m, new DateTime(2025, 1, 10, 9, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 12, 8, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 12, 16, 0, 0, 0, DateTimeKind.Unspecified), 1, null, "Pending", 1, null, null },
                    { 2, null, null, null, 2, 69.0m, new DateTime(2025, 1, 11, 10, 15, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 15, 16, 0, 0, 0, DateTimeKind.Unspecified), 3, null, "Pending", 2, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_TransporterId",
                table: "Contracts",
                column: "TransporterId");

            migrationBuilder.CreateIndex(
                name: "IX_Destinations_ContractId",
                table: "Destinations",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Destinations_TransporterId",
                table: "Destinations",
                column: "TransporterId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CancelledByUserId",
                table: "Orders",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CompanyId",
                table: "Orders",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DestinationId",
                table: "Orders",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TransporterId",
                table: "Orders",
                column: "TransporterId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_FullName",
                table: "Users",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TransporterId",
                table: "Users",
                column: "TransporterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PriceOils");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Destinations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Transporters");
        }
    }
}
