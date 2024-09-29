using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class addNotificationForEmployeesAndDayoffChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResetEveryYear",
                table: "DayOffs",
                newName: "RequireSubstitiuteOptional");

            migrationBuilder.RenameColumn(
                name: "IsEnteredManually",
                table: "DayOffs",
                newName: "RequireSubstitiute");

            migrationBuilder.RenameColumn(
                name: "IsApplicationAfterFirstYear",
                table: "DayOffs",
                newName: "GrantExtraDaysBasedOnTimeOfEmployment");

            migrationBuilder.RenameColumn(
                name: "IsApplicableOnProbation",
                table: "DayOffs",
                newName: "GrantExtraDayWithStartOfNextEntitlementPeriod");

            migrationBuilder.AddColumn<int>(
                name: "AccruralCarryoverFromPreviousYear",
                table: "DayOffs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BeginingOfEmployment",
                table: "DayOffs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CarryOverLimit",
                table: "DayOffs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ConsiderTimeTrackedAsOvertime",
                table: "DayOffs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DayOffGrantEntititlementAt",
                table: "DayOffs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DayOffGrantEntititlementEvery",
                table: "DayOffs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EnableAccruralPolicy",
                table: "DayOffs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableWaitingTime",
                table: "DayOffs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EndOfEmployment",
                table: "DayOffs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EntitilementGrantedDuringWaitingTime",
                table: "DayOffs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExtraDaysAfter",
                table: "DayOffs",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OvertimCalculationBasis",
                table: "Companies",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OvertimeCliffHours",
                table: "Companies",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SetOffDefecitHoursAgainstOvertime",
                table: "Companies",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrackOvertime",
                table: "Companies",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "TotalDefecitHours",
                table: "Attendances",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccruralCarryoverFromPreviousYear",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "BeginingOfEmployment",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "CarryOverLimit",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "ConsiderTimeTrackedAsOvertime",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "DayOffGrantEntititlementAt",
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
                name: "EndOfEmployment",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "EntitilementGrantedDuringWaitingTime",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "ExtraDaysAfter",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "OvertimCalculationBasis",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "OvertimeCliffHours",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SetOffDefecitHoursAgainstOvertime",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "TrackOvertime",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "TotalDefecitHours",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "RequireSubstitiuteOptional",
                table: "DayOffs",
                newName: "ResetEveryYear");

            migrationBuilder.RenameColumn(
                name: "RequireSubstitiute",
                table: "DayOffs",
                newName: "IsEnteredManually");

            migrationBuilder.RenameColumn(
                name: "GrantExtraDaysBasedOnTimeOfEmployment",
                table: "DayOffs",
                newName: "IsApplicationAfterFirstYear");

            migrationBuilder.RenameColumn(
                name: "GrantExtraDayWithStartOfNextEntitlementPeriod",
                table: "DayOffs",
                newName: "IsApplicableOnProbation");
        }
    }
}
