using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoaliBackend.Migrations
{
    /// <inheritdoc />
    public partial class fixedservicetypeid3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_bookings",
                table: "bookings");

            migrationBuilder.RenameTable(
                name: "bookings",
                newName: "ServiceOrders");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "ServiceOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceOrders",
                table: "ServiceOrders",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_OrganizationId",
                table: "ServiceOrders",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_ServiceId",
                table: "ServiceOrders",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrders_Organizations_OrganizationId",
                table: "ServiceOrders",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrders_Services_ServiceId",
                table: "ServiceOrders",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrders_Organizations_OrganizationId",
                table: "ServiceOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrders_Services_ServiceId",
                table: "ServiceOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceOrders",
                table: "ServiceOrders");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_OrganizationId",
                table: "ServiceOrders");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_ServiceId",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "ServiceOrders");

            migrationBuilder.RenameTable(
                name: "ServiceOrders",
                newName: "bookings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bookings",
                table: "bookings",
                column: "Id");
        }
    }
}
