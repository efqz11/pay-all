using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.PayrollDatabase
{
    public partial class dwexcasd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Nationality_NationalityId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Nationality_Companies_CompanyId",
                table: "Nationality");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Nationality",
                table: "Nationality");

            migrationBuilder.RenameTable(
                name: "Nationality",
                newName: "Nationalities");

            migrationBuilder.RenameIndex(
                name: "IX_Nationality_CompanyId",
                table: "Nationalities",
                newName: "IX_Nationalities_CompanyId");

            migrationBuilder.AddColumn<int>(
                name: "TerminationReasonId",
                table: "EmployeeTypes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmergencyContactRelationshipId",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Nationalities",
                table: "Nationalities",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTypes_TerminationReasonId",
                table: "EmployeeTypes",
                column: "TerminationReasonId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Nationalities_NationalityId",
                table: "Employees",
                column: "NationalityId",
                principalTable: "Nationalities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTypes_TerminationReasons_TerminationReasonId",
                table: "EmployeeTypes",
                column: "TerminationReasonId",
                principalTable: "TerminationReasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Nationalities_Companies_CompanyId",
                table: "Nationalities",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmergencyContactRelationships_EmergencyContactRelationshipId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Nationalities_NationalityId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTypes_TerminationReasons_TerminationReasonId",
                table: "EmployeeTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Nationalities_Companies_CompanyId",
                table: "Nationalities");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeTypes_TerminationReasonId",
                table: "EmployeeTypes");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmergencyContactRelationshipId",
                table: "Employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Nationalities",
                table: "Nationalities");

            migrationBuilder.DropColumn(
                name: "TerminationReasonId",
                table: "EmployeeTypes");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelationshipId",
                table: "Employees");

            migrationBuilder.RenameTable(
                name: "Nationalities",
                newName: "Nationality");

            migrationBuilder.RenameIndex(
                name: "IX_Nationalities_CompanyId",
                table: "Nationality",
                newName: "IX_Nationality_CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Nationality",
                table: "Nationality",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Nationality_NationalityId",
                table: "Employees",
                column: "NationalityId",
                principalTable: "Nationality",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Nationality_Companies_CompanyId",
                table: "Nationality",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
