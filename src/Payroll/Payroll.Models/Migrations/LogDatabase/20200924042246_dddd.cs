using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.LogDatabase
{
    public partial class dddd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Environment",
                table: "ApplicationLogs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ApplicationLogs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "ApplicationLogs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Environment",
                table: "ApplicationLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ApplicationLogs");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "ApplicationLogs");
        }
    }
}
