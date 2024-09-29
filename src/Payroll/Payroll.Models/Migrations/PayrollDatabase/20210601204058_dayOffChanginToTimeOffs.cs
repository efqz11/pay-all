using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class dayOffChanginToTimeOffs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccrualMethod",
                table: "DayOffs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccureTimeBasedOn",
                table: "DayOffs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOutstandingBalancePaidUponDismissial",
                table: "DayOffs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOvertimeCountedTowardsTimeOff",
                table: "DayOffs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsThereCarryOverLimit",
                table: "DayOffs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsThereLimit",
                table: "DayOffs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsThereWaitingPeriodForAccrue",
                table: "DayOffs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsThereWaitingPeriodForRequest",
                table: "DayOffs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LengthWaitingPeriodForAccrue",
                table: "DayOffs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LengthWaitingPeriodForRequest",
                table: "DayOffs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxAccuredHoursPerYear",
                table: "DayOffs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxBalance",
                table: "DayOffs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccrualMethod",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "AccureTimeBasedOn",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "IsOutstandingBalancePaidUponDismissial",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "IsOvertimeCountedTowardsTimeOff",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "IsThereCarryOverLimit",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "IsThereLimit",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "IsThereWaitingPeriodForAccrue",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "IsThereWaitingPeriodForRequest",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "LengthWaitingPeriodForAccrue",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "LengthWaitingPeriodForRequest",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "MaxAccuredHoursPerYear",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "MaxBalance",
                table: "DayOffs");
        }
    }
}
