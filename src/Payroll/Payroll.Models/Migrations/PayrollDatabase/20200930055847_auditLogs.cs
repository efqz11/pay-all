using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.PayrollDatabase
{
    public partial class auditLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AuditDateTimeUtc = table.Column<DateTime>(nullable: false),
                    AuditType = table.Column<int>(nullable: false),
                    AuditAction = table.Column<int>(nullable: false),
                    AuditUser = table.Column<string>(nullable: true),
                    AuditUserId = table.Column<string>(nullable: true),
                    AuditUserRoles = table.Column<string>(nullable: true),
                    ContextName = table.Column<string>(nullable: true),
                    TableName = table.Column<string>(nullable: true),
                    KeyId = table.Column<string>(nullable: true),
                    KeyValues = table.Column<string>(nullable: true),
                    OldValues = table.Column<string>(nullable: true),
                    NewValues = table.Column<string>(nullable: true),
                    ChangedColumns = table.Column<string>(nullable: true),
                    Newstatus = table.Column<string>(nullable: true),
                    UpdateSummary = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");
        }
    }
}
