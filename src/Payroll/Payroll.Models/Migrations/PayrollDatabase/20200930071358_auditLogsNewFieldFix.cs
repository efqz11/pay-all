using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.PayrollDatabase
{
    public partial class auditLogsNewFieldFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdateSummary",
                table: "AuditLogs",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Newstatus",
                table: "AuditLogs",
                newName: "Message");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "AuditLogs",
                newName: "UpdateSummary");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "AuditLogs",
                newName: "Newstatus");
        }
    }
}
