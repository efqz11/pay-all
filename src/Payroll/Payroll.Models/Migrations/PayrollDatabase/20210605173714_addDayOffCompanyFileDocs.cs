using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class addDayOffCompanyFileDocs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmploymentStatus",
                table: "Employees",
                newName: "RecordStatus");

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactEmail",
                table: "Individuals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDobDisplayed",
                table: "Individuals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPhoneDisplayed",
                table: "Individuals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "IndividualId",
                table: "IndividualAddresses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "IndividualAddresses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeSecondaryStatus",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeStatus",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyForFutureHires",
                table: "CompanyFiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CompanyFileShareType",
                table: "CompanyFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyFileType",
                table: "CompanyFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "DoesRequireToBeFilled",
                table: "CompanyFiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HtmlTemplate",
                table: "CompanyFiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecordStatus",
                table: "CompanyFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SignitorySignature",
                table: "CompanyFiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignitoryTitle",
                table: "CompanyFiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndividualAddresses_LocationId",
                table: "IndividualAddresses",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_IndividualAddresses_Locations_LocationId",
                table: "IndividualAddresses",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndividualAddresses_Locations_LocationId",
                table: "IndividualAddresses");

            migrationBuilder.DropIndex(
                name: "IX_IndividualAddresses_LocationId",
                table: "IndividualAddresses");

            migrationBuilder.DropColumn(
                name: "EmergencyContactEmail",
                table: "Individuals");

            migrationBuilder.DropColumn(
                name: "IsDobDisplayed",
                table: "Individuals");

            migrationBuilder.DropColumn(
                name: "IsPhoneDisplayed",
                table: "Individuals");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "IndividualAddresses");

            migrationBuilder.DropColumn(
                name: "EmployeeSecondaryStatus",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmployeeStatus",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ApplyForFutureHires",
                table: "CompanyFiles");

            migrationBuilder.DropColumn(
                name: "CompanyFileShareType",
                table: "CompanyFiles");

            migrationBuilder.DropColumn(
                name: "CompanyFileType",
                table: "CompanyFiles");

            migrationBuilder.DropColumn(
                name: "DoesRequireToBeFilled",
                table: "CompanyFiles");

            migrationBuilder.DropColumn(
                name: "HtmlTemplate",
                table: "CompanyFiles");

            migrationBuilder.DropColumn(
                name: "RecordStatus",
                table: "CompanyFiles");

            migrationBuilder.DropColumn(
                name: "SignitorySignature",
                table: "CompanyFiles");

            migrationBuilder.DropColumn(
                name: "SignitoryTitle",
                table: "CompanyFiles");

            migrationBuilder.RenameColumn(
                name: "RecordStatus",
                table: "Employees",
                newName: "EmploymentStatus");

            migrationBuilder.AlterColumn<int>(
                name: "IndividualId",
                table: "IndividualAddresses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
