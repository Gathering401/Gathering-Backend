using Microsoft.EntityFrameworkCore.Migrations;

namespace GatheringAPI.Migrations
{
    public partial class RemovedRequiredList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupEvents",
                columns: table => new
                {
                    GroupId = table.Column<long>(nullable: false),
                    EventId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupEvents", x => new { x.GroupId, x.EventId });
                    table.ForeignKey(
                        name: "FK_GroupEvents_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupEvents_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupEvents_EventId",
                table: "GroupEvents",
                column: "EventId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupEvents");
        }
    }
}
