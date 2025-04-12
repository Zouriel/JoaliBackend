using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoaliBackend.Migrations
{
    /// <inheritdoc />
    public partial class AuthProcessStreamLined : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsACtive",
                table: "Users",
                newName: "IsActive");

            migrationBuilder.AddColumn<int>(
                name: "StaffRole",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemporaryKey",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TemporaryKeyExpiresAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "staffId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    userId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Device = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Os = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Client = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_userId",
                table: "Sessions",
                column: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropColumn(
                name: "StaffRole",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TemporaryKey",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TemporaryKeyExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "staffId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Users",
                newName: "IsACtive");
        }
    }
}
