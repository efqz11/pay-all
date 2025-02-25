using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class RequestDataChangeNewX : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country1",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmpID",
                table: "RequestDataChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NickName",
                table: "RequestDataChanges",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "Country1",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "EmpID",
                table: "RequestDataChanges");

            migrationBuilder.DropColumn(
                name: "NickName",
                table: "RequestDataChanges");
        }
    }
}
