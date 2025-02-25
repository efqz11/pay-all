using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class EmployeeToEmployeeParentRel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveContractId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ActiveContractName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsContractEnded",
                table: "Employees");

            migrationBuilder.AddColumn<int>(
                name: "ReportingEmployeeId",
                table: "Employees",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ReportingEmployeeId",
                table: "Employees",
                column: "ReportingEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_ReportingEmployeeId",
                table: "Employees",
                column: "ReportingEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_ReportingEmployeeId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ReportingEmployeeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ReportingEmployeeId",
                table: "Employees");

            migrationBuilder.AddColumn<int>(
                name: "ActiveContractId",
                table: "Employees",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ActiveContractName",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsContractEnded",
                table: "Employees",
                nullable: false,
                defaultValue: false);
        }
    }
}
