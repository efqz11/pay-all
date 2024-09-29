using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.AccountDatabase
{
    public partial class changeNotifTypesTextSummary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedTextWithPlaceholder",
                table: "NotificationTypes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedTextWithPlaceholder",
                table: "NotificationTypes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestType",
                table: "NotificationTypes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFallbackNotification",
                table: "Notifications",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "fallbackNotificationSummary",
                table: "Notifications",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedTextWithPlaceholder",
                table: "NotificationTypes");

            migrationBuilder.DropColumn(
                name: "RejectedTextWithPlaceholder",
                table: "NotificationTypes");

            migrationBuilder.DropColumn(
                name: "RequestType",
                table: "NotificationTypes");

            migrationBuilder.DropColumn(
                name: "IsFallbackNotification",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "fallbackNotificationSummary",
                table: "Notifications");
        }
    }
}
