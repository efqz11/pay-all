﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Payroll.Database;

namespace Payroll.Migrations.JobScrapeDatabase
{
    [DbContext(typeof(JobScrapeDbContext))]
    [Migration("20191127093456_wasdxx")]
    partial class wasdxx
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.1-servicing-10028")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Payroll.Models.JobScrape.Jobsicle_Company", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CreatedById");

                    b.Property<string>("CreatedByName");

                    b.Property<string>("CreatedByRoles");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("ModifiedById");

                    b.Property<string>("ModifiedByName");

                    b.Property<string>("ModifiedByRoles");

                    b.Property<DateTime?>("ModifiedDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<string>("address");

                    b.Property<int>("allow_ratings");

                    b.Property<bool>("already_rated_by_user");

                    b.Property<int>("category");

                    b.Property<DateTime?>("created_at");

                    b.Property<DateTime?>("deleted_at");

                    b.Property<string>("email");

                    b.Property<int?>("follower_prefix");

                    b.Property<string>("founded_on");

                    b.Property<string>("introduction");

                    b.Property<int>("is_verified");

                    b.Property<int?>("jobsicleId");

                    b.Property<string>("logo");

                    b.Property<string>("mission");

                    b.Property<string>("name");

                    b.Property<int?>("office_hoursid");

                    b.Property<string>("phone");

                    b.Property<int>("rating_count");

                    b.Property<int?>("ratingid");

                    b.Property<string>("sector");

                    b.Property<string>("size");

                    b.Property<DateTime?>("updated_at");

                    b.Property<string>("website");

                    b.Property<string>("why_work_with_us");

                    b.HasKey("id");

                    b.HasIndex("office_hoursid");

                    b.HasIndex("ratingid");

                    b.ToTable("Jobsicle_Companies");
                });

            modelBuilder.Entity("Payroll.Models.JobScrape.Jobsicle_Job", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CreatedById");

                    b.Property<string>("CreatedByName");

                    b.Property<string>("CreatedByRoles");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("ModifiedById");

                    b.Property<string>("ModifiedByName");

                    b.Property<string>("ModifiedByRoles");

                    b.Property<DateTime?>("ModifiedDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<string>("application_form");

                    b.Property<bool>("applied_date");

                    b.Property<string>("attachment");

                    b.Property<string>("category");

                    b.Property<int?>("companyId");

                    b.Property<string>("company_logo");

                    b.Property<string>("company_name");

                    b.Property<DateTime?>("created_at");

                    b.Property<string>("description");

                    b.Property<DateTime>("due_date");

                    b.Property<string>("experience");

                    b.Property<string>("interview_starts_on");

                    b.Property<int>("is_open");

                    b.Property<int?>("jobsicleId");

                    b.Property<string>("location");

                    b.Property<int>("no_of_vacancies");

                    b.Property<string>("notification_email");

                    b.Property<int>("prevent_international_applicants");

                    b.Property<int>("prevent_online_application");

                    b.Property<string>("qualification");

                    b.Property<string>("questions");

                    b.Property<string>("ref_no");

                    b.Property<string>("required_items");

                    b.Property<string>("salary_range");

                    b.Property<string>("sector");

                    b.Property<string>("title");

                    b.Property<int>("total_likes");

                    b.Property<string>("type");

                    b.Property<DateTime?>("updated_at");

                    b.Property<bool>("user_likes_job");

                    b.Property<bool>("user_saved_job");

                    b.HasKey("id");

                    b.HasIndex("companyId");

                    b.ToTable("Jobsicle_Jobs");
                });

            modelBuilder.Entity("Payroll.Models.JobScrape.Jobsicle_OfficeHours", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CreatedById");

                    b.Property<string>("CreatedByName");

                    b.Property<string>("CreatedByRoles");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("ModifiedById");

                    b.Property<string>("ModifiedByName");

                    b.Property<string>("ModifiedByRoles");

                    b.Property<DateTime?>("ModifiedDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<int>("company_id");

                    b.Property<DateTime?>("created_at");

                    b.Property<string>("friday");

                    b.Property<int>("jobsicleId");

                    b.Property<string>("monday");

                    b.Property<string>("saturday");

                    b.Property<string>("sunday");

                    b.Property<string>("thursday");

                    b.Property<string>("tuesday");

                    b.Property<DateTime?>("updated_at");

                    b.Property<string>("wednesday");

                    b.HasKey("id");

                    b.ToTable("Jobsicle_OfficeHourss");
                });

            modelBuilder.Entity("Payroll.Models.JobScrape.Jobsicle_Photo", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CreatedById");

                    b.Property<string>("CreatedByName");

                    b.Property<string>("CreatedByRoles");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("ModifiedById");

                    b.Property<string>("ModifiedByName");

                    b.Property<string>("ModifiedByRoles");

                    b.Property<DateTime?>("ModifiedDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<int>("company_id");

                    b.Property<DateTime?>("created_at");

                    b.Property<int>("jobsicleId");

                    b.Property<string>("picture");

                    b.Property<int>("slot");

                    b.Property<DateTime?>("updated_at");

                    b.HasKey("id");

                    b.HasIndex("company_id");

                    b.ToTable("Jobsicle_Photos");
                });

            modelBuilder.Entity("Payroll.Models.JobScrape.Jobsicle_Rating", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CreatedById");

                    b.Property<string>("CreatedByName");

                    b.Property<string>("CreatedByRoles");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("ModifiedById");

                    b.Property<string>("ModifiedByName");

                    b.Property<string>("ModifiedByRoles");

                    b.Property<DateTime?>("ModifiedDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<string>("compensation");

                    b.Property<string>("culture");

                    b.Property<string>("job_security");

                    b.Property<int>("jobsicleId");

                    b.Property<string>("management");

                    b.Property<string>("work_life_balance");

                    b.HasKey("id");

                    b.ToTable("Jobsicle_Ratings");
                });

            modelBuilder.Entity("Payroll.Models.JobScrape.Jobsicle_Company", b =>
                {
                    b.HasOne("Payroll.Models.JobScrape.Jobsicle_OfficeHours", "office_hours")
                        .WithMany()
                        .HasForeignKey("office_hoursid");

                    b.HasOne("Payroll.Models.JobScrape.Jobsicle_Rating", "rating")
                        .WithMany()
                        .HasForeignKey("ratingid");
                });

            modelBuilder.Entity("Payroll.Models.JobScrape.Jobsicle_Job", b =>
                {
                    b.HasOne("Payroll.Models.JobScrape.Jobsicle_Company", "company")
                        .WithMany()
                        .HasForeignKey("companyId");
                });

            modelBuilder.Entity("Payroll.Models.JobScrape.Jobsicle_Photo", b =>
                {
                    b.HasOne("Payroll.Models.JobScrape.Jobsicle_Company", "company")
                        .WithMany("photos")
                        .HasForeignKey("company_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
