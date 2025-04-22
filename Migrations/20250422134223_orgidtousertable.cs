using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoaliBackend.Migrations
{
    /// <inheritdoc />
    public partial class orgidtousertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrgId",
                table: "Users",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrgId",
                table: "Users");
        }
    }
}
