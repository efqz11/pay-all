using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class changeBackEmploymentToEmployee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employment_Employees_EmployeeId",
                table: "Employment");

            migrationBuilder.DropForeignKey(
                name: "FK_Employment_Jobs_JobId",
                table: "Employment");

            migrationBuilder.DropForeignKey(
                name: "FK_Employment_Employees_ReportingEmployeeId",
                table: "Employment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employment",
                table: "Employment");

            migrationBuilder.RenameTable(
                name: "Employment",
                newName: "Employments");

            migrationBuilder.RenameIndex(
                name: "IX_Employment_ReportingEmployeeId",
                table: "Employments",
                newName: "IX_Employments_ReportingEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Employment_JobId",
                table: "Employments",
                newName: "IX_Employments_JobId");

            migrationBuilder.RenameIndex(
                name: "IX_Employment_EmployeeId",
                table: "Employments",
                newName: "IX_Employments_EmployeeId");

            migrationBuilder.AddColumn<int>(
                name: "EmploymentId",
                table: "EmployeePayComponents",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JobId",
                table: "AuditLogs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employments",
                table: "Employments",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "EmployeeActions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    JobId = table.Column<int>(nullable: true),
                    PayAdjustmentId = table.Column<int>(nullable: true),
                    EmployeePayComponentId = table.Column<int>(nullable: true),
                    OnDate = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    Remarks = table.Column<string>(nullable: true),
                    ActionType = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_EmployeeActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeActions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeActions_EmployeePayComponents_EmployeePayComponentId",
                        column: x => x.EmployeePayComponentId,
                        principalTable: "EmployeePayComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeActions_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeActions_PayAdjustments_PayAdjustmentId",
                        column: x => x.PayAdjustmentId,
                        principalTable: "PayAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayComponents_EmploymentId",
                table: "EmployeePayComponents",
                column: "EmploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeActions_EmployeeId",
                table: "EmployeeActions",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeActions_EmployeePayComponentId",
                table: "EmployeeActions",
                column: "EmployeePayComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeActions_JobId",
                table: "EmployeeActions",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeActions_PayAdjustmentId",
                table: "EmployeeActions",
                column: "PayAdjustmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeePayComponents_Employments_EmploymentId",
                table: "EmployeePayComponents",
                column: "EmploymentId",
                principalTable: "Employments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employments_Employees_EmployeeId",
                table: "Employments",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employments_Jobs_JobId",
                table: "Employments",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employments_Employees_ReportingEmployeeId",
                table: "Employments",
                column: "ReportingEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeePayComponents_Employments_EmploymentId",
                table: "EmployeePayComponents");

            migrationBuilder.DropForeignKey(
                name: "FK_Employments_Employees_EmployeeId",
                table: "Employments");

            migrationBuilder.DropForeignKey(
                name: "FK_Employments_Jobs_JobId",
                table: "Employments");

            migrationBuilder.DropForeignKey(
                name: "FK_Employments_Employees_ReportingEmployeeId",
                table: "Employments");

            migrationBuilder.DropTable(
                name: "EmployeeActions");

            migrationBuilder.DropIndex(
                name: "IX_EmployeePayComponents_EmploymentId",
                table: "EmployeePayComponents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employments",
                table: "Employments");

            migrationBuilder.DropColumn(
                name: "EmploymentId",
                table: "EmployeePayComponents");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "AuditLogs");

            migrationBuilder.RenameTable(
                name: "Employments",
                newName: "Employment");

            migrationBuilder.RenameIndex(
                name: "IX_Employments_ReportingEmployeeId",
                table: "Employment",
                newName: "IX_Employment_ReportingEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Employments_JobId",
                table: "Employment",
                newName: "IX_Employment_JobId");

            migrationBuilder.RenameIndex(
                name: "IX_Employments_EmployeeId",
                table: "Employment",
                newName: "IX_Employment_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employment",
                table: "Employment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employment_Employees_EmployeeId",
                table: "Employment",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employment_Jobs_JobId",
                table: "Employment",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employment_Employees_ReportingEmployeeId",
                table: "Employment",
                column: "ReportingEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
