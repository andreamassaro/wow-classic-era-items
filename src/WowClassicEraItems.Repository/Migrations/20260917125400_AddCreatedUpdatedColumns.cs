using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WowClassicEraItems.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedUpdatedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_BlizzardItemId",
                table: "Items");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Items",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "Items",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Items_BlizzardItemId_Updated",
                table: "Items",
                columns: new[] { "BlizzardItemId", "Updated" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_BlizzardItemId_Updated",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "Items");

            migrationBuilder.CreateIndex(
                name: "IX_Items_BlizzardItemId",
                table: "Items",
                column: "BlizzardItemId",
                unique: true);
        }
    }
}
