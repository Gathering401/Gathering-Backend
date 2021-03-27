using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GatheringAPI.Migrations
{
    public partial class AddEventRepeat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Groups",
                keyColumn: "GroupId",
                keyValue: 1L);

            migrationBuilder.DropColumn(
                name: "DayOfMonth",
                table: "Events");

            migrationBuilder.CreateTable(
                name: "EventRepeats",
                columns: table => new
                {
                    EventRepeatId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ERepeat = table.Column<int>(nullable: false),
                    EventId = table.Column<long>(nullable: false),
                    DayOfWeek = table.Column<int>(nullable: true),
                    DayOfMonth = table.Column<int>(nullable: true),
                    MonthOfYear = table.Column<int>(nullable: true),
                    FirstEventDate = table.Column<DateTime>(nullable: false),
                    EndEventDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRepeats", x => x.EventRepeatId);
                    table.ForeignKey(
                        name: "FK_EventRepeats_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRepeats_EventId",
                table: "EventRepeats",
                column: "EventId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRepeats");

            migrationBuilder.AddColumn<int>(
                name: "DayOfMonth",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Groups",
                columns: new[] { "GroupId", "Description", "GroupName", "GroupSize", "IsPublic", "Location", "MaxEvents", "MaxUsers" },
                values: new object[] { 1L, "HI", "Odysseus", 0, false, "Remote", 0L, 0L });
        }
    }
}
