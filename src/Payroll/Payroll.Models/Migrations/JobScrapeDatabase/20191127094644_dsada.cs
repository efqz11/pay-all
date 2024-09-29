using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.JobScrapeDatabase
{
    public partial class dsada : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobsicle_Companies_Jobsicle_OfficeHourss_office_hoursid",
                table: "Jobsicle_Companies");

            migrationBuilder.DropTable(
                name: "Jobsicle_OfficeHourss");

            migrationBuilder.CreateTable(
                name: "Jobsicle_OfficeHours",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    jobsicleId = table.Column<int>(nullable: false),
                    company_id = table.Column<int>(nullable: false),
                    sunday = table.Column<string>(nullable: true),
                    monday = table.Column<string>(nullable: true),
                    tuesday = table.Column<string>(nullable: true),
                    wednesday = table.Column<string>(nullable: true),
                    thursday = table.Column<string>(nullable: true),
                    friday = table.Column<string>(nullable: true),
                    saturday = table.Column<string>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: true),
                    updated_at = table.Column<DateTime>(nullable: true),
                    CreatedById = table.Column<string>(nullable: true),
                    CreatedByName = table.Column<string>(nullable: true),
                    CreatedByRoles = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GetUtcDate()"),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ModifiedById = table.Column<string>(nullable: true),
                    ModifiedByName = table.Column<string>(nullable: true),
                    ModifiedByRoles = table.Column<string>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GetUtcDate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobsicle_OfficeHours", x => x.id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Jobsicle_Companies_Jobsicle_OfficeHours_office_hoursid",
                table: "Jobsicle_Companies",
                column: "office_hoursid",
                principalTable: "Jobsicle_OfficeHours",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobsicle_Companies_Jobsicle_OfficeHours_office_hoursid",
                table: "Jobsicle_Companies");

            migrationBuilder.DropTable(
                name: "Jobsicle_OfficeHours");

            migrationBuilder.CreateTable(
                name: "Jobsicle_OfficeHourss",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedById = table.Column<string>(nullable: true),
                    CreatedByName = table.Column<string>(nullable: true),
                    CreatedByRoles = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GetUtcDate()"),
                    IsActive = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ModifiedById = table.Column<string>(nullable: true),
                    ModifiedByName = table.Column<string>(nullable: true),
                    ModifiedByRoles = table.Column<string>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GetUtcDate()"),
                    company_id = table.Column<int>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: true),
                    friday = table.Column<string>(nullable: true),
                    jobsicleId = table.Column<int>(nullable: false),
                    monday = table.Column<string>(nullable: true),
                    saturday = table.Column<string>(nullable: true),
                    sunday = table.Column<string>(nullable: true),
                    thursday = table.Column<string>(nullable: true),
                    tuesday = table.Column<string>(nullable: true),
                    updated_at = table.Column<DateTime>(nullable: true),
                    wednesday = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobsicle_OfficeHourss", x => x.id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Jobsicle_Companies_Jobsicle_OfficeHourss_office_hoursid",
                table: "Jobsicle_Companies",
                column: "office_hoursid",
                principalTable: "Jobsicle_OfficeHourss",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
