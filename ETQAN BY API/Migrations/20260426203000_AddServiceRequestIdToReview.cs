using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETQAN_BY_API.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceRequestIdToReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ServiceRequestId",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ClientId",
                table: "Reviews",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ServiceRequestId",
                table: "Reviews",
                column: "ServiceRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Clients_ClientId",
                table: "Reviews",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_ServiceRequests_ServiceRequestId",
                table: "Reviews",
                column: "ServiceRequestId",
                principalTable: "ServiceRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Clients_ClientId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_ServiceRequests_ServiceRequestId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_ClientId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_ServiceRequestId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "ServiceRequestId",
                table: "Reviews");
        }
    }
}
