using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class bankAccInfoXX : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmergencyContactRelationships_EmergencyContactRelationshipId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmergencyContactRelationshipId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelationshipId",
                table: "Employees");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmergencyContactRelationshipId",
                table: "Employees",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmergencyContactRelationshipId",
                table: "Employees",
                column: "EmergencyContactRelationshipId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmergencyContactRelationships_EmergencyContactRelationshipId",
                table: "Employees",
                column: "EmergencyContactRelationshipId",
                principalTable: "EmergencyContactRelationships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
