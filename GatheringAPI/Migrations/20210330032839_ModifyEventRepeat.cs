using Microsoft.EntityFrameworkCore.Migrations;

namespace GatheringAPI.Migrations
{
    public partial class ModifyEventRepeat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventRepeats_Events_EventId1",
                table: "EventRepeats");

            migrationBuilder.DropIndex(
                name: "IX_EventRepeats_EventId1",
                table: "EventRepeats");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "EventRepeats");

            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "EventRepeats");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "EventRepeats",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventName",
                table: "EventRepeats",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "EventRepeats",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "EventRepeats");

            migrationBuilder.DropColumn(
                name: "EventName",
                table: "EventRepeats");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "EventRepeats");

            migrationBuilder.AddColumn<long>(
                name: "EventId",
                table: "EventRepeats",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "EventId1",
                table: "EventRepeats",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventRepeats_EventId1",
                table: "EventRepeats",
                column: "EventId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EventRepeats_Events_EventId1",
                table: "EventRepeats",
                column: "EventId1",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
