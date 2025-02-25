using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.AccountDatabase
{
    public partial class userSurveyFixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Survey_InterestedHealthBenefits",
                table: "AppUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "SurveyCs_NeedTrackTime",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SurveyCs_PayingToWhom",
                table: "AppUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SurveyCs_PayingToWhom",
                table: "AppUsers");

            migrationBuilder.AlterColumn<bool>(
                name: "Survey_InterestedHealthBenefits",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "SurveyCs_NeedTrackTime",
                table: "AppUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
