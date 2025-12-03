using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfriPay.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTagToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserTag",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UserTagSetAt",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserTag",
                table: "Customers",
                column: "UserTag",
                unique: true,
                filter: "[UserTag] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_UserTag",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UserTag",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UserTagSetAt",
                table: "Customers");
        }
    }
}
