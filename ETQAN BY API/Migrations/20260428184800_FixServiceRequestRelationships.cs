using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETQAN_BY_API.Migrations
{
    /// <inheritdoc />
    public partial class FixServiceRequestRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Artisans_ArtisanId1",
                table: "ServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Clients_ClientId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_ArtisanId1",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_Clients_ApplicationUserId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ArtisanId1",
                table: "ServiceRequests");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                table: "ServiceRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ArtisanId",
                table: "ServiceRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Clients_ApplicationUserId",
                table: "Clients",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ArtisanId",
                table: "ServiceRequests",
                column: "ArtisanId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Artisans_ArtisanId",
                table: "ServiceRequests",
                column: "ArtisanId",
                principalTable: "Artisans",
                principalColumn: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Clients_ClientId",
                table: "ServiceRequests",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Artisans_ArtisanId",
                table: "ServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Clients_ClientId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_ArtisanId",
                table: "ServiceRequests");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Clients_ApplicationUserId",
                table: "Clients");

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "ServiceRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ArtisanId",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArtisanId1",
                table: "ServiceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ArtisanId1",
                table: "ServiceRequests",
                column: "ArtisanId1");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ApplicationUserId",
                table: "Clients",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Artisans_ArtisanId1",
                table: "ServiceRequests",
                column: "ArtisanId1",
                principalTable: "Artisans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Clients_ClientId",
                table: "ServiceRequests",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
