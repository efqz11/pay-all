using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class employmentsDataChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmploymentId",
                table: "JobActionHistories",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmploymentStatus",
                table: "Employments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmploymentType",
                table: "Employments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmploymentTypeOther",
                table: "Employments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_JobActionHistories_EmploymentId",
                table: "JobActionHistories",
                column: "EmploymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobActionHistories_Employments_EmploymentId",
                table: "JobActionHistories",
                column: "EmploymentId",
                principalTable: "Employments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobActionHistories_Employments_EmploymentId",
                table: "JobActionHistories");

            migrationBuilder.DropIndex(
                name: "IX_JobActionHistories_EmploymentId",
                table: "JobActionHistories");

            migrationBuilder.DropColumn(
                name: "EmploymentId",
                table: "JobActionHistories");

            migrationBuilder.DropColumn(
                name: "EmploymentStatus",
                table: "Employments");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "Employments");

            migrationBuilder.DropColumn(
                name: "EmploymentTypeOther",
                table: "Employments");
        }
    }
}
