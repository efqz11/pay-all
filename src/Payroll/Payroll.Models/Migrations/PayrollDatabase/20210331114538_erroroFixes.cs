using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class erroroFixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRoleRelations_EmployeeRoles_EmployeeRoleId",
                table: "EmployeeRoleRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_EmployeeRoles_EmployeeRoleId",
                table: "Reminders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeRoles",
                table: "EmployeeRoles");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "EmployeeRoles",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "EmployeeRoles",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeRoles",
                table: "EmployeeRoles",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRoleRelations_EmployeeRoles_EmployeeRoleId",
                table: "EmployeeRoleRelations",
                column: "EmployeeRoleId",
                principalTable: "EmployeeRoles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_EmployeeRoles_EmployeeRoleId",
                table: "Reminders",
                column: "EmployeeRoleId",
                principalTable: "EmployeeRoles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRoleRelations_EmployeeRoles_EmployeeRoleId",
                table: "EmployeeRoleRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_EmployeeRoles_EmployeeRoleId",
                table: "Reminders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeRoles",
                table: "EmployeeRoles");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "EmployeeRoles");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "EmployeeRoles",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeRoles",
                table: "EmployeeRoles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRoleRelations_EmployeeRoles_EmployeeRoleId",
                table: "EmployeeRoleRelations",
                column: "EmployeeRoleId",
                principalTable: "EmployeeRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_EmployeeRoles_EmployeeRoleId",
                table: "Reminders",
                column: "EmployeeRoleId",
                principalTable: "EmployeeRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
