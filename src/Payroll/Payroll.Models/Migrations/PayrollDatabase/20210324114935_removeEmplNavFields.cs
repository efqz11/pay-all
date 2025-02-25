using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class removeEmplNavFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DivisionName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LocationName",
                table: "Employees");

            migrationBuilder.AlterColumn<int>(
                name: "JobType",
                table: "Employees",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "HrStatus",
                table: "Employees",
                nullable: false,
                oldClrType: typeof(string));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JobType",
                table: "Employees",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HrStatus",
                table: "Employees",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DivisionName",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationName",
                table: "Employees",
                nullable: true);
        }
    }
}
