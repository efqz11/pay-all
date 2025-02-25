using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class splitEmployeeToIndividualTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Nationalities_NationalityId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_SecondaryLanguages_SecondaryLanguageId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_NationalityId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_SecondaryLanguageId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BackgroundJobId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BankAccountName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Bio_About",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmailPersonal",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HangfireJobId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IdentityType",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "NationalityId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PhonePersonal",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "SecondaryLanguageId",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "Initial",
                table: "Employees",
                newName: "IndividualId");

            migrationBuilder.RenameColumn(
                name: "DateOfBirth",
                table: "Employees",
                newName: "ProbationStartDate");

            migrationBuilder.AlterColumn<int>(
                name: "ScheduleFor",
                table: "Schedules",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Requests",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "RequestType",
                table: "Requests",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "Locations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencySymbol",
                table: "Locations",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FileType",
                table: "FileDatas",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<decimal>(
                name: "BasicSalary",
                table: "Employees",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractStartDate",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCancelled",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateExpiry",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateRegistered",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmploymentType",
                table: "Employees",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmploymentTypeOther",
                table: "Employees",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "WorkType",
                table: "Companies",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "TaskType",
                table: "BackgroundJobs",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<int>(
                name: "IndividualId",
                table: "BackgroundJobs",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CurrentStatus",
                table: "Attendances",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateTable(
                name: "Individual",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Initial = table.Column<int>(nullable: false),
                    FirstName = table.Column<string>(nullable: false),
                    MiddleName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Gender = table.Column<int>(nullable: false),
                    Avatar = table.Column<string>(nullable: true),
                    EmailPersonal = table.Column<string>(nullable: true),
                    PhonePersonal = table.Column<string>(nullable: true),
                    NationalityId = table.Column<int>(nullable: true),
                    DateOfBirth = table.Column<DateTime>(nullable: true),
                    SecondaryLanguageId = table.Column<int>(nullable: true),
                    IdentityType = table.Column<int>(nullable: false),
                    IdentityNumber = table.Column<string>(nullable: true),
                    RCN = table.Column<int>(nullable: false),
                    LastPromotionDate = table.Column<DateTime>(nullable: true),
                    LastContractEndDate = table.Column<DateTime>(nullable: true),
                    FathersName = table.Column<string>(nullable: true),
                    MothersName = table.Column<string>(nullable: true),
                    Bio_About = table.Column<string>(nullable: true),
                    EmergencyContactName = table.Column<string>(nullable: true),
                    EmergencyContactNumber = table.Column<string>(nullable: true),
                    EmergencyContactRelationshipId = table.Column<int>(nullable: true),
                    BankName = table.Column<string>(nullable: true),
                    BankAccountName = table.Column<string>(nullable: true),
                    BankAccountNumber = table.Column<string>(nullable: true),
                    DisplayOrder = table.Column<string>(nullable: true),
                    TwitterId = table.Column<string>(nullable: true),
                    FacebookId = table.Column<string>(nullable: true),
                    InstagramId = table.Column<string>(nullable: true),
                    LinkedInId = table.Column<string>(nullable: true),
                    NickName = table.Column<string>(nullable: true),
                    CreatedById = table.Column<string>(nullable: true),
                    CreatedByName = table.Column<string>(nullable: true),
                    CreatedByRoles = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GetUtcDate()"),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ModifiedById = table.Column<string>(nullable: true),
                    ModifiedByName = table.Column<string>(nullable: true),
                    ModifiedByRoles = table.Column<string>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GetUtcDate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Individual", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Individual_EmergencyContactRelationships_EmergencyContactRelationshipId",
                        column: x => x.EmergencyContactRelationshipId,
                        principalTable: "EmergencyContactRelationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Individual_Nationalities_NationalityId",
                        column: x => x.NationalityId,
                        principalTable: "Nationalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Individual_SecondaryLanguages_SecondaryLanguageId",
                        column: x => x.SecondaryLanguageId,
                        principalTable: "SecondaryLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IndividualId",
                table: "Employees",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobs_IndividualId",
                table: "BackgroundJobs",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_Individual_EmergencyContactRelationshipId",
                table: "Individual",
                column: "EmergencyContactRelationshipId");

            migrationBuilder.CreateIndex(
                name: "IX_Individual_NationalityId",
                table: "Individual",
                column: "NationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Individual_SecondaryLanguageId",
                table: "Individual",
                column: "SecondaryLanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_BackgroundJobs_Individual_IndividualId",
                table: "BackgroundJobs",
                column: "IndividualId",
                principalTable: "Individual",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Individual_IndividualId",
                table: "Employees",
                column: "IndividualId",
                principalTable: "Individual",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BackgroundJobs_Individual_IndividualId",
                table: "BackgroundJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Individual_IndividualId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "Individual");

            migrationBuilder.DropIndex(
                name: "IX_Employees_IndividualId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_BackgroundJobs_IndividualId",
                table: "BackgroundJobs");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CurrencySymbol",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "BasicSalary",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ContractStartDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DateCancelled",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DateExpiry",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DateRegistered",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmploymentTypeOther",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IndividualId",
                table: "BackgroundJobs");

            migrationBuilder.RenameColumn(
                name: "ProbationStartDate",
                table: "Employees",
                newName: "DateOfBirth");

            migrationBuilder.RenameColumn(
                name: "IndividualId",
                table: "Employees",
                newName: "Initial");

            migrationBuilder.AlterColumn<string>(
                name: "ScheduleFor",
                table: "Schedules",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Requests",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "RequestType",
                table: "Requests",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "FileType",
                table: "FileDatas",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BackgroundJobId",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountName",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio_About",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailPersonal",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactNumber",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Employees",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Employees",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HangfireJobId",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityType",
                table: "Employees",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NationalityId",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhonePersonal",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SecondaryLanguageId",
                table: "Employees",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WorkType",
                table: "Companies",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "TaskType",
                table: "BackgroundJobs",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "CurrentStatus",
                table: "Attendances",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.CreateIndex(
                name: "IX_Employees_NationalityId",
                table: "Employees",
                column: "NationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_SecondaryLanguageId",
                table: "Employees",
                column: "SecondaryLanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Nationalities_NationalityId",
                table: "Employees",
                column: "NationalityId",
                principalTable: "Nationalities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_SecondaryLanguages_SecondaryLanguageId",
                table: "Employees",
                column: "SecondaryLanguageId",
                principalTable: "SecondaryLanguages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
