using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.JobScrapeDatabase
{
    public partial class dddd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobsicle_Jobs_Jobsicle_Companies_companyid",
                table: "Jobsicle_Jobs");

            migrationBuilder.RenameColumn(
                name: "companyid",
                table: "Jobsicle_Jobs",
                newName: "companyId");

            migrationBuilder.RenameIndex(
                name: "IX_Jobsicle_Jobs_companyid",
                table: "Jobsicle_Jobs",
                newName: "IX_Jobsicle_Jobs_companyId");

            migrationBuilder.AlterColumn<string>(
                name: "wednesday",
                table: "Jobsicle_OfficeHourss",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "Jobsicle_OfficeHourss",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<string>(
                name: "tuesday",
                table: "Jobsicle_OfficeHourss",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<string>(
                name: "thursday",
                table: "Jobsicle_OfficeHourss",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<string>(
                name: "sunday",
                table: "Jobsicle_OfficeHourss",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<string>(
                name: "saturday",
                table: "Jobsicle_OfficeHourss",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<string>(
                name: "monday",
                table: "Jobsicle_OfficeHourss",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<string>(
                name: "friday",
                table: "Jobsicle_OfficeHourss",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "Jobsicle_OfficeHourss",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "Jobsicle_Jobs",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "due_date",
                table: "Jobsicle_Jobs",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "Jobsicle_Jobs",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<int>(
                name: "companyId",
                table: "Jobsicle_Jobs",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "Jobsicle_Companies",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_at",
                table: "Jobsicle_Companies",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "Jobsicle_Companies",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AddForeignKey(
                name: "FK_Jobsicle_Jobs_Jobsicle_Companies_companyId",
                table: "Jobsicle_Jobs",
                column: "companyId",
                principalTable: "Jobsicle_Companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobsicle_Jobs_Jobsicle_Companies_companyId",
                table: "Jobsicle_Jobs");

            migrationBuilder.RenameColumn(
                name: "companyId",
                table: "Jobsicle_Jobs",
                newName: "companyid");

            migrationBuilder.RenameIndex(
                name: "IX_Jobsicle_Jobs_companyId",
                table: "Jobsicle_Jobs",
                newName: "IX_Jobsicle_Jobs_companyid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "wednesday",
                table: "Jobsicle_OfficeHourss",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "Jobsicle_OfficeHourss",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "tuesday",
                table: "Jobsicle_OfficeHourss",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "thursday",
                table: "Jobsicle_OfficeHourss",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "sunday",
                table: "Jobsicle_OfficeHourss",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "saturday",
                table: "Jobsicle_OfficeHourss",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "monday",
                table: "Jobsicle_OfficeHourss",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "friday",
                table: "Jobsicle_OfficeHourss",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "Jobsicle_OfficeHourss",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "Jobsicle_Jobs",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "due_date",
                table: "Jobsicle_Jobs",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "Jobsicle_Jobs",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "companyid",
                table: "Jobsicle_Jobs",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "Jobsicle_Companies",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_at",
                table: "Jobsicle_Companies",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "Jobsicle_Companies",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobsicle_Jobs_Jobsicle_Companies_companyid",
                table: "Jobsicle_Jobs",
                column: "companyid",
                principalTable: "Jobsicle_Companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
