using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class removeIndividualFrmEmployee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BackgroundJobs_Individuals_IndividualId",
                table: "BackgroundJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Individuals_IndividualId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_IndividualAddresses_Individuals_IndividualId",
                table: "IndividualAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_IndividualAddresses_Locations_LocationId",
                table: "IndividualAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_JobActionHistories_Individuals_IndividualId",
                table: "JobActionHistories");

            migrationBuilder.DropTable(
                name: "EmployeeTypes");

            migrationBuilder.DropTable(
                name: "IndividualEducations");

            migrationBuilder.DropTable(
                name: "IndividualPassports");

            migrationBuilder.DropTable(
                name: "Individuals");

            migrationBuilder.DropIndex(
                name: "IX_JobActionHistories_IndividualId",
                table: "JobActionHistories");

            migrationBuilder.DropIndex(
                name: "IX_Employees_IndividualId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_BackgroundJobs_IndividualId",
                table: "BackgroundJobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IndividualAddresses",
                table: "IndividualAddresses");

            migrationBuilder.DropColumn(
                name: "IndividualId",
                table: "JobActionHistories");

            migrationBuilder.DropColumn(
                name: "IndividualId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IndividualId",
                table: "BackgroundJobs");

            migrationBuilder.RenameTable(
                name: "IndividualAddresses",
                newName: "EmployeeAddresses");

            migrationBuilder.RenameColumn(
                name: "LastPromotionDate",
                table: "Employees",
                newName: "LastPromotedDate");

            migrationBuilder.RenameColumn(
                name: "LastContractEndDate",
                table: "Employees",
                newName: "LastContractEndedDate");

            migrationBuilder.RenameColumn(
                name: "IndividualId",
                table: "EmployeeAddresses",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_IndividualAddresses_LocationId",
                table: "EmployeeAddresses",
                newName: "IX_EmployeeAddresses_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_IndividualAddresses_IndividualId",
                table: "EmployeeAddresses",
                newName: "IX_EmployeeAddresses_EmployeeId");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountName",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAddress",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankCoutnry",
                table: "Employees",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankCurrency",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankIBAN",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankSwiftCode",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio_About",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactEmail",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactNumber",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmergencyContactRelationshipId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacebookId",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramId",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInId",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwitterId",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "EmployeeAddresses",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeAddresses",
                table: "EmployeeAddresses",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "EmployeeEducations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    CollegeInstitution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegreeId = table.Column<int>(type: "int", nullable: false),
                    Degree = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MajorSpecilization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GPA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    End = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsOnGoing = table.Column<bool>(type: "bit", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EmployeeEducations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeEducations_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePassports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuingCountryCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_EmployeePassports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeePassports_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmergencyContactRelationshipId",
                table: "Employees",
                column: "EmergencyContactRelationshipId");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobs_EmployeeId",
                table: "BackgroundJobs",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEducations_EmployeeId",
                table: "EmployeeEducations",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePassports_EmployeeId",
                table: "EmployeePassports",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BackgroundJobs_Employees_EmployeeId",
                table: "BackgroundJobs",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAddresses_Employees_EmployeeId",
                table: "EmployeeAddresses",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAddresses_Locations_LocationId",
                table: "EmployeeAddresses",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmergencyContactRelationships_EmergencyContactRelationshipId",
                table: "Employees",
                column: "EmergencyContactRelationshipId",
                principalTable: "EmergencyContactRelationships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BackgroundJobs_Employees_EmployeeId",
                table: "BackgroundJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAddresses_Employees_EmployeeId",
                table: "EmployeeAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAddresses_Locations_LocationId",
                table: "EmployeeAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmergencyContactRelationships_EmergencyContactRelationshipId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "EmployeeEducations");

            migrationBuilder.DropTable(
                name: "EmployeePassports");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmergencyContactRelationshipId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_BackgroundJobs_EmployeeId",
                table: "BackgroundJobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeAddresses",
                table: "EmployeeAddresses");

            migrationBuilder.DropColumn(
                name: "BankAccountName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BankAddress",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BankCoutnry",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BankCurrency",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BankIBAN",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BankSwiftCode",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Bio_About",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactEmail",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelationshipId",
                table: "Employees");

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

            migrationBuilder.RenameTable(
                name: "EmployeeAddresses",
                newName: "IndividualAddresses");

            migrationBuilder.RenameColumn(
                name: "LastPromotedDate",
                table: "Employees",
                newName: "LastPromotionDate");

            migrationBuilder.RenameColumn(
                name: "LastContractEndedDate",
                table: "Employees",
                newName: "LastContractEndDate");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "IndividualAddresses",
                newName: "IndividualId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeAddresses_LocationId",
                table: "IndividualAddresses",
                newName: "IX_IndividualAddresses_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeAddresses_EmployeeId",
                table: "IndividualAddresses",
                newName: "IX_IndividualAddresses_IndividualId");

            migrationBuilder.AddColumn<int>(
                name: "IndividualId",
                table: "JobActionHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IndividualId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IndividualId",
                table: "BackgroundJobs",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "IndividualAddresses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_IndividualAddresses",
                table: "IndividualAddresses",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "EmployeeTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GetUtcDate()"),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    EmploymentStatus = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GetUtcDate()"),
                    RecordStatus = table.Column<int>(type: "int", nullable: false),
                    TerminationReasonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeTypes_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeTypes_TerminationReasons_TerminationReasonId",
                        column: x => x.TerminationReasonId,
                        principalTable: "TerminationReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Individuals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankAccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankCurrency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankIBAN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankSwiftCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio_About = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GetUtcDate()"),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisplayOrder = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailPersonal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactRelationshipId = table.Column<int>(type: "int", nullable: true),
                    FacebookId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FathersName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    IdentityNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdentityType = table.Column<int>(type: "int", nullable: false),
                    Initial = table.Column<int>(type: "int", nullable: false),
                    InstagramId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsDobDisplayed = table.Column<bool>(type: "bit", nullable: false),
                    IsPhoneDisplayed = table.Column<bool>(type: "bit", nullable: false),
                    LastContractEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastPromotionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LinkedInId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaritialStatus = table.Column<int>(type: "int", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GetUtcDate()"),
                    MothersName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NationalityId = table.Column<int>(type: "int", nullable: true),
                    NickName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhonePersonal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RCN = table.Column<int>(type: "int", nullable: false),
                    SecondaryLanguageId = table.Column<int>(type: "int", nullable: true),
                    TwitterId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Individuals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Individuals_EmergencyContactRelationships_EmergencyContactRelationshipId",
                        column: x => x.EmergencyContactRelationshipId,
                        principalTable: "EmergencyContactRelationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Individuals_Nationalities_NationalityId",
                        column: x => x.NationalityId,
                        principalTable: "Nationalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Individuals_SecondaryLanguages_SecondaryLanguageId",
                        column: x => x.SecondaryLanguageId,
                        principalTable: "SecondaryLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IndividualEducations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollegeInstitution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GetUtcDate()"),
                    Degree = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DegreeId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    End = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GPA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IndividualId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsOnGoing = table.Column<bool>(type: "bit", nullable: true),
                    MajorSpecilization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GetUtcDate()"),
                    Start = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndividualEducations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndividualEducations_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IndividualPassports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GetUtcDate()"),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IndividualId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssuingCountryCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedByRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GetUtcDate()"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndividualPassports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndividualPassports_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobActionHistories_IndividualId",
                table: "JobActionHistories",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IndividualId",
                table: "Employees",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobs_IndividualId",
                table: "BackgroundJobs",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTypes_EmployeeId",
                table: "EmployeeTypes",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTypes_TerminationReasonId",
                table: "EmployeeTypes",
                column: "TerminationReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualEducations_IndividualId",
                table: "IndividualEducations",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualPassports_IndividualId",
                table: "IndividualPassports",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_EmergencyContactRelationshipId",
                table: "Individuals",
                column: "EmergencyContactRelationshipId");

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_NationalityId",
                table: "Individuals",
                column: "NationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_SecondaryLanguageId",
                table: "Individuals",
                column: "SecondaryLanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_BackgroundJobs_Individuals_IndividualId",
                table: "BackgroundJobs",
                column: "IndividualId",
                principalTable: "Individuals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Individuals_IndividualId",
                table: "Employees",
                column: "IndividualId",
                principalTable: "Individuals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IndividualAddresses_Individuals_IndividualId",
                table: "IndividualAddresses",
                column: "IndividualId",
                principalTable: "Individuals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IndividualAddresses_Locations_LocationId",
                table: "IndividualAddresses",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobActionHistories_Individuals_IndividualId",
                table: "JobActionHistories",
                column: "IndividualId",
                principalTable: "Individuals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
