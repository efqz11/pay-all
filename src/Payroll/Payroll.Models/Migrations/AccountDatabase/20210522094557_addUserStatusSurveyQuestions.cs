using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.AccountDatabase
{
    public partial class addUserStatusSurveyQuestions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_AppUsers_AppUserId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Industries");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Industries");

            migrationBuilder.DropColumn(
                name: "CreatedByRoles",
                table: "Industries");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Industries");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Industries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Industries");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "Industries");

            migrationBuilder.DropColumn(
                name: "ModifiedByName",
                table: "Industries");

            migrationBuilder.DropColumn(
                name: "ModifiedByRoles",
                table: "Industries");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Industries");

            migrationBuilder.AlterColumn<string>(
                name: "AppUserId",
                table: "RefreshTokens",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserStatus",
                table: "CompanyAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SurveyCs_CompanyEntityType",
                table: "AppUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SurveyCs_EmpRoleString",
                table: "AppUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SurveyCs_IndustryId",
                table: "AppUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurveyCs_IndustryOwnWords",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SurveyCs_NeedTrackTime",
                table: "AppUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SurveyCs_NoContractors",
                table: "AppUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SurveyCs_NoW2Employees",
                table: "AppUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SurveyCs_TrackTimeHow",
                table: "AppUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SurveyQn_HowDoRunPayroll",
                table: "AppUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SurveyQn_PayrollThisYear",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Survey_BusinessSetting",
                table: "AppUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Survey_InterestedHealthBenefits",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_SurveyCs_IndustryId",
                table: "AppUsers",
                column: "SurveyCs_IndustryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUsers_Industries_SurveyCs_IndustryId",
                table: "AppUsers",
                column: "SurveyCs_IndustryId",
                principalTable: "Industries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_AppUsers_AppUserId",
                table: "RefreshTokens",
                column: "AppUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUsers_Industries_SurveyCs_IndustryId",
                table: "AppUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_AppUsers_AppUserId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_AppUsers_SurveyCs_IndustryId",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "UserStatus",
                table: "CompanyAccounts");

            migrationBuilder.DropColumn(
                name: "SurveyCs_CompanyEntityType",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "SurveyCs_EmpRoleString",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "SurveyCs_IndustryId",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "SurveyCs_IndustryOwnWords",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "SurveyCs_NeedTrackTime",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "SurveyCs_NoContractors",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "SurveyCs_NoW2Employees",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "SurveyCs_TrackTimeHow",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "SurveyQn_HowDoRunPayroll",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "SurveyQn_PayrollThisYear",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Survey_BusinessSetting",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Survey_InterestedHealthBenefits",
                table: "AppUsers");

            migrationBuilder.AlterColumn<string>(
                name: "AppUserId",
                table: "RefreshTokens",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Industries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Industries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByRoles",
                table: "Industries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Industries",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GetUtcDate()");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Industries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Industries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedById",
                table: "Industries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByName",
                table: "Industries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByRoles",
                table: "Industries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "Industries",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GetUtcDate()");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_AppUsers_AppUserId",
                table: "RefreshTokens",
                column: "AppUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
