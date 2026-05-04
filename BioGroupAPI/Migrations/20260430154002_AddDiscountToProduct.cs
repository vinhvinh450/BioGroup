using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BioGroupAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountEndDate",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountPercent",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DiscountEndDate", "DiscountPercent" },
                values: new object[] { new DateTime(2026, 4, 30, 15, 40, 0, 837, DateTimeKind.Utc).AddTicks(2515), null, 0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DiscountEndDate", "DiscountPercent" },
                values: new object[] { new DateTime(2026, 4, 30, 15, 40, 0, 837, DateTimeKind.Utc).AddTicks(2526), null, 0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 30, 15, 40, 0, 837, DateTimeKind.Utc).AddTicks(2674));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountEndDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 24, 16, 47, 18, 464, DateTimeKind.Utc).AddTicks(2651));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 24, 16, 47, 18, 464, DateTimeKind.Utc).AddTicks(2659));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 24, 16, 47, 18, 464, DateTimeKind.Utc).AddTicks(2808));
        }
    }
}
