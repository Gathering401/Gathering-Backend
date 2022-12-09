using Microsoft.EntityFrameworkCore.Migrations;

namespace GatheringAPI.Migrations
{
    public partial class UpdatedGroupSizes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Groups");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Groups",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
