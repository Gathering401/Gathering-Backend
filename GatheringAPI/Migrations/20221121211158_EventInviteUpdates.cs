using Microsoft.EntityFrameworkCore.Migrations;

namespace GatheringAPI.Migrations
{
    public partial class EventInviteUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventInvites_Events_EventId",
                table: "EventInvites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventInvites",
                table: "EventInvites");

            migrationBuilder.AlterColumn<long>(
                name: "EventId",
                table: "EventInvites",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "EventRepeatId",
                table: "EventInvites",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventInvites",
                table: "EventInvites",
                columns: new[] { "UserId", "EventRepeatId" });

            migrationBuilder.CreateIndex(
                name: "IX_EventInvites_EventRepeatId",
                table: "EventInvites",
                column: "EventRepeatId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvites_Events_EventId",
                table: "EventInvites",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvites_EventRepeats_EventRepeatId",
                table: "EventInvites",
                column: "EventRepeatId",
                principalTable: "EventRepeats",
                principalColumn: "EventRepeatId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventInvites_Events_EventId",
                table: "EventInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_EventInvites_EventRepeats_EventRepeatId",
                table: "EventInvites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventInvites",
                table: "EventInvites");

            migrationBuilder.DropIndex(
                name: "IX_EventInvites_EventRepeatId",
                table: "EventInvites");

            migrationBuilder.DropColumn(
                name: "EventRepeatId",
                table: "EventInvites");

            migrationBuilder.AlterColumn<long>(
                name: "EventId",
                table: "EventInvites",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventInvites",
                table: "EventInvites",
                columns: new[] { "UserId", "EventId" });

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvites_Events_EventId",
                table: "EventInvites",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
