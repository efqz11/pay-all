using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.AccountDatabase
{
    public partial class addedEmplToNotificaitons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeesWithRoles",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Step",
                table: "Notifications",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ToBeReceivedBy",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayrolPeriodClosingDateHDaysAfterEndDate",
                table: "CompanyAccounts",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayrolPeriodPayDate",
                table: "CompanyAccounts",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayrolPeriodPayDateHDaysAfterCloseDate",
                table: "CompanyAccounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EmployeesWithRoles",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Step",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ToBeReceivedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "PayrolPeriodClosingDateHDaysAfterEndDate",
                table: "CompanyAccounts");

            migrationBuilder.DropColumn(
                name: "PayrolPeriodPayDate",
                table: "CompanyAccounts");

            migrationBuilder.DropColumn(
                name: "PayrolPeriodPayDateHDaysAfterCloseDate",
                table: "CompanyAccounts");
        }
    }
}
