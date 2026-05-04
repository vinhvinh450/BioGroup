using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BioGroupAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2026, 4, 24, 16, 47, 18, 464, DateTimeKind.Utc).AddTicks(2651), true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2026, 4, 24, 16, 47, 18, 464, DateTimeKind.Utc).AddTicks(2659), true });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 24, 16, 47, 18, 464, DateTimeKind.Utc).AddTicks(2808));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 23, 8, 13, 7, 500, DateTimeKind.Utc).AddTicks(7792));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 23, 8, 13, 7, 500, DateTimeKind.Utc).AddTicks(7800));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 23, 8, 13, 7, 500, DateTimeKind.Utc).AddTicks(7963));
        }
    }
}
