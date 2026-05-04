using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BioGroupAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStockToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Stock" },
                values: new object[] { new DateTime(2026, 4, 23, 8, 13, 7, 500, DateTimeKind.Utc).AddTicks(7792), 0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Stock" },
                values: new object[] { new DateTime(2026, 4, 23, 8, 13, 7, 500, DateTimeKind.Utc).AddTicks(7800), 0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 23, 8, 13, 7, 500, DateTimeKind.Utc).AddTicks(7963));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 23, 2, 4, 3, 868, DateTimeKind.Utc).AddTicks(3509));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 23, 2, 4, 3, 868, DateTimeKind.Utc).AddTicks(3517));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 23, 2, 4, 3, 868, DateTimeKind.Utc).AddTicks(3702));
        }
    }
}
