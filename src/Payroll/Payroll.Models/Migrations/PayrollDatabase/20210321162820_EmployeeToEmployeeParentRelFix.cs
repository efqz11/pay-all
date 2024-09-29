using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class EmployeeToEmployeeParentRelFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportingEmployeeId",
                table: "EmployeeActions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeActions_ReportingEmployeeId",
                table: "EmployeeActions",
                column: "ReportingEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeActions_Employees_ReportingEmployeeId",
                table: "EmployeeActions",
                column: "ReportingEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeActions_Employees_ReportingEmployeeId",
                table: "EmployeeActions");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeActions_ReportingEmployeeId",
                table: "EmployeeActions");

            migrationBuilder.DropColumn(
                name: "ReportingEmployeeId",
                table: "EmployeeActions");
        }
    }
}
