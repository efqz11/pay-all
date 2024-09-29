using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.PayrollDatabase
{
    public partial class addCompanyFilesAndShares : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileDatas_CompanyFolders_CompanyFolderId",
                table: "FileDatas");

            migrationBuilder.DropIndex(
                name: "IX_FileDatas_CompanyFolderId",
                table: "FileDatas");

            migrationBuilder.DropColumn(
                name: "CompanyFolderId",
                table: "FileDatas");

            migrationBuilder.CreateTable(
                name: "CompanyFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    CompanyFolderId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    FileUrl = table.Column<string>(nullable: true),
                    FileSizeInMb = table.Column<double>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    FileExtension = table.Column<string>(maxLength: 10, nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    ContentLength = table.Column<int>(nullable: false),
                    IsSignatureAvailable = table.Column<bool>(nullable: false),
                    IsSignatureSetupCompleted = table.Column<bool>(nullable: false),
                    FillableConfiguration = table.Column<string>(nullable: true),
                    CreatedById = table.Column<string>(nullable: true),
                    CreatedByName = table.Column<string>(nullable: true),
                    CreatedByRoles = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GetUtcDate()"),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ModifiedById = table.Column<string>(nullable: true),
                    ModifiedByName = table.Column<string>(nullable: true),
                    ModifiedByRoles = table.Column<string>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GetUtcDate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyFiles_CompanyFolders_CompanyFolderId",
                        column: x => x.CompanyFolderId,
                        principalTable: "CompanyFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyFileShares",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    CompanyFileId = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    SharedDate = table.Column<DateTime>(nullable: false),
                    SignedDate = table.Column<DateTime>(nullable: false),
                    IsSigned = table.Column<bool>(nullable: false),
                    FileConfigValues = table.Column<string>(nullable: true),
                    CreatedById = table.Column<string>(nullable: true),
                    CreatedByName = table.Column<string>(nullable: true),
                    CreatedByRoles = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GetUtcDate()"),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ModifiedById = table.Column<string>(nullable: true),
                    ModifiedByName = table.Column<string>(nullable: true),
                    ModifiedByRoles = table.Column<string>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GetUtcDate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyFileShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyFileShares_CompanyFiles_CompanyFileId",
                        column: x => x.CompanyFileId,
                        principalTable: "CompanyFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyFileShares_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFiles_CompanyFolderId",
                table: "CompanyFiles",
                column: "CompanyFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFileShares_CompanyFileId",
                table: "CompanyFileShares",
                column: "CompanyFileId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFileShares_EmployeeId",
                table: "CompanyFileShares",
                column: "EmployeeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyFileShares");

            migrationBuilder.DropTable(
                name: "CompanyFiles");

            migrationBuilder.AddColumn<int>(
                name: "CompanyFolderId",
                table: "FileDatas",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileDatas_CompanyFolderId",
                table: "FileDatas",
                column: "CompanyFolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileDatas_CompanyFolders_CompanyFolderId",
                table: "FileDatas",
                column: "CompanyFolderId",
                principalTable: "CompanyFolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
