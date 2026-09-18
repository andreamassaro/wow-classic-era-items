using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WowClassicEraItems.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlizzardItemId = table.Column<int>(type: "int", nullable: false),
                    RawJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(400)", nullable: true, computedColumnSql: "JSON_VALUE([RawJson], '$.name.en_US')", stored: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", nullable: true, computedColumnSql: "CAST(JSON_VALUE([RawJson], '$.media.id') AS NVARCHAR(50))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_BlizzardItemId",
                table: "Items",
                column: "BlizzardItemId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");
        }
    }
}
