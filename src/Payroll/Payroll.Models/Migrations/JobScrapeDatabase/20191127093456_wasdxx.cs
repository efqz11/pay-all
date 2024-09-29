using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.JobScrapeDatabase
{
    public partial class wasdxx : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobsicle_Jobs_Jobsicle_Companies_companyId",
                table: "Jobsicle_Jobs");

            migrationBuilder.AlterColumn<int>(
                name: "companyId",
                table: "Jobsicle_Jobs",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_Jobsicle_Jobs_Jobsicle_Companies_companyId",
                table: "Jobsicle_Jobs",
                column: "companyId",
                principalTable: "Jobsicle_Companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobsicle_Jobs_Jobsicle_Companies_companyId",
                table: "Jobsicle_Jobs");

            migrationBuilder.AlterColumn<int>(
                name: "companyId",
                table: "Jobsicle_Jobs",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobsicle_Jobs_Jobsicle_Companies_companyId",
                table: "Jobsicle_Jobs",
                column: "companyId",
                principalTable: "Jobsicle_Companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
