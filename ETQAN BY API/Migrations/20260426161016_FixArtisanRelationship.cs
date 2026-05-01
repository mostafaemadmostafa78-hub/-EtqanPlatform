using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETQAN_BY_API.Migrations
{
    /// <inheritdoc />
    public partial class FixArtisanRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Artisans_ArtisanId1",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ArtisanId1",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Artisans_ApplicationUserId",
                table: "Artisans");

            migrationBuilder.DropColumn(
                name: "ArtisanId1",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "ArtisanId",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverPhoto",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Artisans_ApplicationUserId",
                table: "Artisans",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ArtisanId",
                table: "Orders",
                column: "ArtisanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Artisans_ArtisanId",
                table: "Orders",
                column: "ArtisanId",
                principalTable: "Artisans",
                principalColumn: "ApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Artisans_ArtisanId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ArtisanId",
                table: "Orders");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Artisans_ApplicationUserId",
                table: "Artisans");

            migrationBuilder.DropColumn(
                name: "CoverPhoto",
                table: "Companies");

            migrationBuilder.AlterColumn<string>(
                name: "ArtisanId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArtisanId1",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ArtisanId1",
                table: "Orders",
                column: "ArtisanId1");

            migrationBuilder.CreateIndex(
                name: "IX_Artisans_ApplicationUserId",
                table: "Artisans",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Artisans_ArtisanId1",
                table: "Orders",
                column: "ArtisanId1",
                principalTable: "Artisans",
                principalColumn: "Id");
        }
    }
}
