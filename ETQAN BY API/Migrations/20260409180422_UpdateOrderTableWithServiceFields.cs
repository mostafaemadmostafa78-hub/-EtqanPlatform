using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETQAN_BY_API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderTableWithServiceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArtisanId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsServiceOrder",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ServiceRequestId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ArtisanId",
                table: "Orders",
                column: "ArtisanId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ServiceRequestId",
                table: "Orders",
                column: "ServiceRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Artisans_ArtisanId",
                table: "Orders",
                column: "ArtisanId",
                principalTable: "Artisans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ServiceRequests_ServiceRequestId",
                table: "Orders",
                column: "ServiceRequestId",
                principalTable: "ServiceRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Artisans_ArtisanId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ServiceRequests_ServiceRequestId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ArtisanId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ServiceRequestId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ArtisanId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsServiceOrder",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ServiceRequestId",
                table: "Orders");
        }
    }
}
