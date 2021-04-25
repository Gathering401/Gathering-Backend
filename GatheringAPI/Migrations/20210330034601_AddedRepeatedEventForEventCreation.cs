using Microsoft.EntityFrameworkCore.Migrations;

namespace GatheringAPI.Migrations
{
    public partial class AddedRepeatedEventForEventCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepeatedEvents",
                columns: table => new
                {
                    EventRepeatId = table.Column<long>(nullable: false),
                    EventId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepeatedEvents", x => new { x.EventRepeatId, x.EventId });
                    table.ForeignKey(
                        name: "FK_RepeatedEvents_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepeatedEvents_EventRepeats_EventRepeatId",
                        column: x => x.EventRepeatId,
                        principalTable: "EventRepeats",
                        principalColumn: "EventRepeatId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepeatedEvents_EventId",
                table: "RepeatedEvents",
                column: "EventId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepeatedEvents");
        }
    }
}
