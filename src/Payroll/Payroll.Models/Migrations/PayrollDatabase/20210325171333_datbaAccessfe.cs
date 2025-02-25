using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class datbaAccessfe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDayAtWork",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LengthOfProbation",
                table: "Employees",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NoticeDate",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProbationEndDate",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeeklyWorkingHours",
                table: "Employees",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LastDayAtWork",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LengthOfProbation",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "NoticeDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ProbationEndDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "WeeklyWorkingHours",
                table: "Employees");
        }
    }
}
