using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoaliBackend.Migrations
{
    /// <inheritdoc />
    public partial class orgtype1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Organizations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "IsActive", "Name", "OrgId", "Password_hash", "PhoneNumber", "StaffRole", "TemporaryKey", "TemporaryKeyExpiresAt", "UserType", "staffId" },
                values: new object[] { 1001, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@Joali.com", true, "Admin", null, "6G94qKPK8LYNjnTllCqm2G3BUM08AzOK7yW30tfjrMc=", "+9601234567", 0, null, null, 1, "ADM-001" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Organizations");
        }
    }
}
