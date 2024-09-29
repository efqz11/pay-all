using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payroll.Migrations.PayrollDatabase
{
    public partial class initiateDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RowId = table.Column<string>(maxLength: 50, nullable: false),
                    TableName = table.Column<string>(maxLength: 128, nullable: false),
                    Changed = table.Column<string>(nullable: true),
                    Kind = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Address = table.Column<string>(nullable: false),
                    LogoUrl = table.Column<string>(nullable: true),
                    Website = table.Column<string>(nullable: true),
                    Hotline = table.Column<string>(nullable: true),
                    TaxCode = table.Column<string>(nullable: true),
                    TaxPercentValue = table.Column<decimal>(nullable: true),
                    ManagingDirector = table.Column<string>(nullable: true),
                    WorkType = table.Column<string>(nullable: false),
                    FlexibleBreakHourCount = table.Column<double>(nullable: false),
                    IsBreakHourStrict = table.Column<bool>(nullable: false),
                    EarlyOntimeMinutes = table.Column<double>(nullable: false),
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
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyFolders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Color = table.Column<string>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_CompanyFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyFolders_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyPublicHolidays",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Year = table.Column<string>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    IsImported = table.Column<bool>(nullable: false),
                    IsManualEntry = table.Column<bool>(nullable: false),
                    IsPublicHoliday = table.Column<bool>(nullable: false),
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
                    table.PrimaryKey("PK_CompanyPublicHolidays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyPublicHolidays_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyWorkBreakTimes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    StartTime = table.Column<TimeSpan>(nullable: false),
                    EndTime = table.Column<TimeSpan>(nullable: false),
                    IsFlexible = table.Column<bool>(nullable: false),
                    TotakBreaks = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_CompanyWorkBreakTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyWorkBreakTimes_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyWorkTimes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    IsShift = table.Column<bool>(nullable: false),
                    ShiftName = table.Column<string>(nullable: true),
                    StartTime = table.Column<TimeSpan>(nullable: false),
                    EndTime = table.Column<TimeSpan>(nullable: false),
                    ColorCombination = table.Column<string>(nullable: true),
                    TotakBreaks = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_CompanyWorkTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyWorkTimes_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DayOffs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    TotalDays = table.Column<int>(nullable: false),
                    CanRequestForBackDatedDays = table.Column<bool>(nullable: false),
                    MustRequestBefore = table.Column<bool>(nullable: false),
                    MustRequestBeforeAlert = table.Column<int>(nullable: true),
                    ExcludeForPublicHoliday = table.Column<bool>(nullable: false),
                    RequiredDocumentForConseqetiveDays = table.Column<bool>(nullable: false),
                    ConsquetiveDaysRequire = table.Column<int>(nullable: false),
                    RequiredDocuments = table.Column<bool>(nullable: false),
                    RequiredDocumentList = table.Column<string>(nullable: true),
                    IsEmergency = table.Column<bool>(nullable: false),
                    IsForSpecificGender = table.Column<bool>(nullable: false),
                    Gender = table.Column<int>(nullable: true),
                    IsByRequest = table.Column<bool>(nullable: false),
                    IsEnteredManually = table.Column<bool>(nullable: false),
                    ResetEveryYear = table.Column<bool>(nullable: false),
                    ResetAnnuallyFromJoinDate = table.Column<bool>(nullable: false),
                    IsApplicableOnProbation = table.Column<bool>(nullable: false),
                    IsApplicationAfterFirstYear = table.Column<bool>(nullable: false),
                    CanPlanAhead = table.Column<bool>(nullable: false),
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
                    table.PrimaryKey("PK_DayOffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayOffs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Degrees",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CompanyId = table.Column<int>(nullable: true),
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
                    table.PrimaryKey("PK_Degrees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Degrees_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    RenewedFrom = table.Column<string>(nullable: true),
                    DeptCode = table.Column<string>(maxLength: 10, nullable: true),
                    TotalHeadCount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Divisions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    Code = table.Column<string>(maxLength: 10, nullable: true),
                    TotalHeadCount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_Divisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Divisions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyContactRelationships",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CompanyId = table.Column<int>(nullable: true),
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
                    table.PrimaryKey("PK_EmergencyContactRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyContactRelationships_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Nationality",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CompanyId = table.Column<int>(nullable: true),
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
                    table.PrimaryKey("PK_Nationality", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nationality_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    VariationType = table.Column<int>(nullable: false),
                    CalculationOrder = table.Column<int>(nullable: false),
                    IsPensionCharge = table.Column<bool>(nullable: true),
                    IsFilledByEmployee = table.Column<bool>(nullable: true),
                    EnforceRequirement = table.Column<bool>(nullable: true),
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
                    table.PrimaryKey("PK_PayAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayAdjustments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Summary = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    PayDate = table.Column<DateTime>(nullable: true),
                    CompanyId = table.Column<int>(nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    GrossPay = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    NetSalaryLastPeriod = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    GrossPayLastPeriod = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Status = table.Column<string>(nullable: false),
                    IsGenerated = table.Column<bool>(nullable: false),
                    LastGeneratedDateTime = table.Column<DateTime>(nullable: true),
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
                    table.PrimaryKey("PK_PayrollPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollPeriods_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SecondaryLanguages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CompanyId = table.Column<int>(nullable: true),
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
                    table.PrimaryKey("PK_SecondaryLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecondaryLanguages_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TerminationReasons",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CompanyId = table.Column<int>(nullable: true),
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
                    table.PrimaryKey("PK_TerminationReasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TerminationReasons_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Visa",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CompanyId = table.Column<int>(nullable: true),
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
                    table.PrimaryKey("PK_Visa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visa_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    EmployeeIds = table.Column<string>(nullable: true),
                    WorkTimeIds = table.Column<string>(nullable: true),
                    IgnoreDaysData = table.Column<string>(nullable: true),
                    EmployeeIdsData = table.Column<string>(nullable: true),
                    WorkTimeIdsData = table.Column<string>(nullable: true),
                    DaysData = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    End = table.Column<DateTime>(nullable: true),
                    Start = table.Column<DateTime>(nullable: false),
                    Slots = table.Column<int>(nullable: false),
                    TimeZone = table.Column<string>(nullable: true),
                    MinHours = table.Column<int>(nullable: false),
                    IsRepeating = table.Column<bool>(nullable: false),
                    IsForAllEmployees = table.Column<bool>(nullable: false),
                    IsEffectiveImmediately = table.Column<bool>(nullable: false),
                    EffectiveDate = table.Column<DateTime>(nullable: true),
                    Repeat = table.Column<int>(nullable: false),
                    RepeatEndDate = table.Column<DateTime>(nullable: true),
                    IsRepeatEndDateNever = table.Column<bool>(nullable: false),
                    ScheduleFor = table.Column<string>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    HasBackgroundJob = table.Column<bool>(nullable: true),
                    HangfireJobId = table.Column<string>(nullable: true),
                    BackgroundJobId = table.Column<int>(nullable: true),
                    BackgroundJobDetails = table.Column<string>(nullable: true),
                    bySchduler = table.Column<bool>(nullable: false),
                    NextRunDate = table.Column<DateTime>(nullable: true),
                    HasBackgroundJobEnded = table.Column<bool>(nullable: true),
                    DepartmentId = table.Column<int>(nullable: true),
                    IsForDepartment = table.Column<bool>(nullable: false),
                    WorkId = table.Column<int>(nullable: false),
                    WorkName = table.Column<string>(nullable: true),
                    SelectedMenu = table.Column<int>(nullable: false),
                    _Patten = table.Column<string>(nullable: true),
                    _PattenString = table.Column<string>(nullable: true),
                    _ConseqetiveDays = table.Column<int>(nullable: false),
                    _TotalWorkingHoursPerWeek = table.Column<double>(nullable: false),
                    RosterGeneratedDate = table.Column<DateTime>(nullable: false),
                    ParentScheduleId = table.Column<int>(nullable: true),
                    HaveConflicts = table.Column<bool>(nullable: false),
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
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Schedules_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Schedules_Schedules_ParentScheduleId",
                        column: x => x.ParentScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Works",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    DepartmentId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Details = table.Column<string>(nullable: true),
                    StartTime = table.Column<TimeSpan>(nullable: false),
                    EndTime = table.Column<TimeSpan>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    MinsBeforeCheckIn = table.Column<double>(nullable: false),
                    TotalRequiredCount = table.Column<int>(nullable: false),
                    LessDeduct = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MoreCredit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false),
                    Frequency = table.Column<int>(nullable: false),
                    IsAdvancedCreate = table.Column<bool>(nullable: false),
                    HasTime = table.Column<bool>(nullable: false),
                    ColorCombination = table.Column<string>(nullable: true),
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
                    table.PrimaryKey("PK_Works", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Works_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Works_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayAdjustmentFieldConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    PayAdjustmentId = table.Column<int>(nullable: true),
                    BaseType = table.Column<int>(nullable: false),
                    FieldType = table.Column<int>(nullable: false),
                    ListType = table.Column<int>(nullable: false),
                    ListFilter = table.Column<string>(nullable: true),
                    ListSelect = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    Calculation = table.Column<string>(nullable: true),
                    CalculationOrder = table.Column<int>(nullable: false),
                    CalculationIdentifier = table.Column<string>(nullable: true),
                    IsClientCalculatable = table.Column<bool>(nullable: false),
                    IsServerCalculatable = table.Column<bool>(nullable: false),
                    IsAggregated = table.Column<bool>(nullable: false),
                    IsEditable = table.Column<bool>(nullable: false),
                    WorkId = table.Column<int>(nullable: true),
                    OnBlur = table.Column<string>(nullable: true),
                    UpdateInputClass = table.Column<string>(nullable: true),
                    EvalMethod = table.Column<string>(nullable: true),
                    IsReturn = table.Column<bool>(nullable: false),
                    HasCeilValue = table.Column<bool>(nullable: false),
                    CeilValueCalculation = table.Column<string>(nullable: true),
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
                    table.PrimaryKey("PK_PayAdjustmentFieldConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayAdjustmentFieldConfigs_PayAdjustments_PayAdjustmentId",
                        column: x => x.PayAdjustmentId,
                        principalTable: "PayAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Percent = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Grade = table.Column<string>(nullable: true),
                    CssClass = table.Column<string>(nullable: true),
                    PercentStr = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true),
                    UserName = table.Column<string>(nullable: true),
                    UserPicture = table.Column<string>(nullable: true),
                    EmailAddress = table.Column<string>(nullable: true),
                    WorkPhone = table.Column<string>(nullable: true),
                    WorkExt = table.Column<string>(maxLength: 5, nullable: true),
                    PhonePersonal = table.Column<string>(nullable: true),
                    TaxFileNumber = table.Column<string>(nullable: true),
                    Bio_About = table.Column<string>(nullable: true),
                    EmergencyContactName = table.Column<string>(nullable: true),
                    EmergencyContactNumber = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: false),
                    HasUserAccount = table.Column<bool>(nullable: false),
                    IsResponsibleForPensionCharges = table.Column<bool>(nullable: false),
                    IsResponsibleForAccounts = table.Column<bool>(nullable: false),
                    NickName = table.Column<string>(nullable: true),
                    Gender = table.Column<string>(nullable: false),
                    Initial = table.Column<int>(nullable: false),
                    FirstName = table.Column<string>(nullable: false),
                    MiddleName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Avatar = table.Column<string>(nullable: true),
                    NationalityId = table.Column<int>(nullable: false),
                    Nationality = table.Column<string>(nullable: true),
                    HangfireJobId = table.Column<string>(nullable: true),
                    BackgroundJobId = table.Column<int>(nullable: true),
                    SecondaryLanguageId = table.Column<int>(nullable: true),
                    DateOfJoined = table.Column<DateTime>(nullable: true),
                    DateOfTermination = table.Column<DateTime>(nullable: true),
                    DateOfBirth = table.Column<DateTime>(nullable: true),
                    EmpID = table.Column<int>(nullable: false),
                    IdentityType = table.Column<string>(nullable: false),
                    IdentityNumber = table.Column<string>(nullable: true),
                    BankName = table.Column<string>(nullable: true),
                    BankAccountName = table.Column<string>(nullable: true),
                    BankAccountNumber = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Street = table.Column<string>(nullable: true),
                    ZipCode = table.Column<string>(nullable: true),
                    DisplayOrder = table.Column<string>(nullable: true),
                    JobType = table.Column<string>(nullable: true),
                    IsContractActive = table.Column<bool>(nullable: false),
                    IsContractEnding = table.Column<bool>(nullable: false),
                    IsOnProbation = table.Column<bool>(nullable: false),
                    TwitterId = table.Column<string>(nullable: true),
                    FacebookId = table.Column<string>(nullable: true),
                    InstagramId = table.Column<string>(nullable: true),
                    LinkedInId = table.Column<string>(nullable: true),
                    HasBackgroundJob = table.Column<bool>(nullable: true),
                    JobId = table.Column<int>(nullable: true),
                    BackgroundJobDetails = table.Column<string>(nullable: true),
                    bySchduler = table.Column<bool>(nullable: false),
                    NextRunDate = table.Column<DateTime>(nullable: true),
                    HasBackgroundJobEnded = table.Column<bool>(nullable: true),
                    IsContractEnded = table.Column<bool>(nullable: false),
                    ActiveContractName = table.Column<string>(nullable: true),
                    ActiveContractId = table.Column<int>(nullable: false),
                    JobTitle = table.Column<string>(nullable: true),
                    DepartmentId = table.Column<int>(nullable: true),
                    DivisionId = table.Column<int>(nullable: true),
                    LocationId = table.Column<int>(nullable: true),
                    EmploymentStatus = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_SecondaryLanguages_SecondaryLanguageId",
                        column: x => x.SecondaryLanguageId,
                        principalTable: "SecondaryLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedEmployeeId = table.Column<int>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    EmployeeSelectorVm = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    Start = table.Column<DateTime>(nullable: true),
                    End = table.Column<DateTime>(nullable: true),
                    Title = table.Column<string>(maxLength: 100, nullable: false),
                    Summary = table.Column<string>(nullable: true),
                    HasSummary = table.Column<bool>(nullable: false),
                    ViewedCount = table.Column<int>(nullable: false),
                    TotalInteractionsCount = table.Column<int>(nullable: false),
                    EmployeeIdsData = table.Column<string>(nullable: true),
                    PublishedDate = table.Column<DateTime>(nullable: true),
                    ExpiredDate = table.Column<DateTime>(nullable: true),
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
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announcements_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Announcements_Employees_CreatedEmployeeId",
                        column: x => x.CreatedEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    ScheduleId = table.Column<int>(nullable: true),
                    CompanyId = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    Week = table.Column<int>(nullable: false),
                    Day = table.Column<int>(nullable: false),
                    Month = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    CompanyWorkTimeId = table.Column<int>(nullable: false),
                    ShiftId = table.Column<int>(nullable: false),
                    ShiftName = table.Column<string>(nullable: true),
                    ShiftColor = table.Column<string>(nullable: true),
                    WorkStartTime = table.Column<DateTime>(nullable: false),
                    WorkEndTime = table.Column<DateTime>(nullable: false),
                    CheckInTime = table.Column<DateTime>(nullable: true),
                    CheckOutTime = table.Column<DateTime>(nullable: true),
                    IsCheckinUpdated = table.Column<bool>(nullable: false),
                    IsCheckOutUpdated = table.Column<bool>(nullable: false),
                    TotalEarlyMins = table.Column<double>(nullable: false),
                    TotalLateMins = table.Column<double>(nullable: false),
                    TotalAfterWorkMins = table.Column<double>(nullable: false),
                    TotalWorkedHours = table.Column<double>(nullable: false),
                    TotalHoursWorkedPerSchedule = table.Column<double>(nullable: false),
                    TotalWorkedHoursCalculated = table.Column<double>(nullable: false),
                    TotalHoursWorkedOutOfSchedule = table.Column<double>(nullable: false),
                    WorkedOverTime = table.Column<bool>(nullable: false),
                    HasClockRecords = table.Column<bool>(nullable: false),
                    IsTransferred = table.Column<bool>(nullable: false),
                    CurrentStatus = table.Column<string>(nullable: false),
                    StatusUpdates = table.Column<string>(nullable: true),
                    bySchduler = table.Column<bool>(nullable: false),
                    IsCreatedFromRequest = table.Column<bool>(nullable: false),
                    CreatedFromRequestId = table.Column<int>(nullable: true),
                    IsOvertime = table.Column<bool>(nullable: false),
                    IsPublished = table.Column<bool>(nullable: false),
                    PublishedDate = table.Column<DateTime>(nullable: false),
                    HasError = table.Column<bool>(nullable: false),
                    ErroMsg = table.Column<string>(nullable: true),
                    TotalBreakHours = table.Column<double>(nullable: false),
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
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendances_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attendances_CompanyWorkTimes_CompanyWorkTimeId",
                        column: x => x.CompanyWorkTimeId,
                        principalTable: "CompanyWorkTimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attendances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attendances_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DayOffEmployees",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    DayOffId = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    TotalDays = table.Column<int>(nullable: false),
                    TotalRemainingDays = table.Column<int>(nullable: false),
                    TotalCollectedDays = table.Column<int>(nullable: false),
                    NextRefreshDate = table.Column<DateTime>(nullable: false),
                    Year = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_DayOffEmployees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayOffEmployees_DayOffs_DayOffId",
                        column: x => x.DayOffId,
                        principalTable: "DayOffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DayOffEmployees_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentHeads",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: true),
                    DepartmentId = table.Column<int>(nullable: true),
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
                    table.PrimaryKey("PK_DepartmentHeads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentHeads_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentHeads_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeEducations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    CollegeInstitution = table.Column<string>(nullable: true),
                    DegreeId = table.Column<int>(nullable: false),
                    Degree = table.Column<string>(nullable: true),
                    MajorSpecilization = table.Column<string>(nullable: true),
                    GPA = table.Column<string>(nullable: true),
                    Start = table.Column<DateTime>(nullable: false),
                    End = table.Column<DateTime>(nullable: true),
                    IsOnGoing = table.Column<bool>(nullable: true),
                    DisplayOrder = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_EmployeeEducations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeEducations_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeJobInfos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    EffectiveDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    EmployeeId = table.Column<int>(nullable: false),
                    DepartmentId = table.Column<int>(nullable: false),
                    DivisionId = table.Column<int>(nullable: true),
                    LocationId = table.Column<int>(nullable: true),
                    JobTitle = table.Column<string>(nullable: true),
                    ReportingEmployeeId = table.Column<int>(nullable: false),
                    DirectlyReportingToMD = table.Column<bool>(nullable: false),
                    RecordStatus = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_EmployeeJobInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeJobInfos_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeJobInfos_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeJobInfos_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeJobInfos_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeJobInfos_Employees_ReportingEmployeeId",
                        column: x => x.ReportingEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePassports",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    Number = table.Column<string>(nullable: true),
                    IssuedDate = table.Column<DateTime>(nullable: false),
                    ExpirationDate = table.Column<DateTime>(nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    IssuingCountryCode = table.Column<string>(nullable: false),
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
                    table.PrimaryKey("PK_EmployeePassports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeePassports_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePayComponents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    EffectiveDate = table.Column<DateTime>(nullable: false),
                    PayAdjustmentId = table.Column<int>(nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PayComponentChangeReason = table.Column<int>(nullable: true),
                    PayFrequency = table.Column<int>(nullable: false),
                    RecordStatus = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_EmployeePayComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeePayComponents_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeePayComponents_PayAdjustments_PayAdjustmentId",
                        column: x => x.PayAdjustmentId,
                        principalTable: "PayAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    EffectiveDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    Comment = table.Column<string>(nullable: true),
                    EmploymentStatus = table.Column<int>(nullable: false),
                    RecordStatus = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_EmployeeTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeTypes_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeVisaInfos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    VisaId = table.Column<int>(nullable: false),
                    IssuedDate = table.Column<DateTime>(nullable: false),
                    ExpirationDate = table.Column<DateTime>(nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    VisaStatus = table.Column<int>(nullable: false),
                    IssuingCountryCode = table.Column<string>(nullable: false),
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
                    table.PrimaryKey("PK_EmployeeVisaInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeVisaInfos_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeVisaInfos_Visa_VisaId",
                        column: x => x.VisaId,
                        principalTable: "Visa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollPeriodEmployees",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    EmpID = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    PayrollPeriodId = table.Column<int>(nullable: false),
                    GrossPay = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Designation = table.Column<string>(nullable: true),
                    WorkedHours = table.Column<double>(nullable: false),
                    WorkedMins = table.Column<double>(nullable: false),
                    LateMins = table.Column<double>(nullable: false),
                    LateHours = table.Column<double>(nullable: false),
                    LateDays = table.Column<int>(nullable: false),
                    AbsentDays = table.Column<int>(nullable: false),
                    OvertimeHours = table.Column<double>(nullable: false),
                    OvertimeMins = table.Column<double>(nullable: false),
                    LeaveDays = table.Column<double>(nullable: false),
                    TaskCompletedCount = table.Column<double>(nullable: false),
                    TaskFailedCount = table.Column<double>(nullable: false),
                    TaskCreditSum = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TaskDebitSum = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OvertimeCount = table.Column<int>(nullable: false),
                    WorkedRecordsCount = table.Column<int>(nullable: false),
                    TaskSubmissionsCount = table.Column<int>(nullable: false),
                    TaskRemainingCount = table.Column<int>(nullable: false),
                    ChartDataX = table.Column<string>(nullable: true),
                    Grade = table.Column<string>(nullable: true),
                    Percent = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PercentStr = table.Column<string>(nullable: true),
                    GradeGeneratedDateTime = table.Column<DateTime>(nullable: true),
                    IsGraded = table.Column<bool>(nullable: false),
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
                    table.PrimaryKey("PK_PayrollPeriodEmployees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollPeriodEmployees_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollPeriodEmployees_PayrollPeriods_PayrollPeriodId",
                        column: x => x.PayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollPeriodPayAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PayrollPeriodId = table.Column<int>(nullable: false),
                    EmployeeName = table.Column<string>(nullable: true),
                    EmployeeId = table.Column<int>(nullable: false),
                    VariationType = table.Column<int>(nullable: false),
                    Adjustment = table.Column<string>(nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CalculationOrder = table.Column<int>(nullable: false),
                    PayAdjustmentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPeriodPayAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollPeriodPayAdjustments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollPeriodPayAdjustments_PayAdjustments_PayAdjustmentId",
                        column: x => x.PayAdjustmentId,
                        principalTable: "PayAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollPeriodPayAdjustments_PayrollPeriods_PayrollPeriodId",
                        column: x => x.PayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BackgroundJobs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    ScheduleId = table.Column<int>(nullable: true),
                    AnnouncementId = table.Column<int>(nullable: true),
                    EmployeeId = table.Column<int>(nullable: true),
                    CompanyAccountId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    TaskType = table.Column<string>(nullable: false),
                    TaskStatus = table.Column<int>(nullable: false),
                    HangfireJobId = table.Column<string>(nullable: true),
                    NextRunDate = table.Column<DateTime>(nullable: true),
                    EndActions = table.Column<string>(nullable: true),
                    Details = table.Column<string>(nullable: true),
                    RunDate = table.Column<DateTime>(nullable: false),
                    EndedDate = table.Column<DateTime>(nullable: true),
                    Identifier = table.Column<Guid>(nullable: false),
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
                    table.PrimaryKey("PK_BackgroundJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BackgroundJobs_Announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BackgroundJobs_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    WorkId = table.Column<int>(nullable: true),
                    ScheduleId = table.Column<int>(nullable: true),
                    Week = table.Column<int>(nullable: false),
                    Day = table.Column<int>(nullable: false),
                    Month = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    HasAttendance = table.Column<bool>(nullable: false),
                    AttendnaceId = table.Column<int>(nullable: true),
                    AttendanceId = table.Column<int>(nullable: true),
                    EmployeeId = table.Column<int>(nullable: false),
                    WorkStartTime = table.Column<DateTime>(nullable: false),
                    WorkEndTime = table.Column<DateTime>(nullable: false),
                    DueDate = table.Column<DateTime>(nullable: true),
                    CheckInTime = table.Column<DateTime>(nullable: true),
                    CheckOutTime = table.Column<DateTime>(nullable: true),
                    TotalEarlyMins = table.Column<double>(nullable: false),
                    TotalLateMins = table.Column<double>(nullable: false),
                    TotalWorkedMins = table.Column<double>(nullable: false),
                    TotalApproved = table.Column<int>(nullable: false),
                    PercentApproved = table.Column<int>(nullable: false),
                    TotalSubmitted = table.Column<int>(nullable: false),
                    PercentSubmitted = table.Column<int>(nullable: false),
                    RemainingSubmissions = table.Column<int>(nullable: false),
                    TotalAmountCredited = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalAmountDeducted = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IsCompleted = table.Column<bool>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    WorkColor = table.Column<string>(nullable: true),
                    IsEmployeeTask = table.Column<bool>(nullable: false),
                    TaskName = table.Column<string>(nullable: true),
                    TaskDescription = table.Column<string>(nullable: true),
                    IsTransferred = table.Column<bool>(nullable: false),
                    IsCheckinUpdated = table.Column<bool>(nullable: false),
                    IsCheckOutUpdated = table.Column<bool>(nullable: false),
                    IsPublished = table.Column<bool>(nullable: false),
                    PublishedDate = table.Column<DateTime>(nullable: false),
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
                    table.PrimaryKey("PK_WorkItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkItems_Attendances_AttendanceId",
                        column: x => x.AttendanceId,
                        principalTable: "Attendances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItems_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItems_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItems_Works_WorkId",
                        column: x => x.WorkId,
                        principalTable: "Works",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KpiValues",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: true),
                    PayrollPeriodEmployeeId = table.Column<int>(nullable: true),
                    Kpi = table.Column<string>(nullable: true),
                    Best = table.Column<int>(nullable: false),
                    Worst = table.Column<int>(nullable: false),
                    Actual = table.Column<int>(nullable: false),
                    Percent = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Str = table.Column<string>(nullable: true),
                    IsChanged = table.Column<bool>(nullable: false),
                    ChangedDate = table.Column<DateTime>(nullable: false),
                    PercentIncrease = table.Column<float>(nullable: false),
                    PercentDecrease = table.Column<float>(nullable: false),
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
                    table.PrimaryKey("PK_KpiValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiValues_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KpiValues_PayrollPeriodEmployees_PayrollPeriodEmployeeId",
                        column: x => x.PayrollPeriodEmployeeId,
                        principalTable: "PayrollPeriodEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VariationKeyValues",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    PayrollPeriodEmployeeId = table.Column<int>(nullable: false),
                    KeyId = table.Column<int>(nullable: false),
                    Key = table.Column<string>(nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MultiOrder = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_VariationKeyValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VariationKeyValues_PayrollPeriodEmployees_PayrollPeriodEmployeeId",
                        column: x => x.PayrollPeriodEmployeeId,
                        principalTable: "PayrollPeriodEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollPeriodPayAdjustmentFieldValues",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PayrollPeriodPayAdjustmentId = table.Column<int>(nullable: false),
                    PayAdjustment = table.Column<string>(nullable: true),
                    VariationType = table.Column<int>(nullable: false),
                    Key = table.Column<string>(nullable: true),
                    IsManualEntry = table.Column<bool>(nullable: false),
                    ValueString = table.Column<string>(nullable: true),
                    ValueInt = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    BaseType = table.Column<int>(nullable: false),
                    FieldType = table.Column<int>(nullable: false),
                    ListType = table.Column<int>(nullable: false),
                    ListFilter = table.Column<string>(nullable: true),
                    ListSelect = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    Calculation = table.Column<string>(nullable: true),
                    CalculationOrder = table.Column<int>(nullable: false),
                    CalculationIdentifier = table.Column<string>(nullable: true),
                    ThresholdValue = table.Column<bool>(nullable: false),
                    IsAggregated = table.Column<bool>(nullable: false),
                    IsEditable = table.Column<bool>(nullable: false),
                    IsClientCalculatable = table.Column<bool>(nullable: false),
                    IsServerCalculatable = table.Column<bool>(nullable: false),
                    OnBlur = table.Column<string>(nullable: true),
                    UpdateInputClass = table.Column<string>(nullable: true),
                    EvalMethod = table.Column<string>(nullable: true),
                    IsReturn = table.Column<bool>(nullable: false),
                    DisplayedValueFrontEnd = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPeriodPayAdjustmentFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollPeriodPayAdjustmentFieldValues_PayrollPeriodPayAdjustments_PayrollPeriodPayAdjustmentId",
                        column: x => x.PayrollPeriodPayAdjustmentId,
                        principalTable: "PayrollPeriodPayAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BiometricRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    AttendanceId = table.Column<int>(nullable: true),
                    WorkItemId = table.Column<int>(nullable: true),
                    Week = table.Column<int>(nullable: false),
                    Day = table.Column<int>(nullable: false),
                    Month = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Time = table.Column<TimeSpan>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    Hour = table.Column<int>(nullable: false),
                    Minute = table.Column<int>(nullable: false),
                    Second = table.Column<int>(nullable: false),
                    BiometricRecordType = table.Column<int>(nullable: false),
                    BiometricRecordState = table.Column<int>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    IsManualEntry = table.Column<bool>(nullable: false),
                    OrderBy = table.Column<int>(nullable: false),
                    Data = table.Column<string>(nullable: true),
                    MachineId = table.Column<string>(nullable: true),
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
                    table.PrimaryKey("PK_BiometricRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BiometricRecords_Attendances_AttendanceId",
                        column: x => x.AttendanceId,
                        principalTable: "Attendances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BiometricRecords_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BiometricRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BiometricRecords_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    RequestType = table.Column<string>(nullable: false),
                    SubmissionDate = table.Column<DateTime>(nullable: true),
                    ActionTakenUserId = table.Column<string>(nullable: true),
                    ActionTakenUserName = table.Column<string>(nullable: true),
                    ActionTakenDate = table.Column<DateTime>(nullable: true),
                    ActionTakenReasonSummary = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: false),
                    DayOffId = table.Column<int>(nullable: true),
                    Start = table.Column<DateTime>(nullable: true),
                    End = table.Column<DateTime>(nullable: true),
                    Reason = table.Column<string>(nullable: true),
                    IsCustomReason = table.Column<bool>(nullable: false),
                    AttendanceId = table.Column<int>(nullable: true),
                    WorkItemId = table.Column<int>(nullable: true),
                    IsTransferEmployee = table.Column<bool>(nullable: false),
                    TransferredEmployeeName = table.Column<string>(nullable: true),
                    TransferredEmployeeId = table.Column<int>(nullable: true),
                    NewCheckinTime = table.Column<TimeSpan>(nullable: true),
                    NewCheckOutTime = table.Column<TimeSpan>(nullable: true),
                    DocumentType = table.Column<string>(nullable: true),
                    IsLetter = table.Column<bool>(nullable: false),
                    ImagePath = table.Column<string>(nullable: true),
                    DocumentsData = table.Column<string>(nullable: true),
                    StatusUpdates = table.Column<string>(nullable: true),
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
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Attendances_AttendanceId",
                        column: x => x.AttendanceId,
                        principalTable: "Attendances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_DayOffs_DayOffId",
                        column: x => x.DayOffId,
                        principalTable: "DayOffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_Employees_TransferredEmployeeId",
                        column: x => x.TransferredEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkItemSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    WorkItemId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Details = table.Column<string>(nullable: true),
                    IsApproved = table.Column<bool>(nullable: false),
                    SentForApprovalDate = table.Column<DateTime>(nullable: true),
                    ActionTakenUserId = table.Column<string>(nullable: true),
                    ActionTakenUserName = table.Column<string>(nullable: true),
                    ActionTakenDate = table.Column<DateTime>(nullable: true),
                    ActionTakenReasonSummary = table.Column<string>(nullable: true),
                    SubmissionDate = table.Column<DateTime>(nullable: true),
                    Status = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_WorkItemSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkItemSubmissions_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DayOffEmployeeItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    DayOffEmployeeId = table.Column<int>(nullable: false),
                    RequestId = table.Column<int>(nullable: true),
                    Start = table.Column<DateTime>(nullable: false),
                    End = table.Column<DateTime>(nullable: false),
                    TotalDays = table.Column<int>(nullable: false),
                    IsCreatedFromRequest = table.Column<bool>(nullable: false),
                    CreatedFromRequestId = table.Column<int>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    CreatedManually = table.Column<bool>(nullable: false),
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
                    table.PrimaryKey("PK_DayOffEmployeeItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayOffEmployeeItems_DayOffEmployees_DayOffEmployeeId",
                        column: x => x.DayOffEmployeeId,
                        principalTable: "DayOffEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DayOffEmployeeItems_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileDatas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsActive = table.Column<bool>(nullable: false),
                    IsNameChangeable = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    IsUploaded = table.Column<bool>(nullable: false),
                    FileType = table.Column<string>(nullable: false),
                    FileUrl = table.Column<string>(nullable: true),
                    FileExtension = table.Column<string>(maxLength: 10, nullable: true),
                    Description = table.Column<string>(nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    ContentLength = table.Column<int>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    CompanyFolderId = table.Column<int>(nullable: true),
                    WorkItemId = table.Column<int>(nullable: true),
                    RequestId = table.Column<int>(nullable: true),
                    EmployeeId = table.Column<int>(nullable: true),
                    AnnouncementId = table.Column<int>(nullable: true),
                    DisplayOrder = table.Column<int>(nullable: false),
                    IsSignatureAvailable = table.Column<bool>(nullable: false),
                    IsSignatureSetupCompleted = table.Column<bool>(nullable: false),
                    FileSizeInMb = table.Column<double>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    CreatedById = table.Column<string>(nullable: true),
                    CreatedByName = table.Column<string>(nullable: true),
                    CreatedByRoles = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GetUtcDate()"),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ModifiedById = table.Column<string>(nullable: true),
                    ModifiedByName = table.Column<string>(nullable: true),
                    ModifiedByRoles = table.Column<string>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GetUtcDate()"),
                    WorkItemSubmissionId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileDatas_Announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileDatas_CompanyFolders_CompanyFolderId",
                        column: x => x.CompanyFolderId,
                        principalTable: "CompanyFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileDatas_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileDatas_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileDatas_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileDatas_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileDatas_WorkItemSubmissions_WorkItemSubmissionId",
                        column: x => x.WorkItemSubmissionId,
                        principalTable: "WorkItemSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_CompanyId",
                table: "Announcements",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_CreatedEmployeeId",
                table: "Announcements",
                column: "CreatedEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_CompanyId",
                table: "Attendances",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_CompanyWorkTimeId",
                table: "Attendances",
                column: "CompanyWorkTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_EmployeeId",
                table: "Attendances",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_ScheduleId",
                table: "Attendances",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobs_AnnouncementId",
                table: "BackgroundJobs",
                column: "AnnouncementId");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobs_ScheduleId",
                table: "BackgroundJobs",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_BiometricRecords_AttendanceId",
                table: "BiometricRecords",
                column: "AttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_BiometricRecords_CompanyId",
                table: "BiometricRecords",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BiometricRecords_EmployeeId",
                table: "BiometricRecords",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_BiometricRecords_WorkItemId",
                table: "BiometricRecords",
                column: "WorkItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFolders_CompanyId",
                table: "CompanyFolders",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyPublicHolidays_CompanyId",
                table: "CompanyPublicHolidays",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyWorkBreakTimes_CompanyId",
                table: "CompanyWorkBreakTimes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyWorkTimes_CompanyId",
                table: "CompanyWorkTimes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOffEmployeeItems_DayOffEmployeeId",
                table: "DayOffEmployeeItems",
                column: "DayOffEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOffEmployeeItems_RequestId",
                table: "DayOffEmployeeItems",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOffEmployees_DayOffId",
                table: "DayOffEmployees",
                column: "DayOffId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOffEmployees_EmployeeId",
                table: "DayOffEmployees",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOffs_CompanyId",
                table: "DayOffs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Degrees_CompanyId",
                table: "Degrees",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentHeads_DepartmentId",
                table: "DepartmentHeads",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentHeads_EmployeeId",
                table: "DepartmentHeads",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CompanyId",
                table: "Departments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_CompanyId",
                table: "Divisions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContactRelationships_CompanyId",
                table: "EmergencyContactRelationships",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEducations_EmployeeId",
                table: "EmployeeEducations",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeJobInfos_DepartmentId",
                table: "EmployeeJobInfos",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeJobInfos_DivisionId",
                table: "EmployeeJobInfos",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeJobInfos_EmployeeId",
                table: "EmployeeJobInfos",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeJobInfos_LocationId",
                table: "EmployeeJobInfos",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeJobInfos_ReportingEmployeeId",
                table: "EmployeeJobInfos",
                column: "ReportingEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePassports_EmployeeId",
                table: "EmployeePassports",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayComponents_EmployeeId",
                table: "EmployeePayComponents",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayComponents_PayAdjustmentId",
                table: "EmployeePayComponents",
                column: "PayAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DivisionId",
                table: "Employees",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_LocationId",
                table: "Employees",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_SecondaryLanguageId",
                table: "Employees",
                column: "SecondaryLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTypes_EmployeeId",
                table: "EmployeeTypes",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeVisaInfos_EmployeeId",
                table: "EmployeeVisaInfos",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeVisaInfos_VisaId",
                table: "EmployeeVisaInfos",
                column: "VisaId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDatas_AnnouncementId",
                table: "FileDatas",
                column: "AnnouncementId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDatas_CompanyFolderId",
                table: "FileDatas",
                column: "CompanyFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDatas_CompanyId",
                table: "FileDatas",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDatas_EmployeeId",
                table: "FileDatas",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDatas_RequestId",
                table: "FileDatas",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDatas_WorkItemId",
                table: "FileDatas",
                column: "WorkItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDatas_WorkItemSubmissionId",
                table: "FileDatas",
                column: "WorkItemSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiValues_EmployeeId",
                table: "KpiValues",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiValues_PayrollPeriodEmployeeId",
                table: "KpiValues",
                column: "PayrollPeriodEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_CompanyId",
                table: "Locations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Nationality_CompanyId",
                table: "Nationality",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PayAdjustmentFieldConfigs_PayAdjustmentId",
                table: "PayAdjustmentFieldConfigs",
                column: "PayAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PayAdjustments_CompanyId",
                table: "PayAdjustments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriodEmployees_EmployeeId",
                table: "PayrollPeriodEmployees",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriodEmployees_PayrollPeriodId",
                table: "PayrollPeriodEmployees",
                column: "PayrollPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriodPayAdjustmentFieldValues_PayrollPeriodPayAdjustmentId",
                table: "PayrollPeriodPayAdjustmentFieldValues",
                column: "PayrollPeriodPayAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriodPayAdjustments_EmployeeId",
                table: "PayrollPeriodPayAdjustments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriodPayAdjustments_PayAdjustmentId",
                table: "PayrollPeriodPayAdjustments",
                column: "PayAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriodPayAdjustments_PayrollPeriodId",
                table: "PayrollPeriodPayAdjustments",
                column: "PayrollPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_CompanyId",
                table: "PayrollPeriods",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_AttendanceId",
                table: "Requests",
                column: "AttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CompanyId",
                table: "Requests",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_DayOffId",
                table: "Requests",
                column: "DayOffId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_EmployeeId",
                table: "Requests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_TransferredEmployeeId",
                table: "Requests",
                column: "TransferredEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_WorkItemId",
                table: "Requests",
                column: "WorkItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_CompanyId",
                table: "Schedules",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DepartmentId",
                table: "Schedules",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ParentScheduleId",
                table: "Schedules",
                column: "ParentScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_SecondaryLanguages_CompanyId",
                table: "SecondaryLanguages",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminationReasons_CompanyId",
                table: "TerminationReasons",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_VariationKeyValues_PayrollPeriodEmployeeId",
                table: "VariationKeyValues",
                column: "PayrollPeriodEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Visa_CompanyId",
                table: "Visa",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_AttendanceId",
                table: "WorkItems",
                column: "AttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_EmployeeId",
                table: "WorkItems",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ScheduleId",
                table: "WorkItems",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_WorkId",
                table: "WorkItems",
                column: "WorkId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemSubmissions_WorkItemId",
                table: "WorkItemSubmissions",
                column: "WorkItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Works_CompanyId",
                table: "Works",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Works_DepartmentId",
                table: "Works",
                column: "DepartmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoHistory");

            migrationBuilder.DropTable(
                name: "BackgroundJobs");

            migrationBuilder.DropTable(
                name: "BiometricRecords");

            migrationBuilder.DropTable(
                name: "CompanyPublicHolidays");

            migrationBuilder.DropTable(
                name: "CompanyWorkBreakTimes");

            migrationBuilder.DropTable(
                name: "DayOffEmployeeItems");

            migrationBuilder.DropTable(
                name: "Degrees");

            migrationBuilder.DropTable(
                name: "DepartmentHeads");

            migrationBuilder.DropTable(
                name: "EmergencyContactRelationships");

            migrationBuilder.DropTable(
                name: "EmployeeEducations");

            migrationBuilder.DropTable(
                name: "EmployeeJobInfos");

            migrationBuilder.DropTable(
                name: "EmployeePassports");

            migrationBuilder.DropTable(
                name: "EmployeePayComponents");

            migrationBuilder.DropTable(
                name: "EmployeeTypes");

            migrationBuilder.DropTable(
                name: "EmployeeVisaInfos");

            migrationBuilder.DropTable(
                name: "FileDatas");

            migrationBuilder.DropTable(
                name: "KpiValues");

            migrationBuilder.DropTable(
                name: "Nationality");

            migrationBuilder.DropTable(
                name: "PayAdjustmentFieldConfigs");

            migrationBuilder.DropTable(
                name: "PayrollPeriodPayAdjustmentFieldValues");

            migrationBuilder.DropTable(
                name: "TerminationReasons");

            migrationBuilder.DropTable(
                name: "VariationKeyValues");

            migrationBuilder.DropTable(
                name: "DayOffEmployees");

            migrationBuilder.DropTable(
                name: "Visa");

            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "CompanyFolders");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "WorkItemSubmissions");

            migrationBuilder.DropTable(
                name: "PayrollPeriodPayAdjustments");

            migrationBuilder.DropTable(
                name: "PayrollPeriodEmployees");

            migrationBuilder.DropTable(
                name: "DayOffs");

            migrationBuilder.DropTable(
                name: "WorkItems");

            migrationBuilder.DropTable(
                name: "PayAdjustments");

            migrationBuilder.DropTable(
                name: "PayrollPeriods");

            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "Works");

            migrationBuilder.DropTable(
                name: "CompanyWorkTimes");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "Divisions");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "SecondaryLanguages");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
