using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class DayOffTableChanges1DaysToHours : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalCollectedDays",
                table: "DayOffEmployees");

            migrationBuilder.DropColumn(
                name: "TotalDays",
                table: "DayOffEmployees");

            migrationBuilder.DropColumn(
                name: "TotalRemainingDays",
                table: "DayOffEmployees");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPerYear",
                table: "DayOffs",
                type: "decimal(3,1)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TotalDays",
                table: "DayOffs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "LengthWaitingPeriodForRequest",
                table: "DayOffs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "LengthWaitingPeriodForAccrue",
                table: "DayOffs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCollectedHours",
                table: "DayOffEmployees",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalHours",
                table: "DayOffEmployees",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRemainingHours",
                table: "DayOffEmployees",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "DayOffTracker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DayOffId = table.Column<int>(type: "int", nullable: false),
                    DayOffEmployeeId = table.Column<int>(type: "int", nullable: false),
                    TotalHours = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    JobIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GetUtcDate()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GetUtcDate()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayOffTracker", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayOffTracker_DayOffEmployees_DayOffEmployeeId",
                        column: x => x.DayOffEmployeeId,
                        principalTable: "DayOffEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DayOffTracker_DayOffs_DayOffId",
                        column: x => x.DayOffId,
                        principalTable: "DayOffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DayOffTracker_DayOffEmployeeId",
                table: "DayOffTracker",
                column: "DayOffEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOffTracker_DayOffId",
                table: "DayOffTracker",
                column: "DayOffId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DayOffTracker");

            migrationBuilder.DropColumn(
                name: "TotalCollectedHours",
                table: "DayOffEmployees");

            migrationBuilder.DropColumn(
                name: "TotalHours",
                table: "DayOffEmployees");

            migrationBuilder.DropColumn(
                name: "TotalRemainingHours",
                table: "DayOffEmployees");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPerYear",
                table: "DayOffs",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TotalDays",
                table: "DayOffs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LengthWaitingPeriodForRequest",
                table: "DayOffs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LengthWaitingPeriodForAccrue",
                table: "DayOffs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalCollectedDays",
                table: "DayOffEmployees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalDays",
                table: "DayOffEmployees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalRemainingDays",
                table: "DayOffEmployees",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
