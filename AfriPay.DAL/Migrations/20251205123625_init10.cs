using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfriPay.DAL.Migrations
{
    /// <inheritdoc />
    public partial class init10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_BVN",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CustomerReference",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BVN",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Customer_Email",
                table: "Customers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdentityType",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsIdentityVerified",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerReference",
                table: "Customers",
                column: "CustomerReference");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Customer_Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email_IsActive",
                table: "Customers",
                columns: new[] { "Customer_Email", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Identity",
                table: "Customers",
                columns: new[] { "IdentityNumber", "IdentityType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Status",
                table: "Customers",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_CustomerReference",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Email",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Email_IsActive",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Identity",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Status",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Customer_Email",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IdentityType",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsIdentityVerified",
                table: "Customers");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Customers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "BVN",
                table: "Customers",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_BVN",
                table: "Customers",
                column: "BVN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerReference",
                table: "Customers",
                column: "CustomerReference",
                unique: true);
        }
    }
}
