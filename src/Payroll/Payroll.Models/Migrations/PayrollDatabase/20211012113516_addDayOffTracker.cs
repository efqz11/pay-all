using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class addDayOffTracker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DayOffTracker_DayOffEmployees_DayOffEmployeeId",
                table: "DayOffTracker");

            migrationBuilder.DropForeignKey(
                name: "FK_DayOffTracker_DayOffs_DayOffId",
                table: "DayOffTracker");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DayOffTracker",
                table: "DayOffTracker");

            migrationBuilder.RenameTable(
                name: "DayOffTracker",
                newName: "DayOffTrackers");

            migrationBuilder.RenameIndex(
                name: "IX_DayOffTracker_DayOffId",
                table: "DayOffTrackers",
                newName: "IX_DayOffTrackers_DayOffId");

            migrationBuilder.RenameIndex(
                name: "IX_DayOffTracker_DayOffEmployeeId",
                table: "DayOffTrackers",
                newName: "IX_DayOffTrackers_DayOffEmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DayOffTrackers",
                table: "DayOffTrackers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DayOffTrackers_DayOffEmployees_DayOffEmployeeId",
                table: "DayOffTrackers",
                column: "DayOffEmployeeId",
                principalTable: "DayOffEmployees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DayOffTrackers_DayOffs_DayOffId",
                table: "DayOffTrackers",
                column: "DayOffId",
                principalTable: "DayOffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DayOffTrackers_DayOffEmployees_DayOffEmployeeId",
                table: "DayOffTrackers");

            migrationBuilder.DropForeignKey(
                name: "FK_DayOffTrackers_DayOffs_DayOffId",
                table: "DayOffTrackers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DayOffTrackers",
                table: "DayOffTrackers");

            migrationBuilder.RenameTable(
                name: "DayOffTrackers",
                newName: "DayOffTracker");

            migrationBuilder.RenameIndex(
                name: "IX_DayOffTrackers_DayOffId",
                table: "DayOffTracker",
                newName: "IX_DayOffTracker_DayOffId");

            migrationBuilder.RenameIndex(
                name: "IX_DayOffTrackers_DayOffEmployeeId",
                table: "DayOffTracker",
                newName: "IX_DayOffTracker_DayOffEmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DayOffTracker",
                table: "DayOffTracker",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DayOffTracker_DayOffEmployees_DayOffEmployeeId",
                table: "DayOffTracker",
                column: "DayOffEmployeeId",
                principalTable: "DayOffEmployees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DayOffTracker_DayOffs_DayOffId",
                table: "DayOffTracker",
                column: "DayOffId",
                principalTable: "DayOffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
