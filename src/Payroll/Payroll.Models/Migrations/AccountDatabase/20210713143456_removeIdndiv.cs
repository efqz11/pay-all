using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.AccountDatabase
{
    public partial class removeIdndiv : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUsers_CompanyAccounts_CompanyAccountId",
                table: "AppUsers");

            migrationBuilder.DropIndex(
                name: "IX_AppUsers_CompanyAccountId",
                table: "AppUsers");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "CompanyAccounts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAccounts_UserId",
                table: "CompanyAccounts",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyAccounts_AppUsers_UserId",
                table: "CompanyAccounts",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyAccounts_AppUsers_UserId",
                table: "CompanyAccounts");

            migrationBuilder.DropIndex(
                name: "IX_CompanyAccounts_UserId",
                table: "CompanyAccounts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CompanyAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_CompanyAccountId",
                table: "AppUsers",
                column: "CompanyAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUsers_CompanyAccounts_CompanyAccountId",
                table: "AppUsers",
                column: "CompanyAccountId",
                principalTable: "CompanyAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
