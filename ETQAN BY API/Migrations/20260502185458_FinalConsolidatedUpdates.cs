using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETQAN_BY_API.Migrations
{
    /// <inheritdoc />
    public partial class FinalConsolidatedUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletedOrdersCount",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedOrdersCount",
                table: "Companies");
        }
    }
}
