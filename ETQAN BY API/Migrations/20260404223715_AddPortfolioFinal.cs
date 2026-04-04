using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETQAN_BY_API.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtisanPortfolio_Artisans_ArtisanId",
                table: "ArtisanPortfolio");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArtisanPortfolio",
                table: "ArtisanPortfolio");

            migrationBuilder.RenameTable(
                name: "ArtisanPortfolio",
                newName: "ArtisanPortfolios");

            migrationBuilder.RenameIndex(
                name: "IX_ArtisanPortfolio_ArtisanId",
                table: "ArtisanPortfolios",
                newName: "IX_ArtisanPortfolios_ArtisanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArtisanPortfolios",
                table: "ArtisanPortfolios",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtisanPortfolios_Artisans_ArtisanId",
                table: "ArtisanPortfolios",
                column: "ArtisanId",
                principalTable: "Artisans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtisanPortfolios_Artisans_ArtisanId",
                table: "ArtisanPortfolios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArtisanPortfolios",
                table: "ArtisanPortfolios");

            migrationBuilder.RenameTable(
                name: "ArtisanPortfolios",
                newName: "ArtisanPortfolio");

            migrationBuilder.RenameIndex(
                name: "IX_ArtisanPortfolios_ArtisanId",
                table: "ArtisanPortfolio",
                newName: "IX_ArtisanPortfolio_ArtisanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArtisanPortfolio",
                table: "ArtisanPortfolio",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtisanPortfolio_Artisans_ArtisanId",
                table: "ArtisanPortfolio",
                column: "ArtisanId",
                principalTable: "Artisans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
