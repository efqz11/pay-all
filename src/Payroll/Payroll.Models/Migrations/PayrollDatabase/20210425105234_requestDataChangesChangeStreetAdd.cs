using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class requestDataChangesChangeStreetAdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Street1",
                table: "RequestDataChanges",
                newName: "StreetAddress1");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "RequestDataChanges",
                newName: "StreetAddress");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StreetAddress1",
                table: "RequestDataChanges",
                newName: "Street1");

            migrationBuilder.RenameColumn(
                name: "StreetAddress",
                table: "RequestDataChanges",
                newName: "Street");
        }
    }
}
