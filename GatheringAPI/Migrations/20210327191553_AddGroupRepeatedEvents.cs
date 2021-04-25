using Microsoft.EntityFrameworkCore.Migrations;

namespace GatheringAPI.Migrations
{
    public partial class AddGroupRepeatedEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupRepeatedEvents",
                columns: table => new
                {
                    GroupId = table.Column<long>(nullable: false),
                    EventRepeatId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRepeatedEvents", x => new { x.GroupId, x.EventRepeatId });
                    table.ForeignKey(
                        name: "FK_GroupRepeatedEvents_EventRepeats_EventRepeatId",
                        column: x => x.EventRepeatId,
                        principalTable: "EventRepeats",
                        principalColumn: "EventRepeatId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupRepeatedEvents_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupRepeatedEvents_EventRepeatId",
                table: "GroupRepeatedEvents",
                column: "EventRepeatId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupRepeatedEvents");
        }
    }
}
