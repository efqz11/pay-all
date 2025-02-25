using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class locationTiedToAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StreetAddress",
                table: "IndividualAddresses",
                newName: "Street2");

            migrationBuilder.AddColumn<string>(
                name: "Street1",
                table: "IndividualAddresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Street1",
                table: "IndividualAddresses");

            migrationBuilder.RenameColumn(
                name: "Street2",
                table: "IndividualAddresses",
                newName: "StreetAddress");
        }
    }
}
