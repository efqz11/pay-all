using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.JobScrapeDatabase
{
    public partial class wwwq : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "jobsicleId",
                table: "Jobsicle_Ratings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "jobsicleId",
                table: "Jobsicle_OfficeHourss",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "jobsicleId",
                table: "Jobsicle_Jobs",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "jobsicleId",
                table: "Jobsicle_Companies",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "jobsicleId",
                table: "Jobsicle_Ratings");

            migrationBuilder.DropColumn(
                name: "jobsicleId",
                table: "Jobsicle_OfficeHourss");

            migrationBuilder.DropColumn(
                name: "jobsicleId",
                table: "Jobsicle_Jobs");

            migrationBuilder.DropColumn(
                name: "jobsicleId",
                table: "Jobsicle_Companies");
        }
    }
}
