using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class embrachedEmplouymentData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobActionHistories_Employees_PreviousEmployeeId",
                table: "JobActionHistories");

            migrationBuilder.RenameColumn(
                name: "PreviousEmployeeId",
                table: "JobActionHistories",
                newName: "PreviousEmploymentId");

            migrationBuilder.RenameIndex(
                name: "IX_JobActionHistories_PreviousEmployeeId",
                table: "JobActionHistories",
                newName: "IX_JobActionHistories_PreviousEmploymentId");

            migrationBuilder.AddColumn<int>(
                name: "MaritialStatus",
                table: "Individuals",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_JobActionHistories_Employments_PreviousEmploymentId",
                table: "JobActionHistories",
                column: "PreviousEmploymentId",
                principalTable: "Employments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobActionHistories_Employments_PreviousEmploymentId",
                table: "JobActionHistories");

            migrationBuilder.DropColumn(
                name: "MaritialStatus",
                table: "Individuals");

            migrationBuilder.RenameColumn(
                name: "PreviousEmploymentId",
                table: "JobActionHistories",
                newName: "PreviousEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_JobActionHistories_PreviousEmploymentId",
                table: "JobActionHistories",
                newName: "IX_JobActionHistories_PreviousEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobActionHistories_Employees_PreviousEmployeeId",
                table: "JobActionHistories",
                column: "PreviousEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
