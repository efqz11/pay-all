using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class changeJobInfoToEmploymentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Employees_EmployeeId1",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "EmployeeJobInfos");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "EmployeeId1",
                table: "Jobs",
                newName: "ReportingJobId");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_EmployeeId1",
                table: "Jobs",
                newName: "IX_Jobs_ReportingJobId");

            migrationBuilder.CreateTable(
                name: "Employment",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    EffectiveDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    EmployeeId = table.Column<int>(nullable: false),
                    JobId = table.Column<int>(nullable: false),
                    ReportingEmployeeId = table.Column<int>(nullable: true),
                    DirectlyReportingToMD = table.Column<bool>(nullable: false),
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
                    table.PrimaryKey("PK_Employment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employment_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employment_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employment_Employees_ReportingEmployeeId",
                        column: x => x.ReportingEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_JobId",
                table: "Employees",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Employment_EmployeeId",
                table: "Employment",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employment_JobId",
                table: "Employment",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Employment_ReportingEmployeeId",
                table: "Employment",
                column: "ReportingEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Jobs_JobId",
                table: "Employees",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Jobs_ReportingJobId",
                table: "Jobs",
                column: "ReportingJobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Jobs_JobId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Jobs_ReportingJobId",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "Employment");

            migrationBuilder.DropIndex(
                name: "IX_Employees_JobId",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "ReportingJobId",
                table: "Jobs",
                newName: "EmployeeId1");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_ReportingJobId",
                table: "Jobs",
                newName: "IX_Jobs_EmployeeId1");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "Jobs",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeJobInfos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedById = table.Column<string>(nullable: true),
                    CreatedByName = table.Column<string>(nullable: true),
                    CreatedByRoles = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GetUtcDate()"),
                    DepartmentId = table.Column<int>(nullable: false),
                    DirectlyReportingToMD = table.Column<bool>(nullable: false),
                    DivisionId = table.Column<int>(nullable: true),
                    EffectiveDate = table.Column<DateTime>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    JobTitle = table.Column<string>(nullable: true),
                    LocationId = table.Column<int>(nullable: true),
                    ModifiedById = table.Column<string>(nullable: true),
                    ModifiedByName = table.Column<string>(nullable: true),
                    ModifiedByRoles = table.Column<string>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GetUtcDate()"),
                    RecordStatus = table.Column<int>(nullable: false),
                    ReportingEmployeeId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeJobInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeJobInfos_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeJobInfos_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeJobInfos_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeJobInfos_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeJobInfos_Employees_ReportingEmployeeId",
                        column: x => x.ReportingEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeJobInfos_DepartmentId",
                table: "EmployeeJobInfos",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeJobInfos_DivisionId",
                table: "EmployeeJobInfos",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeJobInfos_EmployeeId",
                table: "EmployeeJobInfos",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeJobInfos_LocationId",
                table: "EmployeeJobInfos",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeJobInfos_ReportingEmployeeId",
                table: "EmployeeJobInfos",
                column: "ReportingEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Employees_EmployeeId1",
                table: "Jobs",
                column: "EmployeeId1",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
