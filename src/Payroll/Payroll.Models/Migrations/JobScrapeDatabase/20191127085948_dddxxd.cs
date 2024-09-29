using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.JobScrapeDatabase
{
    public partial class dddxxd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "follower_prefix",
                table: "Jobsicle_Companies",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "follower_prefix",
                table: "Jobsicle_Companies",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
