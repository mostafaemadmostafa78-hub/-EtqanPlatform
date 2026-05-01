using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETQAN_BY_API.Migrations
{
    /// <inheritdoc />
    public partial class SyncIdsToStrings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Artisans_ArtisanId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Artisans_ArtisanId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_ArtisanId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ArtisanId",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "ArtisanId",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArtisanId1",
                table: "ServiceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ArtisanId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArtisanId1",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ArtisanId1",
                table: "ServiceRequests",
                column: "ArtisanId1");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ArtisanId1",
                table: "Orders",
                column: "ArtisanId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Artisans_ArtisanId1",
                table: "Orders",
                column: "ArtisanId1",
                principalTable: "Artisans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Artisans_ArtisanId1",
                table: "ServiceRequests",
                column: "ArtisanId1",
                principalTable: "Artisans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Artisans_ArtisanId1",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Artisans_ArtisanId1",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_ArtisanId1",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ArtisanId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ArtisanId1",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ArtisanId1",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "ArtisanId",
                table: "ServiceRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ArtisanId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ArtisanId",
                table: "ServiceRequests",
                column: "ArtisanId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ArtisanId",
                table: "Orders",
                column: "ArtisanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Artisans_ArtisanId",
                table: "Orders",
                column: "ArtisanId",
                principalTable: "Artisans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Artisans_ArtisanId",
                table: "ServiceRequests",
                column: "ArtisanId",
                principalTable: "Artisans",
                principalColumn: "Id");
        }
    }
}
