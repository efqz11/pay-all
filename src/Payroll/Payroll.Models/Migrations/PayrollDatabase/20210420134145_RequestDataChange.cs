using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class RequestDataChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BackgroundJobs_Individual_IndividualId",
                table: "BackgroundJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Individual_IndividualId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Individual_EmergencyContactRelationships_EmergencyContactRelationshipId",
                table: "Individual");

            migrationBuilder.DropForeignKey(
                name: "FK_Individual_Nationalities_NationalityId",
                table: "Individual");

            migrationBuilder.DropForeignKey(
                name: "FK_Individual_SecondaryLanguages_SecondaryLanguageId",
                table: "Individual");

            migrationBuilder.DropForeignKey(
                name: "FK_JobActionHistories_Individual_IndividualId",
                table: "JobActionHistories");

            migrationBuilder.DropTable(
                name: "EmployeeAddresses");

            migrationBuilder.DropTable(
                name: "EmployeeEducations");

            migrationBuilder.DropTable(
                name: "EmployeePassports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Individual",
                table: "Individual");

            migrationBuilder.DropColumn(
                name: "RemindAbout",
                table: "EmployeeInteractions");

            migrationBuilder.RenameTable(
                name: "Individual",
                newName: "Individuals");

            migrationBuilder.RenameIndex(
                name: "IX_Individual_SecondaryLanguageId",
                table: "Individuals",
                newName: "IX_Individuals_SecondaryLanguageId");

            migrationBuilder.RenameIndex(
                name: "IX_Individual_NationalityId",
                table: "Individuals",
                newName: "IX_Individuals_NationalityId");

            migrationBuilder.RenameIndex(
                name: "IX_Individual_EmergencyContactRelationshipId",
                table: "Individuals",
                newName: "IX_Individuals_EmergencyContactRelationshipId");

            migrationBuilder.AddColumn<int>(
                name: "DataChangeEnttity",
                table: "Requests",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestingEmployeeId",
                table: "EmployeeInteractions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestingEmployeeName",
                table: "EmployeeInteractions",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Individuals",
                table: "Individuals",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "IndividualAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    IndividualId = table.Column<int>(nullable: false),
                    AddressType = table.Column<int>(nullable: false),
                    StreetAddress = table.Column<string>(nullable: true),
                    ZipCode = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    CityId = table.Column<int>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    StateId = table.Column<int>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    CountryId = table.Column<int>(nullable: true),
                    RecordStatus = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_IndividualAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndividualAddresses_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IndividualEducations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    IndividualId = table.Column<int>(nullable: false),
                    CollegeInstitution = table.Column<string>(nullable: true),
                    DegreeId = table.Column<int>(nullable: false),
                    Degree = table.Column<string>(nullable: true),
                    MajorSpecilization = table.Column<string>(nullable: true),
                    GPA = table.Column<string>(nullable: true),
                    Start = table.Column<DateTime>(nullable: false),
                    End = table.Column<DateTime>(nullable: true),
                    IsOnGoing = table.Column<bool>(nullable: true),
                    DisplayOrder = table.Column<int>(nullable: false),
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    IndividualId = table.Column<int>(nullable: false),
                    Number = table.Column<string>(nullable: true),
                    IssuedDate = table.Column<DateTime>(nullable: false),
                    ExpirationDate = table.Column<DateTime>(nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    IssuingCountryCode = table.Column<string>(nullable: false),
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
                    table.PrimaryKey("PK_IndividualPassports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndividualPassports_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RequestDataChanges",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RequestId = table.Column<int>(nullable: false),
                    PropertyName = table.Column<string>(nullable: true),
                    PropertyValue = table.Column<string>(nullable: true),
                    PropertyType = table.Column<string>(nullable: true),
                    Remarks = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestDataChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestDataChanges_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndividualAddresses_IndividualId",
                table: "IndividualAddresses",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualEducations_IndividualId",
                table: "IndividualEducations",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualPassports_IndividualId",
                table: "IndividualPassports",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestDataChanges_RequestId",
                table: "RequestDataChanges",
                column: "RequestId");

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
                name: "FK_Individuals_EmergencyContactRelationships_EmergencyContactRelationshipId",
                table: "Individuals",
                column: "EmergencyContactRelationshipId",
                principalTable: "EmergencyContactRelationships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Individuals_Nationalities_NationalityId",
                table: "Individuals",
                column: "NationalityId",
                principalTable: "Nationalities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Individuals_SecondaryLanguages_SecondaryLanguageId",
                table: "Individuals",
                column: "SecondaryLanguageId",
                principalTable: "SecondaryLanguages",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BackgroundJobs_Individuals_IndividualId",
                table: "BackgroundJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Individuals_IndividualId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Individuals_EmergencyContactRelationships_EmergencyContactRelationshipId",
                table: "Individuals");

            migrationBuilder.DropForeignKey(
                name: "FK_Individuals_Nationalities_NationalityId",
                table: "Individuals");

            migrationBuilder.DropForeignKey(
                name: "FK_Individuals_SecondaryLanguages_SecondaryLanguageId",
                table: "Individuals");

            migrationBuilder.DropForeignKey(
                name: "FK_JobActionHistories_Individuals_IndividualId",
                table: "JobActionHistories");

            migrationBuilder.DropTable(
                name: "IndividualAddresses");

            migrationBuilder.DropTable(
                name: "IndividualEducations");

            migrationBuilder.DropTable(
                name: "IndividualPassports");

            migrationBuilder.DropTable(
                name: "RequestDataChanges");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Individuals",
                table: "Individuals");

            migrationBuilder.DropColumn(
                name: "DataChangeEnttity",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RequestingEmployeeId",
                table: "EmployeeInteractions");

            migrationBuilder.DropColumn(
                name: "RequestingEmployeeName",
                table: "EmployeeInteractions");

            migrationBuilder.RenameTable(
                name: "Individuals",
                newName: "Individual");

            migrationBuilder.RenameIndex(
                name: "IX_Individuals_SecondaryLanguageId",
                table: "Individual",
                newName: "IX_Individual_SecondaryLanguageId");

            migrationBuilder.RenameIndex(
                name: "IX_Individuals_NationalityId",
                table: "Individual",
                newName: "IX_Individual_NationalityId");

            migrationBuilder.RenameIndex(
                name: "IX_Individuals_EmergencyContactRelationshipId",
                table: "Individual",
                newName: "IX_Individual_EmergencyContactRelationshipId");

            migrationBuilder.AddColumn<int>(
                name: "RemindAbout",
                table: "EmployeeInteractions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Individual",
                table: "Individual",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "EmployeeAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AddressType = table.Column<int>(nullable: false),
                    City = table.Column<string>(nullable: true),
                    CityId = table.Column<int>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    CountryId = table.Column<int>(nullable: true),
                    CreatedById = table.Column<string>(nullable: true),
                    CreatedByName = table.Column<string>(nullable: true),
                    CreatedByRoles = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GetUtcDate()"),
                    EmployeeId = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ModifiedById = table.Column<string>(nullable: true),
                    ModifiedByName = table.Column<string>(nullable: true),
                    ModifiedByRoles = table.Column<string>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GetUtcDate()"),
                    RecordStatus = table.Column<int>(nullable: false),
                    State = table.Column<string>(nullable: true),
                    StateId = table.Column<int>(nullable: true),
                    StreetAddress = table.Column<string>(nullable: true),
                    ZipCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeAddresses_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeEducations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CollegeInstitution = table.Column<string>(nullable: true),
                    CreatedById = table.Column<string>(nullable: true),
                    CreatedByName = table.Column<string>(nullable: true),
                    CreatedByRoles = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GetUtcDate()"),
                    Degree = table.Column<string>(nullable: true),
                    DegreeId = table.Column<int>(nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    End = table.Column<DateTime>(nullable: true),
                    GPA = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsOnGoing = table.Column<bool>(nullable: true),
                    MajorSpecilization = table.Column<string>(nullable: true),
                    ModifiedById = table.Column<string>(nullable: true),
                    ModifiedByName = table.Column<string>(nullable: true),
                    ModifiedByRoles = table.Column<string>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GetUtcDate()"),
                    Start = table.Column<DateTime>(nullable: false)
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedById = table.Column<string>(nullable: true),
                    CreatedByName = table.Column<string>(nullable: true),
                    CreatedByRoles = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GetUtcDate()"),
                    EmployeeId = table.Column<int>(nullable: false),
                    ExpirationDate = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IssuedDate = table.Column<DateTime>(nullable: false),
                    IssuingCountryCode = table.Column<string>(nullable: false),
                    ModifiedById = table.Column<string>(nullable: true),
                    ModifiedByName = table.Column<string>(nullable: true),
                    ModifiedByRoles = table.Column<string>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GetUtcDate()"),
                    Notes = table.Column<string>(nullable: true),
                    Number = table.Column<string>(nullable: true)
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
                name: "IX_EmployeeAddresses_EmployeeId",
                table: "EmployeeAddresses",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Individual_EmergencyContactRelationships_EmergencyContactRelationshipId",
                table: "Individual",
                column: "EmergencyContactRelationshipId",
                principalTable: "EmergencyContactRelationships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Individual_Nationalities_NationalityId",
                table: "Individual",
                column: "NationalityId",
                principalTable: "Nationalities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Individual_SecondaryLanguages_SecondaryLanguageId",
                table: "Individual",
                column: "SecondaryLanguageId",
                principalTable: "SecondaryLanguages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobActionHistories_Individual_IndividualId",
                table: "JobActionHistories",
                column: "IndividualId",
                principalTable: "Individual",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
