using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.AccountDatabase
{
    public partial class dayOfWeekHoliday : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DayOfWeekOffDays",
                table: "CompanyAccounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayOfWeekOffDays",
                table: "CompanyAccounts");
        }
    }
}
