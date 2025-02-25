using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.PayrollDatabase
{
    public partial class requestdaysCounter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetAnnuallyFromJoinDate",
                table: "DayOffs");

            migrationBuilder.AddColumn<int>(
                name: "TotalDays",
                table: "Requests",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "Employees",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalDays",
                table: "Requests");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "Employees",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<bool>(
                name: "ResetAnnuallyFromJoinDate",
                table: "DayOffs",
                nullable: false,
                defaultValue: false);
        }
    }
}
