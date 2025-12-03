using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfriPay.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserTagWithNormalizedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_UserTag",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "UserTag",
                table: "Customers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserTag",
                table: "Customers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NormalizedUserTag",
                table: "Customers",
                column: "NormalizedUserTag",
                unique: true,
                filter: "[NormalizedUserTag] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_NormalizedUserTag",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "NormalizedUserTag",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "UserTag",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserTag",
                table: "Customers",
                column: "UserTag",
                unique: true,
                filter: "[UserTag] IS NOT NULL");
        }
    }
}
