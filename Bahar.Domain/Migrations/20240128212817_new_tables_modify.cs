using Microsoft.EntityFrameworkCore.Migrations;

namespace Bahar.Domain.Migrations
{
    public partial class new_tables_modify : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSavedBefore",
                table: "WebScrapeLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSavedBefore",
                table: "WebScrapeLogs");
        }
    }
}
