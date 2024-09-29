using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Models.Migrations.PayrollDatabase
{
    public partial class requestChangeSetFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "JobId",
                table: "JobActionHistories",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "JobActionHistories",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "ChangeSetCount",
                table: "JobActionHistories",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "JobActionHistoryChangeSets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    JobActionHistoryId = table.Column<int>(nullable: false),
                    FieldName = table.Column<string>(nullable: true),
                    NewValue = table.Column<string>(nullable: true),
                    OldValue = table.Column<string>(nullable: true),
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
                    table.PrimaryKey("PK_JobActionHistoryChangeSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobActionHistoryChangeSets_JobActionHistories_JobActionHistoryId",
                        column: x => x.JobActionHistoryId,
                        principalTable: "JobActionHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobActionHistoryChangeSets_JobActionHistoryId",
                table: "JobActionHistoryChangeSets",
                column: "JobActionHistoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobActionHistoryChangeSets");

            migrationBuilder.DropColumn(
                name: "ChangeSetCount",
                table: "JobActionHistories");

            migrationBuilder.AlterColumn<int>(
                name: "JobId",
                table: "JobActionHistories",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "JobActionHistories",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
