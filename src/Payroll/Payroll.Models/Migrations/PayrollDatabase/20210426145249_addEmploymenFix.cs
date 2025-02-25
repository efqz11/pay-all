using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class addEmploymenFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyWorkingHours",
                table: "Employments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WeeklyWorkingHours",
                table: "Employments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DailyWorkingHours",
                table: "Employees",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DaysWorkingInWeek",
                table: "Employees",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IsDaysWorkingInWeekFlexible",
                table: "Employees",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyWorkingHours",
                table: "Employments");

            migrationBuilder.DropColumn(
                name: "WeeklyWorkingHours",
                table: "Employments");

            migrationBuilder.DropColumn(
                name: "DailyWorkingHours",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DaysWorkingInWeek",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsDaysWorkingInWeekFlexible",
                table: "Employees");
        }
    }
}
