using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.AccountDatabase
{
    public partial class notificatinEmployeeFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NotificationActionTypeEnum",
                table: "Notifications",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequestingEmployeeAvatar",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestingEmployeeId",
                table: "Notifications",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequestingEmployeeName",
                table: "Notifications",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationActionTypeEnum",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RequestingEmployeeAvatar",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RequestingEmployeeId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RequestingEmployeeName",
                table: "Notifications");
        }
    }
}
