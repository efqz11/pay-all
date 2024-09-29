using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class requestChangeSetFieldsFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ChangeState",
                table: "JobActionHistoryChangeSets",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobIdString",
                table: "JobActionHistories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedRequestReference",
                table: "JobActionHistories",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeState",
                table: "JobActionHistoryChangeSets");

            migrationBuilder.DropColumn(
                name: "JobIdString",
                table: "JobActionHistories");

            migrationBuilder.DropColumn(
                name: "RelatedRequestReference",
                table: "JobActionHistories");
        }
    }
}
