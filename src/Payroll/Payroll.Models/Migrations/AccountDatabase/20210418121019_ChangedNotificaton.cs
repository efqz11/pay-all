using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.AccountDatabase
{
    public partial class ChangedNotificaton : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AppUsers_UserId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Notifications",
                newName: "ActionTakenUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                newName: "IX_Notifications_ActionTakenUserId");

            migrationBuilder.AddColumn<string>(
                name: "ActionTakenEmployeeAvatar",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActionTakenEmployeeId",
                table: "Notifications",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ActionTakenEmployeeName",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AppUsers_ActionTakenUserId",
                table: "Notifications",
                column: "ActionTakenUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AppUsers_ActionTakenUserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ActionTakenEmployeeAvatar",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ActionTakenEmployeeId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ActionTakenEmployeeName",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "ActionTakenUserId",
                table: "Notifications",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_ActionTakenUserId",
                table: "Notifications",
                newName: "IX_Notifications_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AppUsers_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
