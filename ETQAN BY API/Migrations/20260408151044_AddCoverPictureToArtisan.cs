using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETQAN_BY_API.Migrations
{
    /// <inheritdoc />
    public partial class AddCoverPictureToArtisan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Artisans_ArtisanId",
                table: "Reviews");

            migrationBuilder.AddColumn<string>(
                name: "CoverPicture",
                table: "Artisans",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Artisans_ArtisanId",
                table: "Reviews",
                column: "ArtisanId",
                principalTable: "Artisans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Artisans_ArtisanId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "CoverPicture",
                table: "Artisans");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Artisans_ArtisanId",
                table: "Reviews",
                column: "ArtisanId",
                principalTable: "Artisans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
