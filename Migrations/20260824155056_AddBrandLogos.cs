using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoHub.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandLogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoPath",
                table: "Brands",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql("""
UPDATE Brands SET LogoPath = '/Images/Brands/' + LOWER(Slug) + '.svg'
WHERE LOWER(Slug) IN (
    'audi', 'bmw', 'ford', 'honda', 'hyundai', 'kia', 'mazda',
    'mercedes-benz', 'nissan', 'porsche', 'toyota', 'vinfast',
    'volkswagen', 'volvo'
) AND LogoPath IS NULL;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoPath",
                table: "Brands");
        }
    }
}
