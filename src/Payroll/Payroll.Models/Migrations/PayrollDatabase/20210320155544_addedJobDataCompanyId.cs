using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class addedJobDataCompanyId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classification_Companies_CompanyId",
                table: "Classification");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Classification_ClassificationId",
                table: "Jobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Classification",
                table: "Classification");

            migrationBuilder.RenameTable(
                name: "Classification",
                newName: "Classifications");

            migrationBuilder.RenameIndex(
                name: "IX_Classification_CompanyId",
                table: "Classifications",
                newName: "IX_Classifications_CompanyId");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Jobs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Classifications",
                table: "Classifications",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CompanyId",
                table: "Jobs",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classifications_Companies_CompanyId",
                table: "Classifications",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Classifications_ClassificationId",
                table: "Jobs",
                column: "ClassificationId",
                principalTable: "Classifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Companies_CompanyId",
                table: "Jobs",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classifications_Companies_CompanyId",
                table: "Classifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Classifications_ClassificationId",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Companies_CompanyId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_CompanyId",
                table: "Jobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Classifications",
                table: "Classifications");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Jobs");

            migrationBuilder.RenameTable(
                name: "Classifications",
                newName: "Classification");

            migrationBuilder.RenameIndex(
                name: "IX_Classifications_CompanyId",
                table: "Classification",
                newName: "IX_Classification_CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Classification",
                table: "Classification",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Classification_Companies_CompanyId",
                table: "Classification",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Classification_ClassificationId",
                table: "Jobs",
                column: "ClassificationId",
                principalTable: "Classification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
