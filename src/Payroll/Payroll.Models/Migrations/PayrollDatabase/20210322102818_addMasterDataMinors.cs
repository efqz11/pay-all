using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class addMasterDataMinors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkPhone",
                table: "Employees",
                newName: "PhoneWork");

            migrationBuilder.RenameColumn(
                name: "WorkExt",
                table: "Employees",
                newName: "PhoneWorkExt");

            migrationBuilder.RenameColumn(
                name: "EmailAddress",
                table: "Employees",
                newName: "EmailWork");

            migrationBuilder.AlterColumn<string>(
                name: "EmpID",
                table: "PayrollPeriodEmployees",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "EmpID",
                table: "Employees",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "EmailPersonal",
                table: "Employees",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailPersonal",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "PhoneWorkExt",
                table: "Employees",
                newName: "WorkExt");

            migrationBuilder.RenameColumn(
                name: "PhoneWork",
                table: "Employees",
                newName: "WorkPhone");

            migrationBuilder.RenameColumn(
                name: "EmailWork",
                table: "Employees",
                newName: "EmailAddress");

            migrationBuilder.AlterColumn<int>(
                name: "EmpID",
                table: "PayrollPeriodEmployees",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpID",
                table: "Employees",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
