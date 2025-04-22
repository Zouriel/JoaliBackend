using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoaliBackend.Migrations
{
    /// <inheritdoc />
    public partial class safe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password_hash",
                value: "$2a$11$u9kvVVyl2jMsAn.NoVmaFOKNyVgkJMKCmd/j1R4OCKMt61xHHqx2m");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1001,
                column: "Password_hash",
                value: "6G94qKPK8LYNjnTllCqm2G3BUM08AzOK7yW30tfjrMc=");
        }
    }
}
