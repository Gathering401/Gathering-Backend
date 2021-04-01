using Microsoft.EntityFrameworkCore.Migrations;

namespace GatheringAPI.Migrations
{
    public partial class EventIdAutoGeneration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventRepeatId",
                table: "Events");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "EventRepeatId",
                table: "Events",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
