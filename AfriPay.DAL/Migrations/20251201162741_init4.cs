using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfriPay.DAL.Migrations
{
    /// <inheritdoc />
    public partial class init4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OnboardingRequests_Customers_CustomerId",
                table: "OnboardingRequests");

            migrationBuilder.DropIndex(
                name: "IX_OnboardingRequests_BVN",
                table: "OnboardingRequests");

            migrationBuilder.DropColumn(
                name: "BVN",
                table: "OnboardingRequests");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "OnboardingRequests");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "OnboardingRequests");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "OnboardingRequests");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "OnboardingRequests");

            migrationBuilder.RenameColumn(
                name: "RequestedAt",
                table: "OnboardingRequests",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "OnboardingRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "Country",
                table: "OnboardingRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId1",
                table: "OnboardingRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "OnboardingRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "IdentityCountry",
                table: "OnboardingRequests",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "OnboardingRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SelfieUrl",
                table: "OnboardingRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DailyTransferLimit",
                table: "Accounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DailyTransferTotal",
                table: "Accounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDailyResetDate",
                table: "Accounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SingleTransferLimit",
                table: "Accounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AmlScreenings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OnboardingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RiskLevel = table.Column<int>(type: "int", nullable: false),
                    RiskScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ScreeningProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScreeningReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScreeningDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Flags = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RawResponse = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmlScreenings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdentityVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OnboardingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentityNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IdentityCountry = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    VerificationProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VerificationReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VerificationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MatchScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RawResponse = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityVerifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManualReviewCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OnboardingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManualReviewCases", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingRequests_CustomerId1",
                table: "OnboardingRequests",
                column: "CustomerId1");

            migrationBuilder.CreateIndex(
                name: "IX_AmlScreenings_OnboardingId",
                table: "AmlScreenings",
                column: "OnboardingId");

            migrationBuilder.CreateIndex(
                name: "IX_AmlScreenings_RiskLevel",
                table: "AmlScreenings",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_AmlScreenings_ScreeningReference",
                table: "AmlScreenings",
                column: "ScreeningReference");

            migrationBuilder.CreateIndex(
                name: "IX_AmlScreenings_Status",
                table: "AmlScreenings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerifications_OnboardingId",
                table: "IdentityVerifications",
                column: "OnboardingId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerifications_Status",
                table: "IdentityVerifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerifications_VerificationReference",
                table: "IdentityVerifications",
                column: "VerificationReference");

            migrationBuilder.CreateIndex(
                name: "IX_ManualReviewCases_AssignedTo",
                table: "ManualReviewCases",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_ManualReviewCases_OnboardingId",
                table: "ManualReviewCases",
                column: "OnboardingId");

            migrationBuilder.CreateIndex(
                name: "IX_ManualReviewCases_Priority",
                table: "ManualReviewCases",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_ManualReviewCases_Status",
                table: "ManualReviewCases",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_OnboardingRequests_Customers_CustomerId",
                table: "OnboardingRequests",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_OnboardingRequests_Customers_CustomerId1",
                table: "OnboardingRequests",
                column: "CustomerId1",
                principalTable: "Customers",
                principalColumn: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OnboardingRequests_Customers_CustomerId",
                table: "OnboardingRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_OnboardingRequests_Customers_CustomerId1",
                table: "OnboardingRequests");

            migrationBuilder.DropTable(
                name: "AmlScreenings");

            migrationBuilder.DropTable(
                name: "IdentityVerifications");

            migrationBuilder.DropTable(
                name: "ManualReviewCases");

            migrationBuilder.DropIndex(
                name: "IX_OnboardingRequests_CustomerId1",
                table: "OnboardingRequests");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "OnboardingRequests");

            migrationBuilder.DropColumn(
                name: "CustomerId1",
                table: "OnboardingRequests");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "OnboardingRequests");

            migrationBuilder.DropColumn(
                name: "IdentityCountry",
                table: "OnboardingRequests");

            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "OnboardingRequests");

            migrationBuilder.DropColumn(
                name: "SelfieUrl",
                table: "OnboardingRequests");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "DailyTransferLimit",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "DailyTransferTotal",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LastDailyResetDate",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "SingleTransferLimit",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "OnboardingRequests",
                newName: "RequestedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "OnboardingRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "BVN",
                table: "OnboardingRequests",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "OnboardingRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "OnboardingRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "OnboardingRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "OnboardingRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingRequests_BVN",
                table: "OnboardingRequests",
                column: "BVN");

            migrationBuilder.AddForeignKey(
                name: "FK_OnboardingRequests_Customers_CustomerId",
                table: "OnboardingRequests",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
