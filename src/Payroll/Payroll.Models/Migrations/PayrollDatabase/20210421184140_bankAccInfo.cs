using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class bankAccInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAddress",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankCurrency",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankIBAN",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankSwiftCode",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAddress",
                table: "Individuals",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankCurrency",
                table: "Individuals",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankIBAN",
                table: "Individuals",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankSwiftCode",
                table: "Individuals",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAddress",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "BankCurrency",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "BankIBAN",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "BankSwiftCode",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "BankAddress",
                table: "Individuals");

            migrationBuilder.DropColumn(
                name: "BankCurrency",
                table: "Individuals");

            migrationBuilder.DropColumn(
                name: "BankIBAN",
                table: "Individuals");

            migrationBuilder.DropColumn(
                name: "BankSwiftCode",
                table: "Individuals");
        }
    }
}
