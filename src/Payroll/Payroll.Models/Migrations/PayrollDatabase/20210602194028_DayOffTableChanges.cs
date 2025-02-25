using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class DayOffTableChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccruralCarryoverFromPreviousYear",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "BeginingOfEmployment",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "DayOffGrantEntititlementEvery",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "EnableAccruralPolicy",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "EnableWaitingTime",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "GrantExtraDayWithStartOfNextEntitlementPeriod",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "GrantExtraDaysBasedOnTimeOfEmployment",
                table: "DayOffs");

            migrationBuilder.RenameColumn(
                name: "EntitilementGrantedDuringWaitingTime",
                table: "DayOffs",
                newName: "PerHoursWorked");

            migrationBuilder.RenameColumn(
                name: "EndOfEmployment",
                table: "DayOffs",
                newName: "HoursEarned");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PerHoursWorked",
                table: "DayOffs",
                newName: "EntitilementGrantedDuringWaitingTime");

            migrationBuilder.RenameColumn(
                name: "HoursEarned",
                table: "DayOffs",
                newName: "EndOfEmployment");

            migrationBuilder.AddColumn<int>(
                name: "AccruralCarryoverFromPreviousYear",
                table: "DayOffs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BeginingOfEmployment",
                table: "DayOffs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DayOffGrantEntititlementEvery",
                table: "DayOffs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EnableAccruralPolicy",
                table: "DayOffs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableWaitingTime",
                table: "DayOffs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GrantExtraDayWithStartOfNextEntitlementPeriod",
                table: "DayOffs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GrantExtraDaysBasedOnTimeOfEmployment",
                table: "DayOffs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
