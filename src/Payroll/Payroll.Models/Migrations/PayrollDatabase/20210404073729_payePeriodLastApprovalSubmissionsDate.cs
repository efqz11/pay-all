using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class payePeriodLastApprovalSubmissionsDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastApprovalSubmissionDate",
                table: "PayrollPeriods",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CarryOverLimit",
                table: "DayOffs",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastApprovalSubmissionDate",
                table: "PayrollPeriods");

            migrationBuilder.AlterColumn<int>(
                name: "CarryOverLimit",
                table: "DayOffs",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");
        }
    }
}
