using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.JobScrapeDatabase
{
    public partial class initiateDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobsicle_OfficeHourss",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    company_id = table.Column<int>(nullable: false),
                    sunday = table.Column<DateTime>(nullable: false),
                    monday = table.Column<DateTime>(nullable: false),
                    tuesday = table.Column<DateTime>(nullable: false),
                    wednesday = table.Column<DateTime>(nullable: false),
                    thursday = table.Column<DateTime>(nullable: false),
                    friday = table.Column<DateTime>(nullable: false),
                    saturday = table.Column<DateTime>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false),
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
                    table.PrimaryKey("PK_Jobsicle_OfficeHourss", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Jobsicle_Ratings",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    work_life_balance = table.Column<string>(nullable: true),
                    compensation = table.Column<string>(nullable: true),
                    job_security = table.Column<string>(nullable: true),
                    management = table.Column<string>(nullable: true),
                    culture = table.Column<string>(nullable: true),
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
                    table.PrimaryKey("PK_Jobsicle_Ratings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Jobsicle_Companies",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    name = table.Column<string>(nullable: true),
                    email = table.Column<string>(nullable: true),
                    address = table.Column<string>(nullable: true),
                    website = table.Column<string>(nullable: true),
                    phone = table.Column<string>(nullable: true),
                    size = table.Column<string>(nullable: true),
                    sector = table.Column<string>(nullable: true),
                    founded_on = table.Column<string>(nullable: true),
                    mission = table.Column<string>(nullable: true),
                    introduction = table.Column<string>(nullable: true),
                    why_work_with_us = table.Column<string>(nullable: true),
                    logo = table.Column<string>(nullable: true),
                    is_verified = table.Column<int>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false),
                    allow_ratings = table.Column<int>(nullable: false),
                    category = table.Column<int>(nullable: false),
                    deleted_at = table.Column<DateTime>(nullable: false),
                    follower_prefix = table.Column<int>(nullable: false),
                    ratingid = table.Column<int>(nullable: true),
                    already_rated_by_user = table.Column<bool>(nullable: false),
                    office_hoursid = table.Column<int>(nullable: true),
                    rating_count = table.Column<int>(nullable: false),
                    photos = table.Column<string>(nullable: true),
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
                    table.PrimaryKey("PK_Jobsicle_Companies", x => x.id);
                    table.ForeignKey(
                        name: "FK_Jobsicle_Companies_Jobsicle_OfficeHourss_office_hoursid",
                        column: x => x.office_hoursid,
                        principalTable: "Jobsicle_OfficeHourss",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobsicle_Companies_Jobsicle_Ratings_ratingid",
                        column: x => x.ratingid,
                        principalTable: "Jobsicle_Ratings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Jobsicle_Jobs",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    title = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    company_name = table.Column<string>(nullable: true),
                    attachment = table.Column<string>(nullable: true),
                    due_date = table.Column<string>(nullable: true),
                    applied_date = table.Column<bool>(nullable: false),
                    company_logo = table.Column<string>(nullable: true),
                    user_likes_job = table.Column<bool>(nullable: false),
                    user_saved_job = table.Column<bool>(nullable: false),
                    ref_no = table.Column<string>(nullable: true),
                    sector = table.Column<string>(nullable: true),
                    category = table.Column<string>(nullable: true),
                    type = table.Column<string>(nullable: true),
                    experience = table.Column<string>(nullable: true),
                    qualification = table.Column<string>(nullable: true),
                    salary_range = table.Column<string>(nullable: true),
                    location = table.Column<string>(nullable: true),
                    no_of_vacancies = table.Column<int>(nullable: false),
                    is_open = table.Column<int>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false),
                    companyid = table.Column<int>(nullable: true),
                    total_likes = table.Column<int>(nullable: false),
                    questions = table.Column<string>(nullable: true),
                    prevent_international_applicants = table.Column<int>(nullable: false),
                    prevent_online_application = table.Column<int>(nullable: false),
                    application_form = table.Column<string>(nullable: true),
                    interview_starts_on = table.Column<string>(nullable: true),
                    notification_email = table.Column<string>(nullable: true),
                    required_items = table.Column<string>(nullable: true),
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
                    table.PrimaryKey("PK_Jobsicle_Jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_Jobsicle_Jobs_Jobsicle_Companies_companyid",
                        column: x => x.companyid,
                        principalTable: "Jobsicle_Companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobsicle_Companies_office_hoursid",
                table: "Jobsicle_Companies",
                column: "office_hoursid");

            migrationBuilder.CreateIndex(
                name: "IX_Jobsicle_Companies_ratingid",
                table: "Jobsicle_Companies",
                column: "ratingid");

            migrationBuilder.CreateIndex(
                name: "IX_Jobsicle_Jobs_companyid",
                table: "Jobsicle_Jobs",
                column: "companyid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jobsicle_Jobs");

            migrationBuilder.DropTable(
                name: "Jobsicle_Companies");

            migrationBuilder.DropTable(
                name: "Jobsicle_OfficeHourss");

            migrationBuilder.DropTable(
                name: "Jobsicle_Ratings");
        }
    }
}
