using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class addDayOffTrackerFixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalDays",
                table: "DayOffs");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "DayOffTrackers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAccruredBySystem",
                table: "DayOffTrackers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAddOrSubState",
                table: "DayOffTrackers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCreateByDuringKickOff",
                table: "DayOffTrackers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RequestId",
                table: "DayOffTrackers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DayOffTrackers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "DayOffTrackers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalDays",
                table: "DayOffTrackers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalHoursPerYear",
                table: "DayOffs",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DayOffTrackers_EmployeeId",
                table: "DayOffTrackers",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOffTrackers_RequestId",
                table: "DayOffTrackers",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_DayOffTrackers_Employees_EmployeeId",
                table: "DayOffTrackers",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DayOffTrackers_Requests_RequestId",
                table: "DayOffTrackers",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DayOffTrackers_Employees_EmployeeId",
                table: "DayOffTrackers");

            migrationBuilder.DropForeignKey(
                name: "FK_DayOffTrackers_Requests_RequestId",
                table: "DayOffTrackers");

            migrationBuilder.DropIndex(
                name: "IX_DayOffTrackers_EmployeeId",
                table: "DayOffTrackers");

            migrationBuilder.DropIndex(
                name: "IX_DayOffTrackers_RequestId",
                table: "DayOffTrackers");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "DayOffTrackers");

            migrationBuilder.DropColumn(
                name: "IsAccruredBySystem",
                table: "DayOffTrackers");

            migrationBuilder.DropColumn(
                name: "IsAddOrSubState",
                table: "DayOffTrackers");

            migrationBuilder.DropColumn(
                name: "IsCreateByDuringKickOff",
                table: "DayOffTrackers");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "DayOffTrackers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DayOffTrackers");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "DayOffTrackers");

            migrationBuilder.DropColumn(
                name: "TotalDays",
                table: "DayOffTrackers");

            migrationBuilder.DropColumn(
                name: "TotalHoursPerYear",
                table: "DayOffs");

            migrationBuilder.AddColumn<int>(
                name: "TotalDays",
                table: "DayOffs",
                type: "int",
                nullable: true);
        }
    }
}
