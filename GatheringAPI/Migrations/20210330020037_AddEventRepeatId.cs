using Microsoft.EntityFrameworkCore.Migrations;

namespace GatheringAPI.Migrations
{
    public partial class AddEventRepeatId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventRepeats_Events_EventId",
                table: "EventRepeats");

            migrationBuilder.DropIndex(
                name: "IX_EventRepeats_EventId",
                table: "EventRepeats");

            migrationBuilder.AddColumn<long>(
                name: "EventRepeatId",
                table: "Events",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "EventId1",
                table: "EventRepeats",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventRepeats_Events_EventId1",
                table: "EventRepeats");

            migrationBuilder.DropIndex(
                name: "IX_EventRepeats_EventId1",
                table: "EventRepeats");

            migrationBuilder.DropColumn(
                name: "EventRepeatId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "EventRepeats");

            migrationBuilder.CreateIndex(
                name: "IX_EventRepeats_EventId",
                table: "EventRepeats",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventRepeats_Events_EventId",
                table: "EventRepeats",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
