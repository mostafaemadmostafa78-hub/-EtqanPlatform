using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETQAN_BY_API.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedOrdersCountToArtisan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletedOrdersCount",
                table: "Artisans",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedOrdersCount",
                table: "Artisans");
        }
    }
}
