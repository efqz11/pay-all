using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.AccountDatabase
{
    public partial class userSurveyFixesNew4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserStatus",
                table: "CompanyAccounts");

            migrationBuilder.AddColumn<int>(
                name: "UserStatus",
                table: "AppUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserStatus",
                table: "AppUsers");

            migrationBuilder.AddColumn<int>(
                name: "UserStatus",
                table: "CompanyAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
