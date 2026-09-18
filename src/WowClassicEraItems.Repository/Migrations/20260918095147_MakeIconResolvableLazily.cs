using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WowClassicEraItems.Repository.Migrations
{
    /// <inheritdoc />
    public partial class MakeIconResolvableLazily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "Items",
                type: "nvarchar(500)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldNullable: true,
                oldComputedColumnSql: "CAST(JSON_VALUE([RawJson], '$.media.id') AS NVARCHAR(50))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "Items",
                type: "nvarchar(50)",
                nullable: true,
                computedColumnSql: "CAST(JSON_VALUE([RawJson], '$.media.id') AS NVARCHAR(50))",
                stored: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldNullable: true);
        }
    }
}
