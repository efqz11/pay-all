using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.PayrollDatabase
{
    public partial class empJobInfosFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ReportingEmployeeId",
                table: "EmployeeJobInfos",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ReportingEmployeeId",
                table: "EmployeeJobInfos",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
