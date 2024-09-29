using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class RequestDataChangeNew : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacebookId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "InstagramId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LinkedInId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TwitterId",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "Remarks",
                table: "RequestDataChanges",
                newName: "ZipCode1");

            migrationBuilder.RenameColumn(
                name: "PropertyValue",
                table: "RequestDataChanges",
                newName: "ZipCode");

            migrationBuilder.RenameColumn(
                name: "PropertyType",
                table: "RequestDataChanges",
                newName: "TwitterId");

            migrationBuilder.RenameColumn(
                name: "PropertyName",
                table: "RequestDataChanges",
                newName: "Street1");

            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountName",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City1",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailPersonal",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailWork",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactNumber",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmergencyContactRelationshipId",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacebookId",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "RequestDataChanges",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "RequestDataChanges",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdentityType",
                table: "RequestDataChanges",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Initial",
                table: "RequestDataChanges",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InstagramId",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInId",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NationalityId",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhonePersonal",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneWork",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneWorkExt",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SecondaryLanguageId",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State1",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestDataChanges_EmergencyContactRelationshipId",
                table: "RequestDataChanges",
                column: "EmergencyContactRelationshipId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestDataChanges_NationalityId",
                table: "RequestDataChanges",
                column: "NationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestDataChanges_SecondaryLanguageId",
                table: "RequestDataChanges",
                column: "SecondaryLanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestDataChanges_EmergencyContactRelationships_EmergencyContactRelationshipId",
                table: "RequestDataChanges",
                column: "EmergencyContactRelationshipId",
                principalTable: "EmergencyContactRelationships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestDataChanges_Nationalities_NationalityId",
                table: "RequestDataChanges",
                column: "NationalityId",
                principalTable: "Nationalities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestDataChanges_SecondaryLanguages_SecondaryLanguageId",
                table: "RequestDataChanges",
                column: "SecondaryLanguageId",
                principalTable: "SecondaryLanguages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestDataChanges_EmergencyContactRelationships_EmergencyContactRelationshipId",
                table: "RequestDataChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestDataChanges_Nationalities_NationalityId",
                table: "RequestDataChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestDataChanges_SecondaryLanguages_SecondaryLanguageId",
                table: "RequestDataChanges");

            migrationBuilder.DropIndex(
                name: "IX_RequestDataChanges_EmergencyContactRelationshipId",
                table: "RequestDataChanges");

            migrationBuilder.DropIndex(
                name: "IX_RequestDataChanges_NationalityId",
                table: "RequestDataChanges");

            migrationBuilder.DropIndex(
                name: "IX_RequestDataChanges_SecondaryLanguageId",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "BankAccountName",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "City",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "City1",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "EmailPersonal",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "EmailWork",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "EmergencyContactNumber",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelationshipId",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "FacebookId",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "IdentityType",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "Initial",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "InstagramId",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "LinkedInId",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "NationalityId",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "PhonePersonal",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "PhoneWork",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "PhoneWorkExt",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "SecondaryLanguageId",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "State",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "State1",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "RequestDataChanges");

            migrationBuilder.RenameColumn(
                name: "ZipCode1",
                table: "RequestDataChanges",
                newName: "Remarks");

            migrationBuilder.RenameColumn(
                name: "ZipCode",
                table: "RequestDataChanges",
                newName: "PropertyValue");

            migrationBuilder.RenameColumn(
                name: "TwitterId",
                table: "RequestDataChanges",
                newName: "PropertyType");

            migrationBuilder.RenameColumn(
                name: "Street1",
                table: "RequestDataChanges",
                newName: "PropertyName");

            migrationBuilder.AddColumn<string>(
                name: "FacebookId",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramId",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInId",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwitterId",
                table: "Employees",
                nullable: true);
        }
    }
}
