using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vozila.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePasswordsToHashed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Transporters",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "srtnHMMB9RVC7JR+iyMhp5DhcAhNa7nERsyVDRCrLZc=");

            migrationBuilder.UpdateData(
                table: "Transporters",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "srtnHMMB9RVC7JR+iyMhp5DhcAhNa7nERsyVDRCrLZc=");

            migrationBuilder.UpdateData(
                table: "Transporters",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "srtnHMMB9RVC7JR+iyMhp5DhcAhNa7nERsyVDRCrLZc=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "srtnHMMB9RVC7JR+iyMhp5DhcAhNa7nERsyVDRCrLZc=");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Transporters",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "trans123");

            migrationBuilder.UpdateData(
                table: "Transporters",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "trans123");

            migrationBuilder.UpdateData(
                table: "Transporters",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "trans123");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "admin123");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "trans123");
        }
    }
}
