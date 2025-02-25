using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class addJobActionHistoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobActionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    ActionType = table.Column<int>(nullable: false),
                    IndividualId = table.Column<int>(nullable: false),
                    IndividualName = table.Column<string>(nullable: true),
                    JobId = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    OnDate = table.Column<DateTime>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    RelatedRequestId = table.Column<int>(nullable: true),
                    DepartmentId = table.Column<int>(nullable: false),
                    DivisionId = table.Column<int>(nullable: true),
                    LocationId = table.Column<int>(nullable: true),
                    PreviousJobId = table.Column<int>(nullable: true),
                    PreviousEmployeeId = table.Column<int>(nullable: true),
                    Remarks = table.Column<string>(nullable: true),
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
                    table.PrimaryKey("PK_JobActionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobActionHistories_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobActionHistories_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobActionHistories_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobActionHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobActionHistories_Individual_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individual",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobActionHistories_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobActionHistories_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobActionHistories_Employees_PreviousEmployeeId",
                        column: x => x.PreviousEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobActionHistories_Jobs_PreviousJobId",
                        column: x => x.PreviousJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobActionHistories_Requests_RelatedRequestId",
                        column: x => x.RelatedRequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobActionHistories_CompanyId",
                table: "JobActionHistories",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JobActionHistories_DepartmentId",
                table: "JobActionHistories",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_JobActionHistories_DivisionId",
                table: "JobActionHistories",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_JobActionHistories_EmployeeId",
                table: "JobActionHistories",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_JobActionHistories_IndividualId",
                table: "JobActionHistories",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_JobActionHistories_JobId",
                table: "JobActionHistories",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobActionHistories_LocationId",
                table: "JobActionHistories",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobActionHistories_PreviousEmployeeId",
                table: "JobActionHistories",
                column: "PreviousEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_JobActionHistories_PreviousJobId",
                table: "JobActionHistories",
                column: "PreviousJobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobActionHistories_RelatedRequestId",
                table: "JobActionHistories",
                column: "RelatedRequestId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobActionHistories");
        }
    }
}
