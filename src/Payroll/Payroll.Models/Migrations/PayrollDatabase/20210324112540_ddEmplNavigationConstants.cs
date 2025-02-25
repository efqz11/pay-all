using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class ddEmplNavigationConstants : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Employees");

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DivisionName",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HrStatus",
                table: "Employees",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobIDString",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationName",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Color",
                table: "DayOffs",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DivisionName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HrStatus",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "JobIDString",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LocationName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "DayOffs");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Employees",
                nullable: false,
                defaultValue: "");
        }
    }
}
