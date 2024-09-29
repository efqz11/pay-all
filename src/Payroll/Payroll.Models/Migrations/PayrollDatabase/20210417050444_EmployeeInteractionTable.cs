using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class EmployeeInteractionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    RequestId = table.Column<int>(nullable: true),
                    AnnouncementId = table.Column<int>(nullable: true),
                    Summary = table.Column<string>(nullable: true),
                    Avatar = table.Column<string>(nullable: true),
                    SummaryHtml = table.Column<string>(nullable: true),
                    NotificationActionTakenType = table.Column<int>(nullable: false),
                    EmployeeInteractionType = table.Column<int>(nullable: false),
                    RemindAbout = table.Column<int>(nullable: false),
                    Remarks = table.Column<string>(nullable: true),
                    SentDate = table.Column<DateTime>(nullable: false),
                    ReceivedDate = table.Column<DateTime>(nullable: true),
                    ActionTakenDate = table.Column<DateTime>(nullable: true),
                    ExpiryDate = table.Column<DateTime>(nullable: true),
                    IsRead = table.Column<bool>(nullable: false),
                    ParentNEmployeeInteractionId = table.Column<int>(nullable: true),
                    Step = table.Column<int>(nullable: false),
                    EmployeesWithRoles = table.Column<string>(nullable: true),
                    ToBeReceivedBy = table.Column<int>(nullable: true),
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
                    table.PrimaryKey("PK_EmployeeInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeInteractions_Announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeInteractions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeInteractions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeInteractions_EmployeeInteractions_ParentNEmployeeInteractionId",
                        column: x => x.ParentNEmployeeInteractionId,
                        principalTable: "EmployeeInteractions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeInteractions_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInteractions_AnnouncementId",
                table: "EmployeeInteractions",
                column: "AnnouncementId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInteractions_CompanyId",
                table: "EmployeeInteractions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInteractions_EmployeeId",
                table: "EmployeeInteractions",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInteractions_ParentNEmployeeInteractionId",
                table: "EmployeeInteractions",
                column: "ParentNEmployeeInteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInteractions_RequestId",
                table: "EmployeeInteractions",
                column: "RequestId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeInteractions");
        }
    }
}
