using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class addedJobData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Classification",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
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
                    table.PrimaryKey("PK_Classification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classification_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    JobID = table.Column<string>(nullable: true),
                    ClassificationId = table.Column<int>(nullable: false),
                    DepartmentId = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    EmployeeId1 = table.Column<int>(nullable: true),
                    DivisionId = table.Column<int>(nullable: true),
                    LocationId = table.Column<int>(nullable: true),
                    JobTitle = table.Column<string>(nullable: true),
                    JobType = table.Column<int>(nullable: false),
                    JobStatus = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Classification_ClassificationId",
                        column: x => x.ClassificationId,
                        principalTable: "Classification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_Employees_EmployeeId1",
                        column: x => x.EmployeeId1,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobPayComponents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    JobId = table.Column<int>(nullable: false),
                    EffectiveDate = table.Column<DateTime>(nullable: false),
                    PayAdjustmentId = table.Column<int>(nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PayComponentChangeReason = table.Column<int>(nullable: true),
                    PayFrequency = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_JobPayComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPayComponents_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobPayComponents_PayAdjustments_PayAdjustmentId",
                        column: x => x.PayAdjustmentId,
                        principalTable: "PayAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Classification_CompanyId",
                table: "Classification",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPayComponents_JobId",
                table: "JobPayComponents",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPayComponents_PayAdjustmentId",
                table: "JobPayComponents",
                column: "PayAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ClassificationId",
                table: "Jobs",
                column: "ClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_DepartmentId",
                table: "Jobs",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_DivisionId",
                table: "Jobs",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_EmployeeId1",
                table: "Jobs",
                column: "EmployeeId1");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_LocationId",
                table: "Jobs",
                column: "LocationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPayComponents");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Classification");
        }
    }
}
