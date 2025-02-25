using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.PayrollDatabase
{
    public partial class auditLogsNewFieldFix2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullContextName",
                table: "AuditLogs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullContextName",
                table: "AuditLogs");
        }
    }
}
