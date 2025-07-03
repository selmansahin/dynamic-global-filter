using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DynamicGlobalFilter.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Name", "Price", "TenantId" },
                values: new object[,]
                {
                    { 1, "Tenant 1 - Ürün 1", 100m, 1 },
                    { 2, "Tenant 1 - Ürün 2", 200m, 1 },
                    { 3, "Tenant 1 - Ürün 3", 300m, 1 },
                    { 4, "Tenant 2 - Ürün 1", 150m, 2 },
                    { 5, "Tenant 2 - Ürün 2", 250m, 2 },
                    { 6, "Tenant 3 - Ürün 1", 175m, 3 },
                    { 7, "Tenant 3 - Ürün 2", 275m, 3 },
                    { 8, "Tenant 3 - Ürün 3", 375m, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
